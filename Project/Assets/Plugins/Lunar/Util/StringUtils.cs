//
//  StringUtils.cs
//
//  Lunar Plugin for Unity: a command line solution for your game.
//  https://github.com/SpaceMadness/lunar-unity-plugin
//
//  Copyright 2015 Alex Lementuev, SpaceMadness.
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
//

﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

using UnityEngine;

namespace LunarPluginInternal
{
    public static class StringUtils
    {
        private static readonly char[] kSpaceSplitChars = { ' ' };

        private static readonly Regex kRichTagRegex = new Regex("(<color=.*?>)|(<b>)|(<i>)|(</color>)|(</b>)|(</i>)");
        private static readonly Regex kColorCodeTagRegex = new Regex("<color=\\$(\\d+)>");

        internal static string TryFormat(string format, params object[] args)
        {
            if (format != null && args != null && args.Length > 0)
            {
                try
                {
                    return string.Format(format, args);
                }
                catch (Exception e)
                {
                    Debug.LogError("Error while formatting string: " + e.Message);
                }
            }

            return format;
        }

        //////////////////////////////////////////////////////////////////////////////

        public static bool StartsWithIgnoreCase(string str, string prefix)
        {
            return str != null && prefix != null && str.StartsWith(prefix, StringComparison.OrdinalIgnoreCase);
        }

        public static bool EqualsIgnoreCase(string a, string b)
        {
            return string.Equals(a, b, StringComparison.OrdinalIgnoreCase);
        }

        //////////////////////////////////////////////////////////////////////////////

        #region Parsing

        public static int ParseInt(string str)
        {
            return ParseInt(str, 0);
        }

        public static int ParseInt(string str, int defValue)
        {   
            if (!string.IsNullOrEmpty(str))
            {
                int value;
                bool succeed = int.TryParse(str, out value);
                return succeed ? value : defValue;
            }

            return defValue;
        }

        public static int ParseInt(string str, out bool succeed)
        {
            if (!string.IsNullOrEmpty(str))
            {
                int value;
                succeed = int.TryParse(str, out value);
                return succeed ? value : 0;
            }

            succeed = false;
            return 0;
        }

        public static float ParseFloat(string str)
        {
            return ParseFloat(str, 0.0f);
        }

        public static float ParseFloat(string str, float defValue)
        {
            if (!string.IsNullOrEmpty(str))
            {
                float value;
                bool succeed = float.TryParse(str, out value);
                return succeed ? value : defValue;
            }

            return defValue;
        }

        public static float ParseFloat(string str, out bool succeed)
        {
            if (!string.IsNullOrEmpty(str))
            {
                float value;
                succeed = float.TryParse(str, out value);
                return succeed ? value : 0.0f;
            }

            succeed = false;
            return 0.0f;
        }

        public static bool ParseBool(string str)
        {
            return ParseBool(str, false);
        }

        public static bool ParseBool(string str, bool defValue)
        {
            if (!string.IsNullOrEmpty(str))
            {
                bool value;
                bool succeed = bool.TryParse(str, out value);
                return succeed ? value : defValue;
            }

            return defValue;
        }

        public static bool ParseBool(string str, out bool succeed)
        {
            if (!string.IsNullOrEmpty(str))
            {
                bool value;
                succeed = bool.TryParse(str, out value);
                return succeed ? value : false;
            }

            succeed = false;
            return false;
        }

        public static float[] ParseFloats(string str)
        {
            return str != null ? ParseFloats(str.Split(kSpaceSplitChars, StringSplitOptions.RemoveEmptyEntries)) : null;
        }

        public static float[] ParseFloats(string[] args)
        {
            if (args != null)
            {
                float[] floats = new float[args.Length];
                for (int i = 0; i < args.Length; ++i)
                {
                    if (!float.TryParse(args[i], out floats[i]))
                    {
                        return null;
                    }
                }

                return floats;
            }

            return null;
        }

        public static bool IsNumeric(string str)
        {
            double value;
            return double.TryParse(str, out value);
        }

        public static bool IsInteger(string str)
        {
            int value;
            return int.TryParse(str, out value);
        }

        #endregion

        //////////////////////////////////////////////////////////////////////////////

        #region Words

        internal static int StartOfTheWordOffset(string value, int index)
        {
            return StartOfTheWord(value, index) - index;
        }

        internal static int StartOfTheWord(string value, int index)
        {
            int i = index - 1;

            while (i >= 0 && IsSeparator(value[i]))
            {
                --i;
            }

            while (i >= 0 && !IsSeparator(value[i]))
            {
                --i;
            }

            return i + 1;
        }

        internal static int EndOfTheWordOffset(string value, int index)
        {
            return EndOfTheWord(value, index) - index;
        }

        internal static int EndOfTheWord(string value, int index)
        {
            int i = index;

            while (i < value.Length && IsSeparator(value[i]))
            {
                ++i;
            }

            while (i < value.Length && !IsSeparator(value[i]))
            {
                ++i;
            }

            return i;
        }

        private static bool IsSeparator(char ch)
        {
            return !(char.IsLetter(ch) || char.IsDigit(ch));
        }

        #endregion

        //////////////////////////////////////////////////////////////////////////////

        #region Lines

        internal static int MoveLineUp(string value, int index)
        {
            if (index > 0 && index <= value.Length)
            {
                int startOfPrevLineIndex = StartOfPrevLineIndex(value, index);
                if (startOfPrevLineIndex == -1) // this is the first line (or single line): can't move up
                {
                    return index;
                }

                int offsetInLine = OffsetInLine(value, index);
                int endOfPrevLineIndex = EndOfPrevLineIndex(value, index);

                return Mathf.Min(startOfPrevLineIndex + offsetInLine, endOfPrevLineIndex);
            }

            return index;
        }

        internal static int MoveLineDown(string value, int index)
        {
            if (index >= 0 && index < value.Length)
            {
                int startOfNextLineIndex = StartOfNextLineIndex(value, index);
                if (startOfNextLineIndex == -1)
                {
                    return index; // the last line (of single line): can't move down
                }

                int offsetInLine = OffsetInLine(value, index);
                int endOfNextLineIndex = EndOfNextLineIndex(value, index);

                return Mathf.Min(startOfNextLineIndex + offsetInLine, endOfNextLineIndex);
            }

            return index;
        }

        internal static int StartOfLineOffset(string value, int index)
        {
            return StartOfLineIndex(value, index) - index;
        }

        internal static int StartOfLineIndex(string value, int index)
        {
            return index > 0 ? value.LastIndexOf('\n', index - 1) + 1 : 0;
        }

        internal static int EndOfLineOffset(string value, int index)
        {
            return EndOfLineIndex(value, index) - index;
        }

        internal static int EndOfLineIndex(string value, int index)
        {
            if (index < value.Length)
            {
                int nextLineBreakIndex = value.IndexOf('\n', index);
                if (nextLineBreakIndex != -1)
                {
                    return nextLineBreakIndex;
                }
            }

            return value.Length;
        }

        internal static int OffsetInLine(string value, int index)
        {
            return index - StartOfLineIndex(value, index);
        }

        internal static int StartOfPrevLineIndex(string value, int index)
        {
            int endOfPrevLine = EndOfPrevLineIndex(value, index);
            return endOfPrevLine != -1 ? StartOfLineIndex(value, endOfPrevLine) : -1;
        }

        internal static int EndOfPrevLineIndex(string value, int index)
        {
            return StartOfLineIndex(value, index) - 1;
        }

        internal static int StartOfNextLineIndex(string value, int index)
        {
            int endOfLineIndex = EndOfLineIndex(value, index);
            return endOfLineIndex < value.Length ? endOfLineIndex + 1 : -1;
        }

        internal static int EndOfNextLineIndex(string value, int index)
        {
            int startOfNextLineIndex = StartOfNextLineIndex(value, index);
            return startOfNextLineIndex != -1 ? EndOfLineIndex(value, startOfNextLineIndex) : -1;
        }

        internal static int LinesBreaksCount(string value)
        {
            if (value != null)
            {
                int count = 0;
                for (int i = 0; i < value.Length; ++i)
                {
                    if (value[i] == '\n')
                    {
                        ++count;
                    }
                }

                return count;
            }

            return 0;
        }

        internal static int Strlen(string str)
        {
            return str != null ? str.Length : 0;
        }

        #endregion

        //////////////////////////////////////////////////////////////////////////////

        private static List<string> s_tempList;

        internal static string GetSuggestedText(string token, string[] strings, bool removeTags = false)
        {
            return GetSuggestedText0(token, strings, removeTags);
        }

        internal static string GetSuggestedText(string token, IList<string> strings, bool removeTags = false)
        {
            return GetSuggestedText0(token, (IList)strings, removeTags);
        }

        private static string GetSuggestedText0(string token, IList strings, bool removeTags)
        {
            if (token == null)
            {
                return null;
            }

            if (s_tempList == null) s_tempList = new List<string>(); // TODO: use 'recyclable' list
            else s_tempList.Clear();

            foreach (string str in strings)
            {
                string temp = removeTags ? RemoveRichTextTags(str) : str;
                if (token.Length == 0 || StartsWithIgnoreCase(temp, token))
                {
                    s_tempList.Add(temp);
                }
            }

            return GetSuggestedTextFiltered0(token, s_tempList);
        }

        internal static string GetSuggestedTextFiltered(string token, IList<string> strings)
        {
            return GetSuggestedTextFiltered0(token, (IList)strings);
        }

        internal static string GetSuggestedTextFiltered(string token, string[] strings)
        {
            return GetSuggestedTextFiltered0(token, strings);
        }

        private static string GetSuggestedTextFiltered0(string token, IList strings)
        {
            if (token == null) return null;
            if (strings.Count == 0) return null;
            if (strings.Count == 1) return (string)strings[0];

            string firstString = (string)strings[0];
            if (token.Length == 0)
            {
                token = firstString;
            }

            StringBuilder suggestedToken = new StringBuilder();

            for (int charIndex = 0; charIndex < firstString.Length; ++charIndex)
            {
                char chr = firstString[charIndex];
                char chrLower = char.ToLower(chr);
                
                bool searchFinished = false;
                for (int strIndex = 1; strIndex < strings.Count; ++strIndex)
                {
                    string otherString = (string)strings[strIndex];
                    if (charIndex >= otherString.Length || char.ToLower(otherString[charIndex]) != chrLower)
                    {
                        searchFinished = true;
                        break;
                    }
                }
                
                if (searchFinished)
                {
                    return suggestedToken.Length > 0 ? suggestedToken.ToString() : null;
                }
                
                suggestedToken.Append(chr);
            }
            
            return suggestedToken.Length > 0 ? suggestedToken.ToString() : null;
        }

        //////////////////////////////////////////////////////////////////////////////

        private static readonly string Quote = "\"";
        private static readonly string SingleQuote = "'";

        private static readonly string EscapedQuote = "\\\"";
        private static readonly string EscapedSingleQuote = "\\'";

        internal static string Arg(string value)
        {
            if (value != null && value.Length > 0)
            {
                value = value.Replace(Quote, EscapedQuote);
                value = value.Replace(SingleQuote, EscapedSingleQuote);

                if (value.IndexOf(' ') != -1)
                {
                    value = StringUtils.TryFormat("\"{0}\"", value);
                }

                return value;
            }

            return "\"\"";
        }

        internal static string UnArg(string value)
        {
            if (value != null && value.Length > 0)
            {
                if (value.StartsWith(Quote) && value.EndsWith(Quote) ||
                    value.StartsWith(SingleQuote) && value.EndsWith(SingleQuote))
                {
                    value = value.Substring(1, value.Length - 2);
                }

                value = value.Replace(EscapedQuote, Quote);
                value = value.Replace(EscapedSingleQuote, SingleQuote);
                
                return value;
            }
            
            return "";
        }

        internal static string C(string str, ColorCode color)
        {
            return StringUtils.TryFormat("<color=${0}>{1}</color>", ((int)color).ToString(), str);
        }

        internal static string C(string str, Color color, int startIndex, int endIndex)
        {
            return StringUtils.TryFormat("{0}<color=#{1}>{2}</color>{3}", str.Substring(0, startIndex), 
                ToHexValue(ref color), 
                str.Substring(startIndex, endIndex - startIndex), 
                str.Substring(endIndex)
            );
        }

        public static string C(string str, Color color)
        {
            return StringUtils.TryFormat("<color=#{0}>{1}</color>", ToHexValue(ref color), str);
        }

        private static string ToHexValue(ref Color color)
        {
            int value = (((int)(255 * color.r)) << 24) |
                (((int)(255 * color.g)) << 16) |
                (((int)(255 * color.b)) << 8) |
                (color.a > 0 ? (int)(255 * color.a) : 0xff);

            return value.ToString("x8");
        }

        public static string B(string str)
        {
            return StringUtils.TryFormat("<b>{0}</b>", str);
        }

        public static string I(string str)
        {
            return StringUtils.TryFormat("<i>{0}</i>", str);
        }

        public static string RemoveRichTextTags(string line)
        {
            return kRichTagRegex.Replace(line, String.Empty);
        }

        internal static T RemoveRichTextTags<T>(T lines) where T : IList<String>
        {
            for (int i = 0; i < lines.Count; ++i)
            {
                lines[i] = RemoveRichTextTags(lines[i]);
            }
            return lines;
        }

        internal static string SetColors(string line, Color[] colorLookup)
        {
            Match match = kColorCodeTagRegex.Match(line);

            if (match.Success)
            {
                StringBuilder buffer = new StringBuilder(line.Length); // FIXME: use objects pool

                int lastIndex = 0;
                do
                {
                    int index = match.Index;
                    int length = match.Length;

                    buffer.Append(line, lastIndex, index - lastIndex);

                    int lookupIndex = UnsafeParseInt(match.Groups[1].Value);
                    if (lookupIndex < colorLookup.Length)
                    {
                        buffer.AppendFormat("<color=#{0}>", ToHexValue(ref colorLookup[lookupIndex]));
                    }
                    else
                    {
                        buffer.Append(match.Value);
                    }

                    lastIndex = index + length;
                }
                while ((match = match.NextMatch()).Success);

                if (lastIndex < line.Length)
                {
                    buffer.Append(line, lastIndex, line.Length - lastIndex);
                }

                return buffer.ToString();
            }

            return line;
        }

        private static int UnsafeParseInt(string str)
        {
            int value = 0;
            for (int i = 0; i < str.Length; ++i)
            {
                value = value * 10 + (str[i] - '0');
            }

            return value;
        }

        //////////////////////////////////////////////////////////////////////////////

        #region Null and stuff

        internal static string NonNullOrEmpty(string str)
        {
            return str != null ? str : "";
        }

        #endregion

        //////////////////////////////////////////////////////////////////////////////

        #region string representation

        internal static string ToString(int value)
        {
            return value.ToString();
        }

        internal static string ToString(float value)
        {
            return value.ToString("G");
        }

        internal static string ToString(bool value)
        {
            return value.ToString();
        }

        internal static string ToString(ref Color value)
        {
            if (value.a > 0.0f)
            {
                return string.Format("{0} {1} {2} {3}", 
                    value.r.ToString("G"), 
                    value.g.ToString("G"), 
                    value.b.ToString("G"), 
                    value.a.ToString("G")
                );
            }

            return string.Format("{0} {1} {2}", 
                value.r.ToString("G"), 
                value.g.ToString("G"), 
                value.b.ToString("G")
            );
        }

        internal static string ToString(ref Rect value)
        {
            return string.Format("{0} {1} {2} {3}", 
                value.x.ToString("G"), 
                value.y.ToString("G"), 
                value.width.ToString("G"), 
                value.height.ToString("G")
            );
        }

        internal static string ToString(ref Vector2 value)
        {
            return string.Format("{0} {1}", 
                value.x.ToString("G"), 
                value.y.ToString("G")
            );
        }

        internal static string ToString(ref Vector3 value)
        {
            return string.Format("{0} {1} {2}", 
                value.x.ToString("G"), 
                value.y.ToString("G"), 
                value.z.ToString("G")
            );
        }

        internal static string ToString(ref Vector4 value)
        {
            return string.Format("{0} {1} {2} {3}", 
                value.x.ToString("G"), 
                value.y.ToString("G"), 
                value.z.ToString("G"), 
                value.w.ToString("G")
            );
        }

        public static string Join<T>(IList<T> list, string separator = ",")
        {
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < list.Count; ++i)
            {
                builder.Append(list[i]);
                if (i < list.Count-1) builder.Append(separator);
            }
            return builder.ToString();
        }

        #endregion
    }
}
