/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2018/3/1
// describe:
//----------------------------------------------------------------*/

using UnityEngine;

namespace EasyFramework
{
    /// <summary>
    /// 
    /// </summary>
    public static class Vector3Helper
    {
        #region -------- Vector3

        // ---------------------------------------------------------------------------- Vector3
        
        public static float Distance(float x1, float y1, float x2, float y2)
        {
            float v1 = x1 - x2;
            float v2 = y1 - y2;
            return Mathf.Sqrt(v1 * v1 + v2 * v2);
        }

        public static Vector3 Slerp(Vector3 a, Vector3 b, float t)
        {
            var angle = Vector3.Angle(a, b);
            var normal = Vector3.Cross(a.normalized, b.normalized);
            var result = Rotate(a, normal, angle * t);
            float distance = Mathf.Lerp(a.magnitude, b.magnitude, t);
            return result.normalized * distance;
        }
        
        public static Vector3 Slerp(Vector3 a, Vector3 b, Vector3 n, float t)
        {
            var angle = Vector3.Angle(a, b);
            var normal = Vector3.Cross(a.normalized, b.normalized);
            if (Vector3.Dot(normal.normalized, n) < 0)
            {
                angle = 360 - angle;
                normal = -normal;
            }
            var result = Rotate(a, normal, angle * t);
            float distance = Mathf.Lerp(a.magnitude, b.magnitude, t);
            return result.normalized * distance;
        }
    
        public static Vector3 SlerpInverse(Vector3 a, Vector3 b, float t)
        {
            var angle = 360 - Vector3.Angle(a, b);
            var normal = Vector3.Cross(a.normalized, b.normalized);
            var result = Rotate(a, -normal, angle * t);
            float distance = Mathf.Lerp(a.magnitude, b.magnitude, t);
            return result.normalized * distance;
        }

        /// <summary>
        /// 点绕某个向量旋转angle后的坐标
        /// </summary>
        /// <returns></returns>
        public static Vector3 Rotate(Vector3 p, Vector3 n, float angle) => Quaternion.AngleAxis(angle, n) * p;

        /// <summary>
        /// 获得坐标数组的长度
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static float GetPathDistance(Vector3[] path)
        {
            float dis = 0;
            for (int i = 0; i < path.Length - 1; i++)
            {
                dis += Vector3.Distance(path[i], path[i + 1]);
            }
            return dis;
        }

        /// <summary>
        /// 求两条直线的相交点 return true 存在相交 false 为平行
        /// </summary>
        /// <param name="a">线段一的坐标点a</param>
        /// <param name="b">线段一的坐标点b</param>
        /// <param name="c">线段二的坐标点c</param>
        /// <param name="d">线段二的坐标点d</param>
        /// <param name="result">输出结果</param>
        /// <returns></returns>
        public static bool GetIntersectPoint(Vector2 a, Vector2 b, Vector2 c, Vector2 d, out Vector2 result)
        {
            result = Vector2.zero;
            float denominator = (b.y - a.y) * (d.x - c.x) - (a.x - b.x) * (c.y - d.y);
            if (denominator == 0)
            {
                return false;
            }
            float x = ((b.x - a.x) * (d.x - c.x) * (c.y - a.y)
                + (b.y - a.y) * (d.x - c.x) * a.x
                - (d.y - c.y) * (b.x - a.x) * c.x) / denominator;
            float y = -((b.y - a.y) * (d.y - c.y) * (c.x - a.x)
                + (b.x - a.x) * (d.y - c.y) * a.y
                - (d.x - c.x) * (b.y - a.y) * c.y) / denominator;


            if ((x - a.x) * (x - b.x) <= 0 && (y - a.y) * (y - b.y) <= 0 && (x - c.x) * (x - d.x) <= 0 && (y - c.y) * (y - d.y) <= 0)
            {
                result = new Vector2(x, y);
            }

            return true;
        }

        /// <summary>
        /// 向量绕原点旋转
        /// </summary>
        /// <param name="direction">目标向量</param>
        /// <param name="rotateEuler">旋转的Euler角2</param>
        /// <returns></returns>
        public static Vector3 RotateDirection(Vector3 direction, Vector3 rotateEuler)
        {
            Vector3 result = direction;
            float xAngle = Mathf.PI / 180 * rotateEuler.x;
            float yAngle = Mathf.PI / 180 * rotateEuler.y;
            float zAngle = Mathf.PI / 180 * rotateEuler.z;
            float x, y, z;
            // x 轴
            y = Mathf.Cos(xAngle) * result.y - Mathf.Sin(xAngle) * result.z;
            z = Mathf.Sin(xAngle) * result.y + Mathf.Cos(xAngle) * result.z;
            result.y = y;
            result.z = z;
            // y 轴
            x = Mathf.Cos(yAngle) * result.x + Mathf.Sin(yAngle) * result.z;
            z = -Mathf.Sin(yAngle) * result.x + Mathf.Cos(yAngle) * result.z;
            result.x = x;
            result.z = z;
            // z 轴
            x = Mathf.Cos(zAngle) * result.x - Mathf.Sin(zAngle) * result.y;
            y = Mathf.Sin(zAngle) * result.x + Mathf.Cos(zAngle) * result.y;
            result.x = x;
            result.y = y;
            return result;
        }

        /// <summary>
        /// 计算直线与平面的交点
        /// </summary>
        /// <param name="point">直线上某一点</param>
        /// <param name="direct">直线的方向</param>
        /// <param name="planeNormal">垂直于平面的的向量</param>
        /// <param name="planePoint">平面上的任意一点</param>
        /// <returns></returns>
        public static Vector3 GetIntersectWithLineAndPlane(Vector3 point, Vector3 direct, Vector3 planeNormal, Vector3 planePoint)
        {
            float d = Vector3.Dot(planePoint - point, planeNormal) / Vector3.Dot(direct.normalized, planeNormal);
            return d * direct.normalized + point;
        }

        /// <summary>
        /// 抛物线模拟
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="height"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public static Vector3 Parabola(Vector3 start, Vector3 end, float height, float t)
        {
            float Func(float x) => 4 * (-height * x * x + height * x);

            var mid = Vector3.Lerp(start, end, t);

            return new Vector3(mid.x, Func(t) + Mathf.Lerp(start.y, end.y, t), mid.z);
        }

        public static Vector2 Parabola(Vector2 start, Vector2 end, float height, float t)
        {
            float Func(float x) => 4 * (-height * x * x + height * x);

            var mid = Vector2.Lerp(start, end, t);

            return new Vector2(mid.x, Func(t) + Mathf.Lerp(start.y, end.y, t));
        }


        // ==========================================================================================

        #endregion

        public static float GetBezierValue(float[] points, float t)
        {
            if (points.Length < 1)  // 一个点都没有
                return 0;
            int count = points.Length;
            float[] tmp_points = new float[count];
            for (int i = 1; i < count; ++i)
            {
                for (int j = 0; j < count - i; ++j)
                {
                    if (i == 1) // 计算+搬运数据,在计算的时候不要污染源数据
                    {
                        tmp_points[j] = (points[j] * (1 - t) + points[j + 1] * t);
                        continue;
                    }
                    tmp_points[j] = (tmp_points[j] * (1 - t) + tmp_points[j + 1] * t);
                }
            }
            return tmp_points[0];
        }


    }
}
