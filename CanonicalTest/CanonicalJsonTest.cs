using System.Text;
using System.IO;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Stratumn.CanonicalJson;
using Xunit;

namespace Stratumn.CanonicalJsonTest
{
    /// <summary>
    /// @copyright Stratumn
    /// </summary>
    public class CanonicalJsonTest
    {
        // Look for all input files from canonicaljson-spec
        public static List<object[]> InitData(
            string parentFolder = "",
            List<object[]> dataList = null,
            // This is a weird hack to get the location of this source file
            // https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.callerfilepathattribute?view=netstandard-2.0
            [CallerFilePath] string callerFilePath = ""
        ) {
            string baseFolder = Path.Combine(
                Directory.GetParent(callerFilePath).FullName,
                "canonicaljson-spec",
                "test"
            );
            string folderPath = parentFolder == "" ? baseFolder : parentFolder;
            string[] subfolders = Directory.GetDirectories(folderPath);

            string inputPath = Path.Combine(folderPath, "input.json");
            if (File.Exists(inputPath))
            {
                string expectedPath = Path.Combine(folderPath, "expected.json");
                var res = new object[] {
                    // Description (path with base prefix trimmed)
                    folderPath.Substring(baseFolder.Length + 1),
                    // input.json
                    File.ReadAllText(inputPath, Encoding.UTF8).Trim(),
                    // expected.json
                    File.Exists(expectedPath) ? File.ReadAllText(expectedPath, Encoding.UTF8).Trim() : null
                };
                if (dataList == null)
                {
                    return new List<object[]>{ res };
                }
                else
                {
                    dataList.Add(res);
                    return dataList;
                }
            }
            foreach (string folder in subfolders)
            {
                dataList = InitData(folder, dataList);
            }
            return dataList;
        }
        public static IEnumerable<object[]> GetData()
        {
            var dataList = InitData();
            foreach (object[] data in dataList)
            {
                yield return data;
            }
        }

        [Theory]
        [MemberData(nameof(GetData))]
        public void TestSpecs(string description, string input, string expected)
        {
            if (expected != null)
            {
                string actual = Canonicalizer.Canonicalize(input);
                Assert.Equal(expected, actual);
            }
            else
            {
                var ex = Assert.Throws<IOException>(() => Canonicalizer.Canonicalize(input));
                Assert.True(ex != null, description + " should throw");
            }
        }
    }
}
