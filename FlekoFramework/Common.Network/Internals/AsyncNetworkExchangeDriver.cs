using System;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading;

[assembly: InternalsVisibleTo("Flekosoft.UnitTests")]
namespace Flekosoft.Common.Network.Internals
{
    public abstract class AsyncNetworkExchangeDriver : PropertyChangedErrorNotifyDisposableBase
    {
        private bool _isStarted;
        private bool _isReadThreadEnabled;

        //private readonly Thread _sendDataThread;
        private readonly Thread _readDataThread;
        //private readonly Thread _processDataThread;

        //private readonly ConcurrentQueue<byte[]> _sendDataQueue = new ConcurrentQueue<byte[]>();
        //private EventWaitHandle _hasDataToSendWh = new EventWaitHandle(false, EventResetMode.ManualReset);
        //private readonly object _hasDataToSendLockWhObject = new object();

        //private readonly ConcurrentQueue<byte> _processDataQueue = new ConcurrentQueue<byte>();
        //private EventWaitHandle _hasDataToProcessWh = new EventWaitHandle(false, EventResetMode.ManualReset);
        //private readonly object _hasDataToProcessWhLockObject = new object();

        private byte[] _readBuffer;
        private readonly object _readBufferSyncObject = new object();

        private int _readBufferSize;

        private readonly object _networkInterfaceReadSyncObject = new object();
        private readonly object _networkInterfaceWriteSyncObject = new object();
        private INetworkExchangeInterface _networkInterface;

        private readonly object _writeSyncObject = new object();
        private bool _dataTrace;

        /// <summary>
        /// 
        /// </summary>
        protected AsyncNetworkExchangeDriver()
        {
            ReadBufferSize = 1024;
            DataTrace = false;

            //_networkInterface = networkInterface;

            //_sendDataThread = new Thread(WriteDataThreadFunc);
            //_sendDataThread.Start();
            _readDataThread = new Thread(ReadDataThreadFunc);
            _readDataThread.Start();
            //_processDataThread = new Thread(ProcessDataThreadFunc);
            //_processDataThread.Start();
        }

        #region Properties

        /// <summary>
        /// Is client started
        /// </summary>
        public bool IsStarted
        {
            get { return _isStarted; }
            protected set
            {
                if (_isStarted != value)
                {
                    _isStarted = value;
                    OnPropertyChanged(nameof(IsStarted));
                    if (_isStarted) OnStartedEvent();
                    else OnStoppedEvent();
                }
            }
        }

        public INetworkExchangeInterface ExchangeInterface { get { return _networkInterface; } }

        /// <summary>
        /// Socket read buffer size
        /// </summary>
        public int ReadBufferSize
        {
            get { return _readBufferSize; }
            set
            {
                if (_readBufferSize != value)
                {
                    _readBufferSize = value;
                    lock (_readBufferSyncObject)
                    {
                        _readBuffer = new byte[_readBufferSize];
                    }
                    OnPropertyChanged(nameof(ReadBufferSize));
                }
            }
        }

        /// <summary>
        /// Send trace events on data receive/send
        /// </summary>
        public bool DataTrace
        {
            get { return _dataTrace; }
            set
            {
                if (_dataTrace != value)
                {
                    _dataTrace = value;
                    OnPropertyChanged(nameof(DataTrace));
                }
            }
        }

        #endregion

        #region Threads

        private void ReadDataThreadFunc()
        {
            while (true)
            {
                try
                {
                    lock (_networkInterfaceReadSyncObject)
                    {
                        if (_isReadThreadEnabled && IsStarted && _networkInterface?.IsConnected == true)
                        {
                            lock (_readBufferSyncObject)
                            {
                                int count = _networkInterface.Read(_readBuffer, Timeout.Infinite);
                                if (count > 0)
                                {
                                    if (DataTrace) OnReceiveDataTraceEvent(_readBuffer.ToList().GetRange(0, count).ToArray(), _networkInterface.LocalEndPoint, _networkInterface.RemoteEndPoint);
                                    for (int i = 0; i < count; i++)
                                    {
                                        ProcessByteInternal(new NetworkDataEventArgs(new[] { _readBuffer[i] }, ExchangeInterface.LocalEndPoint, ExchangeInterface.RemoteEndPoint));


                                        //_processDataQueue.Enqueue(_readBuffer[i]);
                                        //var data = new byte[count];
                                        //Array.Copy(_readBuffer, data, count);
                                        //OnReceivedDataEvent(new NetworkDataEventArgs(data, _networkInterface?.RemoteEndpoint));
                                    }
                                    //lock (_hasDataToProcessWhLockObject)
                                    //{
                                    //    if (_processDataQueue.IsEmpty) _hasDataToProcessWh?.Reset();
                                    //    else _hasDataToProcessWh?.Set();
                                    //}
                                }
                            }
                        }
                        else Thread.Sleep(1);
                    }
                }
                catch (ThreadAbortException)
                {
                    break;
                }
                catch (Exception exception)
                {
                    OnErrorEvent(exception);
                    //lock (_hasDataToProcessWhLockObject)
                    //{
                    //    if (_processDataQueue.IsEmpty) _hasDataToProcessWh?.Reset();
                    //    else _hasDataToProcessWh?.Set();
                    //}
                }
            }
        }

        //private void ProcessDataThreadFunc()
        //{
        //    while (true)
        //    {
        //        try
        //        {
        //            if (IsStarted)
        //            {
        //                if (_hasDataToProcessWh != null && _hasDataToProcessWh.WaitOne(Timeout.Infinite))
        //                {
        //                    byte dataByte;
        //                    if (_processDataQueue.TryDequeue(out dataByte))
        //                    {
        //                        ProcessByteInternal(dataByte);
        //                    }
        //                    lock (_hasDataToProcessWhLockObject)
        //                    {
        //                        if (_processDataQueue.IsEmpty) _hasDataToProcessWh?.Reset();
        //                        else _hasDataToProcessWh?.Set();
        //                    }
        //                }
        //            }
        //            else Thread.Sleep(1);
        //        }
        //        catch (ThreadAbortException)
        //        {
        //            break;
        //        }
        //        catch (Exception ex)
        //        {
        //            OnErrorEvent(ex);
        //            lock (_hasDataToProcessWhLockObject)
        //            {
        //                if (_processDataQueue.IsEmpty) _hasDataToProcessWh?.Reset();
        //                else _hasDataToProcessWh?.Set();
        //            }
        //        }
        //    }
        //}

        //private void WriteDataThreadFunc()
        //{
        //    while (true)
        //    {
        //        try
        //        {
        //            if (IsStarted && _networkInterface != null && _networkInterface.IsConnected)
        //            {
        //                if (_hasDataToSendWh != null && _hasDataToSendWh.WaitOne(Timeout.Infinite))
        //                {

        //                    byte[] data;
        //                    if (_sendDataQueue.TryDequeue(out data))
        //                    {
        //                        var written = _networkInterface.Write(data);
        //                        if (written != data.Length)
        //                        {
        //                            throw new NetworkWriteException($"{InstanceName}.SendDataSync: Send error. bytes to send " +
        //                                                                   data.Length + " but sent " +
        //                                                                   written + " bytes ");
        //                        }
        //                        //OnDataSentEvent(new NetworkDataEventArgs(data, _networkInterface.RemoteEndpoint));
        //                    }
        //                    lock (_hasDataToSendLockWhObject)
        //                    {
        //                        if (_sendDataQueue.IsEmpty) _hasDataToSendWh?.Reset();
        //                        else _hasDataToSendWh?.Set();
        //                    }
        //                }
        //            }
        //            else Thread.Sleep(1);
        //        }
        //        catch (ThreadAbortException)
        //        {
        //            break;
        //        }
        //        catch (Exception ex)
        //        {
        //            OnErrorEvent(ex);
        //            lock (_hasDataToSendLockWhObject)
        //            {
        //                if (_sendDataQueue.IsEmpty) _hasDataToSendWh?.Reset();
        //                else _hasDataToSendWh?.Set();
        //            }
        //        }
        //    }
        //}

        #endregion

        #region Methods

        /// <summary>
        /// Start
        /// </summary>
        /// <param name="networkInterface">interface to send and receive data</param>
        internal void StartExchange(INetworkExchangeInterface networkInterface)
        {
            StartExchange(networkInterface, true);
        }

        /// <summary>
        /// Start
        /// </summary>
        /// <param name="networkInterface">interface to send and receive data</param>
        /// <param name="startReadThread"> specify is read thread should be enabled</param>
        internal void StartExchange(INetworkExchangeInterface networkInterface, bool startReadThread)
        {
            lock (_networkInterfaceReadSyncObject)
            {
                lock (_networkInterfaceWriteSyncObject)
                {
                    _networkInterface = networkInterface;
                }
            }

            _isReadThreadEnabled = startReadThread;
            IsStarted = true;
        }

        /// <summary>
        /// Disconnect and stop cilent
        /// </summary>
        internal void StopExchange()
        {
            IsStarted = false;
            _isReadThreadEnabled = false;
            //lock (_networkInterfaceReadSyncObject)
            {
                //lock (_networkInterfaceWriteSyncObject)
                {
                    _networkInterface?.Dispose();
                    _networkInterface = null;
                }
            }
        }

        protected bool Write(byte[] data)
        {
            try
            {
                if (_networkInterface?.IsConnected == true)
                {
                    if (data != null)
                    {
                        lock (_writeSyncObject)
                        {
                            lock (_networkInterfaceWriteSyncObject)
                            {
                                var index = 0;
                                var written = 0;
                                var len = data.Length;
                                while (written < data.Length)
                                {
                                    written = (int)_networkInterface?.Write(data, index, len, Timeout.Infinite);
                                    index += written;
                                    len -= written;
                                    ////if (written != data.Length)
                                    ////{
                                    ////    throw new NetworkWriteException($"{InstanceName}.SendDataSync: Send error. bytes to send " +
                                    ////                                    data.Length + " but sent " +
                                    ////                                    written + " bytes ");
                                    ////}
                                }
                                if (DataTrace)
                                    OnSendDataTraceEvent(data, _networkInterface.LocalEndPoint,
                                        _networkInterface.RemoteEndPoint);
                            }
                        }
                        //_sendDataQueue.Enqueue(data);
                        //lock (_hasDataToSendLockWhObject)
                        //{
                        //    if (_sendDataQueue.IsEmpty) _hasDataToSendWh?.Reset();
                        //    else _hasDataToSendWh?.Set();
                        //}
                        return true;
                    }
                    return false;
                }
                return false;
            }
            catch (Exception exception)
            {
                OnErrorEvent(exception);
                //lock (_hasDataToSendLockWhObject)
                //{
                //    if (_sendDataQueue.IsEmpty) _hasDataToSendWh?.Reset();
                //    else _hasDataToSendWh?.Set();
                //}
                return false;
            }
        }

        protected abstract void ProcessByteInternal(NetworkDataEventArgs e);

        #endregion



        #region events

        public event EventHandler StartedEvent;
        private void OnStartedEvent()
        {
            StartedEvent?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler StoppedEvent;
        private void OnStoppedEvent()
        {
            StoppedEvent?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler<NetworkDataEventArgs> ReceiveDataTraceEvent;
        private void OnReceiveDataTraceEvent(byte[] data, IPEndPoint localEndPoint, IPEndPoint remoteEndPoint)
        {
            ReceiveDataTraceEvent?.Invoke(this, new NetworkDataEventArgs(data, localEndPoint, remoteEndPoint));
        }

        public event EventHandler<NetworkDataEventArgs> SendDataTraceEvent;
        private void OnSendDataTraceEvent(byte[] data, IPEndPoint localEndPoint, IPEndPoint remoteEndPoint)
        {
            SendDataTraceEvent?.Invoke(this, new NetworkDataEventArgs(data, localEndPoint, remoteEndPoint));
        }

        #endregion

        #region Dispodable
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                //if (_sendDataThread != null)
                //{
                //    if (_sendDataThread.IsAlive)
                //    {
                //        _sendDataThread.Abort();
                //    }
                //}

                //lock (_hasDataToSendLockWhObject)
                //{
                //    if (_hasDataToSendWh != null)
                //    {
                //        _hasDataToSendWh.Set();
                //        _hasDataToSendWh.Close();
                //        _hasDataToSendWh = null;
                //    }
                //}

                if (_readDataThread != null)
                {
                    if (_readDataThread.IsAlive)
                    {
                        _readDataThread.Abort();
                    }
                }

                //if (_processDataThread != null)
                //{
                //    if (_processDataThread.IsAlive)
                //    {
                //        _processDataThread.Abort();
                //    }
                //}

                //lock (_hasDataToProcessWhLockObject)
                //{
                //    if (_hasDataToProcessWh != null)
                //    {
                //        _hasDataToProcessWh.Set();
                //        _hasDataToProcessWh.Close();
                //        _hasDataToProcessWh = null;
                //    }
                //}

                StartedEvent = null;
                StoppedEvent = null;
                ReceiveDataTraceEvent = null;
                SendDataTraceEvent = null;

                // ReSharper disable once InconsistentlySynchronizedField
                _networkInterface?.Dispose();
            }
            base.Dispose(disposing);
        }
        #endregion
    }
}
