using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using static NApi.Types.JsFunction;

namespace NApi.Types
{
  public ref struct JSCallbackInfo {
    internal JsScope Scope { get; }

    internal IntPtr CallbackInfo { get; }

    public JSCallbackInfo(JsScope scope, IntPtr callbackInfo)
    {
      Scope = scope;
      CallbackInfo = callbackInfo;
    }
  }

  public delegate JSValue JSCallback(JSCallbackInfo info);

  // New class for JSValue
  public class JSValue
  {
    internal JsScope Scope { get; }

    internal IntPtr ValuePtr { get; }

    protected JSValue(JsScope scope, IntPtr valuePtr)
    {
      Scope = scope;
      ValuePtr = valuePtr;
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
      return new JSValue(scope, valuePtr);
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

    public static JSValue CreateSymbol(JSValue description)
    {
        return CreateJSValue((IntPtr env, IntPtr valuePtr) =>
          NApi.ApiProvider.JsNativeApi.napi_create_symbol(env, description.ValuePtr, valuePtr));
    }

    public static JSValue CreateSymbol(string description)
    {
      return CreateJSValue((IntPtr env, IntPtr valuePtr) =>
        NApi.ApiProvider.JsNativeApi.napi_create_symbol(env, CreateStringUtf16(description).ValuePtr, valuePtr));
    }

    public static unsafe JSValue CreateFunction(ReadOnlyMemory<byte> utf8Name, delegate *unmanaged[Cdecl]<IntPtr, IntPtr, IntPtr> callback, IntPtr data)
    {
        return CreateJSValue((IntPtr env, IntPtr valuePtr) =>
          NApi.ApiProvider.JsNativeApi.napi_create_function(env, utf8Name.Pin().Pointer, (UIntPtr)utf8Name.Length, callback, data, valuePtr));
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    private static unsafe IntPtr InvokeJSCallback(IntPtr env, IntPtr callbackInfo)
    {
      try
      {
        using (var scope = new JsScope(new JsEnv(env)))
        {
          JSCallbackInfo cbInfo = new JSCallbackInfo(scope, callbackInfo);

          IntPtr data;
          NApi.ApiProvider.JsNativeApi.napi_get_cb_info(scope.Env.EnvPtr, callbackInfo, null, IntPtr.Zero, IntPtr.Zero, new IntPtr(&data)).ThrowIfFailed(scope);
          JSCallback cb = (JSCallback)GCHandle.FromIntPtr(data).Target!;
          JSValue result = cb(cbInfo);
          // TODO: implement escapable scope
          return result.ValuePtr;
        }
      }
      catch (System.Exception e)
      {
        //TODO: record as JS error
        Console.Error.WriteLine(e);
        throw;
      }
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    private static unsafe void FinalizeJSCallback(IntPtr env, IntPtr data, IntPtr hint)
    {
      GCHandle callbackHandle = GCHandle.FromIntPtr(data);
      callbackHandle.Free();
    }

    public static unsafe JSValue CreateFunction(ReadOnlyMemory<byte> utf8Name, JSCallback callback)
    {
      GCHandle callbackHandle = GCHandle.Alloc(callback);
      JSValue func = CreateFunction(utf8Name, &InvokeJSCallback, (IntPtr)callbackHandle);
      NApi.ApiProvider.JsNativeApi.napi_add_finalizer(
        func.Scope.Env.EnvPtr, func.ValuePtr, (IntPtr)callbackHandle, &FinalizeJSCallback, IntPtr.Zero, IntPtr.Zero).ThrowIfFailed(func.Scope);
      return func;
    }

    public static unsafe JSValue CreateFunction(string name, JSCallback callback)
    {
      return CreateFunction(Encoding.Default.GetBytes(name), callback);
    }
  }
}
