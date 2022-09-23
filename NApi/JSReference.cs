using System;
using System.Runtime.InteropServices;
using static NApi.NodeApi;

namespace NApi
{
  public class JSReference : SafeHandle
  {
    public JSEnvironment Environment { get; }

    public napi_ref Handle { get { return new napi_ref(handle); } }

    public bool IsWeak { get; }

    public JSReference(JSEnvironment env, JSValue value, bool isWeak = false) : base(IntPtr.Zero, true)
    {
      napi_create_reference((napi_env)env, (napi_value)value, isWeak ? 0u : 1u, out napi_ref handle).ThrowIfFailed();
      Environment = env;
      SetHandle((IntPtr)handle);
      IsWeak = isWeak;
    }

    public void MakeWeak()
    {
      if (!IsWeak)
      {
        napi_reference_unref((napi_env)Environment, Handle, IntPtr.Zero).ThrowIfFailed();
      }
    }
    public void MakeStrong()
    {
      if (IsWeak)
      {
        napi_reference_ref((napi_env)Environment, Handle, IntPtr.Zero).ThrowIfFailed();
      }
    }

    public JSValue? GetValue()
    {
      napi_get_reference_value((napi_env)Environment, Handle, out napi_value result);
      return result.IsNull ? null : result;
    }

    public override bool IsInvalid => handle == IntPtr.Zero;

    protected override bool ReleaseHandle()
    {
      napi_delete_reference((napi_env)Environment, Handle).ThrowIfFailed();
      return true;
    }

    public static explicit operator napi_ref(JSReference value) => value.Handle;
  }
}
