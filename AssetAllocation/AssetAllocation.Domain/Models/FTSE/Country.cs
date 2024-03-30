namespace AssetAllocation.Domain.Models.FTSE
{
    public record Country(
        string Name,
        CountryWeighted GDPWeighted,
        CountryWeighted MarketCapWeighted);
}
