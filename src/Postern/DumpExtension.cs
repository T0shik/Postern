namespace Postern;

public static class DumpExtension
{
    public static void Dump<T>(this T obj)
    {
        DumpContext.Submit(new Dump(DateTime.Now, obj));
    }
}

public record Dump(DateTime Timestamp, object? Obj);