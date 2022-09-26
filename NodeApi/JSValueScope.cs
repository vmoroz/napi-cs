using System;
using System.Runtime.InteropServices;

namespace NodeApi;

public abstract class JSValueScope : SafeHandle
{
  private JSEnvironment _env;
  [ThreadStatic] private static JSValueScope? t_current = null;

  public JSValueScope? ParentScope { get; }

  public JSEnvironment Environment
  {
    get
    {
      EnsureIsValid();
      return _env;
    }
  }

  protected JSValueScope(JSEnvironment env) : base(IntPtr.Zero, true)
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
      return (napi_env)scope.Environment;
    else
      throw new JSException("Out of scope!");
  }
}