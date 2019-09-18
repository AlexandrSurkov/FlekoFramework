namespace Flekosoft.Common.Math
{
    /// <summary>
    /// Класс, описывающий плоскость
    /// </summary>
    public class Plane
    {
        public double A { get; private set; }
        public double B { get; private set; }
        public double C { get; private set; }
        public double D { get; private set; }

        public Plane(Vector3D p1, Vector3D p2, Vector3D p3)
        {
            A = p1.Y * (p2.Z - p3.Z) + p2.Y * (p3.Z - p1.Z) + p3.Y * (p1.Z - p2.Z);

            B = p1.Z * (p2.X - p3.X) + p2.Z * (p3.X - p1.X) + p3.Z * (p1.X - p2.X);

            C = p1.X * (p2.Y - p3.Y) + p2.X * (p3.Y - p1.Y) + p3.X * (p1.Y - p2.Y);

            D = -(p1.X * (p2.Y * p3.Z - p3.Y * p2.Z) + p2.X * (p3.Y * p1.Z - p1.Y * p3.Z) + p3.X * (p1.Y * p2.Z - p2.Y * p1.Z));
        }


        public double DistanceToPoint(Vector3D point)
        {
            var a = A;
            var b = B;
            var c = C;
            var d = D;
            var dist = System.Math.Abs(a * point.X + b * point.Y + c * point.Z + d) / System.Math.Sqrt(a * a + b * b + c * c);
            return dist;
        }



        public Vector3D PointProjection(Vector3D point)
        {
            var p = point;
            var t = (-(A * p.X + B * p.Y + C * p.Z + D)) / (A * A + B * B + C * C);
            var x = System.Math.Round(p.X + A * t, 10);
            var y = System.Math.Round(p.Y + B * t, 10);
            var z = System.Math.Round(p.Z + C * t, 10);

            return new Vector3D(x, y, z);
        }

        public bool Contains(Vector3D point)
        {
            var res = A * point.X + B * point.Y + C * point.Z + D;
            if (System.Math.Abs(res) < 0.0001) return true;
            return false;

        }

        /// <summary>
        /// Найти пересечение линии в пространстве и плоскости
        /// </summary>
        /// <param name="lineSegment"></param>
        /// <returns></returns>
        public Vector3D PointOfLineSegment3DCross(LineSegment3D lineSegment)
        {
            var ls = lineSegment;

            var d = A * ls.l + B * ls.m + C * ls.n;

            if (System.Math.Abs(d) < 0.00001) return null; //Пересечения нет

            var t = (-(A * ls.A0.X + B * ls.A0.Y + C * ls.A0.Z + D)) / (A * ls.l + B * ls.m + C * ls.n);
            var x = System.Math.Round(ls.A0.X + ls.l * t, 10);
            var y = System.Math.Round(ls.A0.Y + ls.m * t, 10);
            var z = System.Math.Round(ls.A0.Z + ls.n * t, 10);

            return new Vector3D(x, y, z);
        }
    }
}
