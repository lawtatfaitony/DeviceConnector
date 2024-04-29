using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using DataBaseBusiness.Models;
using EnumCode;
using LanguageResource;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace Microsoft.AspNetCore.Mvc.Rendering
{
    public static class HtmlDropDownList
    {
        /// <summary>
        /// 获取设备下拉列表
        /// </summary>
        /// <param name="htmlHelper"></param>
        /// <param name="MainComId"></param>
        /// <param name="selDeviceId">默认为0表示所有设备</param>
        /// <returns></returns>
        public static IEnumerable<SelectListItem> DeviceDropDownList(this IHtmlHelper htmlHelper, string MainComId, int selDeviceId = 0)
        {
            using (BusinessContext dbContext = new BusinessContext())
            {
                var deviceQuery = dbContext.FtDevice.Where(c => c.MaincomId == MainComId).ToList();

                if (selDeviceId != 0)
                { 
                    IEnumerable<SelectListItem> selectListItems = deviceQuery.Select(c => new SelectListItem() { Text = c.DeviceName, Value = c.DeviceId.ToString(), Selected = c.DeviceId == selDeviceId }).ToList();
                    return selectListItems;
                }
                else
                {
                    IEnumerable<SelectListItem> selectListItems = deviceQuery.Select(c => new SelectListItem() { Text = c.DeviceName, Value = c.DeviceId.ToString() }).ToList();
                    return selectListItems;
                }
            }
        }

        public static IEnumerable<SelectListItem> LibraryDropDownList(this IHtmlHelper htmlHelper, string MainComId, object objValue = null)
        {
            List<SelectListItem> selectListItems = new List<SelectListItem>();
            using BusinessContext businessContext = new BusinessContext();

            var libLists = businessContext.FtLibrary.Where(c => c.MaincomId.Contains(MainComId) && c.Visible == (int)GeneralVisible.VISIBLE).ToList();

            int selValue = 0;
            if (objValue != null)
            { 
                if(int.TryParse(objValue.ToString(),out int selV))
                {
                    selValue = selV;
                }
            }
              
            foreach (var item in libLists)
            {
                SelectListItem listItem = new SelectListItem { Text = item.Name, Value = item.Id.ToString(), Selected = item.Id == selValue };
                selectListItems.Add(listItem);
            }
            var GetSelectList = new SelectList(selectListItems, "Value", "Text", objValue);
            return GetSelectList;
        }
    }
}


