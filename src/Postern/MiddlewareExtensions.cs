using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Postern.Endpoints;

namespace Postern;

public static class MiddlewareExtensions
{
    public static WebApplication UsePostern(this WebApplication app)
    {
        var env = app.Services.GetRequiredService<IWebHostEnvironment>();

        app.UseStaticFiles(new StaticFileOptions()
        {
            FileProvider = new MyEmbeddedFileProvider(
                typeof(MiddlewareExtensions).Assembly,
                $"{nameof(Postern)}.static"
            ),
            RequestPath = "/_postern/static"
        });

        app.MapGet("/_postern", Debug.Handle);
        app.MapGet("/_postern/obj/{hash:int}", GetObject.Handle);
        app.MapPost("/_postern/execute", ExecuteScript.Handle);

        return app;
    }
}