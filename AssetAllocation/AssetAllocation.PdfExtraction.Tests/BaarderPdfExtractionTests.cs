namespace AssetAllocation.PdfExtraction.Tests
{
    public class BaarderPdfExtractionTests
    {
        [Fact]
        public async Task Test1()
        {
            var pdfExtractor = new BaaderPdfExtractor(new PdfExtractionSettings());

            await pdfExtractor.ReadPdfFileAsync("C:\\Users\\david\\Desktop\\Baader Bank AG _ Internetbanking(1).PDF");
        }
    }
}