using AssetAllocation.Domain.Models.FTSE;
using AssetAllocation.PdfExtraction.Results;
using OneOf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetAllocation.PdfExtraction.FTSE
{
    [GenerateOneOf]
    public partial class FtseAllWorldGdpExtractionResult : OneOfBase<
        FtseAllWorldGDPIndexFactsheet,
        ExtractionError>
    {
    }

    [GenerateOneOf]
    public partial class ExtractionError : OneOfBase<
        CannotFindFile,
        CannotFindAnyCountries,
        CannotExtractCountryInLine,
        MissingWeighting,
        UnexpectedError>
    {
    }

    public readonly struct CannotFindAnyCountries
    {
        public static readonly CannotFindAnyCountries Instance = new();
    }

    public record CannotExtractCountryInLine(int Line);

    public record MissingWeighting(
        Index Index,
        decimal MissingWeighted);

    public enum Index
    {
        FtseAllWorldGdp,
        FtseAllWorldMarketCap
    }
}
