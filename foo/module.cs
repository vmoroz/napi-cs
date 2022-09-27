using NodeApi;
using static System.Console;

namespace Example;

public class Example
{
  [JSModuleExports]
  public static void ModuleExports(JSValue exports)
  {
    exports["hello"] = "world";
    exports["plus"] = (JSCallback)(args => (double)args[0] + (double)args[1]);
    exports["plusWorld"] = (JSCallback)(args => (string)args[0] + " world");
    exports["add"] = (JSCallback)(args =>
    {
      var arg1 = (string)args[0];
      return (JSCallback)(args => arg1 + (string)args[0]);
    });

    exports["MyClass"] = new JSClassBuilder<MyClass>("MyClass", args => new MyClass())
      .Property("prop1", 42)
      .Method("print", obj => obj.Print)
      .Method("printName", obj => obj.PrintName)
      .Property("prop2", obj => obj.Prop2, (obj, value) => obj.Prop2 = (int)value)
      .ToJSValue();
  }
}

public class MyClass
{
  public void Print()
  {
    WriteLine("Print method call");
  }

  public void PrintName(JSCallbackArgs args)
  {
    WriteLine("PrintName method call: {0}", (string)args[0].CoerceToString());
  }

  public int Prop2 { get; set; } = 12;
}