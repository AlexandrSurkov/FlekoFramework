using System;
using System.ComponentModel;
using System.IO;
using Flekosoft.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Flekosoft.UnitTests.Common
{
    class TestClass : PropertyChangedErrorNotifyDisposableBase
    {
        public TestClass()
        {

        }

        public TestClass(string instanceName) : base(instanceName)
        {

        }
        public void SendPropertyChanged(string propName)
        {
            OnPropertyChanged(propName);
        }

        public void SendErrorEvent(Exception ex)
        {
            OnErrorEvent(ex);
        }

        public bool DisposeCalled { get; set; }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                DisposeCalled = true;
                OnPropertyChanged("Dispose");
            }
            base.Dispose(disposing);
        }
    }

    [TestClass]
    public class PropertyChangedErrorNotifyDisposableBaseTests
    {
        [TestMethod]
        public void Test()
        {
            var tc = new TestClass();
            Assert.IsTrue(string.IsNullOrEmpty(tc.InstanceName));

            tc.ErrorEvent += Tc_ErrorEvent;
            ErrorEvent = null;
            var ex = new Exception("test");
            Assert.IsNull(ErrorEvent);
            tc.SendErrorEvent(ex);
            Assert.IsNotNull(ErrorEvent);
            // ReSharper disable once PossibleNullReferenceException
            Assert.AreSame(ex, ErrorEvent.GetException());

            tc.PropertyChanged += Tc_PropertyChanged;
            PropertyChangedEvent = null;
            var propName = nameof(ErrorEvent);
            Assert.IsNull(PropertyChangedEvent);
            tc.SendPropertyChanged(propName);
            Assert.IsNotNull(PropertyChangedEvent);
            // ReSharper disable once PossibleNullReferenceException
            Assert.AreEqual(propName, PropertyChangedEvent.PropertyName);

            tc.DisposeCalled = false;
            Assert.IsFalse(tc.IsDisposed);
            Assert.IsFalse(tc.DisposeCalled);
            tc.Dispose();
            Assert.IsTrue(tc.IsDisposed);
            Assert.IsTrue(tc.DisposeCalled);

            tc.DisposeCalled = false;
            Assert.IsFalse(tc.DisposeCalled);
            tc.Dispose();
            Assert.IsFalse(tc.DisposeCalled);

            var className = "ClassName";
            tc = new TestClass(className);
            Assert.AreEqual(className, tc.InstanceName);
        }

        [TestMethod]
        public void PripertyChangedDisposeTest()
        {
            var tc = new TestClass();
            tc.PropertyChanged += Tc_PropertyChanged;
            PropertyChangedEvent = null;
            tc.Dispose();
            tc.SendPropertyChanged("123");
            Assert.IsNull(PropertyChangedEvent);

        }


        [TestMethod]
        public void IsNotifyPropertyChangedEnabledTest()
        {
            var tc = new TestClass();
            tc.PropertyChanged += Tc_PropertyChanged;
            var propname = "af";
            PropertyChangedEvent = null;
            tc.SendPropertyChanged(propname);
            Assert.IsNotNull(PropertyChangedEvent);
            tc.IsNotifyPropertyChangedEnabled = false;
            PropertyChangedEvent = null;
            tc.SendPropertyChanged(propname);
            Assert.IsNull(PropertyChangedEvent);

        }

        public PropertyChangedEventArgs PropertyChangedEvent { get; set; }

        private void Tc_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            PropertyChangedEvent = e;
        }

        public ErrorEventArgs ErrorEvent { get; set; }

        private void Tc_ErrorEvent(object sender, ErrorEventArgs e)
        {
            ErrorEvent = e;
        }
    }
}
