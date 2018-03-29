using System;
using System.ComponentModel;
using Flekosoft.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Flekosoft.UnitTests.Common
{
    class TestReference : GuidReference
    {
        public bool CheckReferenceResult { get; set; }
        protected override bool CheckReference()
        {
            return CheckReferenceResult;
        }
    }

    [TestClass]
    public class GuidReferenceTests
    {
        [TestMethod]
        public void Test()
        {
            var tr = new TestReference();
            tr.PropertyChanged += Tr_PropertyChanged;
            tr.ValidityChanged += Tr_ValidityChanged;

            var guid = Guid.NewGuid();

            Assert.AreEqual(Guid.Empty, tr.Guid);
            tr.Guid = guid;
            Assert.AreEqual(guid, tr.Guid);

            tr.CheckReferenceResult = false;
            ValidityChangedEvent = null;
            PropertyChangedEvent = null;
            tr.Guid = Guid.NewGuid();
            Assert.AreEqual(nameof(tr.IsValid), PropertyChangedEvent?.PropertyName);
            Assert.AreEqual(tr.CheckReferenceResult, ValidityChangedEvent?.IsValid);
            Assert.AreEqual(tr.CheckReferenceResult, tr.IsValid);

            tr.CheckReferenceResult = true;
            ValidityChangedEvent = null;
            PropertyChangedEvent = null;
            tr.Guid = Guid.NewGuid();
            Assert.AreEqual(nameof(tr.IsValid), PropertyChangedEvent?.PropertyName);
            Assert.AreEqual(tr.CheckReferenceResult, ValidityChangedEvent?.IsValid);
            Assert.AreEqual(tr.CheckReferenceResult, tr.IsValid);

            tr.Dispose();

            tr.CheckReferenceResult = true;
            ValidityChangedEvent = null;
            PropertyChangedEvent = null;
            tr.Guid = Guid.NewGuid();
            Assert.IsNull(ValidityChangedEvent);
            Assert.IsNull(PropertyChangedEvent);
        }

        private void Tr_ValidityChanged(object sender, ValidityChangedEventArgs e)
        {
            ValidityChangedEvent = e;
        }

        private void Tr_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            PropertyChangedEvent = e;
        }

        public PropertyChangedEventArgs PropertyChangedEvent { get; set; }
        public ValidityChangedEventArgs ValidityChangedEvent { get; set; }
    }
}
