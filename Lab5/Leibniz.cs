namespace Lab5;
public static class Leibniz
{
    private const int BlockSize = 100_000;
    private static double SumBlock(long start, long end, CancellationToken token)
    {
        double sum = 0;
        for (long i = start; i < end; i++)
        {
            if ((i & 4095) == 0) token.ThrowIfCancellationRequested();
            sum += (i % 2 == 0 ? 1.0 : -1.0) / (2.0 * i + 1);
        }
        return sum;
    }
    public static double Calculate(long terms, int workers, bool parallel, CancellationToken token = default)
    {
        if (terms <= 0 || terms > 1_000_000_000) throw new ArgumentOutOfRangeException(nameof(terms));
        if (workers <= 0 || workers > 256) throw new ArgumentOutOfRangeException(nameof(workers));
        token.ThrowIfCancellationRequested();
        int blocks = (int)((terms + BlockSize - 1) / BlockSize);
        var sums = new double[blocks];
        void CalculateBlock(int block) => sums[block] = SumBlock((long)block * BlockSize, Math.Min((long)(block + 1) * BlockSize, terms), token);
        if (parallel) Parallel.For(0, blocks, new ParallelOptions { MaxDegreeOfParallelism = workers, CancellationToken = token }, CalculateBlock);
        else for (int block = 0; block < blocks; block++) CalculateBlock(block);
        return 4 * sums.Sum();
    }
}
