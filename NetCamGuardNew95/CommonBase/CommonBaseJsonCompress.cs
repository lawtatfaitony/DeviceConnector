using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace Common
{
    public partial class CommonBase
    {
        public static string JsonCompress(string json)
        {
            StringBuilder sb = new StringBuilder();
            using (StringReader reader = new StringReader(json))
            {
                int ch = -1;
                int lastch = -1;
                bool isQuoteStart = false;
                while ((ch = reader.Read()) > -1)
                {
                    if ((char)lastch != '\\' && (char)ch == '\"')
                    {
                        if (!isQuoteStart)
                        {
                            isQuoteStart = true;
                        }
                        else
                        {
                            isQuoteStart = false;
                        }
                    }
                    if (!Char.IsWhiteSpace((char)ch) || isQuoteStart)
                    {
                        sb.Append((char)ch);
                    }
                    lastch = ch;
                }
            }
            return sb.ToString();
        }
        public static string JsonFormat(string json)
        {
            string strCompress = JsonCompress(json);
            StringBuilder sb = new StringBuilder();
            #region format

            using (StringReader reader = new StringReader(strCompress))
            {
                using (StringWriter writer = new StringWriter(sb))
                {
                    int ch = -1;
                    int lastch = -1;
                    bool isQuoteStart = false;
                    while ((ch = reader.Read()) > -1)
                    {
                        StringBuilder temp = new StringBuilder();
                        switch ((char)ch)
                        {
                            case '{':
                                if (isQuoteStart)
                                {
                                    temp.Append('{');
                                }
                                else
                                {
                                    temp.Append('{');
                                    if ((char)reader.Peek() != '}')
                                    {
                                        temp.Append(Environment.NewLine);
                                    }
                                }
                                break;
                            case '}':
                                if (isQuoteStart)
                                {
                                    temp.Append('}');
                                }
                                else
                                {
                                    if ((char)lastch != '{' && (char)lastch != '}')
                                    {
                                        temp.Append(Environment.NewLine);
                                    }
                                    temp.Append('}');
                                    if ((char)reader.Peek() != ',')
                                    {
                                        temp.Append(Environment.NewLine);
                                    }
                                }
                                break;
                            case '[':
                                if (isQuoteStart)
                                {
                                    temp.Append('[');
                                }
                                else
                                {
                                    temp.Append('[');
                                    if ((char)reader.Peek() != ']')
                                    {
                                        temp.Append(Environment.NewLine);
                                    }
                                }
                                break;
                            case ']':
                                if (isQuoteStart)
                                {
                                    temp.Append(']');
                                }
                                else
                                {
                                    if ((char)lastch != '[' && (char)lastch != ']')
                                    {
                                        temp.Append(Environment.NewLine);
                                    }
                                    temp.Append(']');
                                    if ((char)reader.Peek() != ',' && (char)reader.Peek() != '}')
                                    {
                                        temp.Append(Environment.NewLine);
                                    }
                                }
                                break;
                            case '\"':
                                if ((char)lastch != '\\')
                                {
                                    if (!isQuoteStart)
                                    {
                                        isQuoteStart = true;
                                    }
                                    else
                                    {
                                        isQuoteStart = false;
                                    }
                                }
                                temp.Append("\"");
                                break;
                            case ':':
                                if (isQuoteStart)
                                {
                                    temp.Append(':');
                                }
                                else
                                {
                                    temp.Append(':');
                                    temp.Append(" ");
                                }
                                break;
                            case ',':
                                if (isQuoteStart)
                                {
                                    temp.Append(',');
                                }
                                else
                                {
                                    temp.Append(',');
                                    temp.Append(Environment.NewLine);
                                }
                                break;
                            case ' ':
                                if (isQuoteStart)
                                {
                                    temp.Append(" ");
                                }
                                else
                                {
                                    temp.Append("");
                                    temp.Append(Environment.NewLine);
                                }
                                break;
                            default:
                                temp.Append((char)ch);
                                break;
                        }
                        writer.Write(temp.ToString());
                        lastch = ch;
                    }
                }
            }
            #endregion format

            #region indent
            StringBuilder res = new StringBuilder();
            using (StringReader reader = new StringReader(sb.ToString()))
            {
                using (StringWriter writer = new StringWriter(res))
                {
                    string str = null;

                    int nspace = 0;
                    string space = "\t";
                    bool bEndMid = false;
                    while ((str = reader.ReadLine()) != null)
                    {
                        if (str.Length == 0) continue;
                        if (str.EndsWith("},"))
                        {
                            nspace--;
                        }
                        StringBuilder temp = new StringBuilder();
                        if (!bEndMid)
                        {
                            for (int i = 0; i < (str.EndsWith("],") || (str.EndsWith("}") && !str.EndsWith("{}")) || str.EndsWith("]") ? nspace - 1 : nspace); i++)
                            {
                                temp.Append(space);
                            }
                        }

                        temp.Append(str);
                        if (str.EndsWith("["))
                        {
                            writer.Write(temp);
                            bEndMid = true;
                        }
                        else
                        {
                            writer.WriteLine(temp);
                            bEndMid = false;
                        }
                        if (!(str.EndsWith("{}") || str.EndsWith("[]")))
                        {
                            if (str.StartsWith("{") || str.EndsWith("{") ||
                                str.EndsWith("["))
                            {
                                nspace++;
                            }
                            if (str.EndsWith("}") || str.EndsWith("]"))
                            {
                                nspace--;
                            }
                        }
                    }
                }
            }
            return res.ToString();
            #endregion indent
        }
    }
}
