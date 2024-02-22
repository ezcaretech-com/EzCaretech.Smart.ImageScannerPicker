using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace ImageScannerPicker.DynamicWebTwain
{
    internal class ScannerDevice
    {
        [JsonProperty]
        internal string Device { get; set; }

        [JsonProperty]
        internal string Name { get; set; }

        [JsonProperty]
        internal int Type { get; set; }
    }
}
