using ImageScannerPicker.Properties;
using System.Globalization;

namespace ImageScannerPicker
{
    public enum ColorSet
    {
        /// <summary>
        /// Black & White
        /// </summary>
        BLACKWHITE,

        /// <summary>
        /// Gray-Scale
        /// </summary>
        GRAYSCALE,

        /// <summary>
        /// Color
        /// </summary>
        COLOR,
    }

    public static class ColorSetExtension
    {
        public static string GetName(this ColorSet deviceMethod)
        {
            switch (deviceMethod)
            {
                case ColorSet.BLACKWHITE:
                    return Resources.ResourceManager.GetString("DIC_BlackWhite", CultureInfo.CurrentCulture);
                case ColorSet.GRAYSCALE:
                    return Resources.ResourceManager.GetString("DIC_GrayScale", CultureInfo.CurrentCulture);
                case ColorSet.COLOR:
                    return Resources.ResourceManager.GetString("DIC_Color", CultureInfo.CurrentCulture);
                default:
                    return string.Empty;
            }
        }
    }
}
