using LanguageResource;
using System;
using System.Collections.Generic;
using System.Text;

namespace EnumCode
{
    public class EnumDisplayNameAttribute : Attribute
    {
        public EnumDisplayNameAttribute(string displayName)
        {
            _diaplayName = LangUtilities.GetStringReflectKeyName(displayName);
        }
        private string _diaplayName;
        public string DisplayName
        {
            get
            { 
                return _diaplayName;
            }
        }
    }
}
