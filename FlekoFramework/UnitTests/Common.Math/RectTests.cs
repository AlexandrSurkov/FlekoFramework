using Flekosoft.Common.Math;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Flekosoft.UnitTests.Common.Math
{
    [TestClass]
    public class RectTest
    {
        [TestMethod]
        public void Tests()
        {
            double left = -12.34;
            double top = -10.34;
            double right = 12.34;
            double bottom = 10.34;

            var rect = new Rect(left, top, right, bottom);

            Assert.AreEqual(new Vector2D(left, bottom), rect.LeftBottom);
            Assert.AreEqual(new Vector2D(left, top), rect.LeftTop);
            Assert.AreEqual(new Vector2D(right, bottom), rect.RightBottom);
            Assert.AreEqual(new Vector2D(right, top), rect.RightTop);

            Assert.AreEqual(new Vector2D(0, 0), rect.Centre);

            var radius = rect.OutRadius;

            Assert.AreEqual(new Vector2D(0, 0).Distance(rect.LeftBottom), radius);
            Assert.AreEqual(new Vector2D(0, 0).Distance(rect.LeftTop), radius);
            Assert.AreEqual(new Vector2D(0, 0).Distance(rect.RightBottom), radius);
            Assert.AreEqual(new Vector2D(0, 0).Distance(rect.RightTop), radius);

            radius = rect.InRadius;
            Assert.AreEqual(System.Math.Abs(top - bottom) / 2, radius);

            Assert.IsTrue(rect.Contains(new Vector2D(left, top)));
            Assert.IsTrue(rect.Contains(new Vector2D(left, bottom)));
            Assert.IsTrue(rect.Contains(new Vector2D(right, top)));
            Assert.IsTrue(rect.Contains(new Vector2D(right, bottom)));

            Assert.IsTrue(rect.Contains(new Vector2D(0, 0)));

            Assert.IsFalse(rect.Contains(new Vector2D(left - 0.000001, top)));
            Assert.IsFalse(rect.Contains(new Vector2D(left, top - 0.000001)));

            Assert.IsFalse(rect.Contains(new Vector2D(left - 0.000001, bottom)));
            Assert.IsFalse(rect.Contains(new Vector2D(left, bottom + 0.000001)));

            Assert.IsFalse(rect.Contains(new Vector2D(right + 0.000001, top)));
            Assert.IsFalse(rect.Contains(new Vector2D(right, top - 0.000001)));

            Assert.IsFalse(rect.Contains(new Vector2D(right + 0.000001, bottom)));
            Assert.IsFalse(rect.Contains(new Vector2D(right, bottom + 0.000001)));


            var rect1 = new Rect(new Vector2D(0, 0), 10, 20);

            Assert.AreEqual(10, rect1.Width);
            Assert.AreEqual(20, rect1.Height);

            var rect3 = new Rect(rect1);

            Assert.AreEqual(rect1, rect3);

            Assert.AreNotEqual(rect, rect3);

            rect3 = new Rect();
            rect3.Left = -100;
            rect3.Right = -50;
            rect3.Top = -100;
            rect3.Bottom = -50;

            Assert.IsTrue(rect.Intersects(rect1));
            Assert.IsTrue(rect1.Intersects(rect));

            Assert.IsFalse(rect3.Intersects(rect1));
            Assert.IsFalse(rect3.Intersects(rect));

            Assert.AreEqual(rect1.LeftTop, rect1.TopBorder.Start);
            Assert.AreEqual(rect1.RightTop, rect1.TopBorder.Stop);

            Assert.AreEqual(rect1.LeftTop, rect1.LeftBorder.Start);
            Assert.AreEqual(rect1.LeftBottom, rect1.LeftBorder.Stop);

            Assert.AreEqual(rect1.RightTop, rect1.RightBorder.Start);
            Assert.AreEqual(rect1.RightBottom, rect1.RightBorder.Stop);

            Assert.AreEqual(rect1.LeftBottom, rect1.BottomBorder.Start);
            Assert.AreEqual(rect1.RightBottom, rect1.BottomBorder.Stop);

            var rect4 = rect1.Scale(10);
            Assert.AreEqual(0, rect4.Left);
            Assert.AreEqual(100, rect4.Right);
            Assert.AreEqual(0, rect4.Top);
            Assert.AreEqual(200, rect4.Bottom);

            rect4 = rect.Scale(10);
            Assert.AreEqual(left * 10, rect4.Left);
            Assert.AreEqual(right * 10, rect4.Right);
            Assert.AreEqual(top * 10, rect4.Top);
            Assert.AreEqual(bottom * 10, rect4.Bottom);

            Assert.AreNotEqual(rect.GetHashCode(), rect1.GetHashCode());
            Assert.AreNotEqual(rect.GetHashCode(), rect3.GetHashCode());
            Assert.AreNotEqual(rect.GetHashCode(), rect4.GetHashCode());

            rect1.Union(new Vector2D(40, 40));
            Assert.AreEqual(0, rect1.Left);
            Assert.AreEqual(40, rect1.Right);
            Assert.AreEqual(0, rect1.Top);
            Assert.AreEqual(40, rect1.Bottom);

            rect1.Union(new Vector2D(-40, -40));
            Assert.AreEqual(-40, rect1.Left);
            Assert.AreEqual(40, rect1.Right);
            Assert.AreEqual(-40, rect1.Top);
            Assert.AreEqual(40, rect1.Bottom);

            left = 0;
            right = 10;
            top = 0;
            bottom = 10;
            rect = new Rect(left, top, right, bottom);
            rect1 = new Rect(right, top, right + 10, bottom);
            rect.Union(rect1);
            Assert.AreEqual(left, rect.Left);
            Assert.AreEqual(right + 10, rect.Right);
            Assert.AreEqual(top, rect.Top);
            Assert.AreEqual(bottom, rect.Bottom);

            rect = new Rect(left, top, right, bottom);
            rect1 = new Rect(right, top, right + 10, bottom + 10);
            rect.Union(rect1);
            Assert.AreEqual(left, rect.Left);
            Assert.AreEqual(right + 10, rect.Right);
            Assert.AreEqual(top, rect.Top);
            Assert.AreEqual(bottom + 10, rect.Bottom);

            rect = new Rect(left, top, right, bottom);
            rect1 = new Rect(left - 10, top - 10, right, bottom);
            rect.Union(rect1);
            Assert.AreEqual(left - 10, rect.Left);
            Assert.AreEqual(right, rect.Right);
            Assert.AreEqual(top - 10, rect.Top);
            Assert.AreEqual(bottom, rect.Bottom);


        }
    }


}
