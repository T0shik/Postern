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
                    Styles("/_postern/static/main.css")
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
                    Script("https://unpkg.com/htmx.org@1.9.4",
                        Attrs("integrity", "sha384-zUfuhFKKZCbHTY6aRR46gxiqszMk5tcHjsVFxnUo8VMus4kHGVdIYVbOYYNlKmHV",
                            "crossorigin", "anonymous")
                    ),
                    Script("/_postern/static/vs/loader.js"),
                    Script("/_postern/static/main.js")
                )
            )
            .ToIResult();
}