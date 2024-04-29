using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace VxGuardClient.ExternalDll
{
    public class SearchCamera
    {
        [DllImport("CameraSearch.dll")]
        public static extern string SearchCameras(string key, DeviceInfoRtsp deviceInfo);
    }
    public class DeviceInfoRtsp
    {
        public string uri;
        public string ip;
        public string name;
        public string location;
        public string hardware; 
    }
}
