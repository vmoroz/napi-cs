using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using static NApi.NodeApi;

namespace NApi.Types
{
  public abstract class JsValue
  {
    public JsScope Scope { get; }

    public IntPtr ValuePtr { get; }

    private HashSet<GCHandle> ManagedGCHandles = new();

    private static GCHandle? NApiFinalizeCallbackHandle;
    private static IntPtr? NApiFinalizeCallbackPtr;

    protected internal JsValue(JsScope scope, IntPtr valuePtr)
    {
      scope.EnsureNotDisposed();
      Scope = scope;
      ValuePtr = valuePtr;
    }

    internal static JsValue Create(JsScope scope, IntPtr valuePtr)
    {
      var valueType = Typeof(scope, valuePtr);
      return valueType switch
      {
        JSValueType.Number => new JsNumber(scope, valuePtr),
        JSValueType.Undefined => throw new NotImplementedException(),
        JSValueType.Null => throw new NotImplementedException(),
        JSValueType.Boolean => throw new NotImplementedException(),
        JSValueType.String => new JsString(scope, valuePtr),
        JSValueType.Symbol => throw new NotImplementedException(),
        JSValueType.Object => new JsObject(scope, valuePtr),
        JSValueType.Function => new JsFunction(scope, valuePtr),
        JSValueType.External => throw new NotImplementedException(),
        JSValueType.BigInt => throw new NotImplementedException(),
        _ => throw new ArgumentOutOfRangeException()
      };
    }

    private static unsafe JSValueType Typeof(JsScope scope, IntPtr valuePtr)
    {
      JSValueType valueType;
      napi_typeof(scope.Env, valuePtr, &valueType).ThrowIfFailed(scope);
      return valueType;
    }

    public void AttachGCHandle(GCHandle handle)
    {
      //try
      //{
      //  var handlePtr = GCHandle.ToIntPtr(handle);

      //  if (NApiFinalizeCallbackHandle == null || NApiFinalizeCallbackPtr == null)
      //  {
      //    var cb = (napi_finalize)NApiFinalize;
      //    NApiFinalizeCallbackHandle = GCHandle.Alloc(cb);
      //    NApiFinalizeCallbackPtr = Marshal.GetFunctionPointerForDelegate(cb);
      //  }

      //  NApi.ApiProvider.JsNativeApi.napi_add_finalizer(Scope.Env.EnvPtr, ValuePtr, handlePtr, NApiFinalizeCallbackPtr.Value, IntPtr.Zero, IntPtr.Zero).ThrowIfFailed(Scope);
      //}
      //catch
      //{
      //  handle.Free();
      //  throw;
      //}

      //ManagedGCHandles.Add(handle);
    }

    private delegate void napi_finalize(IntPtr envPtr, IntPtr finalizeDataPtr, IntPtr finalizeHintPtr);

    private void NApiFinalize(IntPtr envPtr, IntPtr finalizeDataPtr, IntPtr finalizeHintPtr)
    {
      var handle = GCHandle.FromIntPtr(finalizeDataPtr);
      handle.Free();
      ManagedGCHandles.Remove(handle);
    }
  }
}