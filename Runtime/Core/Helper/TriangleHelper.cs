/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2018/3/1
// describe:
//----------------------------------------------------------------*/

using System;

namespace EasyFramework
{
    public static class TriangleHelper
    {

        #region -------- Triangle

        // ---------------------------------------------------------------------------- Triangle


        /// <summary>
        /// 获得三角形边A的对角-角度值
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        public static float GetTriangleOppositeAngle(float a, float b, float c)
        {
            // cosA=(b^2+c^2-a^2)/2bc
            float val = (b * b + c * c - a * a) / (2 * b * c);

            if (val < -1 || val > 1)
                return 0;
            return (float)(Math.Acos(val) / Math.PI * 180);
        }

        /// <summary>
        /// 以知三角形的两条边ab跟ab夹角，求c
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="angle"></param>
        /// <returns></returns>
        public static float GetTriangleEdgeByAngle(float a, float b, float angleC)
        {
            // c^2=a^2+b^2-2abCosC
            float val = (float)Math.Sqrt(a * a + b * b - 2 * a * b * Math.Cos(Math.PI / 180 * angleC));
            //Debug.Log(val);
            return val;
        }

        /// <summary>
        /// 以知三角形的两条边ab跟a的对角，求c
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="angle"></param>
        /// <returns></returns>
        public static float GetTriangleEdgeByFreeAngle(float a, float b, float angleA)
        {
            double rate = a / Math.Sin(Math.PI / 180 * angleA);
            //Debug.Log(rate);
            double val = b / rate;
            //Debug.Log(val);
            double angleB = Math.Asin(val) / Math.PI * 180;
            if (double.IsNaN(angleB))
            {
                //Log.Info("参数非法!");
                return 0;
            }
            double angleC = 180 - angleA - angleB;
            //Debug.Log(angleC);
            double c = rate * Math.Sin(Math.PI / 180 * angleC);
            //Debug.Log(c);
            return (float)c;
        }

        /// <summary>
        /// 求直角三角形的斜边
        /// </summary>
        /// <param name="a">邻边长度</param>
        /// <param name="angle">邻边跟斜边夹角</param>
        /// <returns></returns>
        public static float GetRightTriangleHypotenuse(float a, float angle)
        {
            // cosA=（∠A的）邻边/斜边
            return (float)(a / Math.Cos(Math.PI / 180 * angle));
        }

        /// <summary>
        /// 求直角三角形 邻边跟斜边的夹角
        /// </summary>
        /// <param name="a">邻边长度</param>
        /// <param name="b">斜边长度</param>
        /// <returns></returns>
        public static float GetRightTriangleAngle(float a, float b)
        {
            // cosA=（∠A的）邻边/斜边
            return (float)(Math.Acos(a / b) / Math.PI * 180);
        }


        // ==========================================================================================

        #endregion

    }
}
