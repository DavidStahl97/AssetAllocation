
namespace AssetAllocation.PdfExtraction.FTSE
{
    public interface IFtseAllWorldGdpPdfExtractor
    {
        Task<FtseAllWorldGdpExtractionResult> ReadPdfFileAsync(string filePath);
    }
}