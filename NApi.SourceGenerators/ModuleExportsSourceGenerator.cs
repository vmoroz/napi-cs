using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace NApi.SourceGenerators
{
  [Generator]
  public class ModuleExportsSourceGenerator : ISourceGenerator
  {
    public void Execute(GeneratorExecutionContext context)
    {
      if (!(context.SyntaxContextReceiver is SyntaxReceiver receiver))
        return;

      StringBuilder source = new(
@"
using System;
using System.Runtime.InteropServices;

namespace NApi
{
  public class ModuleExports_AutoGen
  {
    private static bool _initialized;

    [UnmanagedCallersOnly(EntryPoint = ""napi_register_module_v1"")]
    public static napi_value napi_register_module_v1(napi_env env, napi_value exports)
    {
      if (!_initialized)
      {
        NodeApi.SetupDllImportResolver(typeof(NodeApi).Assembly);
        _initialized = true;
      }

      using JSRootValueScope scope = new(new JSEnvironment(env));
      var exports_ = new JSValue(scope, exports);
");
      foreach (var method in receiver.MethodList)
      {
        var methodName = method.ContainingType.ToDisplayString() + '.' + method.Name;
        source.Append($"{methodName}(exports_);");
      }

      source.Append(
@"
      return exports;
    }
  }
}
");

      Console.WriteLine(source.ToString());

      context.AddSource("ModuleExports_AutoGen", source.ToString());
    }

    public void Initialize(GeneratorInitializationContext context)
    {
      context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
    }

    public class SyntaxReceiver : ISyntaxContextReceiver
    {
      public List<IMethodSymbol> MethodList = new();

      public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
      {
        // Business logic to decide what we're interested in goes here
        if (context.Node is MethodDeclarationSyntax methodDeclarationSyntax &&
            methodDeclarationSyntax.AttributeLists.Count > 0)
        {
          var methodSymbol =
              context.SemanticModel.GetDeclaredSymbol(methodDeclarationSyntax) as IMethodSymbol;
          if (methodSymbol!.GetAttributes().Any(attr =>
              attr.AttributeClass!.ToDisplayString() == "NApi.ModuleExportsAttribute"))
          {
            MethodList.Add(methodSymbol);
          }
        }
      }
    }
  }
}