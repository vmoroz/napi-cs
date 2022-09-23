﻿using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using static NApi.NodeApi;

namespace NApi
{
  public ref struct JSCallbackArgs
  {
    private JSValue[] _args;

    public JSValue this[int index]
    {
      get { return _args[index]; }
    }

    public int Length
    {
      get { return _args.Length; }
    }

    public JSValue ThisArg { get; }

    public IntPtr Data { get; }

    public JSValue GetNewTarget()
    {
      napi_get_new_target((napi_env)Scope, CallbackInfo, out napi_value result).ThrowIfFailed();
      return result;
    }

    internal JSValueScope Scope { get; }

    internal napi_callback_info CallbackInfo { get; }

    public JSCallbackArgs(JSValueScope scope, napi_callback_info callbackInfo)
    {
      Scope = scope;
      CallbackInfo = callbackInfo;
      unsafe
      {
        nuint argc = 0;
        napi_get_cb_info((napi_env)scope, callbackInfo, &argc, null, null, IntPtr.Zero).ThrowIfFailed();
        napi_value* argv = stackalloc napi_value[(int)argc];
        napi_value thisArg;
        IntPtr data;
        napi_get_cb_info((napi_env)scope, callbackInfo, &argc, argv, &thisArg, new IntPtr(&data)).ThrowIfFailed();

        _args = new JSValue[(int)argc];
        for (int i = 0; i < (int)argc; ++i)
        {
          _args[i] = argv[i];
        }

        ThisArg = thisArg;
        Data = data;
      }
    }
  }

  public delegate JSValue JSCallback(JSCallbackArgs args);

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

  public enum JSKeyCollectionMode : int
  {
    IncludePrototypes = 0,
    OwnOnly = 1,
  }

  [Flags]
  public enum JSKeyFilter : int
  {
    AllProperties = 0,
    Writable = 1,
    Enumerable = 2,
    Configurable = 4,
    SkipStrings = 8,
    SkipSymbols = 16,
  }

  public enum JSKeyConversion : int
  {
    KeepNumbers = 0,
    NumbersToStrings = 1,
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

  public class JSDeferred
  {
    private napi_deferred _handle;

    public JSDeferred(napi_deferred handle)
    {
      _handle = handle;
    }

    public void Resolve(JSValue resolution)
    {
      // _handle becomes invalid after this call
      napi_resolve_deferred((napi_env)resolution.Scope, _handle, (napi_value)resolution).ThrowIfFailed();
    }

    public void Reject(JSValue rejection)
    {
      // _handle becomes invalid after this call
      napi_resolve_deferred((napi_env)rejection.Scope, _handle, (napi_value)rejection).ThrowIfFailed();
    }
  }

  //TODO: implement JSValue array that can have a single Scope field for multiples napi_value

  // New class for JSValue
  public class JSValue
  {
    internal JSValueScope Scope { get; }

    internal napi_value Value { get; }

    public JSValue(JSValueScope scope, napi_value value)
    {
      Scope = scope;
      Value = value;
    }

    public JSValue(napi_value value)
    {
      Scope = JSValueScope.Current ?? throw new InvalidOperationException("No current scope");
      Value = value;
    }

    private static JSValueScope GetScope()
    {
      JSValueScope? scope = JSValueScope.Current;
      if (scope == null)
        throw new InvalidOperationException("Scope is null");
      return scope;
    }

    private static unsafe JSValue CreateJSValue(Func<napi_env, napi_value_ptr, napi_status> creator)
    {
      JSValueScope scope = GetScope();
      napi_value value;
      napi_value_ptr valuePtr = new napi_value_ptr { Pointer = new IntPtr(&value) };
      creator((napi_env)scope, valuePtr).ThrowIfFailed();
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

    public static unsafe JSValue CreateFunction(ReadOnlyMemory<byte> utf8Name,
      delegate* unmanaged[Cdecl]<napi_env, napi_callback_info, napi_value> callback, IntPtr data)
    {
      return CreateJSValue((env, result) =>
        napi_create_function(env, utf8Name.Pin().Pointer, (nuint)utf8Name.Length, callback, data, result));
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    private static unsafe napi_value InvokeJSCallback(napi_env env, napi_callback_info callbackInfo)
    {
      try
      {
        using (var scope = new JSEscapableValueScope(new JSEnvironment(env)))
        {
          JSCallbackArgs args = new JSCallbackArgs(scope, callbackInfo);
          JSCallback callback = (JSCallback)GCHandle.FromIntPtr(args.Data).Target!;
          JSValue result = callback(args);
          return (napi_value)scope.Escape(result);
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
    private static unsafe void FinalizeHandle(IntPtr env, IntPtr data, IntPtr hint)
    {
      GCHandle.FromIntPtr(data).Free();
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    private static unsafe void FinalizeHintHandle(IntPtr env, IntPtr data, IntPtr hint)
    {
      GCHandle.FromIntPtr(hint).Free();
    }

    public unsafe void AddHandleFinalizer(IntPtr handle)
    {
      if (handle != IntPtr.Zero)
      {
        napi_add_finalizer((napi_env)Scope, (napi_value)this, handle, &FinalizeHandle, IntPtr.Zero, null).ThrowIfFailed();
      }
    }

    public static unsafe JSValue CreateFunction(ReadOnlyMemory<byte> utf8Name, JSCallback callback)
    {
      GCHandle callbackHandle = GCHandle.Alloc(callback);
      JSValue func = CreateFunction(utf8Name, &InvokeJSCallback, (IntPtr)callbackHandle);
      func.AddHandleFinalizer((IntPtr)callbackHandle);
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
      napi_typeof((napi_env)Scope, Value, &result).ThrowIfFailed();
      return result;
    }

    public bool TryGetValue(out double value)
    {
      return napi_get_value_double((napi_env)Scope, Value, out value) == napi_status.napi_ok;
    }

    public bool TryGetValue(out int value)
    {
      return napi_get_value_int32((napi_env)Scope, Value, out value) == napi_status.napi_ok;
    }

    public bool TryGetValue(out uint value)
    {
      return napi_get_value_uint32((napi_env)Scope, Value, out value) == napi_status.napi_ok;
    }

    public bool TryGetValue(out long value)
    {
      return napi_get_value_int64((napi_env)Scope, Value, out value) == napi_status.napi_ok;
    }

    public bool TryGetValue(out bool value)
    {
      napi_status status = napi_get_value_bool((napi_env)Scope, Value, out sbyte result);
      value = result != 0;
      return status == napi_status.napi_ok;
    }

    public unsafe bool TryGetValue(out string value)
    {
      // TODO: add check that the object is still alive
      // TODO: should we check value type first?
      nuint length;
      if (napi_get_value_string_utf16((napi_env)Scope, Value, null, 0, &length) != napi_status.napi_ok)
      {
        value = string.Empty;
        return false;
      }

      char[] buf = new char[length + 1];
      fixed (char* bufStart = &buf[0])
      {
        napi_get_value_string_utf16((napi_env)Scope, Value, bufStart, (nuint)buf.Length, null).ThrowIfFailed();
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
      napi_coerce_to_bool((napi_env)Scope, (napi_value)this, out result).ThrowIfFailed();
      return result;
    }

    public JSValue CoerceToNumber()
    {
      napi_value result;
      napi_coerce_to_number((napi_env)Scope, (napi_value)this, out result).ThrowIfFailed();
      return result;
    }

    public JSValue CoerceToObject()
    {
      napi_value result;
      napi_coerce_to_object((napi_env)Scope, (napi_value)this, out result).ThrowIfFailed();
      return result;
    }

    public JSValue CoerceToString()
    {
      napi_value result;
      napi_coerce_to_string((napi_env)Scope, (napi_value)this, out result).ThrowIfFailed();
      return result;
    }

    public JSValue GetPrototype()
    {
      napi_value result;
      napi_get_prototype((napi_env)Scope, (napi_value)this, out result).ThrowIfFailed();
      return result;
    }

    public JSValue GetPropertyNames()
    {
      napi_value result;
      napi_get_property_names((napi_env)Scope, (napi_value)this, out result).ThrowIfFailed();
      return result;
    }

    public void SetProperty(JSValue key, JSValue value)
    {
      napi_set_property((napi_env)Scope, (napi_value)this, (napi_value)key, (napi_value)value).ThrowIfFailed();
    }

    public bool HasProperty(JSValue key)
    {
      napi_has_property((napi_env)Scope, (napi_value)this, (napi_value)key, out sbyte result).ThrowIfFailed();
      return result != 0;
    }

    public JSValue GetProperty(JSValue key)
    {
      napi_get_property((napi_env)Scope, (napi_value)this, (napi_value)key, out napi_value result).ThrowIfFailed();
      return result;
    }

    public bool DeleteProperty(JSValue key)
    {
      napi_delete_property((napi_env)Scope, (napi_value)this, (napi_value)key, out sbyte result).ThrowIfFailed();
      return result != 0;
    }

    public bool HasOwnProperty(JSValue key)
    {
      napi_has_own_property((napi_env)Scope, (napi_value)this, (napi_value)key, out sbyte result).ThrowIfFailed();
      return result != 0;
    }

    public void SetProperty(string name, JSValue value)
    {
      napi_set_named_property((napi_env)Scope, (napi_value)this, name, (napi_value)value).ThrowIfFailed();
    }

    public bool HasProperty(string name)
    {
      napi_has_named_property((napi_env)Scope, (napi_value)this, name, out sbyte result).ThrowIfFailed();
      return result != 0;
    }

    public JSValue GetProperty(string name)
    {
      napi_get_named_property((napi_env)Scope, (napi_value)this, name, out napi_value result).ThrowIfFailed();
      return result;
    }

    public void SetElement(int index, JSValue value)
    {
      napi_set_element((napi_env)Scope, (napi_value)this, (uint)index, (napi_value)value).ThrowIfFailed();
    }

    public bool HasElement(int index)
    {
      napi_has_element((napi_env)Scope, (napi_value)this, (uint)index, out sbyte result).ThrowIfFailed();
      return result != 0;
    }

    public JSValue GetElement(int index)
    {
      napi_get_element((napi_env)Scope, (napi_value)this, (uint)index, out napi_value result).ThrowIfFailed();
      return result;
    }

    public bool DeleteElement(int index)
    {
      napi_delete_element((napi_env)Scope, (napi_value)this, (uint)index, out sbyte result).ThrowIfFailed();
      return result != 0;
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    private static unsafe napi_value InvokeJSMethod(napi_env env, napi_callback_info callbackInfo)
    {
      try
      {
        using (var scope = new JSEscapableValueScope(new JSEnvironment(env)))
        {
          JSCallbackArgs args = new JSCallbackArgs(scope, callbackInfo);
          JSPropertyDescriptor desc = (JSPropertyDescriptor)GCHandle.FromIntPtr(args.Data).Target!;
          JSValue result = desc.Method!.Invoke(args);
          return (napi_value)scope.Escape(result);
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
    private static unsafe napi_value InvokeJSGetter(napi_env env, napi_callback_info callbackInfo)
    {
      try
      {
        using (var scope = new JSEscapableValueScope(new JSEnvironment(env)))
        {
          JSCallbackArgs args = new JSCallbackArgs(scope, callbackInfo);
          JSPropertyDescriptor desc = (JSPropertyDescriptor)GCHandle.FromIntPtr(args.Data).Target!;
          JSValue result = desc.Getter!.Invoke(args);
          return (napi_value)scope.Escape(result);
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
    private static unsafe napi_value InvokeJSSetter(napi_env env, napi_callback_info callbackInfo)
    {
      try
      {
        using (var scope = new JSEscapableValueScope(new JSEnvironment(env)))
        {
          JSCallbackArgs args = new JSCallbackArgs(scope, callbackInfo);
          JSPropertyDescriptor desc = (JSPropertyDescriptor)GCHandle.FromIntPtr(args.Data).Target!;
          JSValue result = desc.Setter!.Invoke(args);
          return (napi_value)scope.Escape(result);
        }
      }
      catch (System.Exception e)
      {
        //TODO: record as JS error
        Console.Error.WriteLine(e);
        throw;
      }
    }

    private unsafe delegate void UseUnmanagedDescriptors(nuint count, napi_property_descriptor* descriptors);

    private static unsafe IntPtr[] ToUnmanagedPropertyDescriptors(JSPropertyDescriptor[] descriptors, UseUnmanagedDescriptors action)
    {
      IntPtr[] handlesToFinalize = new IntPtr[descriptors.Length];
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
          handlesToFinalize[i] = descriptorPtr->data = (IntPtr)GCHandle.Alloc(descriptor);
        }
        else
        {
          handlesToFinalize[i] = descriptorPtr->data = IntPtr.Zero;
        }
      }
      action((nuint)count, descriptorsPtr);
      return handlesToFinalize;
    }

    public unsafe void DefineProperties(params JSPropertyDescriptor[] descriptors)
    {
      IntPtr[] handles = ToUnmanagedPropertyDescriptors(descriptors, (count, descriptorsPtr) =>
        napi_define_properties((napi_env)Scope, (napi_value)this, count, descriptorsPtr).ThrowIfFailed());
      Array.ForEach(handles, handle => AddHandleFinalizer(handle));
    }

    public bool IsArray()
    {
      napi_is_array((napi_env)Scope, (napi_value)this, out sbyte result).ThrowIfFailed();
      return result != 0;
    }

    public int GetLength()
    {
      napi_get_array_length((napi_env)Scope, (napi_value)this, out uint result).ThrowIfFailed();
      return (int)result;
    }

    public bool StrictEquals(JSValue other)
    {
      napi_strict_equals((napi_env)Scope, (napi_value)this, (napi_value)other, out sbyte result);
      return result != 0;
    }

    public unsafe JSValue Call()
    {
      napi_call_function((napi_env)Scope, (napi_value)GetUndefined(), (napi_value)this, 0, null, out napi_value result).ThrowIfFailed();
      return result;
    }

    public unsafe JSValue Call(JSValue thisArg)
    {
      napi_call_function((napi_env)Scope, (napi_value)thisArg, (napi_value)this, 0, null, out napi_value result).ThrowIfFailed();
      return result;
    }

    public unsafe JSValue Call(JSValue thisArg, JSValue arg0)
    {
      napi_value argValue0 = (napi_value)arg0;
      napi_call_function((napi_env)Scope, (napi_value)thisArg, (napi_value)this, 1, &argValue0, out napi_value result).ThrowIfFailed();
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
      napi_call_function((napi_env)Scope, (napi_value)thisArg, (napi_value)this,
        (nuint)argc, argv, out napi_value result).ThrowIfFailed();
      return result;
    }

    public unsafe JSValue CallAsConstructor()
    {
      napi_new_instance((napi_env)Scope, (napi_value)this, 0, null, out napi_value result).ThrowIfFailed();
      return result;
    }

    public unsafe JSValue CallAsConstructor(JSValue arg0)
    {
      napi_value argValue0 = (napi_value)arg0;
      napi_new_instance((napi_env)Scope, (napi_value)this, 1, &argValue0, out napi_value result).ThrowIfFailed();
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
      napi_new_instance((napi_env)Scope, (napi_value)this, (nuint)argc, argv, out napi_value result).ThrowIfFailed();
      return result;
    }

    public bool InstanceOf(JSValue constructor)
    {
      napi_instanceof((napi_env)Scope, (napi_value)this, (napi_value)constructor, out sbyte result).ThrowIfFailed();
      return result != 0;
    }

    public static unsafe JSValue DefineClass(
      ReadOnlyMemory<byte> utf8Name,
      delegate* unmanaged[Cdecl]<napi_env, napi_callback_info, napi_value> callback,
      IntPtr data,
      nuint count,
      napi_property_descriptor* descriptors)
    {
      napi_define_class((napi_env)JSValueScope.Current, utf8Name.Pin().Pointer, (nuint)utf8Name.Length,
        callback, data, count, descriptors, out napi_value result).ThrowIfFailed();
      return result;
    }

    public static unsafe JSValue DefineClass(ReadOnlyMemory<byte> utf8Name, JSCallback callback, params JSPropertyDescriptor[] descriptors)
    {
      GCHandle callbackHandle = GCHandle.Alloc(callback);
      JSValue? func = null;
      IntPtr[] handles = ToUnmanagedPropertyDescriptors(descriptors, (count, descriptorsPtr) =>
      {
        func = DefineClass(utf8Name, &InvokeJSCallback, (IntPtr)callbackHandle, count, descriptorsPtr);
      });
      Array.ForEach(handles, handle => func!.AddHandleFinalizer(handle));
      return func!;
    }

    public static unsafe JSValue DefineClass(string name, JSCallback callback, params JSPropertyDescriptor[] descriptors)
    {
      return DefineClass(Encoding.UTF8.GetBytes(name), callback, descriptors);
    }

    public unsafe void Wrap(object value)
    {
      GCHandle valueHandle = GCHandle.Alloc(value);
      napi_wrap((napi_env)Scope, (napi_value)this, (IntPtr)valueHandle, &FinalizeHandle, IntPtr.Zero, null).ThrowIfFailed();
    }

    public object Unwrap()
    {
      napi_unwrap((napi_env)Scope, (napi_value)this, out IntPtr result).ThrowIfFailed();
      GCHandle handle = GCHandle.FromIntPtr(result);
      return handle.Target!;
    }

    public object RemoveWrap()
    {
      napi_remove_wrap((napi_env)Scope, (napi_value)this, out IntPtr result).ThrowIfFailed();
      return GCHandle.FromIntPtr(result).Target!;
    }

    public static unsafe JSValue CreateExternal(object value)
    {
      GCHandle valueHandle = GCHandle.Alloc(value);
      napi_create_external((napi_env)JSValueScope.Current, (IntPtr)valueHandle, &FinalizeHandle, IntPtr.Zero, out napi_value result).ThrowIfFailed();
      return result;
    }

    public unsafe object GetExternalValue()
    {
      napi_get_value_external((napi_env)Scope, (napi_value)this, out IntPtr result).ThrowIfFailed();
      return GCHandle.FromIntPtr(result).Target!;
    }

    public JSReference CreateReference()
    {
      return new JSReference(Scope.Environment, this);
    }

    public JSReference CreateWeakReference()
    {
      return new JSReference(Scope.Environment, this, isWeak: true);
    }

    public void Throw()
    {
      napi_throw((napi_env)Scope, (napi_value)this).ThrowIfFailed();
    }

    public static void ThrowError(string code, string message)
    {
      napi_throw_error((napi_env)JSValueScope.Current, code, message).ThrowIfFailed();
    }

    public static void ThrowTypeError(string code, string message)
    {
      napi_throw_type_error((napi_env)JSValueScope.Current, code, message).ThrowIfFailed();
    }

    public static void ThrowRangeError(string code, string message)
    {
      napi_throw_range_error((napi_env)JSValueScope.Current, code, message).ThrowIfFailed();
    }

    public bool IsError()
    {
      napi_is_error((napi_env)Scope, (napi_value)this, out sbyte result).ThrowIfFailed();
      return result != 0;
    }

    public static bool IsExceptionPending()
    {
      napi_is_exception_pending((napi_env)JSValueScope.Current, out sbyte result).ThrowIfFailed();
      return result != 0;
    }

    public static JSValue GetAndClearLastException()
    {
      napi_get_and_clear_last_exception((napi_env)JSValueScope.Current, out napi_value result).ThrowIfFailed();
      return result;
    }

    public bool IsArrayBuffer()
    {
      napi_is_arraybuffer((napi_env)Scope, (napi_value)this, out sbyte result).ThrowIfFailed();
      return result != 0;
    }

    public static unsafe JSValue CreateArrayBuffer(ReadOnlySpan<byte> data)
    {
      napi_create_arraybuffer((napi_env)JSValueScope.Current, (nuint)data.Length, out void* buffer, out napi_value result).ThrowIfFailed();
      data.CopyTo(new Span<byte>(buffer, data.Length));
      return result;
    }

    private class PinnedReadOnlyMemory : IDisposable
    {
      private bool _disposedValue = false;
      private object? _owner;
      private ReadOnlyMemory<byte> _memory;
      private MemoryHandle _memoryHandle;

      public PinnedReadOnlyMemory(object? owner, ReadOnlyMemory<byte> memory)
      {
        _owner = owner;
        _memory = memory;
        _memoryHandle = _memory.Pin();
      }

      public unsafe void* Pointer => _memoryHandle.Pointer;

      public int Length => _memory.Length;

      protected virtual void Dispose(bool disposing)
      {
        if (!_disposedValue)
        {
          if (disposing)
          {
            _memoryHandle.Dispose();
          }

          _owner = null;
          _disposedValue = true;
        }
      }

      public void Dispose()
      {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
      }
    }

    public static unsafe JSValue CreateExternalArrayBuffer(object external, ReadOnlyMemory<byte> memory)
    {
      PinnedReadOnlyMemory pinnedMemory = new PinnedReadOnlyMemory(external, memory);
      napi_create_external_arraybuffer(
        (napi_env)JSValueScope.Current,
        pinnedMemory.Pointer,
        (nuint)pinnedMemory.Length,
        &FinalizeHintHandle, // We pass object to finalize as a hint parameter
        (IntPtr)GCHandle.Alloc(pinnedMemory),
        out napi_value result).ThrowIfFailed();
      return result;
    }

    public unsafe Span<byte> GetArrayBufferInfo()
    {
      napi_get_arraybuffer_info((napi_env)JSValueScope.Current, (napi_value)this, out void* data, out nuint length).ThrowIfFailed();
      return new Span<byte>(data, (int)length);
    }

    public bool IsTypedArray()
    {
      napi_is_typedarray((napi_env)Scope, (napi_value)this, out sbyte result).ThrowIfFailed();
      return result != 0;
    }

    public static JSValue CreateTypedArray(JSTypedArrayType type, int length, JSValue arrayBuffer, int byteOffset)
    {
      napi_create_typedarray(
        (napi_env)JSValueScope.Current,
        (napi_typedarray_type)(int)type,
        (nuint)length,
        (napi_value)arrayBuffer,
        (nuint)byteOffset,
        out napi_value result).ThrowIfFailed();
      return result;
    }

    public unsafe void GetTypedArrayInfo(
      out JSTypedArrayType type,
      out int length,
      out void* data,
      out JSValue arrayBuffer,
      out int byteOffset)
    {
      napi_get_typedarray_info(
        (napi_env)Scope,
        (napi_value)this,
        out napi_typedarray_type type_,
        out nuint length_,
        out data,
        out napi_value arrayBuffer_,
        out nuint byteOffset_).ThrowIfFailed();
      type = (JSTypedArrayType)(int)type_;
      length = (int)length_;
      arrayBuffer = arrayBuffer_;
      byteOffset = (int)byteOffset_;
    }

    public static JSValue CreateDataView(int length, JSValue arrayBuffer, int byteOffset)
    {
      napi_create_dataview((napi_env)JSValueScope.Current, (nuint)length, (napi_value)arrayBuffer, (nuint)byteOffset, out napi_value result).ThrowIfFailed();
      return result;
    }

    public bool IsDataView()
    {
      napi_is_dataview((napi_env)Scope, (napi_value)this, out sbyte result).ThrowIfFailed();
      return result != 0;
    }

    public unsafe void GetDataViewInfo(out ReadOnlySpan<byte> viewSpan, out JSValue arrayBuffer, out int byteOffset)
    {
      napi_get_dataview_info(
        (napi_env)Scope,
        (napi_value)this,
        out nuint byteLength,
        out void* data,
        out napi_value arrayBuffer_,
        out nuint byteOffset_).ThrowIfFailed();
      viewSpan = new ReadOnlySpan<byte>(data, (int)byteLength);
      arrayBuffer = arrayBuffer_;
      byteOffset = (int)byteOffset_;
    }

    public static uint GetVersion()
    {
      napi_get_version((napi_env)JSValueScope.Current, out uint result).ThrowIfFailed();
      return result;
    }

    public static JSValue CreatePromise(out JSDeferred deferred)
    {
      napi_create_promise((napi_env)JSValueScope.Current, out napi_deferred deferred_, out napi_value promise).ThrowIfFailed();
      deferred = new JSDeferred(deferred_);
      return promise;
    }

    public bool IsPromise()
    {
      napi_is_promise((napi_env)Scope, (napi_value)this, out sbyte result).ThrowIfFailed();
      return result != 0;
    }

    public JSValue RunScript()
    {
      napi_run_script((napi_env)Scope, (napi_value)this, out napi_value result).ThrowIfFailed();
      return result;
    }

    public static JSValue CreateDate(double time)
    {
      napi_create_date((napi_env)JSValueScope.Current, time, out napi_value result).ThrowIfFailed();
      return result;
    }

    public bool IsDate()
    {
      napi_is_date((napi_env)Scope, (napi_value)this, out sbyte result).ThrowIfFailed();
      return result != 0;
    }

    public double GetDateValue()
    {
      napi_get_date_value((napi_env)Scope, (napi_value)this, out double result).ThrowIfFailed();
      return result;
    }

    public static JSValue CreateBigInt(long value)
    {
      napi_create_bigint_int64((napi_env)JSValueScope.Current, value, out napi_value result).ThrowIfFailed();
      return result;
    }

    public static JSValue CreateBigInt(ulong value)
    {
      napi_create_bigint_uint64((napi_env)JSValueScope.Current, value, out napi_value result).ThrowIfFailed();
      return result;
    }

    public static unsafe JSValue CreateBigInt(int signBit, ReadOnlyMemory<ulong> words)
    {
      napi_create_bigint_words((napi_env)JSValueScope.Current, signBit, (nuint)words.Length, (ulong*)words.Pin().Pointer, out napi_value result).ThrowIfFailed();
      return result;
    }

    public long GetValueBigIntInt64(out bool isLossless)
    {
      napi_get_value_bigint_int64((napi_env)Scope, (napi_value)this, out long result, out sbyte lossless).ThrowIfFailed();
      isLossless = lossless != 0;
      return result;
    }

    public ulong GetValueBigIntUInt64(out bool isLossless)
    {
      napi_get_value_bigint_uint64((napi_env)Scope, (napi_value)this, out ulong result, out sbyte lossless).ThrowIfFailed();
      isLossless = lossless != 0;
      return result;
    }

    public unsafe ulong[] GetValueBigIntWords(out int signBit)
    {
      napi_get_value_bigint_words((napi_env)Scope, (napi_value)this, out signBit, out nuint wordCount, null).ThrowIfFailed();
      ulong[] words = new ulong[wordCount];
      fixed (ulong* wordsPtr = &words[0])
      {
        napi_get_value_bigint_words((napi_env)Scope, (napi_value)this, out signBit, out wordCount, wordsPtr).ThrowIfFailed();
      }
      return words;
    }

    public JSValue GetAllPropertyNames(JSKeyCollectionMode mode, JSKeyFilter filter, JSKeyConversion conversion)
    {
      napi_get_all_property_names(
        (napi_env)Scope,
        (napi_value)this,
        (napi_key_collection_mode)mode,
        (napi_key_filter)filter,
        (napi_key_conversion)conversion,
        out napi_value result).ThrowIfFailed();
      return result;
    }

    public static unsafe void SetInstanceData(object? data)
    {
      napi_get_instance_data((napi_env)JSValueScope.Current, out IntPtr handlePtr).ThrowIfFailed();
      if (handlePtr != IntPtr.Zero)
      {
        GCHandle.FromIntPtr(handlePtr).Free();
      }

      if (data != null)
      {
        GCHandle handle = GCHandle.Alloc(data);
        napi_set_instance_data(
          (napi_env)JSValueScope.Current,
          (IntPtr)handle,
          &FinalizeHandle,
          IntPtr.Zero).ThrowIfFailed();
      }
    }

    public static object? GetInstanceData()
    {
      napi_get_instance_data((napi_env)JSValueScope.Current, out IntPtr data).ThrowIfFailed();
      return (data != IntPtr.Zero) ? GCHandle.FromIntPtr(data).Target : null;
    }

    public void DetachArrayBuffer()
    {
      napi_detach_arraybuffer((napi_env)Scope, (napi_value)this).ThrowIfFailed();
    }

    public bool IsDetachedArrayBuffer()
    {
      napi_is_detached_arraybuffer((napi_env)Scope, (napi_value)this, out sbyte result).ThrowIfFailed();
      return result != 0;
    }

    public void SetObjectTypeTag(ref napi_type_tag typeTag)
    {
      napi_type_tag_object((napi_env)Scope, (napi_value)this, ref typeTag);
    }

    public bool CheckObjectTypeTag(ref napi_type_tag typeTag)
    {
      napi_check_object_type_tag((napi_env)Scope, (napi_value)this, ref typeTag, out sbyte result);
      return result != 0;
    }

    public void FreezeObject()
    {
      napi_object_freeze((napi_env)Scope, (napi_value)this).ThrowIfFailed();
    }

    public void SealObject()
    {
      napi_object_seal((napi_env)Scope, (napi_value)this).ThrowIfFailed();
    }
  }
}
