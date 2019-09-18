using System.Collections.ObjectModel;

namespace Flekosoft.Common.Video
{
    public interface IVideoSource : IFrameGenerator
    {
        Resolution VideoResolution { get; set; }
        int FramesPerSecond { get; set; }

        bool IsStarted { get; }

        string Name { get; set; }

        ReadOnlyCollection<int> SupportedFps { get; }
        ReadOnlyCollection<Resolution> SupportedResolution { get; }

        void Start();
        void Stop();

        
    }
}