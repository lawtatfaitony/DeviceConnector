using Common;
using DataBaseBusiness.ModelHistory;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using static Common.CommonBase;

namespace FastConnector.HistRecognize
{
    public class LiveRoomInitializeSRV
    {
        public void ToInstance()
        {
            _CurrentIndexId = 0;
            DateTime dt = DateTime.Now;
            DateTimeOffset dateTimeOffset = new DateTimeOffset(dt);
            _CurrentIndexTimeStamp = dateTimeOffset.ToUnixTimeMilliseconds();
            
            using (HistoryContext dataBaseContext = new HistoryContext())
            {
                var histRecognizeRecord25 = dataBaseContext.HistRecognizeRecord.AsNoTracking().Select(c => new { c.Id, c.CreateTime, c.CapturePath,c.Visible }).Where(c => !string.IsNullOrEmpty(c.CapturePath) && c.Visible == 1).OrderByDescending(c => c.CreateTime).Take(25).ToList();
                List<HistRecognizeRecord> histTop25 = new List<HistRecognizeRecord>();
                histRecognizeRecord25.ForEach(a =>
                {
                    HistRecognizeRecord histItem = dataBaseContext.HistRecognizeRecord.Find(a.Id);
                    histTop25.Add(histItem);
                });
                List<HistoryLiveRoomSRV> liveRooms25 = new List<HistoryLiveRoomSRV>();
                 
                int i = 0;
                HistoryLiveRoomSRV historyLiveRoomTop1 = new HistoryLiveRoomSRV();
                foreach (var item in histTop25)
                {
                    if(i==0 && item != null)
                    {
                        historyLiveRoomTop1 = HistoryLiveRoomSRV.HistTransToLiveRoomObj(item);
                    }
                    HistoryLiveRoomSRV attendanceLiveRoom = HistoryLiveRoomSRV.HistTransToLiveRoomObj(item);
                    liveRooms25.Add(attendanceLiveRoom);
                    i++;
                }

                if (histRecognizeRecord25.Count > 0)
                {
                    _ListTop25= liveRooms25; 
                }
                else
                {
                    CommonBase.OperateDateLoger("DynamicLiveRoom.cs histRecognizeRecord25 liveRooms20 :: = NULL ", LoggerMode.FATAL);
                }

                if(historyLiveRoomTop1!=null)
                {
                    _CurrentIndexTimeStamp = new DateTimeOffset(historyLiveRoomTop1.CreateTime).ToUnixTimeMilliseconds();
                    _CurrentIndexId = historyLiveRoomTop1.Id;
                    _CurrentIndexItem = historyLiveRoomTop1;
                }
            }    
        }

        private long _CurrentIndexId;
        public long CurrentIndexId
        {
            get
            {
                return _CurrentIndexId;
            }
            set
            {
                _CurrentIndexId = value;
            }
        }
        private long _CurrentIndexTimeStamp;
        public long CurrentIndexTimeStamp
        {
            get
            {

                return _CurrentIndexTimeStamp;
            }
            set
            {
                _CurrentIndexTimeStamp = value;
            }
        }
        private HistoryLiveRoomSRV _CurrentIndexItem;
        public HistoryLiveRoomSRV CurrentIndexItem
        {
            get
            {
                return _CurrentIndexItem;
            }
            set
            {
                _CurrentIndexItem = value;
            }
        }
        private List<HistoryLiveRoomSRV> _ListTop25;
        public List<HistoryLiveRoomSRV> ListTop25
        {
            get
            {
                return _ListTop25;
            }
            set
            {
                _ListTop25 = value;
            }
        }
         
    }
    public class HistoryLiveRoomSRV : HistRecognizeRecord
    {
        private static string SubFolderEntries { get; set; } = "EntriesLogImages";
        private long _CurrentIndexTimeStamp;
        public long CurrentIndexTimeStamp
        {
            get
            {
                return _CurrentIndexTimeStamp;
            }
            set
            {
                _CurrentIndexTimeStamp = value;
            }
        }
        private long _OccurTimeSpan;
        public long OccurTimeSpan
        {
            get
            {
                return _OccurTimeSpan;
            }
            set
            {
                _OccurTimeSpan = value;
            }
        } 
        public static HistoryLiveRoomSRV HistTransToLiveRoomObj(HistRecognizeRecord histRecog)
        {
            HistoryLiveRoomSRV historyLiveRoom = new HistoryLiveRoomSRV
            {
                Id = histRecog.Id,
                CardNo = histRecog.CardNo,
                CapturePath = TransCapturePath(histRecog.OccurDatetime,histRecog.CapturePath),
                Category = histRecog.Category,
                Classify = histRecog.Classify,
                CreateTime = histRecog.CreateTime,
                LibId = histRecog.LibId,
                LibName = histRecog.LibName,
                PersonId = histRecog.PersonId,
                PersonName = histRecog.PersonName,
                PicPath = histRecog.PicPath,
                Remark = histRecog.Remark,
                Sex = histRecog.Sex,
                Similarity = histRecog.Similarity,
                CaptureTime = histRecog.CaptureTime.GetValueOrDefault(),
                UpdateTime = histRecog.UpdateTime,
                Visible = histRecog.Visible,
            };
            DateTimeOffset occurOffset = new DateTimeOffset(histRecog.CreateTime);
            historyLiveRoom.OccurTimeSpan = occurOffset.ToUnixTimeMilliseconds();
            historyLiveRoom.CurrentIndexTimeStamp = occurOffset.ToUnixTimeMilliseconds();
             
            return historyLiveRoom;
        }
        /// <summary>
        /// 識別結果的圖片 改為按月度存儲,需要根據發生時間為 月度文件夾 相應改為如下格式
        /// 如 http://localhost:5002/Upload/EntriesLogImages/202208/1661764535255.jpgs60X60.jpg
        /// s60X60.jpg默認是縮略圖
        /// </summary>
        /// <param name="occur"></param>
        /// <param name="capturePath"></param>
        /// <returns></returns>
        public static string TransCapturePath(long occur, string capturePath)
        {
            if (string.IsNullOrEmpty(capturePath))
                return string.Empty;

            string extend = Path.GetExtension(capturePath);
            DateTime occurDatetime = DateTimeHelp.ConvertToDateTime(occur);
            string newCapturePath = string.Format("/Upload/{0}/{1:yyyyMM}/{2}{3}s60X60.jpg", SubFolderEntries, occurDatetime, occur, extend);
            return newCapturePath;
        }
         
        public DateTime ConvetToDateTime()
        {
            DateTime start = new DateTime(1970, 1, 1, 0, 0, 0);
            DateTime date = start.AddMilliseconds(_CurrentIndexTimeStamp).ToLocalTime();
            return date;
        }
    }

    public class ReturnNewHistoryLiveRoom
    {
        public long CurrentIndexTimeStamp { get; set; }
        public long CurrentIndexId { get; set; }
        public HistoryLiveRoomSRV HistoryLiveRoom { get; set; }
        public static HistoryLiveRoomSRV GetNextHistLiveRoomItem(long CurrentIndexTimeStamp)
        {
            DateTime start = new DateTime(1970, 1, 1, 0, 0, 0);
            DateTime currentDateTime = start.AddMilliseconds(CurrentIndexTimeStamp).ToLocalTime();

            using (HistoryContext historyContext = new HistoryContext())
            {
                HistoryLiveRoomSRV historyLiveRoom = new HistoryLiveRoomSRV();
                var histRecognizeRecords = historyContext.HistRecognizeRecord.AsNoTracking()
                .Select(c => new { c.Id, c.CreateTime, c.CapturePath, c.Visible }).Where(c => c.CreateTime > currentDateTime && !string.IsNullOrEmpty(c.CapturePath) && c.Visible == 1);
                
                if (histRecognizeRecords.Count() > 0)
                {
                    var histRecog = histRecognizeRecords.OrderBy(c => c.CreateTime).FirstOrDefault();
                    int count = histRecognizeRecords.Count();
                    HistRecognizeRecord hist = historyContext.HistRecognizeRecord.Find(histRecog.Id);
                    historyLiveRoom = HistoryLiveRoomSRV.HistTransToLiveRoomObj(hist);
                    historyLiveRoom.Remark = string.Format("{0:yyyy-MM-dd HH:mm:ss fff} count = {1}", currentDateTime, count);
                }
                else
                {
                    historyLiveRoom = new HistoryLiveRoomSRV
                    {
                        Id = 0,
                        CardNo = string.Empty,
                        CapturePath = string.Empty,
                        Category = 0,
                        Classify = 0,
                        CreateTime = currentDateTime,
                        LibId = 0,
                        LibName = string.Empty,
                        PersonId = 0,
                        PersonName = string.Empty,
                        PicPath = string.Empty,
                        Remark = string.Format("{0:yyyy-MM-dd HH:mm:ss fff}", currentDateTime),
                        Sex = 0,
                        Similarity = 0,
                        UpdateTime = currentDateTime,
                        CaptureTime = currentDateTime,
                        Visible = 0
                    };
                }
                return historyLiveRoom;
            }
             
        }
        public static HistoryLiveRoomSRV GetCurrentHistoryLiveRoomItem(long histRecognizeRecordId)
        {
            using (var t = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted }))
            {
                using (HistoryContext historyContext = new HistoryContext())
                {
                    HistRecognizeRecord histRecognize = historyContext.HistRecognizeRecord.Find(histRecognizeRecordId);

                    HistoryLiveRoomSRV historyLiveRoom = new HistoryLiveRoomSRV();
                    if (histRecognize != null)
                    {
                        historyLiveRoom = HistoryLiveRoomSRV.HistTransToLiveRoomObj(histRecognize);
                    }
                    else
                    {
                        historyLiveRoom = null;
                    }
                    return historyLiveRoom;
                }
            }
        }
    }
}