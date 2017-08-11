using System;

namespace Flekosoft.Common.Video
{
    public class Frame : IDisposable
    {
        public Frame(DateTime timeStamp, byte[] rawData, Resolution resolution, FrameFormat frameFormat)
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

        #region IDisposable Members
        private bool _disposed;
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    //Clear resources
                }
                _disposed = true;
            }
        }
        #endregion

    }
}