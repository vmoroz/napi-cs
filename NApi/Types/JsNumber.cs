using System;
using static NApi.NodeApi;

namespace NApi.Types
{
  public sealed class JsNumber : JsValue
  {
    internal JsNumber(JSValueScope scope, IntPtr valuePtr) : base(scope, valuePtr)
    {
    }

    public static unsafe JsNumber Create(JSValueScope scope, double value)
    {
      IntPtr valuePtr = new IntPtr();
      napi_create_double((napi_env)scope, value, new napi_value_ptr { Pointer = new IntPtr(&valuePtr) }).ThrowIfFailed();
      return new JsNumber(scope, valuePtr);
    }

    public double ToDouble()
    {
      double result;
      napi_get_value_double((napi_env)Scope, new napi_value { Pointer = ValuePtr }, out result).ThrowIfFailed();
      return result;
    }
  }
}