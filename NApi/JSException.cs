using System;

namespace NApi
{
  public class JSException : Exception
  {
    public override string Message { get; }

    public unsafe JSException(napi_status status)
    {
      Message = status.ToString();
    }

    public unsafe JSException(string message)
    {
      Message = message;
    }
  }
}