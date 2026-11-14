using Lab1;
using System.Numerics;
namespace Lab4.Tests;
public class AlgorithmsTests
{
    [Theory]
    [InlineData(0, 1)]
    [InlineData(1, 1)]
    [InlineData(5, 120)]
    [InlineData(10, 3628800)]
    public void Factorial_ValidNumber_ReturnsExpected(int n, long expected)
    {
        BigInteger expectedValue = expected;

        BigInteger actual = Algorithms.Factorial(n);

        Assert.Equal(expectedValue, actual);
    }
    [Fact]
    public void Factorial_AboveLongRange_ReturnsBigInteger()
    {
        var expected = BigInteger.Parse("51090942171709440000");

        var result = Algorithms.Factorial(21);

        Assert.Equal(expected, result);
    }
    [Fact]
    public void Factorial_NegativeNumber_Throws()
    {
        int n = -1;

        Action action = () => Algorithms.Factorial(n);

        Assert.Throws<ArgumentOutOfRangeException>(action);
    }
    [Theory]
    [InlineData(0, "0")]
    [InlineData(1, "0,1,1")]
    [InlineData(10, "0,1,1,2,3,5,8")]
    public void Fibonacci_UpperBound_ReturnsExpected(int limit, string expected)
    {
        BigInteger bound = limit;

        string result = string.Join(",", Algorithms.Fibonacci(bound));

        Assert.Equal(expected, result);
    }
    [Fact]
    public void Fibonacci_NegativeLimit_Throws()
    {
        BigInteger limit = -1;

        Action action = () => Algorithms.Fibonacci(limit).ToArray();

        Assert.Throws<ArgumentOutOfRangeException>(action);
    }
    [Theory]
    [InlineData(0)]
    [InlineData(0.5)]
    [InlineData(-1.2)]
    [InlineData(Math.PI / 2)]
    [InlineData(1000000)]
    public void SineTaylor_ValidAngle_MatchesMath(double angle)
    {
        double expected = Math.Sin(angle);

        var actual = Algorithms.SineTaylor(angle);

        Assert.InRange(Math.Abs(actual.Sum - expected), 0, 1e-6);
    }
    [Fact]
    public void SineTaylor_Zero_ReturnsZeroTerms()
    {
        double angle = 0;

        var result = Algorithms.SineTaylor(angle);

        Assert.Equal((0.0, 0), result);
    }
    [Theory]
    [InlineData(0)]
    [InlineData(-0.001)]
    public void SineTaylor_InvalidPrecision_Throws(double precision)
    {
        double angle = 1;

        Action action = () => Algorithms.SineTaylor(angle, precision);

        Assert.Throws<ArgumentOutOfRangeException>(action);
    }
    [Fact]
    public void SineTaylor_NaN_Throws()
    {
        double angle = double.NaN;

        Action action = () => Algorithms.SineTaylor(angle);

        Assert.Throws<ArgumentOutOfRangeException>(action);
    }
    [Fact]
    public void EvaluateFunction_Zero_ReturnsExpected()
    {
        double expected = Math.Sqrt(Math.Log(4.0 / 3)) + 9.0 / 7 - Math.Exp(Math.Sin(-0.7));

        double actual = Algorithms.EvaluateFunction(0);

        Assert.Equal(expected, actual, 12);
    }
    [Fact]
    public void EvaluateFunction_Infinity_Throws()
    {
        double angle = double.PositiveInfinity;

        Action action = () => Algorithms.EvaluateFunction(angle);

        Assert.Throws<ArgumentOutOfRangeException>(action);
    }
    [Fact]
    public void EvaluateFunction_OverflowingAngle_Throws()
    {
        double angle = double.MaxValue;

        Action action = () => Algorithms.EvaluateFunction(angle);

        Assert.Throws<ArgumentOutOfRangeException>(action);
    }
}
