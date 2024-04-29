using System;
using System.Collections.Generic;
using System.Text;

using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace AttModel.Device.DevSynchronize
{
    public class DeviceLogin
    {
        [Required]
        public string DevIp { get; set; }
        [Required]
        public string DevIpPort { get; set; }
        [Required]
        public string LoginId { get; set; }
        [Required]
        public string LoginPassword { get; set; }
    }
    public class DeviceConfig
    {
        public DeviceConfig(DeviceLogin deviceLogin, int maxFace, int maxFingerPrint, int maxAccessCard, int maxPassKey)
        {
            DeviceLogin = deviceLogin;
            MaxFace = maxFace;
            MaxFingerPrint = maxFingerPrint;
            MaxAccessCard = maxAccessCard;
            MaxPassKey = maxPassKey;
        }

        public DeviceLogin DeviceLogin { get; set; }

        public string TypeName { get; set; }
        public string TypeNo { get; set; }
        public string SerialNo { get; set; }
        public string EmployeeNoPrefix { get; set; }
        public int MaxFace { get; set; }        // 0 = no this function
        public int MaxFingerPrint { get; set; } // 0 = no this function
        public int MaxAccessCard { get; set; }  // 0 = no this function
        public int MaxPassKey { get; set; }     // 0 = no this function

        public string FormatEmployeeId(string employeeId)
        {
            if (TypeNo == "DS-K1T804BMF")
            {
                employeeId = Regex.Replace(employeeId, "[a-z]", "", RegexOptions.IgnoreCase);
                return employeeId;
            }
            else
            {
                return employeeId;
            }
        }
    }

    public class DeviceFlatConfig
    {
        public DeviceFlatConfig()
        {
            DevIp = "127.0.0.1";
            DevIpPort = "8000";
            LoginId = "admin";
            LoginPassword = "admin888";
            TypeName = "DS-K1T804BMF FINGERP&CARD";
            TypeNo = "DS-K1T804BMF";
            SerialNo = "123456";
            EmployeeNoPrefix = "E";
            MaxFace = 0;
            MaxFingerPrint = 1000;
            MaxAccessCard = 1000;
            MaxPassKey = 1000;
        }
        [Required]
        public string DeviceId { get; set; }
        [Required]
        public string DevIp { get; set; }
        [Required]
        public string DevIpPort { get; set; }
        [Required]
        public string LoginId { get; set; }
        [Required]
        public string LoginPassword { get; set; }
        [Required]
        public string TypeName { get; set; }
        [Required]
        public string TypeNo { get; set; }
        [Required]
        public string SerialNo { get; set; }
        [Required]
        public string EmployeeNoPrefix { get; set; }
        [Required]
        public int MaxFace { get; set; }
        [Required]
        public int MaxFingerPrint { get; set; }
        [Required]
        public int MaxAccessCard { get; set; }
        [Required]
        public int MaxPassKey { get; set; }
        [Required]
        public string MainComId { get; set; }      
    }
}
