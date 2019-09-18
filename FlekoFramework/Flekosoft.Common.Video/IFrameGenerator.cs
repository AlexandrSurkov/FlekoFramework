using System;

namespace Flekosoft.Common.Video
{
    public interface IFrameGenerator: IDisposable
    {
        event EventHandler<FrameEventArgs> NewFrame;
    }
}
