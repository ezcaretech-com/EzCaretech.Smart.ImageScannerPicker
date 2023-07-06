using Dynamsoft.TWAIN;
using Dynamsoft.TWAIN.Enums;
using Dynamsoft.TWAIN.Interface;
using System;
using System.Collections.Generic;
using System.Drawing;
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

        public IEnumerable<Dpi> Dpis =>
            new List<Dpi>
            {
                Dpi.D75,
                Dpi.D100,
                Dpi.D150,
                Dpi.D180,
                Dpi.D200,
                Dpi.D240,
                Dpi.D300,
                Dpi.D360,
                Dpi.D400,
                Dpi.D600,
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

        public void OpenDeviceSettingWindow() =>
            _twainManager.SelectSource();

        public bool IsDeviceSelected =>
            !string.IsNullOrEmpty(_twainManager.CurrentSourceName);

        public string GetDeviceNameSelected =>
            _twainManager.CurrentSourceName;

        public IEnumerable<string> GetDeviceList()
        {
            List<string> list = new List<string>();

            for (short idx = 0; idx < _twainManager.SourceCount; idx++)
                list.Add(_twainManager.SourceNameItems(idx));

            return list;
        }

        public void Scan(ScanOptions options)
        {
            SetDevice(options.DeviceName);
            _twainManager.OpenSource();

            SetColorSet(options.ColorSet);
            SetFeeder(options.Feeder);
            SetDuplex(options.Duplex);
            SetPaperSize(options.PaperSize);
            SetDpi(options.Dpi);
            SetBrightness(options.Brightness);
            SetContrast(options.Contrast);

            _twainManager.IfDisableSourceAfterAcquire = true;
            _twainManager.IfShowUI = options.IsShowUI;
            _twainManager.AcquireImage(this);
        }

        private void SetDevice(string deviceName)
        {
            for (short idx = 0; idx < _twainManager.SourceCount; idx++)
            {
                if (_twainManager.SourceNameItems(idx).Equals(deviceName))
                {
                    _twainManager.SelectSourceByIndex(idx);
                    break;
                }
            }
        }

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

        private void SetDpi(Dpi value) => _twainManager.Resolution = (float)value;

        private void SetBrightness(int value) => _twainManager.Brightness = value;

        private void SetContrast(int value) => _twainManager.Contrast = value;

        #region Scan SDK Interface

        public void OnPreAllTransfers() =>
            _config.WillBatchDelegate?.Invoke();

        public bool OnPreTransfer()
        {
            _config.WillPageScanDelegate?.Invoke();
            return true;
        }

        public bool OnPostTransfer(Bitmap bit)
        {
            try
            {
                _config.DidPageScanDelegate?.Invoke();

                string filePath = Path.GetTempFileName();
                bit.Save(filePath);

                _config.DonePageScanDelegate?.Invoke(filePath);
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

        public void Dispose()
        {
            _twainManager.Dispose();
        }
    }
}
