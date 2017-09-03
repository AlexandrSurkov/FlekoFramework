using System.Collections.ObjectModel;

namespace Flekosoft.Common.Video
{
    public interface IVideoSource : IFrameGenerator
    {
        Resolution VideoResolution { get; set; }
        int FramesPerSecond { get; set; }

        bool IsStarted { get; }

        ReadOnlyCollection<int> SupportedFps { get; }
        ReadOnlyCollection<Resolution> SupportedResolution { get; }

        void Start();
        void Stop();

        
    }
}