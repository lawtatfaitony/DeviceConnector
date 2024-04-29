using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LanguageResource
{
    public partial class Lang
    {
        private static string _LanguageCode; 

        public static string LanguageCode
        {
            get
            {
                return _LanguageCode;
            }
            set
            {
                _LanguageCode = LangUtilities.StandardLanguageCode(value);
                LangUtilities.LanguageCode = _LanguageCode;
            }
        }

    }

}
