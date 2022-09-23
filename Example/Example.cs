using NApi;

namespace Example
{
  public class Example
  {
    [ModuleExports]
    public static void ModuleExports(JSValueScope scope, JSValue exports)
    {
      //exports.Set(JSValue.CreateStringUtf16("plus"), JSValue.CreateFunction("plus", (scope, @this, args) => JsNumber.Create(scope, ((JsNumber)args[0]).ToDouble() + ((JsNumber)args[1]).ToDouble())));
      //exports.Set(JsString.Create(scope, "helloPlusWorld"), JsFunction.Create(scope, "helloPlusWorld", (scope, @this, args) => JsString.Create(scope, ((JsString)args[0]).ToString() + " world")));
      //exports.Set(JsString.Create(scope, "add"), JsFunction.Create(scope, "add", (scope, @this, args) =>
      //{
      //  var arg1 = ((JsString)args[0]).ToString();
      //  return JsFunction.Create(scope, "hoc", (scope, @this, args) =>
      //  {
      //    var arg2 = ((JsString)args[0]).ToString();
      //    return JsString.Create(scope, arg1 + arg2);
      //  });
      //}));

      //exports["plus"] = (thisArg, args) => JSValue.CreateNumber(args[0].ToDouble() + args[1].ToDouble());
      //JSValue ex = new JSValue(exports.Scope, exports.ValuePtr);
      //ex["plus"] = 42;// (args) => (double)args[0] + (double)args[1];
      //exports["helloPlusWorld"] = (args) => (string)args[0] + " world";
      //exports["add"] = (info) =>
      //{
      //  string arg1 = args[0];
      //  return JSValue.CreateFunction("hoc", (args) =>
      //  {
      //    string arg2 = args[0];
      //    return arg1 + arg2;
      //  });
      //};
    }
  }
}