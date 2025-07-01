using System;

namespace Audio_Controller
{
    /// <summary>
    /// Utility math helpers for older frameworks lacking Math.Clamp.
    /// </summary>
    public static class MathHelpers
    {
        public static int Clamp(int value, int min, int max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }
    }
}
