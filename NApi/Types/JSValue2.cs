using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Formats.Asn1.AsnWriter;

namespace NApi.Types
{
  // New class for JSValue
  public class JSValue
  {
    internal IntPtr ValuePtr { get; }

    internal JsScope Scope { get; }

    protected JSValue(IntPtr valuePtr, JsScope scope)
    {
      ValuePtr = valuePtr;
      Scope = scope;
    }

    private static JsScope GetScope()
    {
      JsScope? scope = JsScope.Current;
      if (scope == null)
        throw new InvalidOperationException("Scope is null");
      return scope;
    }

    public static JSValue GetUndefined()
    {
      JsScope scope = GetScope();
      IntPtr valuePtr;
      unsafe
      {
        NApi.ApiProvider.JsNativeApi.napi_get_undefined(scope.Env.EnvPtr, new IntPtr(&valuePtr)).ThrowIfFailed(scope);
      }
      return new JSValue(valuePtr, scope);
    }

    public static JSValue GetNull()
    {
      JsScope scope = GetScope();
      IntPtr valuePtr;
      unsafe
      {
        NApi.ApiProvider.JsNativeApi.napi_get_null(scope.Env.EnvPtr, new IntPtr(&valuePtr)).ThrowIfFailed(scope);
      }
      return new JSValue(valuePtr, scope);
    }
  }
}
