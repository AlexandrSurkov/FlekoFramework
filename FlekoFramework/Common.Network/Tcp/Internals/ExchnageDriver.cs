using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using Flekosoft.Common.Logging;

namespace Flekosoft.Common.Network.Tcp.Internals
{
    class ExchnageDriver : IDisposable
    {
        //private IPEndPoint _remoteEndPoint;
        
        /// <summary>
        /// Поток отправки сообщений
        /// </summary>
        private Thread _sndDataThread;

        /// <summary>
        /// Поток получения байт из интерфейса
        /// </summary>
        private Thread _readBytesThread;

        /// <summary>
        /// Поток получения сообщений
        /// </summary>
        private Thread _readDataThread;


        //Буфер данных для отправки и его синхронизатор
        private readonly ConcurrentQueue<byte[]> _sndDataFifo = new ConcurrentQueue<byte[]>();
        private EventWaitHandle _sndDataWh = new EventWaitHandle(false, EventResetMode.ManualReset);
        private readonly object _sndDataWhLockObject = new object();

        //Буфер для хранения данных, считанных из интерфейса
        private readonly ConcurrentQueue<byte> _rcvDataFifo = new ConcurrentQueue<byte>();
        private EventWaitHandle _rcvDataWh = new EventWaitHandle(false, EventResetMode.ManualReset);
        private readonly object _rcvDataWhLockObject = new object();
        private readonly object _rcvDataFifoLockObject = new object();

        ////Буфер синхронных данных для приема и его синхронизатор
        //private readonly ConcurrentQueue<MessageData> _rcvDataFifo = new ConcurrentQueue<MessageData>();
        //private EventWaitHandle _rcvDataWh = new EventWaitHandle(false, EventResetMode.ManualReset);
        //private object _rcvDataWhLockObject = new object();

        ////Буфер синхронных событий для приема и его синхронизатор
        //private readonly ConcurrentQueue<MessageData> _rcvEventFifo = new ConcurrentQueue<MessageData>();
        //private EventWaitHandle _rcvEventWh = new EventWaitHandle(false, EventResetMode.ManualReset);
        //private object _rcvEventWhLockObject = new object();



        public ExchnageDriver()
        {
            
        }

        /// <summary>
        /// Отправить данные
        /// </summary>
        /// <param name="data"></param>
        public void SendData(byte[] data)
        {
            if (data != null)
            {
                _sndDataFifo.Enqueue(new List<byte>(data).ToArray());
            }

            lock (_sndDataWhLockObject)
            {
                if (_sndDataWh != null && !_sndDataFifo.IsEmpty) _sndDataWh.Set();
                else _sndDataWh?.Reset();
            }
        }

        /// <summary>
        /// Сихнронная отправка данных. Используется в потоке
        /// </summary>
        /// <param name="dataToSend"></param>
        protected void SendDataSync(ReadOnlyCollection<byte> dataToSend)
        {
            if (!IsConnected)
            {
                OnDisconnectedEvent();
                throw new ExchangeException("ExchangeDriver: Not connected");
            }
            try
            {
                var data = new List<byte>(dataToSend).ToArray();

                var sendedBytes = ExchangeInterface.Write(data);
                OnSendedDataEvent(data);
                if (data.Length != sendedBytes)
                {
                    throw new ExchangeException("ExchangeDriver.SendDataSync: Send error: bytes to send " +
                                                               data.Length.ToString(CultureInfo.CurrentCulture) + " but sent " +
                                                               sendedBytes.ToString(CultureInfo.CurrentCulture) + " bytes ");
                }
            }
            catch (TcpSocketNotConnectedException)
            {
                IsConnected = false;
            }


        }

        ///// <summary>
        ///// Запрос определенного количества данных с таймаутом
        ///// </summary>
        ///// <param name="length"></param>
        ///// <param name="timeout"></param>
        ///// <returns></returns>
        //protected byte[] GetDataSync(int length, int timeout)
        //{
        //    var data = new byte[length];

        //    int index = 0;

        //    DateTime start = DateTime.Now;

        //    while (index < length)
        //    {
        //        //Вычитываем данные из интерфейса побайтно с таймаутом 10 мс
        //        var rd = _exchangeInterface.Read(1, 10);
        //        if (rd.Length == 1)
        //        {
        //            data[index] = rd[0];
        //            index++;
        //        }

        //        //Проверяем на достижение таймаута
        //        DateTime now = DateTime.Now;
        //        if ((now - start).TotalMilliseconds >= timeout)
        //        {
        //            throw new ExchangeException("ExchangeDriver.GetDataSync Timeout. Letngth: " + length + " timeout: " + timeout + " index: " + index);
        //        }
        //    }
        //    return data;
        //}

        /// <summary>
        /// Запускаем потоки
        /// </summary>
        public void StartWork(TcpExchangeInterface exchangeInterface)
        {
            ExchangeInterface = exchangeInterface;
            ExchangeInterface.ErrorEvent += _exchangeInterface_ErrorEvent;
            _remoteEndPoint = exchangeInterface.RemoteEndpoint;

            _sndDataThread = new Thread(SendDataThread)
            {
                //Priority = ThreadPriority.AboveNormal,
                CurrentCulture = _cultureInfo,
                CurrentUICulture = _cultureInfo
            };
            _sndDataThread.Start();

            _readBytesThread = new Thread(ReadBytesThread)
            {
                //Priority = ThreadPriority.AboveNormal,
                CurrentCulture = _cultureInfo,
                CurrentUICulture = _cultureInfo
            };
            _readBytesThread.Start();

            _readDataThread = new Thread(ReadDataThread)
            {
                //Priority = ThreadPriority.AboveNormal,
                CurrentCulture = _cultureInfo,
                CurrentUICulture = _cultureInfo
            };
            _readDataThread.Start();

            //_dataParseThread = new Thread(DataParseThread)
            //{
            //    //Priority = ThreadPriority.AboveNormal,
            //    CurrentCulture = _cultureInfo,
            //    CurrentUICulture = _cultureInfo
            //};
            //_dataParseThread.Start();

            //_eventParseThread = new Thread(EventParseThread)
            //{
            //    //Priority = ThreadPriority.AboveNormal,
            //    CurrentCulture = _cultureInfo,
            //    CurrentUICulture = _cultureInfo
            //};
            //_eventParseThread.Start();

            if (!IsConnected)
            {
                IsConnected = true;
                OnConnectedEvent();
            }
        }

        public void StopWork()
        {
            if (_sndDataThread != null)
            {
                if (_sndDataThread.IsAlive)
                {
                    _sndDataThread.Abort();
                }

                _sndDataThread = null;
            }

            //if (_eventParseThread != null)
            //{
            //    if (_eventParseThread.IsAlive)
            //    {
            //        _eventParseThread.Abort();
            //    }

            //    _eventParseThread = null;
            //}

            //if (_dataParseThread != null)
            //{
            //    if (_dataParseThread.IsAlive)
            //    {
            //        _dataParseThread.Abort();
            //    }

            //    _dataParseThread = null;
            //}

            if (_readDataThread != null)
            {
                if (_readDataThread.IsAlive)
                {
                    _readDataThread.Abort();
                }

                _readDataThread = null;
            }

            if (_readBytesThread != null)
            {
                if (_readBytesThread.IsAlive)
                {
                    _readBytesThread.Abort();
                }

                _readBytesThread = null;
            }

            if (ExchangeInterface != null)
            {
                ExchangeInterface.Dispose();
                ExchangeInterface = null;
            }

            if (IsConnected)
            {
                IsConnected = false;
                OnDisconnectedEvent();
            }


        }

        #region threads

        /// <summary>
        /// Поток для отправки данных
        /// </summary>
        /// <param name="o"></param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        protected virtual void SendDataThread(object o)
        {
            while (true)
            {
                try
                {
                    //Ожидаем наличия данных в буфере отправки иногда отпуская поток для переключения контекстов
                    if (_sndDataWh != null && _sndDataWh.WaitOne(Timeout.Infinite, false))
                    {
                        if (!_sndDataFifo.IsEmpty)
                        {
                            byte[] data;
                            if (_sndDataFifo.TryDequeue(out data))
                            {
                                if (data != null)
                                {
                                    try
                                    {
                                        //Отправляем
                                        SendDataSync(new List<byte>(data).AsReadOnly());
                                        //break; //Выходим из цикла отправки (while (true)) так как данные были отправлены
                                    }
                                    catch (ExchangeException ex)
                                    {
                                        //Произошла ошибка отправки. Пытаемся отпавить данные снова.
                                        Logger.AppendException(new ExchangeException("NetworkExchangeDriver.SendDataThread Error", ex));
                                        OnErrorEvent(ex);
                                    }
                                }

                            }
                            lock (_sndDataWhLockObject)
                            {
                                if (_sndDataWh != null && !_sndDataFifo.IsEmpty) _sndDataWh.Set();
                                else _sndDataWh.Reset();
                            }
                        }
                    }
                }
                catch (ThreadAbortException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Logger.AppendException(new Exception("SendDataThread Error", ex));
                    OnErrorEvent(ex);
                    //throw;
                }
            }
        }

        /// <summary>
        /// Поток, который побайтно считывает данные из интерфейса обмена и складывает их в буфер.
        /// </summary>
        /// <param name="o"></param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        protected virtual void ReadBytesThread(object o)
        {
            while (true)
            {
                try
                {
                    //Минимальная пауза для переключения контекстов потоков
                    //Thread.Sleep(1);

                    //Считываем данные из интерфейса побайтно. 
                    //Если читать не побайтно, то возможна потеря данных при ошибках таймаута
                    //При этом, игнорируем ошибки чтения так как нам интересны только прочитанные данные.
                    try
                    {
                        if (!ExchangeInterface.IsConnected)
                        {
                            Thread.Sleep(1);
                            continue;
                        }
                        var rd = ExchangeInterface.Read(1, Timeout.Infinite);
                        //И помещаем данные в буфер
                        // ReSharper disable once ForCanBeConvertedToForeach
                        for (int i = 0; i < rd.Length; i++)
                        {
                            lock (_interfaceDataFifoLockObject)
                            {
                                _rcvDataFifo.Enqueue(rd[i]);
                            }
                        }
                    }
                    catch (ExchangeException)
                    {

                    }
                    catch (TcpSocketNotConnectedException)
                    {
                        IsConnected = false;
                    }

                    lock (_interfaceDataWhLockObject)
                    {
                        if (_rcvDataFifo.IsEmpty) _interfaceDataWh?.Reset();
                        else _interfaceDataWh?.Set();
                    }
                }
                catch (ThreadAbortException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    lock (_interfaceDataWhLockObject)
                    {
                        if (_rcvDataFifo.IsEmpty) _interfaceDataWh?.Reset();
                        else _interfaceDataWh?.Set();
                    }

                    Logger.AppendException(new ExchangeException("NetworkExchangeDriver.ReadBytesThread Error", ex));
                    OnErrorEvent(ex);
                }
            }
        }

        /// <summary>
        /// Поток, в котором обрабатываются пришедшие данные
        /// </summary>
        /// <param name="o"></param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        protected virtual void ReadDataThread(object o)
        {
            while (true)
            {
                try
                {
                    //Ожидаем пока буфер будет не пустой
                    if (_interfaceDataWh != null && _interfaceDataWh.WaitOne(Timeout.Infinite, false))
                    {
                        byte bt;

                        lock (_interfaceDataFifoLockObject)
                        {
                            if (!_rcvDataFifo.TryDequeue(out bt)) continue;
                            OnReceivedDataEvent(new[] { bt });
                        }
                        lock (_interfaceDataWhLockObject)
                        {
                            if (_rcvDataFifo.IsEmpty) _interfaceDataWh?.Reset();
                            else _interfaceDataWh?.Set();
                        }
                    }
                }
                catch (ThreadAbortException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    lock (_interfaceDataWhLockObject)
                    {
                        if (_rcvDataFifo.IsEmpty) _interfaceDataWh?.Reset();
                        else _interfaceDataWh?.Set();
                    }
                    Logger.AppendException(new ExchangeException("NetworkExchangeDriver.ReadDataThread Error", ex));
                    OnErrorEvent(ex);
                }
            }
        }

        #endregion

        #region properties

        public bool IsConnected { get; protected set; }

        /// <summary>
        /// Снхронный интерфейс обмена
        /// </summary>
        protected TcpExchangeInterface ExchangeInterface { get; set; }

        public IPEndPoint RemoteEndpoint
        {
            get { return _remoteEndPoint; }
        }

        #endregion

        #region Event handlers

        private void _exchangeInterface_ErrorEvent(object sender, GeneralErrorEventArgs e)
        {
            OnErrorEvent(e.GetException());
        }
        #endregion

        #region events

        public event EventHandler<GeneralErrorEventArgs> ErrorEvent;
        protected void OnErrorEvent(Exception exception)
        {
            ErrorEvent?.Invoke(this, new GeneralErrorEventArgs(DateTime.Now, exception));
        }

        public event EventHandler<TcpDataEventArgs> ReceivedDataEvent;
        private void OnReceivedDataEvent(byte[] data)
        {
            ReceivedDataEvent?.Invoke(this, new TcpDataEventArgs(data, RemoteEndpoint));
        }

        public event EventHandler<TcpDataEventArgs> SendedDataEvent;
        private void OnSendedDataEvent(byte[] data)
        {
            SendedDataEvent?.Invoke(this, new TcpDataEventArgs(data, RemoteEndpoint));
        }

        public event EventHandler DisconnectedEvent;
        private void OnDisconnectedEvent()
        {
            DisconnectedEvent?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler ConnectedEvent;
        private void OnConnectedEvent()
        {
            ConnectedEvent?.Invoke(this, EventArgs.Empty);
        }

        #endregion

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
                    StopWork();

                    ExchangeInterface?.Dispose();

                    ErrorEvent = null;
                    ConnectedEvent = null;
                    DisconnectedEvent = null;
                    ReceivedDataEvent = null;
                    SendedDataEvent = null;


                    if (_sndDataWh != null)
                    {
                        _sndDataWh.Set();
                        _sndDataWh.Close();
                        _sndDataWh = null;
                    }

                    if (_interfaceDataWh != null)
                    {
                        _interfaceDataWh.Set();
                        _interfaceDataWh.Close();
                        _interfaceDataWh = null;
                    }

                    //if (_rcvDataWh != null)
                    //{
                    //    _rcvDataWh.Set();
                    //    _rcvDataWh.Close();
                    //    _rcvDataWh = null;
                    //}

                    //if (_rcvEventWh != null)
                    //{
                    //    _rcvEventWh.Set();
                    //    _rcvEventWh.Close();
                    //    _rcvEventWh = null;
                    //}
                }
                _disposed = true;
            }
        }
        #endregion
    }
}
