namespace AssetAllocation.Domain.Models.FTSE
{
    public record CountryWeighted(
        int NumberOfCompanies,
        decimal MarketCap,
        decimal Weighted);
}
