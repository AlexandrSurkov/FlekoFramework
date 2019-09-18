namespace Flekosoft.Common.Messaging
{
    public abstract class MessageHandler: IMessageHandler
    {
        public virtual bool HandleMessage(Message message)
        {
            return true;
        }
    }
}
