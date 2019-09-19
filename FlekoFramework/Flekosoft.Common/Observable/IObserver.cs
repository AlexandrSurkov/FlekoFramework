namespace Flekosoft.Common.Observable
{
    public interface IObserver
    {
        void OnObservableNotification(ObservableNotification notification);
    }
}
