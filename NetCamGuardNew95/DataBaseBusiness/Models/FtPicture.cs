using System;
using System.Collections.Generic;

namespace DataBaseBusiness.Models
{
    public partial class FtPicture
    {
        public long Id { get; set; }
        public long? PicId { get; set; }
        public long? PersonId { get; set; }
        public string PicUrl { get; set; }
        public string PicClientUrl { get; set; }
        public string PicClientUrlBase64 { get; set; }
        public sbyte? Visible { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime UpdateTime { get; set; }
    }
}
