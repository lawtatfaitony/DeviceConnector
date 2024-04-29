using DataBaseBusiness.Models;
using EnumCode;
using LanguageResource;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using VideoGuard.Device;

namespace VideoGuard.ApiModels
{
    #region PersonApiModel 
    public enum Genders
    {
        [EnumDisplayName("Genders_MALE")]
        MALE = 0,
        [EnumDisplayName("Genders_FEMALE")]
        FEMALE = 1,
        [EnumDisplayName("Genders_UNKOWN")]
        UNKOWN = 2
    }
    /// <summary>
    /// 非鎖定(白名單) = 0  和  鎖定(黑名單)=1 
    /// 還可以對此進行擴展分類,分類有 VIP  
    /// </summary>
    public enum PersonCategory
    {
        [EnumDisplayName("PersonCategory_UNBLOCKED")]
        UNBLOCKED = 0 ,
        [EnumDisplayName("PersonCategory_BLOCKED")]
        BLOCKED = 1,
        [EnumDisplayName("PersonCategory_GUEST")]
        GUEST = 2
    }
    public class PersonModelInput
    {
        public PersonModelInput() : base()
        {
        }
        [JsonProperty("maincomId")] 
        public string MaincomId { get; set; }

        [JsonProperty("outerId")]
        [DefaultValue("0")]
        public string OuterId { get; set; }

        [JsonProperty("libId")]
        [DefaultValue(0)] 
        public int LibId { get; set; }

        [JsonProperty("libIdGroups")]
        [DefaultValue("")]
        public string LibIdGroups { get; set; }

        [JsonProperty("name")] 
        public string Name { get; set; }

        [JsonProperty("sex")]
        public int Sex { get; set; }

        [Required]
        [JsonProperty("picUrl")] 
        public string PicUrl { get; set; }
         
        [JsonProperty("picClientUrl")]
        public string PicClientUrl { get; set; }

        [JsonProperty("cardNo")]
        public string CardNo { get; set; }

        [JsonProperty("passKey")]
        public string PassKey { get; set; }

        [JsonProperty("phone")]
        public string Phone { get; set; }

        [DefaultValue(PersonCategory.UNBLOCKED)]
        [JsonProperty("category")]
        public PersonCategory Category { get; set; }

        [JsonProperty("remark")]
        public string Remark { get; set; }
    }

    /// <summary>
    /// 标准API系统人员录入
    /// </summary>
    public class PersonStandardInput
    {   
        [Required]
        public string MainComId { get; set; }

        [Required]
        public string DeviceId { get; set; }

        [Required]
        public string OuterId { get; set; } 
        public string GroupId { get; set; }
        public string Name { get; set; }
        public string CardNo { get; set; }
        public string PassKey { get; set; }

        [Required]
        [DefaultValue(PersonCategory.UNBLOCKED)]
        public PersonCategory Category { get; set; }
        public string Remark { get; set; }
    }

    /// <summary>
    ///标准系统人员头像录入
    /// </summary>
    public class PersonStdPicInput
    {
        [Required]
        public string MainComId { get; set; }
        [Required]
        public string DeviceId { get; set; }
        [Required]
        public string OuterId { get; set; }
        [Required]
        public string PicturePath { get; set; }
    }

    /// <summary>
    /// 標準的人員密碼錄入
    /// </summary>
    public class PersonStdPassKeyInput
    {
        [Required]
        public string MainComId { get; set; }
        [Required]
        public string DeviceId { get; set; }
        [Required]
        public string OuterId { get; set; }
        [Required]
        public string PassKey { get; set; }
    }

    /// <summary>
    /// 標準的人員卡號錄入
    /// </summary>
    public class PersonStdCardNoInput
    {
        [Required]
        public string MainComId { get; set; }
        [Required]
        public string DeviceId { get; set; }
        [Required]
        public string OuterId { get; set; }
        [Required]
        public string CardNo { get; set; }
    }

    /// <summary>
    /// 標準的人員頭像圖 detail 返回
    /// </summary>
    public class PicReturn
    {
        public string MainComId { get; set; }
        public string EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public string EmployeeNo { get; set; }
        public string PicturePath { get; set; }

        public string PicActuallyPath { get; set; }
    }

    /// <summary>
    /// 增加标准人员后返回的最简约的人员信息
    /// </summary>
    public class SimpleStandardUserReturn
    {
        public string MainComId { get; set; }
        /// <summary>
        /// 系统数据表的主键ID
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// 一般和 系统数据表的主键ID一致
        /// </summary>
        public string EmployeeId { get; set; }
        /// <summary>
        /// 工号ID (来自HIK的命名)
        /// </summary>
        public string EmployeeNo { get; set; }
        /// <summary>
        /// 来源于第三方系统的外部工号ID
        /// </summary>
        public string OuterId { get; set; }

        public string Name { get; set; }
         
    }

    public class PersonFromDevInput
    {  
        public string MaincomId { get; set; }
        public string OuterId { get; set; } 
        public string Name { get; set; } 
        public string CardNo { get; set; }
        public string PassKey { get; set; }
    }
    public class PersonDeleteModeInput : GlobalFieldSession
    {
        public PersonDeleteModeInput() : base()
        {
        }
        [JsonProperty("libId")]
        public int LibId { get; set; }
        [JsonProperty("personId")]
        public long PersonId { get; set; }
    }

    public class PersonIdModeInput
    {
        public PersonIdModeInput() : base()
        {
        }
        public long PersonId { get; set; }
    }

    public class EmployeeNoOrIdInput
    { 
        public string EmployeeId { get; set; }
        public string MainComId { get; set; }
    }

    public class PersonUpdateInput 
    {
        public PersonUpdateInput() : base()
        {
        }

        [JsonProperty("maincomId")]
        public string MaincomId { get; set; }

        [JsonProperty("outerId")]
        [DefaultValue("")]
        public string OuterId { get; set; }
        [JsonProperty("libId")]
        public int LibId { get; set; }
        /// <summary>
        /// 1:N 1對多的 選擇人員群組庫
        /// </summary>
        [JsonProperty("libIdGroups")]
        [DefaultValue("")]
        public string LibIdGroups { get; set; }

        [JsonProperty("personId")]
        public long PersonId { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("sex")]
        public int Sex { get; set; }
        [JsonProperty("gender")]
        public EnumBusiness.Genders Gender { get; set; }

        [JsonProperty("picUrl")]
        public string PicUrl { get; set; }

        [JsonProperty("picClientUrl")] 
        public string PicClientUrl { get; set; }

        [JsonProperty("cardNo")]
        public string CardNo { get; set; }

        [JsonProperty("passKey")]
        public string PassKey { get; set; }

        [JsonProperty("phone")]
        public string Phone { get; set; }

        [DefaultValue(PersonCategory.UNBLOCKED)]
        [JsonProperty("category")]
        public PersonCategory Category { get; set; }

        [JsonProperty("remark")]
        public string Remark { get; set; }
    }

    public class QueryPersonListInput
    {
        public QueryPersonListInput() : base()
        {
            _PageNo = 1;
            _PageSize = 64;
            _RequiredPic = true;
        }
         
        public string MaincomId { get; set; }

        private int _PageNo;
       
        public int PageNo
        {
            get
            { 
                return _PageNo;
            }
            set
            {
                _PageNo = value;
            }
        }

        private int _PageSize; 
       
        public int PageSize
        {
            get
            { 
                return _PageSize;
            }
            set
            {
                _PageSize = value;
            }
        }
         
        public string Name { get; set; }
          
        public string CardNo { get; set; }

       
        public string LibraryId { get; set; }

        private string _Category;
      
        public string Category
        {
            get
            { 
                return _Category;
            }
            set
            {
                _Category = value;
            }
        }

        private bool _RequiredPic;
       
        public bool RequiredPic
        {
            get
            {
                return _RequiredPic;
            }
            set
            {
                _RequiredPic = value;
            }
        }
        
    }

    public class QueryPersonListInfoReturn
    {
        private int _PageCount;
        [JsonProperty("pageCount")]
        public int PageCount
        {
            get
            {
                int Totalpages = (TotalCount + PageSize - 1) / PageSize;
                return Totalpages;
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
        public List<Person> Items { get; set; }
    }

    public class StandardDeviceUserListReturn
    {
        public int PageNo { get; set; }

        [DefaultValue(4)] 
        public int PageSize { get; set; }
        public int PageCount { get; set; }
        public int TotalCount { get; set; } 
        public List<StandardDeviceUser> StandardDeviceUserList { get; set; }
    }

    public class DevicePersonListReturn: QueryPersonListInfoReturn
    { 
        [JsonProperty("items")]
        public List<DevicePersonModel> DevicePersonModelList { get; set; }
    }


    public class DevicePersonModel
    { 
        public int DeviceId { get; set; } 
        public string DeviceName { get; set; }

        public long PersonId { get; set; }
        public string PersonName { get; set; }
        public int DeviceLibId { get; set; }
        public string DeviceLibName { get; set; }
        public int DownInsertStatus { get; set; }
        public DateTime DownInsertStatusDt { get; set; }
        public int DownUpdateStatus { get; set; }
        public DateTime DownUpdateStatusDt { get; set; }
        public int DownDelStatus { get; set; }
        public DateTime DownDelStatusDt { get; set; }
        public SynchronizedStatus SynchronizedStatus  { get; set; }
         
        public string MaincomId { get; set; } 
        public string OuterId { get; set; } 
        public int LibId { get; set; }
           
        public List<LibraryItemX> LibIdGroupsList { get; set; } 
        public string LibName { get; set; } 
        public string CardNo { get; set; } 
        public int Category { get; set; }  
        public string PicUrl { get; set; }  
        public string PicClientUrl { get; set; }  
    }
    public class Person
    {
        [JsonProperty("maincomId")]
        public string MaincomId { get; set; }

        [JsonProperty("outerId")]
        public string OuterId { get; set; }

        [JsonProperty("libId")]
        public int LibId { get; set; }

        [JsonProperty("libIdGroups")]
        public string LibIdGroups { get; set; }

        [JsonProperty("libIdGroupsList")]
        public List<LibraryItemX> LibIdGroupsList { get; set; }

        [JsonProperty("libName")]
        public string LibName { get; set; }

        [JsonProperty("personId")]
        public long PersonId { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("sex")]
        public int Sex { get; set; }

        [JsonProperty("cardNo")]
        public string CardNo { get; set; }

        [JsonProperty("passKey")]
        public string PassKey { get; set; }

        [JsonProperty("phone")]
        public string Phone { get; set; }

        [JsonProperty("category")]
        public int Category { get; set; }

        [JsonProperty("picUrl")]
        public string PicUrl { get; set; }

        [JsonProperty("picClientUrl")]
        public string PicClientUrl { get; set; }

        [JsonProperty("remark")]
        public string Remark { get; set; }

        [JsonProperty("createTime")]
        public string CreateTime { get; set; }
    }
    public class LibraryItemX
    {
        public int Id { get; set; }
        public int LibId { get; set; }
        public string Name { get; set; } 
    }
    
    /// <summary>
    /// 通過 卡號(card_no) 返回人員信息
    /// </summary>
    public class PersonCardInfo
    { 
        public string MaincomId { get; set; } 
        public string OuterId { get; set; }
        public sbyte Category { get; set; }
        public int LibId { get; set; }  
        public string LibIdGroups { get; set; } 
        public long PersonId { get; set; } 
        public string Name { get; set; }  
        public string CardNo { get; set; }  
        public DateTime CreateTime { get; set; }
        public DateTime UpdateTime { get; set; }
    }
    #endregion PersonApiModel

    #region UploadPersonPicture
    public class UploadPersonPicture
    {
        [JsonProperty("code")]
        public int Code { get; set; }

        [JsonProperty("picUrl")]
        public string PicUrl { get; set; }

        [JsonProperty("msg")]
        public string Msg { get; set; }
    }
    #endregion
}

