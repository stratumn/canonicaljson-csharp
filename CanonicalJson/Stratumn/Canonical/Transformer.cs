
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Deveel.Math;
using Newtonsoft.Json;

using Stratumn.CanonicalJson.Helpers;

namespace Stratumn.CanonicalJson
{

    /// <summary>
    /// @copyright Stratum
    ///  Transformer converts and JSON stream to an object Vector / Map / Java Object
    /// </summary>
    public class Transformer
    {

        private StringBuilder buffer;
        /* Regular expressions that matches characters otherwise inexpressible in 
          JSON (U+0022 QUOTATION MARK, U+005C REVERSE SOLIDUS, 
         and ASCII control characters U+0000 through U+001F) or UTF-8 (U+D800 through U+DFFF) */
        private static readonly Regex FORBIDDEN = new Regex("[\\u0022\\u005c\\u0000-\\u001F\\ud800-\\udfff]", RegexOptions.IgnoreCase);

        public string Transform(Object obj)
        {
            buffer = new StringBuilder();
            Serialize(obj);
            return buffer.ToString();
        }

        private void Escape(char c)
        {
            buffer.Append(Constants.C_BACK_SLASH).Append(c);
        }

        /***
          * MUST represent all strings (including object member names) in their minimal-length UTF-8 encoding
           * avoiding escape sequences for characters except those otherwise inexpressible in JSON (U+0022 QUOTATION MARK, U+005C REVERSE SOLIDUS, and ASCII control characters U+0000 through U+001F) or UTF-8 (U+D800 through U+DFFF), and
           * avoiding escape sequences for combining characters, variation selectors, and other code points that affect preceding characters, and
           * using two-character escape sequences where possible for characters that require escaping:
           * \b U+0008 BACKSPACE
           * \t U+0009 CHARACTER TABULATION ("tab")
           * \n U+000A LINE FEED ("newline")
           * \f U+000C FORM FEED
           * \r U+000D CARRIAGE RETURN
           * \" U+0022 QUOTATION MARK
           * \\ U+005C REVERSE SOLIDUS ("backslash"), and
           * using six-character \\u00xx uppercase hexadecimal escape sequences for control characters that require escaping but lack a two-character sequence, and
           * using six-character \\uDxxx uppercase hexadecimal escape sequences for lone surrogates
          * @param value
          */
        private void SerializeString(string value)
        {
            buffer.Append(Constants.C_DOUBLE_QUOTE);
            if (FORBIDDEN.Matches(value).Count == 0)
            {
                buffer.Append(value);
            }
            else
            {
                char[] chars = value.ToCharArray();
                for (int i = 0; i < chars.Length; i++)
                {
                    char c = chars[i];
                    {
                        if (FORBIDDEN.Matches(Convert.ToString(c)).Count == 0)
                        {
                            buffer.Append(c);
                            continue;
                        }

                        // The "high surrogate" is the leading character defining the surrogate pair,
                        // the "low surrogate" is the trailing character.
                        if (Char.IsHighSurrogate(chars[i]) && chars.Length > i + 1 && Char.IsSurrogatePair(chars[i], chars[i + 1]))
                        {
                            buffer.Append(Char.ConvertFromUtf32(CharHelper.ToCodePoint(chars[i], chars[++i])));
                            continue;
                        }
                        switch (c)
                        {
                            case Constants.C_LINE_FEED:
                                Escape('n');
                                break;
                            case Constants.C_BACKSPACE:
                                Escape('b');
                                break;

                            case Constants.C_FORM_FEED:
                                Escape('f');
                                break;

                            case Constants.C_CARRIAGE_RETURN:
                                Escape('r');
                                break;

                            case Constants.C_TAB:
                                Escape('t');
                                break;

                            case Constants.C_DOUBLE_QUOTE:
                            case Constants.C_BACK_SLASH:
                                Escape(c);
                                break;

                            default:

                                Escape('u');
                                string hex = string.Format("{0:x4}", (int)c).ToUpper();
                                buffer.Append(hex);
                                break;

                        }
                    }
                }
            }
            buffer.Append(Constants.C_DOUBLE_QUOTE);
        }

        /***
         *  MUST represent all integer numbers (those with a zero-valued fractional part)
        * without a leading minus sign when the value is zero, and
        * without a decimal point, and
        * without an exponent
        *
        * MUST represent all non-integer numbers in exponential notation
        * including a nonzero single-digit significant integer part, and
        * including a nonempty significant fractional part, and
        * including no trailing zeroes in the significant fractional part (other than as part of a ".0" required to satisfy the preceding point), and
        * including a capital "E", and
        * including no plus sign in the exponent, and
        * including no insignificant leading zeroes in the exponent
         * @param bd
         * @throws IOException
         */

        private void SerializeNumber(string value)
        {
            var bd = BigDecimal.Parse(value);

            // Check for integers -> whole format
            // bd is whole if its scale is less or equal to 0
            BigInteger bdUnscaled = bd.UnscaledValue;
            int bdScale = bd.Scale;
            // Try to normalize scale to 0
            if (bdScale != 0)
            {
                while (bdUnscaled % 10 == 0 && bdScale > 0)
                {
                    bdUnscaled /= 10;
                    bdScale--;
                }
                while (bdScale < 0)
                {
                    bdUnscaled *= 10;
                    bdScale++;
                }
            }

            if (bdScale == 0)
            {
                // This is a System.Numerics.BigInteger:
                // we need it because Deveel.Math.BigInteger.ToString
                // and Deveel.Math.BigDecimal.ToString have unwanted behavior
                // that force exponential notation or adds ".0" to the integer
                var sysWhole = System.Numerics.BigInteger.Parse(bdUnscaled.ToString());
                buffer.Append(sysWhole.ToString("D"));
            }
            else
            // Decimals -> force exponential notation,
            // with the value before the exponential normalized to < 10
            {
                // For example, 3.14 will be represented by:
                // unscaled = 314 and scale = 2
                BigInteger unscaled = bd.UnscaledValue;
                int exponent = -bd.Scale;
                // unscaled should not be a multiple of 10
                while (unscaled % 10 == 0)
                {
                    unscaled /= 10;
                    exponent++;
                }
                string unscaledString = unscaled.ToString();
                // Dividing the unscaled integer to only have one leading digit
                // (which is equivalent to 10 to the amount of digits - 1, and the sign taken into account
                // since we're operating with strings)
                exponent += unscaledString.Length - (bd.Sign < 0 ? 2 : 1);
                int decimalIndex = bd.Sign < 0 ? 2 : 1;
                string wholeString = unscaledString.Substring(0, decimalIndex);
                string decimalString = unscaledString.Substring(decimalIndex);
                if (decimalString == "")
                {
                    decimalString = "0";
                }
                buffer.Append(wholeString + "." + decimalString + "E" + exponent);
            }

        }

        private void Serialize(Object o)
        {
            if (o == null)
            {
                buffer.Append("null");
            }
            else if (o is IDictionary<string, Object>)
            {
                SortedDictionary<String, Object> sortedTree = new SortedDictionary<String, Object>(new LexComparator());
                var tree = (IDictionary<string, Object>)o;
                tree.ToList().ForEach(t => sortedTree.Add(t.Key, t.Value));
                buffer.Append('{');
                bool next = false;
                foreach (KeyValuePair<string, object> keyValue in sortedTree.SetOfKeyValuePairs())
                {
                    if (next)
                    {
                        buffer.Append(',');
                    }
                    next = true;
                    SerializeString(keyValue.Key);
                    buffer.Append(':');
                    Debug.WriteLine(keyValue.Value);
                    Serialize(keyValue.Value);
                }
                buffer.Append('}');
            }
            else
              if (o is List<Object>)
            {

                buffer.Append('[');
                bool next = false;

                foreach (Object value in (List<Object>)o)
                {
                    if (next)
                    {
                        buffer.Append(',');
                    }
                    next = true;
                    Serialize(value);
                }
                buffer.Append(']');
            }
            else if (o is string)
            {
                SerializeString((string)o);
            }
            else if (o is bool?)
            {
                buffer.Append(((bool?)o).ToString().ToLower());
            }
            else if (o is double? || o is decimal || o is int? || o is BigDecimal)
            {
                SerializeNumber(o.ToString());
            }
            else
            {

                try
                {
                    //attempt to serialize unknown type.
                    String json = JsonConvert.SerializeObject(o, JsonSerializeSettings);
                    //parse and searialize it to make sure its canonicalized.
                    Serialize(new Parser(json).Parse());
                }
                catch (Exception)
                {
                    throw new ApplicationException("Unknown object: " + o);
                }
                
            }

        }

        public string GetEncodedString()
        {

            return buffer.ToString();
        }

        public byte[] GetEncodedUTF8()
        {
            return Encoding.UTF8.GetBytes(GetEncodedString());
        }

        
        private JsonSerializerSettings JsonSerializeSettings;
        /// <summary>
        /// To support Json serialization of objectsS
        /// </summary>
        /// <returns></returns>
        private JsonSerializerSettings GetJsonSerializeSettings()
        { 
            if (this.JsonSerializeSettings == null)
            {
                JsonSerializeSettings = new JsonSerializerSettings();
                JsonSerializeSettings.NullValueHandling = NullValueHandling.Include;
                JsonSerializeSettings.StringEscapeHandling = StringEscapeHandling.EscapeNonAscii;
            }
            return JsonSerializeSettings;


        }
    }
}
