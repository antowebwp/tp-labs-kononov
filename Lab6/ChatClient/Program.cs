using System.Net.Sockets;
using System.Text;
Console.InputEncoding = Encoding.UTF8;
Console.OutputEncoding = Encoding.UTF8;
if (args.Length > 2 || (args.Length == 2 && (!int.TryParse(args[1], out _) || int.Parse(args[1]) is < 1 or > 65535)))
{ Console.WriteLine("Аргументы: адрес порт. Порт от 1 до 65535."); return; }
string host;
if (args.Length > 0) host = args[0];
else { Console.Write("Адрес сервера (Enter = localhost): "); host = Console.ReadLine() ?? ""; if (host.Length == 0) host = "localhost"; }
int port = args.Length == 2 ? int.Parse(args[1]) : 5555;
using var client = new TcpClient();
using var stop = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) => { e.Cancel = true; stop.Cancel(); client.Close(); };
try
{
    using var connectTimeout = CancellationTokenSource.CreateLinkedTokenSource(stop.Token);
    connectTimeout.CancelAfter(TimeSpan.FromSeconds(5));
    await client.ConnectAsync(host, port, connectTimeout.Token);
    using var reader = new StreamReader(client.GetStream(), new UTF8Encoding(false));
    await using var writer = new StreamWriter(client.GetStream(), new UTF8Encoding(false)) { AutoFlush = true };
    while (true)
    {
        Console.Write("Ваш ник: ");
        string? nick = Console.ReadLine();
        if (nick == null) return;
        await writer.WriteLineAsync(nick);
        string? response = await reader.ReadLineAsync(stop.Token);
        if (response == "OK") break;
        if (response == null) { Console.WriteLine("Сервер закрыл соединение."); return; }
        Console.WriteLine(response);
    }
    Console.WriteLine("Подключено. /list, /w ник текст, /exit");
    async Task ReceiveAsync()
    {
        string? message;
        while ((message = await reader.ReadLineAsync(stop.Token)) != null) Console.WriteLine(message);
        Console.WriteLine("Соединение с сервером закрыто.");
    }
    async Task SendAsync()
    {
        while (!stop.IsCancellationRequested)
        {
            string? line = await Console.In.ReadLineAsync(stop.Token);
            if (line == null) { await writer.WriteLineAsync("/exit"); return; }
            await writer.WriteLineAsync(line);
            if (line == "/exit") return;
        }
    }
    Task receiving = ReceiveAsync();
    Task sending = Task.Run(SendAsync);
    Task completed = await Task.WhenAny(receiving, sending);
    await completed;
    stop.Cancel();
    client.Close();
    try { await receiving; } catch (Exception e) when (e is IOException or OperationCanceledException or ObjectDisposedException) { }
}
catch (Exception e) when (e is SocketException or IOException or OperationCanceledException or ArgumentException or ObjectDisposedException)
{ Console.WriteLine($"Соединение недоступно или закрыто: {e.Message}"); }
