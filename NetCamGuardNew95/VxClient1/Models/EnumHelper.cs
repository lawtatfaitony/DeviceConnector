using LanguageResource;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using VideoGuard.ApiModels;

namespace VxClient.Models
{

    public class EnumItem
    {
        public string Text;
        public string Value;
        public bool Selected = false;
    }
    public partial class AttEnumHelper
    { 
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

        /// <summary>
        ///  将枚举类型的值和自定义属性配对组合为 List<SelectListItem>
        /// </summary>
        /// <param name="enumType"></param>
        /// <returns></returns>
        public static List<EnumItem> GetSelectList<T>()
        {
            var enumType = typeof(T);
            List<EnumItem> selectList = new List<EnumItem>();

            foreach (var obj in Enum.GetValues(enumType))
            {
                //selectList.Add(new EnumItem { Text = LangUtilities.GetStringReflectKeyName(GetEnumDescription(obj)), Value = obj.ToString() });  
                //  selectList.Add(new EnumItem { Text = GetEnumDescription(obj), Value = obj.ToString() }); //

                selectList.Add(new EnumItem { Text = GetEnumDescription(obj), Value = obj.ToString() });
            }
            return selectList;
        }

        /// <summary>
        /// List转SelectListItem
        /// </summary>
        /// <typeparam name="T">Model对象</typeparam>
        /// <param name="t">集合</param>
        /// <param name="text">显示值-属性名</param>
        /// <param name="value">显示值-属性名</param>
        /// <param name="empId"></param>
        /// <returns></returns>
        public static List<EnumItem> CreateSelect<T>(IList<T> t, string text, string value, string selectValue)
        {
            List<EnumItem> list = new List<EnumItem>();
            foreach (var item in t)
            {
                var propers = item.GetType().GetProperty(text);
                var valpropers = item.GetType().GetProperty(value);
                list.Add(new EnumItem
                {
                    Text = propers.GetValue(item, null).ToString(),
                    Value = valpropers.GetValue(item, null).ToString(),
                    Selected = valpropers.GetValue(item, null).ToString() == selectValue
                });
            }
            return list;
        }  
    }
    public static class EnumExtendHelper
    {
        /// <summary>
        /// Enum扩展 获取 EnumDescription 的KeyName的对应多语言陈述
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <returns></returns>
        internal static string GetEnumDesc<T>(this T item) where T : Enum
        {
            return GetEnumDescription<T>(item);
        }
        /// <summary>
        /// 获取自定义属性获取的内容
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string GetEnumDescription<T>(T t) 
        {
            //获取枚举对象的枚举类型
            Type type = t.GetType();
            //通过反射获取该枚举类型的所有属性
            FieldInfo[] fieldInfos = type.GetFields();

            foreach (FieldInfo field in fieldInfos)
            {
                //不是参数obj,就直接跳过

                if (field.Name != t.ToString())
                {
                    continue;
                }

                //取出参数obj的自定义属性
                foreach (var attrItem in field.CustomAttributes)
                {
                    if (attrItem.AttributeType.Name == typeof(EnumDisplayNameAttribute).Name)
                    {
                        string dip = attrItem.ConstructorArguments[0].Value.ToString();
                        dip = LangUtilities.GetStringReflectKeyName(dip);
                        return dip;
                    }
                }
            }
            return t.ToString();
        }
          
    }
    public static class EnumKit
    {
        #region 获取枚举的描述  

        /// <summary>  
        /// 获取枚举的描述信息  
        /// </summary>  
        /// <param name="enumValue">枚举值</param>  
        /// <returns>描述</returns>  
        public static string GetDescriptionX(this Enum enumValue)
        {
            string value = enumValue.ToString();
            FieldInfo field = enumValue.GetType().GetField(value);
            object[] objs = field.GetCustomAttributes(typeof(System.ComponentModel.DescriptionAttribute), false);
            if (objs == null || objs.Length == 0) return value;
            System.ComponentModel.DescriptionAttribute attr = (System.ComponentModel.DescriptionAttribute)objs[0];
            string desc = LangUtilities.GetStringReflectKeyName(attr.Description);
            return desc;
        }

        #endregion
    }
}
