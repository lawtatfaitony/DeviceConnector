using EnumCode;
using LanguageResource;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
namespace VideoGuard.ApiModels.LibraryApiModel
{
    #region LibraryApiModel 
    public class LibraryApiModelInput : GlobalFieldSession
    {
        public LibraryApiModelInput() : base()
        {
            LibraryTypeCode = LibraryTypeCode.LIB_FIX_GROUP;
        }
        /// <summary>
        /// 公司ID
        /// </summary>
        public string MainComId { get; set; }

        [Required(ErrorMessageResourceName = "Library_Name_required", ErrorMessageResourceType = typeof(ResourceLocalize))]
        public string Name { get; set; }

        /// <summary>
        /// Type 人員底庫類型 分為: 拍卡\人臉\指紋\GPS\二維碼\ 拍卡、人臉、指紋、GPS或二維碼 
        /// 常量值來自 LibraryTypeCode
        /// </summary> 
        public LibraryTypeCode LibraryTypeCode { get; set; }  
        public string Remark { get; set; }
    }

    public class LibraryApiModelUpateInput : GlobalFieldSession
    {
        public LibraryApiModelUpateInput() : base()
        {
        }
        [Required(ErrorMessageResourceName = "Library_libId_Required", ErrorMessageResourceType = typeof(ResourceLocalize))]
        public int LibId { get; set; }

        [Required(ErrorMessageResourceName = "Library_Name_required", ErrorMessageResourceType = typeof(ResourceLocalize))]
        public string Name { get; set; }
         
        public LibraryTypeCode LibraryTypeCode { get; set; }
        public string Remark { get; set; }
    }

    /// <summary>
    /// 人員群組庫下拉列表
    /// </summary>
    public class QueryLibraryListSelect
    {

        [JsonProperty("label")]
        public string label { get; set; }


        [JsonProperty("value")]
        public string value { get; set; }

    }
    #endregion LibraryApiModel

    #region QueryLibraryList
    public class QueryLibraryList : GlobalFieldSession
    {
        public QueryLibraryList() : base()
        {
        }
        private int _PageNo;
        [DefaultValue(1)]
        [JsonProperty("pageNo")]
        public int PageNo
        {
            get
            {
                if (_PageNo == 0)
                {
                    return 1;
                }
                return _PageNo;
            }
            set
            {
                _PageNo = value;
            }
        }
        private int _PageSize;
        [DefaultValue(12)]
        [JsonProperty("pageSize")]
        public int PageSize
        {
            get
            {
                if (_PageNo == 0)
                {
                    return 12;
                }
                return _PageSize;
            }
            set
            {
                _PageSize = value;
            }
        }

        [JsonProperty("name")]
        public string Name { get; set; }
         
        [JsonProperty("libraryTypeCode")]
        public LibraryTypeCode? LibraryTypeCode { get; set; }
    }
    public class QueryLibraryListReturn
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
        public List<LibraryItem> Items { get; set; }
    }
    public class LibraryItem
    {
        private string _CreateTime;
        public LibraryItem()
        {
            _CreateTime = string.Format("{0:yyyy-MM-dd HH:mm:ss}", DateTime.Now);
        }
        [Required(ErrorMessageResourceName = "Library_libId_Required", ErrorMessageResourceType = typeof(ResourceLocalize))]
        [JsonProperty("libId")]
        public int LibId { get; set; }

        [Required(ErrorMessageResourceName = "Library_Name_required", ErrorMessageResourceType = typeof(ResourceLocalize))]
        [JsonProperty("name")]
        public string Name { get; set; }

        [DefaultValue(0)]
        [JsonProperty("type")]
        public int Type { get; set; }

        [DefaultValue(0)]
        [JsonProperty("personCount")]
        public int PersonCount { get; set; }

        [JsonProperty("remark")]
        public string Remark { get; set; }

        [DefaultValue(1)]
        [JsonProperty("visible")]
        public int Visible { get; set; }

        [JsonProperty("createTime")]
        public string CreateTime
        {
            get
            {
                return _CreateTime;
            }
            set
            {
                _CreateTime = value;
            }
        }
    }
    public class QueryLibListSelect
    {
        [JsonProperty("label")]
        public string label { get; set; }

        [JsonProperty("value")]
        public string value { get; set; }
    }

    #endregion QueryLibraryList 

    #region Delete
    public class LibraryDelete : GlobalFieldSession
    {
        public LibraryDelete() : base()
        {
        }
        [JsonProperty("libId")]
        public int LibId { get; set; }
    }
    #endregion delete

    /// <summary>
    /// 一维群组列表
    /// 专门提供前端标准人员分组格式
    /// </summary>
    public class GroupsForApi 
    { 
        public string GroupId { get; set; }
        public string ParentsGroupId { get; set; } 
        public string GroupName { get; set; }
    }
}

