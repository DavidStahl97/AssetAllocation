using AssetAllocation.Domain.Models.FTSE;
using AssetAllocation.PdfExtraction.Results;
using iText.Kernel.Pdf;
using iText.Signatures;
using OneOf;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetAllocation.PdfExtraction.FTSE
{
    public class FtseAllWorldGdpPdfExtractor(FtseAllWorldGdpSettings settings) : IFtseAllWorldGdpPdfExtractor
    {
        public async Task<FtseAllWorldGdpExtractionResult> ReadPdfFileAsync(string filePath)
            => await Task.Run(() =>
            {
                try
                {
                    return ReadPdfFile(filePath);
                }
                catch
                {
                    return new ExtractionError(UnexpectedError.Instance);
                }
            });

        private FtseAllWorldGdpExtractionResult ReadPdfFile(string filePath)
        {
            using var reader = new PdfReader(filePath);
            using var pdfDoc = new PdfDocument(reader);

            var numberOfPages = pdfDoc.GetNumberOfPages();

            if (numberOfPages < settings.CountryPageIndex)
            {
                return new ExtractionError(CannotFindAnyCountries.Instance);
            }

            var page = pdfDoc.GetPage(settings.CountryPageIndex);

            var countriesBoundingBox = new BoundingBoxInPercentage(
                X: 0,
                Y: settings.MarginTop,
                Width: 100,
                Height: 100 - settings.MarginTop - settings.MarginBottom);

            var textLines = page.ExtractText(countriesBoundingBox);
            if (textLines.Count < 1)
            {
                return new ExtractionError(CannotFindAnyCountries.Instance);
            }
            
            return ExtractCountires(textLines);
        }

        private static FtseAllWorldGdpExtractionResult ExtractCountires(List<TextLine> textLines)
        {
            var countries = new List<Country>();

            var countryLines = textLines
                .Where(x => !string.IsNullOrWhiteSpace(x.Text))
                .Select((x, i) => (x, i));

            foreach (var (line, i) in countryLines.Take(countryLines.Count() - 1))
            {
                var country = ExtractCountryData(line.Text, i);
                if (country is null)
                {
                    return new ExtractionError(new CannotExtractCountryInLine(i));
                }
                countries.Add(country);
            }

            var marketCapWeightingSum = countries.Sum(x => x.MarketCapWeighted.Weighted);
            if (marketCapWeightingSum < 99)
            {
                return new ExtractionError(new MissingWeighting(Index.FtseAllWorldMarketCap, marketCapWeightingSum));
            }

            var gdpWeightedSum = countries.Sum(x => x.GDPWeighted.Weighted);
            if (gdpWeightedSum < 99)
            {
                return new ExtractionError(new MissingWeighting(Index.FtseAllWorldGdp, gdpWeightedSum));
            }

            return new FtseAllWorldGDPIndexFactsheet(
                Date: DateOnly.FromDateTime(DateTime.UtcNow),
                Countries: countries);
        }

        private static Country? ExtractCountryData(string line, int lineIndex)
        {
            var texts = SplitLineToColumn(line);

            var firstDigitText = texts.FirstOrDefault(text => ConvertTextToDecimal(text) is not null);
            if (firstDigitText is null)
            {
                return null;
            }

            var firstDigitIndex = texts.IndexOf(firstDigitText);
            var countryTexts = texts[..firstDigitIndex];
            if (countryTexts.Count is 0)
            {
                return null;
            }
            
            var countryName = string.Join(' ', countryTexts);

            var digitTexts = texts[firstDigitIndex..];

            if (digitTexts.Count is not 6)
            {
                return null;
            }
            
            var gdpWeightedNumberOfCountries = ConvertTextToInt(digitTexts[0]);
            if (gdpWeightedNumberOfCountries is null)
            {
                return null;
            }

            var gdpWeightedMarketCap = ConvertTextToDecimal(digitTexts[1]);
            if (gdpWeightedMarketCap is null)
            {
                return null;
            }

            var gdpWeightedWeighted = ConvertTextToDecimal(digitTexts[2]);
            if (gdpWeightedWeighted is null)
            {
                return null;
            }

            var marketCapNumberOfCountires = ConvertTextToInt(digitTexts[3]);
            if (marketCapNumberOfCountires is null)
            {
                return null;
            }

            var marketCapMarketCap = ConvertTextToDecimal(digitTexts[4]);
            if (marketCapMarketCap is null)
            {
                return null;
            }

            var marketCapWeighted = ConvertTextToDecimal(digitTexts[5]);
            if (marketCapWeighted is null)
            {
                return null;
            }

            return new Country(
                Name: countryName,
                GDPWeighted: new CountryWeighted(
                    NumberOfCompanies: gdpWeightedNumberOfCountries.Value,
                    MarketCap: gdpWeightedMarketCap.Value,
                    Weighted: gdpWeightedWeighted.Value),
                MarketCapWeighted: new CountryWeighted(
                    NumberOfCompanies: gdpWeightedNumberOfCountries.Value,
                    MarketCap: marketCapMarketCap.Value,
                    Weighted: marketCapWeighted.Value));
        }

        private static decimal? ConvertTextToDecimal(string text)
        {
            var successfull = decimal.TryParse(text.Replace(",", string.Empty),
                CultureInfo.InvariantCulture,
                out decimal value);
            return successfull ? value : null;
        }

        private static int? ConvertTextToInt(string text)
        {
            var successfull = int.TryParse(text, out int value);
            return successfull ? value : null;
        }

        private static List<string> SplitLineToColumn(string line)
        {
            int numberOfSpaceChar = 0;
            var texts = new List<string>();

            var currentText = string.Empty;

            foreach (var character in line)
            {
                if (character is ' ')
                {
                    numberOfSpaceChar++;
                    continue;
                }

                if (numberOfSpaceChar > 1)
                {
                    if (currentText != string.Empty)
                    {
                        texts.Add(currentText);
                    }

                    currentText = character.ToString();
                    numberOfSpaceChar = 0;
                }
                else
                {
                    currentText += character.ToString();
                    numberOfSpaceChar = 0;
                }
            }

            if (currentText != string.Empty)
            {
                texts.Add(currentText);
            }

            return texts;
        }
    }
}
