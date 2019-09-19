using Flekosoft.Common.Messaging;

namespace Flekosoft.Common.FiniteStateMachine
{
    /// <summary>
    /// State for finite state machine
    /// </summary>
    /// <typeparam name="T">Type for which this state is implemented</typeparam>
    public abstract class State<T>
    {
        /// <summary>
        /// Runs on entering state
        /// </summary>
        /// <param name="subject">The object that went into this state</param>
        public abstract void Enter(T subject);

        /// <summary>
        /// State handler. This is the states normal update function.
        /// </summary>
        /// <param name="subject">The object that went into this state</param>
        /// <param name="timeDeltaMs">Elapsed time from last call</param>
        public abstract void Execute(T subject, double timeDeltaMs);

        /// <summary>
        /// Runs when exiting the state.
        /// </summary>
        /// <param name="subject">The object that went into this state</param>
        public abstract void Exit(T subject);

        /// <summary>
        /// The handler of the received message.
        /// </summary>
        /// <param name="subject">The object that went into this state</param>
        /// <param name="message">A message has arrived. See <see cref="Common.Messaging"/></param>
        /// <returns></returns>
        public abstract bool OnMessage(T subject, Message message);
    }
}
