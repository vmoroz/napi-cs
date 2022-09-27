using System;
using System.Collections.Generic;

namespace NodeApi;

public class JSClassBuilder<T> where T : class
{
  private List<JSPropertyDescriptor> _properties = new();

  public delegate T ConstructorDelegate(JSCallbackArgs args);

  public string ClassName { get; }

  public ConstructorDelegate Constructor { get; }

  public JSClassBuilder(string className, ConstructorDelegate constructor)
  {
    ClassName = className;
    Constructor = constructor;
  }

  public JSClassBuilder<T> Property(string name, JSValue value, JSPropertyAttributes attributes = JSPropertyAttributes.DefaultProperty)
  {
    _properties.Add(new JSPropertyDescriptor(name, value, attributes));
    return this;
  }

  public JSClassBuilder<T> Property(string name, JSCallback? getter, JSCallback? setter,
    JSPropertyAttributes attributes = JSPropertyAttributes.Enumerable | JSPropertyAttributes.Configurable)

  {
    _properties.Add(new JSPropertyDescriptor(name, getter, setter, attributes));
    return this;
  }

  public JSClassBuilder<T> Property(string name, Func<T, JSValue>? getter, Action<T, JSValue>? setter,
    JSPropertyAttributes attributes = JSPropertyAttributes.Enumerable | JSPropertyAttributes.Configurable)
  {
    return Property(
      name,
      getter != null ? args => getter((T)args.ThisArg.Unwrap()) : null,
      setter != null ? args =>
      {
        setter((T)args.ThisArg.Unwrap(), args[0]);
        return JSValue.GetUndefined();
      }
    : null,
      attributes);
  }

  public JSClassBuilder<T> Method(string name, JSCallback callback, JSPropertyAttributes attributes = JSPropertyAttributes.DefaultProperty)
  {
    _properties.Add(new JSPropertyDescriptor(name, callback, attributes));
    return this;
  }

  public JSClassBuilder<T> Method(string name, Func<T, Action> getMethod, JSPropertyAttributes attributes = JSPropertyAttributes.DefaultMethod)
  {
    return Method(name, args =>
    {
      getMethod((T)args.ThisArg.Unwrap()).Invoke();
      return JSValue.GetUndefined();
    }, attributes);
  }

  public JSClassBuilder<T> Method(string name, Func<T, Action<JSCallbackArgs>> getMethod, JSPropertyAttributes attributes = JSPropertyAttributes.DefaultMethod)
  {
    return Method(name, args =>
    {
      getMethod((T)args.ThisArg.Unwrap()).Invoke(args);
      return JSValue.GetUndefined();
    }, attributes);
  }

  public JSClassBuilder<T> Method(string name, Func<T, Func<JSValue>> getMethod, JSPropertyAttributes attributes = JSPropertyAttributes.DefaultMethod)
  {
    return Method(name, args => getMethod((T)args.ThisArg.Unwrap()).Invoke(), attributes);
  }

  public JSClassBuilder<T> Method(string name, Func<T, Func<JSCallbackArgs, JSValue>> getMethod, JSPropertyAttributes attributes = JSPropertyAttributes.DefaultMethod)
  {
    return Method(name, args => getMethod((T)args.ThisArg.Unwrap()).Invoke(args), attributes);
  }

  public JSValue ToJSValue()
  {
    return JSNative.DefineClass(ClassName, args => args.ThisArg.Wrap(Constructor(args)), _properties.ToArray());
  }
}
