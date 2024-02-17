using AxKScanLib;
using ImageScannerPicker.Adaptor.Properties;
using KScanLib;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace ImageScannerPicker.Adaptor
{
    public class ScanAxKofaxScanner : IImageScannerPlugin
    {
        public string Name => "ScanAxKofaxScanner";

        public string Description => "KOFAX SDK plugin.";

        public IEnumerable<DeviceMethod> DeviceMethods =>
            new List<DeviceMethod>
            {
                DeviceMethod.AUTO,
                DeviceMethod.MANUAL,
            };

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
            };

        public IEnumerable<RotateDegree> RotateDegrees =>
            new List<RotateDegree>
            {
                RotateDegree.D0,
                RotateDegree.D90,
                RotateDegree.D180,
                RotateDegree.D270,
            };

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

        public IEnumerable<int> Brightnesses => Enumerable.Range(1, 8);

        public IEnumerable<int> Contrasts => throw new NotImplementedException();

        private readonly ImageScannerConfig _config;

        private readonly KScanAxControl kScanAxControl;

        private string filePath = null;

        public ScanAxKofaxScanner(ImageScannerConfig config)
        {
            _config = config;

            kScanAxControl = new KScanAxControl();
            kScanAxControl.Config();

            kScanAxControl.axKScan.BatchStart += new EventHandler(this.KScan_BatchStart);
            kScanAxControl.axKScan.PageStart += new EventHandler(this.KScan_PageStart);
            kScanAxControl.axKScan.PageEnd += new EventHandler(this.KScan_PageEnd);
            kScanAxControl.axKScan.PageDone += new AxKScanLib._DKScanEvents_PageDoneEventHandler(this.KScan_PageDone);
            kScanAxControl.axKScan.BatchEnd += new EventHandler(this.KScan_BatchEnd);
            kScanAxControl.axKScan.ErrorEvent += new AxKScanLib._DKScanEvents_ErrorEventHandler(this.KScan_ErrorEvent);
        }

        ~ScanAxKofaxScanner()
        {
            kScanAxControl.axKScan.BatchStart -= new EventHandler(this.KScan_BatchStart);
            kScanAxControl.axKScan.PageStart -= new EventHandler(this.KScan_PageStart);
            kScanAxControl.axKScan.PageEnd -= new EventHandler(this.KScan_PageEnd);
            kScanAxControl.axKScan.PageDone -= new AxKScanLib._DKScanEvents_PageDoneEventHandler(this.KScan_PageDone);
            kScanAxControl.axKScan.BatchEnd -= new EventHandler(this.KScan_BatchEnd);
            kScanAxControl.axKScan.ErrorEvent -= new AxKScanLib._DKScanEvents_ErrorEventHandler(this.KScan_ErrorEvent);
        }

        public bool IsDataSourceOpened =>
            kScanAxControl.axKScan.DeviceReserved;

        public string SelectedDataSourceName =>
            kScanAxControl.axKScan.DeviceAlias;

        public void ShowSourceSelector()
        {
            kScanAxControl.axKScan.ActiveDialog = enumActiveDialog.KSDIALOGDEVICESETTINGS;
            kScanAxControl.axKScan.Action = enumAction.KSACTIONOPENDIALOG;
        }

        public void ShowSettingUI()
        {
        }

        public void SetDataSource(string dataSourceName)
        {

        }

        public IEnumerable<string> DataSourceList()
        {
            ShowSettingUI();
            return new List<string> { SelectedDataSourceName };
        }

        public void Scan(ScanOptions options)
        {
            kScanAxControl
                .SetDeviceMethod(options.DeviceMethod)
                .SetColorSet(options.ColorSet)
                .SetFeeder(options.Feeder)
                .SetDuplex(options.Duplex)
                .SetPaperSize(options.PaperSize)
                .SetRotateDegree(options.RotateDegree)
                .SetResolution(options.Resolution)
                .SetBrightness(options.Brightness)
                .SetContrast(options.Contrast);

            kScanAxControl.axKScan.ActiveDialog = enumActiveDialog.KSDIALOGSTORAGE; // TODO: 확인필요 AutoScan 에만 있는 코드
            kScanAxControl.axKScan.ScanDirection = (short)enumContants.KSSCANDIRECTPORTRAIT;
            kScanAxControl.axKScan.ScanMode = (short)enumContants.KSSCANMODELINE;

            if (kScanAxControl.axKScan.KodakATPCap == true)
                kScanAxControl.axKScan.KodakATPEnable = false;

            kScanAxControl.axKScan.Action = enumAction.KSACTIONSETSETTINGS;
            kScanAxControl.axKScan.Action = enumAction.KSACTIONSTART;
        }

        public void Dispose()
        {
            kScanAxControl.axKScan.Dispose();
        }

        #region Scan SDK Interface

        private void KScan_BatchStart(object sender, EventArgs e) => _config.WillBatchDelegate?.Invoke();

        private void KScan_PageStart(object sender, EventArgs e) => _config.WillPageScanDelegate?.Invoke();

        private void KScan_PageEnd(object sender, EventArgs e)
        {
            try
            {
                _config.DidPageScanDelegate?.Invoke();

                filePath = Path.GetTempFileName();
                kScanAxControl.axKScan.PEFileName = filePath;
            }
            catch (Exception ex)
            {
                _config.ErrorDelegate?.Invoke(ex);
            }
        }

        private void KScan_PageDone(object sender, _DKScanEvents_PageDoneEvent e)
        {
            kScanAxControl.axKScan.Refresh();
            _config.DonePageScanDelegate?.Invoke(filePath);

            //스캔 소스가 평판인 경우
            //한장만 스캔 하기 위함
            if (kScanAxControl.axKScan.ScanSource == (short)enumContants.KSSOURCEFLATBED)
            {
                kScanAxControl.axKScan.ScanSource = (short)enumContants.KSSOURCEADF;
                kScanAxControl.axKScan.Action = enumAction.KSACTIONSTOP;
            }
        }

        private void KScan_BatchEnd(object sender, EventArgs e) =>
            _config.DidBatchDelegate?.Invoke();

        private void KScan_ErrorEvent(object sender, _DKScanEvents_ErrorEvent e)
        {
            if (e.nError != 0)
            {
                if (e.nError < 20000)
                {
                    string errorMessage = Resources.ResourceManager.GetString("DIC_Error", CultureInfo.CurrentCulture);
                    e.strError = errorMessage;
                }
                else if (e.nError == 20025)
                {
                    return;
                }
                else if (e.nError == 20026)
                {
                    string errorMessage = Resources.ResourceManager.GetString("MSG_PaperJam", CultureInfo.CurrentCulture);
                    _config.ErrorDelegate?.Invoke(new Exception(errorMessage));
                }
            }
        }

        #endregion
    }

    public static class KScanAxControlExtension
    {
        public static KScanAxControl Config(this KScanAxControl ctrl)
        {
            ctrl = new KScanAxControl();

            //KScan.axKScan.get_DeviceScanCap(1) : "Fujitsu fi-6240Z without SVRS"
            //KScan.axKScan.get_DeviceScanCap(2) : "Fujitsu fi-6240Z without SVRS with AIPE"
            //KScan.axKScan.get_DeviceScanCap(3) : "Fujitsu fi-6240Z with SVRS"
            //KScan.axKScan.get_DeviceScanCap(4) : "Fujitsu fi-6240Z with SVRS with AIPE"
            ctrl.axKScan.DeviceAlias = ctrl.axKScan.get_DeviceScanCap(2);

            ctrl.axKScan.Action = enumAction.KSACTIONRESERVE;
            ctrl.axKScan.ActionResetMode = enumActionResetMode.KSACTIONRESETMODENOWAIT;
            ctrl.axKScan.Action = enumAction.KSACTIONSETSETTINGS;

            return ctrl;
        }

        public static KScanAxControl SetDeviceMethod(this KScanAxControl ctrl, DeviceMethod value)
        {
            switch (value)
            {
                case DeviceMethod.AUTO:
                    ctrl.axKScan.DeviceMethod = enumDeviceMethod.KSDEVICEMETHODBATCH;
                    break;
                case DeviceMethod.MANUAL:
                    ctrl.axKScan.DeviceMethod = enumDeviceMethod.KSDEVICEMETHODSINGLE;
                    break;
            }

            return ctrl;
        }

        public static KScanAxControl SetColorSet(this KScanAxControl ctrl, ColorSet value)
        {
            switch (value)
            {
                case ColorSet.BLACKWHITE:
                    ctrl.axKScan.ScanColorMode = (int)enumContants.KSSCANCOLORMODEBITONAL;
                    ctrl.axKScan.IOStgFlt = "TIFF";
                    break;
                case ColorSet.GRAYSCALE:
                    ctrl.axKScan.ScanColorMode = (int)enumContants.KSSCANCOLORMODE256GRAY;
                    ctrl.axKScan.IOStgFlt = "JPG";
                    break;
                case ColorSet.COLOR:
                    ctrl.axKScan.ScanColorMode = (int)enumContants.KSSCANCOLORMODE16MCOLOR;
                    ctrl.axKScan.IOStgFlt = "JPG";
                    break;
            }

            return ctrl;
        }

        public static KScanAxControl SetFeeder(this KScanAxControl ctrl, Feeder value)
        {
            switch (value)
            {
                case Feeder.ADF:
                    ctrl.axKScan.ScanSource = (short)enumContants.KSSOURCEADF;
                    break;
                case Feeder.FLATBED:
                    ctrl.axKScan.ScanSource = (short)enumContants.KSSOURCEFLATBED;
                    break;
            }

            return ctrl;
        }

        public static KScanAxControl SetDuplex(this KScanAxControl ctrl, Duplex value)
        {
            switch (value)
            {
                case Duplex.SINGLE:
                    ctrl.axKScan.ScanDuplex = false;
                    break;
                case Duplex.BOTH:
                    ctrl.axKScan.ScanDuplex = true;
                    break;
            }

            return ctrl;
        }

        public static KScanAxControl SetPaperSize(this KScanAxControl ctrl, PaperSize value)
        {
            switch (value)
            {
                case PaperSize.A3:
                    ctrl.axKScan.ScanSize = (int)enumKGPaperSize.KGSIZEA3;
                    break;
                case PaperSize.A4:
                    ctrl.axKScan.ScanSize = (int)enumKGPaperSize.KGSIZEA4;
                    break;
                case PaperSize.B3:
                    ctrl.axKScan.ScanSize = (int)enumKGPaperSize.KGSIZEB3;
                    break;
                case PaperSize.B4:
                    ctrl.axKScan.ScanSize = (int)enumKGPaperSize.KGSIZEB4;
                    break;
                case PaperSize.B5:
                    ctrl.axKScan.ScanSize = (int)enumKGPaperSize.KGSIZEB5;
                    break;
                case PaperSize.LETTER:
                    ctrl.axKScan.ScanSize = (int)enumKGPaperSize.KGSIZELETTER;
                    break;
            }

            return ctrl;
        }

        public static KScanAxControl SetRotateDegree(this KScanAxControl ctrl, RotateDegree value)
        {
            switch (value)
            {
                case RotateDegree.D0:
                    ctrl.axKScan.DeviceRotate = enumKGRotate.KGROTATE0;
                    break;
                case RotateDegree.D90:
                    ctrl.axKScan.DeviceRotate = enumKGRotate.KGROTATE90;
                    break;
                case RotateDegree.D180:
                    ctrl.axKScan.DeviceRotate = enumKGRotate.KGROTATE180;
                    break;
                case RotateDegree.D270:
                    ctrl.axKScan.DeviceRotate = enumKGRotate.KGROTATE270;
                    break;
            }

            return ctrl;
        }

        public static KScanAxControl SetResolution(this KScanAxControl ctrl, Resolution value)
        {
            switch (value)
            {
                case Resolution.D75:
                    ctrl.axKScan.ScanDpi = (int)enumKGContants.KGDPI75;
                    break;
                case Resolution.D100:
                    ctrl.axKScan.ScanDpi = (int)enumKGContants.KGDPI100;
                    break;
                case Resolution.D150:
                    ctrl.axKScan.ScanDpi = (int)enumKGContants.KGDPI150;
                    break;
                case Resolution.D180:
                    ctrl.axKScan.ScanDpi = (int)enumKGContants.KGDPI180;
                    break;
                case Resolution.D200:
                    ctrl.axKScan.ScanDpi = (int)enumKGContants.KGDPI200;
                    break;
                case Resolution.D240:
                    ctrl.axKScan.ScanDpi = (int)enumKGContants.KGDPI240;
                    break;
                case Resolution.D300:
                    ctrl.axKScan.ScanDpi = (int)enumKGContants.KGDPI300;
                    break;
                case Resolution.D360:
                    ctrl.axKScan.ScanDpi = (int)enumKGContants.KGDPI360;
                    break;
                case Resolution.D400:
                    ctrl.axKScan.ScanDpi = (int)enumKGContants.KGDPI400;
                    break;
                case Resolution.D600:
                    ctrl.axKScan.ScanDpi = (int)enumKGContants.KGDPI600;
                    break;
            }

            return ctrl;
        }

        public static KScanAxControl SetBrightness(this KScanAxControl ctrl, int value)
        {
            switch (value)
            {
                case 1:
                    ctrl.axKScan.ScanDensity = (short)enumKGContants.KGDENSITY1;
                    break;
                case 2:
                    ctrl.axKScan.ScanDensity = (short)enumKGContants.KGDENSITY2;
                    break;
                case 3:
                    ctrl.axKScan.ScanDensity = (short)enumKGContants.KGDENSITY3;
                    break;
                case 4:
                    ctrl.axKScan.ScanDensity = (short)enumKGContants.KGDENSITY4;
                    break;
                case 5:
                    ctrl.axKScan.ScanDensity = (short)enumKGContants.KGDENSITY5;
                    break;
                case 6:
                    ctrl.axKScan.ScanDensity = (short)enumKGContants.KGDENSITY6;
                    break;
                case 7:
                    ctrl.axKScan.ScanDensity = (short)enumKGContants.KGDENSITY7;
                    break;
                case 8:
                    ctrl.axKScan.ScanDensity = (short)enumKGContants.KGDENSITY8;
                    break;
                default:
                    break;
            }

            return ctrl;
        }

        public static KScanAxControl SetContrast(this KScanAxControl ctrl, int value) => ctrl;
    }
}
