using AssetAllocation.Domain.Models;

namespace AssetAllocation.PdfExtraction
{
    public interface IPdfExtractor
    {
        Task<PdfExtractionResult> ReadPdfFileAsync(string fileName);
    }
}
