using AssetAllocation.Domain.Models;

namespace AssetAllocation.PdfExtraction.Baader
{
    public interface IBaaderPdfExtractor
    {
        Task<BaaderExtractionResult> ReadPdfFileAsync(string fileName);
    }
}
