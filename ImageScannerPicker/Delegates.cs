using System;

namespace ImageScannerPicker
{
    public class Delegates
    {
        public delegate void WillBatch();
        public delegate void WillPageScan();
        public delegate void DidPageScan();
        public delegate void DonePageScan(string filePath);
        public delegate void DidBatch();
        public delegate void Error(Exception ex);

        public WillBatch WillBatchDelegate { get; set; }
        public WillPageScan WillPageScanDelegate { get; set; }
        public DidPageScan DidPageScanDelegate { get; set; }
        public DonePageScan DonePageScanDelegate { get; set; }
        public DidBatch DidBatchDelegate { get; set; }
        public Error ErrorDelegate { get; set; }
    }
}
