using System;
using System.ComponentModel;
using System.Reflection;

namespace EnumCode
{
    /// <summary>
    /// PictureErrorCode rename as [PersonCode]. ONLY REPRESENT FOR A CASE OF CODE TO RETURN. 
    /// </summary>
    public enum FileErrorCode
    { 
        [EnumDisplayName("FILE_BACKEND_CLOSED")]
        FILE_BACKEND_CLOSED = 7001  
    }
}