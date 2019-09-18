namespace Flekosoft.Common.Math
{
    public class Quaternion
    {
        public Quaternion()
        {
            W = 0;
            X = 0;
            Y = 0;
            Z = 0;
        }

        public Quaternion(Quaternion quaternion)
        {
            W = quaternion.W;
            X = quaternion.X;
            Y = quaternion.Y;
            Z = quaternion.Z;
        }

        public Quaternion(double w, double x, double y, double z)
        {
            W = w;
            X = x;
            Y = y;
            Z = z;
        }

        #region Methods

        public void SetFromAxisAngleDegrees(double angelDegrees, Vector3D vector)
        {
            SetFromAxisAngleRadian(Utils.ToRadian(angelDegrees), vector);
        }

        public void SetFromAxisAngleRadian(double angelRadian, Vector3D vector)
        {
            var vec = new Vector3D(vector);
            vec.Normalize();

            var vSin = System.Math.Sin(angelRadian / 2);
            X = vec.X * vSin;
            Y = vec.Y * vSin;
            Z = vec.Z * vSin;
            var vCos = System.Math.Cos(angelRadian / 2);
            W = vCos;
        }

        public void SetFromEulerDegrees(double angelX, double angelY, double angelZ)
        {
            SetFromEulerRadian(Utils.ToRadian(angelX), Utils.ToRadian(angelY), Utils.ToRadian(angelZ));
        }

        public void SetFromEulerRadian(double angelX, double angelY, double angelZ)
        {
            var qX = new Quaternion();
            var qY = new Quaternion();
            var qZ = new Quaternion();

            qX.SetFromAxisAngleRadian(angelX, Vector3D.XAxis);
            qY.SetFromAxisAngleRadian(angelY, Vector3D.YAxis);
            qZ.SetFromAxisAngleRadian(angelZ, Vector3D.ZAxis);

            var res = Multiply(qX, qY);
            res.Multiply(qZ);

            W = res.W;
            X = res.X;
            Y = res.Y;
            Z = res.Z;
        }

        public double Length()
        {
            return System.Math.Sqrt(LengthSq());
        }

        public double LengthSq()
        {
            return W * W + X * X + Y * Y + Z * Z;
        }

        public void Multiply(double value)
        {
            var q = Multiply(this, value);
            W = q.W;
            X = q.X;
            Y = q.Y;
            Z = q.Z;
        }

        public static Quaternion Multiply(Quaternion value1, double value2)
        {
            var res = new Quaternion(value1);
            res.X *= value2;
            res.Y *= value2;
            res.Z *= value2;
            res.W *= value2;

            return res;
        }

        public void Normalize()
        {
            var q = Normalize(this);
            W = q.W;
            X = q.X;
            Y = q.Y;
            Z = q.Z;
        }

        public static Quaternion Normalize(Quaternion value1)
        {
            var res = new Quaternion(value1);
            var len = res.Length();
            res.Multiply(1 / len);
            return res;
        }

        public void Invert()
        {
            var q = Invert(this);
            W = q.W;
            X = q.X;
            Y = q.Y;
            Z = q.Z;
        }

        public static Quaternion Invert(Quaternion value1)
        {
            var res = new Quaternion(value1);
            res.X = -res.X;
            res.Y = -res.Y;
            res.Z = -res.Z;
            return res;
        }

        public void Multiply(Quaternion value)
        {
            var q = Multiply(this, value);
            W = q.W;
            X = q.X;
            Y = q.Y;
            Z = q.Z;
        }

        public static Quaternion Multiply(Quaternion value1, Quaternion value2)
        {
            var res = new Quaternion
            {
                W = value1.W * value2.W - value1.X * value2.X - value1.Y * value2.Y - value1.Z * value2.Z,
                X = value1.W * value2.X + value1.X * value2.W + value1.Y * value2.Z - value1.Z * value2.Y,
                Y = value1.W * value2.Y - value1.X * value2.Z + value1.Y * value2.W + value1.Z * value2.X,
                Z = value1.W * value2.Z + value1.X * value2.Y - value1.Y * value2.X + value1.Z * value2.W
                //W = value1.W * value2.W - (value1.X * value2.X + value1.Y * value2.Y) + value1.Z * value2.Z,
                //X = value1.W * value2.X + value1.X * value2.W + value1.Y * value2.Z - value1.Z * value2.Y,
                //Y = value1.W * value2.Y - value1.X * value2.Z + value1.Y * value2.W + value1.Z * value2.X,
                //Z = value1.W * value2.Z + value1.X * value2.Y - value1.Y * value2.X + value1.Z * value2.W
            };


            //double x = q1.X;
            //double y = q1.Y;
            //double z = q1.Z;
            //double w = q1.W;
            //double num4 = q2.X;
            //double num3 = q2.Y;
            //double num2 = q2.Z;
            //double num = q2.W;
            //double num12 = (q1.Y * q2.Z) - (q1.Z * q2.Y);
            //double num11 = (q1.Z * q2.X) - (q1.X * q2.Z);
            //double num10 = (q1.X * q2.Y) - (q1.Y * q2.X);
            //double num9 = ((q1.X * q2.X) + (q1.Y * q2.Y)) + (q1.Z * q2.Z);
            //quaternion.X = ((q1.X * q2.W) + (q2.X * q1.W)) + (q1.Y * q2.Z) - (q1.Z * q2.Y);
            //quaternion.Y = ((q1.Y * q2.W) + (q2.Y * q1.W)) + (q1.Z * q2.X) - (q1.X * q2.Z);
            //quaternion.Z = ((q1.Z * q2.W) + (q2.Z * q1.W)) + (q1.X * q2.Y) - (q1.Y * q2.X);
            //quaternion.W = (q1.W * q2.W) - ((q1.X * q2.X) + (q1.Y * q2.Y)) + (q1.Z * q2.Z);

            return res;
        }

        public void Multiply(Vector3D value)
        {
            var q = Multiply(this, value);
            W = q.W;
            X = q.X;
            Y = q.Y;
            Z = q.Z;
        }

        public static Quaternion Multiply(Quaternion value1, Vector3D value2)
        {
            var q = new Quaternion(0, value2.X, value2.Y, value2.Z);
            return Multiply(value1, q);
        }
        #endregion

        #region properties

        public double X { get; set; }

        public double Y { get; set; }

        public double Z { get; set; }

        public double W { get; set; }

        #endregion

        #region Object Overrides

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Quaternion)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = X.GetHashCode();
                hashCode = (hashCode * 397) ^ Y.GetHashCode();
                hashCode = (hashCode * 397) ^ Z.GetHashCode();
                hashCode = (hashCode * 397) ^ W.GetHashCode();
                return hashCode;
            }
        }

        protected bool Equals(Quaternion other)
        {
            return X.Equals(other.X) && Y.Equals(other.Y) && Z.Equals(other.Z) && W.Equals(other.W);
        }

        public override string ToString()
        {
            return $"Quaternion: W = {W} X = {X} Y = {Y} Z = {Z}";
        }

        #endregion
    }
}
