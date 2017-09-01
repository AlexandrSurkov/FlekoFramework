namespace Flekosoft.Common.Math
{
    public static class Transrormations
    {

        //-------------------------- Vec3DRotateAroundOrigin --------------------------
        //
        //  rotates a vector ang rads around the origin
        //-----------------------------------------------------------------------------
        public static void Vector3DRotate(Vector3D vector, Vector3D axis, double ang)
        {
            //create a transformation matrix
            var mat = new Matrix2D();

            //rotate
            mat.Rotate(axis, ang);

            //now transform the object's vertices
            mat.TransformVector3D(vector);
        }


        public static void Vector3DRotateAroundX(Vector3D vector, double ang)
        {
            //create a transformation matrix
            var mat = new Matrix2D();

            //rotate
            mat.RotateAroundX(ang);

            //now transform the object's vertices
            mat.TransformVector3D(vector);
        }

        public static void Vector3DRotateAroundY(Vector3D vector, double ang)
        {
            //create a transformation matrix
            var mat = new Matrix2D();

            //rotate
            mat.RotateAroundY(ang);

            //now transform the object's vertices
            mat.TransformVector3D(vector);
        }

        public static void Vector3DRotateAroundZ(Vector3D vector, double ang)
        {
            //create a transformation matrix
            var mat = new Matrix2D();

            //rotate
            mat.RotateAroundZ(ang);

            //now transform the object's vertices
            mat.TransformVector3D(vector);
        }

        //-------------------------- Vec2DRotateAroundOrigin --------------------------
        //
        //  rotates a vector ang rads around the origin
        //-----------------------------------------------------------------------------
        public static void Vector2DRotateAroundOrigin(Vector2D vector, double ang)
        {
            //create a transformation matrix
            var mat = new Matrix2D();

            //rotate
            mat.RotateAroundZ(ang);

            //now transform the object's vertices
            mat.TransformVector2D(vector);
        }


        //--------------------- PointToLocalSpace --------------------------------
        //
        //------------------------------------------------------------------------
        public static Vector2D PointToLocalSpace(Vector2D point,
                                     Vector2D agentHeading,
                                     Vector2D agentSide,
                                      Vector2D agentPosition)
        {

            //make a copy of the point
            Vector2D transPoint = new Vector2D(point);

            //create a transformation matrix
            Matrix2D matTransform = new Matrix2D();

            double tx = -agentPosition.DotProduct(agentHeading);
            double ty = -agentPosition.DotProduct(agentSide);

            //create the transformation matrix
            matTransform.P11 = agentHeading.X; matTransform.P12 = agentSide.X;
            matTransform.P21 = agentHeading.Y; matTransform.P22 = agentSide.Y;
            matTransform.P31 = tx; matTransform.P32 = ty;

            //now transform the vertices
            matTransform.TransformVector2D(transPoint);

            return transPoint;
        }
    }
}
