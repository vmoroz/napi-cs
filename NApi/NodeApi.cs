using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;

namespace NApi
{
  struct napi_env
  {
    public IntPtr Pointer;
  }

  [SuppressUnmanagedCodeSecurity]
  public unsafe class NodeApi
  {
    public static void SetupDllImportResolver(Assembly assembly)
    {
      NativeLibrary.SetDllImportResolver(assembly, (name, _, _) =>
      {
        if (name == nameof(NodeApi))
        {
          if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
          {
            return GetModuleHandle(null);
          }
          else
          {
            return dlopen(null, 1);
          }
        }

        return IntPtr.Zero;
      });
    }

    [DllImport("kernel32.dll")]
    private static extern IntPtr GetModuleHandle(string? moduleName);

    [DllImport("dl")]
    private static extern IntPtr dlopen(string? moduleName, int flags);

    // napi_status napi_get_last_error_info(napi_env env, const napi_extended_error_info **result)
    [DllImport(nameof(NodeApi), CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern napi_status napi_get_last_error_info(IntPtr env, IntPtr result);

    // napi_status napi_get_undefined(napi_env env, napi_value *result)
    [DllImport(nameof(NodeApi), CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern napi_status napi_get_undefined(IntPtr env, IntPtr result);

    // napi_status napi_get_null(napi_env env, napi_value *result)
    [DllImport(nameof(NodeApi), CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern napi_status napi_get_null(IntPtr env, IntPtr result);

    // napi_status napi_get_global(napi_env env, napi_value* result)
    [DllImport(nameof(NodeApi), CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern napi_status napi_get_global(IntPtr env, IntPtr result);

    // napi_status napi_get_boolean(napi_env env, bool value, napi_value *result)
    [DllImport(nameof(NodeApi), CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern napi_status napi_get_boolean(IntPtr env, bool value, IntPtr result);

    // napi_status napi_create_object(napi_env env, napi_value *result)
    [DllImport(nameof(NodeApi), CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern napi_status napi_create_object(IntPtr env, IntPtr result);

    // napi_status napi_create_array(napi_env env, napi_value *result)
    [DllImport(nameof(NodeApi), CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern napi_status napi_create_array(IntPtr env, IntPtr result);

    // napi_status napi_create_array_with_length(napi_env env, size_t length, napi_value *result)
    [DllImport(nameof(NodeApi), CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern napi_status napi_create_array_with_length(IntPtr env, UIntPtr length,
        IntPtr result);

    // napi_status napi_create_double(napi_env env, double value, napi_value *result)
    [DllImport(nameof(NodeApi), CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern napi_status napi_create_double(IntPtr env, double value, IntPtr result);

    // napi_status napi_create_int32(napi_env env, int32_t value, napi_value *result)
    [DllImport(nameof(NodeApi), CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern napi_status napi_create_int32(IntPtr env, int value, IntPtr result);

    // napi_status napi_create_uint32(napi_env env, uint32_t value, napi_value *result)
    [DllImport(nameof(NodeApi), CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern napi_status napi_create_uint32(IntPtr env, uint value, IntPtr result);

    // napi_status napi_create_int64(napi_env env, int64_t value, napi_value *result)
    [DllImport(nameof(NodeApi), CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern napi_status napi_create_int64(IntPtr env, long value, IntPtr result);

    // napi_status napi_create_string_latin1(napi_env env, const char *str, size_t length, napi_value *result)
    [DllImport(nameof(NodeApi), CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern napi_status napi_create_string_latin1(IntPtr env, void* str, UIntPtr length, IntPtr result);

    // napi_status napi_create_string_utf8(napi_env env, const char *str, size_t length, napi_value *result)
    [DllImport(nameof(NodeApi), CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern napi_status napi_create_string_utf8(napi_env env, void* str, nuint length, IntPtr result);

    // napi_status napi_create_string_utf16(napi_env env, const char16_t *str, size_t length, napi_value *result)
    [DllImport(nameof(NodeApi), CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern napi_status napi_create_string_utf16(IntPtr env, void* str, UIntPtr length, IntPtr result);

    // napi_status napi_create_symbol(napi_env env, napi_value description, napi_value *result)
    [DllImport(nameof(NodeApi), CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern napi_status napi_create_symbol(IntPtr env, IntPtr description, IntPtr result);

    // napi_status napi_create_function(napi_env env, const char* utf8name, size_t length,
    //    napi_callback cb, void* data, napi_value *result)
    [DllImport(nameof(NodeApi), CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern napi_status napi_create_function(IntPtr env,
        void* utf8name, UIntPtr length, delegate* unmanaged[Cdecl]<IntPtr, IntPtr, IntPtr> cb, IntPtr data, IntPtr result);

    // napi_status napi_create_error(napi_env env, napi_value code, napi_value msg, napi_value *result)
    [DllImport(nameof(NodeApi), CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern napi_status napi_create_error(IntPtr env, IntPtr code, IntPtr msg,
        IntPtr result);

    // napi_status napi_create_type_error(napi_env env, napi_value code, napi_value msg, napi_value *result)
    [DllImport(nameof(NodeApi), CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern napi_status napi_create_type_error(IntPtr env, IntPtr code, IntPtr msg,
        IntPtr result);

    // napi_status napi_create_range_error(napi_env env, napi_value code, napi_value msg, napi_value *result)
    [DllImport(nameof(NodeApi), CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern napi_status napi_create_range_error(IntPtr env, IntPtr code,
        IntPtr msg, IntPtr result);

    // napi_status napi_typeof(napi_env env, napi_value value, napi_valuetype* result)
    [DllImport(nameof(NodeApi), CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern napi_status napi_typeof(IntPtr env, IntPtr value, JSValueType* result);

    // napi_status napi_get_value_double(napi_env env, napi_value value, double *result)
    [DllImport(nameof(NodeApi), CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern napi_status napi_get_value_double(IntPtr env, IntPtr value, out double result);

    // napi_status napi_get_value_int32(napi_env env, napi_value value, int32_t *result)
    [DllImport(nameof(NodeApi), CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern napi_status napi_get_value_int32(IntPtr env, IntPtr value, out int result);

    // napi_status napi_get_value_uint32(napi_env env, napi_value value, uint32_t *result)
    [DllImport(nameof(NodeApi), CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern napi_status napi_get_value_uint32(IntPtr env, IntPtr value, out uint result);

    // napi_status napi_get_value_int64(napi_env env, napi_value value, int64_t *result)
    [DllImport(nameof(NodeApi), CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern napi_status napi_get_value_int64(IntPtr env, IntPtr value, out long result);

    // napi_status napi_get_value_bool(napi_env env, napi_value value, bool *result)
    [DllImport(nameof(NodeApi), CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern napi_status napi_get_value_bool(IntPtr env, IntPtr value, [MarshalAs(UnmanagedType.U1)] out bool result);

    // napi_status napi_get_value_string_latin1(napi_env env, napi_value value,
    //   char* buf, size_t bufsize, size_t* result)
    [DllImport(nameof(NodeApi), CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern napi_status napi_get_value_string_latin1(IntPtr env, IntPtr value,
        sbyte* buf, nuint bufsize, nuint* result);

    // napi_status napi_get_value_string_utf8(napi_env env, napi_value value,
    //    char* buf, size_t bufsize, size_t* result)
    [DllImport(nameof(NodeApi), CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern napi_status napi_get_value_string_utf8(IntPtr env, IntPtr value,
        sbyte* buf, nuint bufsize, nuint* result);

    // napi_status napi_get_value_string_utf16(napi_env env, napi_value value,
    //    char16_t* buf, size_t bufsize, size_t* result)
    [DllImport(nameof(NodeApi), CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern napi_status napi_get_value_string_utf16(IntPtr env, IntPtr value,
        char* buf, nuint bufsize, nuint* result);

    // napi_status napi_coerce_to_bool(napi_env env, napi_value value, napi_value *result)
    [DllImport(nameof(NodeApi), CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern napi_status napi_coerce_to_bool(IntPtr env, IntPtr value,
        IntPtr result);

    // napi_status napi_coerce_to_number(napi_env env, napi_value value, napi_value *result)
    [DllImport(nameof(NodeApi), CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern napi_status napi_coerce_to_number(IntPtr env, IntPtr value,
        IntPtr result);

    // napi_status napi_coerce_to_object(napi_env env, napi_value value, napi_value *result)
    [DllImport(nameof(NodeApi), CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern napi_status napi_coerce_to_object(IntPtr env, IntPtr value,
        IntPtr result);

    // napi_status napi_coerce_to_string(napi_env env, napi_value value, napi_value *result)
    [DllImport(nameof(NodeApi), CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern napi_status napi_coerce_to_string(IntPtr env, IntPtr value,
        IntPtr result);

    // napi_status napi_get_prototype(napi_env env, napi_value object, napi_value *result)
    [DllImport(nameof(NodeApi), CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern napi_status napi_get_prototype(IntPtr env, IntPtr @object,
        IntPtr result);

    // napi_status napi_get_property_names(napi_env env, napi_value object, napi_value *result)
    [DllImport(nameof(NodeApi), CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern napi_status napi_get_property_names(IntPtr env, IntPtr @object,
        IntPtr result);

    // napi_status napi_set_property(napi_env env, napi_value object, napi_value key, napi_value value)
    [DllImport(nameof(NodeApi), CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern napi_status napi_set_property(IntPtr env, IntPtr @object, IntPtr key,
        IntPtr value);

    // napi_status napi_has_property(napi_env env, napi_value object, napi_value key, bool *result)
    [DllImport(nameof(NodeApi), CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern napi_status napi_has_property(IntPtr env, IntPtr @object, IntPtr key,
        bool* result);

    // napi_status napi_get_property(napi_env env, napi_value object, napi_value key, napi_value *result)
    [DllImport(nameof(NodeApi), CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern napi_status napi_get_property(IntPtr env, IntPtr @object, IntPtr key,
        IntPtr result);

    // napi_status napi_delete_property(napi_env env, napi_value object, napi_value key, bool *result)
    [DllImport(nameof(NodeApi), CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern napi_status napi_delete_property(IntPtr env, IntPtr @object,
        IntPtr key, bool* result);

    // napi_status napi_has_own_property(napi_env env, napi_value object, napi_value key, bool *result)
    [DllImport(nameof(NodeApi), CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern napi_status napi_has_own_property(IntPtr env, IntPtr @object,
        IntPtr key, bool* result);

    // napi_status napi_set_named_property(napi_env env, napi_value object,
    //   const char* utf8name, napi_value value)
    [DllImport(nameof(NodeApi), CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern napi_status napi_set_named_property(IntPtr env, IntPtr @object,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string utf8name, IntPtr value);

    // napi_status napi_has_named_property(napi_env env, napi_value object, const char *utf8name, bool *result)
    [DllImport(nameof(NodeApi), CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern napi_status napi_has_named_property(IntPtr env, IntPtr @object,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string utf8name, bool* result);

    // napi_status napi_get_named_property(napi_env env, napi_value object,
    //    const char* utf8name, napi_value *result)
    [DllImport(nameof(NodeApi), CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern napi_status napi_get_named_property(IntPtr env, IntPtr @object,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string utf8name, IntPtr result);

    // napi_status napi_set_element(napi_env env, napi_value object, uint32_t index, napi_value value)
    [DllImport(nameof(NodeApi), CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern napi_status napi_set_element(IntPtr env, IntPtr @object, uint index,
        IntPtr value);

    // napi_status napi_has_element(napi_env env, napi_value object, uint32_t index, bool *result)
    [DllImport(nameof(NodeApi), CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern napi_status napi_has_element(IntPtr env, IntPtr @object, uint index,
        bool* result);

    // napi_status napi_get_element(napi_env env, napi_value object, uint32_t index, napi_value *result)
    [DllImport(nameof(NodeApi), CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern napi_status napi_get_element(IntPtr env, IntPtr @object, uint index,
        IntPtr result);

    // napi_status napi_delete_element(napi_env env, napi_value object, uint32_t index, bool *result)
    [DllImport(nameof(NodeApi), CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern napi_status napi_delete_element(IntPtr env, IntPtr @object, uint index,
        bool* result);

    // napi_status napi_define_properties(napi_env env, napi_value object,
    //    size_t property_count, const napi_property_descriptor* properties)
    [DllImport(nameof(NodeApi), CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern napi_status napi_define_properties(IntPtr env, IntPtr @object,
        ulong property_count, IntPtr properties);

    // napi_status napi_is_array(napi_env env, napi_value value, bool *result)
    [DllImport(nameof(NodeApi), CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern napi_status napi_is_array(IntPtr env, IntPtr value, bool* result);

    // napi_status napi_get_array_length(napi_env env, napi_value value, uint32_t *result)
    [DllImport(nameof(NodeApi), CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern napi_status
        napi_get_array_length(IntPtr env, IntPtr value, uint* result);

    // napi_status napi_strict_equals(napi_env env, napi_value lhs, napi_value rhs, bool *result)
    [DllImport(nameof(NodeApi), CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern napi_status napi_strict_equals(IntPtr env, IntPtr lhs, IntPtr rhs,
        bool* result);

    // napi_status napi_call_function(napi_env env, napi_value recv, napi_value func,
    //   size_t argc, const napi_value* argv, napi_value *result)
    [DllImport(nameof(NodeApi), CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern napi_status napi_call_function(IntPtr env, IntPtr recv, IntPtr func,
        ulong argc, IntPtr argv, IntPtr result);

    // napi_status napi_new_instance(napi_env env, napi_value constructor,
    //    size_t argc, const napi_value* argv, napi_value *result)
    [DllImport(nameof(NodeApi), CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern napi_status napi_new_instance(IntPtr env, IntPtr constructor,
        ulong argc, IntPtr argv, IntPtr result);

    // napi_status napi_instanceof(napi_env env, napi_value object, napi_value constructor, bool *result)
    [DllImport(nameof(NodeApi), CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern napi_status napi_instanceof(IntPtr env, IntPtr @object,
        IntPtr constructor, bool* result);

    // napi_status napi_get_cb_info(
    //     napi_env env,              // [in] NAPI environment handle
    //     napi_callback_info cbinfo, // [in] Opaque callback-info handle
    //     size_t* argc,              // [in-out] Specifies the size of the provided argv array
    //                                // and receives the actual count of args.
    //     napi_value *argv,          // [out] Array of values
    //     napi_value* this_arg,      // [out] Receives the JS 'this' arg for the call
    //     void** data)               // [out] Receives the data pointer for the callback.
    [DllImport(nameof(NodeApi), CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern napi_status napi_get_cb_info(IntPtr env, IntPtr cbinfo, UIntPtr* argc,
        IntPtr argv, IntPtr this_arg, IntPtr data);

    // napi_status napi_get_new_target(napi_env env, napi_callback_info cbinfo, napi_value *result)
    [DllImport(nameof(NodeApi), CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern napi_status napi_get_new_target(IntPtr env, IntPtr cbinfo,
        IntPtr result);

    // napi_status napi_define_class(napi_env env, const char* utf8name, size_t length,
    // napi_callback constructor, void* data, size_t property_count, const napi_property_descriptor* properties, napi_value *result)
    [DllImport(nameof(NodeApi), CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern napi_status napi_define_class(IntPtr env,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string utf8name, ulong length, IntPtr constructor, void* data,
        ulong property_count,
        IntPtr properties, IntPtr result);

    // napi_status napi_wrap(napi_env env, napi_value js_object, void* native_object,
    // napi_finalize finalize_cb, void* finalize_hint, napi_ref *result)
    [DllImport(nameof(NodeApi), CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern napi_status napi_wrap(IntPtr env, IntPtr js_object,
        IntPtr native_object, IntPtr finalize_cb, IntPtr finalize_hint, IntPtr result);

    // napi_status napi_unwrap(napi_env env, napi_value js_object, void **result)
    [DllImport(nameof(NodeApi), CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern napi_status napi_unwrap(IntPtr env, IntPtr js_object, void** result);

    // napi_status napi_remove_wrap(napi_env env, napi_value js_object, void **result)
    [DllImport(nameof(NodeApi), CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern napi_status napi_remove_wrap(IntPtr env, IntPtr js_object,
        void** result);

    // napi_status napi_create_external(napi_env env, void* data,
    // napi_finalize finalize_cb, void* finalize_hint, napi_value *result)
    [DllImport(nameof(NodeApi), CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern napi_status napi_create_external(IntPtr env, IntPtr data,
        IntPtr finalize_cb, IntPtr finalize_hint, IntPtr result);

    // napi_status napi_get_value_external(napi_env env, napi_value value, void **result)
    [DllImport(nameof(NodeApi), CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern napi_status napi_get_value_external(IntPtr env, IntPtr value,
        void** result);

    // napi_status napi_create_reference(napi_env env, napi_value value,
    //  uint32_t initial_refcount, napi_ref *result)
    [DllImport(nameof(NodeApi), CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern napi_status napi_create_reference(IntPtr env, IntPtr value,
        uint initial_refcount, IntPtr result);

    // napi_status napi_delete_reference(napi_env env, napi_ref ref)
    [DllImport(nameof(NodeApi), CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern napi_status napi_delete_reference(IntPtr env, IntPtr @ref);

    // napi_status napi_reference_ref(napi_env env, napi_ref ref, uint32_t *result)
    [DllImport(nameof(NodeApi), CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern napi_status napi_reference_ref(IntPtr env, IntPtr @ref, uint* result);

    // napi_status napi_reference_unref(napi_env env, napi_ref ref, uint32_t *result)
    [DllImport(nameof(NodeApi), CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern napi_status napi_reference_unref(IntPtr env, IntPtr @ref, uint* result);

    // napi_status napi_get_reference_value(napi_env env, napi_ref ref, napi_value *result)
    [DllImport(nameof(NodeApi), CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern napi_status napi_get_reference_value(IntPtr env, IntPtr @ref,
        IntPtr result);

    // napi_status napi_open_handle_scope(napi_env env, napi_handle_scope* result)
    [DllImport(nameof(NodeApi), CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern napi_status napi_open_handle_scope(IntPtr env, IntPtr result);

    // napi_status napi_close_handle_scope(napi_env env, napi_handle_scope scope)
    [DllImport(nameof(NodeApi), CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern napi_status napi_close_handle_scope(IntPtr env, IntPtr scope);

    // napi_status napi_open_escapable_handle_scope(napi_env env, napi_escapable_handle_scope *result)
    [DllImport(nameof(NodeApi), CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern napi_status napi_open_escapable_handle_scope(IntPtr env, IntPtr result);

    // napi_status napi_close_escapable_handle_scope(napi_env env, napi_escapable_handle_scope scope)
    [DllImport(nameof(NodeApi), CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern napi_status napi_close_escapable_handle_scope(IntPtr env, IntPtr scope);

    // napi_status napi_escape_handle(napi_env env, napi_escapable_handle_scope scope,
    // napi_value escapee, napi_value *result)
    [DllImport(nameof(NodeApi), CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern napi_status napi_escape_handle(IntPtr env, IntPtr scope,
        IntPtr escapee, IntPtr result);

    // napi_status napi_throw(napi_env env, napi_value error)
    [DllImport(nameof(NodeApi), CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern napi_status napi_throw(IntPtr env, IntPtr error);

    // napi_status napi_throw_error(napi_env env, const char *code, const char *msg)
    [DllImport(nameof(NodeApi), CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern napi_status napi_throw_error(IntPtr env,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string code, [MarshalAs(UnmanagedType.LPUTF8Str)] string msg);

    // napi_status napi_throw_type_error(napi_env env, const char *code, const char *msg)
    [DllImport(nameof(NodeApi), CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern napi_status napi_throw_type_error(IntPtr env,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string code, [MarshalAs(UnmanagedType.LPUTF8Str)] string msg);

    // napi_status napi_throw_range_error(napi_env env, const char *code, const char *msg)
    [DllImport(nameof(NodeApi), CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern napi_status napi_throw_range_error(IntPtr env,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string code, [MarshalAs(UnmanagedType.LPUTF8Str)] string msg);

    // napi_status napi_is_error(napi_env env, napi_value value, bool *result)
    [DllImport(nameof(NodeApi), CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern napi_status napi_is_error(IntPtr env, IntPtr value, bool* result);

    // napi_status napi_is_exception_pending(napi_env env, bool *result)
    [DllImport(nameof(NodeApi), CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern napi_status napi_is_exception_pending(IntPtr env, bool* result);

    // napi_status napi_get_and_clear_last_exception(napi_env env, napi_value *result)
    [DllImport(nameof(NodeApi), CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern napi_status napi_get_and_clear_last_exception(IntPtr env, IntPtr result);

    // napi_status napi_is_arraybuffer(napi_env env, napi_value value, bool *result)
    [DllImport(nameof(NodeApi), CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern napi_status napi_is_arraybuffer(IntPtr env, IntPtr value, bool* result);

    // napi_status napi_create_arraybuffer(napi_env env, size_t byte_length, void **data, napi_value *result)
    [DllImport(nameof(NodeApi), CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern napi_status napi_create_arraybuffer(IntPtr env, ulong byte_length,
        void** data, IntPtr result);

    // napi_status napi_create_external_arraybuffer(napi_env env, void* external_data,
    // size_t byte_length, napi_finalize finalize_cb, void* finalize_hint, napi_value *result)
    [DllImport(nameof(NodeApi), CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern napi_status napi_create_external_arraybuffer(IntPtr env,
        IntPtr external_data, ulong byte_length, IntPtr finalize_cb, IntPtr finalize_hint, IntPtr result);

    // napi_status napi_get_arraybuffer_info(napi_env env, napi_value arraybuffer,
    // void** data, size_t *byte_length)
    [DllImport(nameof(NodeApi), CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern napi_status napi_get_arraybuffer_info(IntPtr env, IntPtr arraybuffer,
        void** data, ulong* byte_length);

    // napi_status napi_is_typedarray(napi_env env, napi_value value, bool *result)
    [DllImport(nameof(NodeApi), CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern napi_status napi_is_typedarray(IntPtr env, IntPtr value, bool* result);

    // napi_status napi_create_typedarray(napi_env env, napi_typedarray_type type,
    // size_t length, napi_value arraybuffer, size_t byte_offset, napi_value *result)
    [DllImport(nameof(NodeApi), CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern napi_status napi_create_typedarray(IntPtr env,
        napi_typedarray_type type, ulong length, IntPtr arraybuffer, ulong byte_offset,
        IntPtr result);

    // napi_status napi_get_typedarray_info(napi_env env, napi_value typedarray,
    // napi_typedarray_type* type, size_t *length, void** data, napi_value *arraybuffer, size_t* byte_offset)
    [DllImport(nameof(NodeApi), CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern napi_status napi_get_typedarray_info(IntPtr env, IntPtr typedarray,
        napi_typedarray_type* type, ulong* length, void** data, IntPtr arraybuffer,
        ulong* byte_offset);

    // napi_status napi_create_dataview(napi_env env, size_t length,
    // napi_value arraybuffer, size_t byte_offset, napi_value* result)
    [DllImport(nameof(NodeApi), CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern napi_status napi_create_dataview(IntPtr env, ulong length,
        IntPtr arraybuffer, ulong byte_offset, IntPtr result);

    // napi_status napi_is_dataview(napi_env env, napi_value value, bool *result)
    [DllImport(nameof(NodeApi), CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern napi_status napi_is_dataview(IntPtr env, IntPtr value, bool* result);

    // napi_status napi_get_dataview_info(napi_env env, napi_value dataview,
    // size_t* bytelength, void** data, napi_value *arraybuffer, size_t* byte_offset)
    [DllImport(nameof(NodeApi), CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern napi_status napi_get_dataview_info(IntPtr env, IntPtr dataview,
        ulong* bytelength, void** data, IntPtr arraybuffer, ulong* byte_offset);

    // napi_status napi_get_version(napi_env env, uint32_t *result)
    [DllImport(nameof(NodeApi), CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern napi_status napi_get_version(IntPtr env, uint* result);

    // napi_status napi_create_promise(napi_env env, napi_deferred *deferred, napi_value *promise)
    [DllImport(nameof(NodeApi), CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern napi_status napi_create_promise(IntPtr env, IntPtr deferred,
        IntPtr promise);

    // napi_status napi_resolve_deferred(napi_env env, napi_deferred deferred, napi_value resolution)
    [DllImport(nameof(NodeApi), CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern napi_status napi_resolve_deferred(IntPtr env, IntPtr deferred,
        IntPtr resolution);

    // napi_status napi_reject_deferred(napi_env env, napi_deferred deferred, napi_value rejection)
    [DllImport(nameof(NodeApi), CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern napi_status napi_reject_deferred(IntPtr env, IntPtr deferred,
        IntPtr rejection);

    // napi_status napi_is_promise(napi_env env, napi_value value, bool *is_promise)
    [DllImport(nameof(NodeApi), CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern napi_status napi_is_promise(IntPtr env, IntPtr value, bool* is_promise);

    // napi_status napi_run_script(napi_env env, napi_value script, napi_value *result)
    [DllImport(nameof(NodeApi), CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern napi_status napi_run_script(IntPtr env, IntPtr script, IntPtr result);

    // napi_status napi_adjust_external_memory(napi_env env, int64_t change_in_bytes, int64_t *adjusted_value)
    [DllImport(nameof(NodeApi), CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern napi_status napi_adjust_external_memory(IntPtr env, long change_in_bytes,
        long* adjusted_value);

    // napi_status napi_create_date(napi_env env, double time, napi_value *result)
    [DllImport(nameof(NodeApi), CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern napi_status napi_create_date(IntPtr env, double time, IntPtr result);

    // napi_status napi_is_date(napi_env env, napi_value value, bool *is_date)
    [DllImport(nameof(NodeApi), CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern napi_status napi_is_date(IntPtr env, IntPtr value, bool* is_date);

    // napi_status napi_get_date_value(napi_env env, napi_value value, double *result)
    [DllImport(nameof(NodeApi), CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern napi_status
        napi_get_date_value(IntPtr env, IntPtr value, double* result);

    // napi_status napi_add_finalizer(napi_env env, napi_value js_object,
    // void* native_object, napi_finalize finalize_cb, void* finalize_hint, napi_ref *result)
    [DllImport(nameof(NodeApi), CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern napi_status napi_add_finalizer(IntPtr env, IntPtr js_object,
        IntPtr native_object, delegate* unmanaged[Cdecl]<IntPtr, IntPtr, IntPtr, void> finalize_cb, IntPtr finalize_hint, IntPtr result);

    // napi_status napi_create_bigint_int64(napi_env env, int64_t value, napi_value *result)
    [DllImport(nameof(NodeApi), CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern napi_status napi_create_bigint_int64(IntPtr env, long value,
        IntPtr result);

    // napi_status napi_create_bigint_uint64(napi_env env, uint64_t value, napi_value *result)
    [DllImport(nameof(NodeApi), CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern napi_status napi_create_bigint_uint64(IntPtr env, ulong value,
        IntPtr result);

    // napi_status napi_create_bigint_words(napi_env env,int sign_bit, size_t word_count,
    // const uint64_t* words, napi_value *result)
    [DllImport(nameof(NodeApi), CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern napi_status napi_create_bigint_words(IntPtr env, int sign_bit,
        ulong word_count, ulong* words, IntPtr result);

    // napi_status napi_get_value_bigint_int64(napi_env env, napi_value value, int64_t *result, bool *lossless)
    [DllImport(nameof(NodeApi), CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern napi_status napi_get_value_bigint_int64(IntPtr env, IntPtr value,
        long* result, bool* lossless);

    // napi_status napi_get_value_bigint_uint64(napi_env env, napi_value value, uint64_t *result, bool *lossless)
    [DllImport(nameof(NodeApi), CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern napi_status napi_get_value_bigint_uint64(IntPtr env, IntPtr value,
        ulong* result, bool* lossless);

    // napi_status napi_get_value_bigint_words(napi_env env, napi_value value, int* sign_bit,
    // size_t* word_count, uint64_t* words)
    [DllImport(nameof(NodeApi), CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern napi_status napi_get_value_bigint_words(IntPtr env, IntPtr value,
        int* sign_bit, ulong* word_count, ulong* words);

    // napi_status napi_get_all_property_names(napi_env env, napi_value object,
    // napi_key_collection_mode key_mode, napi_key_filter key_filter,
    // napi_key_conversion key_conversion, napi_value *result)
    [DllImport(nameof(NodeApi), CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern napi_status napi_get_all_property_names(IntPtr env, IntPtr @object,
        napi_key_collection_mode key_mode, napi_key_filter key_filter,
        napi_key_conversion key_conversion, IntPtr result);

    // napi_status napi_set_instance_data(napi_env env, void* data,
    // napi_finalize finalize_cb, void* finalize_hint)
    [DllImport(nameof(NodeApi), CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern napi_status napi_set_instance_data(IntPtr env, IntPtr data,
        IntPtr finalize_cb, IntPtr finalize_hint);

    // napi_status napi_get_instance_data(napi_env env, void **data)
    [DllImport(nameof(NodeApi), CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern napi_status napi_get_instance_data(IntPtr env, void** data);

    // napi_status napi_detach_arraybuffer(napi_env env, napi_value arraybuffer)
    [DllImport(nameof(NodeApi), CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern napi_status napi_detach_arraybuffer(IntPtr env, IntPtr arraybuffer);

    // napi_status napi_is_detached_arraybuffer(napi_env env, napi_value value, bool *result)
    [DllImport(nameof(NodeApi), CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern napi_status napi_is_detached_arraybuffer(IntPtr env, IntPtr value,
        bool* result);

    // napi_status napi_type_tag_object(napi_env env, napi_value value, const napi_type_tag *type_tag)
    [DllImport(nameof(NodeApi), CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern napi_status napi_type_tag_object(IntPtr env, IntPtr value,
        IntPtr type_tag);

    // napi_status napi_check_object_type_tag(napi_env env, napi_value value,
    // const napi_type_tag* type_tag, bool* result)
    [DllImport(nameof(NodeApi), CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern napi_status napi_check_object_type_tag(IntPtr env, IntPtr value,
        IntPtr type_tag, bool* result);

    // napi_status napi_object_freeze(napi_env env, napi_value object)
    [DllImport(nameof(NodeApi), CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern napi_status napi_object_freeze(IntPtr env, IntPtr @object);

    // napi_status napi_object_seal(napi_env env, napi_value object)
    [DllImport(nameof(NodeApi), CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern napi_status napi_object_seal(IntPtr env, IntPtr @object);
  }
}