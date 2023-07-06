namespace ImageScannerPicker
{
    public class ScanOptions
    {
        public string DeviceName { get; set; }

        public DeviceMethod DeviceMethod { get; set; }

        public ColorSet ColorSet { get; set; }

        public Feeder Feeder { get; set; }

        public Duplex Duplex { get; set; }

        public PaperSize PaperSize { get; set; }

        public RotateDegree RotateDegree { get; set; }

        public Dpi Dpi { get; set; }

        public int Brightness { get; set; }

        public int Contrast { get; set; }

        public bool IsShowUI { get; set; }
    }
}
