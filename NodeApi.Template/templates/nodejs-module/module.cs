using NodeApi;

namespace Example;

public class Example
{
  [ModuleExports]
  public static void ModuleExports(JSValue exports)
  {
    exports["hello"] = "world";
  }
}