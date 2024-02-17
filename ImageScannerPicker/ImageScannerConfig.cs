using System;
using static ImageScannerPicker.Delegates;

namespace ImageScannerPicker
{
    public class ImageScannerConfig
    {
        public IntPtr WindowHandle { get; set; }

        public string License { get; set; }

        public WillBatch WillBatchDelegate { get; set; }

        public WillPageScan WillPageScanDelegate { get; set; }

        public DidPageScan DidPageScanDelegate { get; set; }

        public DonePageScan DonePageScanDelegate { get; set; }

        public DidBatch DidBatchDelegate { get; set; }

        public Error ErrorDelegate { get; set; }
    }
}
