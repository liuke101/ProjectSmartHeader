using System;

namespace JustAssets.TerrainUtility
{
    static internal class TerrainMath
    {
        public static bool IsPowerOf2(int value)
        {
            return (value & (value - 1)) == 0;
        }

        public static int ClosestPowerOf2(int value)
        {
            var next = (int)Math.Pow(2, Math.Ceiling(Math.Log(value)/Math.Log(2)));
            return next;
        }

        public static int NextPowerOf2(int value)
        {
            value--;
            value |= value >> 1;
            value |= value >> 2;
            value |= value >> 4;
            value |= value >> 8;
            value |= value >> 16;
            value++;

            return value;
        }
    }
}