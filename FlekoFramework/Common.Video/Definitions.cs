using System;

namespace Flekosoft.Common.Video
{
    public enum FrameFormat
    {
        None,
        Unknown,
        Y8,
        Nv21,
        Rgb24,
        Argb32,
        // ReSharper disable once InconsistentNaming
        H264NALUnit,
    }

    public class NewFrameEventArgs : EventArgs
    {
        public NewFrameEventArgs(Frame frame)
        {
            Frame = frame;
        }

        public Frame Frame { get; }
    }
}