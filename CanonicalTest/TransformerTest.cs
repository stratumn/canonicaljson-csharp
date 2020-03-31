using System;
using System.Collections.Generic;
using System.Linq;
using Stratumn.CanonicalJson;
using Xunit;
using Deveel.Math;

namespace Stratumn.CanonicalJsonTest
{
    /// <summary>
    /// @copyright Stratumn
    /// </summary>
    public class TransformerTest
    {
        public static IEnumerable<object[]> GetData()
        {
            yield return new object[] {
                new List<string> {"1337.0", "42e1", ".69e2", "16573000e-3"},
                "[" + String.Join(",", new object[] {"1337", "420", "69", "16573"}) + "]"
            };
            yield return new object[] {
                new List<string> {
                    "3.14",
                    "3.14E0",
                    "0.314E1",
                    "31.4E-1",
                    "11E-3",
                    "179.769313486231590772930519078902473361797697894230657273430081157732675805500963132708477322407536021120113879871393357658789768814416622492847430639474124377767893424865485276302219601246094119453082952085005768838150682342462881473913110540827237163350510684586298239947245938479716304835356329624224137217E305",
                    "17976931348623159077293051907890247336179769789423065727343008115773267580550096313270847732240753602112011387987139335765878976881441662249284743063947412437776789342486548527630221960124609411945308295208500576883815068234246288147391311054082723716335051068458629823994724593847971630483535632962422413721.7",
                    "-3.14",
                    "-3.14E0",
                    "-0.314E1",
                    "-31.4E-1",
                    "-11E-3",
                    "-179.769313486231590772930519078902473361797697894230657273430081157732675805500963132708477322407536021120113879871393357658789768814416622492847430639474124377767893424865485276302219601246094119453082952085005768838150682342462881473913110540827237163350510684586298239947245938479716304835356329624224137217E305",
                    "-17976931348623159077293051907890247336179769789423065727343008115773267580550096313270847732240753602112011387987139335765878976881441662249284743063947412437776789342486548527630221960124609411945308295208500576883815068234246288147391311054082723716335051068458629823994724593847971630483535632962422413721.7"
                },
                "[" + String.Join(",", new object[] {
                    "3.14E0",
                    "3.14E0",
                    "3.14E0",
                    "3.14E0",
                    "1.1E-2",
                    "1.79769313486231590772930519078902473361797697894230657273430081157732675805500963132708477322407536021120113879871393357658789768814416622492847430639474124377767893424865485276302219601246094119453082952085005768838150682342462881473913110540827237163350510684586298239947245938479716304835356329624224137217E307",
                    "1.79769313486231590772930519078902473361797697894230657273430081157732675805500963132708477322407536021120113879871393357658789768814416622492847430639474124377767893424865485276302219601246094119453082952085005768838150682342462881473913110540827237163350510684586298239947245938479716304835356329624224137217E307",
                    "-3.14E0",
                    "-3.14E0",
                    "-3.14E0",
                    "-3.14E0",
                    "-1.1E-2",
                    "-1.79769313486231590772930519078902473361797697894230657273430081157732675805500963132708477322407536021120113879871393357658789768814416622492847430639474124377767893424865485276302219601246094119453082952085005768838150682342462881473913110540827237163350510684586298239947245938479716304835356329624224137217E307",
                    "-1.79769313486231590772930519078902473361797697894230657273430081157732675805500963132708477322407536021120113879871393357658789768814416622492847430639474124377767893424865485276302219601246094119453082952085005768838150682342462881473913110540827237163350510684586298239947245938479716304835356329624224137217E307"
                }) + "]"
            };

        }

        [Theory]
        [MemberData(nameof(GetData))]
        public void TestTransformerSanity(List<string> input, string expected)
        {
            List<BigDecimal> bd = input.Select(st => BigDecimal.Parse(st)).ToList();
            string actual = new Transformer().Transform(new List<Object>(bd));
            // The return value is in the form "[val1, val2, ..., valN]"
            Assert.Equal(expected, actual);
        }
    }
}
