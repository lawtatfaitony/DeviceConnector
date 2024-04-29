using Common;
using EnumCode;
using System;
using System.Collections.Generic;

namespace DataBaseBusiness.ModelHistory
{
    public partial class HistRecognizeRecord
    {
        public HistRecognizeRecord()
        {
            Mode = (int)EnumBusiness.AttendanceMode.CAM_GUARD;
            MaincomId = "0";
            //OccurDatetime = DateTimeHelp.ToUnixTimeMilliseconds(DateTime.MinValue); 導致出錯
            DeviceId = 0;
            DeviceName= string.Empty;
            TaskId = 0; 
            CameraId = 0; 
        }
    }
}
