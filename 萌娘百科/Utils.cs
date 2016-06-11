using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MoeGirl
{
    public class Utils
    {
        public static string RemoveTagsAndDecode(string str)
        {
            MatchCollection mcTag = new Regex("<.+?>").Matches(str);
            foreach (Match mTag in mcTag)
                str = str.Replace(mTag.Value, string.Empty);
            str = HttpUtility.HtmlDecode(HttpUtility.HtmlDecode(str.Trim()));
            return str;
        }
    }
}
