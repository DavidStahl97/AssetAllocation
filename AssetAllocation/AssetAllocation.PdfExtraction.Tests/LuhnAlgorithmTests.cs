using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetAllocation.PdfExtraction.Tests
{
    public class LuhnAlgorithmTests
    {
        [Theory]
        [InlineData("DE000BAY0017")]
        [InlineData("AU0000XVGZA3")]
        [InlineData("DE000ETFL185")]
        [InlineData("IE000AK4O3W6")]
        [InlineData("IE00B3VTMJ91")]
        [InlineData("IE00BK5BQT80")]
        [InlineData("LU2641054551")]
        public void ShouldReturnTrue(string isin)
        {
            LuhnAlgorithm.IsValidISIN(isin).Should().BeTrue();
        }

        [Theory]
        [InlineData("DE000BAY1017")]
        [InlineData("Au0000XVGZA3")]
        [InlineData("DE0f0ETFL185")]
        [InlineData("000000000000")]
        [InlineData("IE00B3VTMJ1")]
        public void ShouldReturnFalse(string isin)
        {
            LuhnAlgorithm.IsValidISIN(isin).Should().BeFalse();
        }

        public record ConvertDigitData(
            string Text,
            int[] ExpectedDigits);

        public static TheoryData<ConvertDigitData> GetConvertDigitData()
#pragma warning disable IDE0028 // Simplify collection initialization
            => new()
            {
                new ConvertDigitData(
                    Text: "DE000BAY0017",
                    ExpectedDigits: [1, 3, 1, 4, 0, 0, 0, 1, 1, 1, 0, 3, 4, 0, 0, 1, 7])
            };
#pragma warning restore IDE0028 // Simplify collection initialization

        [Theory]
        [MemberData(nameof(GetConvertDigitData))]
        public void ShouldConvertToDigitsCorrectly(ConvertDigitData digitData)
        {
            LuhnAlgorithm.ConvertToDigits(digitData.Text).Should()
                .BeEquivalentTo(digitData.ExpectedDigits);
        }
    }
}
