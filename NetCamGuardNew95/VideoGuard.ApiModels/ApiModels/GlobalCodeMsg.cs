namespace VideoGuard.ApiModels
{
    public abstract class GlobalCodeMsg
    {
        public int Code = 0;
        public string Msg { get; set; } = string.Empty;
    }
    public class GlobalReturn: GlobalCodeMsg
    {
        
    }
    //2022年7月2日 去掉 貌似没用任何地方引用
    //public class LanguageUtilities 
    //{
    //    public string LanguageCode{ get; set; } = "en-US";
    //}
     
}

