using System.Collections.Generic;

namespace Flekosoft.Common.Math
{
    public class Matrix2D
    {
        private class Matrix
        {

            // ReSharper disable InconsistentNaming
            public double _11;
            public double _12;
            public double _13;
            public double _21;
            public double _22;
            public double _23;
            public double _31;
            public double _32;
            public double _33;
            // ReSharper restore InconsistentNaming
            public Matrix()
            {
                _11 = 0.0; _12 = 0.0; _13 = 0.0;
                _21 = 0.0; _22 = 0.0; _23 = 0.0;
                _31 = 0.0; _32 = 0.0; _33 = 0.0;
            }

            public Matrix(Matrix matrix)
            {
                _11 = matrix._11; _12 = matrix._12; _13 = matrix._13;
                _21 = matrix._21; _22 = matrix._22; _23 = matrix._23;
                _31 = matrix._31; _32 = matrix._32; _33 = matrix._33;
            }

        };

        private Matrix _matrix = new Matrix();

        private void MatrixMultiply(Matrix multiplier)
        {
            // ReSharper disable once UseObjectOrCollectionInitializer
            var matTemp = new Matrix();

            //first row
            matTemp._11 = (_matrix._11 * multiplier._11) + (_matrix._12 * multiplier._21) + (_matrix._13 * multiplier._31);
            matTemp._12 = (_matrix._11 * multiplier._12) + (_matrix._12 * multiplier._22) + (_matrix._13 * multiplier._32);
            matTemp._13 = (_matrix._11 * multiplier._13) + (_matrix._12 * multiplier._23) + (_matrix._13 * multiplier._33);

            //second
            matTemp._21 = (_matrix._21 * multiplier._11) + (_matrix._22 * multiplier._21) + (_matrix._23 * multiplier._31);
            matTemp._22 = (_matrix._21 * multiplier._12) + (_matrix._22 * multiplier._22) + (_matrix._23 * multiplier._32);
            matTemp._23 = (_matrix._21 * multiplier._13) + (_matrix._22 * multiplier._23) + (_matrix._23 * multiplier._33);

            //third
            matTemp._31 = (_matrix._31 * multiplier._11) + (_matrix._32 * multiplier._21) + (_matrix._33 * multiplier._31);
            matTemp._32 = (_matrix._31 * multiplier._12) + (_matrix._32 * multiplier._22) + (_matrix._33 * multiplier._32);
            matTemp._33 = (_matrix._31 * multiplier._13) + (_matrix._32 * multiplier._23) + (_matrix._33 * multiplier._33);

            _matrix = new Matrix(matTemp);
        }

        public Matrix2D()
        {
            //initialize the matrix to an identity matrix
            Identity();
        }

        /// <summary>
        /// create an identity matrix 
        /// </summary>
        public void Identity()
        {
            _matrix._11 = 1; _matrix._12 = 0; _matrix._13 = 0;
            _matrix._21 = 0; _matrix._22 = 1; _matrix._23 = 0;
            _matrix._31 = 0; _matrix._32 = 0; _matrix._33 = 1;
        }

        /// <summary>
        /// create a transformation matrix
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void Translate(double x, double y)
        {
            // ReSharper disable once UseObjectOrCollectionInitializer
            var mat = new Matrix();

            mat._11 = 1; mat._12 = 0; mat._13 = 0;

            mat._21 = 0; mat._22 = 1; mat._23 = 0;

            mat._31 = x; mat._32 = y; mat._33 = 1;

            //and multiply
            MatrixMultiply(mat);
        }

        /// <summary>
        /// create a scale matrix
        /// </summary>
        /// <param name="xScale"></param>
        /// <param name="yScale"></param>
        public void Scale(double xScale, double yScale)
        {
            // ReSharper disable once UseObjectOrCollectionInitializer
            var mat = new Matrix();

            mat._11 = xScale; mat._12 = 0; mat._13 = 0;

            mat._21 = 0; mat._22 = yScale; mat._23 = 0;

            mat._31 = 0; mat._32 = 0; mat._33 = 1;

            //and multiply
            MatrixMultiply(mat);
        }

        /// <summary>
        /// create a rotation matrix
        /// </summary>
        /// <param name="axis"></param>
        /// <param name="rotation"></param>
        public void Rotate(Vector3D axis, double rotation)
        {
            // ReSharper disable once UseObjectOrCollectionInitializer
            var mat = new Matrix();

            double sin = System.Math.Sin(rotation);
            double cos = System.Math.Cos(rotation);
            var u = axis;

            mat._11 = cos + u.X * u.X * (1 - cos);
            mat._12 = u.X * u.Y * (1 - cos) - u.Z * sin;
            mat._13 = u.X * u.Z * (1 - cos) + u.Y * sin;

            mat._21 = u.Y * u.X * (1 - cos) + u.Z * sin;
            mat._22 = cos + u.Y * u.Y * (1 - cos);
            mat._23 = u.Y * u.Z * (1 - cos) - u.X * sin;

            mat._31 = u.Z * u.X * (1 - cos) - u.Y * sin;
            mat._32 = u.Z * u.Y * (1 - cos) + u.X * sin;
            mat._33 = cos + u.Z * u.Z * (1 - cos);

            //and multiply
            MatrixMultiply(mat);
        }

        /// <summary>
        /// create a rotation matrix arund Z
        /// </summary>
        /// <param name="rotation"></param>
        public void RotateAroundZ(double rotation)
        {
            // ReSharper disable once UseObjectOrCollectionInitializer
            var mat = new Matrix();

            double sin = System.Math.Sin(rotation);
            double cos = System.Math.Cos(rotation);

            mat._11 = cos; mat._12 = sin; mat._13 = 0;

            mat._21 = -sin; mat._22 = cos; mat._23 = 0;

            mat._31 = 0; mat._32 = 0; mat._33 = 1;

            //and multiply
            MatrixMultiply(mat);
        }

        /// <summary>
        /// create a rotation matrix arund X
        /// </summary>
        /// <param name="rotation"></param>
        public void RotateAroundX(double rotation)
        {
            // ReSharper disable once UseObjectOrCollectionInitializer
            var mat = new Matrix();

            double sin = System.Math.Sin(rotation);
            double cos = System.Math.Cos(rotation);

            mat._11 = 1; mat._12 = 0; mat._13 = 0;

            mat._21 = 0; mat._22 = cos; mat._23 = sin;

            mat._31 = 0; mat._32 = -sin; mat._33 = cos;

            //and multiply
            MatrixMultiply(mat);
        }

        /// <summary>
        /// create a rotation matrix arund Y
        /// </summary>
        /// <param name="rotation"></param>
        public void RotateAroundY(double rotation)
        {
            // ReSharper disable once UseObjectOrCollectionInitializer
            var mat = new Matrix();

            double sin = System.Math.Sin(rotation);
            double cos = System.Math.Cos(rotation);

            mat._11 = cos; mat._12 = 0; mat._13 = -sin;

            mat._21 = 0; mat._22 = 1; mat._23 = 0;

            mat._31 = sin; mat._32 = 0; mat._33 = cos;

            //and multiply
            MatrixMultiply(mat);
        }

        /// <summary>
        /// create a rotation matrix from a fwd and side 2D vector
        /// </summary>
        /// <param name="fwd"></param>
        /// <param name="side"></param>
        public void Rotate(Vector2D fwd, Vector2D side)
        {
            // ReSharper disable once UseObjectOrCollectionInitializer
            var mat = new Matrix();

            mat._11 = fwd.X; mat._12 = fwd.Y; mat._13 = 0;

            mat._21 = side.X; mat._22 = side.Y; mat._23 = 0;

            mat._31 = 0; mat._32 = 0; mat._33 = 1;

            //and multiply
            MatrixMultiply(mat);
        }

        /// <summary>
        /// applys a transformation matrix to a listr of points
        /// </summary>
        /// <param name="points"></param>
        public void TransformVector2D(List<Vector2D> points)
        {
            // ReSharper disable once ForCanBeConvertedToForeach
            for (int i = 0; i < points.Count; ++i)
            {
                TransformVector2D(points[i]);

            }
        }

        /// <summary>
        /// applys a transformation matrix to a point
        /// </summary>
        /// <param name="point"></param>
        public void TransformVector2D(Vector2D point)
        {
            double tempX = (_matrix._11 * point.X) + (_matrix._21 * point.Y) + (_matrix._31);

            double tempY = (_matrix._12 * point.X) + (_matrix._22 * point.Y) + (_matrix._32);

            point.X = tempX;

            point.Y = tempY;
        }

        public void TransformVector3D(Vector3D point)
        {

            double tempX = (_matrix._11 * point.X) + (_matrix._21 * point.Y) + (_matrix._31 * point.Z);

            double tempY = (_matrix._12 * point.X) + (_matrix._22 * point.Y) + (_matrix._32 * point.Z);

            double tempZ = (_matrix._13 * point.X) + (_matrix._23 * point.Y) + (_matrix._33 * point.Z);

            point.X = tempX;

            point.Y = tempY;

            point.Z = tempZ;
        }

        public double P11
        {
            set { _matrix._11 = value; }
        }
        public double P12
        {
            set { _matrix._12 = value; }
        }
        public double P13
        {
            set { _matrix._13 = value; }
        }

        public double P21
        {
            set { _matrix._21 = value; }
        }
        public double P22
        {
            set { _matrix._22 = value; }
        }
        public double P23
        {
            set { _matrix._23 = value; }
        }

        public double P31
        {
            set { _matrix._31 = value; }
        }
        public double P32
        {
            set { _matrix._32 = value; }
        }
        public double P33
        {
            set { _matrix._33 = value; }
        }
    }
}
