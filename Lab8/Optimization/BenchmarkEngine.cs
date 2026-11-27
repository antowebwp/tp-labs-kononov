using System.Diagnostics;
namespace Lab8.Optimization;
public sealed record StageMeasurement(string Key, string Name, double BeforeMs, double AfterMs, long BeforeBytes, long AfterBytes, bool Equal, string Change, string BeforeComplexity, string AfterComplexity)
{
    public double Speedup => AfterMs > 0 ? BeforeMs / AfterMs : 0;
}
public sealed record Summary(int UniqueBuyers, int ReportLength, int MedianAmount, string TopBuyer, long TopTotal);
public sealed record BenchmarkResult(int Count, int Repetitions, DateTimeOffset MeasuredAt, StageMeasurement[] Stages, Summary Summary)
{
    public bool Equal => Stages.All(stage => stage.Equal);
    public double BeforeMs => Stages.Sum(stage => stage.BeforeMs);
    public double AfterMs => Stages.Sum(stage => stage.AfterMs);
}
public static class BenchmarkEngine
{
    private sealed record Sample<T>(T Value, double Milliseconds, long Bytes);
    private static Sample<T> Measure<T>(Func<T> action)
    {
        long bytes = GC.GetAllocatedBytesForCurrentThread();
        var watch = Stopwatch.StartNew();
        T value = action();
        watch.Stop();
        return new Sample<T>(value, watch.Elapsed.TotalMilliseconds, GC.GetAllocatedBytesForCurrentThread() - bytes);
    }
    private static StageMeasurement Compare<T>(string key, string name, Func<T> before, Func<T> after,
        Func<T, T, bool> equal, string change, string oldComplexity, string newComplexity, int repetitions, CancellationToken token)
    {
        var oldSamples = new List<Sample<T>>();
        var newSamples = new List<Sample<T>>();
        bool identical = true;
        for (int i = 0; i < repetitions; i++)
        {
            token.ThrowIfCancellationRequested();
            Sample<T> oldSample, newSample;
            if (i % 2 == 0) { oldSample = Measure(before); newSample = Measure(after); }
            else { newSample = Measure(after); oldSample = Measure(before); }
            identical &= equal(oldSample.Value, newSample.Value);
            oldSamples.Add(oldSample); newSamples.Add(newSample);
        }
        double oldTime = oldSamples.OrderBy(sample => sample.Milliseconds).ElementAt(repetitions / 2).Milliseconds;
        double newTime = newSamples.OrderBy(sample => sample.Milliseconds).ElementAt(repetitions / 2).Milliseconds;
        long oldBytes = oldSamples.OrderBy(sample => sample.Bytes).ElementAt(repetitions / 2).Bytes;
        long newBytes = newSamples.OrderBy(sample => sample.Bytes).ElementAt(repetitions / 2).Bytes;
        return new StageMeasurement(key, name, oldTime, newTime, oldBytes, newBytes, identical, change, oldComplexity, newComplexity);
    }
    public static BenchmarkResult Run(int count, int repetitions, Action<int, string>? progress = null, CancellationToken token = default, string? onlyStage = null)
    {
        if (count is < 100 or > 100000) throw new ArgumentOutOfRangeException(nameof(count));
        if (repetitions is not (1 or 3)) throw new ArgumentOutOfRangeException(nameof(repetitions));
        var warm = PurchaseData.Generate(100);
        foreach (bool optimized in new[] { false, true }) RunProfile(warm, optimized, 1, token);
        var data = PurchaseData.Generate(count);
        var unique = Baseline.Unique(data, token);
        var stages = new List<StageMeasurement>();
        void Stage(int percent, string key, Func<StageMeasurement> measure)
        {
            if (onlyStage != null && onlyStage != key) return;
            progress?.Invoke(percent, key);
            stages.Add(measure());
        }
        Stage(10, "unique", () => Compare("unique", "Уникальные покупатели", () => Baseline.Unique(data, token), () => Optimized.Unique(data, token),
            (a, b) => a.SequenceEqual(b), "List.Contains → HashSet", "O(n × m)", "O(n) в среднем", repetitions, token));
        Stage(30, "report", () => Compare("report", "Текстовый отчёт", () => Baseline.Report(data, token), () => Optimized.Report(data, token),
            (a, b) => a == b, "string += → StringBuilder", "O(n²)", "O(n)", repetitions, token));
        Stage(55, "sort", () => Compare("sort", "Сортировка сумм", () => Baseline.Sort(data, token), () => Optimized.Sort(data, token),
            (a, b) => a.SequenceEqual(b), "Пузырьковая → List.Sort", "O(n²)", "O(n log n)", repetitions, token));
        Stage(80, "totals", () => Compare("totals", "Расходы покупателей", () => Baseline.Totals(data, unique, token), () => Optimized.Totals(data, unique, token),
            (a, b) => a.SequenceEqual(b), "Вложенные циклы → Dictionary", "O(n × m)", "O(n + m) в среднем", repetitions, token));
        var sorted = Optimized.Sort(data, token);
        var totals = Optimized.Totals(data, unique, token);
        var top = totals.MaxBy(item => item.Total)!;
        var summary = new Summary(unique.Count, Optimized.Report(data, token).Length, sorted[sorted.Count / 2], top.Buyer, top.Total);
        progress?.Invoke(100, "done");
        return new BenchmarkResult(count, repetitions, DateTimeOffset.Now, stages.ToArray(), summary);
    }
    public static void RunProfile(PurchaseData data, bool optimized, int iterations, CancellationToken token = default)
    {
        for (int i = 0; i < iterations; i++)
        {
            token.ThrowIfCancellationRequested();
            var unique = optimized ? Optimized.Unique(data, token) : Baseline.Unique(data, token);
            var report = optimized ? Optimized.Report(data, token) : Baseline.Report(data, token);
            var sorted = optimized ? Optimized.Sort(data, token) : Baseline.Sort(data, token);
            var totals = optimized ? Optimized.Totals(data, unique, token) : Baseline.Totals(data, unique, token);
            GC.KeepAlive((unique, report, sorted, totals));
        }
    }
}
