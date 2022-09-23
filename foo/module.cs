using NApi;

namespace Example
{
  public class Example
  {
    [ModuleExports]
    public static void ModuleExports(JSValueScope scope, JSValue exports)
    {
      exports.SetProperty("hello", JSValue.CreateStringUtf16("my world"));
    }
  }
}