using Dynamsoft.TWAIN;
using Dynamsoft.TWAIN.Enums;
using Dynamsoft.TWAIN.Interface;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

namespace ImageScannerPicker.Adaptor
{
    /// <summary>
    /// https://www.dynamsoft.com/dotnet-twain/overview/
    /// </summary>
    public class DynamsoftTwainScanner : IImageScannerPlugin, IAcquireCallback
    {
        public string Name => "DynamsoftTwainScanner";

        public string Description => "Dynamic .NET TWAIN SDK plugin.";

        public IEnumerable<DeviceMethod> DeviceMethods => throw new NotImplementedException();

        public IEnumerable<ColorSet> ColorSets =>
            new List<ColorSet>
            {
                ColorSet.BLACKWHITE,
                ColorSet.GRAYSCALE,
                ColorSet.COLOR,
            };

        public IEnumerable<Feeder> Feeders =>
            new List<Feeder>
            {
                Feeder.ADF,
                Feeder.FLATBED,
            };

        public IEnumerable<Duplex> Duplexes =>
            new List<Duplex>
            {
                Duplex.SINGLE,
                Duplex.BOTH,
            };

        public IEnumerable<PaperSize> PaperSizes =>
            new List<PaperSize>
            {
                PaperSize.A3,
                PaperSize.A4,
                PaperSize.B3,
                PaperSize.B4,
                PaperSize.B5,
                PaperSize.LETTER,
                PaperSize.BUSINESSCARD,
            };

        public IEnumerable<RotateDegree> RotateDegrees => throw new NotImplementedException();

        public IEnumerable<Resolution> Resolutions =>
            new List<Resolution>
            {
                Resolution.D75,
                Resolution.D100,
                Resolution.D150,
                Resolution.D180,
                Resolution.D200,
                Resolution.D240,
                Resolution.D300,
                Resolution.D360,
                Resolution.D400,
                Resolution.D600,
            };

        public IEnumerable<int> Brightnesses => Enumerable.Range(-1000, 2001);

        public IEnumerable<int> Contrasts => Enumerable.Range(-1000, 2001);

        private readonly ImageScannerConfig _config;

        private readonly TwainManager _twainManager;

        public DynamsoftTwainScanner(ImageScannerConfig config)
        {
            _config = config;
            _twainManager = new TwainManager(config.License ?? "");
        }

        public bool IsDataSourceOpened =>
            !string.IsNullOrEmpty(_twainManager.CurrentSourceName);

        public string SelectedDataSourceName =>
            _twainManager.CurrentSourceName;

        public bool IfGetImageInfo => true;

        public bool IfGetExtImageInfo => true;

        public void ShowSourceSelector() =>
            _twainManager.SelectSource();

        public void ShowSettingUI()
        {
        }

        public void SetDataSource(string dataSourceName)
        {
            for (short idx = 0; idx < _twainManager.SourceCount; idx++)
            {
                if (_twainManager.SourceNameItems(idx).Equals(dataSourceName))
                {
                    _twainManager.SelectSourceByIndex(idx);
                    break;
                }
            }
        }

        public IEnumerable<string> DataSourceList()
        {
            List<string> list = new List<string>();

            for (short idx = 0; idx < _twainManager.SourceCount; idx++)
                list.Add(_twainManager.SourceNameItems(idx));

            return list;
        }

        public void Scan(ScanOptions options)
        {
            _twainManager.OpenSource();

            SetColorSet(options.ColorSet);
            SetFeeder(options.Feeder);
            SetDuplex(options.Duplex);
            SetPaperSize(options.PaperSize);
            SetResolution(options.Resolution);
            SetBrightness(options.Brightness);
            SetContrast(options.Contrast);

            _twainManager.IfDisableSourceAfterAcquire = true;
            _twainManager.IfShowUI = options.IsShowUI;
            _twainManager.AcquireImage(this);
        }

        public void Dispose()
        {
            _twainManager.Dispose();
        }

        #region Set capabilities

        private void SetColorSet(ColorSet value)
        {
            switch (value)
            {
                case ColorSet.BLACKWHITE:
                    _twainManager.PixelType = TWICapPixelType.TWPT_BW;
                    break;
                case ColorSet.GRAYSCALE:
                    _twainManager.PixelType = TWICapPixelType.TWPT_GRAY;
                    break;
                case ColorSet.COLOR:
                    _twainManager.PixelType = TWICapPixelType.TWPT_RGB;
                    break;
            }
        }

        private void SetFeeder(Feeder value)
        {
            switch (value)
            {
                case Feeder.ADF:
                    _twainManager.IfFeederEnabled = true;
                    break;
                case Feeder.FLATBED:
                    _twainManager.IfFeederEnabled = false;
                    break;
            }
        }

        private void SetDuplex(Duplex value)
        {
            switch (value)
            {
                case Duplex.SINGLE:
                    _twainManager.IfDuplexEnabled = false;
                    break;
                case Duplex.BOTH:
                    _twainManager.IfDuplexEnabled = true;
                    break;
            }
        }

        private void SetPaperSize(PaperSize value)
        {
            switch (value)
            {
                case PaperSize.A3:
                    _twainManager.PageSize = (short)TWICapSupportedSizes.TWSS_A3;
                    break;
                case PaperSize.A4:
                    _twainManager.PageSize = (short)TWICapSupportedSizes.TWSS_A4;
                    break;
                case PaperSize.B3:
                    _twainManager.PageSize = (short)TWICapSupportedSizes.TWSS_B3;
                    break;
                case PaperSize.B4:
                    _twainManager.PageSize = (short)TWICapSupportedSizes.TWSS_B4;
                    break;
                case PaperSize.B5:
                    _twainManager.PageSize = (short)TWICapSupportedSizes.TWSS_B5LETTER;
                    break;
                case PaperSize.LETTER:
                    _twainManager.PageSize = (short)TWICapSupportedSizes.TWSS_USLETTER;
                    break;
                case PaperSize.BUSINESSCARD:
                    _twainManager.PageSize = (short)TWICapSupportedSizes.TWSS_BUSINESSCARD;
                    break;
            }
        }

        private void SetResolution(Resolution value) => _twainManager.Resolution = (float)value;

        private void SetBrightness(int value) => _twainManager.Brightness = value;

        private void SetContrast(int value) => _twainManager.Contrast = value;

        #endregion Set capabilities

        #region Scan SDK Interface

        public void OnPreAllTransfers() =>
            _config.WillBatchDelegate?.Invoke();

        public bool OnPreTransfer()
        {
            _config.WillPageScanDelegate?.Invoke();
            return true;
        }

        public bool OnPostTransfer(Bitmap bit, string info)
        {
            try
            {
                Console.WriteLine(info);
                _config.DidPageScanDelegate?.Invoke();

                string tempFilePath = Path.GetTempFileName();
                bit.Save(tempFilePath);

                ImageFormat format = ImageFormat.Png;
                string outputFilePath = Path.Combine(
                    Path.GetDirectoryName(tempFilePath),
                    $"{Path.GetFileNameWithoutExtension(tempFilePath)}.{format.ToString().ToLower()}");

                File.Move(tempFilePath, outputFilePath);

                _config.DonePageScanDelegate?.Invoke(outputFilePath);
                return true;
            }
            catch (Exception ex)
            {
                _config.ErrorDelegate?.Invoke(ex);
                return false;
            }
        }

        public void OnPostAllTransfers() =>
            _config.DidBatchDelegate?.Invoke();

        public void OnSourceUIClose() { }

        public void OnTransferCancelled() =>
            _config.DidBatchDelegate?.Invoke();

        public void OnTransferError() =>
            _config.ErrorDelegate?.Invoke(new Exception("Scan Error"));

        #endregion
    }
}
