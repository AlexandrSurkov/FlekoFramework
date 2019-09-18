namespace Flekosoft.Common.Observable
{
    public interface IObserver
    {
        void OnObservableNotification(ObservervableNotification notification);
    }
}
