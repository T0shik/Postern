using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Postern.Endpoints;

namespace Postern;

public static class ServicesExtensions
{
    public static IServiceCollection AddPostern(
        this IServiceCollection services,
        Action<PosternConfiguration>? configureActions = null
    )
    {
        var config = new PosternConfiguration();
        configureActions?.Invoke(config);

        services.AddSingleton(config);
        services.AddTransient<ExecuteScript.ScriptState>();

        return services;
    }
}

public class PosternConfiguration
{
    public List<Assembly> IncludedAssemblies { get; } = new();

    public List<string> IncludedNamespaces { get; } = new()
    {
        "Postern",
        "System",
        "System.Linq",
        "System.Collections.Generic",
        "System.Threading.Tasks",
        "Microsoft.Extensions.DependencyInjection"
    };
}