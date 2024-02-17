using System;

namespace ImageScannerPicker.Adaptor
{
    public class StateChangedArgs : EventArgs
    {
        public int NewState { get; set; }
    }
}
