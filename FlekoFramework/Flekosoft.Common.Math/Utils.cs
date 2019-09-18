using System;

namespace Flekosoft.Common.Math
{
    public static class Utils
    {
        /// <summary>
        /// Из градосув в радианы
        /// </summary>
        /// <param name="angleDegrees"></param>
        /// <returns></returns>
        public static double ToRadian(double angleDegrees)
        {
            return (float)angleDegrees * System.Math.PI / 180;
        }

        /// <summary>
        /// Из радиан в градусы
        /// </summary>
        /// <param name="angleRadian"></param>
        /// <returns></returns>
        public static double ToDegrees(double angleRadian)
        {
            return angleRadian * 180 / System.Math.PI;
        }

        /// <summary>
        /// clamps the first argument between the second two
        /// </summary>
        public static double Clamp(double arg, double minVal, double maxVal)
        {
            if (minVal > maxVal) throw new ArgumentException("MinVal > MaxVal");

            var res = arg;
            if (arg < minVal)
            {
                res = minVal;
            }

            if (arg > maxVal)
            {
                res = maxVal;
            }

            return res;
        }
    }
}
