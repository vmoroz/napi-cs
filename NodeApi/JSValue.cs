using System;
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

  public static JSValue Undefined => JSNative.GetUndefined();
  public static JSValue Null => JSNative.GetUndefined();
  public static JSValue Global => JSNative.GetGlobal();
  public static JSValue True => JSNative.GetBoolean(true);
  public static JSValue False => JSNative.GetBoolean(false);
  public static JSValue GetBoolean(bool value) => JSNative.GetBoolean(value);

  public static implicit operator JSValue(bool value) => JSNative.GetBoolean(value);
  public static implicit operator JSValue(sbyte value) => JSNative.CreateNumber(value);
  public static implicit operator JSValue(byte value) => JSNative.CreateNumber(value);
  public static implicit operator JSValue(short value) => JSNative.CreateNumber(value);
  public static implicit operator JSValue(ushort value) => JSNative.CreateNumber(value);
  public static implicit operator JSValue(int value) => JSNative.CreateNumber(value);
  public static implicit operator JSValue(uint value) => JSNative.CreateNumber(value);
  public static implicit operator JSValue(long value) => JSNative.CreateNumber(value);
  public static implicit operator JSValue(ulong value) => JSNative.CreateNumber(value);
  public static implicit operator JSValue(float value) => JSNative.CreateNumber(value);
  public static implicit operator JSValue(double value) => JSNative.CreateNumber(value);
  public static implicit operator JSValue(string value) => JSNative.CreateStringUtf16(value);
  public static implicit operator JSValue(JSCallback callback) => JSNative.CreateFunction("Unknown", callback);

  public static explicit operator bool(JSValue value) => value.GetValueBool();
  public static explicit operator sbyte(JSValue value) => (sbyte)value.GetValueInt32();
  public static explicit operator byte(JSValue value) => (byte)value.GetValueUInt32();
  public static explicit operator short(JSValue value) => (short)value.GetValueInt32();
  public static explicit operator ushort(JSValue value) => (ushort)value.GetValueUInt32();
  public static explicit operator int(JSValue value) => value.GetValueInt32();
  public static explicit operator uint(JSValue value) => value.GetValueUInt32();
  public static explicit operator long(JSValue value) => value.GetValueInt64();
  public static explicit operator ulong(JSValue value) => (ulong)value.GetValueInt64();
  public static explicit operator float(JSValue value) => (float)value.GetValueDouble();
  public static explicit operator double(JSValue value) => value.GetValueDouble();
  public static explicit operator string(JSValue value) => value.GetValueStringUtf16();

  public JSValue this[string name]
  {
    get { return this.GetProperty(name); }
    set { this.SetProperty(name, value); }
  }

  public JSValue this[int index]
  {
    get { return this.GetElement(index); }
    set { this.SetElement(index, value); }
  }

  public static explicit operator napi_value(JSValue value) => value.GetCheckedHandle();

  public static implicit operator JSValue(napi_value value) => new JSValue(value);
}
