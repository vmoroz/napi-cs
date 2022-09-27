using System;
using static NodeApi.JSNative.Interop;

namespace NodeApi;

public sealed class JSEscapableValueScope : JSValueScope
{
  public napi_escapable_handle_scope Handle => (napi_escapable_handle_scope)handle;

  public JSEscapableValueScope(napi_env env) : base(env)
  {
    napi_open_escapable_handle_scope(env, out napi_escapable_handle_scope scope).ThrowIfFailed();
    SetHandle((IntPtr)scope);
  }

  public JSValue Escape(JSValue value)
  {
    if (ParentScope == null)
      throw new InvalidOperationException($"{ParentScope} must not be null");

    napi_escape_handle((napi_env)this, Handle, (napi_value)value, out napi_value result);
    return new JSValue(ParentScope, result);
  }

  protected override bool ReleaseHandle()
  {
    napi_close_escapable_handle_scope((napi_env)this, Handle).ThrowIfFailed();
    return true;
  }
}