using System;

namespace Flekosoft.Common.Video
{
    public class VideoFrame : DisposableBase
    {
        public VideoFrame(DateTime timeStamp, byte[] rawData, Resolution resolution, FrameFormat frameFormat)
        {
            RawData = rawData;
            FrameFormat = frameFormat;
            TimeStamp = timeStamp;
            Resolution = new Resolution(resolution);
        }

        public DateTime TimeStamp{ get; }

        public byte[] RawData { get; }
        public FrameFormat FrameFormat { get; }
        public Resolution Resolution { get; }

        public override string ToString()
        {
            // ReSharper disable once UseFormatSpecifierInInterpolation
            return $"Frame TimeStamp: {TimeStamp.ToString("yy/MM/dd hh:mm:ss.fffffff")}  Resolution: {Resolution} Format: {FrameFormat}";
        }
    }
}