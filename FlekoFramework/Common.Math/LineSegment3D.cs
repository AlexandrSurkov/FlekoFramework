using System;

namespace Flekosoft.Common.Math
{
    public class LineSegment3D
    {
        private Vector3D _start;
        private Vector3D _stop;
        private Vector3D _v;

        public LineSegment3D(LineSegment3D lineSegment)
        {
            Start = lineSegment.Start;
            Stop = lineSegment.Stop;
            _v = Stop - Start;
        }

        public LineSegment3D(Vector3D start, Vector3D stop)
        {
            Start = new Vector3D(start);
            Stop = new Vector3D(stop);
            _v = Stop - Start;
        }

        // ReSharper disable once ConvertToAutoProperty
        public Vector3D Start
        {
            get { return _start; }
            private set { _start = value; }
        }

        // ReSharper disable once ConvertToAutoProperty
        public Vector3D Stop
        {
            get { return _stop; }
            private set { _stop = value; }
        }

        public Box Bounds
        {
            get
            {
                var res = new Box(Start.X, Start.Y, Start.X, Start.Y, Start.Z, Start.Z);
                res.Union(Stop);

                return res;
            }
        }

        /// <summary>
        /// Точка на прямой
        /// </summary>
        public Vector3D A0
        {
            get { return _start; }
        }

        /// <summary>
        /// направляющий вектор
        /// </summary>
        public Vector3D V
        {
            get { return _v; }
        }

        public double l
        {
            get { return _v.X; }
        }

        public double m
        {
            get { return _v.Y; }
        }

        public double n
        {
            get { return _v.Z; }
        }

        public bool Contains(Vector3D point)
        {
            var t = GetT(point);
            var x = (point.X - A0.X) / l;
            var y = (point.Y - A0.Y) / m;
            var z = (point.Z - A0.Z) / n;

            if (m == 0 && n == 0)
            {
                y = x;
                z = x;
            }

            if (m == 0 && l == 0)
            {
                x = z;
                y = z;
            }

            if (n == 0 && l == 0)
            {
                x = y;
                z = y;
            }
            if (System.Math.Abs(x - y) < 0.00001 && System.Math.Abs(y - z) < 0.00001) return true;
            return false;
        }

        public bool IsPointBetweenToPoints(Vector3D startPoint, Vector3D stopPoint, Vector3D checkPoint)
        {
            if (!Contains(startPoint)) return false;
            if (!Contains(stopPoint)) return false;
            if (!Contains(checkPoint)) return false;

            var tS = GetT(startPoint);
            var tE = GetT(stopPoint);
            var tM = GetT(checkPoint);

            if (tS >= tM && tM >= tE) return true;
            if (tS <= tM && tM <= tE) return true;

            return false;
        }

        double GetT(Vector3D point)
        {
            var x = point.X - A0.X / l;
            var y = point.Y - A0.Y / m;
            var z = point.Z - A0.Z / n;

            if (m == 0 && n == 0)
            {
                y = x;
                z = x;
            }

            if (m == 0 && l == 0)
            {
                x = z;
                y = z;
            }

            if (n == 0 && l == 0)
            {
                x = y;
                z = y;
            }

            if (x == y && y == z) return x;
            return Double.NaN;
        }
    }
}
