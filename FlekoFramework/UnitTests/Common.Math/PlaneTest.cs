using Flekosoft.Common.Math;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Flekosoft.UnitTests.Common.Math
{
    [TestClass]
    public class PlaneTest
    {
        [TestMethod]
        public void ParamsTest()
        {
            var v1 = new Vector3D(1, 0, 0);
            var v2 = new Vector3D(0, 1, 0);
            var v3 = new Vector3D(1, 1, 0);

            var p = new Plane(v1, v2, v3);

            Assert.AreEqual(0, p.A);
            Assert.AreEqual(0, p.B);
            Assert.AreEqual(-1, p.C);
            Assert.AreEqual(0, p.D);

            v1 = new Vector3D(0, 0, 1);
            v2 = new Vector3D(0, 1, 0);
            v3 = new Vector3D(0, 1, 1);

            p = new Plane(v1, v2, v3);

            Assert.AreEqual(1, p.A);
            Assert.AreEqual(0, p.B);
            Assert.AreEqual(0, p.C);
            Assert.AreEqual(0, p.D);




            v1 = new Vector3D(1, 0, 0);
            v2 = new Vector3D(0, 1, 0);
            v3 = new Vector3D(0, 0, 1);

            p = new Plane(v1, v2, v3);

            Assert.AreEqual(1, p.A);
            Assert.AreEqual(1, p.B);
            Assert.AreEqual(1, p.C);
            Assert.AreEqual(-1, p.D);



            v1 = new Vector3D(10, 0, 0);
            v2 = new Vector3D(0, 25, 0);
            v3 = new Vector3D(0, 0, 35);

            p = new Plane(v1, v2, v3);

            Assert.AreEqual(875, p.A);
            Assert.AreEqual(350, p.B);
            Assert.AreEqual(250, p.C);
            Assert.AreEqual(-8750, p.D);



            v1 = new Vector3D(-2, 5, 85);
            v2 = new Vector3D(45, 65, 5);
            v3 = new Vector3D(-9, 55, -15);

            p = new Plane(v1, v2, v3);

            Assert.AreEqual(-2000, p.A);
            Assert.AreEqual(5260, p.B);
            Assert.AreEqual(2770, p.C);
            Assert.AreEqual(-265750, p.D);
        }

        [TestMethod]
        public void ProjectionTest()
        {
            var v1 = new Vector3D(1, 0, 0);
            var v2 = new Vector3D(0, 1, 0);
            var v3 = new Vector3D(1, 1, 0);

            var p = new Plane(v1, v2, v3);

            ProjectionTest(p);

            v1 = new Vector3D(0, 0, 1);
            v2 = new Vector3D(0, 1, 0);
            v3 = new Vector3D(0, 1, 1);

            p = new Plane(v1, v2, v3);

            ProjectionTest(p);

            v1 = new Vector3D(1, 0, 0);
            v2 = new Vector3D(0, 1, 0);
            v3 = new Vector3D(0, 0, 1);

            p = new Plane(v1, v2, v3);

            ProjectionTest(p);

            v1 = new Vector3D(10, 0, 0);
            v2 = new Vector3D(0, 25, 0);
            v3 = new Vector3D(0, 0, 35);

            p = new Plane(v1, v2, v3);

            ProjectionTest(p);

            v1 = new Vector3D(-2, 5, 85);
            v2 = new Vector3D(45, 65, 5);
            v3 = new Vector3D(-9, 55, -15);

            p = new Plane(v1, v2, v3);

            ProjectionTest(p);
        }

        void ProjectionTest(Plane plane)
        {
            for (int x = -10; x < 10; x++)
            {
                for (int y = -10; y < 10; y++)
                {
                    for (int z = -10; z < 10; z++)
                    {
                        var point = new Vector3D(x, y, z);

                        var proj = plane.PointProjection(point);
                        Assert.IsTrue(plane.Contains(proj));
                        var dist = proj.Distance(point);
                        var calcDist = plane.DistanceToPoint(point);
                        Assert.AreEqual(dist, calcDist, 0.0001);


                    }
                }
            }
        }

        [TestMethod]
        public void LineCrossTest()
        {
            var v1 = new Vector3D(1, 0, 0);
            var v2 = new Vector3D(0, 1, 0);
            var v3 = new Vector3D(1, 1, 0);

            var p = new Plane(v1, v2, v3);

            LineCrossTest(p);

            v1 = new Vector3D(0, 0, 1);
            v2 = new Vector3D(0, 1, 0);
            v3 = new Vector3D(0, 1, 1);

            p = new Plane(v1, v2, v3);

            LineCrossTest(p);

            v1 = new Vector3D(1, 0, 0);
            v2 = new Vector3D(0, 1, 0);
            v3 = new Vector3D(0, 0, 1);

            p = new Plane(v1, v2, v3);

            LineCrossTest(p);

            v1 = new Vector3D(10, 0, 0);
            v2 = new Vector3D(0, 25, 0);
            v3 = new Vector3D(0, 0, 35);

            p = new Plane(v1, v2, v3);

            LineCrossTest(p);

            v1 = new Vector3D(-2, 5, 85);
            v2 = new Vector3D(45, 65, 5);
            v3 = new Vector3D(-9, 55, -15);

            p = new Plane(v1, v2, v3);

            LineCrossTest(p);
        }

        void LineCrossTest(Plane plane)
        {
            var min = -5;
            var max = 5;
            for (int x1 = min; x1 < max; x1++)
            {
                for (int y1 = min; y1 < max; y1++)
                {
                    for (int z1 = min; z1 < max; z1++)
                    {

                        for (int x2 = min; x2 < max; x2++)
                        {
                            for (int y2 = min; y2 < max; y2++)
                            {
                                for (int z2 = min; z2 < max; z2++)
                                {
                                    if (x1 != x2 && y1 != y2 && z1 != z2)
                                    {
                                        var p1 = new Vector3D(x1, y1, z1);

                                        var p2 = new Vector3D(x2, y2, z2);

                                        var ls = new LineSegment3D(p1, p2);

                                        var crossPoint = plane.PointOfLineSegment3DCross(ls);
                                        if (crossPoint != null)
                                        {
                                            Assert.IsTrue(plane.Contains(crossPoint));
                                            Assert.IsTrue(ls.Contains(crossPoint));
                                        }
                                    }
                                }
                            }
                        }

                    }
                }
            }
        }
    }
}
