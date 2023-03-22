using Dynamsoft.TWAIN;
using Dynamsoft.TWAIN.Enums;
using Dynamsoft.TWAIN.Interface;
using ImageScannerPicker.Adaptor.Properties;
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

        private Delegates delegates = null;
        private static readonly TwainManager twainManager = new TwainManager(Settings.Default.License);

        public void SetDelegates(Delegates delegates)
        {
            this.delegates = delegates;
        }

        public void OpenDeviceSettingWindow() =>
            twainManager.SelectSource();

        public bool IsDeviceSelected =>
            !string.IsNullOrEmpty(twainManager.CurrentSourceName);

        public string GetDeviceNameSelected =>
            twainManager.CurrentSourceName;

        public IEnumerable<string> GetDeviceList()
        {
            List<string> list = new List<string>();

            for (short idx = 0; idx < twainManager.SourceCount; idx++)
                list.Add(twainManager.SourceNameItems(idx));

            return list;
        }

        public void Scan(ScanOptions options)
        {
            twainManager
                .SetDeviceMethod(options.DeviceMethod)
                .SetColorSet(options.ColorSet)
                .SetFeeder(options.Feeder)
                .SetDuplex(options.Duplex)
                .SetPaperSize(options.PaperSize)
                .SetRotateDegree(options.RotateDegree)
                .SetDpi(options.Dpi)
                .SetBrightness(options.Brightness)
                .SetContrast(options.Contrast);

            twainManager.IfDisableSourceAfterAcquire = true;
            twainManager.IfShowUI = options.IsShowUI;
            twainManager.AcquireImage(this);
        }

        #region Scan SDK Interface

        public void OnPreAllTransfers() =>
            delegates?.WillBatchDelegate?.Invoke();

        public bool OnPreTransfer()
        {
            delegates?.WillPageScanDelegate?.Invoke();
            return true;
        }

        public bool OnPostTransfer(Bitmap bit)
        {
            try
            {
                delegates?.DidPageScanDelegate?.Invoke();

                string filePath = Path.GetTempFileName();
                bit.Save(filePath);

                delegates?.DonePageScanDelegate?.Invoke(filePath);
                return true;
            }
            catch (Exception ex)
            {
                delegates?.ErrorDelegate?.Invoke(ex);
                return false;
            }
        }

        public void OnPostAllTransfers() =>
            delegates?.DidBatchDelegate?.Invoke();

        public void OnSourceUIClose() { }

        public void OnTransferCancelled() =>
            delegates?.DidBatchDelegate?.Invoke();

        public void OnTransferError() =>
            delegates?.ErrorDelegate?.Invoke(new Exception("Scan Error"));

        #endregion
    }

    public static class TwainManagerExtension
    {
        public static TwainManager SetDeviceMethod(this TwainManager ctrl, DeviceMethod value) => ctrl;

        public static TwainManager SetColorSet(this TwainManager ctrl, ColorSet value)
        {
            switch (value)
            {
                case ColorSet.BLACKWHITE:
                    ctrl.PixelType = TWICapPixelType.TWPT_BW;
                    break;
                case ColorSet.GRAYSCALE:
                    ctrl.PixelType = TWICapPixelType.TWPT_GRAY;
                    break;
                case ColorSet.COLOR:
                    ctrl.PixelType = TWICapPixelType.TWPT_RGB;
                    break;
            }

            return ctrl;
        }

        public static TwainManager SetFeeder(this TwainManager ctrl, Feeder value)
        {
            switch (value)
            {
                case Feeder.ADF:
                    ctrl.IfFeederEnabled = true;
                    break;
                case Feeder.FLATBED:
                    ctrl.IfFeederEnabled = false;
                    break;
            }

            return ctrl;
        }

        public static TwainManager SetDuplex(this TwainManager ctrl, Duplex value)
        {
            switch (value)
            {
                case Duplex.SINGLE:
                    ctrl.IfDuplexEnabled = false;
                    break;
                case Duplex.BOTH:
                    ctrl.IfDuplexEnabled = true;
                    break;
            }

            return ctrl;
        }

        public static TwainManager SetPaperSize(this TwainManager ctrl, PaperSize value)
        {

            switch (value)
            {
                case PaperSize.A3:
                    ctrl.PageSize = (short)TWICapSupportedSizes.TWSS_A3;
                    break;
                case PaperSize.A4:
                    ctrl.PageSize = (short)TWICapSupportedSizes.TWSS_A4;
                    break;
                case PaperSize.B3:
                    ctrl.PageSize = (short)TWICapSupportedSizes.TWSS_B3;
                    break;
                case PaperSize.B4:
                    ctrl.PageSize = (short)TWICapSupportedSizes.TWSS_B4;
                    break;
                case PaperSize.B5:
                    ctrl.PageSize = (short)TWICapSupportedSizes.TWSS_B5LETTER;
                    break;
                case PaperSize.LETTER:
                    ctrl.PageSize = (short)TWICapSupportedSizes.TWSS_USLETTER;
                    break;
                case PaperSize.BUSINESSCARD:
                    ctrl.PageSize = (short)TWICapSupportedSizes.TWSS_BUSINESSCARD;
                    break;
            }

            return ctrl;
        }

        public static TwainManager SetRotateDegree(this TwainManager ctrl, RotateDegree value) => ctrl;

        public static TwainManager SetDpi(this TwainManager ctrl, Dpi value)
        {
            ctrl.Resolution = (float)value;

            return ctrl;
        }

        public static TwainManager SetBrightness(this TwainManager ctrl, int value)
        {
            ctrl.Brightness = value;

            return ctrl;
        }

        public static TwainManager SetContrast(this TwainManager ctrl, int value)
        {
            ctrl.Contrast = value;

            return ctrl;
        }
    }
}
