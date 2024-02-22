using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ImageScannerPicker.DynamicWebTwain
{
    /// <summary>
    /// https://www.dynamsoft.com/web-twain/overview/
    /// </summary>
    public class DynamicWebTwainScanner : IImageScannerPlugin
    {
        public string Name => "DynamicWebTwainScanner";

        public string Description => "Browser-Based Document Scanning SDK to Rapidly Deploy Your Web Applications";

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

        public IEnumerable<int> Brightnesses => throw new NotImplementedException();

        public IEnumerable<int> Contrasts => throw new NotImplementedException();

        public bool IsDataSourceOpened => _selectedDevice != null;

        public string SelectedDataSourceName =>
            _selectedDevice.ContainsKey("name") ? _selectedDevice["name"].ToString() : null;

        private readonly ImageScannerConfig _config;

        private readonly string _host;

        private readonly ScannerController _controller;

        private List<Dictionary<string, object>> _devices = new List<Dictionary<string, object>>();

        private Dictionary<string, object> _selectedDevice;

        public DynamicWebTwainScanner(ImageScannerConfig config)
        {
            _config = config;
            _host = "http://localhost:18622";
            _controller = new ScannerController();
        }

        public IEnumerable<string> DataSourceList()
        {
            try
            {
                Task<List<Dictionary<string, object>>> getDevicesTask = Task.Run(async () =>
                {
                    return await _controller.GetDevices(_host, 48);
                });
                getDevicesTask.Wait();
                _devices = getDevicesTask.Result;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            return _devices.Select(d => d.ContainsKey("name") ? d["name"].ToString() : null);
        }

        public void Dispose()
        {
        }

        public void Scan(ScanOptions options)
        {
            var parameters = new Dictionary<string, object>
            {
                { "license", _config.License },
                { "device", _selectedDevice["device"] },
            };

            parameters["config"] = new Dictionary<string, object>
            {
                { "IfShowUI", options.IsShowUI },
                { "PixelType", ConvertFromColorSet(options.ColorSet) },
                { "PageSize", ConvertFromPaperSize(options.PaperSize) },
                { "Resolution", options.Resolution },
                { "IfFeederEnabled", options.Feeder.Equals(Feeder.ADF) },
                { "IfDuplexEnabled", options.Duplex.Equals(Duplex.BOTH) },
            };

            _config.WillBatchDelegate?.Invoke();

            try
            {
                Task<string> scanDocumentTask = Task.Run(async () => await _controller.ScanDocument(_host, parameters));
                scanDocumentTask.Wait();
                string jobId = scanDocumentTask.Result;
                Console.WriteLine($"jobId : {jobId}");

                if (!string.IsNullOrEmpty(jobId))
                {
                    Task<List<string>> getImageFilesTask = Task.Run(async () =>
                    {
                        return await _controller.GetImageFiles(_host, jobId, "./");
                    });

                    getImageFilesTask.Wait();
                    List<string> images = getImageFilesTask.Result;

                    for (int i = 0; i < images.Count; i++)
                    {
                        _config.WillPageScanDelegate?.Invoke();

                        ImageFormat format = ImageFormat.Png;
                        string tempFilePath = images[i];
                        string outputFilePath = Path.Combine(
                            Path.GetDirectoryName(tempFilePath),
                            $"{Path.GetFileNameWithoutExtension(tempFilePath)}.{format.ToString().ToLower()}");

                        File.Move(tempFilePath, outputFilePath);

                        Console.WriteLine($"Image {i}: {outputFilePath}");

                        _config.DidPageScanDelegate?.Invoke();

                        _config.DonePageScanDelegate?.Invoke(outputFilePath);
                    }

                    _controller.DeleteJob(_host, jobId);
                }
            }
            catch (Exception ex)
            {
                _config.ErrorDelegate?.Invoke(ex);
            }

            _config.DidBatchDelegate?.Invoke();
        }

        public void SetDataSource(string dataSourceName) =>
            _selectedDevice = _devices.FirstOrDefault(d => d["name"].ToString() == dataSourceName);

        public void ShowSettingUI()
        {
        }

        public void ShowSourceSelector()
        {
        }


        #region Set capabilities

        private TWICapPixelType ConvertFromColorSet(ColorSet value)
        {
            switch (value)
            {
                case ColorSet.BLACKWHITE:
                    return TWICapPixelType.TWPT_BW;
                case ColorSet.GRAYSCALE:
                    return TWICapPixelType.TWPT_GRAY;
                case ColorSet.COLOR:
                default:
                    return TWICapPixelType.TWPT_RGB;
            }
        }

        private TWICapSupportedSizes ConvertFromPaperSize(PaperSize value)
        {
            switch (value)
            {
                case PaperSize.A3:
                    return TWICapSupportedSizes.TWSS_A3;
                case PaperSize.A4:
                    return TWICapSupportedSizes.TWSS_A4;
                case PaperSize.B3:
                    return TWICapSupportedSizes.TWSS_B3;
                case PaperSize.B4:
                    return TWICapSupportedSizes.TWSS_B4;
                case PaperSize.B5:
                    return TWICapSupportedSizes.TWSS_B5LETTER;
                case PaperSize.LETTER:
                    return TWICapSupportedSizes.TWSS_USLETTER;
                case PaperSize.BUSINESSCARD:
                    return TWICapSupportedSizes.TWSS_BUSINESSCARD;
                default:
                    return TWICapSupportedSizes.TWSS_NONE;
            }
        }

        #endregion Set capabilities
    }


    internal enum TWICapPixelType
    {
        TWPT_BW = 0,
        TWPT_GRAY = 1,
        TWPT_RGB = 2,
        TWPT_PALETTE = 3,
        TWPT_CMY = 4,
        TWPT_CMYK = 5,
        TWPT_YUV = 6,
        TWPT_YUVK = 7,
        TWPT_CIEXYZ = 8,
        TWPT_LAB = 9,
        TWPT_SRGB = 10,
        TWPT_SCRGB = 11,
        TWPT_INFRARED = 16
    }

    internal enum TWICapSupportedSizes
    {
        TWSS_NONE = 0,
        TWSS_A4LETTER = 1,
        TWSS_B5LETTER = 2,
        TWSS_USLETTER = 3,
        TWSS_USLEGAL = 4,
        TWSS_A5 = 5,
        TWSS_B4 = 6,
        TWSS_B6 = 7,
        TWSS_USLEDGER = 9,
        TWSS_USEXECUTIVE = 10,
        TWSS_A3 = 11,
        TWSS_B3 = 12,
        TWSS_A6 = 13,
        TWSS_C4 = 14,
        TWSS_C5 = 15,
        TWSS_C6 = 16,
        TWSS_4A0 = 17,
        TWSS_2A0 = 18,
        TWSS_A0 = 19,
        TWSS_A1 = 20,
        TWSS_A2 = 21,
        TWSS_A4 = 1,
        TWSS_A7 = 22,
        TWSS_A8 = 23,
        TWSS_A9 = 24,
        TWSS_A10 = 25,
        TWSS_ISOB0 = 26,
        TWSS_ISOB1 = 27,
        TWSS_ISOB2 = 28,
        TWSS_ISOB3 = 12,
        TWSS_ISOB4 = 6,
        TWSS_ISOB5 = 29,
        TWSS_ISOB6 = 7,
        TWSS_ISOB7 = 30,
        TWSS_ISOB8 = 31,
        TWSS_ISOB9 = 32,
        TWSS_ISOB10 = 33,
        TWSS_JISB0 = 34,
        TWSS_JISB1 = 35,
        TWSS_JISB2 = 36,
        TWSS_JISB3 = 37,
        TWSS_JISB4 = 38,
        TWSS_JISB5 = 2,
        TWSS_JISB6 = 39,
        TWSS_JISB7 = 40,
        TWSS_JISB8 = 41,
        TWSS_JISB9 = 42,
        TWSS_JISB10 = 43,
        TWSS_C0 = 44,
        TWSS_C1 = 45,
        TWSS_C2 = 46,
        TWSS_C3 = 47,
        TWSS_C7 = 48,
        TWSS_C8 = 49,
        TWSS_C9 = 50,
        TWSS_C10 = 51,
        TWSS_USSTATEMENT = 52,
        TWSS_BUSINESSCARD = 53,
        TWSS_MAXSIZE = 54
    }
}
