using System;
using static NApi.NodeApi;

namespace NApi.Types
{
  public sealed class JsObject : JsValue
  {
    internal JsObject(JSValueScope scope, IntPtr valuePtr) : base(scope, valuePtr)
    {
    }

    public JsValue this[JsValue key]
    {
      get => Get(key);
      set => Set(key, value);
    }

    public static unsafe JsObject Create(JSValueScope scope)
    {
      IntPtr valuePtr = new IntPtr();
      napi_create_object((napi_env)scope, new napi_value_ptr { Pointer = new IntPtr(&valuePtr) }).ThrowIfFailed();
      return new JsObject(scope, valuePtr);
    }

    public unsafe JsValue Get(JsValue key)
    {
      napi_get_property((napi_env)Scope, ValuePtr, key.ValuePtr, out napi_value result).ThrowIfFailed();
      return JsValue.Create(Scope, result.Pointer);
    }

    public void Set(JsValue key, JsValue value)
    {
      napi_set_property((napi_env)Scope, ValuePtr, key.ValuePtr, value.ValuePtr).ThrowIfFailed();
    }

    public static JsObject FromPointer(JSValueScope scope, IntPtr valuePtr)
    {
      return new(scope, valuePtr);
    }
  }
}