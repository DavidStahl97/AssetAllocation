using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#nullable disable

namespace AssetAllocation.DB
{
    public class AssetAllocationDbSettings
    {
        [Required]
        public Uri FileName { get; set; }
    }
}
