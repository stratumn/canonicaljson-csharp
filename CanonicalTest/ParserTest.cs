using System;
using System.Collections.Generic;
using Stratumn.CanonicalJson;
using Xunit;
using Deveel.Math;

namespace Stratumn.CanonicalJsonTest
{
    /// <summary>
    ///  @copyright Stratumn
    /// </summary>
    public class ParserTest
    {
        [Theory]
        [InlineData("{\"a\":\"b\"}", "a", "b")]
        [InlineData("{\"string\": \"\\u20ac$\\u000F\\u000aA'\\u0042\\u0022\\u005c\\\\\\\"\\/\"}", "string", "\u20ac$\u000F\u000aA'\u0042\u0022\u005c\\\"/")]
        public void TestSanity(string str, string key, string value)
        {
            var res = (SortedDictionary<string, object>) new Parser(str).Parse();
            // There should only be one key/value pair and it should match the provided key and value
            Assert.Collection(res, kvp => {
                Assert.Equal(key, kvp.Key);
                Assert.Equal(value, kvp.Value);
            });
        }
        public static IEnumerable<object[]> GetData()
        {
            yield return new object[] {"false", false, "Boolean false"};
            yield return new object[] {"true", true, "Boolean true"};
            yield return new object[] {"null", null, "Null"};
            yield return new object[] {"100E+100", BigDecimal.Parse("100e100"), "Big integers"};
            yield return new object[] {"-1", new BigDecimal(-1), "Negative integers"};
            yield return new object[] {"1.21e1", BigDecimal.Parse("12.1"), "Decimals"};
            yield return new object[] {"\"\\ufb01\"", "ﬁ", "Unicode codepoint literals"};
            yield return new object[] {"\"\\b\"", "\b", "Escapes"};
            yield return new object[] {"[]", new List<Object>(), "Empty arrays"};
            yield return new object[] {"[\"a\", 1, true]", new List<Object>(new List<Object>(new Object[] {"a", new BigDecimal(1), true})), "Arbitrary arrays"};
        }

        [Theory]
        [MemberData(nameof(GetData))]
        public void TestValid(string input, object expected, string description)
        {
            Object result;
            result = new Parser(input).Parse();
            if (expected == null)
            {
                Assert.True(result == null, description);
            }
            else
            {
                Assert.True(expected.ToString() == result.ToString(), description);
            }
        }
    }
}