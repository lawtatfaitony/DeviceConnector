using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;
using VideoGuard.Business;
using EnumCode;
using DataBaseBusiness.Models;

namespace VideoGuard.Device
{ 
    public class DeviceListInput : AttPager
    {
        public DeviceListInput() : base()
        {
            Search = string.Empty;
            PageNo = 1;
            PageSize = 64;
            TotalCount = 0;
            TotalPage = 0;
            SortOrder = SortOrderCode.DESC;
        } 
        public string MainComId { get; set; }
    }
      
    /// <summary>
    /// DeviceId是雲端的設備定義的ID
    /// EmployeeId\EquipmentUserId\UserId 都是一個值或可以是邏輯情況返回
    /// EmployeeId是DGX系統的EmployeeId(如E20005),如果終端機不能字母,則返回的形式 是 EmployeeId=E20005 = EquipmentUserId=20005
    /// 如果 是AIG系統則 EmployeeId = PersonId 或FtPerson.OuterId返回  雲端先是通過查詢 表FtPerson where personId OR OuterId 獲得再去查設備用戶列表 取得對象
    /// </summary>
    public class TerminalEquipmentInput
    {
        [Required]
        public string MainComId { get; set; }
        [Required]
        public string DeviceId { get; set; }
        [Required]
        public string EmployeeId { get; set; }
        [Required]
        public string EquipmentUserId { get; set; }

        /// <summary>
        /// 返回三種情況的狀態結果 DevicePersonDownStatus  的字符串UpperCase
        /// 直接改為Enum模式,取消字符串形式 2022-9-5
        /// </summary>
        [Required]
        public string DevicePersonDownStatus { get; set; }

        /// <summary>
        /// 大寫形式 UpperCase 和字段名稱值一致 以便解析為常量
        /// </summary>
        [Required]
        public string DeviceOperateMode { get; set; }
        /// <summary>
        /// 大寫形式 UpperCase 和字段名稱值一致 以便解析為常量
        /// </summary>
        [Required]
        public string DeviceVerifyMode { get; set; }
    }

    

    /// <summary>
    /// 只有是无操作的情况下 提交
    /// </summary>
    public class OperateStatusSubmitInput
    {
        public string MainComId { get; set; }
        public int DeviceId { get; set; }
        public long PersonId { get; set; }
        public DeviceOperateMode DeviceOperateMode { get; set; }
    }
    /// <summary>
    /// 獲取設備配置 INPUT
    /// </summary>
    public class DeviceConfigInput
    {
        public string DeviceId { get; set; }
        public string MainComId { get; set; }
    }

    public class QueryDeviceUserInput: ListPager
    {
        public string MainComId { get; set; }
        public string DeviceId { get; set; }
        /// <summary>
        /// // ACCESS_CARD | FINGERPRINT |  FASE | PASSKEY 四种驗證模式的人员 FACE也可以通过这里获取,UserIconPositiveIsUpdate = true
        /// </summary>
        public string VerifyMode { get; set; }  
        /// <summary>
        /// 请求的操作模式
        /// </summary>
        public string DeviceOperateMode { get; set; } 
    }
    public class QueryDeviceUserModel
    {
        public string MainComId { get; set; }
        public string DeviceId { get; set; }
        public DeviceVerifyMode VerifyMode { get; set; }  // ACCESS_CARD | FINGERPRINT |  FASE | PASSKEY 四种驗證模式的人员 FACE也可以通过这里获取,UserIconPositiveIsUpdate = true
        /// <summary>
        /// 请求的操作模式
        /// </summary>
        public DeviceOperateMode DeviceOperateMode { get; set; }
    }
    /// <summary>
    /// 這裡是記錄同步下行到設備的三個介質的完成狀態標識
    /// 增加Face屬性 和 把Upd改為Sync 2022-8-31
    /// </summary>
    public class SynchronizedStatus
    {
        public SynchronizedStatus()
        {
            FaceNeedToSync = false;
            AccessCardNeedToSync = false;
            FingerPrintNeedToSync = false;
            PassKeyNeedToSync = false;
        }
        public bool FaceNeedToSync { get; set; }
        public bool AccessCardNeedToSync { get; set; }
        public bool FingerPrintNeedToSync { get; set; }
        public bool PassKeyNeedToSync { get; set; } 
    }
      
    /// <summary>
    /// 標準的設備用戶接口 改造DGX的DeviceUser
    /// 目前只是照搬DGX系統的
    /// 輸出查詢用,本身沒有實體媒介數據(人臉指紋卡號等等)
    /// </summary>
    public partial class StandardDeviceUser
    {
        /// <summary>
        /// 主鍵ID  這裡是設備ID+personId復合構成,而DGX直接就是DeviceUserId
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// 系統工號 (PersonId/EmployeeId) 可能由於各個系統不一樣導致包含特殊符號,不利於標準
        /// 和UserId 區別開來
        /// </summary>
        public string EmployeeId { get; set; }
        /// <summary>
        /// 姓名
        /// </summary>
        public string EmployName { get; set; }
        public string DeviceId { get; set; }
        public string DeviceName { get; set; }
        /// <summary>
        /// 设备成功新增用户后,把设备用户id 回发到系统 更新UserProfileId 字段,这是最后一步确实下行设备用户成功.
        /// </summary>
        public string DevidceUserProfileId { get; set; }
        /// <summary>
        /// 帶其他字符的用戶ID,硬件設備的ID,有可能是規定數字類型的,例如低級的硬件
        /// 和UserId的區別就是排除掉非數字的
        /// </summary>
        public string UserId { get; set; }
        /// <summary>
        /// 拍卡號碼 如果是16進制,還需要通過設備信息這個對象設置 
        /// 看看是否右起交叉轉為10進制的,還是左起交叉轉為10進制的
        /// </summary>
        public string AccessCardId { get; set; }
        /// <summary>
        /// 暫時沒用處理指紋的,如果要處理,可能是一個指紋文件的話,把二進制的指紋文件轉為base64保存這裡,
        /// 如果是多個指紋文件,則用JSON結構 例如 左手拇指食指 結構是 {"hand":"left","finger0(拇指)":"base64","finger1(食指)":"base64","finger2(中指)":"base64"} //沒有更多case了,就幾個主要手指
        /// </summary>
        public string FingerPrints { get; set; }

        /// <summary>
        /// 主要記錄幾個同步完成的功能 來自字段 SynchronizedStatusRemark的JSON 轉變過來,初始插入全是未同步
        /// </summary>
        public SynchronizedStatus SynchronizedStatus { get; set; }
        public string PassKey { get; set; }
        /// <summary>
        /// 這個是對於多個識別鏡頭設備使用的 即俗稱 1:N  1對多.
        /// </summary>
        public int SearchMode { get; set; }
        /// <summary>
        /// 正面相1
        /// </summary>
        public string UserIconPositive { get; set; }
        /// <summary>
        /// 正面相是否更新
        /// </summary>
        public bool UserIconPositiveIsUpdate { get; set; }
        /// <summary>
        /// 頭像相片2  [AIG系統不適用]
        /// </summary>
        public string UserIconSide { get; set; }
        /// <summary>
        /// 頭像相片2 是否更新  [AIG系統不適用]
        /// </summary>
        public bool UserIconSideIsUpdate { get; set; }
        /// <summary>
        /// 頭像相片3  [AIG系統不適用]
        /// </summary>
        public string UserIconTopView { get; set; }
        /// <summary>
        /// 頭像相片3 是否更新  [AIG系統不適用]
        /// </summary>
        public bool UserIconTopViewIsUpdate { get; set; }
        /// <summary>
        /// 頭像 更新完成否
        /// </summary>
        public bool UserIconIsCompleted { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public bool CreatedIsCompleted { get; set; }

        /// <summary>
        /// AIG的狀態分三個 分別是 down_insert_status/down_update_status/down_del_status 而 GeneralStatus是DGX 只有激活或刪除的 兩種狀態 
        /// </summary>
        public int GeneralStatus { get; set; }
    }

    public class DevicePersonListInput:AttPager
    {  
        public string MaincomId { get; set; }
        public int DeviceId { get; set; }
        
    }
     
}
