using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace yfxsApp.runtime
{
    public class NavDic:Dictionary<string,string>
    {
        /// <summary>
        /// 将内容转换为编码的URL字符串,不带?前缀
        /// </summary>
        /// <returns></returns>
        public string ToPostString()
        {
            StringBuilder sb = new StringBuilder();
            bool first = true;
            foreach (string a in base.Keys)
            {
                if (!first)
                {
                    sb.Append("&");
                }
                else
                {
                    first = false;
                }
                sb.Append(a);
                sb.Append("=");
                sb.Append(System.Net.WebUtility.UrlEncode(base[a]));
            }
            return sb.ToString();
        }
        /// <summary>
        /// 将内容转换为编码的URL字符串,带?前缀
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("?");
            bool first=true ;
            foreach (string a in Keys)
            {
                if(!first )
                {
                    sb.Append("&");
                }
                else
                {
                    first = false;
                }
                sb.Append(a);
                sb.Append("=");
                sb.Append(System.Net.WebUtility.UrlEncode(base[a]));
            }
            return sb.ToString();
        }
        /// <summary>
        /// 从URL字符串初始化<see cref="NavDic"/>
        /// </summary>
        /// <param name="formatString"></param>
        /// <returns></returns>
        public static NavDic FormatFormString(string formatString)
        {
            if (formatString == null)
                throw new System.ArgumentNullException("url");

            NavDic nvc = new NavDic();

            if (formatString == "")
                return nvc;

            int questionMarkIndex = formatString.IndexOf('?');

            if (questionMarkIndex == -1)
            {
                return nvc;
            }
            if (questionMarkIndex == formatString.Length - 1)
                return nvc;
            string ps = formatString.Substring(questionMarkIndex + 1);

            // 开始分析参数对    
            System.Text.RegularExpressions.Regex re = new System.Text.RegularExpressions.Regex(@"(^|&)?(\w+)=([^&]+)(&|$)?");
            foreach (System.Text.RegularExpressions.Match m in re.Matches(ps))
            {
                nvc.Add(m.Result("$2").ToLower(), System.Net.WebUtility.UrlDecode(m.Result("$3")));
            }
            return nvc;
        }
    }
}
