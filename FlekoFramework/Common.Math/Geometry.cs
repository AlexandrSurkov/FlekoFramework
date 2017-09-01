using System;
using System.Collections.Generic;

namespace Flekosoft.Common.Math
{
    public static class Geometry
    {
        /// <summary>
        /// Given a point P and a circle of radius R centered at C this function
        ///  determines the two points on the circle that intersect with the 
        ///  tangents from P to the circle. Returns false if P is within the circle.
        /// </summary>
        /// <param name="C">Центр окружности</param>
        /// <param name="R">Радиус окружности</param>
        /// <param name="P">Точка, от которой идут касательные </param>
        /// <returns></returns>
        public static bool GetTangentPoints(Vector2D C, double R, Vector2D P, out Vector2D T1, out Vector2D T2)
        {
            T1 = new Vector2D();
            T2 = new Vector2D();

            Vector2D pmC = P - C;
            double sqrLen = pmC.LengthSq();
            double rSqr = R * R;
            if (sqrLen <= rSqr)
            {
                // P is inside or on the circle
                return false;
            }

            double invSqrLen = 1 / sqrLen;
            double root = System.Math.Sqrt(System.Math.Abs(sqrLen - rSqr));

            T1.X = C.X + R * (R * pmC.X - pmC.Y * root) * invSqrLen;
            T1.Y = C.Y + R * (R * pmC.Y + pmC.X * root) * invSqrLen;
            T2.X = C.X + R * (R * pmC.X + pmC.Y * root) * invSqrLen;
            T2.Y = C.Y + R * (R * pmC.Y - pmC.X * root) * invSqrLen;

            return true;
        }


        // Find the points of intersection between
        // an ellipse and a line segment.
        public static Vector2D[] FindEllipseAndLineSegmentIntersections(Ellipse ellipse, LineSegment2D line, bool lineSegmentOnly)
        {
            var pt1 = new Vector2D(line.Start);
            var pt2 = new Vector2D(line.Stop);

            // If the ellipse or line segment are empty, return no intersections.
            if ((System.Math.Abs(ellipse.Bounds.Width) <= 0) ||
                (System.Math.Abs(ellipse.Bounds.Height) <= 0) ||
                ((System.Math.Abs(pt1.X - pt2.X) <= 0) &&
                (System.Math.Abs(pt1.Y - pt2.Y) <= 0))
                ) return new Vector2D[] { };
            // Make sure the rectangle has non-negative width and height.
            if (ellipse.Bounds.Width < 0) throw new ArgumentException("rect has negative Width");
            if (ellipse.Bounds.Height < 0) throw new ArgumentException("rect has negative Height");

            // Translate so the ellipse is centered at the origin.
            double cx = ellipse.Bounds.Centre.X;
            double cy = ellipse.Bounds.Centre.Y;

            var rect = new Rect(
                ellipse.Bounds.Left - cx,
                ellipse.Bounds.Top - cy,
                ellipse.Bounds.Right - cx,
                ellipse.Bounds.Bottom - cy
                );

            //rect.X -= cx;
            //rect.Y -= cy;
            pt1.X -= cx;
            pt1.Y -= cy;
            pt2.X -= cx;
            pt2.Y -= cy;
            // Get the semimajor and semiminor axes.
            double a = rect.Width / 2;
            double b = rect.Height / 2;

            // Calculate the quadratic parameters.
            // ReSharper disable InconsistentNaming
            double A = (pt2.X - pt1.X) * (pt2.X - pt1.X) / a / a +
            (pt2.Y - pt1.Y) * (pt2.Y - pt1.Y) / b / b;
            double B = 2 * pt1.X * (pt2.X - pt1.X) / a / a +
            2 * pt1.Y * (pt2.Y - pt1.Y) / b / b;
            double C = pt1.X * pt1.X / a / a + pt1.Y * pt1.Y / b / b - 1;
            // ReSharper restore InconsistentNaming

            // Make a list of t values.
            var tValues = new List<double>();

            // Calculate the discriminant.
            double discriminant = B * B - 4 * A * C;

            if (System.Math.Abs(discriminant) <= 0)
            {
                // One real solution.
                tValues.Add(-B / 2 / A);
            }
            else if (discriminant > 0)
            {
                // Two real solutions.
                tValues.Add((-B + System.Math.Sqrt(discriminant)) / 2 / A);
                tValues.Add((-B - System.Math.Sqrt(discriminant)) / 2 / A);
            }
            // Convert the t values into points.
            var points = new List<Vector2D>();

            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (double t in tValues)
            {
                // If the points are on the segment (or we
                // don't care if they are), add them to the list.
                if (!lineSegmentOnly || ((t >= 0f) && (t <= 1f)))
                {
                    double x = pt1.X + (pt2.X - pt1.X) * t + cx;
                    double y = pt1.Y + (pt2.Y - pt1.Y) * t + cy;
                    points.Add(new Vector2D(x, y));
                }
            }
            // Return the points.
            return points.ToArray();
        }
    }
}
