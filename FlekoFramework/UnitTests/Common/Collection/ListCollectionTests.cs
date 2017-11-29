using System;
using Flekosoft.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Flekosoft.UnitTests.Common.Collection
{
    class ListItemTestClass : PropertyChangedErrorNotifyDisposableBase
    {
        private int _prop;

        public int Prop
        {
            get => _prop;
            set
            {
                if (_prop != value)
                {
                    _prop = value;
                    OnPropertyChanged(nameof(Prop));
                }
            }
        }
    }

    [TestClass]
    public class ListCollectionTests
    {
        [TestMethod]
        public void TestMethod1()
        {
        }
    }
}
