using Newtonsoft.Json;

namespace VideoGuard.ApiModels
{
    public abstract class GlobalFieldSession
    {
        public GlobalFieldSession()
        { 
            session = "0192023a7bbd73250516f069df18b500"; // WebCookie.ApiSession;
        }
        [JsonProperty("session")]
        public string session { get; set; }
    }

    public class GlobalSession: GlobalFieldSession
    { 
    }

    public abstract class GlobalDeviceSession
    { 
        [JsonProperty("deviceToken")]
        public string DeviceToken { get; set; }
    }
     
}

