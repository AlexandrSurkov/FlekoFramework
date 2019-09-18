namespace Flekosoft.Common.Math
{
    public class Box
    {
        /// <summary>
        /// Левая граница
        /// </summary>
        private double _left;
        /// <summary>
        /// Верхняя граница
        /// </summary>
        private double _top;
        /// <summary>
        /// Правая граница
        /// </summary>
        private double _right;
        /// <summary>
        /// Нижняя граница
        /// </summary>
        private double _bottom;
        /// <summary>
        /// Ближняя граница
        /// </summary>
        private double _front;
        /// <summary>
        /// Дальняя  граница
        /// </summary>
        private double _back;

        public Box(double left, double top, double right, double bottom, double near, double far)
        {
            Left = left;
            Top = top;
            Right = right;
            Near = near;
            Far = far;
            Bottom = bottom;
        }

        #region Properties
        /// <summary>
        /// Левая граница
        /// </summary>
        public double Left
        {
            get { return _left; }
            set { _left = value; }
        }

        /// <summary>
        /// Верхняя граница
        /// </summary>
        public double Top
        {
            get { return _top; }
            set { _top = value; }
        }

        /// <summary>
        /// Правая граница
        /// </summary>
        public double Right
        {
            get { return _right; }
            set { _right = value; }
        }

        /// <summary>
        /// Ближняя
        /// </summary>
        public double Near
        {
            get { return _front; }
            set { _front = value; }
        }

        /// <summary>
        /// Дальняя  граница
        /// </summary>
        public double Far
        {
            get { return _back; }
            set { _back = value; }
        }

        /// <summary>
        /// Нижняя граница
        /// </summary>
        public double Bottom
        {
            get { return _bottom; }
            set { _bottom = value; }
        }

        public Vector3D Centre
        {
            get { return new Vector3D((_left + _right) * 0.5f, (_top + _bottom) * 0.5f, (_front + _back) * 0.5f); }
        }

        public double Width
        {
            get { return _bottom - _top; }
        }

        public double Length
        {
            get { return _right - _left; }
        }

        public double Height
        {
            get { return _front - _back; }
        }


        public Vector3D LeftTopNear
        {
            get { return new Vector3D(Left, Top, Near); }
        }

        public Vector3D LeftTopFar
        {
            get { return new Vector3D(Left, Top, Far); }
        }

        public Vector3D RightTopNear
        {
            get { return new Vector3D(Right, Top, Near); }
        }

        public Vector3D RightTopFar
        {
            get { return new Vector3D(Right, Top, Far); }
        }

        public Vector3D LeftBottomNear
        {
            get { return new Vector3D(Left, Bottom, Near); }
        }

        public Vector3D LeftBottomFar
        {
            get { return new Vector3D(Left, Bottom, Far); }
        }

        public Vector3D RightBottomNear
        {
            get { return new Vector3D(Right, Bottom, Near); }
        }

        public Vector3D RightBottomFar
        {
            get { return new Vector3D(Right, Bottom, Far); }
        }


        public Vector3D LeftCenter
        {
            get { return new Vector3D(_left, (_top + _bottom) * 0.5f, (_front + _back) * 0.5f); }
        }

        public Vector3D RightCenter
        {
            get { return new Vector3D(_right, (_top + _bottom) * 0.5f, (_front + _back) * 0.5f); }
        }

        public Vector3D TopCenter
        {
            get { return new Vector3D((_left + _right) * 0.5f, _top, (_front + _back) * 0.5f); }
        }

        public Vector3D BottomCenter
        {
            get { return new Vector3D((_left + _right) * 0.5f, _bottom, (_front + _back) * 0.5f); }
        }

        public Vector3D FarCenter
        {
            get { return new Vector3D((_left + _right) * 0.5f, (_top + _bottom) * 0.5f, _back); }
        }

        public Vector3D NearCenter
        {
            get { return new Vector3D((_left + _right) * 0.5f, (_top + _bottom) * 0.5f, _front); }
        }

        public Plane LeftPlane
        {
            get { return new Plane(LeftTopNear, LeftBottomNear, LeftBottomFar); }
        }

        public Plane RightPlane
        {
            get { return new Plane(RightTopNear, RightBottomNear, RightBottomFar); }
        }

        public Plane TopPlane
        {
            get { return new Plane(LeftTopNear, RightTopNear, LeftTopFar); }
        }

        public Plane BottomPlane
        {
            get { return new Plane(LeftBottomNear, RightBottomNear, LeftBottomFar); }
        }

        public Plane NearPlane
        {
            get { return new Plane(LeftBottomNear, RightBottomNear, LeftTopNear); }
        }

        public Plane FarPlane
        {
            get { return new Plane(LeftBottomFar, RightBottomFar, LeftTopFar); }
        }


        /// <summary>
        /// Радиус окружности,вписанной в данный прямоугольник
        /// </summary>
        public double InRadius
        {
            get
            {
                var horizontalLen = System.Math.Abs(Left - Right);
                var horizontalWidth = System.Math.Abs(Left - Right);
                var vertical = System.Math.Abs(Top - Bottom);
                var res = horizontalLen;
                if (res > vertical) res = vertical;
                if (res > horizontalWidth) res = horizontalWidth;

                res = res / 2;
                return res;
            }
        }

        /// <summary>
        /// Радиус окружности, описывающий данный прямоугольник
        /// </summary>
        public double OutRadius
        {
            get
            {
                return Centre.Distance(LeftTopNear);
            }
        }

        public bool Contains(Vector3D point)
        {
            return _left <= _right
                   && _top <= _bottom
                   && Far <= Near // check for empty first
                   && point.X >= _left && point.X <= _right
                   && point.Y >= _top && point.Y <= _bottom
                   && point.Z >= Far && point.Z <= Near;
        }

        /// <summary>
        /// Плоскость по осям XY
        /// </summary>
        public Rect XY
        {
            get
            {
                return new Rect(Left, Top, Right, Bottom);
            }
        }

        /// <summary>
        /// Плоскость по осям XZ
        /// </summary>
        public Rect XZ
        {
            get
            {
                return new Rect(Left, Far, Right, Near);
            }
        }

        /// <summary>
        /// Плоскость по осям ZY
        /// </summary>
        public Rect ZY
        {
            get
            {
                return new Rect(Far, Top, Near, Bottom);
            }
        }



        #endregion

        public void Union(Vector3D value)
        {
            if (value.X < _left)
            {
                Left = value.X;
            }
            else if (value.X > _right)
            {
                Right = value.X;
            }
            if (value.Y < _top)
            {
                Top = value.Y;
            }
            else if (value.Y > _bottom)
            {
                Bottom = value.Y;
            }
            if (value.Z < Far)
            {
                Far = value.Z;
            }
            else if (value.Z > Near)
            {
                Near = value.Z;
            }
        }
    }
}
