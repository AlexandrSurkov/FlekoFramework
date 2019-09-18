namespace Flekosoft.Common.Math
{
    public class LineSegment2D
    {
        private Vector2D _start;
        private Vector2D _stop;

        public LineSegment2D(LineSegment2D lineSegment)
        {
            Start = lineSegment.Start;
            Stop = lineSegment.Stop;
        }

        public LineSegment2D(Vector2D start, Vector2D stop)
        {
            Start = new Vector2D(start);
            Stop = new Vector2D(stop);
        }

        public Rect Bounds
        {
            get
            {
                var res = new Rect(Start.X, Start.Y, Start.X, Start.Y);
                res.Union(Stop);

                return res;
            }
        }

        // ReSharper disable once ConvertToAutoProperty
        public Vector2D Start
        {
            get { return _start; }
            private set { _start = value; }
        }

        // ReSharper disable once ConvertToAutoProperty
        public Vector2D Stop
        {
            get { return _stop; }
            private set { _stop = value; }
        }


        /// <summary>
        /// Параметр A общего уравнения прямой
        /// </summary>
        public double A
        {
            get { return _start.Y - _stop.Y; }
        }

        /// <summary>
        /// Параметр B общего уравнения прямой
        /// </summary>
        public double B
        {
            get { return _stop.X - _start.X; }
        }

        /// <summary>
        /// Параметр С общего уравнения прямой
        /// </summary>
        public double C
        {
            get { return (_start.X * _stop.Y) - (_stop.X * _start.Y); }
        }

        public double DistanceToPoint(Vector2D point)
        {
            var a = A;
            var b = B;
            var c = C;
            var d = System.Math.Abs(a * point.X + b * point.Y + c) / System.Math.Sqrt(a * a + b * b);
            return d;
        }


        /// <summary>
        /// Возвращает проекцию точки point на прямой
        /// </summary>
        /// <param name="point">Точка, которую нужно спроецировать</param>
        /// <param name="projectPoint">Проекция точка</param>
        /// <returns>True, если проекция лежит на отрезке. Иначе false</returns>
        public bool ProjectPointToLine(Vector2D point, out Vector2D projectPoint)
        {
            Vector2D v1 = point - _start;
            Vector2D v2 = _stop - _start;
            projectPoint = _start + v2 * ((v1 * v2) / v2.LengthSq());

            if ((v1 * v2 >= 0.0f) && ((v1 * v2) / (v2.LengthSq()) <= 1.0f))
            {
                return true;
            }
            return false;
        }


        public LineSegmentIntersecResult Intersec(LineSegment2D line)
        {

            double x1 = Start.X;
            double y1 = Start.Y;
            double x2 = Stop.X;
            double y2 = Stop.Y;

            double x3 = line.Start.X;
            double y3 = line.Start.Y;
            double x4 = line.Stop.X;
            double y4 = line.Stop.Y;

            // ReSharper disable once CompareOfFloatsByEqualityOperator
            bool l1Vertical = (x1 == x2);
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            bool l2Vertical = (x3 == x4);

            if (l1Vertical && l2Vertical)
            {
                // Если обе линии вертикальны и имеют общую точку, то они
                // накладываются друг на друга
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (x1 == x3)
                {
                    // Если отрезки по Y накладываются, то и линии накладываются
                    if (((System.Math.Min(y1, y2) <= y3) && (y3 <= System.Math.Max(y1, y2)))
                            || ((System.Math.Min(y1, y2) <= y4) && (y3 <= System.Math.Max(y1, y2))))
                    {
                        return new LineSegmentIntersecResult(
                                LineSegmentIntersecResult.IntersecResult.Overlaps, null);
                    }
                    // иначе линии не пересекаются
                    return new LineSegmentIntersecResult(
                            LineSegmentIntersecResult.IntersecResult.DontIntersecs, null);
                }
                // Если обе линии вертикальны и не имеют общей точкк, то они не
                // пересекаются
                else
                    return new LineSegmentIntersecResult(
                            LineSegmentIntersecResult.IntersecResult.DontIntersecs, null);
            }

            // ReSharper disable once CompareOfFloatsByEqualityOperator
            bool l1Horizontal = (y1 == y2);
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            bool l2Horizontal = (y3 == y4);

            if (l1Horizontal && l2Horizontal)
            {
                // Если обе линии горизонтальны и имеют общую точку, то они
                // накладываются друг на друга
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (y1 == y3)
                {
                    // Если отрезки по X накладываются, то и линии накладываются
                    if (((System.Math.Min(x1, x2) <= x3) && (x3 <= System.Math.Max(x1, x2)))
                            || ((System.Math.Min(x1, x2) <= x4) && (x3 <= System.Math.Max(x1, x2))))
                    {
                        return new LineSegmentIntersecResult(
                                LineSegmentIntersecResult.IntersecResult.Overlaps, null);
                    }
                    // иначе линии не пересекаются
                    return new LineSegmentIntersecResult(
                            LineSegmentIntersecResult.IntersecResult.DontIntersecs, null);
                }

                // Если обе линии горизонтальны и не имеют общей точку, то они не
                // пересекаются
                else
                    return new LineSegmentIntersecResult(
                            LineSegmentIntersecResult.IntersecResult.DontIntersecs, null);
            }

            // Считаем коэффицианты
            double h1 = y2 - y1;
            double w1 = x2 - x1;
            double h2 = y4 - y3;
            double w2 = x4 - x3;

            double a1 = h1 / w1;
            double b1 = y1 - x1 * a1;
            double a2 = h2 / w2;
            double b2 = y3 - x2 * a2;

            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (a1 == a2)
            {
                // Линии параллельны и накладываются друг на друга
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (b1 == b2)
                {
                    // имеют тот же сдвиг по Y и при этом имеют накладывающиеся
                    // отрезки по X, то линии накладываются
                    if (((System.Math.Min(x1, x2) <= x3) && (x3 <= System.Math.Max(x1, x2)))
                            || ((System.Math.Min(x1, x2) <= x4) && (x3 <= System.Math.Max(x1, x2))))
                    {
                        return new LineSegmentIntersecResult(
                                LineSegmentIntersecResult.IntersecResult.Overlaps, null);
                    }
                    // иначе линии не пересекаются
                    return new LineSegmentIntersecResult(
                            LineSegmentIntersecResult.IntersecResult.DontIntersecs, null);
                }
                // Линии параллельны и не пересекаются
                return new LineSegmentIntersecResult(
                        LineSegmentIntersecResult.IntersecResult.DontIntersecs, null);
            }

            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (h1 * w2 + h2 * w1 == 0)
            {
                // линии не пересекаются
                return new LineSegmentIntersecResult(
                        LineSegmentIntersecResult.IntersecResult.DontIntersecs, null);
            }

            // Считаем координату Х
            double x = (w1 * w2 * (y3 - y1) - x3 * h2 * w1 + x1 * h1 * w2)
                    / (h1 * w2 - h2 * w1);

            // теперь по координате Х считаем Y
            // ReSharper disable once RedundantAssignment
            double y = 0;
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (w1 != 0)
                y = x * h1 / w1 + y1 - x1 * h1 / w1;
            else
                y = x * h2 / w2 + y3 - x3 * h2 / w2;

            Vector2D intersecPoint = new Vector2D(x, y);

            // Если точка пересечения принадлежит обоим отрезкам, то они
            // пересекаются
            if (Contains(intersecPoint) && line.Contains(intersecPoint))
                return new LineSegmentIntersecResult(
                        LineSegmentIntersecResult.IntersecResult.Intersecs, intersecPoint);

            // Отрезки не пересекаются
            return new LineSegmentIntersecResult(
                    LineSegmentIntersecResult.IntersecResult.DontIntersecs, null);
        }

        public bool Contains(Vector2D point)
        {
            double x1 = Start.X;
            double y1 = Start.Y;
            double x2 = Stop.X;
            double y2 = Stop.Y;

            double minX = System.Math.Min(x1, x2);
            double maxX = System.Math.Max(x1, x2);

            double minY = System.Math.Min(y1, y2);
            double maxY = System.Math.Max(y1, y2);

            var a = (minX <= point.X);// || (Utils.Abs(minX - point.X) < 0.01);
            var b = (point.X <= maxX);// || (Utils.Abs(maxX - point.X) < 0.01);

            var c = (minY <= point.Y);// || (Utils.Abs(minY - point.Y) < 0.01);
            var d = (point.Y <= maxY);// || (Utils.Abs(maxY - point.Y) < 0.01);

            if (((a) && (b)) && ((c) && (d)))
                return true;

            return false;
        }

        #region Object Overrides

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((LineSegment2D)obj);
        }

        protected bool Equals(LineSegment2D other)
        {
            return _start.Equals(other._start) && _stop.Equals(other._stop);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Start.GetHashCode() * 397) ^ Stop.GetHashCode();
            }
        }

        public override string ToString()
        {
            return "LineSegment: Start = " + Start + " Stop = " + Stop;
        }

        #endregion
    }

    public class LineSegmentIntersecResult
    {

        public enum IntersecResult
        {
            /// <summary>
            /// Линии совпадают
            /// </summary>
            Overlaps,
            /// <summary>
            /// Не пересекаются
            /// </summary>
            DontIntersecs,
            /// <summary>
            /// Пересекаются
            /// </summary>
            Intersecs
        }

        public LineSegmentIntersecResult(IntersecResult result, Vector2D intersecPoint)
        {
            Result = result;
            if (intersecPoint != null)
                IntersecPoint = new Vector2D(intersecPoint);
            else
            {
                IntersecPoint = null;
            }
        }


        public Vector2D IntersecPoint { get; }

        public IntersecResult Result { get; }
    }
}
