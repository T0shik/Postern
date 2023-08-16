using HyperTextExpression;
using HyperTextExpression.AspNetCore;
using SampleApp;
using static HyperTextExpression.HtmlExp;

var css = $$$"""
             :root {
               --primary: #adeb96;
               --secondary: #c191ff;
               --obj: #c9a26d;
               --title: #6c95eb;
               --link: #66c3cc;
               --link-accent: #a7eaf0;
               --dark: #2b2b2b;
               --dark-accent: #3b3b3b;
               --gray: #a6a6a6;
             }

             body {
                display: flex;
                background-color: var(--dark);
                color: var(--primary);
             }

             .value {
                 color: var(--secondary);
             }

             .link {
                 color: var(--link);
             }

             .link:hover {
                 color: var(--link-accent);
                 cursor: pointer;
             }


             .obj {
                display: flex;
                flex-direction: column;
                border: 2px solid var(--obj);
                margin: 0.25rem;
             }

             header {
                color: var(--title);
                padding: 0.25rem;
             }

             .member > td {
                border-top: 1px solid var(--gray);
             }

             .member > td:nth-child(1) {
                background-color: var(--dark-accent);
                border-right: 1px solid var(--gray);
             }

             .member > td {
                padding: 0.25rem 0.5rem;
             }
             """;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

static HtmlEl PrintValue(object? val, bool recursive = false, DateTime? stamp = null)
{
    if (val == null)
    {
        return Div(Attrs("class", "value", "null"));
    }

    var type = val.GetType();
    if (val is string || type.IsPrimitive)
    {
        return Div($"{val}");
    }

    if (recursive)
    {
        DumpContext.Instance.SetCacheEntry(val);
        return Div(
            ("a",
                Attrs("class", "link",
                    "hx-get", $"/_debug/obj/{val.GetHashCode()}",
                    "hx-swap", "outerHTML",
                    "hx-target", "this"
                ),
                type.Name
            )
        );
    }

    return
        HtmlEl("div",
            Attrs("class", "obj"),
            ("header", $"{type.Name}"),
            HtmlEl("table",
                ("tbody",
                    type.GetFields()
                        .Select(field =>
                            HtmlEl("tr",
                                Attrs("class", "member"),
                                ("td", field.Name),
                                HtmlEl("td", PrintValue(field.GetValue(val), recursive: true))
                            )
                        )
                        .Concat(type.GetProperties()
                            .Select(property =>
                                HtmlEl("tr",
                                    Attrs("class", "member"),
                                    ("td", property.Name),
                                    HtmlEl("td", PrintValue(property.GetValue(val), recursive: true))
                                )
                            )
                        )
                )
            )
        );
}

app.MapGet("/_debug", () =>
    HtmlDoc(
            Head(("style", css)),
            Body(
                Div(
                    DumpContext.Instance.Entries.Select(x => PrintValue(x.Obj, stamp: x.Timestamp))
                ),
                ("script",
                    Attrs("src", "https://unpkg.com/htmx.org@1.9.4",
                        "integrity", "sha384-zUfuhFKKZCbHTY6aRR46gxiqszMk5tcHjsVFxnUo8VMus4kHGVdIYVbOYYNlKmHV",
                        "crossorigin", "anonymous")
                )
            )
        )
        .ToIResult()
);

app.MapGet(
    "/_debug/obj/{hash:int}",
    (int hash) => PrintValue(DumpContext.Instance.GetCacheEntry(hash)).ToIResult()
);

app.MapGet("/", () =>
{
    new
    {
        a = 5,
        b = "Hello World",
        c = new
        {
            a = 5,
            b = "Hello World",
        },
        d = DateTimeOffset.Now,
    }.Dump();
    return "Ok";
});

app.MapGet("/car", (string make, string year, int miles) =>
{
    var car = new Car(make, year, miles);
    car.Dump();
    return car;
});

app.Run();

public record Car(string Make, string Year, int Miles);