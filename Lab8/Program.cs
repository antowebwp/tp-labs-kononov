using Lab8.Optimization;
if (CommandLine.TryRun(args)) return;
var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls(builder.Configuration["urls"] ?? "http://127.0.0.1:5088");
builder.Services.AddSingleton<RunService>();
var app = builder.Build();
app.UseDefaultFiles();
app.UseStaticFiles();
app.MapPost("/api/runs", (RunRequest request, RunService service) =>
{
    try
    {
        var snapshot = service.Start(request.Count, request.Repetitions);
        return Results.Accepted("/api/runs/" + snapshot.Id, snapshot);
    }
    catch (ArgumentOutOfRangeException) { return Results.BadRequest(new { error = "Количество покупок: 100–100000. Число повторений: 1 или 3." }); }
    catch (InvalidOperationException e) { return Results.Conflict(new { error = e.Message }); }
});
app.MapGet("/api/active", (RunService service, HttpResponse response) =>
{ response.Headers.CacheControl = "no-store"; return Results.Ok(new { run = service.Active() }); });
app.MapGet("/api/runs/{id}", (string id, RunService service, HttpResponse response) =>
{
    response.Headers.CacheControl = "no-store";
    var snapshot = service.Get(id);
    return snapshot == null ? Results.NotFound(new { error = "Расчёт не найден. Запустите новый замер." }) : Results.Ok(snapshot);
});
app.MapPost("/api/runs/{id}/cancel", (string id, RunService service) =>
    service.Cancel(id) ? Results.Ok(new { message = "Запрошена отмена." }) : Results.Conflict(new { error = "Этот расчёт уже завершён." }));
app.Run();
public sealed record RunRequest(int Count = 30000, int Repetitions = 1);
