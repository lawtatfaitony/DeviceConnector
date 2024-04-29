using Newtonsoft.Json;
namespace VideoGuard.ApiModels
{
    public class GlobalReturnResult
    {
        public GlobalReturnResult()
        {

        }
        public int Code { get; set; }
        public string Msg { get; set; }
    }

    public class GlobalReturnInfoResult
    {
        public GlobalReturnInfoResult()
        {

        }
        public int Code { get; set; }
        public object Info { get; set; }
        public string Msg { get; set; }
    }
     
}

