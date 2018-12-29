using System;


namespace GameLogic
{
    public static class ColorTools
    {
        private static float wheel(float c, float distance) {
            return (c + distance) % 1.0f;
        }
        public static (float, float) GetComplementaryHues(float startHue)
        {
            return (startHue,wheel(startHue,0.5f));
        }

        public static (float, float, float) GetAnalogousHues(float startHue)
        {
            return (wheel(startHue,-0.08333f), startHue,wheel(startHue,0.08333f));
        }

        public static (float, float, float) GetSplitComplementaryHues(float startHue)
        {
            float complementary = wheel(startHue,0.5f);
            return (wheel(complementary,-0.0833f), startHue, wheel(complementary,0.0833f));
        }

        public static (float, float, float) GetTriadicHues(float startHue)
        {
            return (wheel(startHue,-0.33333f), startHue, wheel(startHue,0.33333f));
        }

        public static (float, float, float, float) GetSquareHues(float startHue)
        {
            return (wheel(startHue, 0.5f), wheel(startHue, -0.25f), startHue, wheel(startHue, 0.25f));
        }

        public static (float, float, float, float) GetTetradicHues(float startHue)
        {
            float complementary = wheel(startHue, 0.5f);
            return (complementary,wheel(complementary,0.16666f),startHue,wheel(startHue,0.16666f));
        }

        
        public static (float, float, float, float) GetDoubleComplement(float startHue)
        {
            float analog = wheel(startHue, 0.0833f);
            float complement = wheel(startHue, 0.5f);
            float companalog = wheel(complement, 0.0833f);
            return (startHue, analog, complement, companalog);
            
        }

        public static (float, float) GetDiad(float startHue)
        {
            return (startHue, wheel(startHue, 0.16666f));
        }

        public static (float, float, float) HSV2RGB(float h, float s, float v)
        {

            var i = (int)Math.Floor(h * 6.0f);
            var f = h * 6.0f - i;
            var p = v * (1.0f - s);
            var q = v * (1.0f - f * s);
            var t = v * (1.0f - (1.0f - f) * s);

            switch (i % 6)
            {
                case 0: return (v, t, p);
                case 1: return (q, v, p);
                case 2: return (p, v, t);
                case 3: return (p, q, v);
                case 4: return (t, p, v);
                default: return (v, p, q);
            }

        }
    }
}
