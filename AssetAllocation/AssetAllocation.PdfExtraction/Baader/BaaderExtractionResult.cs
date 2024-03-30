using AssetAllocation.Domain.Models.Baader;
using AssetAllocation.PdfExtraction.Results;
using OneOf;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetAllocation.PdfExtraction
{
    [GenerateOneOf]
    public partial class BaaderExtractionResult : OneOfBase<
        ExtractSecurities, 
        ExtractionError>
    {
    }

    public record ExtractSecurities(
        IReadOnlyList<Security> Securities,
        IReadOnlyList<ExtractSecurityFailed> FailedSecurities);
    
    public record ExtractSecurityFailed(
        Asset Asset,
        Column FailedColumn);

    public enum Column
    {
        Quantity = 0,
        PruchasePrice = 1,
        ClosingPrice = 2,
        CurrentValue = 3,
        Profit = 4,
    }

    [GenerateOneOf]
    public partial class ExtractionError : OneOfBase<CannotFindFile, CannotFindAnySecurities, UnexpectedError>
    {
    }

    public readonly struct CannotFindAnySecurities
    {
        public static readonly CannotFindAnySecurities Instance = new();
    }
}
