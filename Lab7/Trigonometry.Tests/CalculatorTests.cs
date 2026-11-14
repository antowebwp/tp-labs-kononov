using Trigonometry.Core;
namespace Trigonometry.Tests;
public class CalculatorTests
{
    [Theory]
    [InlineData(0, 0, 1)]
    [InlineData(90, 1, 0)]
    [InlineData(180, 0, -1)]
    [InlineData(270, -1, 0)]
    [InlineData(-90, -1, 0)]
    public void Calculate_Degrees_ReturnsExpected(double angle, double sin, double cos)
    {
        var calculator = new Calculator(new MemoryHistory());

        var result = calculator.Calculate(angle, true, 12);

        Assert.Equal(sin, result.Sin, 12);
        Assert.Equal(cos, result.Cos, 12);
    }
    [Fact]
    public void Calculate_Radians_MatchesMath()
    {
        var calculator = new Calculator(new MemoryHistory());
        double angle = 1.234;

        var result = calculator.Calculate(angle, false, 10);

        Assert.InRange(Math.Abs(result.Sin - Math.Sin(angle)), 0, 1e-10);
        Assert.InRange(Math.Abs(result.Cos - Math.Cos(angle)), 0, 1e-10);
        Assert.InRange(Math.Abs(result.Tan!.Value - Math.Tan(angle)), 0, 1e-10);
        Assert.InRange(Math.Abs(result.Cot!.Value - 1 / Math.Tan(angle)), 0, 1e-10);
    }
    [Fact]
    public void Calculate_Zero_CotangentUndefined()
    {
        var calculator = new Calculator(new MemoryHistory());

        var result = calculator.Calculate(0, false, 6);

        Assert.Null(result.Cot);
        Assert.Equal(0d, result.Tan);
    }
    [Fact]
    public void Calculate_RightAngle_TangentUndefined()
    {
        var calculator = new Calculator(new MemoryHistory());

        var result = calculator.Calculate(90, true, 6);

        Assert.Null(result.Tan);
    }
    [Fact]
    public void Calculate_WholeTurn_MatchesOriginalAngle()
    {
        var calculator = new Calculator(new MemoryHistory());

        var first = calculator.Calculate(30, true, 12);
        var second = calculator.Calculate(390, true, 12);

        Assert.Equal(first.Sin, second.Sin);
        Assert.Equal(first.Cos, second.Cos);
    }
    [Fact]
    public void Calculate_Precision_RoundsResult()
    {
        var calculator = new Calculator(new MemoryHistory());

        var result = calculator.Calculate(45, true, 3);

        Assert.Equal(0.707, result.Sin);
        Assert.Equal(1d, result.Tan);
    }
    [Theory]
    [InlineData(-1)]
    [InlineData(13)]
    public void Calculate_InvalidDigits_Throws(int digits)
    {
        var calculator = new Calculator(new MemoryHistory());

        Action action = () => calculator.Calculate(30, true, digits);

        Assert.Throws<ArgumentOutOfRangeException>(action);
    }
    [Theory]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(1000001)]
    public void Calculate_InvalidAngle_Throws(double angle)
    {
        var calculator = new Calculator(new MemoryHistory());

        Action action = () => calculator.Calculate(angle, false, 6);

        Assert.Throws<ArgumentOutOfRangeException>(action);
    }
    [Fact]
    public void Calculate_MemoryStorage_SavesResult()
    {
        var storage = new MemoryHistory();
        var calculator = new Calculator(storage);

        var result = calculator.Calculate(30, true, 6);

        Assert.Equal(result, Assert.Single(calculator.GetHistory()));
    }
    [Fact]
    public void Constructor_NullStorage_Throws()
    {
        ICalculationHistory storage = null!;

        Action action = () => new Calculator(storage);

        Assert.Throws<ArgumentNullException>(action);
    }
    [Fact]
    public void MemoryHistory_Empty_ReturnsEmptyCollection()
    {
        var storage = new MemoryHistory();

        var history = storage.ReadAll();

        Assert.Empty(history);
    }
    [Fact]
    public void FileHistory_SaveAndReload_PreservesResult()
    {
        string path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".jsonl");
        try
        {
            var calculator = new Calculator(new FileHistory(path));

            var result = calculator.Calculate(30, true, 6);
            var reloaded = new FileHistory(path).ReadAll();

            Assert.Equal(result, Assert.Single(reloaded));
        }
        finally { File.Delete(path); }
    }
}
