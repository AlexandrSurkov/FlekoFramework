namespace Flekosoft.Common.Messaging
{
    public interface IMessageHandler
    {
        bool HandleMessage(Message message);
    }
}
