using Flekosoft.Common.Math;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Flekosoft.UnitTests.Common.Math
{
    [TestClass]
    public class GeometryTest
    {
        [TestMethod]
        public void FindEllipseAndLineSegmentIntersectionsTest()
        {
            var rx = 10;
            var ry = 10;
            var location = new Vector2D(10, 10);
            var ellipse = new Ellipse(location, rx, ry);

            var line1 = new LineSegment2D(new Vector2D(10, -10), new Vector2D(10, 30));

            var res = Geometry.FindEllipseAndLineSegmentIntersections(ellipse, line1, true);

            Assert.AreEqual(2, res.Length);
            Assert.AreEqual(ellipse.Bounds.BottomBorder.Bounds.Centre, res[0]);
            Assert.AreEqual(ellipse.Bounds.TopBorder.Bounds.Centre, res[1]);

            var line2 = new LineSegment2D(new Vector2D(10, 10), new Vector2D(10, 30));

            res = Geometry.FindEllipseAndLineSegmentIntersections(ellipse, line2, false);
            Assert.AreEqual(2, res.Length);
            Assert.AreEqual(ellipse.Bounds.BottomBorder.Bounds.Centre, res[0]);
            Assert.AreEqual(ellipse.Bounds.TopBorder.Bounds.Centre, res[1]);

            res = Geometry.FindEllipseAndLineSegmentIntersections(ellipse, line2, true);
            Assert.AreEqual(1, res.Length);
            Assert.AreEqual(ellipse.Bounds.BottomBorder.Bounds.Centre, res[0]);

            var a1 = new Vector2D(10, 10);
            rx = 10;
            ry = 10;
            var el4 = new Ellipse(a1, rx, ry);
            for (int i = 0; i < 360; i++)
            {
                var angleRad = Utils.ToRadian(i);
                var opositAngleRad = Utils.ToRadian(i + 180);
                var p1 = el4.GetPointByAngle(angleRad);
                var p2 = el4.GetPointByAngle(opositAngleRad);
                var line = new LineSegment2D(p1, p2);
                res = Geometry.FindEllipseAndLineSegmentIntersections(el4, line, false);
                Assert.AreEqual(2, res.Length);
                Assert.AreEqual((float)p1.X, (float)res[1].X);
                Assert.AreEqual((float)p1.Y, (float)res[1].Y);
                Assert.AreEqual((float)p2.X, (float)res[0].X);
                Assert.AreEqual((float)p2.Y, (float)res[0].Y);
            }

            a1 = new Vector2D(10, 10);
            rx = 10;
            ry = 15;
            el4 = new Ellipse(a1, rx, ry);
            for (int i = 0; i < 360; i++)
            {
                var angleRad = Utils.ToRadian(i);
                var opositAngleRad = Utils.ToRadian(i + 180);
                var p1 = el4.GetPointByAngle(angleRad);
                var p2 = el4.GetPointByAngle(opositAngleRad);
                var line = new LineSegment2D(p1, p2);
                res = Geometry.FindEllipseAndLineSegmentIntersections(el4, line, false);
                Assert.AreEqual(2, res.Length);
                Assert.AreEqual((float)p1.X, (float)res[1].X);
                Assert.AreEqual((float)p1.Y, (float)res[1].Y);
                Assert.AreEqual((float)p2.X, (float)res[0].X);
                Assert.AreEqual((float)p2.Y, (float)res[0].Y);
            }

            a1 = new Vector2D(10, 10);
            rx = 15;
            ry = 10;
            el4 = new Ellipse(a1, rx, ry);
            for (int i = 0; i < 360; i++)
            {
                var angleRad = Utils.ToRadian(i);
                var opositAngleRad = Utils.ToRadian(i + 180);
                var p1 = el4.GetPointByAngle(angleRad);
                var p2 = el4.GetPointByAngle(opositAngleRad);
                var line = new LineSegment2D(p1, p2);
                res = Geometry.FindEllipseAndLineSegmentIntersections(el4, line, false);
                Assert.AreEqual(2, res.Length);
                Assert.AreEqual((float)p1.X, (float)res[1].X);
                Assert.AreEqual((float)p1.Y, (float)res[1].Y);
                Assert.AreEqual((float)p2.X, (float)res[0].X);
                Assert.AreEqual((float)p2.Y, (float)res[0].Y);
            }
        }

        [TestMethod]
        public void GetTangentPointsTest()
        {
            var a1 = new Vector2D(10, 10);
            const int r1 = 10;
            var el4 = new Ellipse(a1, r1, r1);

            for (int i = r1 + 1; i < 300; i++)
            {
                var dotEl = new Ellipse(a1, i, i);

                for (int j = 0; j < 360; j++)
                {
                    Vector2D t1;
                    Vector2D t2;
                    Geometry.GetTangentPoints(a1, r1, dotEl.GetPointByAngle(Utils.ToRadian(j)), out t1, out t2);
                    var line1 = new LineSegment2D(t1, a1);
                    var line2 = new LineSegment2D(t2, a1);

                    var dots1 = Geometry.FindEllipseAndLineSegmentIntersections(el4, line1, false);
                    var dots2 = Geometry.FindEllipseAndLineSegmentIntersections(el4, line2, false);

                    var res = System.Math.Abs((t1.X - dots1[1].X)) < 0.0001;
                    Assert.IsTrue(res);
                    res = System.Math.Abs((t1.Y - dots1[1].Y)) < 0.0001;
                    Assert.IsTrue(res);
                    res = System.Math.Abs((t2.X - dots2[1].X)) < 0.0001;
                    Assert.IsTrue(res);
                    res = System.Math.Abs((t2.Y - dots2[1].Y)) < 0.0001;
                    Assert.IsTrue(res);
                }
            }
        }
    }
}

