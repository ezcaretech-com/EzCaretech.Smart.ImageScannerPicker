using System;
using System.Collections.Generic;

namespace ImageScannerPicker
{
    public interface IImageScannerPlugin : IDisposable
    {
        string Name { get; }

        string Description { get; }

        IEnumerable<DeviceMethod> DeviceMethods { get; }

        IEnumerable<ColorSet> ColorSets { get; }

        IEnumerable<Feeder> Feeders { get; }

        IEnumerable<Duplex> Duplexes { get; }

        IEnumerable<PaperSize> PaperSizes { get; }

        IEnumerable<RotateDegree> RotateDegrees { get; }

        IEnumerable<Resolution> Resolutions { get; }

        IEnumerable<int> Brightnesses { get; }

        IEnumerable<int> Contrasts { get; }

        bool IsDataSourceOpened { get; }

        string SelectedDataSourceName { get; }

        void ShowSourceSelector();

        void ShowSettingUI();

        void SetDataSource(string dataSourceName);

        IEnumerable<string> DataSourceList();

        void Scan(ScanOptions options);
    }
}
