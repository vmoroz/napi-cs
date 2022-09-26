using System;
using static NodeApi.JSNative.Interop;

namespace NodeApi;

public class JSCallbackArgs
{
  private JSValue[] _args;

  public JSValue this[int index]
  {
    get { return _args[index]; }
  }

  public int Length
  {
    get { return _args.Length; }
  }

  public JSValue ThisArg { get; }

  public IntPtr Data { get; }

  public JSValue GetNewTarget()
  {
    napi_get_new_target((napi_env)Scope, CallbackInfo, out napi_value result).ThrowIfFailed();
    return result;
  }

  internal JSValueScope Scope { get; }

  internal napi_callback_info CallbackInfo { get; }

  public JSCallbackArgs(JSValueScope scope, napi_callback_info callbackInfo)
  {
    Scope = scope;
    CallbackInfo = callbackInfo;
    unsafe
    {
      nuint argc = 0;
      napi_get_cb_info((napi_env)scope, callbackInfo, &argc, null, null, IntPtr.Zero).ThrowIfFailed();
      napi_value* argv = stackalloc napi_value[(int)argc];
      napi_value thisArg;
      IntPtr data;
      napi_get_cb_info((napi_env)scope, callbackInfo, &argc, argv, &thisArg, new IntPtr(&data)).ThrowIfFailed();

      _args = new JSValue[(int)argc];
      for (int i = 0; i < (int)argc; ++i)
      {
        _args[i] = argv[i];
      }

      ThisArg = thisArg;
      Data = data;
    }
  }
}