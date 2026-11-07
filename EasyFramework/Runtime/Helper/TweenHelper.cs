/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2018/3/1
// describe:
//----------------------------------------------------------------*/

using System;

namespace EasyFramework
{
    public static class TweenHelper
    {
        /// <summary>
        /// 类型枚举
        /// </summary>
        public enum EaseType
        {
            EaseInQuad,
            EaseOutQuad,
            EaseInOutQuad,
            EaseInCubic,
            EaseOutCubic,
            EaseInOutCubic,
            EaseInQuart,
            EaseOutQuart,
            EaseInOutQuart,
            EaseInQuint,
            EaseOutQuint,
            EaseInOutQuint,
            EaseInSine,
            EaseOutSine,
            EaseInOutSine,
            EaseInExpo,
            EaseOutExpo,
            EaseInOutExpo,
            EaseInCirc,
            EaseOutCirc,
            EaseInOutCirc,
            Linear,
            Spring,
            /* GFX47 MOD START */
            //bounce,
            EaseInBounce,
            EaseOutBounce,
            EaseInOutBounce,
            /* GFX47 MOD END */
            EaseInBack,
            EaseOutBack,
            EaseInOutBack,
            /* GFX47 MOD START */
            //elastic,
            EaseInElastic,
            EaseOutElastic,
            EaseInOutElastic,
            /* GFX47 MOD END */
            Punch
        }

        public static float Lerp(float start, float end, float value)
        {
            return (end - start) * value + start;
        }

        public static float Clamp(float val, float min, float max)
        {
            if (val < min)
                return min;
            if (val > max)
                return max;
            return val;
        }

        #region Easing Curves

        public static float Linear(float start, float end, float value)
        {
            return Lerp(start, end, value);
        }

        public static float Clerp(float start, float end, float value)
        {
            float min = 0.0f;
            float max = 360.0f;
            float half = Math.Abs((max - min) * 0.5f);
            float retval = 0.0f;
            float diff = 0.0f;
            if ((end - start) < -half)
            {
                diff = ((max - start) + end) * value;
                retval = start + diff;
            }
            else if ((end - start) > half)
            {
                diff = -((max - end) + start) * value;
                retval = start + diff;
            }
            else retval = start + (end - start) * value;
            return retval;
        }

        public static float Spring(float start, float end, float value)
        {
            value = Clamp(value, 0, 1);
            value = (float)((Math.Sin(value * Math.PI * (0.2f + 2.5f * value * value * value)) * Math.Pow(1f - value, 2.2f) + value) * (1f + (1.2f * (1f - value))));
            return start + (end - start) * value;
        }

        public static float EaseInQuad(float start, float end, float value)
        {
            end -= start;
            return end * value * value + start;
        }

        public static float EaseOutQuad(float start, float end, float value)
        {
            end -= start;
            return -end * value * (value - 2) + start;
        }

        public static float EaseInOutQuad(float start, float end, float value)
        {
            value /= .5f;
            end -= start;
            if (value < 1) return end * 0.5f * value * value + start;
            value--;
            return -end * 0.5f * (value * (value - 2) - 1) + start;
        }

        public static float EaseInCubic(float start, float end, float value)
        {
            end -= start;
            return end * value * value * value + start;
        }

        public static float EaseOutCubic(float start, float end, float value)
        {
            value--;
            end -= start;
            return end * (value * value * value + 1) + start;
        }

        public static float EaseInOutCubic(float start, float end, float value)
        {
            value /= .5f;
            end -= start;
            if (value < 1) return end * 0.5f * value * value * value + start;
            value -= 2;
            return end * 0.5f * (value * value * value + 2) + start;
        }

        public static float EaseInQuart(float start, float end, float value)
        {
            end -= start;
            return end * value * value * value * value + start;
        }

        public static float EaseOutQuart(float start, float end, float value)
        {
            value--;
            end -= start;
            return -end * (value * value * value * value - 1) + start;
        }

        public static float EaseInOutQuart(float start, float end, float value)
        {
            value /= .5f;
            end -= start;
            if (value < 1) return end * 0.5f * value * value * value * value + start;
            value -= 2;
            return -end * 0.5f * (value * value * value * value - 2) + start;
        }

        public static float EaseInQuint(float start, float end, float value)
        {
            end -= start;
            return end * value * value * value * value * value + start;
        }

        public static float EaseOutQuint(float start, float end, float value)
        {
            value--;
            end -= start;
            return end * (value * value * value * value * value + 1) + start;
        }

        public static float EaseInOutQuint(float start, float end, float value)
        {
            value /= .5f;
            end -= start;
            if (value < 1) return end * 0.5f * value * value * value * value * value + start;
            value -= 2;
            return end * 0.5f * (value * value * value * value * value + 2) + start;
        }

        public static float EaseInSine(float start, float end, float value)
        {
            end -= start;
            return (float)(-end * Math.Cos(value * (Math.PI * 0.5f)) + end + start);
        }

        public static float EaseOutSine(float start, float end, float value)
        {
            end -= start;
            return (float)(end * Math.Sin(value * (Math.PI * 0.5f)) + start);
        }

        public static float EaseInOutSine(float start, float end, float value)
        {
            end -= start;
            return (float)(-end * 0.5f * (Math.Cos(Math.PI * value) - 1) + start);
        }

        public static float EaseInExpo(float start, float end, float value)
        {
            end -= start;
            return (float)(end * Math.Pow(2, 10 * (value - 1)) + start);
        }

        public static float EaseOutExpo(float start, float end, float value)
        {
            end -= start;
            return (float)(end * (-Math.Pow(2, -10 * value) + 1) + start);
        }

        public static float EaseInOutExpo(float start, float end, float value)
        {
            value /= .5f;
            end -= start;
            if (value < 1) return (float)(end * 0.5f * Math.Pow(2, 10 * (value - 1)) + start);
            value--;
            return (float)(end * 0.5f * (-Math.Pow(2, -10 * value) + 2) + start);
        }

        public static float EaseInCirc(float start, float end, float value)
        {
            end -= start;
            return (float)(-end * (Math.Sqrt(1 - value * value) - 1) + start);
        }

        public static float EaseOutCirc(float start, float end, float value)
        {
            value--;
            end -= start;
            return (float)(end * Math.Sqrt(1 - value * value) + start);
        }

        public static float EaseInOutCirc(float start, float end, float value)
        {
            value /= .5f;
            end -= start;
            if (value < 1) return (float)(-end * 0.5f * (Math.Sqrt(1 - value * value) - 1) + start);
            value -= 2;
            return (float)(end * 0.5f * (Math.Sqrt(1 - value * value) + 1) + start);
        }

        /* GFX47 MOD START */
        public static float EaseInBounce(float start, float end, float value)
        {
            end -= start;
            float d = 1f;
            return end - EaseOutBounce(0, end, d - value) + start;
        }
        /* GFX47 MOD END */

        /* GFX47 MOD START */
        //private float bounce(float start, float end, float value){
        public static float EaseOutBounce(float start, float end, float value)
        {
            value /= 1f;
            end -= start;
            if (value < (1 / 2.75f))
            {
                return end * (7.5625f * value * value) + start;
            }
            else if (value < (2 / 2.75f))
            {
                value -= (1.5f / 2.75f);
                return end * (7.5625f * (value) * value + .75f) + start;
            }
            else if (value < (2.5 / 2.75))
            {
                value -= (2.25f / 2.75f);
                return end * (7.5625f * (value) * value + .9375f) + start;
            }
            else
            {
                value -= (2.625f / 2.75f);
                return end * (7.5625f * (value) * value + .984375f) + start;
            }
        }
        /* GFX47 MOD END */

        /* GFX47 MOD START */
        public static float EaseInOutBounce(float start, float end, float value)
        {
            end -= start;
            float d = 1f;
            if (value < d * 0.5f) return EaseInBounce(0, end, value * 2) * 0.5f + start;
            else return EaseOutBounce(0, end, value * 2 - d) * 0.5f + end * 0.5f + start;
        }
        /* GFX47 MOD END */

        public static float EaseInBack(float start, float end, float value)
        {
            end -= start;
            value /= 1;
            float s = 1.70158f;
            return end * (value) * value * ((s + 1) * value - s) + start;
        }

        public static float EaseOutBack(float start, float end, float value)
        {
            float s = 1.70158f;
            end -= start;
            value = (value) - 1;
            return end * ((value) * value * ((s + 1) * value + s) + 1) + start;
        }

        public static float EaseInOutBack(float start, float end, float value)
        {
            float s = 1.70158f;
            end -= start;
            value /= .5f;
            if ((value) < 1)
            {
                s *= (1.525f);
                return end * 0.5f * (value * value * (((s) + 1) * value - s)) + start;
            }
            value -= 2;
            s *= (1.525f);
            return end * 0.5f * ((value) * value * (((s) + 1) * value + s) + 2) + start;
        }

        public static float Punch(float amplitude, float value)
        {
            float s = 9;
            if (value == 0)
            {
                return 0;
            }
            else if (value == 1)
            {
                return 0;
            }
            float period = 1 * 0.3f;
            s = (float)(period / (2 * Math.PI) * Math.Asin(0));
            return ((float)(amplitude * Math.Pow(2, -10 * value) * Math.Sin((value * 1 - s) * (2 * Math.PI) / period)));
        }

        /* GFX47 MOD START */
        public static float EaseInElastic(float start, float end, float value)
        {
            end -= start;

            float d = 1f;
            float p = d * .3f;
            float s = 0;
            float a = 0;

            if (value == 0) return start;

            if ((value /= d) == 1) return start + end;

            if (a == 0f || a < Math.Abs(end))
            {
                a = end;
                s = p / 4;
            }
            else
            {
                s = (float)(p / (2 * Math.PI) * Math.Asin(end / a));
            }

            return (float)(-(a * Math.Pow(2, 10 * (value -= 1)) * Math.Sin((value * d - s) * (2 * Math.PI) / p)) + start);
        }
        /* GFX47 MOD END */

        /* GFX47 MOD START */
        //private float elastic(float start, float end, float value){
        public static float EaseOutElastic(float start, float end, float value)
        {
            /* GFX47 MOD END */
            //Thank you to rafael.marteleto for fixing this as a port over from Pedro's UnityTween
            end -= start;

            float d = 1f;
            float p = d * .3f;
            float s = 0;
            float a = 0;

            if (value == 0) return start;

            if ((value /= d) == 1) return start + end;

            if (a == 0f || a < Math.Abs(end))
            {
                a = end;
                s = p * 0.25f;
            }
            else
            {
                s = (float)(p / (2 * Math.PI) * Math.Asin(end / a));
            }

            return ((float)(a * Math.Pow(2, -10 * value) * Math.Sin((value * d - s) * (2 * Math.PI) / p) + end + start));
        }

        /* GFX47 MOD START */
        public static float EaseInOutElastic(float start, float end, float value)
        {
            end -= start;

            float d = 1f;
            float p = d * .3f;
            float s = 0;
            float a = 0;

            if (value == 0) return start;

            if ((value /= d * 0.5f) == 2) return start + end;

            if (a == 0f || a < Math.Abs(end))
            {
                a = end;
                s = p / 4;
            }
            else
            {
                s = (float)(p / (2 * Math.PI) * Math.Asin(end / a));
            }

            if (value < 1) return (float)(-0.5f * (a * Math.Pow(2, 10 * (value -= 1)) * Math.Sin((value * d - s) * (2 * Math.PI) / p)) + start);
            return (float)(a * Math.Pow(2, -10 * (value -= 1)) * Math.Sin((value * d - s) * (2 * Math.PI) / p) * 0.5f + end + start);
        }
        /* GFX47 MOD END */

        #endregion

    }
}