using System;
using static NApi.NodeApi;

namespace NApi.Types
{
  public sealed class JsNumber : JsValue
  {
    internal JsNumber(JsScope scope, IntPtr valuePtr) : base(scope, valuePtr)
    {
    }

    public static unsafe JsNumber Create(JsScope scope, double value)
    {
      IntPtr valuePtr = new IntPtr();
      napi_create_double(scope.Env, value, new IntPtr(&valuePtr)).ThrowIfFailed(scope);
      return new JsNumber(scope, valuePtr);
    }

    public double ToDouble()
    {
      double result;
      napi_get_value_double(Scope.Env, ValuePtr, out result).ThrowIfFailed(Scope);
      return result;
    }
  }
}