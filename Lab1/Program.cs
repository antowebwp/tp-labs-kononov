using System.Globalization;
using System.Numerics;
using Lab1;

Console.InputEncoding = System.Text.Encoding.UTF8;
Console.OutputEncoding = System.Text.Encoding.UTF8;

while (true)
{
    Console.WriteLine("1 — Факториал\n2 — Числа Фибоначчи\n3 — Функция варианта 1\n4 — Ряд Тейлора для sin(x)\n0 — Выход");
    string? choice = Console.ReadLine();
    if (choice is null || choice == "0")
        break;
    if (!int.TryParse(choice, out int task) || task is < 1 or > 4)
    {
        Console.WriteLine("Ошибка: выберите задание от 1 до 4 или 0 для выхода.");
        continue;
    }

    if (task == 1)
    {
        Console.Write("Введите неотрицательное целое n: ");
        if (!int.TryParse(Console.ReadLine(), out int n) || n < 0)
            Console.WriteLine("Ошибка: нужно неотрицательное целое число в диапазоне int.");
        else
            Console.WriteLine($"{n}! = {Algorithms.Factorial(n)}");
    }
    else if (task == 2)
    {
        Console.Write("Верхняя граница чисел Фибоначчи n: ");
        if (!BigInteger.TryParse(Console.ReadLine(), out BigInteger limit) || limit < 0)
            Console.WriteLine("Ошибка: граница должна быть неотрицательным целым числом.");
        else
            Console.WriteLine(string.Join(", ", Algorithms.Fibonacci(limit)));
    }
    else
    {
        Console.Write("Введите x в радианах (дробная часть через точку или запятую): ");
        string? input = Console.ReadLine();
        if (!double.TryParse(input?.Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out double x)
            || !double.IsFinite(x))
        {
            Console.WriteLine("Ошибка: нужно конечное вещественное число.");
            continue;
        }

        if (task == 3)
        {
            try
            {
                Console.WriteLine($"A(x) = {Algorithms.EvaluateFunction(x):G17}");
            }
            catch (ArgumentOutOfRangeException)
            {
                Console.WriteLine("Ошибка: аргумент слишком велик для вычисления функции.");
            }
        }
        else
        {
            if (Math.Abs(x) > 1_000_000)
            {
                Console.WriteLine("Ошибка: для надёжного приведения аргумента требуется |x| ≤ 1000000.");
                continue;
            }
            var result = Algorithms.SineTaylor(x);
            Console.WriteLine($"Сумма ряда (ε = 10⁻⁶): {result.Sum:G17}");
            Console.WriteLine($"Math.Sin(x): {Math.Sin(x):G17}");
            Console.WriteLine($"Количество просуммированных членов: {result.Terms}");
        }
    }
    Console.WriteLine();
}
