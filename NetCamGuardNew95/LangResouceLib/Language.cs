namespace LanguageResource.Modal
{
    using System;

    /// <summary>
    /// 多语言  zh(中文地区): zh-TW(台湾) zh-CN(大陆) zh-HK(香港) zh-SG(新加波) zh-MO(澳门) 
    ///         参考代码:https://msdn.microsoft.com/zh-cn/library/kx54z3k7(VS.80).aspx
    /// </summary> 
    [Serializable]
    public partial class Language
    {
        public string KeyName { get; set; }

        public string KeyType { get; set; }
        public string Zh_CN { get; set; }
        public string Zh_HK { get; set; }
        public string En_US { get; set; }
        public string IndustryIdArr { get; set; }
        public string MainComIdArr { get; set; }
        public string Remark { get; set; }
    }
}
