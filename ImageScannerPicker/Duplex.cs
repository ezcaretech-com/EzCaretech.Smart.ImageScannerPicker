using ImageScannerPicker.Properties;
using System.Globalization;

namespace ImageScannerPicker
{
    public enum Duplex
    {
        /// <summary>
        /// Single side feeder
        /// </summary>
        SINGLE,

        /// <summary>
        /// Both side feeder
        /// </summary>
        BOTH,
    }

    public static class DuplexExtension
    {
        public static string GetName(this Duplex duplex)
        {
            switch (duplex)
            {
                case Duplex.SINGLE:
                    return Resources.ResourceManager.GetString("DIC_Single", CultureInfo.CurrentCulture);
                case Duplex.BOTH:
                    return Resources.ResourceManager.GetString("DIC_Both", CultureInfo.CurrentCulture);
                default:
                    return string.Empty;
            }
        }
    }
}
