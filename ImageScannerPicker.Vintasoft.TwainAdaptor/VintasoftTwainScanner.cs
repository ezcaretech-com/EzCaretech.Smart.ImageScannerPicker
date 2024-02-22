using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;
using Vintasoft.WpfTwain;

namespace ImageScannerPicker.Vintasoft.TwainAdaptor
{
    public class VintasoftTwainScanner : IImageScannerPlugin, IDisposable
    {
        public string Name => "Vintasoft TWAIN Scanner";

        public string Description => "Provides functionality for scanning images using Vintasoft TWAIN.";

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
                Duplex.BOTH
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
                Resolution.D100,
                Resolution.D150,
                Resolution.D200,
                Resolution.D300,
                Resolution.D600,
            };

        public IEnumerable<int> Brightnesses => Enumerable.Range(-1000, 2001);

        public IEnumerable<int> Contrasts => Enumerable.Range(-1000, 2001);

        public bool IsDataSourceOpened => !string.IsNullOrEmpty(_deviceManager.DefaultDevice.ToString());

        public string SelectedDataSourceName => _deviceManager.DefaultDevice.ToString();

        private readonly ImageScannerConfig _config;
        // 나머지 프로퍼티 구현은 여러분의 요구사항에 맞게 구현해야 합니다.
        // 예제에서는 NotImplementedException을 반환합니다.

        private DeviceManager _deviceManager;
        private Device _device;
        private Bitmap bit = new Bitmap(200, 200);
        private string _dataSourceName = string.Empty;

        /// <summary>
        /// Indicates that device is acquiring image(s).
        /// </summary>
        private bool _isImageAcquiring;

        /// <summary>
        /// Acquired image collection.
        /// </summary>
        private AcquiredImageCollection _images = new AcquiredImageCollection();

        /// <summary>
        /// Determines that image acquistion must be canceled because application's window is closing.
        /// </summary>
        private bool _cancelTransferBecauseWindowIsClosing;

        // get country and language for TWAIN device manager


        public VintasoftTwainScanner(ImageScannerConfig config)
        {
            _config = config;
            InitializeScanner();
        }

        private void InitializeScanner()
        {
            // create TWAIN device manager
            TwainGlobalSettings.Register("JUNSEOK CHA", "cjs3750@ezcaretech.com", "2024-03-20", _config.License);
            GetCountryAndLanguage(out CountryCode country, out LanguageType language);
            _deviceManager = new DeviceManager(_config.WindowHandle, country, language);
        }

        /// <summary>
        /// Returns country and language for TWAIN device manager.
        /// </summary>
        /// <remarks>
        /// Unfortunately only KODAK scanners allow to set country and language.
        /// </remarks>
        private void GetCountryAndLanguage(out CountryCode country, out LanguageType language)
        {
            country = CountryCode.Usa;
            language = LanguageType.EnglishUsa;

            switch (CultureInfo.CurrentUICulture.Parent.IetfLanguageTag)
            {
                case "de":
                    country = CountryCode.Germany;
                    language = LanguageType.German;
                    break;

                case "es":
                    country = CountryCode.Spain;
                    language = LanguageType.Spanish;
                    break;

                case "fr":
                    country = CountryCode.France;
                    language = LanguageType.French;
                    break;

                case "it":
                    country = CountryCode.Italy;
                    language = LanguageType.Italian;
                    break;

                case "pt":
                    country = CountryCode.Portugal;
                    language = LanguageType.Portuguese;
                    break;

                case "ru":
                    country = CountryCode.Russia;
                    language = LanguageType.Russian;
                    break;
            }
        }

        public IEnumerable<string> DataSourceList()
        {
            try
            {
                var dataSourceNames = new List<string>();

                _deviceManager.IsTwain2Compatible = true;
                _deviceManager.Open();

                DeviceInfo deviceInfo;
                DeviceCollection devices = _deviceManager.Devices;

                // 조회된 디바이스 목록을 리스트에 추가
                for (int i = 0; i < devices.Count; i++)
                {
                    deviceInfo = devices[i].Info;
                    dataSourceNames.Add(deviceInfo.ProductName);
                }

                return dataSourceNames;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                // 디바이스 관리자 닫기
                _deviceManager.Close();
            }
        }

        public void Scan(ScanOptions options)
        {
            _config.WillBatchDelegate?.Invoke();
            // specify that image acquisition is started
            _isImageAcquiring = true;

            try
            {
                _device.Open();

                if (_deviceManager.Devices.Count == 0)
                {
                    throw new Exception("Devices are not found.");
                }

                _device = _deviceManager.Devices.Find(_dataSourceName);
                _device.TransferMode = TransferMode.Memory;

                _device.ShowUI = options.IsShowUI;
                _device.ModalUI = false;
                _device.ShowIndicators = true;
                _device.DisableAfterAcquire = false;

                SetColorSet(options.ColorSet);
                SetResolution(options.Resolution);
                SetFeeder(options.Feeder);
                SetDuplex(options.Duplex);
                SetPaperSize(options.PaperSize);
                SetBrightness(options.Brightness);
                SetContrast(options.Contrast);

                _config.DidBatchDelegate?.Invoke();

                AcquireModalState acquireModalState;
                try
                {
                    do
                    {
                        acquireModalState = _device.AcquireModal();

                        switch (acquireModalState)
                        {
                            case AcquireModalState.ImageAcquired:

                                // 임시 파일 경로 생성
                                string tempFilePath = Path.GetTempFileName();

                                PngBitmapEncoder encoder = new PngBitmapEncoder();
                                encoder.Frames.Add(BitmapFrame.Create(_device.AcquiredImage.GetAsBitmapSource()));

                                // 파일에 저장
                                using (var fileStream = new FileStream(tempFilePath, FileMode.Create))
                                {
                                    encoder.Save(fileStream);
                                }

                                // 최종 파일 경로 생성 (.png 확장자 사용)
                                string outputFilePath = Path.Combine(
                                    Path.GetDirectoryName(tempFilePath),
                                    $"{Path.GetFileNameWithoutExtension(tempFilePath)}.png");

                                // 임시 파일을 최종 파일 경로로 이동
                                File.Move(tempFilePath, outputFilePath);

                                _config.DonePageScanDelegate?.Invoke(outputFilePath);

                                _device.AcquiredImage.Dispose();
                                break;
                        }
                    }
                    while (acquireModalState != AcquireModalState.None);
                }
                catch (TwainException exd) { }


                // close the device
                _device.Close();

                // specify that image acquisition is finished
                _isImageAcquiring = false;
            }
            catch (Exception ex)
            {
                throw new Exception($"Scanning failed: {ex.Message}", ex);
            }
            finally
            {
                // _deviceManager.Close();
            }
        }

        private void SetColorSet(ColorSet value)
        {
            PixelType pixelType = PixelType.BW;

            switch (value)
            {
                case ColorSet.BLACKWHITE:
                    pixelType = PixelType.BW;
                    break;
                case ColorSet.GRAYSCALE:
                    pixelType = PixelType.Gray;
                    break;
                case ColorSet.COLOR:
                    pixelType = PixelType.RGB;
                    break;
            }

            if (_device.PixelType != pixelType)
                _device.PixelType = pixelType;
        }

        private void SetResolution(Resolution resolution)
        {

            switch (resolution)
            {
                case Resolution.D150:
                    _device.SetResolution(100, 100);
                    break;
                case Resolution.D200:
                    _device.SetResolution(200, 200);
                    break;
                case Resolution.D300:
                    _device.SetResolution(300, 300);
                    break;
                case Resolution.D600:
                    _device.SetResolution(600, 600);
                    break;
            }
        }

        private void SetFeeder(Feeder feeder)
        {
            switch (feeder)
            {
                case Feeder.ADF:
                    _device.DocumentFeeder.Enabled = true;
                    break;
                case Feeder.FLATBED:
                    _device.DocumentFeeder.Enabled = false;
                    break;
            }
        }

        private void SetDuplex(Duplex duplex)
        {
            switch (duplex)
            {
                case Duplex.SINGLE:
                    _device.DocumentFeeder.DuplexEnabled = false;
                    break;
                case Duplex.BOTH:
                    _device.DocumentFeeder.DuplexEnabled = true;
                    break;
            }
        }

        private void SetPaperSize(PaperSize paperSize)
        {
            switch (paperSize)
            {
                case PaperSize.A3:
                    _device.PageSize = PageSize.A3;
                    break;
                case PaperSize.A4:
                    _device.PageSize = PageSize.A4;
                    break;
                case PaperSize.B3:
                    _device.PageSize = PageSize.B3;
                    break;
                case PaperSize.B4:
                    _device.PageSize = PageSize.B4;
                    break;
                case PaperSize.B5:
                    _device.PageSize = PageSize.B5;
                    break;
                case PaperSize.LETTER:
                    _device.PageSize = PageSize.USLETTER;
                    break;
                case PaperSize.BUSINESSCARD:
                    _device.PageSize = PageSize.BUSINESSCARD;
                    break;
            }
        }

        private void SetBrightness(int brightness) => _device.Brightness = (float)brightness;


        private void SetContrast(int contrast) => _device.Contrast = (float)contrast;


        private void _device_ImageAcquiringProgress(object sender, ImageAcquiringProgressEventArgs e)
        {
            // image acquistion must be canceled because application's window is closing
            if (_cancelTransferBecauseWindowIsClosing)
            {
                // cancel image acquisition
                _device.CancelTransfer();
                return;
            }
        }

        private void _device_ImageAcquired(object sender, ImageAcquiredEventArgs e)
        {
            // image acquistion must be canceled because application's window is closing
            if (_cancelTransferBecauseWindowIsClosing)
            {
                // cancel image acquisition
                _device.CancelTransfer();
                return;
            }

            _images.Add(e.Image);
        }

        private void device_ScanFinished(object sender, EventArgs e)
        {
            // close the device
            _device.Close();

            // specify that image acquisition is finished
            _isImageAcquiring = false;
        }

        public void SetDataSource(string dataSourceName)
        {
            DeviceCollection devices = _deviceManager.Devices;

            // for each device
            for (int i = 0; i < devices.Count; i++)
            {
                // if device is default device
                if (devices[i] == _deviceManager.DefaultDevice)
                {
                    // close the device manager
                    if (_deviceManager.State == DeviceManagerState.Opened)
                        _deviceManager.Close();

                    _deviceManager.IsTwain2Compatible = true;

                    // if 32-bit devices must be used
                    if (!_deviceManager.Are32BitDevicesUsed)
                        _deviceManager.Use32BitDevices();

                    _deviceManager.Open();

                    Device device = _deviceManager.Devices.Find(dataSourceName);
                    _dataSourceName = dataSourceName;
                    _device = device;

                }
            }

        }

        public void ShowSettingUI()
        {
            // 설정 UI를 표시하는 로직 구현
            throw new NotImplementedException();
        }

        public void ShowSourceSelector()
        {
            // 소스 선택기를 표시하는 로직 구현
            throw new NotImplementedException();
        }

        #region IDisposable Support
        private bool _disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // 관리되는 상태(관리되는 객체)를 삭제합니다.
                    _device?.Close();
                    _deviceManager?.Close();
                }

                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
