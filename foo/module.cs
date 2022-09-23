using NApi;

namespace Example
{
  public class Example
  {
    [ModuleExports]
    public static void ModuleExports(JSValue exports)
    {
      exports["hello"] = "my world";
      exports["hi"] = "everyone";
    }
  }
}