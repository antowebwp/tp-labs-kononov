namespace Trigonometry.Core;
static class Taylor
{
    public static (double Sin, double Cos) Evaluate(double x)
    {
        double negativeSquare = -x * x;
        double sin = x;
        double cos = 1;
        double sineTerm = x;
        double cosineTerm = 1;
        for (int n = 2; n <= 22; n += 2)
        {
            sineTerm *= negativeSquare / (n * (n + 1));
            cosineTerm *= negativeSquare / (n * (n - 1));
            double nextSine = sin + sineTerm;
            double nextCosine = cos + cosineTerm;
            if (nextSine == sin && nextCosine == cos) break;
            sin = nextSine;
            cos = nextCosine;
        }
        return (sin, cos);
    }
}
