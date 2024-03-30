using AssetAllocation.Domain.Models.Baader;
using AssetAllocation.PdfExtraction.Results;
using CommunityToolkit.Diagnostics;
using iText.Kernel.Pdf;
using OneOf;
using System.Collections.Immutable;
using System.Globalization;

namespace AssetAllocation.PdfExtraction.Baader
{
    public class BaaderPdfExtractor(BaaderPdfSettings settings) : IBaaderPdfExtractor
    {
        record AssetCell(Asset Asset, BoundingBoxInPercentage BoundingBox);

        public async Task<BaaderExtractionResult> ReadPdfFileAsync(string filePath)
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

        private BaaderExtractionResult ReadPdfFile(string filePath)
        {
            using var reader = new PdfReader(filePath);
            using var pdfDoc = new PdfDocument(reader);

            var numberOfPages = pdfDoc.GetNumberOfPages();

            if (numberOfPages - settings.NumberOfPageSkips <= 0)
            {
                return new ExtractionError(CannotFindAnySecurities.Instance);
            }

            var securities = new List<Security>();
            var failedSecurities = new List<ExtractSecurityFailed>();

            foreach (var pageIndex in Enumerable.Range(1, numberOfPages).Skip(settings.NumberOfPageSkips))
            {
                var page = pdfDoc.GetPage(pageIndex);

                var assetCells = ExtractAssets(page);

                foreach (var assetCell in assetCells)
                {
                    ExtractSecurity(page, assetCell).Switch(
                        securities.Add,
                        failedSecurities.Add);
                }
            }

            return new ExtractSecurities(securities, failedSecurities);
        }

        private List<AssetCell> ExtractAssets(PdfPage page)
        {
            var lines = page.ExtractText(new BoundingBoxInPercentage(
                X: settings.MarginLeft,
                Y: settings.MarginTop,
                Width: settings.AssetColumnLength,
                Height: 100 - settings.MarginTop));

            var assets = new List<AssetCell>();

            string assetDescription = string.Empty;
            float topY = settings.MarginTop;
            float leftX = float.MaxValue;
            float rightX = float.MinValue;

            foreach (var line in lines)
            {
                if (line.StartPoint.X < leftX)
                {
                    leftX = line.StartPoint.X;
                }

                if (line.EndPoint.X > rightX)
                {
                    rightX = line.EndPoint.X;
                }

                string? isin = line.Text.Length >= 12 ?
                    line.Text[..12] : null;

                var isISIN = isin is not null &&
                    LuhnAlgorithm.IsValidISIN(isin);

                if (!isISIN)
                {
                    assetDescription += string.IsNullOrEmpty(assetDescription) ?
                        line.Text : $" {line.Text}";

                    continue;
                }

                Guard.IsNotNull(isin);

                var asset = new Asset(
                    ISIN: isin,
                    Description: assetDescription);

                assets.Add(new(
                    Asset: asset,
                    BoundingBox: new BoundingBoxInPercentage(
                        X: leftX,
                        Y: topY,
                        Width: rightX - leftX,
                        Height: line.StartPoint.Y + 1 - topY)));

                assetDescription = string.Empty;
                topY = line.StartPoint.Y + 2;
                leftX = 0;
                rightX = 0;
            }

            return assets;
        }

        private OneOf<Security, ExtractSecurityFailed> ExtractSecurity(PdfPage page, AssetCell assetCell)
        {
            var quanitityBoundingBox = new BoundingBoxInPercentage(
                X: settings.MarginLeft + settings.AssetColumnLength,
                Y: assetCell.BoundingBox.Y,
                Width: settings.QuanityColumnLength,
                Height: assetCell.BoundingBox.Height);

            var quanitity = ExtractDecimalValue(page, quanitityBoundingBox);
            if (quanitity is null)
            {
                return new ExtractSecurityFailed(assetCell.Asset, Column.Quantity);
            }

            var purchasePriceBoundingBox = quanitityBoundingBox with
            {
                X = quanitityBoundingBox.X + settings.QuanityColumnLength,
                Width = settings.PurchasePriceColumnLength,
            };

            var purchasePrice = ExtractDecimalValue(page, purchasePriceBoundingBox);
            if (purchasePrice is null)
            {
                return new ExtractSecurityFailed(assetCell.Asset, Column.PruchasePrice);
            }

            var closingPriceBoundingBox = purchasePriceBoundingBox with
            {
                X = purchasePriceBoundingBox.X + settings.PurchasePriceColumnLength,
                Width = settings.ClosingPriceColumnLength,
            };

            var closingPrice = ExtractDecimalValue(page, closingPriceBoundingBox);
            if (closingPrice is null)
            {
                return new ExtractSecurityFailed(assetCell.Asset, Column.ClosingPrice);
            }

            var currentValueBoundingBox = closingPriceBoundingBox with
            {
                X = closingPriceBoundingBox.X + settings.ClosingPriceColumnLength,
                Width = settings.CurrentValueColumnLength,
            };

            var currentValue = ExtractDecimalValue(page, currentValueBoundingBox);
            if (currentValue is null)
            {
                return new ExtractSecurityFailed(assetCell.Asset, Column.CurrentValue);
            }

            var profitBoundingBox = currentValueBoundingBox with
            {
                X = currentValueBoundingBox.X + settings.CurrentValueColumnLength,
                Width = settings.ProfitColumnLength,
            };

            var profit = ExtractDecimalValue(page, profitBoundingBox);
            if (profit is null)
            {
                return new ExtractSecurityFailed(assetCell.Asset, Column.Profit);
            }

            return new Security(
                Asset: assetCell.Asset,
                Date: DateOnly.FromDateTime(DateTime.UtcNow),
                Quantity: quanitity.Value,
                PurchasePrice: purchasePrice.Value,
                ClosingPrice: closingPrice.Value,
                CurrentValue: currentValue.Value,
                Profit: profit.Value);
        }

        private static decimal? ExtractDecimalValue(PdfPage page, BoundingBoxInPercentage boundingBox)
        {
            var textLines = page.ExtractText(boundingBox);
            if (textLines.Count is 0)
            {
                return null;
            }

            var valueString = textLines[0].Text;
            var valueNumberString = valueString.Split(' ').FirstOrDefault();
            if (valueNumberString is null)
            {
                return null;
            }

            bool successfull = decimal.TryParse(valueNumberString, out decimal decimalValue);
            return successfull ? decimalValue : null;
        }
    }
}
