using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ImageScannerPicker.AtalasoftTwainX64Adaptor
{
    /// <summary>
    /// https://www.atalasoft.com/Products/DotImage
    /// </summary>
    public class AtalasoftTwainX64Scanner : IImageScannerPlugin
    {
        public string Name => "AtalasoftTwainX64Scanner";

        public string Description => "Use this library to enable interaction with TWAIN drivers for scanners and cameras to capture images directly into custom applications.";

        public IEnumerable<DeviceMethod> DeviceMethods => throw new NotImplementedException();

        public IEnumerable<ColorSet> ColorSets => throw new NotImplementedException();

        public IEnumerable<Feeder> Feeders => throw new NotImplementedException();

        public IEnumerable<Duplex> Duplexes => throw new NotImplementedException();

        public IEnumerable<PaperSize> PaperSizes => throw new NotImplementedException();

        public IEnumerable<RotateDegree> RotateDegrees => throw new NotImplementedException();

        public IEnumerable<Resolution> Resolutions => throw new NotImplementedException();

        public IEnumerable<int> Brightnesses => throw new NotImplementedException();

        public IEnumerable<int> Contrasts => throw new NotImplementedException();

        public bool IsDataSourceOpened => throw new NotImplementedException();

        public string SelectedDataSourceName => throw new NotImplementedException();

        private readonly ImageScannerConfig _config;

        public AtalasoftTwainX64Scanner(ImageScannerConfig config)
        {
            _config = config;
        }

        public IEnumerable<string> DataSourceList()
        {
            return new List<string>();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public void Scan(ScanOptions options)
        {
            throw new NotImplementedException();
        }

        public void SetDataSource(string dataSourceName)
        {
            throw new NotImplementedException();
        }

        public void ShowSettingUI()
        {
            throw new NotImplementedException();
        }

        public void ShowSourceSelector()
        {
            throw new NotImplementedException();
        }
    }
}
