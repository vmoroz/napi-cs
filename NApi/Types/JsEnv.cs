namespace NApi.Types
{
  public sealed class JsEnv
  {
    private napi_env _env;

    public JsEnv(napi_env env)
    {
      _env = env;
    }

    public static explicit operator napi_env(JsEnv env) => (napi_env)env._env;
  }
}