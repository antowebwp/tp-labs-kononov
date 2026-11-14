using System.Net.Sockets;
using System.Text;
namespace ChatServer;
public sealed class ChatRoom : IDisposable
{
    private readonly Dictionary<string, Session> _users = new(StringComparer.OrdinalIgnoreCase);
    private readonly object _usersLock = new();
    private readonly object _logLock = new();
    private readonly StreamWriter _log;

    public ChatRoom(string logPath) => _log = new StreamWriter(logPath, true, new UTF8Encoding(false)) { AutoFlush = true };

    private void Log(string text)
    {
        string line = $"[{DateTimeOffset.Now:O}] {text}";
        lock (_logLock)
        {
            try { _log.WriteLine(line); }
            catch (Exception e) when (e is IOException or UnauthorizedAccessException) { Console.Error.WriteLine($"Ошибка журнала: {e.Message}"); }
        }
        Console.WriteLine(line);
    }

    private async Task BroadcastAsync(string text)
    {
        Session[] users;
        lock (_usersLock) users = _users.Values.ToArray();
        await Task.WhenAll(users.Select(user => user.SendAsync(text)));
    }

    public async Task HandleAsync(TcpClient client, CancellationToken token)
    {
        using var session = new Session(client);
        using var registration = token.Register(client.Close);
        string? nick = null;
        bool registered = false;
        try
        {
            while (!registered)
            {
                string? proposed = await session.Reader.ReadLineAsync(token);
                if (proposed == null) return;
                proposed = proposed.Trim();
                if (proposed.Length is < 1 or > 24 || proposed.Any(char.IsWhiteSpace) || proposed.Contains('/'))
                { await session.SendAsync("ERROR Ник: 1–24 символа, без пробелов и /."); continue; }
                lock (_usersLock)
                {
                    if (!_users.ContainsKey(proposed))
                    { _users.Add(proposed, session); nick = proposed; registered = true; }
                }
                if (!registered) await session.SendAsync("ERROR Ник занят. Введите другой.");
            }
            if (!await session.SendAsync("OK")) return;
            Log($"CONNECT {nick}");
            await BroadcastAsync($"*** {nick} вошёл в чат ***");
            string? line;
            while ((line = await session.Reader.ReadLineAsync(token)) != null)
            {
                if (line == "/exit") break;
                if (line.Length > 4096) { await session.SendAsync("ERROR Сообщение длиннее 4096 символов."); continue; }
                if (string.IsNullOrWhiteSpace(line)) continue;
                if (line == "/list")
                {
                    string[] names;
                    lock (_usersLock) names = _users.Keys.OrderBy(name => name, StringComparer.OrdinalIgnoreCase).ToArray();
                    await session.SendAsync("USERS " + string.Join(", ", names));
                }
                else if (line.StartsWith("/w ", StringComparison.Ordinal))
                {
                    string[] parts = line.Split(' ', 3, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length != 3) { await session.SendAsync("ERROR Формат: /w ник текст"); continue; }
                    Session? recipient;
                    lock (_usersLock) _users.TryGetValue(parts[1], out recipient);
                    if (recipient == null) { await session.SendAsync("ERROR Пользователь не найден."); continue; }
                    string message = $"[личное] {nick} → {parts[1]}: {parts[2]}";
                    if (await recipient.SendAsync(message))
                    {
                        if (recipient != session) await session.SendAsync(message);
                        Log($"PRIVATE {nick} -> {parts[1]}: {parts[2]}");
                    }
                    else await session.SendAsync("ERROR Пользователь отключился.");
                }
                else if (line.StartsWith('/')) await session.SendAsync("ERROR Команды: /list, /w ник текст, /exit");
                else
                {
                    Log($"MESSAGE {nick}: {line}");
                    await BroadcastAsync($"[{DateTime.Now:HH:mm:ss}] {nick}: {line}");
                }
            }
        }
        catch (Exception e) when (e is IOException or SocketException or OperationCanceledException or ObjectDisposedException) { }
        finally
        {
            if (registered && nick != null)
            {
                lock (_usersLock) _users.Remove(nick);
                Log($"DISCONNECT {nick}");
                await BroadcastAsync($"*** {nick} покинул чат ***");
            }
        }
    }

    public void DisconnectAll()
    {
        lock (_usersLock) foreach (Session user in _users.Values) user.Client.Close();
    }
    public void Dispose() { DisconnectAll(); _log.Dispose(); }

    private sealed class Session : IDisposable
    {
        private readonly StreamWriter _writer;
        private readonly SemaphoreSlim _sendLock = new(1, 1);
        public TcpClient Client { get; }
        public StreamReader Reader { get; }
        public Session(TcpClient client)
        {
            Client = client;
            var stream = client.GetStream();
            Reader = new StreamReader(stream, new UTF8Encoding(false));
            _writer = new StreamWriter(stream, new UTF8Encoding(false)) { AutoFlush = true };
        }
        public async Task<bool> SendAsync(string text)
        {
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(3));
            bool entered = false;
            try
            {
                await _sendLock.WaitAsync(timeout.Token);
                entered = true;
                await _writer.WriteLineAsync(text.AsMemory(), timeout.Token);
                return true;
            }
            catch (Exception e) when (e is IOException or SocketException or ObjectDisposedException or OperationCanceledException)
            { Client.Close(); return false; }
            finally { if (entered) _sendLock.Release(); }
        }
        public void Dispose() => Client.Dispose();
    }
}
