using Flekosoft.Common.Math;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Flekosoft.UnitTests.Common.Math
{
    [TestClass]
    public class LineSegment2DTest
    {
        [TestMethod]
        public void Test()
        {
            var a1 = new Vector2D(10, 20);
            var a2 = new Vector2D(30, 40);
            var seg = new LineSegment2D(a1, a2);

            Assert.AreEqual(a1, seg.Start);
            Assert.AreEqual(a2, seg.Stop);

            Assert.AreEqual(10, seg.Bounds.Left);
            Assert.AreEqual(30, seg.Bounds.Right);
            Assert.AreEqual(20, seg.Bounds.Top);
            Assert.AreEqual(40, seg.Bounds.Bottom);

            var seg2 = new LineSegment2D(seg);

            Assert.AreEqual(seg, seg2);

            var seg3 = new LineSegment2D(a1, a1);

            Assert.AreEqual(seg.GetHashCode(), seg2.GetHashCode());
            Assert.AreNotEqual(seg3.GetHashCode(), seg2.GetHashCode());
            Assert.AreNotEqual(seg3.GetHashCode(), seg.GetHashCode());

            var eq = seg.Equals(seg2);
            Assert.IsTrue(eq);

            eq = seg.Equals(seg3);
            Assert.IsFalse(eq);

            var seg4 = new LineSegment2D(a2, a2);
            eq = seg.Equals(seg4);
            Assert.IsFalse(eq);

            var str = seg.ToString();

            var exp = "LineSegment: Start = " + a1 + " Stop = " + a2;

            Assert.AreEqual(exp, str);

        }


        [TestMethod]
        public void DistToPoint_Test()
        {
            var a11 = new Vector2D(-10, 0);
            var a12 = new Vector2D(10, 0);
            var seg1 = new LineSegment2D(a11, a12);

            var a21 = new Vector2D(0, -10);
            var a22 = new Vector2D(0, 10);
            var seg2 = new LineSegment2D(a21, a22);

            for (int x = -10; x < 10; x++)
            {
                for (int y = -20; y < 20; y++)
                {
                    var p = new Vector2D(x, y);

                    var dist1 = seg1.DistanceToPoint(p);
                    Assert.AreEqual(System.Math.Abs(y), dist1, "dist1 = " + dist1 + " y = " + System.Math.Abs(y));

                    var dist2 = seg2.DistanceToPoint(p);
                    Assert.AreEqual(System.Math.Abs(x), dist2, "dist2 = " + dist2 + " x = " + System.Math.Abs(x));

                    Vector2D projPoint;

                    if (seg1.ProjectPointToLine(p, out projPoint))
                    {
                        var dist = projPoint.Distance(p);
                        Assert.AreEqual(dist1, dist, "dist1 = " + dist1 + " dist = " + dist);
                    }

                    if (seg2.ProjectPointToLine(p, out projPoint))
                    {
                        var dist = projPoint.Distance(p);
                        Assert.AreEqual(dist2, dist, "dist2 = " + dist1 + " dist = " + dist);
                    }
                }
            }
        }


        [TestMethod]
        public void ProjectPointToLine_Test()
        {
            var a11 = new Vector2D(-10, 0);
            var a12 = new Vector2D(10, 0);
            var seg1 = new LineSegment2D(a11, a12);

            for (int x = -20; x < 20; x++)
            {
                var p = new Vector2D(x, 10);

                var dist1 = seg1.DistanceToPoint(p);
                Assert.AreEqual(10, dist1);

                Vector2D projPoint;

                var res = seg1.ProjectPointToLine(p, out projPoint);


                if (x >= a11.X && x <= a12.X)
                {
                    Assert.IsTrue(res);
                    var dist = projPoint.Distance(p);
                    Assert.AreEqual(dist1, dist, "dist1 = " + dist1 + " dist = " + dist);
                    var contains = seg1.Bounds.Contains(projPoint);
                    Assert.IsTrue(contains);
                }
                else
                {
                    Assert.IsFalse(res);
                    var contains = seg1.Bounds.Contains(projPoint);
                    Assert.IsFalse(contains);
                }
            }

            var a21 = new Vector2D(0, -10);
            var a22 = new Vector2D(0, 10);
            var seg2 = new LineSegment2D(a21, a22);

            for (int y = -20; y < 20; y++)
            {
                var p = new Vector2D(10, y);

                var dist2 = seg2.DistanceToPoint(p);
                Assert.AreEqual(10, dist2);

                Vector2D projPoint;

                var res = seg2.ProjectPointToLine(p, out projPoint);


                if (y >= a21.Y && y <= a22.Y)
                {
                    Assert.IsTrue(res);
                    var dist = projPoint.Distance(p);
                    Assert.AreEqual(dist2, dist, "dist2 = " + dist2 + " dist = " + dist);
                    var contains = seg2.Bounds.Contains(projPoint);
                    Assert.IsTrue(contains);
                }
                else
                {
                    Assert.IsFalse(res);
                    var contains = seg2.Bounds.Contains(projPoint);
                    Assert.IsFalse(contains);
                }
            }






            //for (int y = -20; y < 20; y++)
            //{

            //}
        }

        [TestMethod]
        public void ContainsPoint_Test()
        {
            for (int x0 = -10; x0 < 10; x0++)
            {
                for (int y0 = -10; y0 < 10; y0++)
                {
                    var el = new Ellipse(new Vector2D(x0, y0), 10, 10);

                    for (int a = 0; a < 360; a++)
                    {
                        var p1 = el.GetPointByAngle(Utils.ToRadian(a));
                        var p2 = el.GetPointByAngle(Utils.ToRadian(a) + System.Math.PI);

                        var line = new LineSegment2D(p1, p2);

                        var dx = System.Math.Abs(p1.X - p2.X);
                        var dy = System.Math.Abs(p1.Y - p2.Y);

                        if (dx >= dy)
                        {
                            //проекция на ось Х больше проекции на ось Y
                            //Проверяем точки, перемещаясь по X

                            //A x + B y + C = 0
                            //y = k x + b

                            var k = -(line.A / line.B);
                            var b = -(line.C / line.B);

                            var delta = dx / 100;
                            var start = System.Math.Min(p1.X, p2.X);
                            var stop = System.Math.Max(p1.X, p2.X);

                            for (double i = (start - delta * 10); i <= (stop + delta * 10); i += delta)
                            {
                                var x = System.Math.Round(i, 5);
                                var y = k * x + b;
                                var p = new Vector2D(x, y);
                                var res = line.Contains(p);
                                if ((x >= start) && (x <= stop))
                                {
                                    Assert.IsTrue(res);
                                }
                                else Assert.IsFalse(res);
                            }
                        }
                        else
                        {
                            //проекция на ось Y больше проекции на ось X
                            //Проверяем точки, перемещаясь по Y

                            //A x + B y + C = 0
                            //x = k y + b

                            var k = -(line.B / line.A);
                            var b = -(line.C / line.A);

                            var delta = dy / 100;
                            var start = System.Math.Min(p1.Y, p2.Y);
                            var stop = System.Math.Max(p1.Y, p2.Y);

                            for (double i = (start - delta * 10); i <= (stop + delta * 10); i += delta)
                            {
                                var y = System.Math.Round(i, 5);
                                var x = k * y + b;
                                var p = new Vector2D(x, y);
                                var res = line.Contains(p);
                                if ((y >= start) && (y <= stop))
                                {
                                    Assert.IsTrue(res);
                                }
                                else Assert.IsFalse(res);
                            }
                        }
                    }
                }

            }
        }

        [TestMethod]
        public void Intersec_Test()
        {
            for (int y0 = -20; y0 < 20; y0++)
            {
                var line1 = new LineSegment2D(new Vector2D(-10, y0), new Vector2D(10, y0));

                for (int x0 = -20; x0 < 20; x0++)
                {
                    var line2 = new LineSegment2D(new Vector2D(x0, -10), new Vector2D(x0, 10));

                    var res1 = line1.Intersec(line2);
                    var res2 = line2.Intersec(line1);

                    Assert.AreEqual(res2.Result, res1.Result);
                    Assert.AreEqual(res2.IntersecPoint, res1.IntersecPoint);

                    if (x0 >= -10 && x0 <= 10 && y0 >= -10 && y0 <= 10)
                    {
                        Assert.AreEqual(LineSegmentIntersecResult.IntersecResult.Intersecs, res2.Result);
                        Assert.AreEqual(y0, res2.IntersecPoint.Y);
                        Assert.AreEqual(x0, res2.IntersecPoint.X);
                    }
                    else
                    {
                        Assert.AreEqual(LineSegmentIntersecResult.IntersecResult.DontIntersecs, res2.Result);
                        Assert.IsNull(res2.IntersecPoint);
                    }
                }
            }

        }
    }
}
