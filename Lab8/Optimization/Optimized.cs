using System.Text;
using System.Runtime.CompilerServices;
namespace Lab8.Optimization;
public static class Optimized
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static List<string> Unique(PurchaseData data, CancellationToken token = default) 
    {
        var seen = new HashSet<string>(StringComparer.Ordinal);
        var unique = new List<string>();
        for (int i = 0; i < data.Buyers.Count; i++)
        {
            if ((i & 255) == 0) token.ThrowIfCancellationRequested();
            if (seen.Add(data.Buyers[i])) unique.Add(data.Buyers[i]);
        }
        return unique;
    }
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static string Report(PurchaseData data, CancellationToken token = default) 
    {
        var report = new StringBuilder();
        for (int i = 0; i < data.Buyers.Count; i++)
        {
            if ((i & 255) == 0) token.ThrowIfCancellationRequested();
            report.Append(data.Buyers[i]).Append(';').Append(data.Amounts[i]).Append(Environment.NewLine);
        }
        return report.ToString();
    }
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static List<int> Sort(PurchaseData data, CancellationToken token = default) 
    {
        token.ThrowIfCancellationRequested();
        var sorted = new List<int>(data.Amounts);
        sorted.Sort();
        token.ThrowIfCancellationRequested();
        return sorted;
    }
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static List<BuyerTotal> Totals(PurchaseData data, List<string> unique, CancellationToken token = default) 
    {
        var totals = new Dictionary<string, long>(StringComparer.Ordinal);
        for (int i = 0; i < data.Buyers.Count; i++)
        {
            if ((i & 255) == 0) token.ThrowIfCancellationRequested();
            totals.TryGetValue(data.Buyers[i], out long total);
            totals[data.Buyers[i]] = total + data.Amounts[i];
        }
        return unique.Select(buyer => new BuyerTotal(buyer, totals[buyer])).ToList();
    }
}
