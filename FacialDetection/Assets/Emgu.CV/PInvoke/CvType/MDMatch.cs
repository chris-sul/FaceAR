//----------------------------------------------------------------------------
//  Copyright (C) 2004-2017 by EMGU Corporation. All rights reserved.       
//----------------------------------------------------------------------------

using System;
using System.Runtime.InteropServices;

namespace Emgu.CV.Structure
{
    /// <summary>
    /// OpenCV's DMatch structure
    /// </summary>
#if !NETFX_CORE
    [Serializable]
#endif
    [StructLayout(LayoutKind.Sequential)]
    public struct MDMatch
    {
        /// <summary>
        /// Query descriptor index
        /// </summary>
        public int QueryIdx;

        /// <summary>
        /// Train descriptor index
        /// </summary>
        public int TrainIdx;

        /// <summary>
        /// Train image index
        /// </summary>
        public int ImgIdx;

        /// <summary>
        /// Distance
        /// </summary>
        public float Distance;
    }
}
