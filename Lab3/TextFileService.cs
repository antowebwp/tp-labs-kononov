using System.Text;
namespace Lab3;
public static class TextFileService
{
    public static Encoding GetEncoding(int codePage)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        if (codePage == 65001) return new UTF8Encoding(false, true);
        if (codePage != 1251 && codePage != 866) throw new ArgumentOutOfRangeException(nameof(codePage));
        return Encoding.GetEncoding(codePage, EncoderFallback.ExceptionFallback, DecoderFallback.ExceptionFallback);
    }

    public static async Task<Dictionary<int, long?>> MeasureAsync(string source)
    {
        var sizes = new Dictionary<int, long?> { [65001] = 0, [1251] = 0, [866] = 0 };
        using var reader = new StreamReader(source, GetEncoding(65001), false);
        char[] buffer = new char[4096];
        var encoders = sizes.Keys.ToDictionary(code => code, code => GetEncoding(code).GetEncoder());
        byte[] encoded = new byte[16384];
        int count;
        while ((count = await reader.ReadAsync(buffer)) > 0)
            foreach (int code in sizes.Keys.ToArray())
            {
                if (sizes[code] == null) continue;
                try
                {
                    encoders[code].Convert(buffer, 0, count, encoded, 0, encoded.Length, false, out _, out int bytes, out _);
                    sizes[code] += bytes;
                }
                catch (EncoderFallbackException) { sizes[code] = null; }
            }
        foreach (int code in sizes.Keys.ToArray())
        {
            if (sizes[code] == null) continue;
            try
            {
                encoders[code].Convert([], 0, 0, encoded, 0, encoded.Length, true, out _, out int bytes, out _);
                sizes[code] += bytes;
            }
            catch (EncoderFallbackException) { sizes[code] = null; }
        }
        return sizes;
    }

    public static async Task<string> SaveAsync(string source, string folder, string name, int codePage, bool overwrite)
    {
        if (!Directory.Exists(folder)) throw new DirectoryNotFoundException("Папка не существует.");
        if (string.IsNullOrWhiteSpace(name) || name is "." or ".." || name.Contains('/') || name.Contains('\\')
            || name.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            throw new ArgumentException("Некорректное имя файла.", nameof(name));
        Encoding encoding = GetEncoding(codePage);
        var sizes = await MeasureAsync(source);
        if (sizes[codePage] == null) throw new EncoderFallbackException("Текст содержит символы, отсутствующие в выбранной кодировке.");
        string target = Path.Combine(folder, name);
        if (File.Exists(target) && !overwrite) throw new IOException("Файл уже существует.");
        string temporary = Path.Combine(folder, $".{Guid.NewGuid():N}.tmp");
        try
        {
            using (var reader = new StreamReader(source, GetEncoding(65001), false))
            await using (var writer = new StreamWriter(temporary, false, encoding))
            {
                char[] buffer = new char[4096];
                int count;
                while ((count = await reader.ReadAsync(buffer)) > 0)
                    await writer.WriteAsync(buffer.AsMemory(0, count));
            }
            File.Move(temporary, target, overwrite);
            return Path.GetFullPath(target);
        }
        finally { if (File.Exists(temporary)) File.Delete(temporary); }
    }
}
