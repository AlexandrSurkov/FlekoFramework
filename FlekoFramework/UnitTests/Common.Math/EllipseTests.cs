using Flekosoft.Common.Math;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Flekosoft.UnitTests.Common.Math
{
    [TestClass]
    public class EllipseTests
    {
        [TestMethod]
        public void EllipseTest()
        {
            var a1 = new Vector2D(10, 20);
            var rx = 30;
            var ry = 40;
            const int sta = 90;
            const int spa = 180;

            var el = new Ellipse(a1, rx, ry);

            Assert.AreEqual(a1, el.Location);
            Assert.AreEqual(rx, el.RadiusX);
            Assert.AreEqual(ry, el.RadiusY);
            Assert.AreEqual(0, el.StartAngle);
            Assert.AreEqual(360, el.SweepAngle);

            var el1 = new Ellipse(a1, rx, ry, sta, spa);

            Assert.AreEqual(a1, el1.Location);
            Assert.AreEqual(rx, el1.RadiusX);
            Assert.AreEqual(ry, el1.RadiusY);
            Assert.AreEqual(sta, el1.StartAngle);
            Assert.AreEqual(spa, el1.SweepAngle);

            var el2 = el1.Scale(10);

            Assert.AreEqual(a1 * 10, el2.Location);
            Assert.AreEqual(rx * 10, el2.RadiusX);
            Assert.AreEqual(ry * 10, el2.RadiusY);
            Assert.AreEqual(sta, el2.StartAngle);
            Assert.AreEqual(spa, el2.SweepAngle);

            var el3 = new Ellipse(el2);

            Assert.AreEqual(el2, el3);
            Assert.AreNotEqual(el1, el3);

            Assert.AreEqual(el2.GetHashCode(), el3.GetHashCode());
            Assert.AreNotEqual(el1.GetHashCode(), el3.GetHashCode());

            el3.Location = el1.Location;
            el3.RadiusX = el1.RadiusX;
            el3.RadiusY = el1.RadiusY;
            el3.StartAngle = el1.StartAngle;
            el3.SweepAngle = el1.SweepAngle;

            Assert.AreEqual(el3.Bounds, el1.Bounds);

            Assert.AreEqual(a1.X - rx, el1.Bounds.Left);
            Assert.AreEqual(a1.X + rx, el1.Bounds.Right);
            Assert.AreEqual(a1.Y - ry, el1.Bounds.Top);
            Assert.AreEqual(a1.Y + ry, el1.Bounds.Bottom);

            a1 = new Vector2D(10, 10);
            rx = 10;
            ry = 10;

            var el4 = new Ellipse(a1, rx, ry);

            for (int i = 0; i < 360; i++)
            {
                var angleRad = Utils.ToRadian(i);
                var p = el4.GetPointByAngle(angleRad);
                var r1 = el4.F1.Distance(p);
                var r2 = el4.F2.Distance(p);
                var res1 = r1 + r2;
                var res2 = 2 * el4.a;
                Assert.AreEqual((float)res1, (float)res2);
                var radius = el4.GetRadius(angleRad);
                var len = p.Distance(el4.Bounds.Centre);
                var res = System.Math.Abs((radius - len)) < 1;
                Assert.IsTrue(res);
            }

            a1 = new Vector2D(10, 10);
            rx = 15;
            ry = 10;
            el4 = new Ellipse(a1, rx, ry);
            for (int i = 0; i < 360; i++)
            {
                var angleRad = Utils.ToRadian(i);
                var p = el4.GetPointByAngle(angleRad);
                var r1 = el4.F1.Distance(p);
                var r2 = el4.F2.Distance(p);
                var res1 = r1 + r2;
                var res2 = 2 * el4.a;
                Assert.AreEqual((float)res1, (float)res2);
                var radius = el4.GetRadius(angleRad);
                var len = p.Distance(el4.Bounds.Centre);
                var res = System.Math.Abs((radius - len)) < 1;
                Assert.IsTrue(res);
            }

            a1 = new Vector2D(10, 10);
            rx = 10;
            ry = 15;
            el4 = new Ellipse(a1, rx, ry);
            for (int i = 0; i < 360; i++)
            {
                var angleRad = Utils.ToRadian(i);
                var p = el4.GetPointByAngle(angleRad);
                var r1 = el4.F1.Distance(p);
                var r2 = el4.F2.Distance(p);
                var res1 = r1 + r2;
                var res2 = 2 * el4.a;
                Assert.AreEqual((float)res1, (float)res2);
                var radius = el4.GetRadius(angleRad);
                var len = p.Distance(el4.Bounds.Centre);
                var res = System.Math.Abs((radius - len)) < 1;
                Assert.IsTrue(res);
            }
        }
    }
}
