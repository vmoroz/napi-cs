using System;

namespace NApi.Types
{
  public sealed class JsEnv
  {
    internal IntPtr EnvPtr { get; }

    internal JsEnv(IntPtr envPtr)
    {
      EnvPtr = envPtr;
    }

    public static JsEnv FromPointer(IntPtr envPtr)
    {
      return new(envPtr);
    }

    public static implicit operator napi_env(JsEnv value) => new napi_env { Pointer = value.EnvPtr };
  }
}