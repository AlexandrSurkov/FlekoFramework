//using Flekosoft.Common.Observable;

namespace Flekosoft.Common.Math
{
    public class Vector3D// : IObservable
    {
        public static Vector3D XAxis
        {
            get
            {
                var value = new Vector3D(1, 0, 0);
                return value;

            }
        }

        public static Vector3D YAxis
        {
            get
            {
                var value = new Vector3D(0, 1, 0);
                return value;

            }
        }

        public static Vector3D ZAxis
        {
            get
            {
                var value = new Vector3D(0, 0, 1);
                return value;

            }
        }


        private double _x;
        private double _y;
        private double _z;

        public Vector3D()
        {
            _x = 0.0;
            _y = 0.0;
            _z = 0.0;
        }

        public Vector3D(double x, double y, double z)
        {
            _x = x;
            _y = y;
            _z = z;
        }

        public Vector3D(Vector3D vector)
        {
            _x = vector.X;
            _y = vector.Y;
            _z = vector.Z;
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
                    //NotifyObservers((int)ObservableNotifyTypes.XValueChanged, new EventArgs());
                }
            }
        }

        public double Z
        {
            get { return _z; }
            set
            {
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (_z != value)
                {
                    _z = value;
                    //NotifyObservers((int)ObservableNotifyTypes.ZValueChanged, new EventArgs());
                }
            }
        }

        public Vector2D XY
        {
            get { return new Vector2D(X, Y); }
        }

        public Vector2D XZ
        {
            get { return new Vector2D(X, Z); }
        }

        public Vector2D ZY
        {
            get { return new Vector2D(Z, Y); }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Нолевой ли вектор
        /// </summary>
        /// <returns>Thre если x и y оба равны 0 иначе false</returns>
        public bool IsZero()
        {
            return System.Math.Abs((_x * _x + _y * _y + _z * _z)) <= double.Epsilon;
        }

        /// <summary>
        /// Делает вектор нулевым
        /// </summary>
        public void Zero()
        {
            _x = 0.0;
            _y = 0.0;
            _z = 0.0;
        }

        /// <summary>
        /// Длина вектора
        /// </summary>
        /// <returns>Возвращает длину вектора</returns>
        public double Length()
        {
            return System.Math.Sqrt(_x * _x + _y * _y + _z * _z);
        }

        /// <summary>
        /// Квадрат длины вектора.
        /// </summary>
        /// <returns>Возвращает квадрат длены вектора</returns>
        public double LengthSq()
        {
            return (_x * _x + _y * _y + _z * _z);
        }

        /// <summary>
        /// Нормализовать вектор
        /// </summary>
        public void Normalize()
        {
            double vectorLength = Length();

            if (vectorLength > double.Epsilon)
            {
                _x /= vectorLength;
                _y /= vectorLength;
                _z /= vectorLength;
            }
        }

        /// <summary>
        /// Скалярное произведение
        /// </summary>
        /// <param name="vector">На который нужно умножить</param>
        /// <returns></returns>
        public double DotProduct(Vector3D vector)
        {
            return X * vector.X + Y * vector.Y + Z * vector.Z;
        }


        /// <summary>
        /// Векторное произведение
        /// </summary>
        /// <param name="vector">На который нужно умножить</param>
        /// <returns></returns>
        public Vector3D CrossProduct(Vector3D vector)
        {
            return new Vector3D(Y * vector.Z - vector.Y * Z, Z * vector.X - vector.Z * X, X * vector.Y - vector.X * Y);
        }

        //#region Observable

        //public enum ObservableNotifyTypes
        //{
        //    None = 0,
        //    XValueChanged = 1,
        //    YValueChanged = 2,
        //    ZValueChanged = 3,
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
                Z *= max;
            }
        }

        /// <summary>
        /// Подсчитывает расстояние между векторами
        /// </summary>
        /// <param name="vector"></param>
        /// <returns></returns>
        public double Distance(Vector3D vector)
        {
            double ySeparation = vector.Y - Y;
            double xSeparation = vector.X - X;
            double zSeparation = vector.Z - Z;

            return System.Math.Sqrt(ySeparation * ySeparation + xSeparation * xSeparation + zSeparation * zSeparation);
        }

        /// <summary>
        /// Подсчитывает квадрат расстояния между векторами
        /// </summary>
        /// <param name="vector"></param>
        /// <returns></returns>
        public double DistanceSq(Vector3D vector)
        {
            double ySeparation = vector.Y - Y;
            double xSeparation = vector.X - X;
            double zSeparation = vector.Z - Z;

            return ySeparation * ySeparation + xSeparation * xSeparation + zSeparation * zSeparation;
        }


        /// <summary>
        /// Отражает данный вектор относительно поверхности, заданной нормалью
        /// </summary>
        /// <param name="norm">Нормаль поверхности для отражения</param>
        public void Reflect(Vector3D norm)
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
        public void Reflect(Vector3D norm, double factor)
        {
            var vec = 2.0 * DotProduct(norm) * Invert(norm);
            X += vec.X;
            Y += vec.Y;
            Z += vec.Z;
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
            Z = v.Z;
        }

        /// <summary>
        /// Returns inverted vector
        /// </summary>
        /// <returns></returns>
        public static Vector3D Invert(Vector3D value)
        {
            return new Vector3D(-value.X, -value.Y, -value.Z);
        }


        public void RotateAroundOriginXY(double angRad)
        {
            var xy = XY;
            Transrormations.Vector2DRotateAroundOrigin(xy, angRad);
            X = xy.X;
            Y = xy.Y;
        }

        public void RotateAroundOriginXZ(double angRad)
        {
            var xz = XZ;
            Transrormations.Vector2DRotateAroundOrigin(xz, angRad);
            X = xz.X;
            Z = xz.Y;
        }

        public void RotateAroundOriginZY(double angRad)
        {
            var zy = ZY;
            Transrormations.Vector2DRotateAroundOrigin(zy, angRad);
            Z = zy.X;
            Y = zy.Y;
        }

        public void Rotate(Vector3D axis, double ang)
        {
            Transrormations.Vector3DRotate(this, axis, ang);
        }


        public void Rotate(Quaternion quaternion)
        {
            var v = Rotate(this, quaternion);
            X = v.X;
            Y = v.Y;
            Z = v.Z;
        }

        public static Vector3D Rotate(Vector3D vector, Quaternion quaternion)
        {
            var t = Quaternion.Multiply(quaternion, vector);
            t = Quaternion.Multiply(t, Quaternion.Invert(quaternion));

            return new Vector3D(t.X, t.Y, t.Z);
        }

        #endregion

        #region Object Overrides

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Vector3D)obj);
        }

        protected bool Equals(Vector3D other)
        {
            return _x.Equals(other._x) && _y.Equals(other._y) && _z.Equals(other._z);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (X.GetHashCode() * 397) ^ (Y.GetHashCode() * 397) ^ Z.GetHashCode();
            }
        }

        public override string ToString()
        {
            return "Vector3D: X = " + X + " Y = " + Y + " Z = " + Z;
        }

        #endregion

        #region Operators
        public static Vector3D operator +(Vector3D lhs, Vector3D rhs)
        {
            var result = new Vector3D(lhs);
            result.X += rhs.X;
            result.Y += rhs.Y;
            result.Z += rhs.Z;

            return result;
        }

        public static Vector3D operator *(Vector3D lhs, double rhs)
        {
            var result = new Vector3D(lhs);
            result.X *= rhs;
            result.Y *= rhs;
            result.Z *= rhs;
            return result;
        }

        public static Vector3D operator *(double lhs, Vector3D rhs)
        {
            var result = new Vector3D(rhs);
            result.X *= lhs;
            result.Y *= lhs;
            result.Z *= lhs;
            return result;
        }

        public static Vector3D operator -(Vector3D lhs, Vector3D rhs)
        {
            var result = new Vector3D(lhs);
            result.X -= rhs.X;
            result.Y -= rhs.Y;
            result.Z -= rhs.Z;

            return result;
        }

        public static Vector3D operator /(Vector3D lhs, double val)
        {
            var result = new Vector3D(lhs);
            result.X /= val;
            result.Y /= val;
            result.Z /= val;

            return result;
        }

        public static bool operator ==(Vector3D lhs, Vector3D rhs)
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

        public static bool operator !=(Vector3D lhs, Vector3D rhs)
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
        public static Vector3D Normalize(Vector3D vector)
        {
            var vec = new Vector3D(vector);

            double vectorLength = vec.Length();

            if (vectorLength > double.Epsilon)
            {
                vec.X /= vectorLength;
                vec.Y /= vectorLength;
                vec.Z /= vectorLength;
            }

            return vec;
        }


        /// <summary>
        /// Расстояние между векторами
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        public static double Distance(Vector3D v1, Vector3D v2)
        {
            double ySeparation = v2.Y - v1.Y;
            double xSeparation = v2.X - v1.X;
            double zSeparation = v2.Z - v1.Z;

            return System.Math.Sqrt(ySeparation * ySeparation + xSeparation * xSeparation + zSeparation * zSeparation);
        }

        /// <summary>
        /// Квадрат расстояния между векторами
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        public static double DistanceSq(Vector3D v1, Vector3D v2)
        {
            double ySeparation = v2.Y - v1.Y;
            double xSeparation = v2.X - v1.X;
            double zSeparation = v2.Z - v1.Z;

            return ySeparation * ySeparation + xSeparation * xSeparation + zSeparation * zSeparation;
        }

        /// <summary>
        /// Длина вектора
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public static double Length(Vector3D v)
        {
            return System.Math.Sqrt(v.X * v.X + v.Y * v.Y + v.Z * v.Z);
        }

        /// <summary>
        /// Квадрат длины вектора
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public static double LengthSq(Vector3D v)
        {
            return v.X * v.X + v.Y * v.Y + v.Z * v.Z;
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
