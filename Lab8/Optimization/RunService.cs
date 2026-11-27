namespace Lab8.Optimization;
public sealed record RunSnapshot(string Id, string State, int Progress, string Stage, BenchmarkResult? Result, string? Error);
public sealed class RunService : IDisposable
{
    private sealed class Job
    {
        public CancellationTokenSource Cancellation { get; } = new();
        public RunSnapshot Snapshot { get; set; }
        public Job(string id) => Snapshot = new(id, "running", 0, "warmup", null, null);
    }
    private readonly object _gate = new();
    private readonly Dictionary<string, Job> _jobs = [];
    private string? _active;
    private Task? _worker;
    public RunSnapshot Start(int count, int repetitions)
    {
        if (count is < 100 or > 100000 || repetitions is not (1 or 3)) throw new ArgumentOutOfRangeException(nameof(count));
        lock (_gate)
        {
            if (_active != null) throw new InvalidOperationException("Расчёт уже выполняется. Дождитесь завершения или отмените его.");
            while (_jobs.Count >= 8)
            {
                string oldest = _jobs.Keys.First();
                _jobs[oldest].Cancellation.Dispose();
                _jobs.Remove(oldest);
            }
            string id = Guid.NewGuid().ToString("N");
            var job = new Job(id);
            _jobs.Add(id, job);
            _active = id;
            _worker = Task.Run(() =>
            {
                try
                {
                    var result = BenchmarkEngine.Run(count, repetitions, (progress, stage) =>
                    {
                        lock (_gate) job.Snapshot = job.Snapshot with { Progress = progress, Stage = stage };
                    }, job.Cancellation.Token);
                    lock (_gate) job.Snapshot = job.Snapshot with { State = "completed", Progress = 100, Stage = "done", Result = result };
                }
                catch (OperationCanceledException)
                { lock (_gate) job.Snapshot = job.Snapshot with { State = "cancelled", Stage = "cancelled" }; }
                catch (Exception)
                { lock (_gate) job.Snapshot = job.Snapshot with { State = "failed", Error = "Не удалось выполнить замер. Попробуйте меньше покупок." }; }
                finally { lock (_gate) _active = null; }
            });
            return job.Snapshot;
        }
    }
    public RunSnapshot? Get(string id) { lock (_gate) return _jobs.TryGetValue(id, out var job) ? job.Snapshot : null; }
    public RunSnapshot? Active() { lock (_gate) return _active == null ? null : _jobs[_active].Snapshot; }
    public bool Cancel(string id)
    {
        lock (_gate)
        {
            if (!_jobs.TryGetValue(id, out var job) || job.Snapshot.State != "running") return false;
            job.Cancellation.Cancel();
            return true;
        }
    }
    public void Dispose()
    {
        Task? worker;
        lock (_gate) { if (_active != null) _jobs[_active].Cancellation.Cancel(); worker = _worker; }
        worker?.GetAwaiter().GetResult();
        lock (_gate) foreach (Job job in _jobs.Values) job.Cancellation.Dispose();
    }
}
