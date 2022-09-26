using static NodeApi.JSNative.Interop;

namespace NodeApi;

public record struct JSErrorInfo(string Message, napi_status Status);