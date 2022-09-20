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

    public static JSValue GetUndefined()
    {
      JsScope? scope = JsScope.Current;
      if (scope == null)
        throw new InvalidOperationException("Scope is null");
      IntPtr valuePtr;
      unsafe
      {
        NApi.ApiProvider.JsNativeApi.napi_get_undefined(scope.Env.EnvPtr, &valuePtr).ThrowIfFailed(scope);
      }
      return new JSValue(valuePtr, scope);
    }
  }
}
