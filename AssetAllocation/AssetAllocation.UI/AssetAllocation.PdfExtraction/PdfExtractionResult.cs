using AssetAllocation.Domain.Models;
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
    public partial class PdfExtractionResult : OneOfBase<ImmutableArray<Security>, ExtractionError>
    {
    }

    [GenerateOneOf]
    public partial class ExtractionError : OneOfBase<CannotFindFile, CannotFindAnySecurities, UnexpectedError>
    {
    }

    public record CannotFindFile(string FileName);

    public readonly struct CannotFindAnySecurities
    {
        public static readonly CannotFindAnySecurities Instance = new();
    }

    public readonly struct UnexpectedError 
    {
        public static readonly UnexpectedError Instance = new();
    }    
}
