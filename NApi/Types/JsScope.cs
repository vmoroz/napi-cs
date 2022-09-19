using System;
using System.Threading;
using NApi.Exception;
using NApi.Utils;

namespace NApi.Types
{
  public sealed class JsScope : Disposable
  {
    private JsEnv _env;
    private IntPtr? _scopePtr;
    private JsScope? _parentScope;
    [ThreadStatic] private static JsScope? t_current;

    internal JsEnv Env
    {
      get
      {
        EnsureNotDisposed();
        return _env;
      }
    }

    internal IntPtr? ScopePtr
    {
      get
      {
        EnsureNotDisposed();
        return _scopePtr;
      }
    }

    internal JsScope(JsEnv env, IntPtr? scopePtr = null)
    {
      _env = env;
      _scopePtr = scopePtr;
      _parentScope = t_current;
      t_current = this;
    }

    protected override void DisposeManaged()
    {
      base.DisposeManaged();
      t_current = _parentScope;
    }

    public static JsScope? Current { get { return t_current; } }

    public static JsScope FromPointer(JsEnv env, IntPtr? scopePtr)
    {
      return new(env, scopePtr);
    }

    public void EnsureNotDisposed()
    {
      if (Disposed)
      {
        throw new NApiException("Out of scope!");
      }
    }
  }
}