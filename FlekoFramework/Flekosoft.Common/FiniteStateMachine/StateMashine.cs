using Flekosoft.Common.Messaging;

namespace Flekosoft.Common.FiniteStateMachine
{
    /// <summary>
    ///Finite-state machine
    /// </summary>
    /// <typeparam name="T">Тип, для которого реализуется конечный автомат</typeparam>
    public class StateMashine<T> : PropertyChangedErrorNotifyDisposableBase, IMessageHandler
    {
        /// <summary>
        /// Родитель данного экземпляра
        /// </summary>
        private readonly T _owner;

        /// <summary>
        /// Текущее состояние родителя
        /// </summary>
        private State<T> _currentState;

        /// <summary>
        /// Предыдущее состояние
        /// </summary>
        private State<T> _previousState;

        /// <summary>
        /// Глобальное состояние. Вызывается каждяй раз при обновлении
        /// </summary>
        private State<T> _globalState;

        public StateMashine(T owner)
        {
            _owner = owner;
            _currentState = null;
            _previousState = null;
            _globalState = null;
        }

        #region properties

        /// <summary>
        /// Текущее состояние родителя
        /// </summary>
        public State<T> CurrentState
        {
            get { return _currentState; }
        }

        /// <summary>
        /// Предыдущее состояние
        /// </summary>
        public State<T> PreviousState
        {
            get { return _previousState; }
        }

        /// <summary>
        /// Глобальное состояние. Вызывается каждяй раз при обновлении
        /// </summary>
        public State<T> GlobalState
        {
            get { return _globalState; }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Обновление состояния
        /// </summary>
        public void Update(double timeDeltaMs)
        {
            _globalState?.Execute(_owner, timeDeltaMs);
            _currentState?.Execute(_owner, timeDeltaMs);
        }

        /// <summary>
        /// Обработчик сообщения. См. Common.Messaging
        /// </summary>
        /// <param name="message">Полученное сообщение</param>
        /// <returns></returns>
        public bool HandleMessage(Message message)
        {
            bool globalRes = false;
            bool currentRes = false;
            //Отправляем сообщение глобальному состоянию
            if (_globalState != null)
            {
                globalRes = _globalState.OnMessage(_owner, message);
            }

            //Если глобальное состояние не обработало сообщение, то отправляем текущему.
            if (!globalRes)
            {
                if (_currentState != null)
                {
                    currentRes = _currentState.OnMessage(_owner, message);
                }
            }
            return globalRes || currentRes;
        }

        /// <summary>
        /// Переход в новое состояние
        /// </summary>
        /// <param name="newState">новое состояние</param>
        public void SetState(State<T> newState)
        {
            if (_currentState != null && _currentState.Equals(newState)) return;

            _previousState = _currentState;
            OnPropertyChanged(nameof(PreviousState));

            _currentState?.Exit(_owner);
            _currentState = newState;
            _currentState?.Enter(_owner);
            OnPropertyChanged(nameof(CurrentState));


            if (_previousState == null)
            {
                AppendDebugLogMessage(_owner + " StateMashine.\tState changed to " + _currentState);
            }
            else
            {
                AppendDebugLogMessage(_owner + " StateMashine.\tState changed from: " + _previousState + " to " +
                                            _currentState);
            }
        }

        /// <summary>
        /// Установка нового глобального состояния
        /// </summary>
        /// <param name="newGlobalState">новое состояние</param>
        public void SetGlobalState(State<T> newGlobalState)
        {
            if(_globalState != null && _globalState.Equals(newGlobalState)) return;

            _globalState?.Exit(_owner);
            if (newGlobalState != null)
            {
                _globalState = newGlobalState;
                _globalState.Enter(_owner);
                OnPropertyChanged(nameof(GlobalState));
            }
        }

        /// <summary>
        /// Вернуться к предыдущему состоянию
        /// </summary>
        public void RevertToPreviousState()
        {
            SetState(_previousState);
        }

        /// <summary>
        /// Находится ли объект в указанном состоянии
        /// </summary>
        /// <param name="state">Состояние для проверки</param>
        /// <returns></returns>
        public bool IsInState(State<T> state)
        {
            if (_currentState.Equals(state)) return true;
            return false;
        }

#if DEBUG
        /// <summary>
        /// only ever used during debugging to grab the name of the current state
        /// </summary>
        /// <returns></returns>
        public string GetNameOfCurrentState()
        {
            return _currentState.ToString();
        }
#endif

        #endregion
    }
}
