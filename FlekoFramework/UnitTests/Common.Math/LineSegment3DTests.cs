using Flekosoft.Common.Math;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Flekosoft.UnitTests.Common.Math
{
    [TestClass]
    public class LineSegment3DTests
    {
        [TestMethod]
        public void Params_Test()
        {
            var a = new Vector3D(0, 0, 0);
            var b = new Vector3D(1, 1, 1);

            var ls = new LineSegment3D(a, b);

            Assert.AreEqual(1, ls.l);
            Assert.AreEqual(1, ls.m);
            Assert.AreEqual(1, ls.n);
            Assert.IsTrue(ls.Contains(a));
            Assert.IsTrue(ls.Contains(b));


            a = new Vector3D(-1, -1, -1);
            b = new Vector3D(1, 1, 1);

            ls = new LineSegment3D(a, b);

            Assert.AreEqual(2, ls.l);
            Assert.AreEqual(2, ls.m);
            Assert.AreEqual(2, ls.n);
            Assert.IsTrue(ls.Contains(a));
            Assert.IsTrue(ls.Contains(b));
            Assert.IsTrue(ls.Contains(new Vector3D(0, 0, 0)));

            a = new Vector3D(-1, 24, 56);
            b = new Vector3D(45, -67, 33);

            ls = new LineSegment3D(a, b);

            Assert.AreEqual(46, ls.l);
            Assert.AreEqual(-91, ls.m);
            Assert.AreEqual(-23, ls.n);
            Assert.IsTrue(ls.Contains(a));
            Assert.IsTrue(ls.Contains(b));
        }
    }
}
