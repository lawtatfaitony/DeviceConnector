using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace VideoGuard.ApiModels.ApiModels
{
    public class PictureUploadRet
    {
        [JsonProperty("picUrl")]
        public string PicUrl { get; set; }

        [JsonProperty("picClientUrl")]
        public string PicClientUrl { get; set; }   
    }
}
