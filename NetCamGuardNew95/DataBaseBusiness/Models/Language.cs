using System;
using System.Collections.Generic;

namespace DataBaseBusiness.Models
{
    public partial class Language
    {
        public string KeyName { get; set; }
        public string KeyType { get; set; }
        public string ZhCn { get; set; }
        public string ZhHk { get; set; }
        public string EnUs { get; set; }
        public string Remarks { get; set; }
        public string IndustryIdArr { get; set; }
        public string MainComIdArr { get; set; }
    }
}
