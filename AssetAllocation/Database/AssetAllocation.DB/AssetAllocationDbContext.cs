using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetAllocation.DB
{
    public class AssetAllocationDbContext(AssetAllocationDbSettings settings) : DbContext, IAssetAllocationDbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite($"Filename={settings.FileName}");
        }
    }
}
