using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roster.Presentation.Helpers
{
    public class JsonTextWriter
    {
        public static string EscapeJScriptString(string s)
        {
            if (string.IsNullOrEmpty(s)) {
                return string.Empty;
            }

            StringBuilder builder = null;
            int startIndex = 0;
            int count = 0;
            for (int i = 0; i < s.Length; i++)
            {
                char ch = s[i];
                if ((((ch == '\r') || (ch == '\t')) || ((ch == '"') || (ch == '/'))) || (((ch == '\\') || (ch < ' ')) || (ch > '\x007f')))
                {
                    if (builder == null)
                    {
                        builder = new StringBuilder(s.Length + 6);
                    }
                    if (count > 0)
                    {
                        builder.Append(s, startIndex, count);
                    }
                    startIndex = i + 1;
                    count = 0;
                }
                switch (ch)
                {
                    case '/':
                        {
                            builder.Append(@"\u002f");
                            continue;
                        }
                    case '\\':
                        {
                            builder.Append(@"\\");
                            continue;
                        }
                    case '\t':
                        {
                            builder.Append(@"\t");
                            continue;
                        }
                    case '\n':
                        {
                            builder.Append(@"\n");
                            continue;
                        }
                    case '\r':
                        {
                            builder.Append(@"\r");
                            continue;
                        }
                    case '"':
                        {
                            builder.Append("\\\"");
                            continue;
                        }
                }
                if ((ch < ' ') || (ch > '\x007f')) {
                    builder.AppendFormat(CultureInfo.InvariantCulture, @"\u{0:x4}", new object[] { (int)ch });
                } else {
                    count++;
                }
            }

            string str = s;
            if (builder == null) {
                return str;
            }
            if (count > 0) {
                builder.Append(s, startIndex, count);
            }

            return builder.ToString();
        }
    }
}
