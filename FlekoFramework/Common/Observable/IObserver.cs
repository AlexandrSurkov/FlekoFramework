namespace FlekoSoft.Common.Observable
{
    public interface IObserver
    {
        void OnObservableNotification(ObservervableNotification notification);
    }
}
