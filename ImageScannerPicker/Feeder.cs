using ImageScannerPicker.Properties;
using System.Globalization;

namespace ImageScannerPicker
{
    public enum Feeder
    {
        /// <summary>
        /// Automatic Document Feeder
        /// </summary>
        ADF,

        /// <summary>
        /// a flat surface for scanning documents
        /// </summary>
        FLATBED,
    }

    public static class FeederExtension
    {
        public static string GetName(this Feeder feeder)
        {
            switch (feeder)
            {
                case Feeder.ADF:
                    return Resources.ResourceManager.GetString("DIC_ADF", CultureInfo.CurrentCulture);
                case Feeder.FLATBED:
                    return Resources.ResourceManager.GetString("DIC_Flatbed", CultureInfo.CurrentCulture);
                default:
                    return string.Empty;
            }
        }
    }
}
