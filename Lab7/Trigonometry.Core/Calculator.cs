namespace Trigonometry.Core;
public sealed record Calculation(double Angle, bool Degrees, int Digits, double Sin, double Cos, double? Tan, double? Cot);
public sealed class Calculator
{
    private readonly ICalculationHistory _history;
    public Calculator(ICalculationHistory history)
    {
        ArgumentNullException.ThrowIfNull(history);
        _history = history;
    }
    public Calculation Calculate(double angle, bool degrees, int digits)
    {
        if (!double.IsFinite(angle) || Math.Abs(angle) > 1_000_000) throw new ArgumentOutOfRangeException(nameof(angle));
        if (digits is < 0 or > 12) throw new ArgumentOutOfRangeException(nameof(digits));
        double period = degrees ? 360 : Math.Tau;
        double quarter = period / 4;
        double normalized = Math.IEEERemainder(angle, period);
        int quadrant = (int)Math.Round(normalized / quarter);
        double offset = normalized - quadrant * quarter;
        double radians = degrees ? offset * (Math.PI / 180) : offset;
        var (sin, cos) = Taylor.Evaluate(radians);
        (sin, cos) = ((quadrant % 4 + 4) % 4) switch
        {
            1 => (cos, -sin),
            2 => (-sin, -cos),
            3 => (-cos, sin),
            _ => (sin, cos)
        };
        double Round(double value) => Math.Round(value, digits, MidpointRounding.AwayFromZero);
        double? tan = Math.Abs(cos) <= 1e-14 ? null : Round(sin / cos);
        double? cot = Math.Abs(sin) <= 1e-14 ? null : Round(cos / sin);
        var result = new Calculation(angle, degrees, digits, Round(sin), Round(cos), tan, cot);
        _history.Save(result);
        return result;
    }
    public IReadOnlyList<Calculation> GetHistory() => _history.ReadAll();
}
