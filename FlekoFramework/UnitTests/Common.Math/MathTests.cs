using System;
using Flekosoft.Common.Math;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Flekosoft.UnitTests.Common.Math
{
    [TestClass]
    public class MathTests
    {
        [TestMethod]
        public void Tests()
        {
            const double v1 = -1000.2345;
            const double v2 = 1000.2345;
            const double v3 = 0.2345;

            const double min = -1;
            const double max = 1;

            var res = Utils.Clamp(v1, min, max);

            Assert.IsTrue(res <= max);
            Assert.IsTrue(res >= min);

            res = Utils.Clamp(v2, min, max);
            Assert.IsTrue(res <= max);
            Assert.IsTrue(res >= min);

            res = Utils.Clamp(v3, min, max);
            Assert.IsTrue(res <= max);
            Assert.IsTrue(res >= min);


            Assert.AreEqual(10, System.Math.Abs(-10));
            Assert.AreEqual(10, System.Math.Abs(10));

            Assert.AreEqual((float)3.14159265358979, (float)System.Math.PI);

            Assert.AreEqual(1, System.Math.Cos(0));
            Assert.AreEqual(-1, System.Math.Cos(System.Math.PI));

            Assert.AreEqual(0, System.Math.Sin(0));
            Assert.AreEqual(0, System.Math.Round(System.Math.Sin(System.Math.PI)));


            Assert.AreEqual(0, System.Math.Tan(0));
            Assert.AreEqual(1, System.Math.Round(System.Math.Tan(System.Math.PI / 4)));
            Assert.AreEqual(0, System.Math.Round(System.Math.Tan(System.Math.PI)));
            Assert.AreEqual(0, System.Math.Round(System.Math.Tan(2 * System.Math.PI)));
            Assert.AreEqual(System.Math.Round(1 / System.Math.Sqrt(3)), System.Math.Round(System.Math.Tan(System.Math.PI / 6)));
            Assert.AreEqual(System.Math.Round(System.Math.Sqrt(3)), System.Math.Round(System.Math.Tan(System.Math.PI / 3)));


            Assert.AreEqual(v2, System.Math.Max(v1, v2));
            Assert.AreEqual(v1, System.Math.Min(v1, v2));

            Assert.AreEqual(2, System.Math.Sqrt(4));
            Assert.AreEqual(3, System.Math.Sqrt(9));

            Assert.AreEqual((float)0, (float)Utils.ToRadian(0));
            Assert.AreEqual((float)0.785398163397448, (float)Utils.ToRadian(45));
            Assert.AreEqual((float)1.570796326794897, (float)Utils.ToRadian(90));
            Assert.AreEqual((float)2.356194490192345, (float)Utils.ToRadian(135));
            Assert.AreEqual((float)3.141592653589793, (float)Utils.ToRadian(180));
            Assert.AreEqual((float)System.Math.PI, (float)Utils.ToRadian(180));
            Assert.AreEqual((float)3.926990816987241, (float)Utils.ToRadian(225));
            Assert.AreEqual((float)4.71238898038469, (float)Utils.ToRadian(270));
            Assert.AreEqual((float)5.497787143782138, (float)Utils.ToRadian(315));
            Assert.AreEqual((float)6.283185307179586, (float)Utils.ToRadian(360));
            Assert.AreEqual((float)(2 * System.Math.PI), (float)Utils.ToRadian(360));

            Assert.AreEqual((float)0, (float)Utils.ToDegrees(0));
            Assert.AreEqual((float)45, (float)Utils.ToDegrees(0.785398163397448));
            Assert.AreEqual((float)90, (float)Utils.ToDegrees(1.570796326794897));
            Assert.AreEqual((float)135, (float)Utils.ToDegrees(2.356194490192345));
            Assert.AreEqual((float)180, (float)Utils.ToDegrees(3.141592653589793));
            Assert.AreEqual((float)180, (float)Utils.ToDegrees(System.Math.PI));
            Assert.AreEqual((float)225, (float)Utils.ToDegrees(3.926990816987241));
            Assert.AreEqual((float)270, (float)Utils.ToDegrees(4.71238898038469));
            Assert.AreEqual((float)315, (float)Utils.ToDegrees(5.497787143782138));
            Assert.AreEqual((float)360, (float)Utils.ToDegrees(6.283185307179586));
            Assert.AreEqual((float)360, (float)Utils.ToDegrees((2 * System.Math.PI)));

            Assert.AreEqual(1, System.Math.Sin(System.Math.Asin(1)));
            Assert.AreEqual(1, System.Math.Cos(System.Math.Acos(1)));

            Assert.AreEqual(System.Math.Round((double)1), System.Math.Round(System.Math.Round(System.Math.Tan(System.Math.Atan(1)))));
            Assert.AreEqual(System.Math.Round((double)1), System.Math.Round(System.Math.Atan(System.Math.Tan(1))));



            Assert.AreEqual(3.14, System.Math.Round(System.Math.PI, 2));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ErrorTest()
        {
            var res = Utils.Clamp(1, 2, 1);
        }
    }


}
