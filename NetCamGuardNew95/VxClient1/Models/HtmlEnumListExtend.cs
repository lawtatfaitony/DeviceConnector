using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using EnumCode;
using LanguageResource;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Newtonsoft.Json;
using static EnumCode.EnumBusiness;

namespace Microsoft.AspNetCore.Mvc.Rendering
{
    public static class HtmlEnumListExtend
    {
        public static List<EnumItem> GetEnumSelectList<T>()
        {
            var enumType = typeof(T);
            List<EnumItem> selectList = new List<EnumItem>();

            foreach (var obj in Enum.GetValues(enumType))
            {
                //selectList.Add(new EnumItem { Text = LangUtilities.GetStringReflectKeyName(GetEnumDescription(obj)), Value = obj.ToString() }); //原始句法
                //GetEnumDescription()函數的_displayName已經反射KeyName,無需要重複 
                selectList.Add(new EnumItem { Text = GetEnumDescription(obj), Value = obj.ToString() });
            }
            return selectList;
        }
        /// <summary>
        /// 获取自定义属性获取的内容
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string GetEnumDescription(Object obj)
        {
            //获取枚举对象的枚举类型
            Type type = obj.GetType();
            //通过反射获取该枚举类型的所有属性
            FieldInfo[] fieldInfos = type.GetFields();

            foreach (FieldInfo field in fieldInfos)
            {
                //不是参数obj,就直接跳过

                if (field.Name != obj.ToString())
                {
                    continue;
                }
                //取出参数obj的自定义属性
                if (field.IsDefined(typeof(EnumDisplayNameAttribute), true))
                {
                    string dip = (field.GetCustomAttributes(typeof(EnumDisplayNameAttribute), true)[0] as EnumDisplayNameAttribute).DisplayName;
                    return dip;
                }
            }
            return obj.ToString();
        }

        public static string DateTimeFormate(this IHtmlHelper htmlHelper, DateTime dateTime)
        {
            return String.Format(@"{0:yyyy-MM-dd HH:mm:ss}", dateTime);
        }
        /// <summary>
        /// 任务类型 ： 人脸识别 车牌识别 等等
        /// </summary>
        /// <param name="htmlHelper"></param>
        /// <param name="selVal"></param>
        /// <returns></returns>
        public static IEnumerable<SelectListItem> TaskTypeDropDownList(this IHtmlHelper htmlHelper, string selVal = null)
        {
            if (string.IsNullOrEmpty(selVal))
                selVal = string.Empty;

            var taskEnumTypeQuery = GetEnumSelectList<TaskType>();
            IEnumerable<SelectListItem> selectListItems = taskEnumTypeQuery.Select(c => new SelectListItem() { Text = c.Text, Value = c.Value, Selected = c.Value == selVal }).ToList();
            return selectListItems;
        }

        /// <summary>
        /// On Duty PaidRatio
        /// </summary>
        /// <param name="selectedValue">1.0</param>
        public static IEnumerable<SelectListItem> OnDutyPaidRatioDropDownList(this IHtmlHelper htmlHelper, string selectedValue)
        {
            if (string.IsNullOrEmpty(selectedValue))
            {
                selectedValue = string.Empty;
            }

            List<SelectListItem> selectLists = new List<SelectListItem>();
            for (int i = 10; i <= 30; i++)
            {
                string Text1 = string.Format("{0}%", i * 10);
                float V1 = (float)i / 10;
                string Value1 = string.Format("{0:f1}", V1);
                bool Selected1 = false;
                if (Text1 == "100%" && string.IsNullOrEmpty(selectedValue))
                {
                    Selected1 = true;
                }
                if (selectedValue.Trim() == Value1)
                {
                    Selected1 = true;
                }
                SelectListItem selectListItem = new SelectListItem { Text = Text1, Value = Value1, Selected = Selected1 };
                selectLists.Add(selectListItem);
            }
            selectLists.Insert(0, new SelectListItem { Text = "0%", Value = "0" });
            IEnumerable<SelectListItem> selectListItems = selectLists.Select(c => new SelectListItem() { Text = c.Text, Value = c.Value }).ToList();
            return selectListItems;
        }


        /// <summary>
        /// 性別下拉列表
        /// </summary>
        /// <param name="htmlHelper"></param>
        /// <param name="selVal"></param>
        /// <returns></returns>
        public static IEnumerable<SelectListItem> GendersDropDownList(this IHtmlHelper htmlHelper, string selVal = null)
        {
            var GenderQuery = GetEnumSelectList<EnumBusiness.Genders>();
            IEnumerable<SelectListItem> selectListItems;
            if (selVal != null)
            {
                selVal = selVal.ToUpper();
                selectListItems = GenderQuery.Select(c => new SelectListItem() { Text = c.Text, Value = c.Value, Selected = c.Value.ToUpper() == selVal }).ToList();
                return selectListItems;
            }
            else
            {
                selectListItems = GenderQuery.Select(c => new SelectListItem() { Text = c.Text, Value = c.Value }).ToList();
                return selectListItems;
            }
        }
        /// <summary>
        /// 通用狀態下拉 使用中 | 停用中
        /// </summary>
        /// <param name="htmlHelper"></param>
        /// <param name="selValue"></param>
        /// <returns></returns>
        public static IEnumerable<SelectListItem> GeneralStatusDropDownList(this IHtmlHelper htmlHelper, string selValue = null)
        {
            var GeneralStatusList = GetEnumSelectList<GeneralStatus>();
            if (string.IsNullOrEmpty(selValue))
                selValue = string.Empty;
            IEnumerable<SelectListItem> selectListGeneralStatus = GeneralStatusList.Select(c => new SelectListItem()
            {
                Text = LangUtilities.GetStringReflectKeyName(c.Text),
                Value = c.Value,
                Selected = c.Value.ToString().ToUpper() == selValue.ToUpper()
            });

            return selectListGeneralStatus;
        }

        /// <summary>
        /// 設備出入口列表ddl
        /// </summary>
        /// <param name="htmlHelper"></param>
        /// <param name="selValue"></param>
        /// <returns></returns>
        public static IEnumerable<SelectListItem> DeviceEntryModeDropDownList(this IHtmlHelper htmlHelper, string selValue)
        {
            var DeviceEntryModeList = GetEnumSelectList<EnumBusiness.DeviceEntryMode>();

            if (string.IsNullOrEmpty(selValue))
                selValue = string.Empty;

            IEnumerable<SelectListItem> selectList = DeviceEntryModeList.Select(c => new SelectListItem()
            {
                Text = LangUtilities.GetStringReflectKeyName(c.Text),
                Value = c.Value,
                Selected = c.Value.ToString().ToUpper() == selValue
            });

            return selectList;
        }

        /// <summary>
        /// 設備类型列表
        /// </summary>
        /// <param name="htmlHelper"></param>
        /// <param name="selValue"></param>
        /// <returns></returns>
        public static IEnumerable<SelectListItem> DeviceTypeDropDownList(this IHtmlHelper htmlHelper, string selValue)
        {
            var DeviceTypeList = GetEnumSelectList<EnumBusiness.DeviceType>();

            if (string.IsNullOrEmpty(selValue))
                selValue = string.Empty;

            IEnumerable<SelectListItem> selectList = DeviceTypeList.Select(c => new SelectListItem()
            {
                Text = LangUtilities.GetStringReflectKeyName(c.Text),
                Value = c.Value,
                Selected = c.Value == selValue.ToString().ToUpper()
            });

            return selectList;


        }
        /// <summary>
        /// 設備类型列表
        /// </summary>
        /// <param name="htmlHelper"></param>
        /// <param name="selValue"></param>
        /// <returns></returns>
        public static IEnumerable<SelectListItem> CameraRecordStatusDropDownList(this IHtmlHelper htmlHelper, string selValue)
        {
            var CameraRecordStatusList = GetEnumSelectList<CameraRecordStatus>();

            if (string.IsNullOrEmpty(selValue))
                selValue = string.Empty;

            IEnumerable<SelectListItem> selectList = CameraRecordStatusList.Select(c => new SelectListItem()
            {
                Text = LangUtilities.GetStringReflectKeyName(c.Text),
                Value = c.Value,
                Selected = c.Value == selValue.ToString().ToUpper()
            });

            return selectList;


        }

        /// <summary>
        /// 人員群組庫 :   基本固定的:規定群組庫,  人員底庫類型 分為: 拍卡\人臉\指紋\GPS\二維碼\ 拍卡、人臉、指紋、GPS或二維碼
        /// </summary>
        /// <param name="htmlHelper"></param>
        /// <param name="selValue"></param>
        /// <returns></returns>
        public static IEnumerable<SelectListItem> LibraryTypeCodeDropDownList(this IHtmlHelper htmlHelper, string selValue)
        {
            var libraryTypeCodeList = GetEnumSelectList<LibraryTypeCode>();

            if (string.IsNullOrEmpty(selValue))
                selValue = string.Empty;

            IEnumerable<SelectListItem> selectList = libraryTypeCodeList.Select(c => new SelectListItem()
            {
                Text = LangUtilities.GetStringReflectKeyName(c.Text),
                Value = c.Value,
                Selected = c.Value == selValue.ToString().ToUpper()
            });

            return selectList;
        }

        /// <summary>
        /// PersonCategory 列表 / 通用人员,访客/被禁止的人员
        /// </summary>
        /// <param name="htmlHelper"></param>
        /// <param name="selValue"></param>
        /// <returns></returns>
        public static IEnumerable<SelectListItem> PersonCategoryDropDownList(this IHtmlHelper htmlHelper, string selValue)
        {
            var PersonCategoryList = GetEnumSelectList<VideoGuard.ApiModels.PersonCategory>();

            if (string.IsNullOrEmpty(selValue))
                selValue = string.Empty;

            IEnumerable<SelectListItem> selectList = PersonCategoryList.Select(c => new SelectListItem()
            {
                Text = LangUtilities.GetStringReflectKeyName(c.Text),
                Value = c.Value,
                Selected = c.Value.ToString().ToUpper() == selValue
            });

            return selectList;
        }

        public static IEnumerable<SelectListItem> DeviceOperateModeDropDownList(this IHtmlHelper htmlHelper, string selValue = null)
        {
            var DeviceOperateModeList = GetEnumSelectList<DeviceOperateMode>();

            if (string.IsNullOrEmpty(selValue))
                selValue = DeviceOperateMode.DEVICE_OPERATE_MODE_ALL.ToString();

            IEnumerable<SelectListItem> selectList = DeviceOperateModeList.Select(c => new SelectListItem()
            {
                Text = LangUtilities.GetStringReflectKeyName(c.Text),
                Value = c.Value,
                Selected = c.Value.ToString().ToUpper() == selValue
            });

            return selectList;
        }

        /// <summary>
        /// 錄像系統類型（目前有兩款錄像軟件類型：Media.exe 和 MediaGuard.exe）
        /// </summary>
        /// <param name="htmlHelper"></param>
        /// <param name="selValue"></param>
        /// <returns></returns>
        public static IEnumerable<SelectListItem> DeviceNVRTypeDropDownList(this IHtmlHelper htmlHelper, string selValue = null)
        {
            var DeviceNVRTypeModeList = GetEnumSelectList<NVR_TYPE>();

            if (string.IsNullOrEmpty(selValue))
                selValue = NVR_TYPE.MEDIA_GUARD.ToString();

            IEnumerable<SelectListItem> selectList = DeviceNVRTypeModeList.Select(c => new SelectListItem()
            {
                Text = LangUtilities.GetStringReflectKeyName(c.Text),
                Value = c.Value,
                Selected = c.Value.ToString().ToUpper() == selValue
            });

            return selectList;
        }
    }
}


