using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using static NApi.NodeApi;

namespace NApi.Types
{
  public ref struct JSCallbackInfo
  {
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

    public JSValue(JsScope scope, IntPtr valuePtr)
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
      return CreateJSValue((env, result) => napi_get_undefined(env, result));
    }

    public static JSValue GetNull()
    {
      return CreateJSValue((IntPtr env, IntPtr valuePtr) =>
        napi_get_null(env, valuePtr));
    }

    public static JSValue GetGlobal()
    {
      return CreateJSValue((IntPtr env, IntPtr valuePtr) =>
        napi_get_global(env, valuePtr));
    }

    public static JSValue GetBoolean(bool value)
    {
      return CreateJSValue((IntPtr env, IntPtr valuePtr) =>
        napi_get_boolean(env, value, valuePtr));
    }

    public static JSValue CreateObject()
    {
      return CreateJSValue((IntPtr env, IntPtr valuePtr) =>
        napi_create_object(env, valuePtr));
    }

    public static JSValue CreateArray()
    {
      return CreateJSValue((IntPtr env, IntPtr valuePtr) =>
        napi_create_array(env, valuePtr));
    }

    public static JSValue CreateArray(uint length)
    {
      return CreateJSValue((IntPtr env, IntPtr valuePtr) =>
        napi_create_array_with_length(env, (UIntPtr)length, valuePtr));
    }

    public static JSValue CreateNumber(double value)
    {
      return CreateJSValue((IntPtr env, IntPtr valuePtr) =>
        napi_create_double(env, value, valuePtr));
    }

    public static JSValue CreateNumber(int value)
    {
      return CreateJSValue((IntPtr env, IntPtr valuePtr) =>
        napi_create_int32(env, value, valuePtr));
    }

    public static JSValue CreateNumber(uint value)
    {
      return CreateJSValue((IntPtr env, IntPtr valuePtr) =>
        napi_create_uint32(env, value, valuePtr));
    }

    public static JSValue CreateNumber(long value)
    {
      return CreateJSValue((IntPtr env, IntPtr valuePtr) =>
        napi_create_int64(env, value, valuePtr));
    }

    public static JSValue CreateStringLatin1(ReadOnlyMemory<byte> value)
    {
      unsafe
      {
        return CreateJSValue((IntPtr env, IntPtr valuePtr) =>
          napi_create_string_latin1(env, value.Pin().Pointer, (UIntPtr)value.Length, valuePtr));
      }
    }

    public static JSValue CreateStringUtf8(ReadOnlyMemory<byte> value)
    {
      unsafe
      {
        return CreateJSValue((IntPtr env, IntPtr valuePtr) =>
          napi_create_string_utf8(env, value.Pin().Pointer, (UIntPtr)value.Length, valuePtr));
      }
    }

    public static JSValue CreateStringUtf16(ReadOnlyMemory<char> value)
    {
      unsafe
      {
        return CreateJSValue((IntPtr env, IntPtr valuePtr) =>
          napi_create_string_utf16(env, value.Pin().Pointer, (UIntPtr)value.Length, valuePtr));
      }
    }

    public static JSValue CreateStringUtf16(string value)
    {
      return CreateStringUtf16(value.AsMemory());
    }

    public static JSValue CreateSymbol(JSValue description)
    {
      return CreateJSValue((IntPtr env, IntPtr valuePtr) =>
        napi_create_symbol(env, description.ValuePtr, valuePtr));
    }

    public static JSValue CreateSymbol(string description)
    {
      return CreateJSValue((IntPtr env, IntPtr valuePtr) =>
        napi_create_symbol(env, CreateStringUtf16(description).ValuePtr, valuePtr));
    }

    public static unsafe JSValue CreateFunction(ReadOnlyMemory<byte> utf8Name, delegate* unmanaged[Cdecl]<IntPtr, IntPtr, IntPtr> callback, IntPtr data)
    {
      return CreateJSValue((IntPtr env, IntPtr valuePtr) =>
        napi_create_function(env, utf8Name.Pin().Pointer, (UIntPtr)utf8Name.Length, callback, data, valuePtr));
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
          napi_get_cb_info(scope.Env.EnvPtr, callbackInfo, null, IntPtr.Zero, IntPtr.Zero, new IntPtr(&data)).ThrowIfFailed(scope);
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
      napi_add_finalizer(
        func.Scope.Env.EnvPtr, func.ValuePtr, (IntPtr)callbackHandle, &FinalizeJSCallback, IntPtr.Zero, IntPtr.Zero).ThrowIfFailed(func.Scope);
      return func;
    }

    public static JSValue CreateFunction(string name, JSCallback callback)
    {
      return CreateFunction(Encoding.UTF8.GetBytes(name), callback);
    }

    public static JSValue CreateError(JSValue code, JSValue message)
    {
      return CreateJSValue((IntPtr env, IntPtr valuePtr) =>
        napi_create_error(env, code.ValuePtr, message.ValuePtr, valuePtr));
    }

    public static JSValue CreateTypeError(JSValue code, JSValue message)
    {
      return CreateJSValue((IntPtr env, IntPtr valuePtr) =>
        napi_create_type_error(env, code.ValuePtr, message.ValuePtr, valuePtr));
    }

    public static JSValue CreateRangeError(JSValue code, JSValue message)
    {
      return CreateJSValue((IntPtr env, IntPtr valuePtr) =>
        napi_create_range_error(env, code.ValuePtr, message.ValuePtr, valuePtr));
    }

    public unsafe JSValueType TypeOf()
    {
      JSValueType result;
      napi_typeof(Scope.Env.EnvPtr, ValuePtr, &result).ThrowIfFailed(Scope);
      return result;
    }

    public bool TryGetValue(out double value)
    {
      return napi_get_value_double(Scope.Env.EnvPtr, ValuePtr, out value) == napi_status.napi_ok;
    }

    public bool TryGetValue(out int value)
    {
      return napi_get_value_int32(Scope.Env.EnvPtr, ValuePtr, out value) == napi_status.napi_ok;
    }

    public bool TryGetValue(out uint value)
    {
      return napi_get_value_uint32(Scope.Env.EnvPtr, ValuePtr, out value) == napi_status.napi_ok;
    }

    public bool TryGetValue(out long value)
    {
      return napi_get_value_int64(Scope.Env.EnvPtr, ValuePtr, out value) == napi_status.napi_ok;
    }

    public bool TryGetValue(out bool value)
    {
      return napi_get_value_bool(Scope.Env.EnvPtr, ValuePtr, out value) == napi_status.napi_ok;
    }

    public unsafe bool TryGetValue(out string value)
    {
      // TODO: add check that the object is still alive
      // TODO: should we check value type first?
      nuint length;
      if (napi_get_value_string_utf16(Scope.Env.EnvPtr, ValuePtr, null, 0, &length) != napi_status.napi_ok)
      {
        value = string.Empty;
        return false;
      }

      char[] buf = new char[length + 1];
      fixed (char* bufStart = &buf[0])
      {
        napi_get_value_string_utf16(Scope.Env.EnvPtr, ValuePtr, bufStart, (nuint)buf.Length, null).ThrowIfFailed(Scope);
        value = new string(buf);
        return true;
      }
    }

    //TODO: implement in future
    //public static explicit operator double(JSValue value)
    //{
    //  double result;
    //  return value.TryGetValue(out result) ? result : 0.0;
    //}

    //public static implicit operator JSValue(double value) => CreateNumber(value);

    //public JSValue this[string name]
    //{
    //  get { return GetProperty(name); }
    //  set { SetProperty(name, value); }
    //}
  }
}
