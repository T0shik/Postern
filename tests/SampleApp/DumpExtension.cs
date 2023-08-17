using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net.Http.Headers;

namespace SampleApp;

public static class DumpExtension
{
    [Conditional("DEBUG")]
    public static void Dump<T>(this T obj)
    {
        DumpContext.Instance.Submit(new Dump(DateTime.Now, obj));
    }
}

public class DumpContext
{
    public static DumpContext Instance { get; } = new DumpContext();

    private readonly List<Dump> _queue;
    private readonly Dictionary<int, object> _cache;

    private DumpContext()
    {
        _queue = new();
        _cache = new();
    }

    public void Submit(Dump obj) => _queue.Add(obj);
    public List<Dump> Entries => _queue;
    public void SetCacheEntry(object val) => _cache[val.GetHashCode()] = val;
    public object GetCacheEntry(int hash) => _cache[hash];
}

public record Dump(DateTime Timestamp, object? Obj);