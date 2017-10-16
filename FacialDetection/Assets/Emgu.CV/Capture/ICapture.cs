//----------------------------------------------------------------------------
//  Copyright (C) 2004-2017 by EMGU Corporation. All rights reserved.       
//----------------------------------------------------------------------------

using System;
#if !(__ANDROID__ || __UNIFIED__ || NETFX_CORE || UNITY_ANDROID || UNITY_IOS || UNITY_EDITOR || UNITY_STANDALONE)
using System.ServiceModel;
#endif
using Emgu.CV.Structure;

namespace Emgu.CV
{
    ///<summary> The interface that is used for WCF to provide a image capture service</summary>
#if !(__ANDROID__ || __UNIFIED__ || NETFX_CORE || UNITY_ANDROID || UNITY_IOS || UNITY_EDITOR || UNITY_STANDALONE)
   [XmlSerializerFormat]
   [ServiceContract]
#endif
    public interface ICapture
   {
        ///<summary> Capture a Bgr image frame </summary>
        ///<returns> A Bgr image frame</returns>
#if !(__ANDROID__ || __UNIFIED__ || NETFX_CORE || UNITY_ANDROID || UNITY_IOS || UNITY_EDITOR || UNITY_STANDALONE)
      [OperationContract]
#endif
        Mat QueryFrame();

        ///<summary> Capture a Bgr image frame that is half width and half heigh</summary>
        ///<returns> A Bgr image frame that is half width and half height</returns>
#if !(__ANDROID__ || __UNIFIED__ || NETFX_CORE || UNITY_ANDROID || UNITY_IOS || UNITY_EDITOR || UNITY_STANDALONE)
      [OperationContract]
#endif
        Mat QuerySmallFrame();
   }
}
