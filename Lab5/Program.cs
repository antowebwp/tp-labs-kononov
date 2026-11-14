using Lab5;
using System.Diagnostics;
using System.Globalization;
Console.OutputEncoding = System.Text.Encoding.UTF8;
using var cancellation = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) => { e.Cancel = true; cancellation.Cancel(); };
try
{
    if (args.Length > 2) throw new ArgumentException("Аргументы: число_членов число_потоков");
    Console.WriteLine("Ctrl+C — отмена вычислений.");
    string? countInput, workersInput;
    if (args.Length == 2) { countInput = args[0]; workersInput = args[1]; }
    else
    {
        Console.Write("Число членов (Enter = 100000000): "); countInput = args.Length == 1 ? args[0] : Console.ReadLine();
        Console.Write("Количество потоков (1–256): "); workersInput = Console.ReadLine();
    }
    if (string.IsNullOrWhiteSpace(countInput)) countInput = "100000000";
    if (!long.TryParse(countInput, out long terms) || !int.TryParse(workersInput, out int workers))
        throw new ArgumentException("Введите целые числа.");
    Leibniz.Calculate(1000, 1, false);
    Leibniz.Calculate(1000, workers, true);
    var timer = Stopwatch.StartNew();
    double sequential = Leibniz.Calculate(terms, workers, false, cancellation.Token);
    double sequentialMs = timer.Elapsed.TotalMilliseconds;
    timer.Restart();
    double parallel = Leibniz.Calculate(terms, workers, true, cancellation.Token);
    double parallelMs = timer.Elapsed.TotalMilliseconds;
    Console.WriteLine($"N={terms}, потоки={workers}");
    Console.WriteLine($"Последовательно: {sequential:G17}; {sequentialMs.ToString("F3", CultureInfo.InvariantCulture)} мс");
    Console.WriteLine($"Параллельно: {parallel:G17}; {parallelMs.ToString("F3", CultureInfo.InvariantCulture)} мс");
    Console.WriteLine($"Результаты совпадают: {sequential == parallel}");
    Console.WriteLine($"Ошибка относительно Math.PI: {Math.Abs(parallel - Math.PI):G17}");
    Console.WriteLine($"Ускорение: {sequentialMs / parallelMs:F3}x");
}
catch (OperationCanceledException) { Console.WriteLine("Вычисление отменено."); }
catch (ArgumentException e) { Console.WriteLine($"Ошибка: {e.Message}"); }
