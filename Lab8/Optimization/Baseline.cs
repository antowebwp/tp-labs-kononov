using System.Runtime.CompilerServices;
namespace Lab8.Optimization;
public static class Baseline
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static List<string> Unique(PurchaseData data, CancellationToken token = default)
    {
        var unique = new List<string>();
        for (int i = 0; i < data.Buyers.Count; i++)
        {
            if ((i & 255) == 0) token.ThrowIfCancellationRequested();
            if (!unique.Contains(data.Buyers[i])) unique.Add(data.Buyers[i]);
        }
        return unique;
    }
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static string Report(PurchaseData data, CancellationToken token = default)
    {
        string report = "";
        for (int i = 0; i < data.Buyers.Count; i++)
        {
            if ((i & 255) == 0) token.ThrowIfCancellationRequested();
            report += data.Buyers[i] + ";" + data.Amounts[i] + Environment.NewLine;
        }
        return report;
    }
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static List<int> Sort(PurchaseData data, CancellationToken token = default)
    {
        var sorted = new List<int>(data.Amounts);
        for (int i = 0; i < sorted.Count - 1; i++)
        {
            if ((i & 255) == 0) token.ThrowIfCancellationRequested();
            for (int j = 0; j < sorted.Count - i - 1; j++)
                if (sorted[j] > sorted[j + 1]) (sorted[j], sorted[j + 1]) = (sorted[j + 1], sorted[j]);
        }
        return sorted;
    }
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static List<BuyerTotal> Totals(PurchaseData data, List<string> unique, CancellationToken token = default)
    {
        var totals = new List<BuyerTotal>();
        foreach (string buyer in unique)
        {
            token.ThrowIfCancellationRequested();
            long total = 0;
            for (int i = 0; i < data.Buyers.Count; i++)
                if (data.Buyers[i] == buyer) total += data.Amounts[i];
            totals.Add(new BuyerTotal(buyer, total));
        }
        return totals;
    }
}
