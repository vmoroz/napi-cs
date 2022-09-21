using System;
using System.Text;
using static NApi.NodeApi;

namespace NApi.Types
{
  public sealed class JsString : JsValue
  {
    internal JsString(JsScope scope, IntPtr valuePtr) : base(scope, valuePtr)
    {
    }

    public static unsafe JsString Create(JsScope scope, string value)
    {
      IntPtr valuePtr = new IntPtr();
      byte[] utf8Str = Encoding.UTF8.GetBytes(value);
      napi_create_string_utf8(new napi_env { Pointer = scope.Env.EnvPtr }, utf8Str.AsMemory().Pin().Pointer,
        (UIntPtr)utf8Str.Length, new napi_value_ptr { Pointer = new IntPtr(&valuePtr) }).ThrowIfFailed(scope);
      return new JsString(scope, valuePtr);
    }

    public override unsafe string ToString()
    {
      nuint length;
      napi_get_value_string_utf8(Scope.Env, new napi_value { Pointer = ValuePtr }, null, 0, &length).ThrowIfFailed(Scope);

      sbyte[] buf = new sbyte[length + 1];
      fixed (sbyte* bufStart = &buf[0])
      {
        napi_get_value_string_utf8(Scope.Env, new napi_value { Pointer = ValuePtr }, bufStart, (nuint)buf.Length, null).ThrowIfFailed(Scope);
        return new string(bufStart, 0, buf.Length - 1, Encoding.UTF8);
      }
    }
  }
}