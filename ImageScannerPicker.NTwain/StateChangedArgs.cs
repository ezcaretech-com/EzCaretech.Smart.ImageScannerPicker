using System;

namespace ImageScannerPicker.NTwain
{
    public class StateChangedArgs : EventArgs
    {
        public int NewState { get; set; }
    }
}
