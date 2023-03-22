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
        private readonly Delegates _delegates;
        private IImageScannerPlugin _kofaxPlugin;
        private IImageScannerPlugin _twainPlugin;
        private readonly List<string> _resultFiles = new List<string>();

        public MainWindow()
        {
            InitializeComponent();

            LoadPlugins();

            _delegates = new Delegates
            {
                WillBatchDelegate = OnWillBatch,
                WillPageScanDelegate = OnWillPageScan,
                DidPageScanDelegate = OnDidPageScan,
                DonePageScanDelegate = OnDonePageScan,
                DidBatchDelegate = OnDidBatch,
                ErrorDelegate = OnError,
            };

            //InitKofax();
            InitTwain();

            ResultList.ItemsSource = _resultFiles;
        }

        private void LoadPlugins()
        {
            try
            {
                ImageScannerLoader loader = new ImageScannerLoader();
                loader.LoadPlugins(".");
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format("Plugins couldn't be loaded: {0}", ex.Message));
                Environment.Exit(0);
            }
        }

        private void InitKofax()
        {
            try
            {
                _kofaxPlugin = ImageScannerLoader.GetPlugin("ScanAxKofaxScanner", _delegates);
            }
            catch
            {
                KofaxSelectScannerBtn.IsEnabled = false;
                KofaxScanBtn.IsEnabled = false;
                return;
            }

            try { _ = _kofaxPlugin?.IsDeviceSelected; }
            catch { KofaxSelectScannerBtn.IsEnabled = false; }

            //_kofaxPlugin.DeviceMethods
            //_kofaxPlugin.ColorSets
            //_kofaxPlugin.Feeders
            //_kofaxPlugin.Duplexs
            //_kofaxPlugin.PaperSizes
            //_kofaxPlugin.RotateDegrees
            //_kofaxPlugin.Dpis
            //_kofaxPlugin.Brightnesses
            //_kofaxPlugin.Contrasts
            //_kofaxPlugin.IsShowUI
        }

        private void InitTwain()
        {
            try
            {
                _twainPlugin = ImageScannerLoader.GetPlugin("DynamsoftTwainScanner", _delegates, TwainLicense.Text);
            }
            catch
            {
                TwainSelectScannerBtn.IsEnabled = false;
                TwainScanBtn.IsEnabled = false;
                return;
            }

            try
            {
                Console.WriteLine($"Device name : {_twainPlugin.GetDeviceNameSelected}");
            }
            catch
            {
                TwainSelectScannerBtn.IsEnabled = false;
            }

            InitTwainDeviceMethod();
            InitTwainColorSet();
            InitTwainFeeder();
            InitTwainDuplex();
            InitTwainPaperSize();
            InitTwainRotateDegree();
            InitTwainDpi();
            InitTwainBrightness();
            InitTwainContrast();
        }

        private void InitTwainDeviceMethod()
        {
            try
            {
                List<OptionItem> list = new List<OptionItem>();

                foreach (DeviceMethod item in _twainPlugin.DeviceMethods)
                {
                    list.Add(new OptionItem
                    {
                        Code = item.ToString(),
                        Name = item.GetName(),
                    });
                }

                TwainDeviceMethod.ItemsSource = list;
                TwainDeviceMethod.DisplayMemberPath = "Name";
            }
            catch
            {
                TwainDeviceMethod.IsEnabled = false;
            }
        }

        private void InitTwainColorSet()
        {
            try
            {
                List<OptionItem> list = new List<OptionItem>();

                foreach (ColorSet item in _twainPlugin.ColorSets)
                {
                    list.Add(new OptionItem
                    {
                        Code = item.ToString(),
                        Name = item.GetName(),
                    });
                }

                TwainColorSet.ItemsSource = list;
                TwainColorSet.DisplayMemberPath = "Name";
            }
            catch
            {
                TwainColorSet.IsEnabled = false;
            }
        }

        private void InitTwainFeeder()
        {
            try
            {
                List<OptionItem> list = new List<OptionItem>();

                foreach (Feeder item in _twainPlugin.Feeders)
                {
                    list.Add(new OptionItem
                    {
                        Code = item.ToString(),
                        Name = item.GetName(),
                    });
                }

                TwainFeeder.ItemsSource = list;
                TwainFeeder.DisplayMemberPath = "Name";
            }
            catch
            {
                TwainFeeder.IsEnabled = false;
            }
        }

        private void InitTwainDuplex()
        {
            try
            {
                List<OptionItem> list = new List<OptionItem>();

                foreach (Duplex item in _twainPlugin.Duplexes)
                {
                    list.Add(new OptionItem
                    {
                        Code = item.ToString(),
                        Name = item.GetName(),
                    });
                }

                TwainDuplex.ItemsSource = list;
                TwainDuplex.DisplayMemberPath = "Name";
            }
            catch
            {
                TwainDuplex.IsEnabled = false;
            }
        }

        private void InitTwainPaperSize()
        {
            try
            {
                List<OptionItem> list = new List<OptionItem>();

                foreach (PaperSize item in _twainPlugin.PaperSizes)
                {
                    list.Add(new OptionItem
                    {
                        Code = item.ToString(),
                        Name = item.GetName(),
                    });
                }

                TwainPaperSize.ItemsSource = list;
                TwainPaperSize.DisplayMemberPath = "Name";
            }
            catch
            {
                TwainPaperSize.IsEnabled = false;
            }
        }

        private void InitTwainRotateDegree()
        {
            try
            {
                List<OptionItem> list = new List<OptionItem>();

                foreach (RotateDegree item in _twainPlugin.RotateDegrees)
                {
                    list.Add(new OptionItem
                    {
                        Code = item.ToString(),
                        Name = item.GetName(),
                    });
                }

                TwainRotateDegree.ItemsSource = list;
                TwainRotateDegree.DisplayMemberPath = "Name";
            }
            catch
            {
                TwainRotateDegree.IsEnabled = false;
            }
        }

        private void InitTwainDpi()
        {
            try
            {
                List<OptionItem> list = new List<OptionItem>();

                foreach (Dpi item in _twainPlugin.Dpis)
                {
                    list.Add(new OptionItem
                    {
                        Code = item.ToString(),
                        Name = item.GetName(),
                    });
                }

                TwainDpi.ItemsSource = list;
                TwainDpi.DisplayMemberPath = "Name";
            }
            catch
            {
                TwainDpi.IsEnabled = false;
            }
        }

        private void InitTwainBrightness()
        {
            try
            {
                TwainBrightnessLbl.Content = "0";
                TwainBrightnessSlider.Value = 0;
                TwainBrightnessSlider.Minimum = _twainPlugin.Brightnesses.Min();
                TwainBrightnessSlider.Maximum = _twainPlugin.Brightnesses.Max();
                TwainBrightnessSlider.SmallChange = 10;
                TwainBrightnessSlider.LargeChange = 100;
            }
            catch
            {
                TwainBrightnessSlider.IsEnabled = false;
            }
        }

        private void InitTwainContrast()
        {
            try
            {
                TwainContrastLbl.Content = "0";
                TwainContrastSlider.Value = 0;
                TwainContrastSlider.Minimum = _twainPlugin.Contrasts.Min();
                TwainContrastSlider.Maximum = _twainPlugin.Contrasts.Max();
                TwainContrastSlider.SmallChange = 10;
                TwainContrastSlider.LargeChange = 100;
            }
            catch
            {
                TwainContrastSlider.IsEnabled = false;
            }
        }

        private void KofaxSelectScannerBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_kofaxPlugin.IsDeviceSelected)
                {
                    Console.WriteLine($"Device is selected already: {_kofaxPlugin.GetDeviceNameSelected}");
                }

                _kofaxPlugin.OpenDeviceSettingWindow();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private void TwainSelectScannerBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_twainPlugin.IsDeviceSelected)
                {
                    Console.WriteLine($"Device is selected already: {_twainPlugin.GetDeviceNameSelected}");
                }

                _twainPlugin.OpenDeviceSettingWindow();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private void KofaxScanBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!_kofaxPlugin.IsDeviceSelected)
                    return;

                Console.WriteLine($"Device is selected already: {_kofaxPlugin.GetDeviceNameSelected}");

                ScanOptions options = new ScanOptions();

                _kofaxPlugin.Scan(options);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private void TwainScanBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!_twainPlugin.IsDeviceSelected)
                    _twainPlugin.OpenDeviceSettingWindow();

                if (!_twainPlugin.IsDeviceSelected)
                    return;

                Console.WriteLine($"Device is selected already: {_twainPlugin.GetDeviceNameSelected}");

                ScanOptions options = new ScanOptions();

                if (TwainDeviceMethod.SelectedItem is OptionItem item1 && Enum.Parse(typeof(DeviceMethod), item1.Code) is DeviceMethod value1) options.DeviceMethod = value1;
                if (TwainColorSet.SelectedItem is OptionItem item2 && Enum.Parse(typeof(ColorSet), item2.Code) is ColorSet value2) options.ColorSet = value2;
                if (TwainFeeder.SelectedItem is OptionItem item3 && Enum.Parse(typeof(Feeder), item3.Code) is Feeder value3) options.Feeder = value3;
                if (TwainDuplex.SelectedItem is OptionItem item4 && Enum.Parse(typeof(Duplex), item4.Code) is Duplex value4) options.Duplex = value4;
                if (TwainPaperSize.SelectedItem is OptionItem item5 && Enum.Parse(typeof(PaperSize), item5.Code) is PaperSize value5) options.PaperSize = value5;
                if (TwainRotateDegree.SelectedItem is OptionItem item6 && Enum.Parse(typeof(RotateDegree), item6.Code) is RotateDegree value6) options.RotateDegree = value6;
                if (TwainDpi.SelectedItem is OptionItem item7 && Enum.Parse(typeof(Dpi), item7.Code) is Dpi value7) options.Dpi = value7;

                options.Brightness = Convert.ToInt32(TwainBrightnessSlider.Value);
                options.Contrast = Convert.ToInt32(TwainContrastSlider.Value);
                options.IsShowUI = TwainShowUI.IsChecked ?? false;

                _twainPlugin.Scan(options);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private void TwainBrightnessSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            TwainBrightnessLbl.Content = Convert.ToInt32(e.NewValue).ToString();
        }

        private void TwainContrastSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            TwainContrastLbl.Content = Convert.ToInt32(e.NewValue).ToString();
        }

        private void TwainSetLicenseBtn_Click(object sender, RoutedEventArgs e)
        {
            InitTwain();
        }

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
        }

        private void OnDidBatch()
        {
            Console.WriteLine($"OnDidBatch invoked.");
        }

        private void OnError(Exception ex)
        {
            Console.WriteLine($"OnError invoked: {ex}");
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
    }
}
