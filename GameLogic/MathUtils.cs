using System.Runtime.CompilerServices;

namespace GameLogic
{
    public static class MathUtils
    {
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
    }
}
