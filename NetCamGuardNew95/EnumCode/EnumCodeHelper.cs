using LanguageResource;
using System;
using System.Reflection;

namespace EnumCode
{
    public static class EnumCodeExtendHelper
    {
        /// <summary>
        /// Enum扩展 获取 EnumDescription 的KeyName的对应多语言陈述
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <returns></returns>
        internal static string GetEnumCodeDesc<T>(this T item) where T : Enum
        {
            return GetEnumCodeDescription<T>(item);
        }
        /// <summary>
        /// 获取自定义属性获取的内容
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string GetEnumCodeDescription<T>(T t) 
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
}
