using System.Numerics;

namespace Lab1;

public static class Algorithms
{
    public static BigInteger Factorial(int n)
    {
        if (n < 0)
            throw new ArgumentOutOfRangeException(nameof(n), "Число должно быть неотрицательным.");

        BigInteger result = 1;
        for (int i = 2; i <= n; i++)
            result *= i;
        return result;
    }

    public static IEnumerable<BigInteger> Fibonacci(BigInteger limit)
    {
        if (limit < 0)
            throw new ArgumentOutOfRangeException(nameof(limit), "Граница должна быть неотрицательной.");

        for (BigInteger a = 0, b = 1; a <= limit; (a, b) = (b, a + b))
            yield return a;
    }

    public static double EvaluateFunction(double x)
    {
        if (!double.IsFinite(x))
            throw new ArgumentOutOfRangeException(nameof(x), "Нужно конечное число.");

        double angle = 1.3 * x - 0.7;
        if (!double.IsFinite(angle))
            throw new ArgumentOutOfRangeException(nameof(x), "Слишком большой аргумент.");

        return Math.Sqrt(Math.Log(4.0 / 3)) + (x + 9.0 / 7) - Math.Exp(Math.Sin(angle));
    }

    public static (double Sum, int Terms) SineTaylor(double x, double epsilon = 1e-6)
    {
        if (!double.IsFinite(x))
            throw new ArgumentOutOfRangeException(nameof(x), "Нужно конечное число.");
        if (!double.IsFinite(epsilon) || epsilon <= 0)
            throw new ArgumentOutOfRangeException(nameof(epsilon), "Точность должна быть положительной.");

        double reduced = Math.IEEERemainder(x, 2 * Math.PI);
        if (reduced > Math.PI / 2)
            reduced = Math.PI - reduced;
        else if (reduced < -Math.PI / 2)
            reduced = -Math.PI - reduced;

        double term = reduced;
        double sum = 0;
        int terms = 0;
        while (Math.Abs(term) > epsilon)
        {
            sum += term;
            terms++;
            term *= -reduced * reduced / ((2.0 * terms) * (2.0 * terms + 1));
        }
        return (sum, terms);
    }
}
