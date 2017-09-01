using System;

namespace Flekosoft.Common.Math
{
    /// <summary>
    /// Прямоугольник
    /// </summary>
    public class Rect// : IObservable
    {
        private double _left;
        private double _top;
        private double _right;
        private double _bottom;

        public Rect()
        {
            Left = 0;
            Top = 0;
            Right = 0;
            Bottom = 0;
        }

        public Rect(double left, double top, double right, double bottom)
        {
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
        }

        public Rect(Vector2D leftTop, double width, double height)
        {
            Left = leftTop.X;
            Top = leftTop.Y;
            Right = leftTop.X + width;
            Bottom = leftTop.Y + height;
        }

        //public Rect(Vector2D centr, double width, double height)
        //{
        //    Left = centr.X - width/2;
        //    Top = centr.Y - height / 2;
        //    Right = centr.X + width/2;
        //    Bottom = centr.Y + height/2;
        //}

        public Rect(Rect r)
        {
            if (r == null)
                throw new ArgumentNullException();

            Left = r.Left;
            Top = r.Top;
            Right = r.Right;
            Bottom = r.Bottom;
        }

        #region Properties

        public Vector2D LeftTop
        {
            get { return new Vector2D(Left, Top); }
        }

        public Vector2D LeftBottom
        {
            get { return new Vector2D(Left, Bottom); }
        }

        public Vector2D RightTop
        {
            get { return new Vector2D(Right, Top); }
        }

        public Vector2D RightBottom
        {
            get { return new Vector2D(Right, Bottom); }
        }

        public double Left
        {
            get { return _left; }
            set
            {
                _left = value;
                //NotifyObservers((int)ObservableNotifyTypes.LeftValueChanged, new EventArgs());
            }
        }

        public double Top
        {
            get { return _top; }
            set
            {
                _top = value;
                //NotifyObservers((int)ObservableNotifyTypes.TopValueChanged, new EventArgs());
            }
        }

        public double Right
        {
            get { return _right; }
            set
            {
                _right = value;
                //NotifyObservers((int)ObservableNotifyTypes.RightValueChanged, new EventArgs());
            }
        }

        public double Bottom
        {
            get { return _bottom; }
            set
            {
                _bottom = value;
                //NotifyObservers((int)ObservableNotifyTypes.BottomValueChanged, new EventArgs());
            }
        }

        public double Width
        {
            get { return _right - _left; }
        }

        public double Height
        {
            get { return _bottom - _top; }
        }

        /// <summary>
        /// Геометрический центр прямоугольника
        /// </summary>
        public Vector2D Centre
        {
            get { return new Vector2D((_left + _right) * 0.5f, (_top + _bottom) * 0.5f); }
        }

        /// <summary>
        /// Радиус окружности,вписанной в данный прямоугольник
        /// </summary>
        public double InRadius
        {
            get
            {
                var horizontal = System.Math.Abs(Left - Right);
                var vertical = System.Math.Abs(Top - Bottom);
                var res = horizontal;
                if (res > vertical) res = vertical;

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
                return Centre.Distance(LeftTop);
            }
        }

        public LineSegment2D LeftBorder
        {
            get { return new LineSegment2D(LeftTop, LeftBottom); }
        }

        public LineSegment2D TopBorder
        {
            get { return new LineSegment2D(LeftTop, RightTop); }
        }

        public LineSegment2D RightBorder
        {
            get
            {
                return new LineSegment2D(RightTop, RightBottom);
            }
        }

        public LineSegment2D BottomBorder
        {
            get
            {
                return new LineSegment2D(LeftBottom, RightBottom);
            }

        }

        #endregion

        public Rect Scale(double factor)
        {
            return new Rect(Left * factor, Top * factor, Right * factor, Bottom * factor);
        }


        //public final boolean isEmpty() {
        //    return _left >= _right || _top >= _bottom;
        //}

        //static RectF Empty() {
        //    return new RectF();
        //}

        //@Override
        //public boolean equals(Object o) {
        //    if (this == o)
        //        return true;
        //    if (o == null || getClass() != o.getClass())
        //        return false;

        //    RectF r = (RectF) o;
        //    return _left == r._left && _top == r._top && _right == r._right
        //            && _bottom == r._bottom;
        //}

        //public boolean contains(float x, float y) {
        //    return _left < _right && _top < _bottom // check for empty first
        //            && x >= _left && x <= _right && y >= _top && y <= _bottom;
        //}

        public bool Contains(Vector2D point)
        {
            return _left <= _right
                    && _top <= _bottom // check for empty first
                    && point.X >= _left && point.X <= _right
                    && point.Y >= _top && point.Y <= _bottom;
        }

        //public boolean contains(float left, float top, float right, float bottom) {
        //    // check for empty first
        //    return this._left < this._right && this._top < this._bottom
        //            // now check for containment
        //            && this._left <= left && this._top <= top
        //            && this._right >= right && this._bottom >= bottom;
        //}

        //public boolean contains(RectF r) {
        //    // check for empty first
        //    return this._left < this._right && this._top < this._bottom
        //            // now check for containment
        //            && _left <= r.getLeft() && _top <= r.getTop()
        //            && _right >= r.getRight() && _bottom >= r.getBottom();
        //}

        //public boolean contains(Rect r) {
        //    // check for empty first
        //    return this._left < this._right && this._top < this._bottom
        //            // now check for containment
        //            && _left <= r.getLeft() && _top <= r.getTop()
        //            && _right >= r.getRight() && _bottom >= r.getBottom();
        //}

        //public boolean intersects(float left, float top, float right, float bottom) {
        //    return this._left < right && left < this._right && this._top < bottom
        //            && top < this._bottom;
        //}

        public bool Intersects(Rect rect)
        {
            return _left < rect.Right && rect.Left < _right
                    && _top < rect.Bottom && rect.Top < _bottom;
        }

        // /**
        // * Рассчитать область пересечения данного прямоугольника с другим
        // * 
        // * @param left
        // *            - левая граница другого прямоугольника
        // * @param top
        // *            - верхняя граница другого прямоугольника
        // * @param right
        // *            - правая граница другого прямоугольника
        // * @param bottom
        // *            - нижняя граница другого прямоугольника
        // * @return прямоугольник, содержащию область пересечения. Если
        // *         прямоугольники не пересекаются, то возвращается нулевой
        // *         прямоугольник.
        // */
        //public RectF intersect(float left, float top, float right, float bottom) {
        //    if (!intersects(left, top, right, bottom))
        //        return new RectF();

        //    RectF res = new RectF();

        //    if (this.getLeft() < left)
        //        res.setLeft(left);
        //    else
        //        res.setLeft(this.getLeft());

        //    if (this.getTop() < top)
        //        res.setTop(top);
        //    else
        //        res.setTop(this.getTop());

        //    if (this.getRight() > right)
        //        res.setRight(right);
        //    else
        //        res.setRight(this.getRight());

        //    if (this.getBottom() > bottom)
        //        res.setBottom(bottom);
        //    else
        //        res.setBottom(this.getBottom());

        //    return res;
        //}

        //public RectF intersect(RectF rect) {
        //    return intersect(rect.getLeft(), rect.getTop(), rect.getRight(),
        //            rect.getBottom());
        //}

        //public RectF intersect(Rect rect) {
        //    return intersect(rect.getLeft(), rect.getTop(), rect.getRight(),
        //            rect.getBottom());
        //}

        //public void union(RectF r) {
        //    union(r.getLeft(), r.getTop(), r.getRight(), r.getBottom());
        //}

        public void Union(Rect r)
        {
            Union(r.Left, r.Top, r.Right, r.Bottom);
        }

        public void Union(double left, double top, double right, double bottom)
        {
            if ((left <= right) && (top <= bottom))
            {
                if ((_left < _right) && (_top < _bottom))
                {
                    if (_left > left)
                        Left = left;
                    if (_top > top)
                        Top = top;
                    if (_right < right)
                        Right = right;
                    if (_bottom < bottom)
                        Bottom = bottom;
                }
                else
                {
                    Left = left;
                    Top = top;
                    Right = right;
                    Bottom = bottom;
                }
            }
        }

        //public void union(float x, float y) {
        //    if (x < _left) {
        //        Left = x;
        //    } else if (x > _right) {
        //        _right = x;
        //    }
        //    if (y < _top) {
        //        _top = y;
        //    } else if (y > _bottom) {
        //        _bottom = y;
        //    }
        //}

        public void Union(Vector2D value)
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
        }

        #region Object Overrides

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Rect)obj);
        }

        protected bool Equals(Rect other)
        {
            return _left.Equals(other._left) && _right.Equals(other._right) && _top.Equals(other._top) && _bottom.Equals(other._bottom);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Left.GetHashCode() * 397) ^ (Right.GetHashCode() * 397) ^ (Top.GetHashCode() * 397) ^ Bottom.GetHashCode();
            }
        }

        public override string ToString()
        {
            return "Rect: Left = " + Left + " Right = " + Right + " Top = " + Top + " Bottom = " + Bottom;
        }

        #endregion

        //#region Observable

        //public enum ObservableNotifyTypes
        //{
        //    None = 0,
        //    LeftValueChanged = 1,
        //    TopValueChanged = 2,
        //    RightValueChanged = 3,
        //    BottomValueChanged = 4
        //}

        ///// <summary>
        ///// Список слушателей событий этого класа
        ///// </summary>
        //private readonly List<IObserver> _observersList = new List<IObserver>();

        //public void AddObserver(IObserver o)
        //{
        //    _observersList.Add(o);
        //}

        //public void RemoveObserver(IObserver o)
        //{
        //    _observersList.Remove(o);
        //}

        //public void NotifyObservers(int type, EventArgs eventArgs)
        //{
        //    foreach (var observer in _observersList)
        //    {
        //        observer.OnObservableNotification(new ObservervableNotification(this, type, eventArgs));
        //    }
        //}

        //#endregion
    }
}
