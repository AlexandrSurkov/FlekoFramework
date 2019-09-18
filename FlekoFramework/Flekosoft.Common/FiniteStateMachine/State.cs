using Flekosoft.Common.Messaging;

namespace Flekosoft.Common.FiniteStateMachine
{
    /// <summary>
    /// Состояние, в конечном автомате
    /// </summary>
    /// <typeparam name="T">Тип, для которого реализовано данное состояние</typeparam>
    public abstract class State<T>
    {
        /// <summary>
        /// Выполняется при входе в состояние
        /// </summary>
        /// <param name="subject">Объект, который перешел в данное состояние</param>
        public abstract void Enter(T subject);

        /// <summary>
        /// Обработчик состояния. This is the states normal update function
        /// </summary>
        /// <param name="subject">Объект, который перешел в данное состояние</param>
        /// <param name="timeDeltaMs">Времы, прошедшее с последнего вызова</param>
        public abstract void Execute(T subject, double timeDeltaMs);

        /// <summary>
        /// Выполняется при выходе из фостояния
        /// </summary>
        /// <param name="subject">Объект, который перешел в данное состояние</param>
        public abstract void Exit(T subject);

        /// <summary>
        /// Обработчик полученного сообщения.
        /// </summary>
        /// <param name="subject">Объект, который перешел в данное состояние</param>
        /// <param name="message">Пришедшее сообщение. см. Common.Messaging</param>
        /// <returns></returns>
        public abstract bool OnMessage(T subject, Message message);
    }
}
