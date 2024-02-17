using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

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

            OpenScanner();
        }

        private void BrightnessSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            BrightnessLbl.Content = Convert.ToInt32(e.NewValue).ToString();
        }

        private void ContrastSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            ContrastLbl.Content = Convert.ToInt32(e.NewValue).ToString();
        }

        private void SelectScannerBtn_Click(object sender, RoutedEventArgs e)
        {
            OpenScannerPicker();
        }

        private void OptionBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void StartScanBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!_selectedPlugin.IsDeviceSelected)
                {
                    OpenScannerPicker();
                }

                if (!_selectedPlugin.IsDeviceSelected)
                    return;

                Console.WriteLine($"Device : {_selectedPlugin.GetDeviceNameSelected}");

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
                options.IsShowUI = TwainShowUI.IsChecked ?? false;

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
                ResultFilePath.Content = fi.Name;
                ResultFileSize.Content = fi.Length.ToString("#,##0");
            }
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
            _resultFiles.Clear();
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
            _resultFiles.Add(filePath);
            ResultList.Items.Refresh();
            ResultList.SelectedIndex = ResultList.Items.Count - 1;
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

                ImageScannerConfig config = new ImageScannerConfig
                {
                    License = LicenseTbx.Text,
                    WillBatchDelegate = OnWillBatch,
                    WillPageScanDelegate = OnWillPageScan,
                    DidPageScanDelegate = OnDidPageScan,
                    DonePageScanDelegate = OnDonePageScan,
                    DidBatchDelegate = OnDidBatch,
                    ErrorDelegate = OnError,
                };

                _selectedPlugin = ImageScannerLoader.GetPlugin(pluginName, config);

                SelectScannerBtn.IsEnabled = true;
            }
            catch
            {
                SelectScannerBtn.IsEnabled = false;
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

        private void OpenScannerPicker()
        {
            try
            {
                _selectedPlugin.OpenDeviceSettingWindow();

                if (_selectedPlugin.IsDeviceSelected)
                {
                    ScannerList.SelectedItem = _selectedPlugin.GetDeviceNameSelected;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

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

            OptionsStackPanel.IsEnabled = true;
            OptionBtn.IsEnabled = true;
            StartScanBtn.IsEnabled = true;
        }

        private void InitScannerDevice()
        {
            ScannerList.ItemsSource = _selectedPlugin.GetDeviceList();
        }

        private void InitDeviceMethod()
        {
            try
            {
                DeviceMethod.ItemsSource = _selectedPlugin.DeviceMethods
                    .Select(item => new OptionItem { Code = item.ToString(), Name = item.GetName() })
                    .ToList();
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
                BrightnessLbl.Content = "0";
                BrightnessSlider.Value = 0;
                BrightnessSlider.Minimum = _selectedPlugin.Brightnesses.Min();
                BrightnessSlider.Maximum = _selectedPlugin.Brightnesses.Max();
                BrightnessSlider.SmallChange = 10;
                BrightnessSlider.LargeChange = 100;
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
                ContrastLbl.Content = "0";
                ContrastSlider.Value = 0;
                ContrastSlider.Minimum = _selectedPlugin.Contrasts.Min();
                ContrastSlider.Maximum = _selectedPlugin.Contrasts.Max();
                ContrastSlider.SmallChange = 10;
                ContrastSlider.LargeChange = 100;
            }
            catch
            {
                ContrastSlider.IsEnabled = false;
            }
        }
    }
}
