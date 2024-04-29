using System;
using System.Collections.Generic;
using System.Text;

namespace Common
{
    public class OSHelper
    {
        private const string Windows2000 = "5.0";
        private const string WindowsXP = "5.1";
        private const string Windows2003 = "5.2";
        private const string Windows2008 = "6.0";
        private const string Windows7 = "6.1";
        private const string Windows8OrWindows81 = "6.2";
        private const string Windows10 = "10.0";

        public OSHelper()
        {
            switch (System.Environment.OSVersion.Version.Major + "." + System.Environment.OSVersion.Version.Minor)
            {
                case Windows2000:
                    setOSystemName("Windows2000");
                    break;
                case WindowsXP:
                    setOSystemName("WindowsXP");
                    break;
                case Windows2003:
                    setOSystemName("Windows2003");
                    break;
                case Windows2008:
                    setOSystemName("Windows2008");
                    break;
                case Windows7:
                    setOSystemName("Windows7");
                    break;
                case Windows8OrWindows81:
                    setOSystemName("Windows8.OrWindows8.1");
                    break;
                case Windows10:
                    setOSystemName("Windows10");
                    break;
            }
             
        }
        private string _OSystemName;
        public string OSystemName
        {
            get
            {
                return _OSystemName;
            }
        }
        public void setOSystemName(string oSystemName)
        {
            this._OSystemName = oSystemName;
        }

        // 获取操作系统ID
        public static System.PlatformID GetPlatformID()
        {
            //获取系统信息
            System.OperatingSystem osInfo = System.Environment.OSVersion;

            //获取操作系统ID
            System.PlatformID platformID = osInfo.Platform;

            return platformID;
        }

        // 获取主版本号
        public static int GetVersionMajor()
        {
            //获取系统信息
            System.OperatingSystem osInfo = System.Environment.OSVersion;

            //获取主版本号
            int versionMajor = osInfo.Version.Major;

            return versionMajor;
        }

        // 获取副版本号
        public static int GetVersionMinor()
        {
            //获取系统信息
            System.OperatingSystem osInfo = System.Environment.OSVersion;

            //获取副版本号
            int versionMinor = osInfo.Version.Minor;

            return versionMinor;
        }

        //C#判断操作系统是否为Windows98
        public static bool IsWindows98
        {
            get
            {
                return (Environment.OSVersion.Platform == PlatformID.Win32Windows) && (Environment.OSVersion.Version.Minor == 10) && (Environment.OSVersion.Version.Revision.ToString() != "2222A");
            }
        }
        //C#判断操作系统是否为Windows98第二版
        public static bool IsWindows98Second
        {
            get
            {
                return (Environment.OSVersion.Platform == PlatformID.Win32Windows) && (Environment.OSVersion.Version.Minor == 10) && (Environment.OSVersion.Version.Revision.ToString() == "2222A");
            }
        }
        //C#判断操作系统是否为Windows2000
        public static bool IsWindows2000
        {
            get
            {
                return (Environment.OSVersion.Platform == PlatformID.Win32NT) && (Environment.OSVersion.Version.Major == 5) && (Environment.OSVersion.Version.Minor == 0);
            }
        }
        //C#判断操作系统是否为WindowsXP
        public static bool IsWindowsXP
        {
            get
            {
                return (Environment.OSVersion.Platform == PlatformID.Win32NT) && (Environment.OSVersion.Version.Major == 5) && (Environment.OSVersion.Version.Minor == 1);
            }
        }
        //C#判断操作系统是否为Windows2003
        public static bool IsWindows2003
        {
            get
            {
                return (Environment.OSVersion.Platform == PlatformID.Win32NT) && (Environment.OSVersion.Version.Major == 5) && (Environment.OSVersion.Version.Minor == 2);
            }
        }
        //C#判断操作系统是否为WindowsVista
        public static bool IsWindowsVista
        {
            get
            {
                return (Environment.OSVersion.Platform == PlatformID.Win32NT) && (Environment.OSVersion.Version.Major == 6) && (Environment.OSVersion.Version.Minor == 0);
            }
        }
        //C#判断操作系统是否为Windows7
        public static bool IsWindows7
        {
            get
            {
                return (Environment.OSVersion.Platform == PlatformID.Win32NT) && (Environment.OSVersion.Version.Major == 6) && (Environment.OSVersion.Version.Minor == 1);
            }
        }
        //C#判断操作系统是否为Unix
        public static bool IsUnix
        {
            get
            {
                return Environment.OSVersion.Platform == PlatformID.Unix;
            }
        }
        //利用C#判断当前操作系统是否为Win8系统
        public static bool IsWindows8()
        {
            Version currentVersion = Environment.OSVersion.Version;
            Version compareToVersion = new Version("6.1");
            if (currentVersion.CompareTo(compareToVersion) >= 1)
                return true;
            else
                return false;
        }
        /// <summary>
        ///  Low version Basic on Win10
        /// </summary>
        /// <returns></returns>
        public static bool IsLowVsersion()
        {
            Version currentVersion = Environment.OSVersion.Version;
            Version win10Version = new Version("6.2");
            if (currentVersion < win10Version)
                return true;
            else
                return false;
        }
    }
}

