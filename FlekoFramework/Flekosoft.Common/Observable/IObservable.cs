using System;

namespace Flekosoft.Common.Observable
{
    public interface IObservable
    {
        void AddObserver(IObserver o);
        void RemoveObserver(IObserver o);
        void NotifyObservers(int type, EventArgs eventArgs);
    }
}
