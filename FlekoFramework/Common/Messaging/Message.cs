namespace Flekosoft.Common.Messaging
{
    public abstract class Message
    {
        /// <summary>
        /// Сообщение, отправляемое сразу и всем подписчикам
        /// </summary>
        /// <param name="sender">Отправитель сообщения</param>
        protected Message(object sender)
        {
            Sender = sender;
            Receiver = null;

        }

        /// <summary>
        /// Сообщение, отправляемое сразу
        /// </summary>
        /// <param name="sender">Отправитель сообщения</param>
        /// <param name="receiver">Получатель сообщения</param>
        protected Message(object sender, object receiver)
        {
            Sender = sender;
            Receiver = receiver;
        }

        /// <summary>
        /// Отправитель сообщения
        /// </summary>
        public object Sender { get; }

        /// <summary>
        /// Получатель сообщения
        /// </summary>
        public object Receiver { get; }

        public override string ToString()
        {
            var str = "Sender: " + Sender + " to: ";
            if (Receiver == null)
                str += "Broadcast";
            else
                str += Receiver;
            return str;
        }
    }
}
