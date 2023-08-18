using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;

namespace Postern.Endpoints;

public class ExecuteScript
{
    public record ExecuteCodeCommand(string Code);
    public static async Task<IResult> Handle(
        ExecuteCodeCommand command, 
        ScriptState scriptState,
        PosternConfiguration posternConfiguration
    )
    {
        try
        {
            var result = await CSharpScript.EvaluateAsync(
                command.Code,
                options: ScriptOptions.Default
                    .WithReferences(
                        posternConfiguration.IncludedAssemblies
                            .Append(typeof(ExecuteScript).Assembly)
                    )
                    .WithImports(
                        posternConfiguration.IncludedNamespaces
                    )
                ,
                globals: scriptState
            );

            return Results.Ok(result);
        }
        catch (Exception e)
        {
            return Results.BadRequest(e.ToString());
        }
    }
    
    public class ScriptState
    {
        public ScriptState(IServiceProvider services)
        {
            Services = services;
        }

        public IServiceProvider Services { get; }
        public DumpContext Context => DumpContext.Instance;
    }
}



