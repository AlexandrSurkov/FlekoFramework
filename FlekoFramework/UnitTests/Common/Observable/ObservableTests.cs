using System;
using System.Collections.Generic;
using Flekosoft.Common.Observable;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Flekosoft.UnitTests.Common.Observable
{
    class Observable : IObservable
    {
        private readonly List<IObserver> _observersList = new List<IObserver>();

        public void AddObserver(IObserver o)
        {
            _observersList.Add(o);
        }

        public void RemoveObserver(IObserver o)
        {
            _observersList.Remove(o);
        }

        public void NotifyObservers(int type, EventArgs eventArgs)
        {
            foreach (var observer in _observersList)
            {
                observer.OnObservableNotification(new ObservableNotification(this, type, eventArgs));
            }
        }
    }

    [TestClass]
    public class ObservableTests : IObserver
    {
        private ObservableNotification _lastNotification;

        [TestMethod]
        public void NotificationTest()
        {
            var observable = new Observable();

            const int type = 1;

            _lastNotification = null;
            observable.NotifyObservers(type, EventArgs.Empty);
            Assert.IsNull(_lastNotification);

            observable.AddObserver(this);
            observable.NotifyObservers(type, EventArgs.Empty);

            Assert.IsNotNull(_lastNotification);
            // ReSharper disable once PossibleNullReferenceException
            Assert.AreEqual(type, _lastNotification.Type);
            Assert.AreSame(observable, _lastNotification.Sender);
            Assert.AreEqual(EventArgs.Empty, _lastNotification.EventArgs);

        }

        public void OnObservableNotification(ObservableNotification notification)
        {
            _lastNotification = new ObservableNotification(notification.Sender, notification.Type, notification.EventArgs);
        }
    }
}

