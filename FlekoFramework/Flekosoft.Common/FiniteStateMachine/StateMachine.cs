using Flekosoft.Common.Messaging;

namespace Flekosoft.Common.FiniteStateMachine
{
    /// <summary>
    ///Finite-state machine
    /// </summary>
    /// <typeparam name="T">The type for which the state machine is implemented</typeparam>
    public class StateMachine<T> : PropertyChangedErrorNotifyDisposableBase, IMessageHandler
    {
        /// <summary>
        /// Owner. The type for which the state machine is implemented
        /// </summary>
        private readonly T _owner;

        /// <summary>
        /// Owner's current state
        /// </summary>
        private State<T> _currentState;

        /// <summary>
        /// Owner's previous state
        /// </summary>
        private State<T> _previousState;

        /// <summary>
        /// Owner's Global state. It is called every time when updating.
        /// </summary>
        private State<T> _globalState;

        public StateMachine(T owner)
        {
            _owner = owner;
            _currentState = null;
            _previousState = null;
            _globalState = null;
        }

        #region properties

        /// <summary>
        /// Owner's current state
        /// </summary>
        public State<T> CurrentState => _currentState;

        /// <summary>
        /// Owner's previous state
        /// </summary>
        public State<T> PreviousState => _previousState;

        /// <summary>
        /// Owner's Global state. It is called every time when updating.
        /// </summary>
        public State<T> GlobalState => _globalState;

        #endregion

        #region Methods

        /// <summary>
        /// State update
        /// </summary>
        /// <param name="timeDeltaMs">Time in ms passed from last update</param>
        public void Update(double timeDeltaMs)
        {
            _globalState?.Execute(_owner, timeDeltaMs);
            _currentState?.Execute(_owner, timeDeltaMs);
        }

        /// <summary>
        /// The handler of the received message.
        /// </summary>
        /// <param name="message">A message has arrived. See <see cref="Common.Messaging"/></param>
        /// <returns></returns>
        public bool HandleMessage(Message message)
        {
            bool globalRes = false;
            bool currentRes = false;
            //Send a message to the global state
            if (_globalState != null)
            {
                globalRes = _globalState.OnMessage(_owner, message);
            }

            //If the global state has not processed the message, then send it to the current one.
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
        /// Transition to a new state
        /// </summary>
        /// <param name="newState">New state</param>
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
                AppendDebugLogMessage(_owner + " StateMachine.\tState changed to " + _currentState);
            }
            else
            {
                AppendDebugLogMessage(_owner + " StateMachine.\tState changed from: " + _previousState + " to " +
                                            _currentState);
            }
        }

        /// <summary>
        /// Setting a new global state
        /// </summary>
        /// <param name="newGlobalState">New global state</param>
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
        /// Return to previous state
        /// </summary>
        public void RevertToPreviousState()
        {
            SetState(_previousState);
        }

        /// <summary>
        /// Is the object in the specified state
        /// </summary>
        /// <param name="state">State to check</param>
        /// <returns></returns>
        public bool IsInState(State<T> state)
        {
            if (_currentState.Equals(state)) return true;
            return false;
        }

        #endregion
    }
}
