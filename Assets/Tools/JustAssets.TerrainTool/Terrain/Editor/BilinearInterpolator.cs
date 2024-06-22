namespace JustAssets.TerrainUtility
{
    public static class BilinearInterpolator
    {
        public static float Interpolate(float[,] data, float xPos, float yPos, int parentWidthMinus1)
        {
            var x = parentWidthMinus1 < xPos ? parentWidthMinus1 : xPos;
            var y = parentWidthMinus1 < yPos ? parentWidthMinus1 : yPos;
            var xi = (int) x;
            var yi = (int) y;
            var b = xi + 1;
            var xiPlus1 = parentWidthMinus1 < b ? parentWidthMinus1 : b;
            var b1 = yi + 1;
            var yiPlus1 = parentWidthMinus1 < b1 ? parentWidthMinus1 : b1;

            var c00 = data[xi, yi];
            var c10 = data[xiPlus1, yi];
            var c01 = data[xi, yiPlus1];
            var c11 = data[xiPlus1, yiPlus1];

            var tx = x - xi;
            var ty = y - yi;
            var s = c00 + (c10 - c00) * tx;
            var e = c01 + (c11 - c01) * tx;
            return s + (e - s) * ty;
        }

        public static float Interpolate(int[,] data, float xPos, float yPos, int parentResolutionMinus1)
        {
            var x = parentResolutionMinus1 < xPos ? parentResolutionMinus1 : xPos;
            var y = parentResolutionMinus1 < yPos ? parentResolutionMinus1 : yPos;
            var xi = (int) x;
            var yi = (int) y;
            var b = xi + 1;
            var xiPlus1 = parentResolutionMinus1 < b ? parentResolutionMinus1 : b;
            var b1 = yi + 1;
            var yiPlus1 = parentResolutionMinus1 < b1 ? parentResolutionMinus1 : b1;

            float c00 = data[xi, yi];
            float c10 = data[xiPlus1, yi];
            float c01 = data[xi, yiPlus1];
            float c11 = data[xiPlus1, yiPlus1];

            var tx = x - xi;
            var ty = y - yi;
            var s = c00 + (c10 - c00) * tx;
            var e = c01 + (c11 - c01) * tx;
            return s + (e - s) * ty;
        }

        public static float Interpolate(float[,,] data, int c, float xPos, float yPos, int parentWidthMinus1)
        {
            var x = parentWidthMinus1 < xPos ? parentWidthMinus1 : xPos;
            var y = parentWidthMinus1 < yPos ? parentWidthMinus1 : yPos;
            var xi = (int) x;
            var yi = (int) y;
            var b = xi + 1;
            var xiPlus1 = parentWidthMinus1 < b ? parentWidthMinus1 : b;
            var b1 = yi + 1;
            var yiPlus1 = parentWidthMinus1 < b1 ? parentWidthMinus1 : b1;

            var c00 = data[xi, yi, c];
            var c10 = data[xiPlus1, yi, c];
            var c01 = data[xi, yiPlus1, c];
            var c11 = data[xiPlus1, yiPlus1, c];

            var tx = x - xi;
            var ty = y - yi;
            var s = c00 + (c10 - c00) * tx;
            var e = c01 + (c11 - c01) * tx;
            return s + (e - s) * ty;
        }
    }
}