using System;
using static NApi.NodeApi;

namespace NApi
{
  public sealed class JSSimpleValueScope : JSValueScope
  {
    public napi_handle_scope Handle => (napi_handle_scope)handle;

    public JSSimpleValueScope(JSEnvironment env) : base(env)
    {
      napi_open_handle_scope((napi_env)env, out napi_handle_scope scope).ThrowIfFailed();
      SetHandle((IntPtr)scope);
    }

    protected override bool ReleaseHandle()
    {
      napi_close_handle_scope((napi_env)Environment, Handle).ThrowIfFailed();
      return true;
    }
  }
}