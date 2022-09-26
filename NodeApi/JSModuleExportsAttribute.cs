using System;

namespace NodeApi;

[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
public sealed class JSModuleExportsAttribute : Attribute
{
  public JSModuleExportsAttribute()
  {
  }
}