using System;
using System.ComponentModel;
using System.Reflection;
namespace EnumCode
{ 
    public enum SortOrderCode
    {
        [EnumDisplayName("GeneralUI_ASC")]
        ASC = -1,
        [EnumDisplayName("GeneralUI_DESC")]
        DESC = 1
    }
     
}
