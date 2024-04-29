using System;
using System.Collections.Generic;

namespace DataBaseBusiness.ModelHistory
{
    public partial class HistGpsRecord
    {
        public long HistGpsRecordId { get; set; }
        public long HistRecognizeRecordId { get; set; }
        public float Latitude { get; set; }
        public float Longitude { get; set; }
        public string ShortAddress { get; set; }
        public string LongAddress { get; set; }
        public string LatitudeAndLongitudeJson { get; set; }
    }
}
