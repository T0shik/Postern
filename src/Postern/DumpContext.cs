namespace Postern;

public class DumpContext
{
    public static readonly DumpContext Instance = new DumpContext();

    private readonly List<Dump> _dumps;
    private readonly Dictionary<int, object> _cache;

    private DumpContext()
    {
        _dumps = new();
        _cache = new();
    }

    public static void Submit(Dump obj) => Instance._dumps.Add(obj);
    public static List<Dump> Entries => Instance._dumps;
    public static void SetCacheEntry(object val) => Instance._cache[val.GetHashCode()] = val;
    public static object GetCacheEntry(int hash) => Instance._cache[hash];
}