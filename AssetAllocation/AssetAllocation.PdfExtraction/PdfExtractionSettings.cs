using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#nullable disable

namespace AssetAllocation.PdfExtraction
{
    public class PdfExtractionSettings
    {
        [Required]
        public int NumberOfPageSkips { get; set; } = 1;

        [Required]
        public float MarginTop { get; set; } = 19.3f;

        [Required]
        public float MarginLeft { get; set; } = 7.2f;

        [Required]
        public float AssetColumnLength { get; set; } = 18.9f;

        [Required]
        public float QuanityColumnLength { get; set; } = 10.4f;

        [Required]
        public float PurchasePriceColumnLength { get; set; } = 15.2f;

        [Required]
        public float ClosingPriceColumnLength { get; set; } = 20.0f;

        [Required]
        public float CurrentValueColumnLength { get; set; } = 11.4f;

        [Required]
        public float ProfitColumnLength { get; set; } = 10.1f;
    }
}
