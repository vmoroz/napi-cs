using System;
using static NodeApi.JSNative.Interop;

namespace NodeApi;

public sealed class JSRootValueScope : JSValueScope
{
  public JSRootValueScope(napi_env env) : base(env)
  {
    SetHandle((IntPtr)1);
  }

  protected override bool ReleaseHandle()
  {
    return true;
  }
}