using System;
using System.Runtime.InteropServices;
using static NodeApi.JSNative.Interop;

namespace NodeApi;

public class JSReference : SafeHandle
{
  public napi_ref Handle { get { return new napi_ref(handle); } }

  public bool IsWeak { get; }

  public JSReference(JSValue value, bool isWeak = false) : base(IntPtr.Zero, true)
  {
    napi_create_reference(Env, (napi_value)value, isWeak ? 0u : 1u, out napi_ref handle).ThrowIfFailed();
    SetHandle((IntPtr)handle);
    IsWeak = isWeak;
  }

  public void MakeWeak()
  {
    if (!IsWeak)
    {
      napi_reference_unref(Env, Handle, IntPtr.Zero).ThrowIfFailed();
    }
  }
  public void MakeStrong()
  {
    if (IsWeak)
    {
      napi_reference_ref(Env, Handle, IntPtr.Zero).ThrowIfFailed();
    }
  }

  public JSValue? GetValue()
  {
    napi_get_reference_value(Env, Handle, out napi_value result);
    return result.IsNull ? null : result;
  }

  public override bool IsInvalid => handle == IntPtr.Zero;

  protected override bool ReleaseHandle()
  {
    napi_delete_reference(Env, Handle).ThrowIfFailed();
    return true;
  }

  public static explicit operator napi_ref(JSReference value) => value.Handle;

  private static napi_env Env => (napi_env)JSValueScope.Current;
}
