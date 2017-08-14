﻿using System;
using System.Threading;

namespace Flekosoft.Common.Network.Tcp.Internals
{
    public abstract class AsyncNetworkExchangeDriver : PropertyChangedErrorNotifyDisposableBase
    {
        private bool _isStarted;

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

        private INetworkExchangeInterface _networkInterface;

        private object _writeWyncObject = new object();

        protected AsyncNetworkExchangeDriver()
        {
            ReadBufferSize = 1024;

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
            private set
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

        #endregion

        #region Threads

        private void ReadDataThreadFunc()
        {
            while (true)
            {
                try
                {
                    if (IsStarted && _networkInterface != null && _networkInterface.IsConnected)
                    {
                        lock (_readBufferSyncObject)
                        {
                            //Clear buffer. 
                            //TODO: May be we will not need to do it. Will see after use experiance
                            //for (int i = 0; i < _readBuffer.Length; i++)
                            //{
                            //    _readBuffer[i] = 0x00;
                            //}

                            int count = _networkInterface.Read(_readBuffer);
                            if (count > 0)
                            {
                                for (int i = 0; i < count; i++)
                                {
                                    ProcessByteInternal(_readBuffer[i]);

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
        //                            throw new NetworkWriteException($"{Name}.SendDataSync: Send error. bytes to send " +
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
        public void Start(INetworkExchangeInterface networkInterface)
        {
            _networkInterface = networkInterface;
            IsStarted = true;
        }

        /// <summary>
        /// Disconnect and stop cilent
        /// </summary>
        public void Stop()
        {
            IsStarted = false;
            _networkInterface = null;
        }

        protected bool Write(byte[] data)
        {
            if (_networkInterface.IsConnected)
            {
                if (data != null)
                {
                    lock (_writeWyncObject)
                    {
                        var written = _networkInterface?.Write(data);
                        if (written != data.Length)
                        {
                            throw new NetworkWriteException($"{Name}.SendDataSync: Send error. bytes to send " +
                                                            data.Length + " but sent " +
                                                            written + " bytes ");
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

        #endregion

        protected abstract void ProcessByteInternal(byte data);

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

        //public event EventHandler<NetworkDataEventArgs> ReceivedDataEvent;
        //private void OnReceivedDataEvent(NetworkDataEventArgs e)
        //{
        //    ReceivedDataEvent?.Invoke(this, e);
        //}

        //public event EventHandler<NetworkDataEventArgs> DataSentEvent;
        //private void OnDataSentEvent(NetworkDataEventArgs e)
        //{
        //    DataSentEvent?.Invoke(this, e);
        //}

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
                //DataSentEvent = null;
                //ReceivedDataEvent = null;
            }
            base.Dispose(disposing);
        }
        #endregion
    }
}
