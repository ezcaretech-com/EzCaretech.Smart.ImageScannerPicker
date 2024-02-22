using Leadtools;
using Leadtools.Codecs;
using Leadtools.Twain;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace ImageScannerPicker.LeadtoolsTwain
{
    public class LeadtoolsTwainScanner : IImageScannerPlugin
    {
        public string Name => "LeadtoolsTwainScanner";

        public string Description => "LEADTOOLS provides everything needed to control any TWAIN scanner, digital camera, or capture card and is an important component of many business workflows. With LEADTOOLS, .NET 6+, .NET Framework, C++ Class Library, C#, VB, C/C++, HTML / JavaScript, and Python developers can use TWAIN to capture images for OCR, barcode, forms recognition, image processing, annotation, and more. High-level acquisition functions are included for ease of use while low-level functionality is provided for flexibility and control in even the most demanding scanning applications.";

        public IEnumerable<DeviceMethod> DeviceMethods => throw new NotImplementedException(nameof(DeviceMethods));

        public IEnumerable<ColorSet> ColorSets => throw new NotImplementedException(nameof(ColorSets));

        public IEnumerable<Feeder> Feeders => throw new NotImplementedException(nameof(Feeders));

        public IEnumerable<Duplex> Duplexes => throw new NotImplementedException(nameof(Duplexes));

        public IEnumerable<PaperSize> PaperSizes => throw new NotImplementedException(nameof(PaperSizes));

        public IEnumerable<RotateDegree> RotateDegrees => throw new NotImplementedException(nameof(RotateDegrees));

        public IEnumerable<Resolution> Resolutions => throw new NotImplementedException(nameof(Resolutions));

        public IEnumerable<int> Brightnesses => throw new NotImplementedException(nameof(Brightnesses));

        public IEnumerable<int> Contrasts => throw new NotImplementedException(nameof(Contrasts));

        public bool IsDataSourceOpened => !string.IsNullOrEmpty(_twainSession.SelectedSourceName());

        public string SelectedDataSourceName
        {
            get
            {
                try
                {
                    return _twainSession.SelectedSourceName();
                }
                catch
                {
                    return null;
                }
            }
        }

        private readonly ImageScannerConfig _config;

        private TwainSession _twainSession;

        private RasterCodecs _codecs;

        public TwainTransferMechanism _transferMode = TwainTransferMechanism.Native;

        bool _twainAvailable = false;

        bool _cleanupAfterAcquire = false;

        int _scanCount;

        public LeadtoolsTwainScanner(ImageScannerConfig config)
        {
            _config = config;
            LoadLicense();
            LoadLibrary();
        }

        private void LoadLicense()
        {
            string dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string licenseFileRelativePath = Path.Combine(dir, "LEADTOOLS.LIC");
            string keyFileRelativePath = Path.Combine(dir, "LEADTOOLS.LIC.key");

            if (File.Exists(licenseFileRelativePath) && File.Exists(keyFileRelativePath))
            {
                string developerKey = File.ReadAllText(keyFileRelativePath);
                try
                {
                    RasterSupport.SetLicense(licenseFileRelativePath, developerKey);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.Write(ex.Message);
                }
            }
        }

        private void LoadLibrary()
        {
            _codecs = new RasterCodecs();

            // Determine whether a TWAIN source is installed
            _twainAvailable = TwainSession.IsAvailable(_config.WindowHandle);
            if (_twainAvailable)
            {
                try
                {
                    // Construct a new TwainSession object with default values
                    _twainSession = new TwainSession();
                    // Initialize the TWAIN session 
                    // This method must be called before calling any other methods that require a TWAIN session
                    //For 32-bit driver support in 64-bit applications, use the following TWAIN initialization method instead:
                    _twainSession.Startup(_config.WindowHandle, "LEAD Technologies, Inc.", "LEAD Test Applications", "Version 1.0", "TWAIN Test Application", TwainStartupFlags.UseThunkServer);

                    //_twainSession.Startup(_config.WindowHandle, "LEAD Technologies, Inc.", "LEAD Test Applications", "Version 1.0", "TWAIN Test Application", TwainStartupFlags.None);
                }
                catch (TwainException ex)
                {
                    if (ex.Code == TwainExceptionCode.InvalidDll)
                    {
                        //_miFileAcquire.Enabled = false;
                        //_miFileAcquireCleanup.Enabled = false;
                        //_miFileSelectSource.Enabled = false;
                        //Messager.ShowError(this, "You have an old version of TWAINDSM.DLL. Please download latest version of this DLL from www.twain.org");
                    }
                    else
                    {
                        //_miFileAcquire.Enabled = false;
                        //_miFileAcquireCleanup.Enabled = false;
                        //_miFileSelectSource.Enabled = false;
                        //Messager.ShowError(this, ex);
                    }
                }
                catch (Exception ex)
                {
                    //Messager.ShowError(this, ex);
                    //_miFileAcquire.Enabled = false;
                    //_miFileAcquireCleanup.Enabled = false;
                    //_miFileSelectSource.Enabled = false;
                }
            }
            else
            {
                //_miFileAcquire.Enabled = false;
                //_miFileAcquireCleanup.Enabled = false;
                //_miFileSelectSource.Enabled = false;
            }
        }

        public void ShowSettingUI()
        {
            throw new NotImplementedException(nameof(ShowSettingUI));
        }

        public void ShowSourceSelector()
        {
            // Display the TWAIN dialog box to be used to select a TWAIN source for acquiring images
            _ = _twainSession.SelectSource(string.Empty);
        }

        public void SetDataSource(string dataSourceName)
        {
            _ = _twainSession.SelectSource(dataSourceName);
        }

        public IEnumerable<string> DataSourceList()
        {
            //_twainSession.Source
            return new List<string>();
            //throw new NotImplementedException();
        }

        public void Scan(ScanOptions options)
        {
            try
            {
                //    SetTransferMode();

                //if (!CheckKnown3rdPartyTwainIssues(_twainSession.SelectedSourceName()))
                //    return;

                _config.WillBatchDelegate?.Invoke();

                // Add the Acquire page event.
                _twainSession.AcquirePage += new EventHandler<TwainAcquirePageEventArgs>(_twain_AcquirePage);
                // Acquire pages

                TwainUserInterfaceFlags flags = options.IsShowUI
                    ? TwainUserInterfaceFlags.Show | TwainUserInterfaceFlags.Modal
                    : TwainUserInterfaceFlags.None;

                // Acquire one or more images from a TWAIN source.
                _twainSession.Acquire(flags);
                // Remove the Acquire page event.
                _twainSession.AcquirePage -= new EventHandler<TwainAcquirePageEventArgs>(_twain_AcquirePage);
                _config.DidBatchDelegate?.Invoke();
            }
            catch (Exception ex)
            {
                _config.ErrorDelegate?.Invoke(ex);
            }
        }

        private void SetTransferMode()
        {
            using (TwainCapability twnCap = new TwainCapability())
            {
                twnCap.Information.Type = TwainCapabilityType.ImageTransferMechanism;
                twnCap.Information.ContainerType = TwainContainerType.OneValue;

                twnCap.OneValueCapability.ItemType = TwainItemType.Uint16;
                twnCap.OneValueCapability.Value = (ushort)_transferMode;

                // Set the value of ICAP_XFERMECH (Image Transfer Mechanism) capability
                _twainSession.SetCapability(twnCap, TwainSetCapabilityMode.Set);
            }
        }

        public static bool CheckKnown3rdPartyTwainIssues(string sourceName)
        {
            bool thirdPartyTwainWithKnownProblem = false;
            bool continueScan = true;

            // The TWAIN2 FreeImage Software Scanner 64-bit has a problem when running under .NET 4.5 or later, check
            const string twain2FreeImageSourceName = "TWAIN2 FreeImage Software Scanner";
            if (sourceName == twain2FreeImageSourceName && IntPtr.Size == 8)
            {
                // Check if we are running under .NET 4.5 or later
                var targetFrameworks = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(System.Runtime.Versioning.TargetFrameworkAttribute), false);
                if (targetFrameworks != null && targetFrameworks[0] != null)
                {
                    var attr = targetFrameworks[0] as System.Runtime.Versioning.TargetFrameworkAttribute;
                    if (attr != null && !attr.FrameworkName.Contains("v4.0"))
                    {
                        thirdPartyTwainWithKnownProblem = true;
                    }
                }
            }

            if (thirdPartyTwainWithKnownProblem)
            {
                string message = "The 64-bit TWAIN Free Image Scanner virtual TWAIN driver has known compatibility issues with .NET 4.5 and above.\n" +
                                 "If you are using this TWAIN driver as a source with our LEADTOOLS TWAIN SDK and are having any issues, you will need to upgrade the FreeImagex64.dll that is included with the driver to v3.18.0.\n" +
                                 "Another option is to change the target .NET Framework from 4.5 to 4.0 or lower.\n" +
                                 "For more information see: https://www.leadtools.com/support/forum/posts/t12411-";
                //DialogResult ret = MessageBox.Show(window, message, "Warning", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
                //if (ret == DialogResult.Cancel)
                //{
                continueScan = false;
                //}
            }

            return continueScan;
        }

        public void Dispose()
        {
            _twainSession.Shutdown();
        }

        private void _twain_AcquirePage(object sender, TwainAcquirePageEventArgs e)
        {
            _config.WillPageScanDelegate?.Invoke();

            string tempFilePath = Path.GetTempFileName();
            RasterImageFormat format = RasterImageFormat.Png;

            using (RasterCodecs codecs = new RasterCodecs())
            {
                codecs.Save(e.Image, tempFilePath, format, 0);
            }

            string outputFilePath = Path.Combine(
                Path.GetDirectoryName(tempFilePath),
                $"{Path.GetFileNameWithoutExtension(tempFilePath)}.{format.ToString().ToLower()}");

            File.Move(tempFilePath, outputFilePath);

            _config.DidPageScanDelegate?.Invoke();
            _config.DonePageScanDelegate?.Invoke(outputFilePath);
        }
    }
}
