using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace ImageScannerPicker.TestApplication
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        private IImageScannerPlugin _selectedPlugin;
        private readonly List<string> _resultFiles = new List<string>();

        public MainWindow()
        {
            InitializeComponent();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            LoadPlugins();

            ResultList.ItemsSource = _resultFiles;
        }

        #region Windows Events

        private void PluginList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            OpenPlugin();
        }

        private void SetLicenseBtn_Click(object sender, RoutedEventArgs e)
        {
            OpenPlugin();
        }

        private void ScannerList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!(ScannerList.SelectedItem is string deviceName)) return;
            if (string.IsNullOrEmpty(deviceName)) return;

            _selectedPlugin.SetDataSource(deviceName);

            OpenScanner();
        }

        private void BrightnessSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            BrightnessLbl.Text = Convert.ToInt32(e.NewValue).ToString();
        }

        private void ContrastSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            ContrastLbl.Text = Convert.ToInt32(e.NewValue).ToString();
        }

        private void SelectSourceBtn_Click(object sender, RoutedEventArgs e)
        {
            ShowSourceSelector();
        }

        private void ShowSettingBtn_Click(object sender, RoutedEventArgs e)
        {
            ShowSettingUI();
        }

        private void StartScanBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!_selectedPlugin.IsDataSourceOpened)
                {
                    ShowSourceSelector();
                }

                if (!_selectedPlugin.IsDataSourceOpened)
                    return;

                Console.WriteLine($"Device : {_selectedPlugin.SelectedDataSourceName}");

                ScanOptions options = new ScanOptions();

                if (ScannerList.SelectedItem is string scannerName) options.DeviceName = scannerName;
                if (DeviceMethod.SelectedItem is OptionItem item1 && Enum.Parse(typeof(DeviceMethod), item1.Code) is DeviceMethod value1) options.DeviceMethod = value1;
                if (ColorSet.SelectedItem is OptionItem item2 && Enum.Parse(typeof(ColorSet), item2.Code) is ColorSet value2) options.ColorSet = value2;
                if (Feeder.SelectedItem is OptionItem item3 && Enum.Parse(typeof(Feeder), item3.Code) is Feeder value3) options.Feeder = value3;
                if (Duplex.SelectedItem is OptionItem item4 && Enum.Parse(typeof(Duplex), item4.Code) is Duplex value4) options.Duplex = value4;
                if (PaperSize.SelectedItem is OptionItem item5 && Enum.Parse(typeof(PaperSize), item5.Code) is PaperSize value5) options.PaperSize = value5;
                if (RotateDegree.SelectedItem is OptionItem item6 && Enum.Parse(typeof(RotateDegree), item6.Code) is RotateDegree value6) options.RotateDegree = value6;
                if (Resolution.SelectedItem is OptionItem item7 && Enum.Parse(typeof(Resolution), item7.Code) is Resolution value7) options.Resolution = value7;

                options.Brightness = Convert.ToInt32(BrightnessSlider.Value);
                options.Contrast = Convert.ToInt32(ContrastSlider.Value);
                options.IsShowUI = IfShowUI.IsChecked ?? false;

                _selectedPlugin.Scan(options);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private void ResultList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.Source is ListBox listBox && listBox.SelectedItem is string filePath)
            {
                FileInfo fi = new FileInfo(filePath);

                ResultFileImage.Source = new BitmapImage(new Uri(filePath));
                ResultFilePath.Text = fi.Name;
                ResultFileSize.Text = fi.Length.ToString("#,##0");
            }
        }

        private void ClearResultsBtn_Click(object sender, RoutedEventArgs e)
        {
            _resultFiles.Clear();
            ResultList.Items.Refresh();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Application.Current.Shutdown();
        }

        #endregion // End of Window Events

        #region Plugin Events

        private void OnWillBatch()
        {
            Console.WriteLine($"OnWillBatch invoked.");
        }

        private void OnWillPageScan()
        {
            Console.WriteLine($"OnWillPageScan invoked.");
        }

        private void OnDidPageScan()
        {
            Console.WriteLine($"OnDidPageScan invoked.");
        }

        private void OnDonePageScan(string filePath)
        {
            Console.WriteLine($"OnDonePageScan invoked: {filePath}");
            Dispatcher.BeginInvoke(new Action(() =>
            {
                _resultFiles.Add(filePath);
                ResultList.Items.Refresh();
                ResultList.SelectedIndex = ResultList.Items.Count - 1;
            }), DispatcherPriority.Normal);
        }

        private void OnDidBatch()
        {
            Console.WriteLine($"OnDidBatch invoked.");
        }

        private void OnError(Exception ex)
        {
            Console.WriteLine($"OnError invoked: {ex}");
        }

        #endregion

        private void LoadPlugins()
        {
            try
            {
                ImageScannerLoader loader = new ImageScannerLoader();
                loader.LoadPluginAssemblies(AppDomain.CurrentDomain.BaseDirectory);
                PluginList.ItemsSource = ImageScannerLoader.PlugIns;
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format("Plugins couldn't be loaded: {0}", ex.Message));
                Environment.Exit(0);
            }
        }

        private void InitPlugin(string pluginName)
        {
            try
            {
                _selectedPlugin?.Dispose();
                ScannerList.ItemsSource = null;

                ShowSettingBtn.IsEnabled = false;
                StartScanBtn.IsEnabled = false;

                ImageScannerConfig config = new ImageScannerConfig
                {
                    WindowHandle = new WindowInteropHelper(this).Handle,
                    License = LicenseTbx.Text,
                    WillBatchDelegate = OnWillBatch,
                    WillPageScanDelegate = OnWillPageScan,
                    DidPageScanDelegate = OnDidPageScan,
                    DonePageScanDelegate = OnDonePageScan,
                    DidBatchDelegate = OnDidBatch,
                    ErrorDelegate = OnError,
                };

                _selectedPlugin = ImageScannerLoader.GetPlugin(pluginName, config);

                SelectSourceBtn.IsEnabled = true;
            }
            catch
            {
                SelectSourceBtn.IsEnabled = false;
                return;
            }
        }

        private void OpenPlugin()
        {
            if (!(PluginList.SelectedItem is string pluginName)) return;
            if (string.IsNullOrEmpty(pluginName)) return;

            PluginName.Content = pluginName;
            InitPlugin(pluginName);
            InitScannerDevice();
        }

        private void ShowSourceSelector()
        {
            try
            {
                _selectedPlugin.ShowSourceSelector();

                if (_selectedPlugin.IsDataSourceOpened)
                {
                    ScannerList.SelectedItem = _selectedPlugin.SelectedDataSourceName;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private void ShowSettingUI() => _selectedPlugin.ShowSettingUI();

        private void OpenScanner()
        {
            InitDeviceMethod();
            InitColorSet();
            InitFeeder();
            InitDuplex();
            InitPaperSize();
            InitRotateDegree();
            InitResolution();
            InitBrightness();
            InitContrast();

            ShowSettingBtn.IsEnabled = true;
            StartScanBtn.IsEnabled = true;
        }

        private void InitScannerDevice()
        {
            ScannerList.ItemsSource = _selectedPlugin.DataSourceList();
        }

        private void InitDeviceMethod()
        {
            try
            {
                DeviceMethod.ItemsSource = _selectedPlugin.DeviceMethods
                    .Select(item => new OptionItem { Code = item.ToString(), Name = item.GetName() })
                    .ToList();
                DeviceMethod.IsEnabled = true;
            }
            catch
            {
                DeviceMethod.IsEnabled = false;
            }
        }

        private void InitColorSet()
        {
            try
            {
                var list = _selectedPlugin.ColorSets
                    .Select(item => new OptionItem { Code = item.ToString(), Name = item.GetName() })
                    .ToList();
                ColorSet.ItemsSource = list;
                ColorSet.SelectedItem = list.FirstOrDefault(x => x.Name == "Color");
                ColorSet.IsEnabled = true;
            }
            catch
            {
                ColorSet.IsEnabled = false;
            }
        }

        private void InitFeeder()
        {
            try
            {
                var list = _selectedPlugin.Feeders
                    .Select(item => new OptionItem { Code = item.ToString(), Name = item.GetName() })
                    .ToList();
                Feeder.ItemsSource = list;
                Feeder.SelectedItem = list.FirstOrDefault(x => x.Name == "ADF");
                Feeder.IsEnabled = true;
            }
            catch
            {
                Feeder.IsEnabled = false;
            }
        }

        private void InitDuplex()
        {
            try
            {
                var list = _selectedPlugin.Duplexes
                    .Select(item => new OptionItem { Code = item.ToString(), Name = item.GetName() })
                    .ToList();
                Duplex.ItemsSource = list;
                Duplex.SelectedItem = list.FirstOrDefault(x => x.Name == "Single");
                Duplex.IsEnabled = true;
            }
            catch
            {
                Duplex.IsEnabled = false;
            }
        }

        private void InitPaperSize()
        {
            try
            {
                var list = _selectedPlugin.PaperSizes
                    .Select(item => new OptionItem { Code = item.ToString(), Name = item.GetName() })
                    .ToList();
                PaperSize.ItemsSource = list;
                PaperSize.SelectedItem = list.FirstOrDefault(x => x.Name == "A4");
                PaperSize.IsEnabled = true;
            }
            catch
            {
                PaperSize.IsEnabled = false;
            }
        }

        private void InitRotateDegree()
        {
            try
            {
                RotateDegree.ItemsSource = _selectedPlugin.RotateDegrees
                    .Select(item => new OptionItem { Code = item.ToString(), Name = item.GetName() })
                    .ToList();
                RotateDegree.IsEnabled = true;
            }
            catch
            {
                RotateDegree.IsEnabled = false;
            }
        }

        private void InitResolution()
        {
            try
            {
                var list = _selectedPlugin.Resolutions
                    .Select(item => new OptionItem { Code = item.ToString(), Name = item.GetName() })
                    .ToList();
                Resolution.ItemsSource = list;
                Resolution.SelectedItem = list.FirstOrDefault(x => x.Name == "200");
                Resolution.IsEnabled = true;
            }
            catch
            {
                Resolution.IsEnabled = false;
            }
        }

        private void InitBrightness()
        {
            try
            {
                BrightnessLbl.Text = "0";
                BrightnessSlider.Value = 0;
                BrightnessSlider.Minimum = _selectedPlugin.Brightnesses.Min();
                BrightnessSlider.Maximum = _selectedPlugin.Brightnesses.Max();
                BrightnessSlider.SmallChange = 10;
                BrightnessSlider.LargeChange = 100;
                BrightnessSlider.IsEnabled = true;
            }
            catch
            {
                BrightnessSlider.IsEnabled = false;
            }
        }

        private void InitContrast()
        {
            try
            {
                ContrastLbl.Text = "0";
                ContrastSlider.Value = 0;
                ContrastSlider.Minimum = _selectedPlugin.Contrasts.Min();
                ContrastSlider.Maximum = _selectedPlugin.Contrasts.Max();
                ContrastSlider.SmallChange = 10;
                ContrastSlider.LargeChange = 100;
                ContrastSlider.IsEnabled = true;
            }
            catch
            {
                ContrastSlider.IsEnabled = false;
            }
        }
    }
}
