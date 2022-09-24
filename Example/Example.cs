using NApi;

namespace Example
{
  public class Example
  {
    [ModuleExports]
    public static void ModuleExports(JSValue exports)
    {
      exports["plus"] = (JSCallback)(args => (double)args[0] + (double)args[1]);
      exports["helloPlusWorld"] = (JSCallback)(args => (string)args[0] + " world");
      exports["add"] = (JSCallback)(args =>
      {
        var arg1 = (string)args[0];
        return (JSCallback)(args => arg1 + (string)args[0]);
      });
    }
  }
}