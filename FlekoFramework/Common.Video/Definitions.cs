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

    public class FrameEventArgs : EventArgs
    {
        public FrameEventArgs(VideoFrame frame)
        {
            Frame = frame;
        }

        public VideoFrame Frame { get; }
    }
}