using NTwain;
using NTwain.Data;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace ImageScannerPicker.Adaptor
{
    /// <summary>
    /// https://github.com/soukoku/ntwain
    /// </summary>
    public class NTwainScanner : IImageScannerPlugin
    {
        public string Name => "NTwainScanner";

        public string Description => "This is a library created to make working with TWAIN interface possible in dotnet.";

        public IEnumerable<DeviceMethod> DeviceMethods => throw new NotImplementedException();

        public IEnumerable<ColorSet> ColorSets
        {
            get
            {
                if (_twainSession.CurrentSource == null)
                    throw new ArgumentNullException(nameof(_twainSession.CurrentSource));
                var capability = _twainSession.CurrentSource?.Capabilities.ICapPixelType;
                if (!capability.IsSupported) throw new NotSupportedException(nameof(capability));
                IEnumerable<PixelType> pixelTypes = capability.GetValues();
                var values = new List<ColorSet>();

                if (pixelTypes.Contains(PixelType.BlackWhite)) values.Add(ColorSet.BLACKWHITE);
                if (pixelTypes.Contains(PixelType.Gray)) values.Add(ColorSet.GRAYSCALE);
                if (pixelTypes.Contains(PixelType.RGB)) values.Add(ColorSet.COLOR);

                return values;
            }
        }

        public IEnumerable<Feeder> Feeders
        {
            get
            {
                if (_twainSession.CurrentSource == null)
                    throw new ArgumentNullException(nameof(_twainSession.CurrentSource));
                var capability = _twainSession.CurrentSource?.Capabilities.CapFeederEnabled;
                if (!capability.IsSupported) throw new NotSupportedException(nameof(capability));
                return new List<Feeder>() { Feeder.ADF, Feeder.FLATBED };
            }
        }

        public IEnumerable<Duplex> Duplexes
        {
            get
            {
                if (_twainSession.CurrentSource == null)
                    throw new ArgumentNullException(nameof(_twainSession.CurrentSource));
                var capability = _twainSession.CurrentSource?.Capabilities.CapDuplexEnabled;
                if (!capability.IsSupported) throw new NotSupportedException(nameof(capability));
                return new List<Duplex>() { Duplex.SINGLE, Duplex.BOTH };
            }
        }

        public IEnumerable<PaperSize> PaperSizes
        {
            get
            {
                if (_twainSession.CurrentSource == null)
                    throw new ArgumentNullException(nameof(_twainSession.CurrentSource));
                var capability = _twainSession.CurrentSource?.Capabilities.ICapSupportedSizes;
                if (!capability.IsSupported) throw new NotSupportedException(nameof(capability));
                IEnumerable<SupportedSize> supportedSizes = capability.GetValues();
                var values = new List<PaperSize>();

                if (supportedSizes.Contains(SupportedSize.A3)) values.Add(PaperSize.A3);
                if (supportedSizes.Contains(SupportedSize.A4)) values.Add(PaperSize.A4);
                if (supportedSizes.Contains(SupportedSize.IsoB3)) values.Add(PaperSize.B3);
                if (supportedSizes.Contains(SupportedSize.IsoB4)) values.Add(PaperSize.B4);
                if (supportedSizes.Contains(SupportedSize.IsoB5)) values.Add(PaperSize.B5);
                if (supportedSizes.Contains(SupportedSize.USLetter)) values.Add(PaperSize.LETTER);
                if (supportedSizes.Contains(SupportedSize.BusinessCard)) values.Add(PaperSize.BUSINESSCARD);

                return values;
            }
        }

        public IEnumerable<RotateDegree> RotateDegrees => throw new NotImplementedException();

        public IEnumerable<Resolution> Resolutions
        {
            get
            {
                if (_twainSession.CurrentSource == null)
                    throw new ArgumentNullException(nameof(_twainSession.CurrentSource));
                var capability = _twainSession.CurrentSource?.Capabilities.ICapXResolution;
                if (!capability.IsSupported) throw new NotSupportedException(nameof(capability));
                return capability.GetValues()
                    .Select(v => (Resolution)Enum.Parse(typeof(Resolution), v.ToString()));
            }
        }

        public IEnumerable<int> Brightnesses
        {
            get
            {
                if (_twainSession.CurrentSource == null)
                    throw new ArgumentNullException(nameof(_twainSession.CurrentSource));
                var capability = _twainSession.CurrentSource?.Capabilities.ICapBrightness;
                if (!capability.IsSupported) throw new NotSupportedException(nameof(capability));
                return capability.GetValues().Select(v => int.Parse(v.ToString()));
            }
        }

        public IEnumerable<int> Contrasts
        {
            get
            {
                if (_twainSession.CurrentSource == null)
                    throw new ArgumentNullException(nameof(_twainSession.CurrentSource));
                var capability = _twainSession.CurrentSource?.Capabilities.ICapContrast;
                if (!capability.IsSupported) throw new NotSupportedException(nameof(capability));
                return capability.GetValues().Select(v => int.Parse(v.ToString()));
            }
        }

        private readonly ImageScannerConfig _config;

        private readonly TwainSession _twainSession;

        private readonly List<string> _tempPaths = new List<string>();

        public event EventHandler<StateChangedArgs> StateChanged;
        public event EventHandler<EventArgs> ScanCompleted;

        private int _state;

        public int State
        {
            get => _state = _twainSession.State;
            private set => _state = value;
        }

        public NTwainScanner(ImageScannerConfig config)
        {
            _config = config;
            var appId = TWIdentity.CreateFromAssembly(DataGroups.Image | DataGroups.Audio, Assembly.GetExecutingAssembly());
            _twainSession = new TwainSession(appId);

            PlatformInfo.Current.PreferNewDSM = false;

            _twainSession.TransferReady += OnTransferReady;
            _twainSession.DataTransferred += OnDataTransferred;
            _twainSession.TransferCanceled += OnTransferCanceled;
            _twainSession.StateChanged += OnStateChanged;

            if (_twainSession.Open() != ReturnCode.Success)
                throw new InvalidProgramException("Erreur de l'ouverture de la session");
        }

        public bool IsDataSourceOpened =>
            _twainSession.CurrentSource != null;

        public string SelectedDataSourceName =>
            _twainSession.CurrentSource.Name;

        public void ShowSourceSelector()
        {
            if (State == 4)
            {
                _twainSession.CurrentSource.Close();
            }

            DataSource ds = _twainSession.ShowSourceSelector();
            ds?.Open();
            ds?.Capabilities.ICapXferMech.SetValue(XferMech.File);
        }

        public void ShowSettingUI()
        {
            if (State == 3) ShowSourceSelector();
            if (State != 4) return;

            _twainSession.CurrentSource.Enable(SourceEnableMode.ShowUIOnly, true, _config.WindowHandle);
        }

        public void SetDataSource(string dataSourceName)
        {
            if (State == 4)
            {
                _twainSession.CurrentSource.Close();
            }

            DataSource ds = _twainSession.GetSources().FirstOrDefault(x => x.Name == dataSourceName);
            ds?.Open();
            ds?.Capabilities.ICapXferMech.SetValue(XferMech.File);
        }

        public IEnumerable<string> DataSourceList() =>
            _twainSession.GetSources().Select(s => s.Name);

        public void Scan(ScanOptions options)
        {
            if (State != 4) return;

            _config.WillBatchDelegate?.Invoke();

            SetColorSet(options.ColorSet);
            SetFeeder(options.Feeder);
            SetDuplex(options.Duplex);
            SetPaperSize(options.PaperSize);
            SetResolution(options.Resolution);
            SetBrightness(options.Brightness);
            SetContrast(options.Contrast);
            SetAutoDiscardBlankPages(true);

            var tcs = new TaskCompletionSource<List<string>>();

            var fileResult = string.Empty;
            _tempPaths.Clear();

            ScanCompleted = (sender, e) =>
            {
                _tempPaths.Clear();
                _config.DidBatchDelegate?.Invoke();

                tcs.TrySetResult(_tempPaths);
            };

            _twainSession.TransferError += (sender, e) =>
            {
                tcs.TrySetException(e.Exception);
            };

            if (_twainSession.State == 4)
            {
                var mode = options.IsShowUI ? SourceEnableMode.ShowUI : SourceEnableMode.NoUI;

                _twainSession.CurrentSource.Enable(mode, true, _config.WindowHandle);
            }

            try
            {
                _ = tcs.Task.Result;
            }
            catch (Exception ex)
            {
                _config.ErrorDelegate?.Invoke(ex);
            }
        }

        public void Dispose()
        {
            _twainSession.CurrentSource?.Close();
            _twainSession.Close();
        }

        #region Set capabilities

        private void SetColorSet(ColorSet colorSet)
        {
            PixelType value;
            switch (colorSet)
            {
                case ColorSet.BLACKWHITE: value = PixelType.BlackWhite; break;
                case ColorSet.GRAYSCALE: value = PixelType.Gray; break;
                default: value = PixelType.RGB; break;
            }

            _twainSession.CurrentSource?.Capabilities
                .ICapPixelType.SetValue(value);
        }

        private void SetFeeder(Feeder feeder)
        {
            BoolType value = feeder == Feeder.ADF ? BoolType.True : BoolType.False;
            _twainSession.CurrentSource?.Capabilities
                .CapFeederEnabled.SetValue(value);
        }

        private void SetDuplex(Duplex duplex)
        {
            BoolType value = duplex == Duplex.BOTH ? BoolType.True : BoolType.False;
            _twainSession.CurrentSource?.Capabilities
                .CapDuplexEnabled.SetValue(value);
        }

        private void SetPaperSize(PaperSize paperSize)
        {
            SupportedSize value;
            switch (paperSize)
            {
                case PaperSize.A3: value = SupportedSize.A3; break;
                case PaperSize.A4: value = SupportedSize.A4; break;
                case PaperSize.B3: value = SupportedSize.IsoB3; break;
                case PaperSize.B4: value = SupportedSize.IsoB4; break;
                case PaperSize.B5: value = SupportedSize.IsoB5; break;
                case PaperSize.LETTER: value = SupportedSize.USLetter; break;
                case PaperSize.BUSINESSCARD: value = SupportedSize.BusinessCard; break;
                default: value = SupportedSize.None; break;
            }

            _twainSession.CurrentSource?.Capabilities
                .ICapSupportedSizes.SetValue(value);
        }

        private void SetResolution(Resolution resolution)
        {
            _twainSession.CurrentSource?.Capabilities
                .ICapXResolution.SetValue((int)resolution);

            _twainSession.CurrentSource?.Capabilities
                .ICapYResolution.SetValue((int)resolution);
        }

        private void SetBrightness(int brightness)
        {
            _twainSession.CurrentSource?.Capabilities
                .ICapBrightness.SetValue(brightness);
        }

        private void SetContrast(int contrast)
        {
            _twainSession.CurrentSource?.Capabilities
                .ICapContrast.SetValue(contrast);
        }

        private void SetAutoDiscardBlankPages(bool ifAutoDiscardBlankPages)
        {
            if (!ifAutoDiscardBlankPages) return;

            _twainSession.CurrentSource?.Capabilities
                .ICapAutoDiscardBlankPages.SetValue(BlankPage.Auto);
        }

        //Console.WriteLine($"ICapImageFileFormat = {string.Join("/", _twainSession.CurrentSource?.Capabilities.ICapImageFileFormat.GetValues())}");
        //Console.WriteLine($"ICapCompression = {string.Join("/", _twainSession.CurrentSource?.Capabilities.ICapCompression.GetValues())}");
        //Console.WriteLine($"CapAutoFeed = {string.Join("/", _twainSession.CurrentSource?.Capabilities.CapAutoFeed.GetValues())}");
        //Console.WriteLine($"ICapAutoSize = {string.Join("/", _twainSession.CurrentSource?.Capabilities.ICapAutoSize.IsSupported)}");
        //Console.WriteLine($"ICapAutoDiscardBlankPages = {string.Join("/", _twainSession.CurrentSource?.Capabilities.ICapAutoDiscardBlankPages.IsSupported)}");

        // 기울기 보정 : ICapAutomaticDeskew
        //ICapAutomaticLengthDetection
        // 자동 회전 : ICapAutomaticRotate
        //ICapSupportedSizes SupportedSize
        //ICapOrientation
        //ICapOverScan
        //ICapAutoBright
        //ICapAutoDiscardBlankPages

        #endregion Set capabilities

        #region Scan SDK Interface

        private void OnTransferReady(object sender, TransferReadyEventArgs e)
        {
            _config.WillPageScanDelegate?.Invoke();

            var mech = _twainSession.CurrentSource.Capabilities.ICapXferMech.GetCurrent();
            if (mech == XferMech.File)
            {
                var formats = _twainSession.CurrentSource.Capabilities.ICapImageFileFormat.GetValues();
                var wantFormat = formats.Contains(FileFormat.Tiff) ? FileFormat.Tiff : FileFormat.Bmp;
                Console.WriteLine(string.Join(", ", formats));

                var fileSetup = new TWSetupFileXfer
                {
                    Format = wantFormat,
                    FileName = Path.GetTempFileName(),
                };

                _ = _twainSession.CurrentSource.DGControl.SetupFileXfer.Set(fileSetup);
            }
        }

        private void OnDataTransferred(object sender, DataTransferredEventArgs e)
        {
            ImageFormat format = ImageFormat.Png;
            string outputFilePath = Path.Combine(
                Path.GetDirectoryName(e.FileDataPath),
                $"{Path.GetFileNameWithoutExtension(e.FileDataPath)}.{format.ToString().ToLower()}");

            using (Image image = Image.FromFile(e.FileDataPath))
            {
                image.Save(outputFilePath, format);
            }

            File.Delete(e.FileDataPath);
            _tempPaths.Add(outputFilePath);
            _config.DidPageScanDelegate?.Invoke();
            _config.DonePageScanDelegate?.Invoke(outputFilePath);
        }

        private void OnTransferCanceled(object sender, TransferCanceledEventArgs e) =>
            _config.DidBatchDelegate?.Invoke();

        private void OnStateChanged(object sender, EventArgs e)
        {
            if (State != 6 && _tempPaths.Count > 0)
            {
                ScanCompleted?.Invoke(sender, e);
            }

            State = _twainSession.State;
            StateChanged?.Invoke(this, new StateChangedArgs() { NewState = State });
        }

        #endregion
    }
}
