using System.Net;
using System.Net.Sockets;
using ChatServer;
Console.OutputEncoding = System.Text.Encoding.UTF8;
if (args.Length > 1 || (args.Length == 1 && (!int.TryParse(args[0], out _) || int.Parse(args[0]) is < 1 or > 65535)))
{ Console.WriteLine("Порт должен быть целым числом от 1 до 65535."); return; }
int port = args.Length == 0 ? 5555 : int.Parse(args[0]);
using var stop = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) => { e.Cancel = true; stop.Cancel(); };
try
{
    using var server = new ChatRoom("chat.log");
    var listener = new TcpListener(IPAddress.Any, port);
    var tasks = new List<Task>();
    listener.Start();
    Console.WriteLine($"Сервер: порт {port}. Журнал: {Path.GetFullPath("chat.log")}. Ctrl+C — выход.");
    try
    {
        while (!stop.IsCancellationRequested)
        {
            TcpClient client = await listener.AcceptTcpClientAsync(stop.Token);
            tasks.RemoveAll(task => task.IsCompleted);
            tasks.Add(server.HandleAsync(client, stop.Token));
        }
    }
    catch (OperationCanceledException) { }
    finally
    {
        listener.Stop();
        server.DisconnectAll();
        await Task.WhenAll(tasks);
    }
}
catch (Exception e) when (e is SocketException or IOException or UnauthorizedAccessException)
{ Console.WriteLine($"Ошибка сервера: {e.Message}"); }
