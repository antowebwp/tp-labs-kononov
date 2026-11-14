using System.Text;

Console.InputEncoding = System.Text.Encoding.UTF8;
Console.OutputEncoding = System.Text.Encoding.UTF8;

Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
Console.WriteLine("Введите текст. Для окончания ввода введите отдельную строку .");
var lines = new List<string>();
while (Console.ReadLine() is { } line && line != ".") lines.Add(line);

Console.Write("Папка: ");
string? folder = Console.ReadLine();
Console.Write("Имя файла: ");
string? name = Console.ReadLine();
if (!Directory.Exists(folder) || string.IsNullOrWhiteSpace(name) ||
    name is "." or ".." || name.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
{
    Console.WriteLine("Укажите существующую папку и допустимое имя файла.");
    return;
}
Console.Write("Кодировка (1 — UTF-8, 2 — Windows-1251, 3 — DOS 866): ");
int codePage = Console.ReadLine() switch { "1" => 65001, "2" => 1251, "3" => 866, _ => 0 };
if (codePage == 0) return;
try
{
    string path = Path.Combine(folder!, name);
    var encoding = Encoding.GetEncoding(codePage, EncoderFallback.ExceptionFallback, DecoderFallback.ExceptionFallback);
    byte[] bytes = encoding.GetBytes(string.Join(Environment.NewLine, lines));
    File.WriteAllBytes(path, bytes);
    Console.WriteLine($"Сохранено: {Path.GetFullPath(path)}");
}
catch (Exception e) when (e is IOException or UnauthorizedAccessException or ArgumentException)
{
    Console.WriteLine(e.Message);
}
