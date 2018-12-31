using System.Runtime.CompilerServices;

namespace GameLogic
{
    public static class MathUtils
    {

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Lerp(float a, float b, float t) { return a + t * (b - a); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Lerp(double a, double b, double t) { return a + t * (b - a); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Lerp(int a, int b, double t) { return (int)(a + t * (b - a)); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int FastFloor(float f) { return (f >= 0 ? (int)f : (int)f - 1); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Constrain(float x, float lo, float hi)
        {
            if (float.IsNaN(x)) { return 0.0f; }
            if (float.IsPositiveInfinity(x)) { return 1.0f; }
            if (float.IsNegativeInfinity(x)) { return -1.0f; }
            float t = (x - lo) / (hi - lo);
            var result = lo + (hi - lo) * (t - FastFloor(t));            
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float FixNan(float x)
        {
            if (float.IsNaN(x)) { return 0.0f; }
            if (float.IsPositiveInfinity(x)) { return 1.0f; }
            if (float.IsNegativeInfinity(x)) { return -1.0f; }
            return x;
        }
    }
}
