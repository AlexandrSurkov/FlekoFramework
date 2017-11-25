using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Flekosoft.Common.Logging;

namespace Flekosoft.Common.Video
{
    public abstract class VideoSource : PropertyChangedErrorNotifyDisposableBase, IVideoSource
    {
        private Resolution _videoResolution;
        private int _framesPerSecond;
        private string _name;
        private bool _isStarted;
        private ReadOnlyCollection<int> _supportedFps;
        private ReadOnlyCollection<Resolution> _supportedResolution;

        // ReSharper disable once EmptyConstructor
        protected VideoSource()
        {
#if DEBUG
            Logger.Instance.AppendLog(new LogRecord(DateTime.Now, new List<string> { $"VideoSource {this}: Created" }, LogRecordLevel.Debug));
#endif
        }

        public Resolution VideoResolution
        {
            get => _videoResolution;
            set
            {
                if(value == null) throw new ArgumentNullException();
                if (!SupportedResolution.Contains(value)) throw new ArgumentOutOfRangeException();
                if (_videoResolution != value)
                {
                    _videoResolution = value;
                    OnPropertyChanged(nameof(VideoResolution));
                    Logger.Instance.AppendLog(new LogRecord(DateTime.Now, new List<string> { $"VideoSource {this}: video resolution changed to {_videoResolution}" }, LogRecordLevel.Info));
                }
            }
        }
        public int FramesPerSecond
        {
            get => _framesPerSecond;
            set
            {
                if (!SupportedFps.Contains(value)) throw new ArgumentOutOfRangeException();
                if (_framesPerSecond != value)
                {
                    _framesPerSecond = value;
                    OnPropertyChanged(nameof(FramesPerSecond));
                    Logger.Instance.AppendLog(new LogRecord(DateTime.Now, new List<string> { $"VideoSource {this}: fps changed to {_framesPerSecond}" }, LogRecordLevel.Info));
                }
            }
        }

        public new string Name
        {
            get { return _name; }
            set
            {
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged(nameof(Name));
#if DEBUG
                    Logger.Instance.AppendLog(new LogRecord(DateTime.Now, new List<string> { $"VideoSource {this}: Name was changed to {_name}" }, LogRecordLevel.Debug));
#endif
                }
            }
        }

        public bool IsStarted
        {
            get => _isStarted;
            private set
            {
                if (_isStarted != value)
                {
                    _isStarted = value;
                    OnPropertyChanged(nameof(IsStarted));
                    // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
                    if (_isStarted) Logger.Instance.AppendLog(new LogRecord(DateTime.Now, new List<string> { $"VideoSource {this}: Started" }, LogRecordLevel.Info));
                    else Logger.Instance.AppendLog(new LogRecord(DateTime.Now, new List<string> { $"VideoSource {this}: Stopped" }, LogRecordLevel.Info));
                }
            }
        }

        public ReadOnlyCollection<int> SupportedFps
        {
            get { return _supportedFps; }
            protected set
            {
                if (_supportedFps != value)
                {
                    _supportedFps = value;
                    OnPropertyChanged(nameof(SupportedFps));
                    Logger.Instance.AppendLog(new LogRecord(DateTime.Now, new List<string> { $"VideoSource {this}: Supported fps changed" }, LogRecordLevel.Info));

#if DEBUG
                    var logStrings = new List<string>() { $"VideoSource {this}: Supported Fps:" };
                    foreach (int supportedFp in SupportedFps)
                    {
                        logStrings.Add(supportedFp.ToString());
                    }

                    Logger.Instance.AppendLog(new LogRecord(DateTime.Now, logStrings, LogRecordLevel.Debug));
#endif
                }
            }
        }

        public ReadOnlyCollection<Resolution> SupportedResolution
        {
            get { return _supportedResolution; }
            protected set
            {
                if (_supportedResolution != value)
                {
                    _supportedResolution = value;
                    OnPropertyChanged(nameof(SupportedResolution));
                    Logger.Instance.AppendLog(new LogRecord(DateTime.Now, new List<string> { $"VideoSource {this}: Supported resolution changed" }, LogRecordLevel.Info));

#if DEBUG
                    var logStrings = new List<string>() { $"VideoSource {this}: Supported Resolutions:" };
                    foreach (Resolution supported in SupportedResolution)
                    {
                        logStrings.Add(supported.ToString());
                    }

                    Logger.Instance.AppendLog(new LogRecord(DateTime.Now, logStrings, LogRecordLevel.Debug));
#endif
                }
            }
        }

        public event EventHandler<FrameEventArgs> NewFrame;
        protected void OnNewFrame(VideoFrame frame)
        {
#if DEBUG
            Logger.Instance.AppendLog(new LogRecord(DateTime.Now, new List<string> { $"VideoSource {this}: New frame {frame}" }, LogRecordLevel.Debug));
#endif
            NewFrame?.Invoke(this, new FrameEventArgs(frame));
        }

        public void Start()
        {
            if (!IsStarted)
                IsStarted = StartInternal();
        }

        public void Stop()
        {
            if (IsStarted)
            {
                StopInternal();
                IsStarted = false;
            }
        }

        protected abstract bool StartInternal();
        protected abstract void StopInternal();

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                NewFrame = null;
            }
            base.Dispose(disposing);
#if DEBUG
            Logger.Instance.AppendLog(new LogRecord(DateTime.Now, new List<string> { $"VideoSource {this}: Disposed" }, LogRecordLevel.Info));
#endif
        }
    }
}
