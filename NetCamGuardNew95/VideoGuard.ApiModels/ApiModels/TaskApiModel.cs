using DataBaseBusiness.Models;
using EnumCode;
using LanguageResource;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace VideoGuard.ApiModels.Task
{
    #region TaskApiModel 
    public enum Type
    {
        [EnumDisplayName("Type_GENERAL")]
        GENERAL = 0,
        [EnumDisplayName("Type_ROLLCALL")]
        ROLLCALL = 1
    }
    public class TaskModelInput : GlobalFieldSession
    {
        public TaskModelInput() : base()
        {
        }
        [Required(ErrorMessageResourceName = "Task_Name_Required", ErrorMessageResourceType = typeof(ResourceLocalize))]
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("maincomId")]
        public string MaincomId { get; set; }

        [JsonProperty("type")]
        public TaskType Type { get; set; }

        [Required(ErrorMessageResourceName = "Task_CameraList1_Required", ErrorMessageResourceType = typeof(ResourceLocalize))]
        [JsonProperty("cameraList1")]
        [DefaultValue("")]
        public string CameraList1 { get; set; }

        [JsonProperty("CameraList2")]
        public string CameraList2 { get; set; }

        [Required]
        [JsonProperty("libList")]
        public string LibList { get; set; }

        [DefaultValue(1)]
        [Range(1, 10)]
        [JsonProperty("interval")]
        public int Interval { get; set; }

        [DefaultValue(0.8)]
        [Range(0.1, 0.99)]
        [JsonProperty("threshold")]
        public double Threshold { get; set; }

        [JsonProperty("plan")]
        public string Plan { get; set; }

        [JsonProperty("remark")]
        public string Remark { get; set; }
    }
    public class TaskDeleteModeInput : GlobalFieldSession
    {
        public TaskDeleteModeInput() : base()
        {
        }

        [JsonProperty("taskId")]
        public int TaskId { get; set; }
    }

    public class TaskIdInput : GlobalFieldSession
    {
        public TaskIdInput() : base()
        {
        }

        [JsonProperty("taskId")]
        public int TaskId { get; set; }
    }

    public class TaskUpdateInput : GlobalFieldSession
    {
        public TaskUpdateInput() : base()
        {
        }

        [JsonProperty("taskId")]
        public int TaskId { get; set; }
        [JsonProperty("maincomId")]
        public string MaincomId { get; set; }
        [Required]
        [JsonProperty("name")]
        public string Name { get; set; }

        [Required]
        [JsonProperty("type")]
        public TaskType Type { get; set; }

        [Required]
        [JsonProperty("cameraList1")]
        public string CameraList1 { get; set; }

        [Required]
        [JsonProperty("cameraList2")]
        public string CameraList2 { get; set; }

        [Required]
        [JsonProperty("libList")]
        public string LibList { get; set; }

        [Required]
        [JsonProperty("interval")]
        public int Interval { get; set; }

        [Required]
        [JsonProperty("threshold")]
        public double Threshold { get; set; }

        [JsonProperty("plan")]
        public string Plan { get; set; }

        [JsonProperty("remark")]
        public string Remark { get; set; }
    }

    public class QueryTaskListInput : GlobalFieldSession
    {
        public QueryTaskListInput() : base()
        {
            _PageNo = 1;
            _PageSize = 64;
        }
        private int _PageNo;
        [JsonProperty("pageNo")]
        public int PageNo
        {
            get
            {
                return _PageNo;
            }

            set
            {
                if (value == 0)
                {
                    _PageNo = 1;
                }
                else
                {
                    _PageNo = value;
                }
            }
        }
        private int _PageSize;
        [JsonProperty("pageSize")]
        public int PageSize
        {
            get
            {
                return _PageSize;
            }

            set
            {
                if (value == 0)
                {
                    _PageSize = 64;
                }
                else
                {
                    _PageSize = value;
                }
            }
        }

        [JsonProperty("name")]
        public string Name { get; set; }

        private int _Type;
        [JsonProperty("type")]
        public int Type
        {
            get
            {
                return _Type;
            }

            set
            {
                if (value == 0)
                {
                    _Type = 0;
                }
                else
                {
                    _Type = value;
                }
            }
        }
        private int _State;
        [JsonProperty("state")]
        public int State
        {
            get
            {
                return _State;
            }
            set
            {
                if (value == 0)
                {
                    _State = 0;
                }
                else
                {
                    _State = value;
                }
            }
        }
    }

    public class QueryTaskListInfoReturn
    {
        private int _PageCount;
        [JsonProperty("pageCount")]
        public int PageCount
        {
            get
            {
                int _PageCount = (TotalCount + PageSize - 1) / PageSize;
                return _PageCount;
            }
            set
            {
                _PageCount = value;
            }
        }
        [JsonProperty("pageNo")]
        public int PageNo { get; set; }

        [DefaultValue(4)]
        [JsonProperty("pageSize")]
        public int PageSize { get; set; }

        [JsonProperty("totalCount")]
        public int TotalCount { get; set; }

        [JsonProperty("items")]
        public List<Task> Items { get; set; }

        public string ReplaceFirstStr(string sourceStr, string splitStr, string newStr)
        {
            try
            {
                int indexOfFirstSplitStr = sourceStr.IndexOf(splitStr);
                StringBuilder sb = new StringBuilder(sourceStr);
                sb.Replace(splitStr, newStr, indexOfFirstSplitStr, splitStr.Length);
                sourceStr = sb.ToString();
            }
            catch
            {
                return sourceStr;
            }
            return sourceStr;
        }
    }
    public class StartTaskModeInput : GlobalFieldSession
    {
        public StartTaskModeInput() : base()
        {
        }

        [JsonProperty("taskId")]
        public int TaskId { get; set; }
    }
    public class StopTaskModeInput : GlobalFieldSession
    {
        public StopTaskModeInput() : base()
        {
        }

        [JsonProperty("taskId")]
        public int TaskId { get; set; }
    }
    public class Task
    {
        [JsonProperty("taskId")]
        public int TaskId { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("type")]
        public int Type { get; set; }

        [JsonProperty("cameraList1")]
        public List<CameraList> CameraList1 { get; set; }
        [JsonIgnore]
        public virtual string CameraArray1
        {
            get
            {
                if (CameraList1.Count == 0)
                {
                    return "";
                }
                else
                {
                    string arr = string.Join(",", CameraList1.Select(c => c.CameraId).ToArray());
                    return arr;
                }
            }
        }

        [JsonProperty("cameraList2")]
        public List<CameraList> CameraList2 { get; set; }
        [JsonIgnore]
        public virtual string CameraArray2
        {
            get
            {
                if (CameraList2.Count == 0)
                {
                    return "";
                }
                else
                {
                    string arr = string.Join(",", CameraList2.Select(c => c.CameraId).ToArray());
                    return arr;
                }
            }
        }

        [JsonProperty("libList")]
        public string LibList { get; set; }

        [JsonProperty("libIdGroups")]
        public List<FtLibrary> LibIdGroupsList { get; set; }

        [JsonProperty("interval")]
        public int Interval { get; set; }

        [JsonProperty("threshold")]
        public double Threshold { get; set; }

        [JsonProperty("state")]
        public int State { get; set; }

        [JsonProperty("plan")]
        public string Plan { get; set; }

        [JsonProperty("remark")]
        public string Remark { get; set; }

        [JsonProperty("createTime")]
        public string CreateTime { get; set; }
    }

    public class CameraList
    {
        [JsonProperty("cameraId")]
        public int CameraId { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("state")]
        public int State { get; set; }
    }
    #endregion TaskApiModel
}

