using System;
using System.Collections.Generic;

namespace DataBaseBusiness.Models
{
    public partial class FtCameraMpeg
    {
        public long Id { get; set; }
        public int DeviceId { get; set; }
        public int CameraId { get; set; }
        public string MpegFilename { get; set; }
        public long GroupTimestamp { get; set; }
        public long FileSize { get; set; }
        public string FileFomat { get; set; }
        public ulong IsGroup { get; set; }
        public long StartTimestamp { get; set; }
        public long EndTimestamp { get; set; }
        public sbyte Visible { get; set; }
        public sbyte IsFormatVirified { get; set; }
        public sbyte IsUpload { get; set; }
        public DateTime CreateTime { get; set; }
    }
}
