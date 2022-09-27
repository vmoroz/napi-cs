﻿using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using static NodeApi.JSNative.Interop;

namespace NodeApi;

// Node API managed wrappers
public static partial class JSNative
{
  public static void ThrowIfFailed(this napi_status status)
  {
    if (status != napi_status.napi_ok)
    {
      throw new JSException(status);
    }
  }

  public static unsafe JSErrorInfo GetLastErrorInfo()
  {
    napi_get_last_error_info(Env, out napi_extended_error_info* errorInfo).ThrowIfFailed();
    if (errorInfo->error_message != null)
    {
      string message = Encoding.UTF8.GetString(MemoryMarshal.CreateReadOnlySpanFromNullTerminated(errorInfo->error_message));
      return new JSErrorInfo(message, errorInfo->error_code);
    }
    return new JSErrorInfo("Error", errorInfo->error_code);
  }
  public static JSValue GetUndefined()
  {
    napi_get_undefined(Env, out napi_value result).ThrowIfFailed();
    return result;
  }

  public static JSValue GetNull()
  {
    napi_get_null(Env, out napi_value result).ThrowIfFailed();
    return result;
  }

  public static JSValue GetGlobal()
  {
    napi_get_global(Env, out napi_value result).ThrowIfFailed();
    return result;
  }

  public static JSValue GetBoolean(bool value)
  {
    napi_get_boolean(Env, (byte)(value ? 1 : 0), out napi_value result).ThrowIfFailed();
    return result;
  }

  public static JSValue CreateObject()
  {
    napi_create_object(Env, out napi_value result).ThrowIfFailed();
    return result;
  }

  public static JSValue CreateArray()
  {
    napi_create_array(Env, out napi_value result).ThrowIfFailed();
    return result;
  }

  public static JSValue CreateArray(int length)
  {
    napi_create_array_with_length(Env, (nuint)length, out napi_value result).ThrowIfFailed();
    return result;
  }

  public static JSValue CreateNumber(double value)
  {
    napi_create_double(Env, value, out napi_value result).ThrowIfFailed();
    return result;
  }

  public static JSValue CreateNumber(int value)
  {
    napi_create_int32(Env, value, out napi_value result).ThrowIfFailed();
    return result;
  }

  public static JSValue CreateNumber(uint value)
  {
    napi_create_uint32(Env, value, out napi_value result).ThrowIfFailed();
    return result;
  }

  public static JSValue CreateNumber(long value)
  {
    napi_create_int64(Env, value, out napi_value result).ThrowIfFailed();
    return result;
  }

  public static unsafe JSValue CreateStringLatin1(ReadOnlyMemory<byte> value)
  {
    napi_create_string_latin1(Env, (byte*)value.Pin().Pointer, (nuint)value.Length, out napi_value result).ThrowIfFailed();
    return result;
  }

  public static unsafe JSValue CreateStringUtf8(ReadOnlyMemory<byte> value)
  {
    napi_create_string_utf8(Env, (byte*)value.Pin().Pointer, (nuint)value.Length, out napi_value result).ThrowIfFailed();
    return result;
  }

  public static unsafe JSValue CreateStringUtf16(ReadOnlyMemory<char> value)
  {
    napi_create_string_utf16(Env, (char*)value.Pin().Pointer, (nuint)value.Length, out napi_value result).ThrowIfFailed();
    return result;
  }

  public static JSValue CreateStringUtf16(string value)
  {
    return CreateStringUtf16(value.AsMemory());
  }

  public static JSValue CreateSymbol(JSValue description)
  {
    napi_create_symbol(Env, (napi_value)description, out napi_value result).ThrowIfFailed();
    return result;
  }

  public static unsafe JSValue CreateFunction(
    ReadOnlyMemory<byte> utf8Name,
    delegate* unmanaged[Cdecl]<napi_env, napi_callback_info, napi_value> callback,
    IntPtr data)
  {
    napi_create_function(Env, (byte*)utf8Name.Pin().Pointer, (nuint)utf8Name.Length,
      callback, data, out napi_value result).ThrowIfFailed();
    return result;
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

  public static unsafe void AddHandleFinalizer(this JSValue thisValue, IntPtr handle)
  {
    if (handle != IntPtr.Zero)
    {
      napi_add_finalizer(Env, (napi_value)thisValue, handle, &FinalizeHandle, IntPtr.Zero, null).ThrowIfFailed();
    }
  }

  public static JSValue CreateError(JSValue code, JSValue message)
  {
    napi_create_error(Env, (napi_value)code, (napi_value)message, out napi_value result).ThrowIfFailed();
    return result;
  }

  public static JSValue CreateTypeError(JSValue code, JSValue message)
  {
    napi_create_type_error(Env, (napi_value)code, (napi_value)message, out napi_value result).ThrowIfFailed();
    return result;
  }

  public static JSValue CreateRangeError(JSValue code, JSValue message)
  {
    napi_create_range_error(Env, (napi_value)code, (napi_value)message, out napi_value result).ThrowIfFailed();
    return result;
  }
  public static unsafe JSValueType TypeOf(this JSValue value)
  {
    napi_typeof(Env, (napi_value)value, out napi_valuetype result).ThrowIfFailed();
    return (JSValueType)result;
  }

  public static double GetValueDouble(this JSValue value)
  {
    napi_get_value_double(Env, (napi_value)value, out double result).ThrowIfFailed();
    return result;
  }

  public static int GetValueInt32(this JSValue value)
  {
    napi_get_value_int32(Env, (napi_value)value, out int result).ThrowIfFailed();
    return result;
  }

  public static uint GetValueUInt32(this JSValue value)
  {
    napi_get_value_uint32(Env, (napi_value)value, out uint result).ThrowIfFailed();
    return result;
  }

  public static long GetValueInt64(this JSValue value)
  {
    napi_get_value_int64(Env, (napi_value)value, out long result).ThrowIfFailed();
    return result;
  }

  public static bool GetValueBool(this JSValue value)
  {
    napi_get_value_bool(Env, (napi_value)value, out byte result).ThrowIfFailed();
    return result != 0;
  }

  public static unsafe int GetValueStringLatin1(this JSValue thisValue, Span<byte> buffer)
  {
    if (buffer.IsEmpty)
    {
      napi_get_value_string_latin1(Env, (napi_value)thisValue, null, 0, out nuint result).ThrowIfFailed();
      return (int)result;
    }
    fixed (byte* ptr = &buffer[0])
    {
      napi_get_value_string_latin1(Env, (napi_value)thisValue, ptr, (nuint)buffer.Length, out nuint result).ThrowIfFailed();
      return (int)result;
    }
  }

  public static byte[] GetValueStringLatin1(this JSValue value)
  {
    int length = GetValueStringLatin1(value, Span<byte>.Empty);
    byte[] result = new byte[length + 1];
    GetValueStringLatin1(value, new Span<byte>(result));
    return result;
  }

  public static unsafe int GetValueStringUtf8(this JSValue thisValue, Span<byte> buffer)
  {
    if (buffer.IsEmpty)
    {
      napi_get_value_string_utf8(Env, (napi_value)thisValue, null, 0, out nuint result).ThrowIfFailed();
      return (int)result;
    }
    fixed (byte* ptr = &buffer[0])
    {
      napi_get_value_string_utf8(Env, (napi_value)thisValue, ptr, (nuint)buffer.Length, out nuint result).ThrowIfFailed();
      return (int)result;
    }
  }

  public static byte[] GetValueStringUtf8(this JSValue value)
  {
    int length = GetValueStringUtf8(value, Span<byte>.Empty);
    byte[] result = new byte[length + 1];
    GetValueStringUtf8(value, new Span<byte>(result));
    return result;
  }

  public static unsafe int GetValueStringUtf16(this JSValue thisValue, Span<char> buffer)
  {
    if (buffer.IsEmpty)
    {
      napi_get_value_string_utf16(Env, (napi_value)thisValue, null, 0, out nuint result).ThrowIfFailed();
      return (int)result;
    }
    fixed (char* ptr = &buffer[0])
    {
      napi_get_value_string_utf16(Env, (napi_value)thisValue, ptr, (nuint)buffer.Length, out nuint result).ThrowIfFailed();
      return (int)result;
    }
  }

  public static string GetValueStringUtf16(this JSValue value)
  {
    int length = GetValueStringUtf16(value, Span<char>.Empty);
    char[] result = new char[length + 1];
    GetValueStringUtf16(value, new Span<char>(result));
    return new string(result, 0, length);
  }

  public static JSValue CoerceToBoolean(this JSValue value)
  {
    napi_coerce_to_bool(Env, (napi_value)value, out napi_value result).ThrowIfFailed();
    return result;
  }

  public static JSValue CoerceToNumber(this JSValue value)
  {
    napi_coerce_to_number(Env, (napi_value)value, out napi_value result).ThrowIfFailed();
    return result;
  }

  public static JSValue CoerceToObject(this JSValue value)
  {
    napi_coerce_to_object(Env, (napi_value)value, out napi_value result).ThrowIfFailed();
    return result;
  }

  public static JSValue CoerceToString(this JSValue value)
  {
    napi_coerce_to_string(Env, (napi_value)value, out napi_value result).ThrowIfFailed();
    return result;
  }
  public static JSValue GetPrototype(this JSValue value)
  {
    napi_value result;
    napi_get_prototype(Env, (napi_value)value, out result).ThrowIfFailed();
    return result;
  }

  public static JSValue GetPropertyNames(this JSValue value)
  {
    napi_get_property_names(Env, (napi_value)value, out napi_value result).ThrowIfFailed();
    return result;
  }

  public static void SetProperty(this JSValue thisValue, JSValue key, JSValue value)
  {
    napi_set_property(Env, (napi_value)thisValue, (napi_value)key, (napi_value)value).ThrowIfFailed();
  }

  public static bool HasProperty(this JSValue thisValue, JSValue key)
  {
    napi_has_property(Env, (napi_value)thisValue, (napi_value)key, out byte result).ThrowIfFailed();
    return result != 0;
  }

  public static JSValue GetProperty(this JSValue thisValue, JSValue key)
  {
    napi_get_property(Env, (napi_value)thisValue, (napi_value)key, out napi_value result).ThrowIfFailed();
    return result;
  }

  public static bool DeleteProperty(this JSValue thisValue, JSValue key)
  {
    napi_delete_property(Env, (napi_value)thisValue, (napi_value)key, out byte result).ThrowIfFailed();
    return result != 0;
  }

  public static bool HasOwnProperty(this JSValue thisValue, JSValue key)
  {
    napi_has_own_property(Env, (napi_value)thisValue, (napi_value)key, out byte result).ThrowIfFailed();
    return result != 0;
  }

  public static unsafe void SetProperty(this JSValue thisValue, byte* utf8Name, JSValue value)
  {
    napi_set_named_property(Env, (napi_value)thisValue, utf8Name, (napi_value)value).ThrowIfFailed();
  }

  public static unsafe void SetProperty(this JSValue thisValue, ReadOnlyMemory<byte> utf8Name, JSValue value)
  {
    byte* utf8NameBytes = stackalloc byte[utf8Name.Length + 1];
    utf8NameBytes[utf8Name.Length] = 0;
    Buffer.MemoryCopy((byte*)utf8Name.Pin().Pointer, utf8NameBytes, utf8Name.Length, utf8Name.Length);
    SetProperty(thisValue, utf8NameBytes, value);
  }

  public static unsafe bool HasProperty(this JSValue thisValue, byte* utf8Name)
  {
    napi_has_named_property(Env, (napi_value)thisValue, utf8Name, out byte result).ThrowIfFailed();
    return result != 0;
  }

  public static unsafe bool HasProperty(this JSValue thisValue, ReadOnlyMemory<byte> utf8Name)
  {
    byte* utf8NameBytes = stackalloc byte[utf8Name.Length + 1];
    utf8NameBytes[utf8Name.Length] = 0;
    Buffer.MemoryCopy((byte*)utf8Name.Pin().Pointer, utf8NameBytes, utf8Name.Length, utf8Name.Length);
    return HasProperty(thisValue, utf8NameBytes);
  }

  public static unsafe JSValue GetProperty(this JSValue thisValue, byte* utf8Name)
  {
    napi_get_named_property(Env, (napi_value)thisValue, utf8Name, out napi_value result).ThrowIfFailed();
    return result;
  }

  public static unsafe JSValue GetProperty(this JSValue thisValue, ReadOnlyMemory<byte> utf8Name)
  {
    byte* utf8NameBytes = stackalloc byte[utf8Name.Length + 1];
    utf8NameBytes[utf8Name.Length] = 0;
    Buffer.MemoryCopy((byte*)utf8Name.Pin().Pointer, utf8NameBytes, utf8Name.Length, utf8Name.Length);
    return GetProperty(thisValue, utf8NameBytes);
  }
  public static void SetElement(this JSValue thisValue, int index, JSValue value)
  {
    napi_set_element(Env, (napi_value)value, (uint)index, (napi_value)value).ThrowIfFailed();
  }

  public static bool HasElement(this JSValue thisValue, int index)
  {
    napi_has_element(Env, (napi_value)thisValue, (uint)index, out byte result).ThrowIfFailed();
    return result != 0;
  }

  public static JSValue GetElement(this JSValue thisValue, int index)
  {
    napi_get_element(Env, (napi_value)thisValue, (uint)index, out napi_value result).ThrowIfFailed();
    return result;
  }

  public static bool DeleteElement(this JSValue thisValue, int index)
  {
    napi_delete_element(Env, (napi_value)thisValue, (uint)index, out byte result).ThrowIfFailed();
    return result != 0;
  }

  public static unsafe void DefineProperties(this JSValue thisValue, params JSPropertyDescriptor[] descriptors)
  {
    IntPtr[] handles = ToUnmanagedPropertyDescriptors(descriptors, (count, descriptorsPtr) =>
      napi_define_properties(Env, (napi_value)thisValue, count, descriptorsPtr).ThrowIfFailed());
    Array.ForEach(handles, handle => thisValue.AddHandleFinalizer(handle));
  }

  public static bool IsArray(this JSValue thisValue)
  {
    napi_is_array(Env, (napi_value)thisValue, out byte result).ThrowIfFailed();
    return result != 0;
  }

  public static int GetArrayLength(this JSValue thisValue)
  {
    napi_get_array_length(Env, (napi_value)thisValue, out uint result).ThrowIfFailed();
    return (int)result;
  }

  public static bool StrictEquals(this JSValue thisValue, JSValue other)
  {
    napi_strict_equals(Env, (napi_value)thisValue, (napi_value)other, out byte result);
    return result != 0;
  }

  public static unsafe JSValue Call(this JSValue thisValue)
  {
    napi_call_function(Env, (napi_value)GetUndefined(), (napi_value)thisValue, 0, null, out napi_value result).ThrowIfFailed();
    return result;
  }

  public static unsafe JSValue Call(this JSValue thisValue, JSValue thisArg)
  {
    napi_call_function(Env, (napi_value)thisArg, (napi_value)thisValue, 0, null, out napi_value result).ThrowIfFailed();
    return result;
  }

  public static unsafe JSValue Call(this JSValue thisValue, JSValue thisArg, JSValue arg0)
  {
    napi_value argValue0 = (napi_value)arg0;
    napi_call_function(Env, (napi_value)thisArg, (napi_value)thisValue, 1, &argValue0, out napi_value result).ThrowIfFailed();
    return result;
  }

  public static unsafe JSValue Call(this JSValue thisValue, JSValue thisArg, params JSValue[] args)
  {
    int argc = args.Length;
    if (argc == 0)
    {
      return Call(thisValue, thisArg);
    }
    napi_value* argv = stackalloc napi_value[argc];
    for (int i = 0; i < argc; ++i)
    {
      argv[i] = (napi_value)args[i];
    }
    napi_call_function(Env, (napi_value)thisArg, (napi_value)thisValue,
      (nuint)argc, argv, out napi_value result).ThrowIfFailed();
    return result;
  }


  public static unsafe JSValue CallAsConstructor(this JSValue thisValue)
  {
    napi_new_instance(Env, (napi_value)thisValue, 0, null, out napi_value result).ThrowIfFailed();
    return result;
  }

  public static unsafe JSValue CallAsConstructor(this JSValue thisValue, JSValue arg0)
  {
    napi_value argValue0 = (napi_value)arg0;
    napi_new_instance(Env, (napi_value)thisValue, 1, &argValue0, out napi_value result).ThrowIfFailed();
    return result;
  }

  public static unsafe JSValue CallAsConstructor(this JSValue thisValue, params JSValue[] args)
  {
    int argc = args.Length;
    napi_value* argv = stackalloc napi_value[argc];
    for (int i = 0; i < argc; ++i)
    {
      argv[i] = (napi_value)args[i];
    }
    napi_new_instance(Env, (napi_value)thisValue, (nuint)argc, argv, out napi_value result).ThrowIfFailed();
    return result;
  }

  public static bool InstanceOf(this JSValue thisValue, JSValue constructor)
  {
    napi_instanceof(Env, (napi_value)thisValue, (napi_value)constructor, out byte result).ThrowIfFailed();
    return result != 0;
  }

  public static unsafe JSValue DefineClass(
    ReadOnlyMemory<byte> utf8Name,
    delegate* unmanaged[Cdecl]<napi_env, napi_callback_info, napi_value> callback,
    IntPtr data,
    nuint count,
    napi_property_descriptor* descriptors)
  {
    napi_define_class(Env, (byte*)utf8Name.Pin().Pointer, (nuint)utf8Name.Length,
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
    func!.Value.AddHandleFinalizer((IntPtr)callbackHandle);
    Array.ForEach(handles, handle => func!.Value.AddHandleFinalizer(handle));
    return func!.Value;
  }

  public static unsafe JSValue DefineClass(string name, JSCallback callback, params JSPropertyDescriptor[] descriptors)
  {
    return DefineClass(Encoding.UTF8.GetBytes(name), callback, descriptors);
  }

  public static unsafe JSValue Wrap(this JSValue thisValue, object value)
  {
    GCHandle valueHandle = GCHandle.Alloc(value);
    napi_wrap(Env, (napi_value)thisValue, (IntPtr)valueHandle, &FinalizeHandle, IntPtr.Zero, null).ThrowIfFailed();
    return thisValue;
  }

  public static object Unwrap(this JSValue thisValue)
  {
    napi_unwrap(Env, (napi_value)thisValue, out IntPtr result).ThrowIfFailed();
    return GCHandle.FromIntPtr(result).Target!;
  }

  public static object RemoveWrap(this JSValue thisValue)
  {
    napi_remove_wrap(Env, (napi_value)thisValue, out IntPtr result).ThrowIfFailed();
    return GCHandle.FromIntPtr(result).Target!;
  }

  public static unsafe JSValue CreateExternal(object value)
  {
    GCHandle valueHandle = GCHandle.Alloc(value);
    napi_create_external(Env, (IntPtr)valueHandle, &FinalizeHandle, IntPtr.Zero, out napi_value result).ThrowIfFailed();
    return result;
  }

  public static unsafe object GetValueExternal(this JSValue thisValue)
  {
    napi_get_value_external(Env, (napi_value)thisValue, out IntPtr result).ThrowIfFailed();
    return GCHandle.FromIntPtr(result).Target!;
  }

  public static JSReference CreateReference(this JSValue thisValue)
  {
    return new JSReference(thisValue);
  }

  public static JSReference CreateWeakReference(this JSValue thisValue)
  {
    return new JSReference(thisValue, isWeak: true);
  }

  public static void Throw(this JSValue thisValue)
  {
    napi_throw(Env, (napi_value)thisValue).ThrowIfFailed();
  }

  public static void ThrowError(string code, string message)
  {
    napi_throw_error(Env, code, message).ThrowIfFailed();
  }

  public static void ThrowTypeError(string code, string message)
  {
    napi_throw_type_error(Env, code, message).ThrowIfFailed();
  }

  public static void ThrowRangeError(string code, string message)
  {
    napi_throw_range_error(Env, code, message).ThrowIfFailed();
  }

  public static bool IsError(this JSValue thisValue)
  {
    napi_is_error(Env, (napi_value)thisValue, out byte result).ThrowIfFailed();
    return result != 0;
  }

  public static bool IsExceptionPending()
  {
    napi_is_exception_pending(Env, out byte result).ThrowIfFailed();
    return result != 0;
  }

  public static JSValue GetAndClearLastException()
  {
    napi_get_and_clear_last_exception(Env, out napi_value result).ThrowIfFailed();
    return result;
  }

  public static bool IsArrayBuffer(this JSValue thisValue)
  {
    napi_is_arraybuffer(Env, (napi_value)thisValue, out byte result).ThrowIfFailed();
    return result != 0;
  }

  public static unsafe JSValue CreateArrayBuffer(ReadOnlySpan<byte> data)
  {
    napi_create_arraybuffer(Env, (nuint)data.Length, out void* buffer, out napi_value result).ThrowIfFailed();
    data.CopyTo(new Span<byte>(buffer, data.Length));
    return result;
  }

  public static unsafe JSValue CreateExternalArrayBuffer(object external, ReadOnlyMemory<byte> memory)
  {
    PinnedReadOnlyMemory pinnedMemory = new PinnedReadOnlyMemory(external, memory);
    napi_create_external_arraybuffer(
      Env,
      pinnedMemory.Pointer,
      (nuint)pinnedMemory.Length,
      &FinalizeHintHandle, // We pass object to finalize as a hint parameter
      (IntPtr)GCHandle.Alloc(pinnedMemory),
      out napi_value result).ThrowIfFailed();
    return result;
  }

  public static unsafe Span<byte> GetArrayBufferInfo(this JSValue thisValue)
  {
    napi_get_arraybuffer_info(Env, (napi_value)thisValue, out void* data, out nuint length).ThrowIfFailed();
    return new Span<byte>(data, (int)length);
  }

  public static bool IsTypedArray(this JSValue thisValue)
  {
    napi_is_typedarray(Env, (napi_value)thisValue, out byte result).ThrowIfFailed();
    return result != 0;
  }

  public static JSValue CreateTypedArray(JSTypedArrayType type, int length, JSValue arrayBuffer, int byteOffset)
  {
    napi_create_typedarray(
      Env,
      (napi_typedarray_type)type,
      (nuint)length,
      (napi_value)arrayBuffer,
      (nuint)byteOffset,
      out napi_value result).ThrowIfFailed();
    return result;
  }

  public static unsafe void GetTypedArrayInfo(
    this JSValue thisValue,
    out JSTypedArrayType type,
    out int length,
    out void* data,
    out JSValue arrayBuffer,
    out int byteOffset)
  {
    napi_get_typedarray_info(
      Env,
      (napi_value)thisValue,
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
    napi_create_dataview(Env, (nuint)length, (napi_value)arrayBuffer, (nuint)byteOffset, out napi_value result).ThrowIfFailed();
    return result;
  }

  public static bool IsDataView(this JSValue thisValue)
  {
    napi_is_dataview(Env, (napi_value)thisValue, out byte result).ThrowIfFailed();
    return result != 0;
  }

  public static unsafe void GetDataViewInfo(this JSValue thisValue, out ReadOnlySpan<byte> viewSpan, out JSValue arrayBuffer, out int byteOffset)
  {
    napi_get_dataview_info(
      Env,
      (napi_value)thisValue,
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
    napi_get_version(Env, out uint result).ThrowIfFailed();
    return result;
  }

  public static JSValue CreatePromise(out JSDeferred deferred)
  {
    napi_create_promise(Env, out napi_deferred deferred_, out napi_value promise).ThrowIfFailed();
    deferred = new JSDeferred(deferred_);
    return promise;
  }

  public static bool IsPromise(this JSValue thisValue)
  {
    napi_is_promise(Env, (napi_value)thisValue, out byte result).ThrowIfFailed();
    return result != 0;
  }

  public static JSValue RunScript(this JSValue thisValue)
  {
    napi_run_script(Env, (napi_value)thisValue, out napi_value result).ThrowIfFailed();
    return result;
  }

  public static JSValue CreateDate(double time)
  {
    napi_create_date(Env, time, out napi_value result).ThrowIfFailed();
    return result;
  }

  public static bool IsDate(this JSValue thisValue)
  {
    napi_is_date(Env, (napi_value)thisValue, out byte result).ThrowIfFailed();
    return result != 0;
  }

  public static double GetDateValue(this JSValue thisValue)
  {
    napi_get_date_value(Env, (napi_value)thisValue, out double result).ThrowIfFailed();
    return result;
  }

  public static JSValue CreateBigInt(long value)
  {
    napi_create_bigint_int64(Env, value, out napi_value result).ThrowIfFailed();
    return result;
  }

  public static JSValue CreateBigInt(ulong value)
  {
    napi_create_bigint_uint64(Env, value, out napi_value result).ThrowIfFailed();
    return result;
  }

  public static unsafe JSValue CreateBigInt(int signBit, ReadOnlyMemory<ulong> words)
  {
    napi_create_bigint_words(Env, signBit, (nuint)words.Length, (ulong*)words.Pin().Pointer, out napi_value result).ThrowIfFailed();
    return result;
  }

  public static long GetValueBigIntInt64(this JSValue thisValue, out bool isLossless)
  {
    napi_get_value_bigint_int64(Env, (napi_value)thisValue, out long result, out byte lossless).ThrowIfFailed();
    isLossless = lossless != 0;
    return result;
  }

  public static ulong GetValueBigIntUInt64(this JSValue thisValue, out bool isLossless)
  {
    napi_get_value_bigint_uint64(Env, (napi_value)thisValue, out ulong result, out byte lossless).ThrowIfFailed();
    isLossless = lossless != 0;
    return result;
  }

  public static unsafe ulong[] GetValueBigIntWords(this JSValue thisValue, out int signBit)
  {
    napi_get_value_bigint_words(Env, (napi_value)thisValue, out signBit, out nuint wordCount, null).ThrowIfFailed();
    ulong[] words = new ulong[wordCount];
    fixed (ulong* wordsPtr = &words[0])
    {
      napi_get_value_bigint_words(Env, (napi_value)thisValue, out signBit, out wordCount, wordsPtr).ThrowIfFailed();
    }
    return words;
  }

  public static JSValue GetAllPropertyNames(this JSValue thisValue, JSKeyCollectionMode mode, JSKeyFilter filter, JSKeyConversion conversion)
  {
    napi_get_all_property_names(
      Env,
      (napi_value)thisValue,
      (napi_key_collection_mode)mode,
      (napi_key_filter)filter,
      (napi_key_conversion)conversion,
      out napi_value result).ThrowIfFailed();
    return result;
  }

  public static unsafe void SetInstanceData(object? data)
  {
    napi_get_instance_data(Env, out IntPtr handlePtr).ThrowIfFailed();
    if (handlePtr != IntPtr.Zero)
    {
      GCHandle.FromIntPtr(handlePtr).Free();
    }

    if (data != null)
    {
      GCHandle handle = GCHandle.Alloc(data);
      napi_set_instance_data(
        Env,
        (IntPtr)handle,
        &FinalizeHandle,
        IntPtr.Zero).ThrowIfFailed();
    }
  }

  public static object? GetInstanceData()
  {
    napi_get_instance_data(Env, out IntPtr data).ThrowIfFailed();
    return (data != IntPtr.Zero) ? GCHandle.FromIntPtr(data).Target : null;
  }

  public static void DetachArrayBuffer(this JSValue thisValue)
  {
    napi_detach_arraybuffer(Env, (napi_value)thisValue).ThrowIfFailed();
  }

  public static bool IsDetachedArrayBuffer(this JSValue thisValue)
  {
    napi_is_detached_arraybuffer(Env, (napi_value)thisValue, out byte result).ThrowIfFailed();
    return result != 0;
  }

  public static void SetObjectTypeTag(this JSValue thisValue, ref napi_type_tag typeTag)
  {
    napi_type_tag_object(Env, (napi_value)thisValue, ref typeTag);
  }

  public static unsafe void SetObjectTypeTag(this JSValue thisValue, ref Guid typeGuid)
  {
    // TODO: simplify in .Net 7 by using MemoryMarshal.AsRef
    napi_type_tag typeTag = new napi_type_tag { lower = 0, upper = 0 };

    fixed (byte* source = &MemoryMarshal.AsBytes(MemoryMarshal.CreateReadOnlySpan(ref typeGuid, 1))[0])
    {
      fixed (byte* dest = &MemoryMarshal.AsBytes(MemoryMarshal.CreateReadOnlySpan(ref typeTag, 1))[0])
      {
        Buffer.MemoryCopy(source, dest, 16, 16);
      }
    }

    thisValue.SetObjectTypeTag(ref typeTag);
  }

  public static bool CheckObjectTypeTag(this JSValue thisValue, ref napi_type_tag typeTag)
  {
    napi_check_object_type_tag(Env, (napi_value)thisValue, ref typeTag, out byte result);
    return result != 0;
  }

  public static unsafe bool CheckObjectTypeTag(this JSValue thisValue, ref Guid typeGuid)
  {
    // TODO: simplify in .Net 7 by using MemoryMarshal.AsRef
    napi_type_tag typeTag = new napi_type_tag { lower = 0, upper = 0 };

    fixed (byte* source = &MemoryMarshal.AsBytes(MemoryMarshal.CreateReadOnlySpan(ref typeGuid, 1))[0])
    {
      fixed (byte* dest = &MemoryMarshal.AsBytes(MemoryMarshal.CreateReadOnlySpan(ref typeTag, 1))[0])
      {
        Buffer.MemoryCopy(source, dest, 16, 16);
      }
    }

    return thisValue.CheckObjectTypeTag(ref typeTag);
  }

  public static void FreezeObject(this JSValue thisValue)
  {
    napi_object_freeze(Env, (napi_value)thisValue).ThrowIfFailed();
  }

  public static void SealObject(this JSValue thisValue)
  {
    napi_object_seal(Env, (napi_value)thisValue).ThrowIfFailed();
  }

  private static napi_env Env => (napi_env)JSValueScope.Current;

  [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
  private static unsafe napi_value InvokeJSCallback(napi_env env, napi_callback_info callbackInfo)
  {
    using var scope = new JSValueScope(env);
    JSCallbackArgs args = new JSCallbackArgs(scope, callbackInfo);
    JSCallback callback = (JSCallback)GCHandle.FromIntPtr(args.Data).Target!;
    return (napi_value)callback(args);
  }

  [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
  private static unsafe napi_value InvokeJSMethod(napi_env env, napi_callback_info callbackInfo)
  {
    using var scope = new JSValueScope(env);
    JSCallbackArgs args = new JSCallbackArgs(scope, callbackInfo);
    JSPropertyDescriptor desc = (JSPropertyDescriptor)GCHandle.FromIntPtr(args.Data).Target!;
    return (napi_value)desc.Method!.Invoke(args);
  }

  [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
  private static unsafe napi_value InvokeJSGetter(napi_env env, napi_callback_info callbackInfo)
  {
    using var scope = new JSValueScope(env);
    JSCallbackArgs args = new JSCallbackArgs(scope, callbackInfo);
    JSPropertyDescriptor desc = (JSPropertyDescriptor)GCHandle.FromIntPtr(args.Data).Target!;
    return (napi_value)desc.Getter!.Invoke(args);
  }

  [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
  private static unsafe napi_value InvokeJSSetter(napi_env env, napi_callback_info callbackInfo)
  {
    using var scope = new JSValueScope(env);
    JSCallbackArgs args = new(scope, callbackInfo);
    JSPropertyDescriptor desc = (JSPropertyDescriptor)GCHandle.FromIntPtr(args.Data).Target!;
    return (napi_value)desc.Setter!.Invoke(args);
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
      descriptorPtr->value = (napi_value)descriptor.Value;
      descriptorPtr->attributes = (napi_property_attributes)descriptor.Attributes;
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

  private unsafe delegate void UseUnmanagedDescriptors(nuint count, napi_property_descriptor* descriptors);

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
}
