using EnumCode;
using LanguageResource;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace VideoGuard.ApiModels
{
    public partial class ResponseModalX
    {
        delegate T TransDataType<T>(T x);
        public ResponseModalX()
        {
            _meta = new MetaModalX { ErrorCode = (int)GeneralReturnCode.SUCCESS, Success = true, Message = "OK" };
            _data = null;
        }
        private MetaModalX _meta;
        [JsonProperty("meta")]
        public MetaModalX meta { 
            get
            {
                return _meta;
            }
            set
            {
                _meta = value;
            }
        }
        private Object _data;
        [JsonProperty("data")]
        public Object data {
            get {
                return _data;
            }
            set {
                _data = value;
            }
        }

        public static implicit operator Task<object>(ResponseModalX v)
        {
            throw new NotImplementedException();
        }
    }
     
    public partial class MetaModalX
    {
        [JsonProperty("success")]
        public bool Success { get; set; } = true;

        [JsonProperty("message")]
        public string Message { get; set; } = "OK";

        [JsonProperty("errorCode")]
        public int ErrorCode { get; set; } = (int)GeneralReturnCode.SUCCESS;
    }
}
