using System.Linq;
using TypefaceUtil.OpenType;
using Xunit;
using FluentAssertions;

namespace TheoryDataExamples
{

    public class SampleUsage
    {
        [Fact]
        public void GeneratedUnicodeData_UsesCharName()
        {
            var charNameMap = UnicodeData.CharCodeNameMap;

            charNameMap.Should().ContainKeys(0x0041);
            charNameMap[0x0041].Should().Be("LATIN CAPITAL LETTER A");
        }

        [Fact]
        public void GeneratedUnicodeData_SupportsRanges()
        {
            var charNameMap = UnicodeData.CharCodeNameMap;

            var allPrivateUseCharKeys = charNameMap
                .Where(kvp => kvp.Value == "<Private Use>")
                .Select(kvp => kvp.Key)
                .OrderBy(i => i);
            allPrivateUseCharKeys.First().Should().Be(0xE000);
            allPrivateUseCharKeys.Last().Should().Be(0xF8FF);
            allPrivateUseCharKeys.Count().Should().Be(0xF8FF-0xE000+1);
        }
    }
}