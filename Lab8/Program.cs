using Lab8.Optimization;
using System.Text.Json;
if (args.Length == 2 && args[0] == "--measure")
{
    Console.WriteLine(JsonSerializer.Serialize(BenchmarkEngine.Run(30000, 3, onlyStage: args[1])));
    return;
}
BenchmarkEngine.RunProfile(PurchaseData.Generate(30000), false, 1);
Console.WriteLine("Исходный расчёт завершён.");
