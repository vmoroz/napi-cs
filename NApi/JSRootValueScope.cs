using System;

namespace NApi
{
  public sealed class JSRootValueScope : JSValueScope
  {
    public JSRootValueScope(JSEnvironment env) : base(env)
    {
      SetHandle((IntPtr)1);
    }

    protected override bool ReleaseHandle()
    {
      return true;
    }
  }
}