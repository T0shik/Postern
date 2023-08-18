using Postern;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddPostern(opts =>
{
    opts.IncludedAssemblies.Add(typeof(Car).Assembly);
});

builder.Services.AddSingleton<Endpoints>();
builder.Services.AddSingleton(new Car("laaaada", "1980", 0));
builder.Services.AddSingleton(new Functionality()
{
    NumberFn = () => 5,
});

var app = builder.Build();

app.UsePostern();

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


app.MapGet("/res", () => typeof(Postern.PosternConfiguration).Assembly.GetManifestResourceInfo("Postern.static"));
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

public class Functionality
{
    public Func<int> NumberFn { get; set; }
}

public class Endpoints : Dictionary<string, Func<Task<object>>> { }
