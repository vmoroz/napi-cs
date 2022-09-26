using static NodeApi.JSNative.Interop;

namespace NodeApi;

public static class JSExtensionMethods
{
  public static void ThrowIfFailed(this napi_status status)
  {
    if (status != napi_status.napi_ok)
    {
      throw new JSException(status);
    }
  }

  public static void ThrowIfFailed(this napi_status status, string message)
  {
    if (status != napi_status.napi_ok)
    {
      throw new JSException(message);
    }
  }
}