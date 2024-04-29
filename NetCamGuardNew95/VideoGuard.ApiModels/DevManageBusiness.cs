using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Common;
using DataBaseBusiness.Models;
using EnumCode;
using LanguageResource;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using VideoGuard.ApiModels; 
using VideoGuard.Device;
using static EnumCode.EnumBusiness; 
namespace VideoGuard.Business
{
    public partial class DevManageBusiness
    {
        /// <summary>
        /// 更新设备用户
        /// 当选择所属的群组库 则更新或插入
        /// </summary>
        /// <param name="devicePersons"></param>
        public static void DevicePersonAddUpdate(List<DevicePerson> devicePersons)
        {
            if (devicePersons == null)
                return;

            using BusinessContext businessContext = new BusinessContext();
            DateTime dt = DateTime.Now;  
            try
            {
                foreach (var item in devicePersons)
                {
                    FtDevicePerson devicePerson = businessContext.FtDevicePerson.Where(c => c.DeviceId == item.DeviceId && c.PersonId == item.PersonId).FirstOrDefault();
                    if (devicePerson == null)
                    {
                        FtDevicePerson ftDevicePerson = new FtDevicePerson
                        {
                            DeviceId = item.DeviceId,
                            DeviceName = item.DeviceName,
                            PersonId = item.PersonId,
                            PersonName = item.PersonName,
                            LibId = item.LibId,
                            LibName = item.LibName,
                            MaincomId = item.MaincomId,
                            DownInsertStatus = (int)DevicePersonDownInsertStatus.DEVICE_PERSON_DOWN_INSERT_WAIT,  //只有这个状态下才会被同步。请求设备同步列表按此为准 一旦插入设备用户列表，则会设置此状态
                            DownInsertStatusDt = dt,
                            DownUpdateStatus = (int)DevicePersonDownUpdateStatus.DEVICE_PERSON_DOWN_UPDATE_NO_OPERATE, //初始为没有任何操作
                            DownUpdateStatusDt = dt,
                            DownDelStatus = (int)DevicePersonDownDelStatus.DEVICE_PERSON_DOWN_DEL_NO_OPERATE, //初始为没有任何操作
                            DownDelStatusDt = dt,
                            SynchronizedStatusRemark = item.SynchronizedStatusRemark
                        };
                        businessContext.FtDevicePerson.Add(ftDevicePerson);
                    }
                    else
                    {
                        //前端一次判斷
                        //1．時間是今天的,JOB週期內的,例如作業週期是1小時,那麼條件 update>= 作業週期內 <= update + 1小時 而且SynchronizedStatus = True。
                        //2．設備端判斷：設備中沒有對應的介質，則更新。有則也表示成功,把SynchronizedStatus的對應的介質 = false。 
                        //3．前端必須回傳更新結果，成功後，必須把SynchronizedStatus的對應的介質 = false。 
                        //4．只有设备媒介才會被更新 才会改变DownUpdateStatus = (int)DevicePersonDownUpdateStatus.DEVICE_PERSON_DOWN_UPDATE_WAIT,
                        //5．前端判斷四個介質更新完畢後,把狀態改為完成.DEVICE_PERSON_DOWN_UPDATE_COMPLETE 
                        //6．devicePerson.DownUpdateStatus = (int)DevicePersonDownUpdateStatus.DEVICE_PERSON_DOWN_UPDATE_WAIT;
                        //7．前端判斷創建日期和更新日期不一致，表示後台有更新邏輯更改。down_insert_status_dt 不等於 down_update_status_dt

                        SynchronizedStatus synchronizedStatusRaw = JsonConvert.DeserializeObject<SynchronizedStatus>(devicePerson.SynchronizedStatusRemark);

                        bool isUpdate = synchronizedStatusRaw == item.SynchronizedStatus;
                        if (isUpdate) //如果原來沒有的情況下
                        {
                            devicePerson.DownUpdateStatusDt = DateTime.Now;

                            synchronizedStatusRaw.AccessCardNeedToSync = synchronizedStatusRaw.AccessCardNeedToSync == false && item.SynchronizedStatus.AccessCardNeedToSync == true;
                            synchronizedStatusRaw.FaceNeedToSync = synchronizedStatusRaw.FaceNeedToSync == false && item.SynchronizedStatus.FaceNeedToSync == true;
                            synchronizedStatusRaw.FingerPrintNeedToSync = synchronizedStatusRaw.FingerPrintNeedToSync == false && item.SynchronizedStatus.FingerPrintNeedToSync == true;
                            synchronizedStatusRaw.PassKeyNeedToSync = synchronizedStatusRaw.PassKeyNeedToSync == false && item.SynchronizedStatus.PassKeyNeedToSync == true;
 
                            devicePerson.DownUpdateStatus = (int)DevicePersonDownUpdateStatus.DEVICE_PERSON_DOWN_UPDATE_WAIT; 
                            devicePerson.SynchronizedStatusRemark = JsonConvert.SerializeObject(synchronizedStatusRaw);
                        } 
                        devicePerson.DeviceName = item.DeviceName;
                        devicePerson.PersonName = item.PersonName; 
                        businessContext.FtDevicePerson.Update(devicePerson);
                    }
                }
                businessContext.SaveChanges(); 
            }
            catch (Exception ex)
            {
                string logline = $"[FUNC::DeviceBusiness.DevicePersonAddUpdate()][ADD OR UPDATE][EXCEPTION][{ex.Message}]";
                CommonBase.ConsoleWriteline(logline);
                return ;
            }
        }
        /// <summary>
        /// 检测需要同步删除的用户
        /// 当选择所属的群组库 则更新或插入之后，重新遍历设备用户，有哪些用户已经不存在当前群组库，标志为删除
        /// </summary>
        /// <param name="deviceId"></param>
        public static int DevicePersonCheckDelete(int deviceId)
        {
            if (deviceId == 0)
                return 0; //0 record
            using BusinessContext businessContext = new BusinessContext();
            var device = businessContext.FtDevice.Find(deviceId);

            //查询没有删除的（没有任何删除操作的记录）
            var deviceUsers = businessContext.FtDevicePerson.Where(c => c.DeviceId == deviceId && c.DownDelStatus == (int)DevicePersonDownDelStatus.DEVICE_PERSON_DOWN_DEL_NO_OPERATE);
            var persons = businessContext.FtPerson.Select(s => new { s.Id, s.LibId, s.LibIdGroups }).Where(c => c.LibId == device.LibId || c.LibIdGroups.Contains(device.LibId.ToString()));
            
            // var selPerson = persons.Where(c=>c.Id.CompareTo(ftDevicePerson.Select(s=>s.PersonId).ToList()))
            var needToChangedIntoDeletedUsers = from d in deviceUsers
                                                where !(from p in persons
                                                        select p.Id).Contains((long)d.PersonId)
                                                select d;
            needToChangedIntoDeletedUsers.ToList().ForEach(c => c.DownDelStatus = (int)DevicePersonDownDelStatus.DEVICE_PERSON_DOWN_DEL_WAIT);
            try
            {
                businessContext.FtDevicePerson.UpdateRange(needToChangedIntoDeletedUsers);
                int deletedStatusRec = businessContext.SaveChanges();
                return deletedStatusRec;
            }
            catch(Exception ex)
            {
                string loggerline = $"[FUNC::DevManageBusiness.DevicePersonCheckDelete()][{ex.Message}]";
                CommonBase.OperateDateLoger(loggerline);
                return -1;
            }
        }

        /// <summary>
        /// 删除设备用户表里的 当前Person
        /// 凡是字段含有personId=xxx都要删除
        /// 目的:由于用户禁止(BLOCKED)或删除(INVISIBLE)一个person的操作,为了防止漏洞,必须指示下行删除设备了的当前人员
        /// 具体:改变 down_del_status = WAIT 等待执行
        /// </summary>
        /// <param name="deviceId"></param>
        /// <returns></returns>
        public static bool DevicePersonChangeDeleteToWait(long personId, out int handleRecord)
        {
            handleRecord = 0;
            if (personId == 0)  //非法数据
                return false;

            using BusinessContext businessContext = new BusinessContext();
            var devicePerson = businessContext.FtDevicePerson.Where(c => c.PersonId == personId).ToList();
            if (devicePerson == null)
                return false;

            DateTime dt = DateTime.Now;
            devicePerson.ForEach(c =>{ c.DownDelStatus = (int)DevicePersonDownDelStatus.DEVICE_PERSON_DOWN_DEL_WAIT;c.DownDelStatusDt = dt; }) ;
            try
            {
                businessContext.FtDevicePerson.UpdateRange(devicePerson);
                handleRecord = businessContext.SaveChanges();
                bool result = handleRecord > 0;
                return result;
            }
            catch (Exception ex)
            {
                string loggerline = $"[FUNC::DevManageBusiness.DevicePersonChangeDeleteToWait()][{ex.Message}]";
                CommonBase.OperateDateLoger(loggerline);
                return false;
            }
        }

        /// <summary>
        /// 獲取標準的設備用戶資料包括設備介質  FINGERPRINT ( ACCESS_CARD | FINGERPRINT | FACE | PASSKEY  
        /// </summary>
        /// <param name="deviceId"></param>
        /// <param name="personId"></param>
        /// <param name="responseModalX"></param>
        /// <returns></returns>
        public static StandardDeviceUser GetStandardDeviceUser(string uploadFolderPath,int deviceId,long personId, out ResponseModalX responseModalX)
        {
            responseModalX = new ResponseModalX();

            if (deviceId == 0 || personId==0)
            {
                responseModalX.meta = new MetaModalX { Success = false, ErrorCode = (int)DeviceErrorCode.ILLEGAL_DEVICE_ID, Message = $"deviceId{deviceId} OR personId{personId} ERROR" };
                return null;
            }

            BusinessContext businessContext = new BusinessContext();

            var device = businessContext.FtDevice.Find(deviceId);
            if (device == null)
            {
                responseModalX.meta = new MetaModalX { Success = false, ErrorCode = (int)DeviceErrorCode.DEVICE_NOT_EXIST, Message = $"{Lang.DEVICE_NOT_EXIST}" };
                return null;
            }

            var person = PersonBusiness.GetPersonDetails(personId,out responseModalX);
            SynchronizedStatus synchronizedStatus = new SynchronizedStatus();
            StandardDeviceUser standardDeviceUser = new StandardDeviceUser
            {
                Id = $"{deviceId}_{personId}",
                EmployeeId = person.OuterId,
                EmployName = person.Name,
                DeviceId = deviceId.ToString(),
                DeviceName = device.DeviceName,
                DevidceUserProfileId = person.PersonId.ToString(), //现在是用PersonId作为此值，如果第三方工号会出现不规则情况，兼容性差 //用於同步成功到設備的用戶ID,由於兼容性問題:例如低級設備不允許字母等等,則 由同步的程序自行判斷去掉特殊符號後,返回正確的user id 並更新到雲端記錄
                UserId =  person.PersonId.ToString(),
                AccessCardId = person.CardNo,
                FingerPrints = string.Empty, // 系統沒有提供指紋功能,後續設計是json格式的字符串
                SynchronizedStatus = new SynchronizedStatus(), //返回默認,下一步將邏輯處理 (關鍵) 
                PassKey = person.PassKey, //"123456", // 系統沒有提供 密碼鍵功能功能,默認是123456 ,如果設備有此功能則默認是這個值 
                SearchMode = 1, ///暫時沒用,主要是多個鏡頭識別用
                UserIconPositive = PersonBusiness.TransPersonActualPicture(uploadFolderPath, person.PicClientUrl),
                UserIconPositiveIsUpdate = !string.IsNullOrEmpty(person.PicClientUrl),
                UserIconSide = string.Empty,
                UserIconSideIsUpdate = false,
                UserIconTopView = string.Empty,
                UserIconTopViewIsUpdate = false,
                UserIconIsCompleted = synchronizedStatus.FaceNeedToSync,
                CreatedDate = DateTime.Now, //後續deviceperson處理
                UpdatedDate = DateTime.Now, //後續deviceperson處理
                CreatedIsCompleted = false ,//指設備完成創建一個用戶
                GeneralStatus = (int)GeneralStatus.ACTIVE  //平台已經邏輯上是ACTIVE,所以這裡恆定值ACTIVE 例如判斷了BLOCKED, VISIBLE等等問題
            };

            return standardDeviceUser;
        }

        /// <summary>
        /// 取得需要更新的设备人员 VerifyMode = ACCESS_CARD/FINGERPRINT/PASSKEY/FACE
        /// 返回null = 没有任何更新的人员
        /// </summary>
        /// <param name="uploadFolderPath"></param>
        /// <param name="deviceVerifyMode"></param>
        /// <param name="listUsers"></param>
        /// <returns></returns>
        public static List<StandardDeviceUser> GetDeviceUsersByNeedToSync(string uploadFolderPath, DeviceVerifyMode deviceVerifyMode, List<FtDevicePerson> listUsers)
        {
            List<StandardDeviceUser> standardDeviceUserLists = new List<StandardDeviceUser>();

            foreach (var item in listUsers)
            {
                StandardDeviceUser standardDeviceUser = DevManageBusiness.GetStandardDeviceUser(uploadFolderPath, item.DeviceId, item.PersonId, out ResponseModalX responseModalX);
                if (responseModalX.meta.Success == false)
                {
                    string loggerLine = $"[FUNC::DeviceManageController.GetDeviceUsersByNeedToUpd()][{responseModalX.meta.Message}]";
                    CommonBase.OperateDateLoger(loggerLine);
                }
                if (ValidJson.IsJson(item.SynchronizedStatusRemark))
                {
                    try
                    {
                        SynchronizedStatus synchronizedStatus = JsonConvert.DeserializeObject<SynchronizedStatus>(item.SynchronizedStatusRemark);
                        switch (deviceVerifyMode)
                        {
                            case DeviceVerifyMode.ACCESS_CARD:
                                if (synchronizedStatus.AccessCardNeedToSync && !string.IsNullOrEmpty(standardDeviceUser.AccessCardId))
                                {
                                    standardDeviceUser.SynchronizedStatus = synchronizedStatus;
                                    standardDeviceUserLists.Add(standardDeviceUser);
                                }
                                break;

                            case DeviceVerifyMode.FINGERPRINT:

                                if (synchronizedStatus.FingerPrintNeedToSync && !string.IsNullOrEmpty(standardDeviceUser.FingerPrints))
                                {
                                    standardDeviceUser.SynchronizedStatus = synchronizedStatus;
                                    standardDeviceUserLists.Add(standardDeviceUser);
                                }
                                break;

                            case DeviceVerifyMode.PASSKEY:
                                if (synchronizedStatus.PassKeyNeedToSync && !string.IsNullOrEmpty(standardDeviceUser.PassKey))
                                {
                                    standardDeviceUser.SynchronizedStatus = synchronizedStatus;
                                    standardDeviceUserLists.Add(standardDeviceUser);
                                }
                                break;

                            case DeviceVerifyMode.FACE:
                                if (synchronizedStatus.FaceNeedToSync && !string.IsNullOrEmpty(standardDeviceUser.UserIconPositive))
                                {
                                    standardDeviceUser.SynchronizedStatus = synchronizedStatus;
                                    standardDeviceUserLists.Add(standardDeviceUser);
                                }
                                break;
                            default:
                                standardDeviceUserLists.Add(standardDeviceUser);
                                break;
                        }
                    }
                    catch (Exception e)
                    {
                        string err = string.Format("[{0:F}][FUNC::DeviceManageController.GetDeviceUsersByNeedToUpd][{1}]", DateTime.Now, e.Message);
                        CommonBase.OperateDateLoger(err, CommonBase.LoggerMode.FATAL);
                        return null;
                    }
                }
                else  //[DeviceUser.SynchronizedStatusRemark]必须要有JSON格式
                {
                    string loggerLine = $"[FUNC::DeviceManageController.GetDeviceUsersByNeedToUpd()][DeviceUser.SynchronizedStatusRemark][INVALID JSON]";
                    CommonBase.OperateDateLoger(loggerLine, CommonBase.LoggerMode.FATAL);
                }
            }
            return standardDeviceUserLists;
        }

        #region 變更設備用戶的狀態 三個操作的狀態 INSERT UPDATE DELETE
        public static ResponseModalX ChangeDeviceUsersStatusSyncCallBack(string uploadFolderPath, DeviceOperateMode deviceOperateMode, DeviceVerifyMode deviceVerifyMode, DevicePersonDownStatus devicePersonDownStatus, int deviceId, long personId)
        {
            ResponseModalX responseModalX = new ResponseModalX();
            
            using BusinessContext businessContext = new BusinessContext();

            var devicePerson = businessContext.FtDevicePerson.Where(c => c.DeviceId == deviceId && c.PersonId == personId).FirstOrDefault();

            if (devicePerson == null)
            {
                responseModalX.meta = new MetaModalX { ErrorCode = (int)GeneralReturnCode.GENERALUI_NO_RECORD, Success = false, Message = $"{Lang.GeneralUI_Fail} TABLE::FtDevicePerson NOT EXIST RECORD" };
                return responseModalX;
            }

            if(!ValidJson.IsJson(devicePerson.SynchronizedStatusRemark))
            {
                responseModalX.meta = new MetaModalX { ErrorCode = (int)GeneralReturnCode.FAIL, Success = false, Message = $"{Lang.GeneralUI_Fail} TABLE::FtDevicePerson.SynchronizedStatusRemark Reuired JSON FORMAT" };
                return responseModalX;
            }
            
            try
            {
                switch (deviceOperateMode)
                {
                    case DeviceOperateMode.DEVICE_OPERATE_MODE_INSERT:
                          
                        devicePerson.DownInsertStatus = (int)devicePersonDownStatus;
                        devicePerson.DownInsertStatusDt = DateTime.Now;
                        break;

                    case DeviceOperateMode.DEVICE_OPERATE_MODE_UPDATE:
                         
                        devicePerson.DownUpdateStatus = (int)devicePersonDownStatus;
                        devicePerson.DownUpdateStatusDt = DateTime.Now;
                        break; 

                    case DeviceOperateMode.DEVICE_OPERATE_MODE_DELETE:
                         
                        devicePerson.DownDelStatus = (int)devicePersonDownStatus;
                        devicePerson.DownDelStatusDt = DateTime.Now;
                        break; 

                    default:
                        break;
                }
            }
            catch (Exception e)
            {
                string err = $"[FUNC::DevManageBusiness.ChangeDeviceUsersStatusSyncCallBack][DeviceOperateMode][{e.Message}]"; 
                CommonBase.OperateDateLoger(err, CommonBase.LoggerMode.FATAL); 
                responseModalX.meta = new MetaModalX { ErrorCode = (int)GeneralReturnCode.FAIL, Success = false, Message = $"{Lang.GeneralUI_Fail} {err}" };
                return responseModalX; 
            }

            SynchronizedStatus synchronizedStatus = JsonConvert.DeserializeObject<SynchronizedStatus>(devicePerson.SynchronizedStatusRemark);

            switch (deviceVerifyMode)
            {
                case DeviceVerifyMode.ACCESS_CARD:
                    synchronizedStatus.AccessCardNeedToSync = false;
                    break;

                case DeviceVerifyMode.FINGERPRINT:
                    synchronizedStatus.FingerPrintNeedToSync = false;
                    break;

                case DeviceVerifyMode.PASSKEY:
                    synchronizedStatus.PassKeyNeedToSync = false;
                    break;

                case DeviceVerifyMode.FACE:
                    synchronizedStatus.FaceNeedToSync = false;
                    break;
                default:
                    break;
            }

            devicePerson.SynchronizedStatusRemark = JsonConvert.SerializeObject(synchronizedStatus);

            try
            {
                businessContext.FtDevicePerson.Update(devicePerson);
                businessContext.SaveChanges();

                responseModalX.meta = new MetaModalX { ErrorCode = (int)GeneralReturnCode.SUCCESS, Success = true, Message = $"{Lang.GeneralUI_OK} [DEVICE USER STATUS UPDATE SUCCESS]" };

                StandardDeviceUser standardDeviceUser = DevManageBusiness.GetStandardDeviceUser(uploadFolderPath,devicePerson.DeviceId, devicePerson.PersonId, out ResponseModalX responseModalStdDevUser);
                if(responseModalStdDevUser.meta.Success)
                {
                    responseModalX.data = devicePerson;
                } 
                return responseModalX;
            }
            catch(Exception ex)
            {
                string err = $"[FUNC::DevManageBusiness.ChangeDeviceUsersStatusSyncCallBack][DATABASE SAVE][{ex.Message}]";
                responseModalX.meta = new MetaModalX { ErrorCode = (int)GeneralReturnCode.FAIL, Success = false, Message = $"{Lang.GeneralUI_Fail} {err}" };
                return responseModalX;
            }
        }
        #endregion

    }
    public class DevicePerson:FtDevicePerson
    {
        public SynchronizedStatus SynchronizedStatus { get; set; }
    }
}
