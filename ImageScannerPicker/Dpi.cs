using ImageScannerPicker.Properties;
using System.Globalization;

namespace ImageScannerPicker
{
    public enum Dpi
    {
        D75 = 75,
        D100 = 100,
        D150 = 150,
        D180 = 180,
        D200 = 200,
        D240 = 240,
        D300 = 300,
        D360 = 360,
        D400 = 400,
        D600 = 600,
    }

    public static class DpiExtension
    {
        public static string GetName(this Dpi dpi) =>
            ((int)dpi).ToString();
    }
}
