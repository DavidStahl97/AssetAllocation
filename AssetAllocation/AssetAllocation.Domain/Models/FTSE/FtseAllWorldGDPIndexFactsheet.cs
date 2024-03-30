using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetAllocation.Domain.Models.FTSE
{
    public record FtseAllWorldGDPIndexFactsheet(
        DateOnly Date,
        IReadOnlyList<Country> Countries);
}
