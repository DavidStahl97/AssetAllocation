using AssetAllocation.PdfExtraction.FTSE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetAllocation.PdfExtraction.Tests
{
    public class FtseAllWorldGdpPdfExtractorTests
    {
        [Fact]
        public async Task Test()
        {
            var filePath = "C:\\Users\\david\\Desktop\\GDPWLDS_20240229.pdf";

            var result = await new FtseAllWorldGdpPdfExtractor(new()).ReadPdfFileAsync(filePath);
        }
    }
}
