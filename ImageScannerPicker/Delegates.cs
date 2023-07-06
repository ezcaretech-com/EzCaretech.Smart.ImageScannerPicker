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
    }
}
