using System.Collections.Generic;

namespace ImageScannerPicker
{
    public interface IImageScannerPlugin
    {
        string Name { get; }

        string Description { get; }

        IEnumerable<DeviceMethod> DeviceMethods { get; }

        IEnumerable<ColorSet> ColorSets { get; }

        IEnumerable<Feeder> Feeders { get; }

        IEnumerable<Duplex> Duplexes { get; }

        IEnumerable<PaperSize> PaperSizes { get; }

        IEnumerable<RotateDegree> RotateDegrees { get; }

        IEnumerable<Dpi> Dpis { get; }

        IEnumerable<int> Brightnesses { get; }

        IEnumerable<int> Contrasts { get; }

        void OpenDeviceSettingWindow();

        bool IsDeviceSelected { get; }

        string GetDeviceNameSelected { get; }

        IEnumerable<string> GetDeviceList();

        void Scan(ScanOptions options);
    }
}
