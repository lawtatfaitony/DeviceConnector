using Common;
using System;
using EnumCode;
using System.Runtime.InteropServices;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        { 

            Console.WriteLine("=======Epoch timestamp: 1655450415 Timestamp in milliseconds: 1655450415000 Date and time(GMT): 2022年6月17日Friday 07:20:15==============================");

            DateTime dt =  DateTimeHelp.ConvertToDateTime(1655450415);
          
            Console.WriteLine("Hello World! 1655450415 (at lease 13) = {0:yyyy-MM-dd HH:mm:ss fff}", dt);
            Console.WriteLine("1655450415\nGMT: Friday, June 17, 2022 7:20:15 AM \nYour time zone: Friday, June 17, 2022 3:20:15 PM GMT+08:00\n");

            string sex = Enum.GetName(typeof(EnumBusiness.Genders),2);
            Console.WriteLine("xxxxxx==="+  sex);
            Console.WriteLine("sex :={0}", sex);

            Console.ReadKey();

        }

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
}
