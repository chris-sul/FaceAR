﻿//----------------------------------------------------------------------------
//  Copyright (C) 2004-2017 by EMGU Corporation. All rights reserved.       
//----------------------------------------------------------------------------

using System;
using System.Drawing;
using System.Runtime.InteropServices;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.Util;

namespace Emgu.CV.VideoSurveillance
{
    /// <summary>
    /// An abstract class that can be use the perform background / foreground detection.
    /// </summary>
    public abstract class BackgroundSubtractor : UnmanagedObject
    {
        static BackgroundSubtractor()
        {
            CvInvoke.CheckLibraryLoaded();
        }

        /// <summary>
        /// Update the background model
        /// </summary>
        /// <param name="image">The image that is used to update the background model</param>
        /// <param name="learningRate">Use -1 for default</param>
        /// <param name="fgMask">The output foreground mask</param>
        public void Apply(IInputArray image, IOutputArray fgMask, double learningRate = -1)
        {
            using (InputArray iaImage = image.GetInputArray())
            using (OutputArray oaFgMask = fgMask.GetOutputArray())
                CvInvoke.cveBackgroundSubtractorUpdate(_ptr, iaImage, oaFgMask, learningRate);
        }

        /// <summary>
        /// Computes a background image.
        /// </summary>
        /// <param name="backgroundImage">The output background image</param>
        /// <remarks> Sometimes the background image can be very blurry, as it contain the average background statistics.</remarks>
        public void GetBackgroundImage(IOutputArray backgroundImage)
        {
            using (OutputArray oaBackgroundImage = backgroundImage.GetOutputArray())
                CvInvoke.cveBackgroundSubtractorGetBackgroundImage(_ptr, oaBackgroundImage);
        }
    }
}

namespace Emgu.CV
{
    public static partial class CvInvoke
    {
        [DllImport(CvInvoke.ExternLibrary, CallingConvention = CvInvoke.CvCallingConvention)]
        internal static extern void cveBackgroundSubtractorUpdate(IntPtr bgSubtractor, IntPtr image, IntPtr fgmask, double learningRate);

        [DllImport(CvInvoke.ExternLibrary, CallingConvention = CvInvoke.CvCallingConvention)]
        internal static extern void cveBackgroundSubtractorGetBackgroundImage(IntPtr bgSubtractor, IntPtr backgroundImage);
    }
}