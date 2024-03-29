﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetAllocation.Domain.Models
{
    public record Asset(
        string ISIN,
        string Description);
}
