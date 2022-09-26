namespace NApi;

public sealed class JSEnvironment
{
  private napi_env _env;

  public JSEnvironment(napi_env env)
  {
    _env = env;
  }

  public static explicit operator napi_env(JSEnvironment env) => env._env;
}