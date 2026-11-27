using Lab8.Optimization;
namespace Lab8.Tests;
public class OptimizationTests
{
    [Theory]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(100)]
    [InlineData(500)]
    public void Algorithms_SeededData_AllOutputsMatch(int count)
    {
        var data = PurchaseData.Generate(count);
        var unique = Baseline.Unique(data);

        var newUnique = Optimized.Unique(data);
        var report = Optimized.Report(data);
        var sorted = Optimized.Sort(data);
        var totals = Optimized.Totals(data, newUnique);

        Assert.Equal(unique, newUnique);
        Assert.Equal(Baseline.Report(data), report);
        Assert.Equal(Baseline.Sort(data), sorted);
        Assert.Equal(Baseline.Totals(data, unique), totals);
    }
    [Fact]
    public void Unique_Duplicates_PreservesFirstOccurrenceOrder()
    {
        var data = new PurchaseData(["b", "a", "b", "c", "a"], [1, 2, 3, 4, 5]);

        var result = Optimized.Unique(data);

        Assert.Equal(new[] { "b", "a", "c" }, result);
    }
    [Fact]
    public void Sort_UnsortedInput_DoesNotChangeOriginal()
    {
        var data = new PurchaseData(["a", "b", "c"], [3, 1, 2]);

        var result = Optimized.Sort(data);

        Assert.Equal(new[] { 1, 2, 3 }, result);
        Assert.Equal(new[] { 3, 1, 2 }, data.Amounts);
    }
    [Fact]
    public void Report_TwoPurchases_PreservesTextAndLineEndings()
    {
        var data = new PurchaseData(["Антон", "user2"], [17, 42]);
        string expected = "Антон;17" + Environment.NewLine + "user2;42" + Environment.NewLine;

        var result = Optimized.Report(data);

        Assert.Equal(expected, result);
    }
    [Fact]
    public void Totals_EqualTotals_PreservesTieOrder()
    {
        var data = new PurchaseData(["a", "b", "a", "b"], [5, 10, 5, 0]);
        var unique = Optimized.Unique(data);

        var result = Optimized.Totals(data, unique);

        Assert.Equal(new[] { new BuyerTotal("a", 10), new BuyerTotal("b", 10) }, result);
        Assert.Equal("a", result.MaxBy(item => item.Total)!.Buyer);
    }
    [Fact]
    public void Generate_SameSeed_ReturnsIdenticalData()
    {
        var first = PurchaseData.Generate(100);

        var second = PurchaseData.Generate(100);

        Assert.Equal(first.Buyers, second.Buyers);
        Assert.Equal(first.Amounts, second.Amounts);
    }
    [Theory]
    [InlineData(0)]
    [InlineData(100001)]
    public void Generate_InvalidCount_Throws(int count)
    {
        Action action = () => PurchaseData.Generate(count);

        Assert.Throws<ArgumentOutOfRangeException>(action);
    }
    [Fact]
    public void Benchmark_SmallData_ReportsFourEqualStages()
    {
        int count = 100;

        var result = BenchmarkEngine.Run(count, 1);

        Assert.True(result.Equal);
        Assert.Equal(4, result.Stages.Length);
        Assert.All(result.Stages, stage => Assert.True(stage.BeforeMs >= 0 && stage.AfterMs >= 0));
    }
    [Theory]
    [InlineData(99, 1)]
    [InlineData(100001, 1)]
    [InlineData(100, 2)]
    public void Benchmark_InvalidParameters_Throws(int count, int repetitions)
    {
        Action action = () => BenchmarkEngine.Run(count, repetitions);

        Assert.Throws<ArgumentOutOfRangeException>(action);
    }
    [Fact]
    public void Benchmark_CancelledToken_StopsWork()
    {
        using var source = new CancellationTokenSource();
        source.Cancel();

        Action action = () => BenchmarkEngine.Run(100, 1, token: source.Token);

        Assert.Throws<OperationCanceledException>(action);
    }
    [Fact]
    public void RunService_ConcurrentRun_IsRejected()
    {
        using var service = new RunService();
        var first = service.Start(100000, 3);

        Action action = () => service.Start(100, 1);

        Assert.Throws<InvalidOperationException>(action);
        Assert.True(service.Cancel(first.Id));
    }
    [Fact]
    public async Task RunService_CancelledJob_EndsInCancelledState()
    {
        using var service = new RunService();
        var job = service.Start(100000, 3);

        service.Cancel(job.Id);
        for (int i = 0; i < 300 && service.Get(job.Id)!.State == "running"; i++) await Task.Delay(10);

        Assert.Equal("cancelled", service.Get(job.Id)!.State);
    }
}
