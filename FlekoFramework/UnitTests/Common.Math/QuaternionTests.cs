using System.Numerics;
using Flekosoft.Common.Math;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Quaternion = Flekosoft.Common.Math.Quaternion;

namespace Flekosoft.UnitTests.Common.Math
{
    [TestClass]
    public class QuaternionTests
    {
        [TestMethod]
        public void Constructor_Test()
        {
            var q = new Quaternion();
            Assert.AreEqual(0, q.X);
            Assert.AreEqual(0, q.Y);
            Assert.AreEqual(0, q.Z);
            Assert.AreEqual(0, q.W);

            q = new Quaternion(1, 2, 3, 4);

            Assert.AreEqual(1, q.W);
            Assert.AreEqual(2, q.X);
            Assert.AreEqual(3, q.Y);
            Assert.AreEqual(4, q.Z);

            var t = new Quaternion(q);

            Assert.AreEqual(1, t.W);
            Assert.AreEqual(2, t.X);
            Assert.AreEqual(3, t.Y);
            Assert.AreEqual(4, t.Z);
        }

        [TestMethod]
        public void SetFromAxisAngleDegrees_Test()
        {
            var q = new Quaternion();
            for (int i = 0; i <= 360; i++)
            {
                q.SetFromAxisAngleDegrees(i, Vector3D.XAxis);
                var tQ = System.Numerics.Quaternion.CreateFromAxisAngle(Vector3.UnitX, (float)Utils.ToRadian(i));
                Assert.AreEqual(tQ.X, q.X, 0.000001);
                Assert.AreEqual(tQ.Y, q.Y, 0.000001);
                Assert.AreEqual(tQ.Z, q.Z, 0.000001);
                Assert.AreEqual(tQ.W, q.W, 0.000001);

                q.SetFromAxisAngleDegrees(i, Vector3D.YAxis);
                tQ = System.Numerics.Quaternion.CreateFromAxisAngle(Vector3.UnitY, (float)Utils.ToRadian(i));
                Assert.AreEqual(tQ.X, q.X, 0.000001);
                Assert.AreEqual(tQ.Y, q.Y, 0.000001);
                Assert.AreEqual(tQ.Z, q.Z, 0.000001);
                Assert.AreEqual(tQ.W, q.W, 0.000001);

                q.SetFromAxisAngleDegrees(i, Vector3D.ZAxis);
                tQ = System.Numerics.Quaternion.CreateFromAxisAngle(Vector3.UnitZ, (float)Utils.ToRadian(i));
                Assert.AreEqual(tQ.X, q.X, 0.000001);
                Assert.AreEqual(tQ.Y, q.Y, 0.000001);
                Assert.AreEqual(tQ.Z, q.Z, 0.000001);
                Assert.AreEqual(tQ.W, q.W, 0.000001);
            }
        }

        [TestMethod]
        public void Length_Test()
        {
            var q = new Quaternion();
            for (int i = 0; i <= 360; i++)
            {
                q.SetFromAxisAngleDegrees(i, Vector3D.XAxis);
                var tQ = System.Numerics.Quaternion.CreateFromAxisAngle(Vector3.UnitX, (float)Utils.ToRadian(i));
                Assert.AreEqual(tQ.Length(), q.Length(), 0.000001);

                q.SetFromAxisAngleDegrees(i, Vector3D.YAxis);
                tQ = System.Numerics.Quaternion.CreateFromAxisAngle(Vector3.UnitY, (float)Utils.ToRadian(i));
                Assert.AreEqual(tQ.Length(), q.Length(), 0.000001);

                q.SetFromAxisAngleDegrees(i, Vector3D.ZAxis);
                tQ = System.Numerics.Quaternion.CreateFromAxisAngle(Vector3.UnitZ, (float)Utils.ToRadian(i));
                Assert.AreEqual(tQ.Length(), q.Length(), 0.000001);
            }
        }

        [TestMethod]
        public void LengthSq_Test()
        {
            var q = new Quaternion();
            for (int i = 0; i <= 360; i++)
            {
                q.SetFromAxisAngleDegrees(i, Vector3D.XAxis);
                var tQ = System.Numerics.Quaternion.CreateFromAxisAngle(Vector3.UnitX, (float)Utils.ToRadian(i));
                Assert.AreEqual(tQ.LengthSquared(), q.LengthSq(), 0.000001);

                q.SetFromAxisAngleDegrees(i, Vector3D.YAxis);
                tQ = System.Numerics.Quaternion.CreateFromAxisAngle(Vector3.UnitY, (float)Utils.ToRadian(i));
                Assert.AreEqual(tQ.LengthSquared(), q.LengthSq(), 0.000001);

                q.SetFromAxisAngleDegrees(i, Vector3D.ZAxis);
                tQ = System.Numerics.Quaternion.CreateFromAxisAngle(Vector3.UnitZ, (float)Utils.ToRadian(i));
                Assert.AreEqual(tQ.LengthSquared(), q.LengthSq(), 0.000001);
            }
        }

        [TestMethod]
        public void Multiply_double_Test()
        {
            var tqq = new System.Numerics.Quaternion(2, 3, 4, 1);
            for (int i = 0; i <= 360; i++)
            {
                var q = new Quaternion(1, 2, 3, 4);
                q.Multiply(i);
                var tQ = System.Numerics.Quaternion.Multiply(tqq, i);
                Assert.AreEqual(tQ.X, q.X, 0.000001);
                Assert.AreEqual(tQ.Y, q.Y, 0.000001);
                Assert.AreEqual(tQ.Z, q.Z, 0.000001);
                Assert.AreEqual(tQ.W, q.W, 0.000001);
            }
        }

        [TestMethod]
        public void Multiply_Vector_Test()
        {

            var tqq = new System.Numerics.Quaternion(2, 3, 4, 1);

            for (int i = 0; i <= 100; i++)
            {
                for (int j = 0; j <= 100; j++)
                {
                    for (int k = 0; k <= 100; k++)
                    {
                        var q = new Quaternion(1, 2, 3, 4);
                        var v = new Vector3D(i, j, k);
                        q.Multiply(v);
                        var tV = new System.Numerics.Vector3(i, j, k);
                        var tttq = new System.Numerics.Quaternion(tV, 0);
                        var tQ = System.Numerics.Quaternion.Multiply(tqq, tttq);
                        Assert.AreEqual(tQ.X, q.X, 0.000001);
                        Assert.AreEqual(tQ.Y, q.Y, 0.000001);
                        Assert.AreEqual(tQ.Z, q.Z, 0.000001);
                        Assert.AreEqual(tQ.W, q.W, 0.000001);
                    }
                }
            }
        }

        [TestMethod]
        public void Multiply_Quaternion_Test()
        {
            var tqq = new System.Numerics.Quaternion(2, 3, 4, 1);
            for (int i = 0; i <= 360; i++)
            {
                var q = new Quaternion(1, 2, 3, 4);
                var qm = new Quaternion();
                qm.SetFromAxisAngleDegrees(i, Vector3D.XAxis);
                q.Multiply(qm);
                var qtt = System.Numerics.Quaternion.CreateFromAxisAngle(Vector3.UnitX, (float)Utils.ToRadian(i));
                var tQ = System.Numerics.Quaternion.Multiply(tqq, qtt);
                Assert.AreEqual(tQ.X, q.X, 0.000001);
                Assert.AreEqual(tQ.Y, q.Y, 0.000001);
                Assert.AreEqual(tQ.Z, q.Z, 0.000001);
                Assert.AreEqual(tQ.W, q.W, 0.000001);

                q = new Quaternion(1, 2, 3, 4);
                qm = new Quaternion();
                qm.SetFromAxisAngleDegrees(i, Vector3D.YAxis);
                q.Multiply(qm);
                qtt = System.Numerics.Quaternion.CreateFromAxisAngle(Vector3.UnitY, (float)Utils.ToRadian(i));
                tQ = System.Numerics.Quaternion.Multiply(tqq, qtt);
                Assert.AreEqual(tQ.X, q.X, 0.000001);
                Assert.AreEqual(tQ.Y, q.Y, 0.000001);
                Assert.AreEqual(tQ.Z, q.Z, 0.000001);
                Assert.AreEqual(tQ.W, q.W, 0.000001);

                q = new Quaternion(1, 2, 3, 4);
                qm = new Quaternion();
                qm.SetFromAxisAngleDegrees(i, Vector3D.ZAxis);
                q.Multiply(qm);
                qtt = System.Numerics.Quaternion.CreateFromAxisAngle(Vector3.UnitZ, (float)Utils.ToRadian(i));
                tQ = System.Numerics.Quaternion.Multiply(tqq, qtt);
                Assert.AreEqual(tQ.X, q.X, 0.000001);
                Assert.AreEqual(tQ.Y, q.Y, 0.000001);
                Assert.AreEqual(tQ.Z, q.Z, 0.000001);
                Assert.AreEqual(tQ.W, q.W, 0.000001);
            }
        }

        [TestMethod]
        public void Invert_Test()
        {
            var q = new Quaternion();
            for (int i = 0; i <= 360; i++)
            {
                q.SetFromAxisAngleDegrees(i, Vector3D.XAxis);
                var tQ = System.Numerics.Quaternion.CreateFromAxisAngle(Vector3.UnitX, (float)Utils.ToRadian(i));
                q.Invert();
                tQ = System.Numerics.Quaternion.Inverse(tQ);
                Assert.AreEqual(tQ.X, q.X, 0.000001);
                Assert.AreEqual(tQ.Y, q.Y, 0.000001);
                Assert.AreEqual(tQ.Z, q.Z, 0.000001);
                Assert.AreEqual(tQ.W, q.W, 0.000001);

                q.SetFromAxisAngleDegrees(i, Vector3D.YAxis);
                tQ = System.Numerics.Quaternion.CreateFromAxisAngle(Vector3.UnitY, (float)Utils.ToRadian(i));
                q.Invert();
                tQ = System.Numerics.Quaternion.Inverse(tQ);
                Assert.AreEqual(tQ.X, q.X, 0.000001);
                Assert.AreEqual(tQ.Y, q.Y, 0.000001);
                Assert.AreEqual(tQ.Z, q.Z, 0.000001);
                Assert.AreEqual(tQ.W, q.W, 0.000001);

                q.SetFromAxisAngleDegrees(i, Vector3D.ZAxis);
                tQ = System.Numerics.Quaternion.CreateFromAxisAngle(Vector3.UnitZ, (float)Utils.ToRadian(i));
                q.Invert();
                tQ = System.Numerics.Quaternion.Inverse(tQ);
                Assert.AreEqual(tQ.X, q.X, 0.000001);
                Assert.AreEqual(tQ.Y, q.Y, 0.000001);
                Assert.AreEqual(tQ.Z, q.Z, 0.000001);
                Assert.AreEqual(tQ.W, q.W, 0.000001);
            }
        }


        [TestMethod]
        public void Normalize_Test()
        {
            var q = new Quaternion();
            for (int i = 0; i <= 360; i++)
            {
                q.SetFromAxisAngleDegrees(i, Vector3D.XAxis);
                var tQ = System.Numerics.Quaternion.CreateFromAxisAngle(Vector3.UnitX, (float)Utils.ToRadian(i));
                q.Normalize();
                tQ = System.Numerics.Quaternion.Normalize(tQ);
                Assert.AreEqual(tQ.X, q.X, 0.000001);
                Assert.AreEqual(tQ.Y, q.Y, 0.000001);
                Assert.AreEqual(tQ.Z, q.Z, 0.000001);
                Assert.AreEqual(tQ.W, q.W, 0.000001);

                q.SetFromAxisAngleDegrees(i, Vector3D.YAxis);
                tQ = System.Numerics.Quaternion.CreateFromAxisAngle(Vector3.UnitY, (float)Utils.ToRadian(i));
                q.Normalize();
                tQ = System.Numerics.Quaternion.Normalize(tQ);
                Assert.AreEqual(tQ.X, q.X, 0.000001);
                Assert.AreEqual(tQ.Y, q.Y, 0.000001);
                Assert.AreEqual(tQ.Z, q.Z, 0.000001);
                Assert.AreEqual(tQ.W, q.W, 0.000001);

                q.SetFromAxisAngleDegrees(i, Vector3D.ZAxis);
                tQ = System.Numerics.Quaternion.CreateFromAxisAngle(Vector3.UnitZ, (float)Utils.ToRadian(i));
                q.Normalize();
                tQ = System.Numerics.Quaternion.Normalize(tQ);
                Assert.AreEqual(tQ.X, q.X, 0.000001);
                Assert.AreEqual(tQ.Y, q.Y, 0.000001);
                Assert.AreEqual(tQ.Z, q.Z, 0.000001);
                Assert.AreEqual(tQ.W, q.W, 0.000001);
            }
        }

        [TestMethod]
        public void SetFromEulerDegrees_Test()
        {
            var q = new Quaternion();
            for (int i = 0; i <= 360; i++)
            {

                q.SetFromEulerDegrees(i, 0, 0);
                var tQ = System.Numerics.Quaternion.CreateFromAxisAngle(Vector3.UnitX, (float)Utils.ToRadian(i));
                Assert.AreEqual(tQ.X, q.X, 0.000001);
                Assert.AreEqual(tQ.Y, q.Y, 0.000001);
                Assert.AreEqual(tQ.Z, q.Z, 0.000001);
                Assert.AreEqual(tQ.W, q.W, 0.000001);

                q.SetFromEulerDegrees(0, i, 0);
                tQ = System.Numerics.Quaternion.CreateFromAxisAngle(Vector3.UnitY, (float)Utils.ToRadian(i));
                Assert.AreEqual(tQ.X, q.X, 0.000001);
                Assert.AreEqual(tQ.Y, q.Y, 0.000001);
                Assert.AreEqual(tQ.Z, q.Z, 0.000001);
                Assert.AreEqual(tQ.W, q.W, 0.000001);

                q.SetFromEulerDegrees(0, 0, i);
                tQ = System.Numerics.Quaternion.CreateFromAxisAngle(Vector3.UnitZ, (float)Utils.ToRadian(i));
                Assert.AreEqual(tQ.X, q.X, 0.000001);
                Assert.AreEqual(tQ.Y, q.Y, 0.000001);
                Assert.AreEqual(tQ.Z, q.Z, 0.000001);
                Assert.AreEqual(tQ.W, q.W, 0.000001);
            }
        }
    }
}
