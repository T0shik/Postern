using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using HyperTextExpression;
using HyperTextExpression.AspNetCore;
using Microsoft.AspNetCore.Http;
using Postern.RenderHtml;
using static HyperTextExpression.HtmlExp;

namespace Postern.Endpoints;

public class Debug
{
    public static IResult Handle() =>
        HtmlDoc(
                Head(
                    ("link", Attrs("rel", "stylesheet", "href", "/_postern/static/main.css"))
                ),
                Body(
                    Div(
                        Div(
                            Attrs("id", "editor")
                        ),
                        Div(
                            ("button", Attrs("onclick", "executeCode()"), "Run")
                        )
                    ),
                    Div(
                        DumpContext.Entries.Select(x => DumpToHtml.Value(x.Obj, stamp: x.Timestamp))
                    ),
                    ("script", Attrs("src", "https://unpkg.com/htmx.org@1.9.4",
                        "integrity", "sha384-zUfuhFKKZCbHTY6aRR46gxiqszMk5tcHjsVFxnUo8VMus4kHGVdIYVbOYYNlKmHV",
                        "crossorigin", "anonymous")
                    ),
                    ("script", Attrs("src", "/_postern/static/vs/loader.js")),
                    ("script", Attrs("src", "/_postern/static/main.js"))
                )
            )
            .ToIResult();
}