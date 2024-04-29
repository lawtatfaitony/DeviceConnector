using LanguageResource;
using System;
using System.Reflection;
using static EnumCode.EnumBusiness;

namespace VideoGuard.ApiModels
{
    /// <summary>
    /// 公司常量值 對標Attendnace System 默認值
    /// </summary>
    public class MainCom
    { 
        public string MainComId { get; set; } = "6000014";
        public string SiteId { get; set; } = "S2000";
        public string DepartmentId { get; set; } = "D2000";
        public string PositionId { get; set; } = "P2000";
        public string JobId { get; set; } = "J2000";
        public string SystemDefaultUser { get; set; } = "SYSTEM";
         public string SysModuleId { get; set; } = SysModuleType.DEVICE.ToString();
        public string Remarks { get; set; } = "Default Value";
    }


    public class EnumDisplayNameAttribute : Attribute
    {
        private string _diaplayName;
        public string DisplayName
        {
            get
            {
                // return _diaplayName; //LangUtilities.GetStringReflectKeyName(displayName);
                _diaplayName = LangUtilities.GetStringReflectKeyName(_diaplayName); //Modified in 2022-6-27
                return _diaplayName; //LangUtilities.GetStringReflectKeyName(displayName);
            }
        }
        public EnumDisplayNameAttribute(string displayName)
        {
            _diaplayName = displayName;
        }

        public static string GetEnumDescription<T>(T t) where T : Enum
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