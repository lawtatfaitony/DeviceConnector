using LanguageResource;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace VideoGuard.ApiModels.ApiModels
{
    public partial class SiteInputEntry
    {
        public int SiteId { get; set; }

        [Required]
        public string MaincomId { get; set; }

        [Required(ErrorMessageResourceName = "Site_ParentsIdRequired", ErrorMessageResourceType = typeof(ResourceLocalize))]
        public int ParentsId { get; set; }
        [DefaultValue("")]
        public string ParentsSiteName { get; set; }
        [Required]
        public string SiteName { get; set; } 
        public string Address { get; set; }
    }
}
