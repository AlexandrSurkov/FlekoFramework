namespace Flekosoft.Common.Messaging
{
    public class MessageHandler: IMessageHandler
    {
        public virtual bool HandleMessage(Message message)
        {
            return true;
        }
    }
}
