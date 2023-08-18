using HyperTextExpression.AspNetCore;
using Microsoft.AspNetCore.Http;
using Postern.RenderHtml;

namespace Postern.Endpoints;

public class GetObject
{
    public static IResult Handle(int hash) =>
        DumpToHtml.Value(DumpContext.GetCacheEntry(hash)).ToIResult();
}