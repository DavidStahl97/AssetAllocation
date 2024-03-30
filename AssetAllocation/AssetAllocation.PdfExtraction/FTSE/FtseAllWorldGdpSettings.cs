using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#nullable disable

namespace AssetAllocation.PdfExtraction.FTSE
{
    public class FtseAllWorldGdpSettings
    {
        [Required]
        public int CountryPageIndex { get; set; } = 3;

        [Required]
        public float MarginTop { get; set; } = 12.23f;

        [Required]
        public float MarginBottom { get; set; } = 4.3f;
    }
}
