namespace AssetAllocation.Domain.Models.Baader
{
    public record Security(
        Asset Asset,
        DateOnly Date,
        decimal Quantity,
        decimal PurchasePrice,
        decimal ClosingPrice,
        decimal CurrentValue,
        decimal Profit);
}
