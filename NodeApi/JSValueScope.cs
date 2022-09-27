using System;
using System.Runtime.InteropServices;
using static NodeApi.JSNative.Interop;

namespace NodeApi;

public abstract class JSValueScope : SafeHandle
{
  private napi_env _env;
  [ThreadStatic] private static JSValueScope? t_current = null;

  public JSValueScope? ParentScope { get; }

   protected JSValueScope(napi_env env) : base(IntPtr.Zero, true)
  {
    _env = env;
    ParentScope = t_current;
    t_current = this;
  }

  public static JSValueScope? Current { get { return t_current; } }

  public void EnsureIsValid()
  {
    if (IsInvalid)
      throw new JSException("Out of scope!");
  }

  public override bool IsInvalid => handle == IntPtr.Zero;

  public static explicit operator napi_env(JSValueScope? scope)
  {
    if (scope != null)
      return scope._env;
    else
      throw new JSException("Out of scope!");
  }
}