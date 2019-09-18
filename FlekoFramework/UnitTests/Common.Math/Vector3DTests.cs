using Flekosoft.Common.Math;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Flekosoft.UnitTests.Common.Math
{
    [TestClass]
    public class Vector3DTests
    {
        [TestMethod]
        public void Zero_Tests()
        {
            var v1 = new Vector3D(0, 0, 0);
            Assert.IsTrue(v1.IsZero());
            v1 = new Vector3D(1, -1, 0);
            Assert.IsFalse(v1.IsZero());
            v1.Zero();
            Assert.IsTrue(v1.IsZero());
        }


        [TestMethod]
        public void Operators_Tests()
        {
            //+
            var v1 = new Vector3D(1, -1, 1);
            var v2 = new Vector3D(-2, 2, 2);
            var v3 = v1 + v2;
            Assert.AreEqual(-1, v3.X);
            Assert.AreEqual(1, v3.Y);
            Assert.AreEqual(3, v3.Z);
            //*
            v3 = v1 * 10;
            Assert.AreEqual(10, v3.X);
            Assert.AreEqual(-10, v3.Y);
            Assert.AreEqual(10, v3.Z);
            v3 = 15 * v2;
            Assert.AreEqual(-30, v3.X);
            Assert.AreEqual(30, v3.Y);
            Assert.AreEqual(30, v3.Z);
            //-
            v3 = v1 - v2;
            Assert.AreEqual(3, v3.X);
            Assert.AreEqual(-3, v3.Y);
            Assert.AreEqual(-1, v3.Z);
            // /
            v3 = v1 / 10;
            Assert.AreEqual(0.1, v3.X);
            Assert.AreEqual(-0.1, v3.Y);
            Assert.AreEqual(0.1, v3.Z);
            //==
            var v4 = new Vector3D(v1);
            Assert.IsFalse(v1 == v2);
            Assert.IsTrue(v1 == v4);

            Assert.IsTrue(v1 != v2);
            Assert.IsFalse(v1 != v4);

        }

        [TestMethod]
        public void Length_Tests()
        {
            var v1 = new Vector3D(2, 2, 2);
            var l = v1.Length();
            Assert.AreEqual(System.Math.Sqrt(12), l);
            l = v1.LengthSq();
            Assert.AreEqual(12, l);
        }

        [TestMethod]
        public void DotProduct_Tests()
        {
            var v1 = new Vector3D(2, 4, 5);
            var v2 = new Vector3D(3, 5, 7);
            var dp = v1.DotProduct(v2);
            var a = System.Math.Abs(dp - 61);
            var res = a < 0.000000001;
            Assert.IsTrue(res);
        }


        [TestMethod]
        public void Truncate_Tests()
        {
            var v1 = new Vector3D(4, 6, 8);
            var l = v1.Length();
            Assert.IsTrue(l > 3);
            v1.Truncate(3);
            l = v1.Length();
            Assert.IsTrue(System.Math.Round(l) <= 3);
        }

        [TestMethod]
        public void Distance_Tests()
        {
            var v1 = new Vector3D(1, 0, 0);
            var v2 = new Vector3D(1, 2, 0);
            var d = System.Math.Round(v1.Distance(v2));
            Assert.AreEqual(2, d);
            d = System.Math.Round(v1.DistanceSq(v2));
            Assert.AreEqual(4, d);
        }

        [TestMethod]
        public void Reflect_Tests()
        {
            var v1 = new Vector3D(1, 0, 0);
            var n = new Vector3D(-1, 0, 0);
            v1.Reflect(n);
            Assert.AreEqual(-1, v1.X);
            Assert.AreEqual(0, v1.Y);

            v1 = new Vector3D(1, 1, 0);
            v1.Reflect(n);
            Assert.AreEqual(-1, v1.X);
            Assert.AreEqual(1, v1.Y);

            v1 = new Vector3D(-1, 1, 0);
            v1.Reflect(n);
            Assert.AreEqual(1, v1.X);
            Assert.AreEqual(1, v1.Y);

            v1 = new Vector3D(-1, 1, 0);
            var oldL = v1.Length();
            v1.Reflect(n, 0.7);
            var l = System.Math.Round(v1.Length(), 2);
            Assert.AreEqual(System.Math.Round(oldL * 0.7, 2), l);
        }

        [TestMethod]
        public void GetReverse_Tests()
        {
            var v1 = new Vector3D(1, 0, 3);
            var v2 = new Vector3D(1, 2, 3);
            var res = Vector3D.Invert(v1);
            Assert.AreEqual(-1, res.X);
            Assert.AreEqual(0, res.Y);
            Assert.AreEqual(-3, res.Z);
            res = Vector3D.Invert(v2);
            Assert.AreEqual(-1, res.X);
            Assert.AreEqual(-2, res.Y);
            Assert.AreEqual(-3, res.Z);
        }

        [TestMethod]
        public void Static_Normalize_Tests()
        {
            var v1 = new Vector3D(2, 2, 2);
            var l = v1.Length();
            Assert.AreNotEqual(1, l);
            var v2 = Vector3D.Normalize(v1);
            l = v2.Length();
            var a = System.Math.Abs(l - 1);
            var res = a < 0.000000001;
            Assert.IsTrue(res);
        }

        [TestMethod]
        public void Static_Distance_Tests()
        {
            var v1 = new Vector3D(1, 0, 0);
            var v2 = new Vector3D(1, 2, 0);
            var d = System.Math.Round(Vector3D.Distance(v1, v2));
            Assert.AreEqual(2, d);
            d = System.Math.Round(Vector3D.DistanceSq(v1, v2));
            Assert.AreEqual(4, d);
        }

        [TestMethod]
        public void Static_Length_Tests()
        {
            var v1 = new Vector3D(2, 2, 2);
            var l = Vector3D.Length(v1);
            Assert.AreEqual(System.Math.Sqrt(12), l);
            l = Vector3D.LengthSq(v1);
            Assert.AreEqual(12, l);
        }

        [TestMethod]
        public void RotateAroundOriginXYTests()
        {
            var v = new Vector3D(1, 0, 0);

            var ang0 = Utils.ToRadian(0);
            var ang360 = Utils.ToRadian(360);

            for (int i = 0; i < 360; i++)
            {
                var angRad = Utils.ToRadian(i);
                v.RotateAroundOriginXY(angRad);
                Assert.AreEqual(angRad, v.XY.Angle, 0.000000001, "i = " + i);
                v.RotateAroundOriginXY(-angRad);

                var res1 = System.Math.Abs(ang0 - v.XY.Angle) < 0.000000001;
                var res2 = System.Math.Abs(ang360 - v.XY.Angle) < 0.000000001;

                Assert.IsTrue(res2 || res1, "i = " + i);
            }
        }

        [TestMethod]
        public void RotateAroundOriginXZTests()
        {
            var v = new Vector3D(1, 0, 0);

            var ang0 = Utils.ToRadian(0);
            var ang360 = Utils.ToRadian(360);

            for (int i = 0; i < 360; i++)
            {
                var angRad = Utils.ToRadian(i);
                v.RotateAroundOriginXZ(angRad);
                Assert.AreEqual(angRad, v.XZ.Angle, 0.000000001, "i = " + i);
                v.RotateAroundOriginXZ(-angRad);

                var res1 = System.Math.Abs(ang0 - v.XZ.Angle) < 0.000000001;
                var res2 = System.Math.Abs(ang360 - v.XZ.Angle) < 0.000000001;

                Assert.IsTrue(res2 || res1, "i = " + i);
            }
        }

        [TestMethod]
        public void RotateAroundOriginZYTests()
        {
            var v = new Vector3D(0, 0, 1);

            var ang0 = Utils.ToRadian(0);
            var ang360 = Utils.ToRadian(360);

            for (int i = 0; i < 360; i++)
            {
                var angRad = Utils.ToRadian(i);
                v.RotateAroundOriginZY(angRad);
                Assert.AreEqual(angRad, v.ZY.Angle, 0.000000001, "i = " + i);
                v.RotateAroundOriginZY(-angRad);

                var res1 = System.Math.Abs(ang0 - v.ZY.Angle) < 0.000000001;
                var res2 = System.Math.Abs(ang360 - v.ZY.Angle) < 0.000000001;

                Assert.IsTrue(res2 || res1, "i = " + i);
            }
        }



        [TestMethod]
        public void RotateAroundOriginQuaternionXYTests()
        {
            var v = new Vector3D(1, 0, 0);

            var ang0 = Utils.ToRadian(0);
            var ang360 = Utils.ToRadian(360);

            for (int i = 0; i < 360; i++)
            {
                var angRad = Utils.ToRadian(i);
                var q = new Quaternion();
                q.SetFromAxisAngleRadian(angRad, Vector3D.ZAxis);
                v.Rotate(q);
                Assert.AreEqual(angRad, v.XY.Angle, 0.000000001, "i = " + i);
                v.RotateAroundOriginXY(-angRad);

                var res1 = System.Math.Abs(ang0 - v.XY.Angle) < 0.000000001;
                var res2 = System.Math.Abs(ang360 - v.XY.Angle) < 0.000000001;

                Assert.IsTrue(res2 || res1, "i = " + i);
            }
        }

        [TestMethod]
        public void RotateAroundOriginQuaternionXZTests()
        {
            var v = new Vector3D(1, 0, 0);

            var ang0 = Utils.ToRadian(0);
            var ang360 = Utils.ToRadian(360);

            for (int i = 0; i < 360; i++)
            {
                var angRad = Utils.ToRadian(i);
                var q = new Quaternion();
                var acx = Vector3D.YAxis;
                acx.Invert();
                q.SetFromAxisAngleRadian(angRad, acx);
                v.Rotate(q);
                Assert.AreEqual(angRad, v.XZ.Angle, 0.000000001, "i = " + i);
                v.RotateAroundOriginXZ(-angRad);

                var res1 = System.Math.Abs(ang0 - v.XZ.Angle) < 0.000000001;
                var res2 = System.Math.Abs(ang360 - v.XZ.Angle) < 0.000000001;

                Assert.IsTrue(res2 || res1, "i = " + i);
            }
        }

        [TestMethod]
        public void RotateAroundOriginQuaternionZYTests()
        {
            var v = new Vector3D(0, 0, 1);

            var ang0 = Utils.ToRadian(0);
            var ang360 = Utils.ToRadian(360);

            for (int i = 0; i < 360; i++)
            {
                var angRad = Utils.ToRadian(i);
                var q = new Quaternion();
                var acx = Vector3D.XAxis;
                acx.Invert();
                q.SetFromAxisAngleRadian(angRad, acx);
                v.Rotate(q);
                Assert.AreEqual(angRad, v.ZY.Angle, 0.000000001, "i = " + i);
                v.RotateAroundOriginZY(-angRad);

                var res1 = System.Math.Abs(ang0 - v.ZY.Angle) < 0.000000001;
                var res2 = System.Math.Abs(ang360 - v.ZY.Angle) < 0.000000001;

                Assert.IsTrue(res2 || res1, "i = " + i);
            }
        }
    }


}
