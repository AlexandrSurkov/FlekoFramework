namespace FlekoSoft.Common.Messaging
{
    public interface IMessageHandler
    {
        bool HandleMessage(Message message);
    }
}
