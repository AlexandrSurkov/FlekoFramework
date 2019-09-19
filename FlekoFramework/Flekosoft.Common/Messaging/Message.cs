namespace Flekosoft.Common.Messaging
{
    public abstract class Message
    {
        /// <summary>
        /// Message for all subscribers
        /// </summary>
        /// <param name="sender">Message sender</param>
        protected Message(object sender)
        {
            Sender = sender;
            Receiver = null;

        }

        /// <summary>
        /// Message for separate subscriber
        /// </summary>
        /// <param name="sender">Message sender</param>
        /// <param name="receiver">Message receiver</param>
        protected Message(object sender, object receiver)
        {
            Sender = sender;
            Receiver = receiver;
        }

        /// <summary>
        /// Message sender
        /// </summary>
        public object Sender { get; }

        /// <summary>
        /// Message receiver
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
