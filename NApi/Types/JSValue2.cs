using System;

namespace NApi.Types
{
  // New class for JSValue
  public class JSValue
  {
    internal IntPtr ValuePtr { get; }

    internal JsScope Scope { get; }

    protected JSValue(IntPtr valuePtr, JsScope scope)
    {
      ValuePtr = valuePtr;
      Scope = scope;
    }

    private static JsScope GetScope()
    {
      JsScope? scope = JsScope.Current;
      if (scope == null)
        throw new InvalidOperationException("Scope is null");
      return scope;
    }

    private static JSValue CreateJSValue(Func<IntPtr, IntPtr, napi_status> creator)
    {
      JsScope scope = GetScope();
      IntPtr valuePtr;
      unsafe
      {
        creator(scope.Env.EnvPtr, new IntPtr(&valuePtr)).ThrowIfFailed(scope);
      }
      return new JSValue(valuePtr, scope);
    }

    public static JSValue GetUndefined()
    {
      return CreateJSValue((IntPtr env, IntPtr valuePtr) =>
        NApi.ApiProvider.JsNativeApi.napi_get_undefined(env, valuePtr));
    }

    public static JSValue GetNull()
    {
      return CreateJSValue((IntPtr env, IntPtr valuePtr) =>
        NApi.ApiProvider.JsNativeApi.napi_get_null(env, valuePtr));
    }

    public static JSValue GetGlobal()
    {
      return CreateJSValue((IntPtr env, IntPtr valuePtr) =>
        NApi.ApiProvider.JsNativeApi.napi_get_global(env, valuePtr));
    }

    public static JSValue GetBoolean(bool value)
    {
      return CreateJSValue((IntPtr env, IntPtr valuePtr) =>
        NApi.ApiProvider.JsNativeApi.napi_get_boolean(env, value, valuePtr));
    }

    public static JSValue CreateObject()
    {
      return CreateJSValue((IntPtr env, IntPtr valuePtr) =>
        NApi.ApiProvider.JsNativeApi.napi_create_object(env, valuePtr));
    }

    public static JSValue CreateArray()
    {
      return CreateJSValue((IntPtr env, IntPtr valuePtr) =>
        NApi.ApiProvider.JsNativeApi.napi_create_array(env, valuePtr));
    }

    public static JSValue CreateArray(uint length)
    {
      return CreateJSValue((IntPtr env, IntPtr valuePtr) =>
        NApi.ApiProvider.JsNativeApi.napi_create_array_with_length(env, (UIntPtr)length, valuePtr));
    }

    public static JSValue CreateNumber(double value)
    {
      return CreateJSValue((IntPtr env, IntPtr valuePtr) =>
        NApi.ApiProvider.JsNativeApi.napi_create_double(env, value, valuePtr));
    }

    public static JSValue CreateNumber(int value)
    {
      return CreateJSValue((IntPtr env, IntPtr valuePtr) =>
        NApi.ApiProvider.JsNativeApi.napi_create_int32(env, value, valuePtr));
    }

    public static JSValue CreateNumber(uint value)
    {
      return CreateJSValue((IntPtr env, IntPtr valuePtr) =>
        NApi.ApiProvider.JsNativeApi.napi_create_uint32(env, value, valuePtr));
    }

    public static JSValue CreateNumber(long value)
    {
      return CreateJSValue((IntPtr env, IntPtr valuePtr) =>
        NApi.ApiProvider.JsNativeApi.napi_create_int64(env, value, valuePtr));
    }

    public static JSValue CreateStringLatin1(ReadOnlyMemory<byte> value)
    {
      unsafe
      {
        return CreateJSValue((IntPtr env, IntPtr valuePtr) =>
          NApi.ApiProvider.JsNativeApi.napi_create_string_latin1(env, value.Pin().Pointer, (UIntPtr)value.Length, valuePtr));
      }
    }

    public static JSValue CreateStringUtf8(ReadOnlyMemory<byte> value)
    {
      unsafe
      {
        return CreateJSValue((IntPtr env, IntPtr valuePtr) =>
          NApi.ApiProvider.JsNativeApi.napi_create_string_utf8(env, value.Pin().Pointer, (UIntPtr)value.Length, valuePtr));
      }
    }

    public static JSValue CreateStringUtf16(ReadOnlyMemory<char> value)
    {
      unsafe
      {
        return CreateJSValue((IntPtr env, IntPtr valuePtr) =>
          NApi.ApiProvider.JsNativeApi.napi_create_string_utf16(env, value.Pin().Pointer, (UIntPtr)value.Length, valuePtr));
      }
    }

    public static JSValue CreateStringUtf16(string value)
    {
      return CreateStringUtf16(value.AsMemory());
    }
  }
}
