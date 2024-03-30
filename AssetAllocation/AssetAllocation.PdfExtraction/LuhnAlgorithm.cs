using CommunityToolkit.Diagnostics;
using iText.Forms.Xfdf;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetAllocation.PdfExtraction
{
    public static class LuhnAlgorithm
    {
        public static bool IsValidISIN([DisallowNull] string isin)
        {
            if (isin.Length is not 12)
            {
                return false;
            }

            if (isin.Any(c => !char.IsAscii(c) || !(char.IsDigit(c) || char.IsUpper(c))))
            {
                return false;
            }

            if (!char.IsUpper(isin[0]) || !char.IsUpper(isin[1]) ||
                !char.IsDigit(isin[^1]))
            {
                return false;
            }

            var digits = ConvertToDigits(isin);
            var isValid = IsValid(digits);
            return isValid;
        }

        private static bool IsValid(in int[] digits)
        {
            int sum = 0;

            foreach (var (digit, i) in digits.Reverse().Skip(1).Select((x, i) => (x, i)))
            {
                int toBeAdded = digit;

                if (i % 2 == 0)
                {
                    toBeAdded *= 2;
                    if (toBeAdded > 9)
                    {
                        toBeAdded -= 9;
                    }
                }

                sum += toBeAdded;
            }

            var expectedCheckDigit = (10 - (sum % 10)) % 10;
            var actualCheckDigit = digits[^1];

            return expectedCheckDigit == actualCheckDigit;
        }

        public static int[] ConvertToDigits(string isin)
        {
            var numberOfDigits = isin.Sum(c => char.IsDigit(c) ? 1 : 2);
            var digits = new int[numberOfDigits];

            var digitIndex = 0;
            for (int i = 0; i < isin.Length; i++)
            {
                var asciiValue = isin[i];
                if (char.IsDigit(isin[i]))
                {
                    var integer = asciiValue - 48;
                    Guard.IsGreaterThanOrEqualTo(integer, 0);
                    Guard.IsLessThanOrEqualTo(integer, 9);

                    digits[digitIndex] = integer;
                    digitIndex++;
                }
                else
                {
                    var letterInteger = asciiValue - 65 + 10;
                    Guard.IsGreaterThanOrEqualTo(letterInteger, 10);
                    Guard.IsLessThanOrEqualTo(letterInteger, 35);

                    digits[digitIndex] = letterInteger / 10;
                    digits[digitIndex + 1] = letterInteger % 10;
                    digitIndex += 2;
                }
            }

            return digits;
        }
    }
}
