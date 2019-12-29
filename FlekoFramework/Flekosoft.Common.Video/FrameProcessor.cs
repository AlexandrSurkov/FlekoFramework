using System;
using System.Collections.Concurrent;
using System.Threading;
#if DEBUG
using Flekosoft.Common.Logging;
using System.Collections.Generic;
#endif

namespace Flekosoft.Common.Video
{
    public abstract class FrameProcessor : PropertyChangedErrorNotifyDisposableBase, IFrameProcessor
    {
        private readonly ConcurrentQueue<VideoFrame> _frameQueue = new ConcurrentQueue<VideoFrame>();
        readonly EventWaitHandle _newFrameWh = new EventWaitHandle(false, EventResetMode.ManualReset);
        private readonly Thread _processThread;

        // Define the cancellation token.
        readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        readonly EventWaitHandle _threadFinishedWaitHandle = new EventWaitHandle(false, EventResetMode.ManualReset);

        protected FrameProcessor()
        {
            _processThread = new Thread(ProcessFrameThreadFunc);
            _processThread.Start(_cancellationTokenSource.Token);
#if DEBUG
            Logger.Instance.AppendLog(new LogRecord(DateTime.Now, new List<string> { $"FrameProcessor {this}: Created" }, LogRecordLevel.Debug));
#endif
        }

        public void ProcessFrame(VideoFrame frame)
        {
#if DEBUG
            Logger.Instance.AppendLog(new LogRecord(DateTime.Now, new List<string> { $"FrameProcessor {this}: Frame enqueued: {frame}" }, LogRecordLevel.Debug));
#endif
            _frameQueue.Enqueue(frame);
#if DEBUG
            Logger.Instance.AppendLog(new LogRecord(DateTime.Now, new List<string> { $"FrameProcessor {this}: Queue length: {_frameQueue.Count}" }, LogRecordLevel.Debug));
#endif
            _newFrameWh.Set();
        }

        protected abstract void ProcessFrameInternal(VideoFrame frame);

        private void ProcessFrameThreadFunc(object o)
        {
            var cancellationToken = (CancellationToken)o;
            while (true)
            {
                try
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    if (_newFrameWh.WaitOne(TimeSpan.FromSeconds(1)))
                    {
                        VideoFrame frame;
                        if (_frameQueue.TryDequeue(out frame))
                        {
#if DEBUG
                            Logger.Instance.AppendLog(new LogRecord(DateTime.Now, new List<string> { $"FrameProcessor {this}: Processing: {frame}" }, LogRecordLevel.Debug));
#endif
                            ProcessFrameInternal(frame);
                        }
                        if (_frameQueue.IsEmpty) _newFrameWh.Reset();
                        else _newFrameWh.Set();
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (ThreadAbortException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    OnErrorEvent(ex);
                }
            }
            _threadFinishedWaitHandle.Set();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_processThread != null)
                {
                    if (_processThread.IsAlive)
                    {
                        _cancellationTokenSource.Cancel();
                        _threadFinishedWaitHandle.WaitOne(Timeout.Infinite);
                        //_processThread.Abort();
                    }
                }
                _cancellationTokenSource.Dispose();
                _threadFinishedWaitHandle?.Dispose();

                _newFrameWh?.Dispose();
            }
            base.Dispose(disposing);
#if DEBUG
            Logger.Instance.AppendLog(new LogRecord(DateTime.Now, new List<string> { $"FrameProcessor {this}: Disposed" }, LogRecordLevel.Debug));
#endif
        }
    }
}