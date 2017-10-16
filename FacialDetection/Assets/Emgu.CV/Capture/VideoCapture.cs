//----------------------------------------------------------------------------
//  Copyright (C) 2004-2017 by EMGU Corporation. All rights reserved.       
//----------------------------------------------------------------------------

#if !(__ANDROID__ || __UNIFIED__ || NETFX_CORE || UNITY_ANDROID || UNITY_IOS || UNITY_EDITOR || UNITY_STANDALONE)
#define WITH_SERVICE_MODEL
#endif

//#define TEST_CAPTURE

using System;
#if WITH_SERVICE_MODEL
using System.ServiceModel;
#endif
using System.Runtime.InteropServices;
using System.Drawing;
using System.Threading;
#if NETFX_CORE
using Windows.System.Threading;
using System.Threading.Tasks;
#endif
using Emgu.Util;
using Emgu.CV.Structure;

namespace Emgu.CV
{
    /// <summary> 
    /// Capture images from either camera or video file. 
    /// </summary>
#if WITH_SERVICE_MODEL
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
#endif
    public partial class VideoCapture :
        UnmanagedObject,
#if WITH_SERVICE_MODEL
 IDuplexCapture,
#endif
 ICapture
    {

        AutoResetEvent _pauseEvent = new AutoResetEvent(false);

        /// <summary>
        /// the type of flipping
        /// </summary>
        private CvEnum.FlipType _flipType = Emgu.CV.CvEnum.FlipType.None;

        /// <summary>
        /// The type of capture source
        /// </summary>
        public enum CaptureModuleType
        {
            /// <summary>
            /// Capture from camera
            /// </summary>
            Camera,
            /// <summary>
            /// Capture from file using HighGUI
            /// </summary>
            Highgui,
            /*
            /// <summary>
            /// Capture from file using FFMPEG
            /// </summary>
            FFMPEG,*/
        }

        private CaptureModuleType _captureModuleType;

        #region Properties
        /// <summary>
        /// Get the type of the capture module
        /// </summary>
        public CaptureModuleType CaptureSource
        {
            get
            {
                return _captureModuleType;
            }
        }

        /// <summary>
        /// Get and set the flip type
        /// </summary>
        public CvEnum.FlipType FlipType
        {
            get
            {
                return _flipType;
            }
            set
            {
                _flipType = value;
            }
        }

        /// <summary>
        /// Get or Set if the captured image should be flipped horizontally
        /// </summary>
        public bool FlipHorizontal
        {
            get
            {
                return (_flipType & Emgu.CV.CvEnum.FlipType.Horizontal) == Emgu.CV.CvEnum.FlipType.Horizontal;
            }
            set
            {
                if (value != FlipHorizontal)
                    _flipType ^= Emgu.CV.CvEnum.FlipType.Horizontal;
            }
        }

        /// <summary>
        /// Get or Set if the captured image should be flipped vertically
        /// </summary>
        public bool FlipVertical
        {
            get
            {
                return (_flipType & Emgu.CV.CvEnum.FlipType.Vertical) == Emgu.CV.CvEnum.FlipType.Vertical;
            }
            set
            {
                if (value != FlipVertical)
                    _flipType ^= Emgu.CV.CvEnum.FlipType.Vertical;
            }
        }

        ///<summary> The width of this capture</summary>
        public int Width
        {
            get
            {
                return Convert.ToInt32(GetCaptureProperty(CvEnum.CapProp.FrameWidth));
            }
        }

        ///<summary> The height of this capture </summary>
        public int Height
        {
            get
            {
                return Convert.ToInt32(GetCaptureProperty(CvEnum.CapProp.FrameHeight));
            }
        }
        #endregion

        #region constructors
        /// <summary>
        /// Create a capture using the specific camera
        /// </summary>
        /// <param name="captureType">The capture type</param>
        public VideoCapture(CvEnum.CaptureType captureType)
           : this((int)captureType)
        {
        }

        ///<summary> Create a capture using the default camera </summary>
        public VideoCapture()
           : this(0)
        {
        }

        ///<summary> Create a capture using the specific camera</summary>
        ///<param name="camIndex"> The index of the camera to create capture from, starting from 0</param>
        public VideoCapture(int camIndex)
        {
            _captureModuleType = CaptureModuleType.Camera;

#if TEST_CAPTURE
#else
            _ptr = CvInvoke.cveVideoCaptureCreateFromDevice(camIndex);
            if (_ptr == IntPtr.Zero)
            {
                throw new NullReferenceException(String.Format("Error: Unable to create capture from camera {0}", camIndex));
            }
#endif
        }

        /// <summary>
        /// Create a capture from file or a video stream
        /// </summary>
        /// <param name="fileName">The name of a file, or an url pointed to a stream.</param>
        public VideoCapture(String fileName)
        {
            using (CvString s = new CvString(fileName))
            {
                /*
                if (Util.CvToolbox.HasFFMPEG)
                {
                   _captureModuleType = CaptureModuleType.FFMPEG;
                   _ptr = CvInvoke.cvCreateFileCapture_FFMPEG(fileName);
                }
                else*/
                {
                    _captureModuleType = CaptureModuleType.Highgui;
                    _ptr = CvInvoke.cveVideoCaptureCreateFromFile(s);
                }

                if (_ptr == IntPtr.Zero)
                    throw new NullReferenceException(String.Format("Unable to create capture from {0}", fileName));
            }
        }
        #endregion

        #region implement UnmanagedObject
        /// <summary>
        /// Release the resource for this capture
        /// </summary>
        protected override void DisposeObject()
        {
#if TEST_CAPTURE
#else
            Stop();
            CvInvoke.cveVideoCaptureRelease(ref _ptr);

#endif
        }
        #endregion

        /// <summary>
        /// Obtain the capture property
        /// </summary>
        /// <param name="index">The index for the property</param>
        /// <returns>The value of the specific property</returns>
        public double GetCaptureProperty(CvEnum.CapProp index)
        {
            return CvInvoke.cveVideoCaptureGet(_ptr, index);
        }

        /// <summary>
        /// Sets the specified property of video capturing
        /// </summary>
        /// <param name="property">Property identifier</param>
        /// <param name="value">Value of the property</param>
        /// <returns>True if success</returns>
        public bool SetCaptureProperty(CvEnum.CapProp property, double value)
        {
            return CvInvoke.cveVideoCaptureSet(Ptr, property, value);
        }

        /// <summary>
        /// Grab a frame
        /// </summary>
        /// <returns>True on success</returns>
        public virtual bool Grab()
        {
            if (_ptr == IntPtr.Zero)
                return false;

            bool grabbed = CvInvoke.cveVideoCaptureGrab(Ptr);
            if (grabbed && ImageGrabbed != null)
                ImageGrabbed(this, new EventArgs());
            return grabbed;
        }

        #region Grab process
        /// <summary>
        /// The event to be called when an image is grabbed
        /// </summary>
        public event EventHandler ImageGrabbed;

        private enum GrabState
        {
            Stopped,
            Running,
            Pause,
            Stopping,
        }

        private volatile GrabState _grabState = GrabState.Stopped;

        private void Run(
#if WITH_SERVICE_MODEL
                System.ServiceModel.Dispatcher.ExceptionHandler eh = null
#endif
            )
        {
            try
            {
                while (_grabState == GrabState.Running || _grabState == GrabState.Pause)
                {
                    if (_grabState == GrabState.Pause)
                    {
                        _pauseEvent.WaitOne();
                    }
                    else if (IntPtr.Zero.Equals(_ptr) || !Grab())
                    {
                        //capture has been released, or
                        //no more frames to grab, this is the end of the stream. 
                        //We should stop.
                        _grabState = GrabState.Stopping;
                    }
                }
            }
            catch (Exception e)
            {
#if WITH_SERVICE_MODEL
                if (eh != null && eh.HandleException(e))
                        return;
#endif                
                throw new Exception("Capture error", e);
            }
            finally
            {
                _grabState = GrabState.Stopped;
            }
        }

        private static void Wait(int millisecond)
        {
#if NETFX_CORE
         Task t = Task.Delay(millisecond);
         t.Wait();
#else
            Thread.Sleep(millisecond);
#endif
        }

        /// <summary>
        /// Start the grab process in a separate thread. Once started, use the ImageGrabbed event handler and RetrieveGrayFrame/RetrieveBgrFrame to obtain the images.
        /// </summary>
        /// <param name="eh">An exception handler. If provided, it will be used to handle exception in the capture thread.</param>
        public void Start(
#if WITH_SERVICE_MODEL
            System.ServiceModel.Dispatcher.ExceptionHandler eh = null
#endif
        )
        {
            if (_grabState == GrabState.Pause)
            {
                _grabState = GrabState.Running;
                _pauseEvent.Set();

            }
            else if (_grabState == GrabState.Stopped || _grabState == GrabState.Stopping)
            {
                _grabState = GrabState.Running;
#if NETFX_CORE 
                ThreadPool.RunAsync(delegate { Run(); });
#elif !WITH_SERVICE_MODEL
                ThreadPool.QueueUserWorkItem(delegate { Run(); });
#else
                ThreadPool.QueueUserWorkItem(delegate { Run(eh); });
#endif
            }
        }

        /// <summary>
        /// Pause the grab process if it is running.
        /// </summary>
        public void Pause()
        {
            if (_grabState == GrabState.Running)
                _grabState = GrabState.Pause;
        }

        /// <summary>
        /// Stop the grabbing thread
        /// </summary>
        public void Stop()
        {
            if (_grabState == GrabState.Pause)
            {
                _grabState = GrabState.Stopping;
                _pauseEvent.Set();
            }
            else
               if (_grabState == GrabState.Running)
                _grabState = GrabState.Stopping;
        }
        #endregion

        /// <summary> 
        /// Retrieve a Gray image frame after Grab()
        /// </summary>
        /// <param name="image">The output image</param>
        /// <param name="channel">The channel to retrieve image</param>
        /// <returns>True if the frame can be retrieved</returns>
        public virtual bool Retrieve(IOutputArray image, int channel = 0)
        {
            using (OutputArray oaImage = image.GetOutputArray())
            {
                if (FlipType == CvEnum.FlipType.None)
                {
                    return CvInvoke.cveVideoCaptureRetrieve(Ptr, oaImage, channel);
                }
                else
                {
                    bool success = CvInvoke.cveVideoCaptureRetrieve(Ptr, oaImage, channel);
                    if (success)
                        CvInvoke.Flip(image, image, FlipType);
                    return success;
                }
            }
        }

        /// <summary>
        /// Similar to the C++ implementation of cv::Capture >> Mat
        /// </summary>
        /// <param name="m">The matrix the image will be read into.</param>
        public void Read(Mat m)
        {
            CvInvoke.cveVideoCaptureReadToMat(Ptr, m);
        }

        #region implement ICapture
        /// <summary> 
        /// Capture a Bgr image frame
        /// </summary>
        /// <returns> A Bgr image frame. If no more frames are available, null will be returned.</returns>
        public virtual Mat QueryFrame()
        {
            if (Grab())
            {
                Mat image = new Mat();
                Retrieve(image);
                return image;
            }
            else
            {
                return null;
            }
        }

        ///<summary> 
        /// Capture a Bgr image frame that is half width and half height. 
        /// Mainly used by WCF when sending image to remote locations in a bandwidth conservative scenario 
        ///</summary>
        ///<remarks>Internally, this is a cvQueryFrame operation follow by a cvPyrDown</remarks>
        ///<returns> A Bgr image frame that is half width and half height</returns>
        public virtual Mat QuerySmallFrame()
        {
            Mat tmp = QueryFrame();

            if (tmp != null)
            {
                if (!tmp.IsEmpty)
                {
                    CvInvoke.PyrDown(tmp, tmp);
                    return tmp;
                }
                else
                {
                    tmp.Dispose();
                }
            }
            return null;

        }
        #endregion

        /*
          ///<summary> Capture Bgr image frame with timestamp</summary>
          ///<returns> A timestamped Bgr image frame</returns>
          public TimedImage<Bgr, Byte> QueryTimedFrame()
          {
              IntPtr img = CvInvoke.cvQueryFrame(_ptr);
              TimedImage<Bgr, Byte> res = new TimedImage<Bgr, Byte>(Width, Height);

              res.Timestamp = System.DateTime.Now;

              if (FlipType == Emgu.CV.CvEnum.FLIP.None)
              {
                  CvInvoke.cvCopy(img, res.Ptr, IntPtr.Zero);
                  return res;
              }
              else
              {
                  //code = 0 indicates vertical flip only
                  int code = 0;
                  //code = -1 indicates vertical and horizontal flip
                  if (FlipType == (Emgu.CV.CvEnum.FLIP.HORIZONTAL | Emgu.CV.CvEnum.FLIP.VERTICAL)) code = -1;
                  //code = 1 indicates horizontal flip only
                  else if (FlipType == Emgu.CV.CvEnum.FLIP.HORIZONTAL) code = 1;
                  CvInvoke.cvFlip(img, res.Ptr, code);
                  return res;
              }
          }*/

#if WITH_SERVICE_MODEL
        /// <summary>
        /// Query a frame duplexly over WCF
        /// </summary>
        public virtual void DuplexQueryFrame()
        {
            IDuplexCaptureCallback callback = OperationContext.Current.GetCallbackChannel<IDuplexCaptureCallback>();
            using (Mat img = QueryFrame())
            {
                callback.ReceiveFrame(img);
            }
        }

        /// <summary>
        /// Query a small frame duplexly over WCF
        /// </summary>
        public virtual void DuplexQuerySmallFrame()
        {
            IDuplexCaptureCallback callback = OperationContext.Current.GetCallbackChannel<IDuplexCaptureCallback>();
            using (Mat img = QuerySmallFrame())
            {
                callback.ReceiveFrame(img);
            }
        }
#endif


    }


    partial class CvInvoke
    {
        [DllImport(ExternLibrary, CallingConvention = CvInvoke.CvCallingConvention)]
        internal static extern void cveVideoCaptureReadToMat(IntPtr capture, IntPtr mat);

#if NETFX_CORE
        [UnmanagedFunctionPointer(CvInvoke.CvCallingConvention)]
        public delegate void WinrtMessageLoopCallback();

        [DllImport(ExternLibrary, EntryPoint= "cveWinrtStartMessageLoop", CallingConvention = CvInvoke.CvCallingConvention)]
        public static extern void WinrtStartMessageLoop(WinrtMessageLoopCallback callback);

        [DllImport(ExternLibrary, EntryPoint = "cveWinrtSetFrameContainer", CallingConvention = CvInvoke.CvCallingConvention)]
        public static extern void WinrtSetFrameContainer(Windows.UI.Xaml.Controls.Image image);

        [DllImport(ExternLibrary, EntryPoint = "cveWinrtImshow", CallingConvention = CvInvoke.CvCallingConvention)]
        public static extern void WinrtImshow();

        [DllImport(ExternLibrary, EntryPoint = "cveWinrtOnVisibilityChanged", CallingConvention = CvInvoke.CvCallingConvention)]
        public static extern void WinrtOnVisibilityChanged(
            [MarshalAs(CvInvoke.BoolMarshalType)]
            bool visible);
#endif
        /// <summary>
        /// Allocates and initialized the CvCapture structure for reading a video stream from the camera. Currently two camera interfaces can be used on Windows: Video for Windows (VFW) and Matrox Imaging Library (MIL); and two on Linux: V4L and FireWire (IEEE1394). 
        /// </summary>
        /// <param name="index">Index of the camera to be used. If there is only one camera or it does not matter what camera to use -1 may be passed</param>
        /// <returns>Pointer to the capture structure</returns>
        [DllImport(ExternLibrary, CallingConvention = CvInvoke.CvCallingConvention)]
        internal static extern IntPtr cveVideoCaptureCreateFromDevice(int index);

        /// <summary>
        /// Allocates and initialized the CvCapture structure for reading the video stream from the specified file. 
        ///After the allocated structure is not used any more it should be released by cvReleaseCapture function. 
        /// </summary>
        /// <param name="filename">Name of the video file.</param>
        /// <returns>Pointer to the capture structure.</returns>
        [DllImport(ExternLibrary, CallingConvention = CvInvoke.CvCallingConvention)]
        internal static extern IntPtr cveVideoCaptureCreateFromFile(IntPtr filename);

        /// <summary>
        /// The function cvReleaseCapture releases the CvCapture structure allocated by cvCreateFileCapture or cvCreateCameraCapture
        /// </summary>
        /// <param name="capture">pointer to video capturing structure.</param>
        [DllImport(ExternLibrary, CallingConvention = CvInvoke.CvCallingConvention)]
        internal static extern void cveVideoCaptureRelease(ref IntPtr capture);

        /// <summary>
        /// Grabs a frame from camera or video file, decompresses and returns it. This function is just a combination of cvGrabFrame and cvRetrieveFrame in one call. 
        /// </summary>
        /// <param name="capture">Video capturing structure</param>
        /// <param name="frame">The output frame</param>
        /// <returns>true id a frame is read</returns>
        /// <remarks>The returned image should not be released or modified by user. </remarks>
        [DllImport(ExternLibrary, CallingConvention = CvInvoke.CvCallingConvention)]
        [return: MarshalAs(CvInvoke.BoolToIntMarshalType)]
        internal static extern bool cveVideoCaptureRead(IntPtr capture, IntPtr frame);

        /// <summary>
        /// Grab a frame
        /// </summary>
        /// <param name="capture">Video capturing structure</param>
        /// <returns>True on success</returns>
        [DllImport(ExternLibrary, CallingConvention = CvInvoke.CvCallingConvention)]
        [return: MarshalAs(CvInvoke.BoolToIntMarshalType)]
        internal static extern bool cveVideoCaptureGrab(IntPtr capture);

        /// <summary>
        /// Get the frame grabbed with cvGrabFrame(..)
        /// This function may apply some frame processing like frame decompression, flipping etc.
        /// </summary>
        /// <param name="capture">Video capturing structure</param>
        /// <param name="image">The output image</param>
        /// <param name="flag">The frame retrieve flag</param>
        /// <returns>True on success</returns>
        /// <remarks>The returned image should not be released or modified by user. </remarks>
        [DllImport(ExternLibrary, CallingConvention = CvInvoke.CvCallingConvention)]
        [return: MarshalAs(CvInvoke.BoolToIntMarshalType)]
        internal static extern bool cveVideoCaptureRetrieve(IntPtr capture, IntPtr image, int flag);

        /// <summary>
        /// Retrieves the specified property of camera or video file
        /// </summary>
        /// <param name="capture">Video capturing structure</param>
        /// <param name="prop">Property identifier</param>
        /// <returns>The specified property of camera or video file</returns>
        [DllImport(ExternLibrary, CallingConvention = CvInvoke.CvCallingConvention)]
        public static extern double cveVideoCaptureGet(IntPtr capture, CvEnum.CapProp prop);

        /// <summary>
        /// Sets the specified property of video capturing
        /// </summary>
        /// <param name="capture">Video capturing structure</param>
        /// <param name="propertyId">Property identifier</param>
        /// <param name="value">Value of the property</param>
        /// <returns>True on success</returns>
        [DllImport(ExternLibrary, CallingConvention = CvInvoke.CvCallingConvention)]
        [return: MarshalAs(CvInvoke.BoolToIntMarshalType)]
        public static extern bool cveVideoCaptureSet(IntPtr capture, CvEnum.CapProp propertyId, double value);

    }
}
