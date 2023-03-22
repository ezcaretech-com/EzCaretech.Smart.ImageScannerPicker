using ImageScannerPicker.Properties;
using System.Globalization;

namespace ImageScannerPicker
{

    public enum RotateDegree
    {
        /// <summary>
        /// 0 degree
        /// </summary>
        D0 = 0,

        /// <summary>
        /// 90 degrees
        /// </summary>
        D90 = 90,

        /// <summary>
        /// 180 degrees
        /// </summary>
        D180 = 180,

        /// <summary>
        /// 270 degrees
        /// </summary>
        D270 = 270,
    }

    public static class RotateDegreeExtension
    {
        public static string GetName(this RotateDegree rotateDegree)
        {
            string unitName = Resources.ResourceManager.GetString("DIC_Angle", CultureInfo.CurrentCulture);

            switch (rotateDegree)
            {
                case RotateDegree.D0: return $"0{unitName}";
                case RotateDegree.D90: return $"90{unitName}";
                case RotateDegree.D180: return $"180{unitName}";
                case RotateDegree.D270: return $"270{unitName}";
                default: return string.Empty;
            }
        }
    }
}
