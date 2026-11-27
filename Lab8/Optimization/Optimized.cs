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
    public static string Report(PurchaseData data, CancellationToken token = default) => Baseline.Report(data, token);
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static List<int> Sort(PurchaseData data, CancellationToken token = default) => Baseline.Sort(data, token);
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static List<BuyerTotal> Totals(PurchaseData data, List<string> unique, CancellationToken token = default) => Baseline.Totals(data, unique, token);
}
