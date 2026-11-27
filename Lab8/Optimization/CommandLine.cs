using System.Text.Json;
namespace Lab8.Optimization;
public static class CommandLine
{
    public static bool TryRun(string[] args)
    {
        if (args.Length == 0 || args[0] is not ("--profile" or "--benchmark" or "--measure")) return false;
        try
        {
            string? Value(string key) { int index = Array.IndexOf(args, key); return index < 0 ? null : index + 1 < args.Length ? args[index + 1] : throw new ArgumentException("Нет значения для " + key); }
            int count = int.Parse(Value("--count") ?? "30000");
            if (count is < 100 or > 100000) throw new ArgumentOutOfRangeException(nameof(count));
            if (args[0] == "--profile")
            {
                string mode = Value("--profile")!;
                if (mode is not ("baseline" or "optimized")) throw new ArgumentException("Режим: baseline или optimized.");
                int iterations = int.Parse(Value("--iterations") ?? "1");
                if (iterations is < 1 or > 1000) throw new ArgumentOutOfRangeException(nameof(iterations));
                BenchmarkEngine.RunProfile(PurchaseData.Generate(count), mode == "optimized", iterations);
                Console.WriteLine($"Профиль: {mode}, N={count}, повторений={iterations}.");
            }
            else
            {
                int repetitions = int.Parse(Value("--repetitions") ?? "3");
                var result = BenchmarkEngine.Run(count, repetitions, onlyStage: Value("--measure"));
                var options = new JsonSerializerOptions(JsonSerializerDefaults.Web) { WriteIndented = true };
                string json = JsonSerializer.Serialize(result, options);
                string? output = Value("--output");
                if (output != null) File.WriteAllText(output, json);
                Console.WriteLine(json);
                if (!result.Equal) Environment.ExitCode = 1;
            }
        }
        catch (Exception e) when (e is ArgumentException or FormatException or OverflowException or IOException)
        { Console.Error.WriteLine(e.Message); Environment.ExitCode = 2; }
        return true;
    }
}
