using ImageScannerPicker.Properties;
using System.Globalization;

namespace ImageScannerPicker
{
    public enum PaperSize
    {
        A3,
        A4,
        B3,
        B4,
        B5,
        LETTER,
        BUSINESSCARD,
    }

    public static class PaperSizeExtension
    {
        public static string GetName(this PaperSize paperSize)
        {
            switch (paperSize)
            {
                case PaperSize.A3:
                    return Resources.ResourceManager.GetString("DIC_A3", CultureInfo.CurrentCulture);
                case PaperSize.A4:
                    return Resources.ResourceManager.GetString("DIC_A4", CultureInfo.CurrentCulture);
                case PaperSize.B3:
                    return Resources.ResourceManager.GetString("DIC_B3", CultureInfo.CurrentCulture);
                case PaperSize.B4:
                    return Resources.ResourceManager.GetString("DIC_B4", CultureInfo.CurrentCulture);
                case PaperSize.B5:
                    return Resources.ResourceManager.GetString("DIC_B5", CultureInfo.CurrentCulture);
                case PaperSize.LETTER:
                    return Resources.ResourceManager.GetString("DIC_Letter", CultureInfo.CurrentCulture);
                case PaperSize.BUSINESSCARD:
                    return Resources.ResourceManager.GetString("DIC_BusinessCard", CultureInfo.CurrentCulture);
                default:
                    return string.Empty;
            }
        }
    }
}
