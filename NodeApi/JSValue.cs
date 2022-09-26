using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using static NodeApi.JSNative.Interop;

namespace NodeApi;

public struct JSValue
{
  private JSValueScope _scope;
  private napi_value _handle;

  public JSValueScope Scope => _scope;

  public JSValue(JSValueScope scope, napi_value handle)
  {
    _scope = scope;
    _handle = handle;
  }

  public JSValue(napi_value handle)
  {
    _scope = JSValueScope.Current ?? throw new InvalidOperationException("No current scope");
    _handle = handle;
  }

  public napi_value GetCheckedHandle()
  {
    if (_scope.IsInvalid)
    {
      throw new InvalidOperationException("The value handle is invalid because its scope is closed");
    }
    return _handle;
  }

  public static JSValue GetUndefined()
  {
    napi_get_undefined((napi_env)JSValueScope.Current, out napi_value result).ThrowIfFailed();
    return result;
  }

  public static JSValue GetNull()
  {
    napi_get_null((napi_env)JSValueScope.Current, out napi_value result).ThrowIfFailed();
    return result;
  }

  public static JSValue GetGlobal()
  {
    napi_get_global((napi_env)JSValueScope.Current, out napi_value result).ThrowIfFailed();
    return result;
  }

  public static JSValue GetBoolean(bool value)
  {
    napi_get_boolean((napi_env)JSValueScope.Current, (byte)(value ? 1 : 0), out napi_value result).ThrowIfFailed();
    return result;
  }

  public static JSValue CreateObject()
  {
    napi_create_object((napi_env)JSValueScope.Current, out napi_value result).ThrowIfFailed();
    return result;
  }

  public static JSValue CreateArray()
  {
    napi_create_array((napi_env)JSValueScope.Current, out napi_value result).ThrowIfFailed();
    return result;
  }

  public static JSValue CreateArray(int length)
  {
    napi_create_array_with_length((napi_env)JSValueScope.Current, (nuint)length, out napi_value result).ThrowIfFailed();
    return result;
  }

  public static JSValue CreateNumber(double value)
  {
    napi_create_double((napi_env)JSValueScope.Current, value, out napi_value result).ThrowIfFailed();
    return result;
  }

  public static JSValue CreateNumber(int value)
  {
    napi_create_int32((napi_env)JSValueScope.Current, value, out napi_value result).ThrowIfFailed();
    return result;
  }

  public static JSValue CreateNumber(uint value)
  {
    napi_create_uint32((napi_env)JSValueScope.Current, value, out napi_value result).ThrowIfFailed();
    return result;
  }

  public static JSValue CreateNumber(long value)
  {
    napi_create_int64((napi_env)JSValueScope.Current, value, out napi_value result).ThrowIfFailed();
    return result;
  }

  public static unsafe JSValue CreateStringLatin1(ReadOnlyMemory<byte> value)
  {
    napi_create_string_latin1((napi_env)JSValueScope.Current, value.Pin().Pointer, (nuint)value.Length, out napi_value result).ThrowIfFailed();
    return result;
  }

  public static unsafe JSValue CreateStringUtf8(ReadOnlyMemory<byte> value)
  {
    napi_create_string_utf8((napi_env)JSValueScope.Current, value.Pin().Pointer, (nuint)value.Length, out napi_value result).ThrowIfFailed();
    return result;
  }

  public static unsafe JSValue CreateStringUtf16(ReadOnlyMemory<char> value)
  {
    napi_create_string_utf16((napi_env)JSValueScope.Current, value.Pin().Pointer, (nuint)value.Length, out napi_value result).ThrowIfFailed();
    return result;
  }

  public static JSValue CreateStringUtf16(string value)
  {
    return CreateStringUtf16(value.AsMemory());
  }

  public static JSValue CreateSymbol(JSValue description)
  {
    napi_create_symbol((napi_env)JSValueScope.Current, (napi_value)description, out napi_value result).ThrowIfFailed();
    return result;
  }

  public static JSValue CreateSymbol(string description)
  {
    napi_create_symbol((napi_env)JSValueScope.Current, (napi_value)CreateStringUtf16(description), out napi_value result).ThrowIfFailed(); ;
    return result;
  }

  public static unsafe JSValue CreateFunction(ReadOnlyMemory<byte> utf8Name,
    delegate* unmanaged[Cdecl]<napi_env, napi_callback_info, napi_value> callback, IntPtr data)
  {
    napi_create_function((napi_env)JSValueScope.Current, utf8Name.Pin().Pointer, (nuint)utf8Name.Length, callback, data, out napi_value result).ThrowIfFailed();
    return result;
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
    napi_create_error((napi_env)JSValueScope.Current, (napi_value)code, (napi_value)message, out napi_value result).ThrowIfFailed();
    return result;
  }

  public static JSValue CreateTypeError(JSValue code, JSValue message)
  {
    napi_create_type_error((napi_env)JSValueScope.Current, (napi_value)code, (napi_value)message, out napi_value result).ThrowIfFailed();
    return result;
  }

  public static JSValue CreateRangeError(JSValue code, JSValue message)
  {
    napi_create_range_error((napi_env)JSValueScope.Current, (napi_value)code, (napi_value)message, out napi_value result).ThrowIfFailed();
    return result;
  }

  public unsafe JSValueType TypeOf()
  {
    napi_typeof((napi_env)Scope, (napi_value)this, out napi_valuetype result).ThrowIfFailed();
    return (JSValueType)result;
  }

  public bool TryGetValue(out double value)
  {
    return napi_get_value_double((napi_env)Scope, (napi_value)this, out value) == napi_status.napi_ok;
  }

  public bool TryGetValue(out int value)
  {
    return napi_get_value_int32((napi_env)Scope, (napi_value)this, out value) == napi_status.napi_ok;
  }

  public bool TryGetValue(out uint value)
  {
    return napi_get_value_uint32((napi_env)Scope, (napi_value)this, out value) == napi_status.napi_ok;
  }

  public bool TryGetValue(out long value)
  {
    return napi_get_value_int64((napi_env)Scope, (napi_value)this, out value) == napi_status.napi_ok;
  }

  public bool TryGetValue(out bool value)
  {
    napi_status status = napi_get_value_bool((napi_env)Scope, (napi_value)this, out byte result);
    value = result != 0;
    return status == napi_status.napi_ok;
  }

  public unsafe bool TryGetValue(out string value)
  {
    // TODO: add check that the object is still alive
    // TODO: should we check value type first?
    nuint length;
    if (napi_get_value_string_utf16((napi_env)Scope, (napi_value)this, null, 0, &length) != napi_status.napi_ok)
    {
      value = string.Empty;
      return false;
    }

    char[] buf = new char[length + 1];
    fixed (char* bufStart = &buf[0])
    {
      napi_get_value_string_utf16((napi_env)Scope, (napi_value)this, bufStart, (nuint)buf.Length, null).ThrowIfFailed();
      value = new string(buf);
      return true;
    }
  }

  //TODO: add more string functions

  public static implicit operator JSValue(bool value) => GetBoolean(value);
  public static implicit operator JSValue(sbyte value) => CreateNumber(value);
  public static implicit operator JSValue(byte value) => CreateNumber(value);
  public static implicit operator JSValue(short value) => CreateNumber(value);
  public static implicit operator JSValue(ushort value) => CreateNumber(value);
  public static implicit operator JSValue(int value) => CreateNumber(value);
  public static implicit operator JSValue(uint value) => CreateNumber(value);
  public static implicit operator JSValue(long value) => CreateNumber(value);
  public static implicit operator JSValue(ulong value) => CreateNumber(value);
  public static implicit operator JSValue(float value) => CreateNumber(value);
  public static implicit operator JSValue(double value) => CreateNumber(value);
  public static implicit operator JSValue(string value) => CreateStringUtf16(value);
  public static implicit operator JSValue(JSCallback callback) => CreateFunction("Unknown", callback);

  public static explicit operator bool(JSValue value)
  {
    if (!value.TryGetValue(out bool result))
      throw new InvalidOperationException("Cannot get bool value");
    return result;
  }

  public static explicit operator sbyte(JSValue value)
  {
    if (!value.TryGetValue(out int result))
      throw new InvalidOperationException("Cannot get int value");
    return (sbyte)result;
  }

  public static explicit operator byte(JSValue value)
  {
    if (!value.TryGetValue(out uint result))
      throw new InvalidOperationException("Cannot get int value");
    return (byte)result;
  }

  public static explicit operator short(JSValue value)
  {
    if (!value.TryGetValue(out int result))
      throw new InvalidOperationException("Cannot get int value");
    return (short)result;
  }

  public static explicit operator ushort(JSValue value)
  {
    if (!value.TryGetValue(out uint result))
      throw new InvalidOperationException("Cannot get int value");
    return (ushort)result;
  }

  public static explicit operator int(JSValue value)
  {
    if (!value.TryGetValue(out int result))
      throw new InvalidOperationException("Cannot get int value");
    return result;
  }

  public static explicit operator uint(JSValue value)
  {
    if (!value.TryGetValue(out uint result))
      throw new InvalidOperationException("Cannot get int value");
    return result;
  }

  public static explicit operator long(JSValue value)
  {
    if (!value.TryGetValue(out long result))
      throw new InvalidOperationException("Cannot get int value");
    return result;
  }

  public static explicit operator ulong(JSValue value)
  {
    if (!value.TryGetValue(out long result))
      throw new InvalidOperationException("Cannot get int value");
    return (ulong)result;
  }

  public static explicit operator float(JSValue value)
  {
    if (!value.TryGetValue(out double result))
      throw new InvalidOperationException("Cannot get int value");
    return (float)result;
  }

  public static explicit operator double(JSValue value)
  {
    if (!value.TryGetValue(out double result))
      throw new InvalidOperationException("Cannot get int value");
    return result;
  }

  public static explicit operator string(JSValue value)
  {
    if (!value.TryGetValue(out string result))
      throw new InvalidOperationException("Cannot get int value");
    return result;
  }

  public JSValue this[string name]
  {
    get { return GetProperty(name); }
    set { SetProperty(name, value); }
  }

  public static explicit operator napi_value(JSValue value) => value.GetCheckedHandle();

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
    napi_has_property((napi_env)Scope, (napi_value)this, (napi_value)key, out byte result).ThrowIfFailed();
    return result != 0;
  }

  public JSValue GetProperty(JSValue key)
  {
    napi_get_property((napi_env)Scope, (napi_value)this, (napi_value)key, out napi_value result).ThrowIfFailed();
    return result;
  }

  public bool DeleteProperty(JSValue key)
  {
    napi_delete_property((napi_env)Scope, (napi_value)this, (napi_value)key, out byte result).ThrowIfFailed();
    return result != 0;
  }

  public bool HasOwnProperty(JSValue key)
  {
    napi_has_own_property((napi_env)Scope, (napi_value)this, (napi_value)key, out byte result).ThrowIfFailed();
    return result != 0;
  }

  public void SetProperty(string name, JSValue value)
  {
    napi_set_named_property((napi_env)Scope, (napi_value)this, name, (napi_value)value).ThrowIfFailed();
  }

  public bool HasProperty(string name)
  {
    napi_has_named_property((napi_env)Scope, (napi_value)this, name, out byte result).ThrowIfFailed();
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
    napi_has_element((napi_env)Scope, (napi_value)this, (uint)index, out byte result).ThrowIfFailed();
    return result != 0;
  }

  public JSValue GetElement(int index)
  {
    napi_get_element((napi_env)Scope, (napi_value)this, (uint)index, out napi_value result).ThrowIfFailed();
    return result;
  }

  public bool DeleteElement(int index)
  {
    napi_delete_element((napi_env)Scope, (napi_value)this, (uint)index, out byte result).ThrowIfFailed();
    return result != 0;
  }

  [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
  private static unsafe napi_value InvokeJSMethod(napi_env env, napi_callback_info callbackInfo)
  {
    using JSRootValueScope scope = new(new JSEnvironment(env));
    JSCallbackArgs args = new JSCallbackArgs(scope, callbackInfo);
    JSPropertyDescriptor desc = (JSPropertyDescriptor)GCHandle.FromIntPtr(args.Data).Target!;
    return (napi_value)desc.Method!.Invoke(args);
  }

  [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
  private static unsafe napi_value InvokeJSGetter(napi_env env, napi_callback_info callbackInfo)
  {
    using JSRootValueScope scope = new(new JSEnvironment(env));
    JSCallbackArgs args = new JSCallbackArgs(scope, callbackInfo);
    JSPropertyDescriptor desc = (JSPropertyDescriptor)GCHandle.FromIntPtr(args.Data).Target!;
    return (napi_value)desc.Getter!.Invoke(args);
  }

  [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
  private static unsafe napi_value InvokeJSSetter(napi_env env, napi_callback_info callbackInfo)
  {
    using JSRootValueScope scope = new(new JSEnvironment(env));
    JSCallbackArgs args = new(scope, callbackInfo);
    JSPropertyDescriptor desc = (JSPropertyDescriptor)GCHandle.FromIntPtr(args.Data).Target!;
    return (napi_value)desc.Setter!.Invoke(args);
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
    JSValue self = this;
    IntPtr[] handles = ToUnmanagedPropertyDescriptors(descriptors, (count, descriptorsPtr) =>
      napi_define_properties((napi_env)self.Scope, (napi_value)self, count, descriptorsPtr).ThrowIfFailed());
    Array.ForEach(handles, handle => self.AddHandleFinalizer(handle));
  }

  public bool IsArray()
  {
    napi_is_array((napi_env)Scope, (napi_value)this, out byte result).ThrowIfFailed();
    return result != 0;
  }

  public int GetLength()
  {
    napi_get_array_length((napi_env)Scope, (napi_value)this, out uint result).ThrowIfFailed();
    return (int)result;
  }

  public bool StrictEquals(JSValue other)
  {
    napi_strict_equals((napi_env)Scope, (napi_value)this, (napi_value)other, out byte result);
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
    napi_instanceof((napi_env)Scope, (napi_value)this, (napi_value)constructor, out byte result).ThrowIfFailed();
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
    Array.ForEach(handles, handle => func!.Value.AddHandleFinalizer(handle));
    return func!.Value;
  }

  public static unsafe JSValue DefineClass(string name, JSCallback callback, params JSPropertyDescriptor[] descriptors)
  {
    return DefineClass(Encoding.UTF8.GetBytes(name), callback, descriptors);
  }

  public unsafe JSValue Wrap(object value)
  {
    GCHandle valueHandle = GCHandle.Alloc(value);
    napi_wrap((napi_env)Scope, (napi_value)this, (IntPtr)valueHandle, &FinalizeHandle, IntPtr.Zero, null).ThrowIfFailed();
    return this;
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
    napi_is_error((napi_env)Scope, (napi_value)this, out byte result).ThrowIfFailed();
    return result != 0;
  }

  public static bool IsExceptionPending()
  {
    napi_is_exception_pending((napi_env)JSValueScope.Current, out byte result).ThrowIfFailed();
    return result != 0;
  }

  public static JSValue GetAndClearLastException()
  {
    napi_get_and_clear_last_exception((napi_env)JSValueScope.Current, out napi_value result).ThrowIfFailed();
    return result;
  }

  public bool IsArrayBuffer()
  {
    napi_is_arraybuffer((napi_env)Scope, (napi_value)this, out byte result).ThrowIfFailed();
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
    napi_is_typedarray((napi_env)Scope, (napi_value)this, out byte result).ThrowIfFailed();
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
    napi_is_dataview((napi_env)Scope, (napi_value)this, out byte result).ThrowIfFailed();
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
    napi_is_promise((napi_env)Scope, (napi_value)this, out byte result).ThrowIfFailed();
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
    napi_is_date((napi_env)Scope, (napi_value)this, out byte result).ThrowIfFailed();
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
    napi_get_value_bigint_int64((napi_env)Scope, (napi_value)this, out long result, out byte lossless).ThrowIfFailed();
    isLossless = lossless != 0;
    return result;
  }

  public ulong GetValueBigIntUInt64(out bool isLossless)
  {
    napi_get_value_bigint_uint64((napi_env)Scope, (napi_value)this, out ulong result, out byte lossless).ThrowIfFailed();
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
    napi_is_detached_arraybuffer((napi_env)Scope, (napi_value)this, out byte result).ThrowIfFailed();
    return result != 0;
  }

  public void SetObjectTypeTag(ref napi_type_tag typeTag)
  {
    napi_type_tag_object((napi_env)Scope, (napi_value)this, ref typeTag);
  }

  public bool CheckObjectTypeTag(ref napi_type_tag typeTag)
  {
    napi_check_object_type_tag((napi_env)Scope, (napi_value)this, ref typeTag, out byte result);
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
