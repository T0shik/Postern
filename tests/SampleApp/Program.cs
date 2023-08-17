using HyperTextExpression;
using HyperTextExpression.AspNetCore;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.Extensions.FileProviders;
using SampleApp;
using static HyperTextExpression.HtmlExp;

var editorJs =
    $$$"""
       var editor;
        require.config({ paths: { vs: '../node_modules/monaco-editor/min/vs' } });
              
        require(['vs/editor/editor.main'], function () {
              editor = monaco.editor.create(document.getElementById('editor'), {
              value: "Console.WriteLine(\"Hello World\");",
              language: 'csharp'
              });
        });
        
        function executeCode(){
            const code = editor.getValue();
            
            return fetch("/_debug/execute", {
                method: "POST",
                headers: {
                    'content-type': 'application/json'
                },
                body: JSON.stringify({
                    code
                })
            })
        }
       """;

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
                background-color: var(--dark);
                color: var(--primary);
             }

             #editor {
                width: 800px;
                height: 300px;
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

builder.Services.AddTransient<ScriptState>();
builder.Services.AddSingleton<Endpoints>();
builder.Services.AddSingleton(new Car("laaaada", "1980", 0));
builder.Services.AddSingleton(new Functionality()
{
    NumberFn = () => 5,
});
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

app.UseStaticFiles(new StaticFileOptions()
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(builder.Environment.ContentRootPath, "node_modules")
    ),
    RequestPath = "/node_modules"
});

app.Use(async (ctx, next) =>
{
    var customEndpoints = ctx.RequestServices.GetRequiredService<Endpoints>();

    if (customEndpoints.TryGetValue(ctx.Request.Path, out var endpoint))
    {
        var result = await endpoint();
        await ctx.Response.WriteAsJsonAsync(result);
    }
    else
    {
        await next();
    }
});

app.MapGet("/_debug", () =>
    HtmlDoc(
            Head(("style", css)),
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
                    DumpContext.Instance.Entries.Select(x => PrintValue(x.Obj, stamp: x.Timestamp))
                ),
                ("script",
                    Attrs("src", "https://unpkg.com/htmx.org@1.9.4",
                        "integrity", "sha384-zUfuhFKKZCbHTY6aRR46gxiqszMk5tcHjsVFxnUo8VMus4kHGVdIYVbOYYNlKmHV",
                        "crossorigin", "anonymous")
                ),
                ("script",
                    Attrs("src", "/node_modules/monaco-editor/dev/vs/loader.js")
                ),
                ("script", editorJs)
            )
        )
        .ToIResult()
);

app.MapGet(
    "/_debug/obj/{hash:int}",
    (int hash) => PrintValue(DumpContext.Instance.GetCacheEntry(hash)).ToIResult()
);

app.MapPost("/_debug/execute", async (ExecuteCodeCommand command, ScriptState scriptState) =>
    await CSharpScript.EvaluateAsync(
        command.Code,
        options: ScriptOptions.Default
            .WithReferences(
                typeof(Car).Assembly
            )
            .WithImports(
                "System",
                "System.Linq",
                "System.Collections.Generic",
                "System.Threading.Tasks",
                "Microsoft.Extensions.DependencyInjection"
            )
        ,
        globals: scriptState
    )
);

app.MapGet("/", () =>
{
    var a = new
    {
        a = 5,
        b = "Hello World",
        c = new
        {
            a = 5,
            b = "Hello World",
        },
        d = DateTimeOffset.Now,
    };
    return a;
});

app.MapGet("/car", (string make, string year, int miles) =>
{
    var car = new Car(make, year, miles);
    car.Dump();
    return car;
});

app.MapGet("/lada", (Car car) => car);

app.MapGet("/number", (Functionality func) => func.NumberFn());

app.Run();

public class Car
{
    public string Make { get; set; }
    public string Year { get; set; }
    public int Miles { get; set; }

    public Car(string make, string year, int miles)
    {
        Make = make;
        Year = year;
        Miles = miles;
    }
}

public record ExecuteCodeCommand(string Code);

public class ScriptState
{
    public ScriptState(IServiceProvider services)
    {
        Services = services;
    }

    public IServiceProvider Services { get; }
    public DumpContext Context => DumpContext.Instance;
}

public class Functionality
{
    public Func<int> NumberFn { get; set; }
}

public class Endpoints : Dictionary<string, Func<Task<object>>> { }