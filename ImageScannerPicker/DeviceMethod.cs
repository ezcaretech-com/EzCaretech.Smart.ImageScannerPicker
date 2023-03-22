using ImageScannerPicker.Properties;
using System.Globalization;

namespace ImageScannerPicker
{
    public enum DeviceMethod
    {
        AUTO,
        MANUAL,
    }

    public static class DeviceMethodExtension
    {
        public static string GetName(this DeviceMethod deviceMethod)
        {
            switch (deviceMethod)
            {
                case DeviceMethod.AUTO:
                    return Resources.ResourceManager.GetString("DIC_Auto", CultureInfo.CurrentCulture);
                case DeviceMethod.MANUAL:
                    return Resources.ResourceManager.GetString("DIC_Manual", CultureInfo.CurrentCulture);
                default:
                    return string.Empty;
            }
        }
    }
}
