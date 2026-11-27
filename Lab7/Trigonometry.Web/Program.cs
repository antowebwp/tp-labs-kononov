using Trigonometry.Core;
using Newtonsoft.Json;
var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls(builder.Configuration["urls"] ?? "http://127.0.0.1:5077");
var app = builder.Build();
string dataDirectory = Path.Combine(app.Environment.ContentRootPath, "Data");
Directory.CreateDirectory(dataDirectory);
var calculators = new Dictionary<string, Calculator>
{
    ["memory"] = new(new MemoryHistory()),
    ["file"] = new(new FileHistory(Path.Combine(dataDirectory, "history.jsonl")))
};
var gate = new object();
app.UseDefaultFiles();
app.UseStaticFiles();
app.MapPost("/api/calculate", (CalculationRequest request) =>
{
    if (request.Storage == null || !calculators.TryGetValue(request.Storage, out var calculator))
        return Results.BadRequest(new { error = "Выберите хранение в памяти или в файле." });
    try
    {
        lock (gate) return Results.Ok(calculator.Calculate(request.Angle, request.Degrees, request.Digits));
    }
    catch (ArgumentOutOfRangeException)
    { return Results.BadRequest(new { error = "Угол должен быть конечным числом от −1000000 до 1000000, точность — от 0 до 12 знаков." }); }
    catch (Exception e) when (e is IOException or UnauthorizedAccessException or JsonException)
    { return Results.Problem("Не удалось записать историю. Проверьте доступ к папке Data или выберите хранение в памяти."); }
});
app.MapGet("/api/history", (string? storage, HttpResponse response) =>
{
    response.Headers.CacheControl = "no-store";
    if (!calculators.TryGetValue(storage ?? "memory", out var calculator))
        return Results.BadRequest(new { error = "Неизвестное хранилище." });
    try
    {
        lock (gate) return Results.Ok(calculator.GetHistory().TakeLast(30).Reverse().ToArray());
    }
    catch (Exception e) when (e is IOException or UnauthorizedAccessException or JsonException)
    { return Results.Problem("Не удалось прочитать файл истории. Выберите хранение в памяти или проверьте файл Data/history.jsonl."); }
});
app.Run();
public sealed record CalculationRequest(double Angle, bool Degrees, int Digits, string Storage = "memory");
