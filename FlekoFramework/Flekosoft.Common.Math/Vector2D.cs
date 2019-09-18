//using System.Collections.Generic;
//using Flekosoft.Common.Observable;

namespace Flekosoft.Common.Math
{
    /// <summary>
    /// Направление одного вектора относительно другого
    /// </summary>
    public enum VectorSign
    {
        None = 0,
        /// <summary>
        /// По часоой стрелке
        /// </summary>
        Clockwise = 1,
        /// <summary>
        /// Против часовой стрелки
        /// </summary>
        Antickockwise = -1,
    }
    public class Vector2D //: IObservable
    {
        private double _x;
        private double _y;

        public Vector2D()
        {
            _x = 0.0;
            _y = 0.0;
        }

        public Vector2D(double x, double y)
        {
            _x = x;
            _y = y;
        }

        public Vector2D(Vector2D vector)
        {
            _x = vector.X;
            _y = vector.Y;
        }

        #region properties

        public double X
        {
            get { return _x; }
            set
            {
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (_x != value)
                {
                    _x = value;
                    //NotifyObservers((int)ObservableNotifyTypes.XValueChanged, new EventArgs());
                }
            }
        }

        public double Y
        {
            get { return _y; }
            set
            {
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (_y != value)
                {
                    _y = value;
                    //NotifyObservers((int) ObservableNotifyTypes.XValueChanged, new EventArgs());
                }
            }
        }

        /// <summary>
        /// Вовращает угол в радианах относительно положительного направления оси Х
        /// </summary>
        public double Angle
        {
            get
            {
                var res = System.Math.Acos(X / Length());
                if (Y >= 0)
                    return res;

                return 2 * System.Math.PI - res;
            }
        }



        #endregion

        #region Methods

        /// <summary>
        /// Нолевой ли вектор
        /// </summary>
        /// <returns>Thre если x и y оба равны 0 иначе false</returns>
        public bool IsZero()
        {
            return System.Math.Abs((X * X + Y * Y)) <= double.Epsilon;
        }

        /// <summary>
        /// Делает вектор нулевым
        /// </summary>
        public void Zero() { X = 0.0; Y = 0.0; }

        /// <summary>
        /// Длина вектора
        /// </summary>
        /// <returns>Возвращает длину вектора</returns>
        public double Length()
        {
            return System.Math.Sqrt(X * X + Y * Y);
        }

        /// <summary>
        /// Квадрат длины вектора.
        /// </summary>
        /// <returns>Возвращает квадрат длены вектора</returns>
        public double LengthSq()
        {
            return (X * X + Y * Y);
        }

        /// <summary>
        /// Нормализовать вектор
        /// </summary>
        public void Normalize()
        {
            double vectorLength = Length();

            if (vectorLength > double.Epsilon)
            {
                X /= vectorLength;
                Y /= vectorLength;
            }
        }

        /// <summary>
        /// Скалярное произведение
        /// </summary>
        /// <param name="vector">На который нужно умножить</param>
        /// <returns></returns>
        public double DotProduct(Vector2D vector)
        {
            return X * vector.X + Y * vector.Y;
        }

        /// <summary>
        /// Возвращает расположение вектора, учитывая что ось Y направлена вниз
        /// returns positive if v2 is clockwise of this vector,
        /// negative if anticlockwise (assuming the Y axis is pointing down,
        /// X axis to right like a Window app)
        /// </summary>
        /// <param name="vector">Вектор, расположение которого нужно определить</param>
        /// <returns>Clockwise = 1, если по часовой стрелке иначе Antickockwise =-1 </returns>
        public VectorSign Sign(Vector2D vector)
        {
            if (Y * vector.X > X * vector.Y)
            {
                return VectorSign.Antickockwise;
            }
            return VectorSign.Clockwise;
        }

        /// <summary>
        /// Возвращает вектор, перпендикулярный данному
        /// </summary>
        /// <returns></returns>
        public Vector2D Perp()
        {
            return new Vector2D(-Y, X);
        }

        /// <summary>
        /// Adjusts x and y so that the length of the vector does not exceed max
        /// </summary>
        /// <param name="max"></param>
        public void Truncate(double max)
        {
            if (Length() > max)
            {
                Normalize();

                X *= max;
                Y *= max;
            }
        }

        /// <summary>
        /// Рассчитывает угол между векторами
        /// </summary>
        /// <returns></returns>
        public double AngleTo(Vector2D vector)
        {
            //double res = System.Math.Acos(Vector2D.Normalize(this)*Vector2D.Normalize(vector));
            double res = Angle - vector.Angle;
            return res;
        }

        /// <summary>
        /// Подсчитывает расстояние между векторами
        /// </summary>
        /// <param name="vector"></param>
        /// <returns></returns>
        public double Distance(Vector2D vector)
        {
            double ySeparation = vector.Y - Y;
            double xSeparation = vector.X - X;

            return System.Math.Sqrt(ySeparation * ySeparation + xSeparation * xSeparation);
        }

        /// <summary>
        /// Подсчитывает квадрат расстояния между векторами
        /// </summary>
        /// <param name="vector"></param>
        /// <returns></returns>
        public double DistanceSq(Vector2D vector)
        {
            double ySeparation = vector.Y - Y;
            double xSeparation = vector.X - X;

            return ySeparation * ySeparation + xSeparation * xSeparation;
        }


        /// <summary>
        /// Отражает данный вектор относительно поверхности, заданной нормалью
        /// </summary>
        /// <param name="norm">Нормаль поверхности для отражения</param>
        public void Reflect(Vector2D norm)
        {
            //var vec = 2.0 * DotProduct(norm) * norm.GetReverse();
            //X += vec.X;
            //Y += vec.Y;
            Reflect(norm, 1);
        }

        /// <summary>
        /// Отражает данный вектор относительно поверхности, заданной нормалью и изменяет длину вектора на заданную величину
        /// </summary>
        /// <param name="norm">Нормаль поверхности для отражения</param>
        /// <param name="factor">коефициент изменения длины вектора</param>
        public void Reflect(Vector2D norm, double factor)
        {
            var vec = 2.0 * DotProduct(norm) * Invert(norm);
            X += vec.X;
            Y += vec.Y;
            var len = Length();
            len *= factor;
            Truncate(len);
        }

        /// <summary>
        /// Inverts current vector
        /// </summary>
        public void Invert()
        {
            var v = Invert(this);
            X = v.X;
            Y = v.Y;
        }

        /// <summary>
        /// Returns inverted vector
        /// </summary>
        /// <returns></returns>
        public static Vector2D Invert(Vector2D value)
        {
            return new Vector2D(-value.X, -value.Y);
        }

        public void RotateAroundOrigin(double ang)
        {
            Transrormations.Vector2DRotateAroundOrigin(this, ang);
        }



        #endregion

        #region Object Overrides

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Vector2D)obj);
        }

        protected bool Equals(Vector2D other)
        {
            return X.Equals(other.X) && Y.Equals(other.Y);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (X.GetHashCode() * 397) ^ Y.GetHashCode();
            }
        }

        public override string ToString()
        {
            return "Vector2D: X = " + X + " Y = " + Y;
        }

        #endregion

        //#region Observable

        //public enum ObservableNotifyTypes
        //{
        //    None = 0,
        //    XValueChanged = 1,
        //    YValueChanged = 2
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

        #region Operators
        public static Vector2D operator +(Vector2D lhs, Vector2D rhs)
        {
            var result = new Vector2D(lhs);
            result.X += rhs.X;
            result.Y += rhs.Y;

            return result;
        }

        public static Vector2D operator *(Vector2D lhs, double rhs)
        {
            var result = new Vector2D(lhs);
            result.X *= rhs;
            result.Y *= rhs;
            return result;
        }

        public static double operator *(Vector2D lhs, Vector2D rhs)
        {
            var result = lhs.X * rhs.X + lhs.Y * rhs.Y;
            return result;
        }

        public static Vector2D operator *(double lhs, Vector2D rhs)
        {
            var result = new Vector2D(rhs);
            result.X *= lhs;
            result.Y *= lhs;
            return result;
        }

        public static Vector2D operator -(Vector2D lhs, Vector2D rhs)
        {
            var result = new Vector2D(lhs);
            result.X -= rhs.X;
            result.Y -= rhs.Y;

            return result;
        }

        public static Vector2D operator /(Vector2D lhs, double val)
        {
            var result = new Vector2D(lhs);
            result.X /= val;
            result.Y /= val;

            return result;
        }

        public static bool operator ==(Vector2D lhs, Vector2D rhs)
        {
            // If both are null, or both are same instance, return true.
            if (ReferenceEquals(lhs, rhs))
            {
                return true;
            }

            // If one is null, but not both, return false.
            if (((object)lhs == null) || ((object)rhs == null))
            {
                return false;
            }

            return lhs.Equals(rhs);
        }

        public static bool operator !=(Vector2D lhs, Vector2D rhs)
        {
            return !(lhs == rhs);
        }

        #endregion

        #region static members

        /// <summary>
        /// Возвращает нормализованный вектор
        /// </summary>
        /// <param name="vector"></param>
        /// <returns></returns>
        public static Vector2D Normalize(Vector2D vector)
        {
            var vec = new Vector2D(vector);

            double vectorLength = vec.Length();

            if (vectorLength > double.Epsilon)
            {
                vec.X /= vectorLength;
                vec.Y /= vectorLength;
            }

            return vec;
        }


        /// <summary>
        /// Расстояние между векторами
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        public static double Distance(Vector2D v1, Vector2D v2)
        {
            double ySeparation = v2.Y - v1.Y;
            double xSeparation = v2.X - v1.X;

            return System.Math.Sqrt(ySeparation * ySeparation + xSeparation * xSeparation);
        }

        /// <summary>
        /// Квадрат расстояния между векторами
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        public static double DistanceSq(Vector2D v1, Vector2D v2)
        {
            double ySeparation = v2.Y - v1.Y;
            double xSeparation = v2.X - v1.X;

            return ySeparation * ySeparation + xSeparation * xSeparation;
        }

        /// <summary>
        /// Длина вектора
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public static double Length(Vector2D v)
        {
            return System.Math.Sqrt(v.X * v.X + v.Y * v.Y);
        }

        /// <summary>
        /// Квадрат длины вектора
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public static double LengthSq(Vector2D v)
        {
            return v.X * v.X + v.Y * v.Y;
        }


        //static Vector2D POINTStoVector(const POINTS& p)
        //{
        //  return Vector2D(p.x, p.y);
        //}

        //static Vector2D POINTtoVector(const POINT& p)
        //{
        //  return Vector2D((double)p.x, (double)p.y);
        //}

        //static POINTS VectorToPOINTS(const Vector2D& v)
        //{
        //  POINTS p;
        //  p.x = (short)v.x;
        //  p.y = (short)v.y;

        //  return p;
        //}

        //static POINT VectorToPOINT(const Vector2D& v)
        //{
        //  POINT p;
        //  p.x = (long)v.x;
        //  p.y = (long)v.y;

        //  return p;
        //}


        //        //treats a window as a toroid
        //inline void WrapAround(Vector2D &pos, int MaxX, int MaxY)
        //{
        //  if (pos.x > MaxX) {pos.x = 0.0;}

        //  if (pos.x < 0)    {pos.x = (double)MaxX;}

        //  if (pos.y < 0)    {pos.y = (double)MaxY;}

        //  if (pos.y > MaxY) {pos.y = 0.0;}
        //}

        ////returns true if the point p is not inside the region defined by top_left
        ////and bot_rgt
        //inline bool NotInsideRegion(Vector2D p,
        //                            Vector2D top_left,
        //                            Vector2D bot_rgt)
        //{
        //  return (p.x < top_left.x) || (p.x > bot_rgt.x) || 
        //         (p.y < top_left.y) || (p.y > bot_rgt.y);
        //}

        //inline bool InsideRegion(Vector2D p,
        //                         Vector2D top_left,
        //                         Vector2D bot_rgt)
        //{
        //  return !((p.x < top_left.x) || (p.x > bot_rgt.x) || 
        //         (p.y < top_left.y) || (p.y > bot_rgt.y));
        //}

        //inline bool InsideRegion(Vector2D p, int left, int top, int right, int bottom)
        //{
        //  return !( (p.x < left) || (p.x > right) || (p.y < top) || (p.y > bottom) );
        //}

        ////------------------ isSecondInFOVOfFirst -------------------------------------
        ////
        ////  returns true if the target position is in the field of view of the entity
        ////  positioned at posFirst facing in facingFirst
        ////-----------------------------------------------------------------------------
        //inline bool isSecondInFOVOfFirst(Vector2D posFirst,
        //                                 Vector2D facingFirst,
        //                                 Vector2D posSecond,
        //                                 double    fov)
        //{
        //  Vector2D toTarget = Vec2DNormalize(posSecond - posFirst);

        //  return facingFirst.Dot(toTarget) >= cos(fov/2.0);
        //}
        #endregion
    }
}
