namespace ColorTweaks;

internal static class Utils
{
    internal static float Remap(this float self, float oldMin, float oldMax, float newMin, float newMax) {
        return (self - oldMin) / (oldMax - oldMin) * (newMax - newMin) + newMin;
    }
}