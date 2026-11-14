using Lab3;
using System.Text;
Console.InputEncoding = Encoding.UTF8;
Console.OutputEncoding = Encoding.UTF8;
string source = Path.GetTempFileName();
try
{
    Console.WriteLine("Введите текст. Пустая строка завершает ввод:");
    await using (var writer = new StreamWriter(source, false, TextFileService.GetEncoding(65001)))
    {
        string? line;
        while (!string.IsNullOrEmpty(line = Console.ReadLine())) await writer.WriteLineAsync(line);
    }
    Console.Write("Папка: "); string folder = Console.ReadLine() ?? "";
    Console.Write("Имя файла: "); string name = Console.ReadLine() ?? "";
    Console.Write("Кодировка (1 — UTF-8, 2 — Windows-1251, 3 — CP866): ");
    int code = Console.ReadLine() switch { "1" => 65001, "2" => 1251, "3" => 866, _ => throw new ArgumentException("Выберите 1, 2 или 3.") };
    var sizes = await TextFileService.MeasureAsync(source);
    foreach (var item in sizes)
        Console.WriteLine($"{TextFileService.GetEncoding(item.Key).WebName}: {(item.Value.HasValue ? item.Value + " байт" : "текст не представим без потерь")}");
    bool overwrite = false;
    if (File.Exists(Path.Combine(folder, name)))
    {
        Console.Write("Файл существует. Перезаписать? (да): ");
        overwrite = string.Equals(Console.ReadLine(), "да", StringComparison.OrdinalIgnoreCase);
        if (!overwrite) { Console.WriteLine("Сохранение отменено."); return; }
    }
    string saved = await TextFileService.SaveAsync(source, folder, name, code, overwrite);
    Console.WriteLine($"Сохранено: {saved}. Размер: {new FileInfo(saved).Length} байт.");
}
catch (Exception e) when (e is IOException or UnauthorizedAccessException or ArgumentException or NotSupportedException)
{ Console.WriteLine($"Ошибка: {e.Message}"); }
finally { try { File.Delete(source); } catch (IOException) { } catch (UnauthorizedAccessException) { } }
