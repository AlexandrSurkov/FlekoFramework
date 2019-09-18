using System;

namespace Flekosoft.Common.Video
{
    public interface IFrameProcessor : IDisposable
    {
        void ProcessFrame(VideoFrame frame);
    }
}