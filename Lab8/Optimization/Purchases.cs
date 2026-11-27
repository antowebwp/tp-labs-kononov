namespace Lab8.Optimization;
public sealed record PurchaseData(List<string> Buyers, List<int> Amounts)
{
    public static PurchaseData Generate(int count)
    {
        if (count is < 1 or > 100000) throw new ArgumentOutOfRangeException(nameof(count));
        var random = new Random(42);
        var buyers = new List<string>();
        var amounts = new List<int>();
        for (int i = 0; i < count; i++)
        {
            buyers.Add("user" + random.Next(0, 5000));
            amounts.Add(random.Next(1, 10000));
        }
        return new PurchaseData(buyers, amounts);
    }
}
public sealed record BuyerTotal(string Buyer, long Total);
