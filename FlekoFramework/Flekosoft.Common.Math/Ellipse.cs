namespace Flekosoft.Common.Math
{
    public class Ellipse
    {
        /// <summary>
        /// Точка центра эллипса
        /// </summary>
        private Vector2D _location;
        private double _radiusX;
        private double _radiusY;
        private double _startAngle;
        private double _sweepAngle;
        private double _a;
        private double _b;
        private double _c;
        private double _e;
        private bool _isXBigHalfAxis;
        private Vector2D _f1;
        private Vector2D _f2;
        private Rect _bounds;


        public Ellipse(Ellipse ellipse)
        {
            _location = ellipse.Location;
            _radiusX = ellipse.RadiusX;
            _radiusY = ellipse.RadiusY;
            _startAngle = ellipse.StartAngle;
            _sweepAngle = ellipse.SweepAngle;
            UpdateBounds();
        }

        public Ellipse(Vector2D location, double radiusX, double radiusY,
                double startAngle, double sweepAngle)
        {
            _location = location;
            _radiusX = radiusX;
            _radiusY = radiusY;
            _startAngle = startAngle;
            _sweepAngle = sweepAngle;
            UpdateBounds();
        }

        public Ellipse(Vector2D location, double radiusX, double radiusY)
        {
            _location = location;
            _radiusX = radiusX;
            _radiusY = radiusY;
            _startAngle = 0;
            _sweepAngle = 360;
            UpdateBounds();
        }

        public Ellipse Scale(double factor)
        {
            return new Ellipse(_location * factor, _radiusX * factor, _radiusY * factor, _startAngle, _sweepAngle);
        }

        /// <summary>
        /// Точка центра эллипса
        /// </summary>
        public Vector2D Location
        {
            get { return _location; }
            set
            {
                _location = value;
                UpdateBounds();
            }
        }


        /// <summary>
        /// большая полуось
        /// </summary>
        // ReSharper disable once InconsistentNaming
        public double a
        {
            get
            {
                return _a;
            }
        }

        /// <summary>
        /// малая полуось
        /// </summary>
        // ReSharper disable once InconsistentNaming
        public double b
        {
            get
            {
                return _b;
            }
        }

        /// <summary>
        /// Эксцентриситет
        /// </summary>
        public double Eccentricity
        {
            get
            {
                return _e;
            }
        }

        /// <summary>
        /// Фокальное расстояние
        /// </summary>
        // ReSharper disable once InconsistentNaming
        public double c
        {
            get
            {
                return _c;
            }
        }

        /// <summary>
        /// Фокус 1
        /// </summary>
        public Vector2D F1
        {
            get { return _f1; }
        }

        /// <summary>
        /// Фокус 2
        /// </summary>
        public Vector2D F2
        {
            get { return _f2; }
        }

        /// <summary>
        /// Получить точку на эллипсе по углу относительно положительного направления оси X
        /// </summary>
        /// <param name="angleInRadians"></param>
        /// <returns></returns>
        public Vector2D GetPointByAngle(double angleInRadians)
        {
            var x = System.Math.Round(a * System.Math.Cos(angleInRadians), 10);
            var y = System.Math.Round(b * System.Math.Sin(angleInRadians), 10);
            if (!_isXBigHalfAxis)
            {
                y = System.Math.Round(a * System.Math.Cos(angleInRadians), 10);
                x = System.Math.Round(b * System.Math.Sin(angleInRadians), 10);
            }
            x += Bounds.Centre.X;
            y += Bounds.Centre.Y;
            return new Vector2D(x, y);
        }

        /// <summary>
        /// Получить значение радиуса по углу относительно положительного направления оси X
        /// </summary>
        /// <param name="angleInRadians"></param>
        /// <returns></returns>
        public double GetRadius(double angleInRadians)
        {
            var res = b / (System.Math.Sqrt(1 - Eccentricity * Eccentricity * System.Math.Cos(angleInRadians) * System.Math.Cos(angleInRadians)));
            return res;
        }

        public double RadiusX
        {
            get { return _radiusX; }
            set
            {
                _radiusX = value;
                UpdateBounds();
            }
        }

        public double RadiusY
        {
            get { return _radiusY; }
            set
            {
                _radiusY = value;
                UpdateBounds();
            }
        }

        public double StartAngle
        {
            get { return _startAngle; }
            set
            {
                _startAngle = value;
                UpdateBounds();
            }
        }

        public double SweepAngle
        {
            get { return _sweepAngle; }
            set
            {
                _sweepAngle = value;
                UpdateBounds();
            }
        }

        public Rect Bounds
        {
            get { return _bounds; }
        }

        protected void UpdateBounds()
        {
            _bounds = new Rect(Location.X - RadiusX, Location.Y - RadiusY, Location.X + RadiusX, Location.Y + RadiusY);

            _isXBigHalfAxis = true;


            _a = RadiusX;
            if ((float)RadiusY > (float)RadiusX)
            {
                _a = RadiusY;
                _isXBigHalfAxis = false;
            }

            _b = RadiusX;
            if (RadiusY < RadiusX) _b = RadiusY;

            //var res = ((b * b) / (a * a));
            //res = 1 - res;
            //res = System.Math.Sqrt(res);

            _e = System.Math.Sqrt(1 - ((b * b) / (a * a)));
            _c = Eccentricity * a;

            if (_isXBigHalfAxis)
            {
                //Большая полуось лежит на оси X
                _f1 = new Vector2D(Bounds.Centre.X - c, Bounds.Centre.Y);
                _f2 = new Vector2D(Bounds.Centre.X + c, Bounds.Centre.Y);
            }
            else
            {
                //Большая полуось лежит на оси Y
                _f1 = new Vector2D(Bounds.Centre.X, Bounds.Centre.Y - c);
                _f2 = new Vector2D(Bounds.Centre.X, Bounds.Centre.Y + c);
            }
        }


        #region Object Overrides

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Ellipse)obj);
        }

        protected bool Equals(Ellipse other)
        {
            return _location.Equals(other._location) && _radiusX.Equals(other._radiusX) && _radiusY.Equals(other._radiusY) && _startAngle.Equals(other._startAngle) && _sweepAngle.Equals(other._sweepAngle);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Location.GetHashCode() * 397) ^ (RadiusX.GetHashCode() * 397) ^ (RadiusY.GetHashCode() * 397) ^ (StartAngle.GetHashCode() * 397) ^ SweepAngle.GetHashCode();
            }
        }

        public override string ToString()
        {
            return "Ellipse: Location = " + Location + " RadiusX = " + RadiusX + " RadiusY = " + RadiusY + " StartAngle = " + StartAngle + " SweepAngle = " + SweepAngle;
        }

        #endregion
    }
}
