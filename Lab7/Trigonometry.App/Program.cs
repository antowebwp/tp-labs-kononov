using System.Globalization;
using Newtonsoft.Json;
using Trigonometry.Core;
Console.InputEncoding = System.Text.Encoding.UTF8;
Console.OutputEncoding = System.Text.Encoding.UTF8;
try
{
    if (args.Length > 1) throw new ArgumentException("Укажите файл истории или --memory.");
    ICalculationHistory history = args.Length == 1 && args[0] == "--memory"
        ? new MemoryHistory() : new FileHistory(args.Length == 1 ? args[0] : "history.jsonl");
    var calculator = new Calculator(history);
    string Format(double? value, int digits) => value?.ToString($"F{digits}", CultureInfo.CurrentCulture) ?? "не определён";
    void Print(Calculation value) => Console.WriteLine($"x={value.Angle}, {(value.Degrees ? "градусы" : "радианы")}: sin={Format(value.Sin, value.Digits)}, cos={Format(value.Cos, value.Digits)}, tg={Format(value.Tan, value.Digits)}, ctg={Format(value.Cot, value.Digits)}");
    while (true)
    {
        Console.WriteLine("1 — вычислить, 2 — история, 0 — выход");
        string? command = Console.ReadLine();
        if (command == null || command == "0") break;
        try
        {
            if (command == "2") { foreach (var item in calculator.GetHistory()) Print(item); continue; }
            if (command != "1") throw new ArgumentException("Выберите 0, 1 или 2.");
            Console.Write("Угол (|x| ≤ 1000000): ");
            if (!double.TryParse(Console.ReadLine()?.Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out double angle))
                throw new ArgumentException("Нужно вещественное число.");
            Console.Write("Единицы (1 — радианы, 2 — градусы): ");
            string? unit = Console.ReadLine();
            if (unit is not ("1" or "2")) throw new ArgumentException("Выберите 1 или 2.");
            Console.Write("Знаков после запятой (0–12): ");
            if (!int.TryParse(Console.ReadLine(), out int digits)) throw new ArgumentException("Нужно целое число.");
            Print(calculator.Calculate(angle, unit == "2", digits));
        }
        catch (Exception e) when (e is ArgumentException or IOException or UnauthorizedAccessException or JsonException)
        { Console.WriteLine($"Ошибка: {e.Message}"); }
    }
}
catch (Exception e) when (e is ArgumentException or IOException or UnauthorizedAccessException)
{ Console.WriteLine($"Ошибка: {e.Message}"); }
