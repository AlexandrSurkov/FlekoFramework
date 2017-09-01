using Flekosoft.Common.Math;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Flekosoft.UnitTests.Common.Math
{
    [TestClass]
    public class Vector2DTests
    {
        [TestMethod]
        public void Zero_Tests()
        {
            var v1 = new Vector2D(0, 0);
            Assert.IsTrue(v1.IsZero());
            v1 = new Vector2D(1, -1);
            Assert.IsFalse(v1.IsZero());
            v1.Zero();
            Assert.IsTrue(v1.IsZero());
        }


        [TestMethod]
        public void Operators_Tests()
        {
            //+
            var v1 = new Vector2D(1, -1);
            var v2 = new Vector2D(-2, 2);
            var v3 = v1 + v2;
            Assert.AreEqual(-1, v3.X);
            Assert.AreEqual(1, v3.Y);
            //*
            v3 = v1 * 10;
            Assert.AreEqual(10, v3.X);
            Assert.AreEqual(-10, v3.Y);
            v3 = 15 * v2;
            Assert.AreEqual(-30, v3.X);
            Assert.AreEqual(30, v3.Y);
            //-
            v3 = v1 - v2;
            Assert.AreEqual(3, v3.X);
            Assert.AreEqual(-3, v3.Y);
            // /
            v3 = v1 / 10;
            Assert.AreEqual(0.1, v3.X);
            Assert.AreEqual(-0.1, v3.Y);
            //==
            var v4 = new Vector2D(v1);
            Assert.IsFalse(v1 == v2);
            Assert.IsTrue(v1 == v4);

            Assert.IsTrue(v1 != v2);
            Assert.IsFalse(v1 != v4);

        }

        [TestMethod]
        public void Length_Tests()
        {
            var v1 = new Vector2D(2, 2);
            var l = v1.Length();
            Assert.AreEqual(System.Math.Sqrt(8), l);
            l = v1.LengthSq();
            Assert.AreEqual(8, l);
        }

        [TestMethod]
        public void DotProduct_Tests()
        {
            var v1 = new Vector2D(2, 4);
            var v2 = new Vector2D(3, 5);
            var dp = v1.DotProduct(v2);
            var a = System.Math.Abs(dp - 26);
            var res = a < 0.000000001;
            Assert.IsTrue(res);
        }

        [TestMethod]
        public void Sign_Tests()
        {
            var v1 = new Vector2D(1, 0);
            var v2 = new Vector2D(1, 1);
            var dp = v1.Sign(v2);
            Assert.AreEqual(VectorSign.Clockwise, dp);
            var v3 = new Vector2D(1, -1);
            dp = v1.Sign(v3);
            Assert.AreEqual(VectorSign.Antickockwise, dp);
        }

        [TestMethod]
        public void Perp_Tests()
        {
            var v1 = new Vector2D(1, 0);
            var dp = v1.Perp();
            Assert.AreEqual(0, dp.X);
            Assert.AreEqual(1, dp.Y);

            dp = dp.Perp();
            Assert.AreEqual(-1, dp.X);
            Assert.AreEqual(0, dp.Y);
        }

        [TestMethod]
        public void Truncate_Tests()
        {
            var v1 = new Vector2D(4, 6);
            var l = v1.Length();
            Assert.IsTrue(l > 3);
            v1.Truncate(3);
            l = v1.Length();
            Assert.IsTrue(System.Math.Round(l) <= 3);
        }

        [TestMethod]
        public void Distance_Tests()
        {
            var v1 = new Vector2D(1, 0);
            var v2 = new Vector2D(1, 2);
            var d = System.Math.Round(v1.Distance(v2));
            Assert.AreEqual(2, d);
            d = System.Math.Round(v1.DistanceSq(v2));
            Assert.AreEqual(4, d);
        }

        [TestMethod]
        public void Reflect_Tests()
        {
            var v1 = new Vector2D(1, 0);
            var n = new Vector2D(-1, 0);
            v1.Reflect(n);
            Assert.AreEqual(-1, v1.X);
            Assert.AreEqual(0, v1.Y);

            v1 = new Vector2D(1, 1);
            v1.Reflect(n);
            Assert.AreEqual(-1, v1.X);
            Assert.AreEqual(1, v1.Y);

            v1 = new Vector2D(-1, 1);
            v1.Reflect(n);
            Assert.AreEqual(1, v1.X);
            Assert.AreEqual(1, v1.Y);

            v1 = new Vector2D(-1, 1);
            var oldL = v1.Length();
            v1.Reflect(n, 0.7);
            var l = System.Math.Round(v1.Length(), 2);
            Assert.AreEqual(System.Math.Round(oldL * 0.7, 2), l);
        }

        [TestMethod]
        public void GetReverse_Tests()
        {
            var v1 = new Vector2D(1, 0);
            var v2 = new Vector2D(1, 2);
            var res = v1.GetReverse();
            Assert.AreEqual(-1, res.X);
            Assert.AreEqual(0, res.Y);
            res = v2.GetReverse();
            Assert.AreEqual(-1, res.X);
            Assert.AreEqual(-2, res.Y);
        }

        [TestMethod]
        public void Static_Normalize_Tests()
        {
            var v1 = new Vector2D(2, 2);
            var l = v1.Length();
            Assert.AreNotEqual(1, l);
            var v2 = Vector2D.Normalize(v1);
            l = v2.Length();
            var a = System.Math.Abs(l - 1);
            var res = a < 0.000000001;
            Assert.IsTrue(res);
        }

        [TestMethod]
        public void Static_Distance_Tests()
        {
            var v1 = new Vector2D(1, 0);
            var v2 = new Vector2D(1, 2);
            var d = System.Math.Round(Vector2D.Distance(v1, v2));
            Assert.AreEqual(2, d);
            d = System.Math.Round(Vector2D.DistanceSq(v1, v2));
            Assert.AreEqual(4, d);
        }

        [TestMethod]
        public void Static_Length_Tests()
        {
            var v1 = new Vector2D(2, 2);
            var l = Vector2D.Length(v1);
            Assert.AreEqual(System.Math.Sqrt(8), l);
            l = Vector2D.LengthSq(v1);
            Assert.AreEqual(8, l);
        }

        [TestMethod]
        public void Angle_Tests()
        {
            var v1 = new Vector2D(1, 0);
            var l = v1.Angle;
            Assert.AreEqual(0, l);

            v1 = new Vector2D(0, 1);
            l = v1.Angle;
            Assert.AreEqual(System.Math.PI / 2, l);

            v1 = new Vector2D(-1, 0);
            l = v1.Angle;
            Assert.AreEqual(System.Math.PI, l);

            v1 = new Vector2D(0, -1);
            l = v1.Angle;
            Assert.AreEqual(3 * System.Math.PI / 2, l);

            v1 = new Vector2D(1, -1);
            l = v1.Angle;
            Assert.AreEqual(1.75 * System.Math.PI, l);
        }

        [TestMethod]
        public void AngleTo_Tests()
        {
            var v = new Vector2D(1, 0);
            var l = v.Angle;
            Assert.AreEqual(0, l);

            var v1 = new Vector2D(0, 1);
            l = v1.Angle;
            Assert.AreEqual(System.Math.PI / 2, l);

            var v2 = new Vector2D(-1, 0);
            l = v2.Angle;
            Assert.AreEqual(System.Math.PI, l);

            var v3 = new Vector2D(0, -1);
            l = v3.Angle;
            Assert.AreEqual(3 * System.Math.PI / 2, l);

            var v4 = new Vector2D(1, -1);
            l = v4.Angle;
            Assert.AreEqual(1.75 * System.Math.PI, l);

            var a = v.AngleTo(v1);
            Assert.AreEqual(-System.Math.PI / 2, a);

            a = v.AngleTo(v2);
            Assert.AreEqual(-System.Math.PI, a);

            a = v.AngleTo(v3);
            Assert.AreEqual(-(3 * System.Math.PI / 2), a);

            a = v.AngleTo(v4);
            Assert.AreEqual(-(1.75 * System.Math.PI), a);
        }
    }


}
