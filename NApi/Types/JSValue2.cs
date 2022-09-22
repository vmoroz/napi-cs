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

  [Flags]
  public enum JSPropertyAttributes : int
  {
    Default = 0,
    Writable = 1,
    Enumerable = 2,
    Configurable = 4,
    Static = 1024,
    DefaultMethod = Writable | Configurable,
    DefaultProperty = Writable | Enumerable | Configurable,
  }

  public class JSPropertyDescriptor
  {
    public JSValue Name { get; }
    public JSCallback? Method { get; } = null;
    public JSCallback? Getter { get; } = null;
    public JSCallback? Setter { get; } = null;
    public JSValue? Value { get; } = null;
    public JSPropertyAttributes Attributes { get; } = JSPropertyAttributes.Default;

    public JSPropertyDescriptor(JSValue name, JSValue value, JSPropertyAttributes attributes = JSPropertyAttributes.DefaultProperty)
    {
      Name = name;
      Value = value;
      Attributes = attributes;
    }

    public JSPropertyDescriptor(string name, JSValue value, JSPropertyAttributes attributes = JSPropertyAttributes.DefaultProperty)
      : this(JSValue.CreateStringUtf16(name), value, attributes)
    {
    }

    public JSPropertyDescriptor(JSValue name, JSCallback method, JSPropertyAttributes attributes = JSPropertyAttributes.DefaultMethod)
    {
      Name = name;
      Method = method;
      Attributes = attributes;
    }

    public JSPropertyDescriptor(string name, JSCallback method, JSPropertyAttributes attributes = JSPropertyAttributes.DefaultMethod)
      : this(JSValue.CreateStringUtf16(name), method, attributes)
    {
    }

    public JSPropertyDescriptor(JSValue name, JSCallback? getter, JSCallback? setter, JSPropertyAttributes attributes = JSPropertyAttributes.Configurable)
    {
      if (getter == null && setter == null)
        throw new ArgumentException($"Either `{nameof(getter)}` or `{nameof(setter)}` must be not null");
      Name = name;
      Getter = getter;
      Setter = setter;
      Attributes = attributes;
    }

    public JSPropertyDescriptor(string name, JSCallback? getter, JSCallback? setter, JSPropertyAttributes attributes = JSPropertyAttributes.Configurable)
      : this(JSValue.CreateStringUtf16(name), getter, setter, attributes)
    {
    }
  }

  // New class for JSValue
  public class JSValue
  {
    internal JsScope Scope { get; }

    internal napi_value Value { get; }

    public JSValue(JsScope scope, napi_value value)
    {
      Scope = scope;
      Value = value;
    }

    public JSValue(napi_value value)
    {
      Scope = JsScope.Current ?? throw new InvalidOperationException("No current scope");
      Value = value;
    }

    private static JsScope GetScope()
    {
      JsScope? scope = JsScope.Current;
      if (scope == null)
        throw new InvalidOperationException("Scope is null");
      return scope;
    }

    private static unsafe JSValue CreateJSValue(Func<napi_env, napi_value_ptr, napi_status> creator)
    {
      JsScope scope = GetScope();
      napi_value value;
      napi_value_ptr valuePtr = new napi_value_ptr { Pointer = new IntPtr(&value) };
      creator(scope.Env, valuePtr).ThrowIfFailed(scope);
      return new JSValue(scope, value);
    }

    public static JSValue GetUndefined()
    {
      return CreateJSValue((env, result) => napi_get_undefined(env, result));
    }

    public static JSValue GetNull()
    {
      return CreateJSValue((env, result) => napi_get_null(env, result));
    }

    public static JSValue GetGlobal()
    {
      return CreateJSValue((env, result) => napi_get_global(env, result));
    }

    public static JSValue GetBoolean(bool value)
    {
      return CreateJSValue((env, result) => napi_get_boolean(env, value, result));
    }

    public static JSValue CreateObject()
    {
      return CreateJSValue((env, result) => napi_create_object(env, result));
    }

    public static JSValue CreateArray()
    {
      return CreateJSValue((env, result) => napi_create_array(env, result));
    }

    public static JSValue CreateArray(int length)
    {
      return CreateJSValue((env, result) =>
        napi_create_array_with_length(env, (nuint)length, result));
    }

    public static JSValue CreateNumber(double value)
    {
      return CreateJSValue((env, result) => napi_create_double(env, value, result));
    }

    public static JSValue CreateNumber(int value)
    {
      return CreateJSValue((env, result) => napi_create_int32(env, value, result));
    }

    public static JSValue CreateNumber(uint value)
    {
      return CreateJSValue((env, result) => napi_create_uint32(env, value, result));
    }

    public static JSValue CreateNumber(long value)
    {
      return CreateJSValue((env, result) => napi_create_int64(env, value, result));
    }

    public static JSValue CreateStringLatin1(ReadOnlyMemory<byte> value)
    {
      unsafe
      {
        return CreateJSValue((env, result) =>
          napi_create_string_latin1(env, value.Pin().Pointer, (nuint)value.Length, result));
      }
    }

    public static JSValue CreateStringUtf8(ReadOnlyMemory<byte> value)
    {
      unsafe
      {
        return CreateJSValue((env, result) =>
          napi_create_string_utf8(env, value.Pin().Pointer, (nuint)value.Length, result));
      }
    }

    public static JSValue CreateStringUtf16(ReadOnlyMemory<char> value)
    {
      unsafe
      {
        return CreateJSValue((env, result) =>
          napi_create_string_utf16(env, value.Pin().Pointer, (nuint)value.Length, result));
      }
    }

    public static JSValue CreateStringUtf16(string value)
    {
      return CreateStringUtf16(value.AsMemory());
    }

    public static JSValue CreateSymbol(JSValue description)
    {
      return CreateJSValue((env, result) =>
        napi_create_symbol(env, description.Value, result));
    }

    public static JSValue CreateSymbol(string description)
    {
      return CreateJSValue((env, result) =>
        napi_create_symbol(env, CreateStringUtf16(description).Value, result));
    }

    public static unsafe JSValue CreateFunction(ReadOnlyMemory<byte> utf8Name, delegate* unmanaged[Cdecl]<IntPtr, IntPtr, IntPtr> callback, IntPtr data)
    {
      return CreateJSValue((env, result) =>
        napi_create_function(env, utf8Name.Pin().Pointer, (nuint)utf8Name.Length, callback, data, result));
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
          napi_get_cb_info(scope.Env, callbackInfo, null, IntPtr.Zero, IntPtr.Zero, new IntPtr(&data)).ThrowIfFailed(scope);
          JSCallback cb = (JSCallback)GCHandle.FromIntPtr(data).Target!;
          JSValue result = cb(cbInfo);
          // TODO: implement escapable scope
          return result.Value.Pointer;
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
        func.Scope.Env, func.Value.Pointer, (IntPtr)callbackHandle, &FinalizeJSCallback, IntPtr.Zero, null).ThrowIfFailed(func.Scope);
      return func;
    }

    public static JSValue CreateFunction(string name, JSCallback callback)
    {
      return CreateFunction(Encoding.UTF8.GetBytes(name), callback);
    }

    public static JSValue CreateError(JSValue code, JSValue message)
    {
      return CreateJSValue((env, result) =>
        napi_create_error(env, code.Value, message.Value, result));
    }

    public static JSValue CreateTypeError(JSValue code, JSValue message)
    {
      return CreateJSValue((env, result) =>
        napi_create_type_error(env, code.Value, message.Value, result));
    }

    public static JSValue CreateRangeError(JSValue code, JSValue message)
    {
      return CreateJSValue((env, result) =>
        napi_create_range_error(env, code.Value, message.Value, result));
    }

    public unsafe JSValueType TypeOf()
    {
      JSValueType result;
      napi_typeof(Scope.Env, Value, &result).ThrowIfFailed(Scope);
      return result;
    }

    public bool TryGetValue(out double value)
    {
      return napi_get_value_double(Scope.Env, Value, out value) == napi_status.napi_ok;
    }

    public bool TryGetValue(out int value)
    {
      return napi_get_value_int32(Scope.Env, Value, out value) == napi_status.napi_ok;
    }

    public bool TryGetValue(out uint value)
    {
      return napi_get_value_uint32(Scope.Env, Value, out value) == napi_status.napi_ok;
    }

    public bool TryGetValue(out long value)
    {
      return napi_get_value_int64(Scope.Env, Value, out value) == napi_status.napi_ok;
    }

    public bool TryGetValue(out bool value)
    {
      return napi_get_value_bool(Scope.Env, Value, out value) == napi_status.napi_ok;
    }

    public unsafe bool TryGetValue(out string value)
    {
      // TODO: add check that the object is still alive
      // TODO: should we check value type first?
      nuint length;
      if (napi_get_value_string_utf16(Scope.Env, Value, null, 0, &length) != napi_status.napi_ok)
      {
        value = string.Empty;
        return false;
      }

      char[] buf = new char[length + 1];
      fixed (char* bufStart = &buf[0])
      {
        napi_get_value_string_utf16(Scope.Env, Value, bufStart, (nuint)buf.Length, null).ThrowIfFailed(Scope);
        value = new string(buf);
        return true;
      }
    }

    //TODO: add more string functions

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

    public static explicit operator napi_value(JSValue value) => value.Value;

    public static implicit operator JSValue(napi_value value) => new JSValue(value);

    public JSValue CoerceToBoolean()
    {
      napi_value result;
      napi_coerce_to_bool(Scope.Env, (napi_value)this, out result).ThrowIfFailed(Scope);
      return result;
    }

    public JSValue CoerceToNumber()
    {
      napi_value result;
      napi_coerce_to_number(Scope.Env, (napi_value)this, out result).ThrowIfFailed(Scope);
      return result;
    }

    public JSValue CoerceToObject()
    {
      napi_value result;
      napi_coerce_to_object(Scope.Env, (napi_value)this, out result).ThrowIfFailed(Scope);
      return result;
    }

    public JSValue CoerceToString()
    {
      napi_value result;
      napi_coerce_to_string(Scope.Env, (napi_value)this, out result).ThrowIfFailed(Scope);
      return result;
    }

    public JSValue GetPrototype()
    {
      napi_value result;
      napi_get_prototype(Scope.Env, (napi_value)this, out result).ThrowIfFailed(Scope);
      return result;
    }

    public JSValue GetPropertyNames()
    {
      napi_value result;
      napi_get_property_names(Scope.Env, (napi_value)this, out result).ThrowIfFailed(Scope);
      return result;
    }

    public void SetProperty(JSValue key, JSValue value)
    {
      napi_set_property(Scope.Env, (napi_value)this, (napi_value)key, (napi_value)value).ThrowIfFailed(Scope);
    }

    public bool HasProperty(JSValue key)
    {
      napi_has_property(Scope.Env, (napi_value)this, (napi_value)key, out sbyte result).ThrowIfFailed(Scope);
      return result != 0;
    }

    public JSValue GetProperty(JSValue key)
    {
      napi_get_property(Scope.Env, (napi_value)this, (napi_value)key, out napi_value result).ThrowIfFailed(Scope);
      return result;
    }

    public bool DeleteProperty(JSValue key)
    {
      napi_delete_property(Scope.Env, (napi_value)this, (napi_value)key, out sbyte result).ThrowIfFailed(Scope);
      return result != 0;
    }

    public bool HasOwnProperty(JSValue key)
    {
      napi_has_own_property(Scope.Env, (napi_value)this, (napi_value)key, out sbyte result).ThrowIfFailed(Scope);
      return result != 0;
    }

    public void SetProperty(string name, JSValue value)
    {
      napi_set_named_property(Scope.Env, (napi_value)this, name, (napi_value)value).ThrowIfFailed(Scope);
    }

    public bool HasProperty(string name)
    {
      napi_has_named_property(Scope.Env, (napi_value)this, name, out sbyte result).ThrowIfFailed(Scope);
      return result != 0;
    }

    public JSValue GetProperty(string name)
    {
      napi_get_named_property(Scope.Env, (napi_value)this, name, out napi_value result).ThrowIfFailed(Scope);
      return result;
    }

    public void SetElement(int index, JSValue value)
    {
      napi_set_element(Scope.Env, (napi_value)this, (uint)index, (napi_value)value).ThrowIfFailed(Scope);
    }

    public bool HasElement(int index)
    {
      napi_has_element(Scope.Env, (napi_value)this, (uint)index, out sbyte result).ThrowIfFailed(Scope);
      return result != 0;
    }

    public JSValue GetElement(int index)
    {
      napi_get_element(Scope.Env, (napi_value)this, (uint)index, out napi_value result).ThrowIfFailed(Scope);
      return result;
    }

    public bool DeleteElement(int index)
    {
      napi_delete_element(Scope.Env, (napi_value)this, (uint)index, out sbyte result).ThrowIfFailed(Scope);
      return result != 0;
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    private static unsafe IntPtr InvokeJSMethod(IntPtr env, IntPtr callbackInfo)
    {
      try
      {
        using (var scope = new JsScope(new JsEnv(env)))
        {
          JSCallbackInfo cbInfo = new JSCallbackInfo(scope, callbackInfo);

          IntPtr data;
          napi_get_cb_info(scope.Env, callbackInfo, null, IntPtr.Zero, IntPtr.Zero, new IntPtr(&data)).ThrowIfFailed(scope);
          JSPropertyDescriptor desc = (JSPropertyDescriptor)GCHandle.FromIntPtr(data).Target!;
          JSValue result = desc.Method!.Invoke(cbInfo);
          // TODO: implement escapable scope
          return result.Value.Pointer;
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
    private static unsafe IntPtr InvokeJSGetter(IntPtr env, IntPtr callbackInfo)
    {
      try
      {
        using (var scope = new JsScope(new JsEnv(env)))
        {
          JSCallbackInfo cbInfo = new JSCallbackInfo(scope, callbackInfo);

          IntPtr data;
          napi_get_cb_info(scope.Env, callbackInfo, null, IntPtr.Zero, IntPtr.Zero, new IntPtr(&data)).ThrowIfFailed(scope);
          JSPropertyDescriptor desc = (JSPropertyDescriptor)GCHandle.FromIntPtr(data).Target!;
          JSValue result = desc.Getter!.Invoke(cbInfo);
          // TODO: implement escapable scope
          return result.Value.Pointer;
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
    private static unsafe IntPtr InvokeJSSetter(IntPtr env, IntPtr callbackInfo)
    {
      try
      {
        using (var scope = new JsScope(new JsEnv(env)))
        {
          JSCallbackInfo cbInfo = new JSCallbackInfo(scope, callbackInfo);

          IntPtr data;
          napi_get_cb_info(scope.Env, callbackInfo, null, IntPtr.Zero, IntPtr.Zero, new IntPtr(&data)).ThrowIfFailed(scope);
          JSPropertyDescriptor desc = (JSPropertyDescriptor)GCHandle.FromIntPtr(data).Target!;
          JSValue result = desc.Setter!.Invoke(cbInfo);
          // TODO: implement escapable scope
          return result.Value.Pointer;
        }
      }
      catch (System.Exception e)
      {
        //TODO: record as JS error
        Console.Error.WriteLine(e);
        throw;
      }
    }

    public unsafe void DefineProperties(params JSPropertyDescriptor[] descriptors)
    {
      int count = descriptors.Length;
      napi_property_descriptor* descriptorsPtr = stackalloc napi_property_descriptor[count];
      for (int i = 0; i < count; i++)
      {
        JSPropertyDescriptor descriptor = descriptors[i];
        napi_property_descriptor* descriptorPtr = &descriptorsPtr[i];
        descriptorPtr->name = (napi_value)descriptor.Name;
        descriptorPtr->utf8name = IntPtr.Zero;
        descriptorPtr->method = descriptor.Method != null ? &InvokeJSMethod : null;
        descriptorPtr->getter = descriptor.Getter != null ? &InvokeJSGetter : null;
        descriptorPtr->setter = descriptor.Setter != null ? &InvokeJSSetter : null;
        descriptorPtr->value = descriptor.Value != null ? (napi_value)descriptor.Value : napi_value.Null;
        descriptorPtr->attributes = (napi_property_attributes)(int)descriptor.Attributes;
        if (descriptor.Method != null || descriptor.Getter != null || descriptor.Setter != null)
        {
          GCHandle descriptorHandle = GCHandle.Alloc(descriptor);
          napi_add_finalizer(
            Scope.Env, (napi_value)this, (IntPtr)descriptorHandle, &FinalizeJSCallback, IntPtr.Zero, null).ThrowIfFailed(Scope);
          descriptorPtr->data = (IntPtr)descriptorHandle;
        }
        else
        {
          descriptorPtr->data = IntPtr.Zero;
        }
      }
      napi_define_properties(Scope.Env, (napi_value)this, (nuint)count, descriptorsPtr).ThrowIfFailed(Scope);
    }

    public bool IsArray()
    {
      napi_is_array(Scope.Env, (napi_value)this, out sbyte result).ThrowIfFailed(Scope);
      return result != 0;
    }

    public int GetLength()
    {
      napi_get_array_length(Scope.Env, (napi_value)this, out uint result).ThrowIfFailed(Scope);
      return (int)result;
    }

    public bool StrictEquals(JSValue other)
    {
      napi_strict_equals(Scope.Env, (napi_value)this, (napi_value)other, out sbyte result);
      return result != 0;
    }

    public unsafe JSValue Call()
    {
      napi_call_function(Scope.Env, (napi_value)GetUndefined(), (napi_value)this, 0, null, out napi_value result).ThrowIfFailed(Scope);
      return result;
    }

    public unsafe JSValue Call(JSValue thisArg)
    {
      napi_call_function(Scope.Env, (napi_value)thisArg, (napi_value)this, 0, null, out napi_value result).ThrowIfFailed(Scope);
      return result;
    }

    public unsafe JSValue Call(JSValue thisArg, JSValue arg0)
    {
      napi_value argValue0 = (napi_value)arg0;
      napi_call_function(Scope.Env, (napi_value)thisArg, (napi_value)this, 1, &argValue0, out napi_value result).ThrowIfFailed(Scope);
      return result;
    }

    public unsafe JSValue Call(JSValue thisArg, params JSValue[] args)
    {
      int argc = args.Length;
      napi_value* argv = stackalloc napi_value[argc];
      for (int i = 0; i < argc; ++i)
      {
        argv[i] = (napi_value)args[i];
      }
      napi_call_function(Scope.Env, (napi_value)thisArg, (napi_value)this,
        (nuint)argc, argv, out napi_value result).ThrowIfFailed(Scope);
      return result;
    }

    public unsafe JSValue CallAsConstructor()
    {
      napi_new_instance(Scope.Env, (napi_value)this, 0, null, out napi_value result).ThrowIfFailed(Scope);
      return result;
    }

    public unsafe JSValue CallAsConstructor(JSValue arg0)
    {
      napi_value argValue0 = (napi_value)arg0;
      napi_new_instance(Scope.Env, (napi_value)this, 1, &argValue0, out napi_value result).ThrowIfFailed(Scope);
      return result;
    }

    public unsafe JSValue CallAsConstructor(params JSValue[] args)
    {
      int argc = args.Length;
      napi_value* argv = stackalloc napi_value[argc];
      for (int i = 0; i < argc; ++i)
      {
        argv[i] = (napi_value)args[i];
      }
      napi_new_instance(Scope.Env, (napi_value)this, (nuint)argc, argv, out napi_value result).ThrowIfFailed(Scope);
      return result;
    }

    public bool InstanceOf(JSValue constructor)
    {
      napi_instanceof(Scope.Env, (napi_value)this, (napi_value)constructor, out sbyte result).ThrowIfFailed(Scope);
      return result != 0;
    }
  }
}
