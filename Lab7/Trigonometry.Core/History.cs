using System.Text;
using Newtonsoft.Json;
namespace Trigonometry.Core;
public interface ICalculationHistory
{
    void Save(Calculation calculation);
    IReadOnlyList<Calculation> ReadAll();
}
public sealed class MemoryHistory : ICalculationHistory
{
    private readonly List<Calculation> _items = [];
    public void Save(Calculation calculation)
    {
        ArgumentNullException.ThrowIfNull(calculation);
        _items.Add(calculation);
    }
    public IReadOnlyList<Calculation> ReadAll() => _items.AsReadOnly();
}
public sealed class FileHistory : ICalculationHistory
{
    private readonly string _path;
    private readonly Encoding _encoding;
    public FileHistory(string path)
    {
        if (string.IsNullOrWhiteSpace(path)) throw new ArgumentException("Укажите файл истории.", nameof(path));
        _path = Path.GetFullPath(path);
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        _encoding = Encoding.GetEncoding(1251, EncoderFallback.ExceptionFallback, DecoderFallback.ExceptionFallback);
    }
    public void Save(Calculation calculation)
    {
        ArgumentNullException.ThrowIfNull(calculation);
        using var writer = new StreamWriter(_path, true, _encoding);
        writer.WriteLine(JsonConvert.SerializeObject(calculation));
    }
    public IReadOnlyList<Calculation> ReadAll()
    {
        if (!File.Exists(_path)) return Array.Empty<Calculation>();
        var items = new List<Calculation>();
        using var reader = new StreamReader(_path, _encoding, false);
        string? line;
        while ((line = reader.ReadLine()) != null)
            items.Add(JsonConvert.DeserializeObject<Calculation>(line) ?? throw new InvalidDataException("Повреждена запись истории."));
        return items.AsReadOnly();
    }
}
