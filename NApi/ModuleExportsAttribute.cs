using System;

namespace NApi;

[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
public sealed class ModuleExportsAttribute : Attribute
{
  public ModuleExportsAttribute()
  {
  }
}