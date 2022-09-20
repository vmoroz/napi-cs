using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;

namespace NApi
{
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

    [DllImport(nameof(NodeApi), EntryPoint = "napi_get_last_error_info", CallingConvention = CallingConvention.Cdecl)]
    internal static extern napi_status napi_get_last_error_info(IntPtr env, IntPtr result);

    [DllImport(nameof(NodeApi), EntryPoint = "napi_get_undefined", CallingConvention = CallingConvention.Cdecl)]
    internal static extern napi_status napi_get_undefined(IntPtr env, IntPtr result);

    [DllImport(nameof(NodeApi), EntryPoint = "napi_get_null", CallingConvention = CallingConvention.Cdecl)]
    internal static extern napi_status napi_get_null(IntPtr env, IntPtr result);

    [DllImport(nameof(NodeApi), EntryPoint = "napi_get_global", CallingConvention = CallingConvention.Cdecl)]
    internal static extern napi_status napi_get_global(IntPtr env, IntPtr result);

    [DllImport(nameof(NodeApi), EntryPoint = "napi_get_boolean", CallingConvention = CallingConvention.Cdecl)]
    internal static extern napi_status napi_get_boolean(IntPtr env, bool value, IntPtr result);

    [DllImport(nameof(NodeApi), EntryPoint = "napi_create_object", CallingConvention = CallingConvention.Cdecl)]
    internal static extern napi_status napi_create_object(IntPtr env, IntPtr result);

    [DllImport(nameof(NodeApi), EntryPoint = "napi_create_array", CallingConvention = CallingConvention.Cdecl)]
    internal static extern napi_status napi_create_array(IntPtr env, IntPtr result);

    [DllImport(nameof(NodeApi), EntryPoint = "napi_create_array_with_length", CallingConvention = CallingConvention.Cdecl)]
    internal static extern napi_status napi_create_array_with_length(IntPtr env, UIntPtr length,
        IntPtr result);

    [DllImport(nameof(NodeApi), EntryPoint = "napi_create_double", CallingConvention = CallingConvention.Cdecl)]
    internal static extern napi_status napi_create_double(IntPtr env, double value, IntPtr result);

    [DllImport(nameof(NodeApi), EntryPoint = "napi_create_int32", CallingConvention = CallingConvention.Cdecl)]
    internal static extern napi_status napi_create_int32(IntPtr env, int value, IntPtr result);

    [DllImport(nameof(NodeApi), EntryPoint = "napi_create_uint32", CallingConvention = CallingConvention.Cdecl)]
    internal static extern napi_status napi_create_uint32(IntPtr env, uint value, IntPtr result);

    [DllImport(nameof(NodeApi), EntryPoint = "napi_create_int64", CallingConvention = CallingConvention.Cdecl)]
    internal static extern napi_status napi_create_int64(IntPtr env, long value, IntPtr result);

    [DllImport(nameof(NodeApi), EntryPoint = "napi_create_string_latin1", CallingConvention = CallingConvention.Cdecl)]
    internal static extern napi_status napi_create_string_latin1(IntPtr env, void* str, UIntPtr length, IntPtr result);

    [DllImport(nameof(NodeApi), EntryPoint = "napi_create_string_utf8", CallingConvention = CallingConvention.Cdecl)]
    internal static extern napi_status napi_create_string_utf8(IntPtr env, void* str, UIntPtr length, IntPtr result);

    [DllImport(nameof(NodeApi), EntryPoint = "napi_create_string_utf16", CallingConvention = CallingConvention.Cdecl)]
    internal static extern napi_status napi_create_string_utf16(IntPtr env, void* str, UIntPtr length, IntPtr result);

    [DllImport(nameof(NodeApi), EntryPoint = "napi_create_symbol", CallingConvention = CallingConvention.Cdecl)]
    internal static extern napi_status napi_create_symbol(IntPtr env, IntPtr description, IntPtr result);

    [DllImport(nameof(NodeApi), EntryPoint = "napi_create_function", CallingConvention = CallingConvention.Cdecl)]
    internal static extern napi_status napi_create_function(IntPtr env,
        void* utf8name, UIntPtr length, delegate* unmanaged[Cdecl]<IntPtr, IntPtr, IntPtr> cb, IntPtr data, IntPtr result);

    [DllImport(nameof(NodeApi), EntryPoint = "napi_create_error", CallingConvention = CallingConvention.Cdecl)]
    internal static extern napi_status napi_create_error(IntPtr env, IntPtr code, IntPtr msg,
        IntPtr result);

    [DllImport(nameof(NodeApi), EntryPoint = "napi_create_type_error", CallingConvention = CallingConvention.Cdecl)]
    internal static extern napi_status napi_create_type_error(IntPtr env, IntPtr code, IntPtr msg,
        IntPtr result);

    [DllImport(nameof(NodeApi), EntryPoint = "napi_create_range_error", CallingConvention = CallingConvention.Cdecl)]
    internal static extern napi_status napi_create_range_error(IntPtr env, IntPtr code,
        IntPtr msg, IntPtr result);

    [DllImport(nameof(NodeApi), EntryPoint = nameof(napi_typeof), CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern napi_status napi_typeof(IntPtr env, IntPtr value, JSValueType* result);

    [DllImport(nameof(NodeApi), EntryPoint = nameof(napi_get_value_double), CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern napi_status napi_get_value_double(IntPtr env, IntPtr value, out double result);

    [DllImport(nameof(NodeApi), EntryPoint = nameof(napi_get_value_int32), CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern napi_status napi_get_value_int32(IntPtr env, IntPtr value, out int result);

    [DllImport(nameof(NodeApi), EntryPoint = nameof(napi_get_value_uint32), CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern napi_status napi_get_value_uint32(IntPtr env, IntPtr value, out uint result);

    [DllImport(nameof(NodeApi), EntryPoint = nameof(napi_get_value_int64), CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern napi_status napi_get_value_int64(IntPtr env, IntPtr value, out long result);

    [DllImport(nameof(NodeApi), EntryPoint = nameof(napi_get_value_bool), CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern napi_status napi_get_value_bool(IntPtr env, IntPtr value, [MarshalAs(UnmanagedType.U1)] out bool result);

    [DllImport(nameof(NodeApi), EntryPoint = nameof(napi_get_value_string_latin1), CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern napi_status napi_get_value_string_latin1(IntPtr env, IntPtr value,
        sbyte* buf, nuint bufsize, nuint* result);

    [DllImport(nameof(NodeApi), EntryPoint = nameof(napi_get_value_string_utf8), CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern napi_status napi_get_value_string_utf8(IntPtr env, IntPtr value,
        sbyte* buf, nuint bufsize, nuint* result);

    [DllImport(nameof(NodeApi), EntryPoint = nameof(napi_get_value_string_utf16), CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern napi_status napi_get_value_string_utf16(IntPtr env, IntPtr value,
        char* buf, nuint bufsize, nuint* result);

    [DllImport(nameof(NodeApi), EntryPoint = "napi_coerce_to_bool", CallingConvention = CallingConvention.Cdecl)]
    internal static extern napi_status napi_coerce_to_bool(IntPtr env, IntPtr value,
        IntPtr result);

    [DllImport(nameof(NodeApi), EntryPoint = "napi_coerce_to_number", CallingConvention = CallingConvention.Cdecl)]
    internal static extern napi_status napi_coerce_to_number(IntPtr env, IntPtr value,
        IntPtr result);

    [DllImport(nameof(NodeApi), EntryPoint = "napi_coerce_to_object", CallingConvention = CallingConvention.Cdecl)]
    internal static extern napi_status napi_coerce_to_object(IntPtr env, IntPtr value,
        IntPtr result);

    [DllImport(nameof(NodeApi), EntryPoint = "napi_coerce_to_string", CallingConvention = CallingConvention.Cdecl)]
    internal static extern napi_status napi_coerce_to_string(IntPtr env, IntPtr value,
        IntPtr result);

    [DllImport(nameof(NodeApi), EntryPoint = "napi_get_prototype", CallingConvention = CallingConvention.Cdecl)]
    internal static extern napi_status napi_get_prototype(IntPtr env, IntPtr @object,
        IntPtr result);

    [DllImport(nameof(NodeApi), EntryPoint = "napi_get_property_names", CallingConvention = CallingConvention.Cdecl)]
    internal static extern napi_status napi_get_property_names(IntPtr env, IntPtr @object,
        IntPtr result);

    [DllImport(nameof(NodeApi), EntryPoint = "napi_set_property", CallingConvention = CallingConvention.Cdecl)]
    internal static extern napi_status napi_set_property(IntPtr env, IntPtr @object, IntPtr key,
        IntPtr value);

    [DllImport(nameof(NodeApi), EntryPoint = "napi_has_property", CallingConvention = CallingConvention.Cdecl)]
    internal static extern napi_status napi_has_property(IntPtr env, IntPtr @object, IntPtr key,
        bool* result);

    [DllImport(nameof(NodeApi), EntryPoint = "napi_get_property", CallingConvention = CallingConvention.Cdecl)]
    internal static extern napi_status napi_get_property(IntPtr env, IntPtr @object, IntPtr key,
        IntPtr result);

    [DllImport(nameof(NodeApi), EntryPoint = "napi_delete_property", CallingConvention = CallingConvention.Cdecl)]
    internal static extern napi_status napi_delete_property(IntPtr env, IntPtr @object,
        IntPtr key, bool* result);

    [DllImport(nameof(NodeApi), EntryPoint = "napi_has_own_property", CallingConvention = CallingConvention.Cdecl)]
    internal static extern napi_status napi_has_own_property(IntPtr env, IntPtr @object,
        IntPtr key, bool* result);

    [DllImport(nameof(NodeApi), EntryPoint = "napi_set_named_property", CallingConvention = CallingConvention.Cdecl)]
    internal static extern napi_status napi_set_named_property(IntPtr env, IntPtr @object,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string utf8name, IntPtr value);

    [DllImport(nameof(NodeApi), EntryPoint = "napi_has_named_property", CallingConvention = CallingConvention.Cdecl)]
    internal static extern napi_status napi_has_named_property(IntPtr env, IntPtr @object,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string utf8name, bool* result);

    [DllImport(nameof(NodeApi), EntryPoint = "napi_get_named_property", CallingConvention = CallingConvention.Cdecl)]
    internal static extern napi_status napi_get_named_property(IntPtr env, IntPtr @object,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string utf8name, IntPtr result);

    [DllImport(nameof(NodeApi), EntryPoint = "napi_set_element", CallingConvention = CallingConvention.Cdecl)]
    internal static extern napi_status napi_set_element(IntPtr env, IntPtr @object, uint index,
        IntPtr value);

    [DllImport(nameof(NodeApi), EntryPoint = "napi_has_element", CallingConvention = CallingConvention.Cdecl)]
    internal static extern napi_status napi_has_element(IntPtr env, IntPtr @object, uint index,
        bool* result);

    [DllImport(nameof(NodeApi), EntryPoint = "napi_get_element", CallingConvention = CallingConvention.Cdecl)]
    internal static extern napi_status napi_get_element(IntPtr env, IntPtr @object, uint index,
        IntPtr result);

    [DllImport(nameof(NodeApi), EntryPoint = "napi_delete_element", CallingConvention = CallingConvention.Cdecl)]
    internal static extern napi_status napi_delete_element(IntPtr env, IntPtr @object, uint index,
        bool* result);

    [DllImport(nameof(NodeApi), EntryPoint = "napi_define_properties", CallingConvention = CallingConvention.Cdecl)]
    internal static extern napi_status napi_define_properties(IntPtr env, IntPtr @object,
        ulong property_count, IntPtr properties);

    [DllImport(nameof(NodeApi), EntryPoint = "napi_is_array", CallingConvention = CallingConvention.Cdecl)]
    internal static extern napi_status napi_is_array(IntPtr env, IntPtr value, bool* result);

    [DllImport(nameof(NodeApi), EntryPoint = "napi_get_array_length", CallingConvention = CallingConvention.Cdecl)]
    internal static extern napi_status
        napi_get_array_length(IntPtr env, IntPtr value, uint* result);

    [DllImport(nameof(NodeApi), EntryPoint = "napi_strict_equals", CallingConvention = CallingConvention.Cdecl)]
    internal static extern napi_status napi_strict_equals(IntPtr env, IntPtr lhs, IntPtr rhs,
        bool* result);

    [DllImport(nameof(NodeApi), EntryPoint = "napi_call_function", CallingConvention = CallingConvention.Cdecl)]
    internal static extern napi_status napi_call_function(IntPtr env, IntPtr recv, IntPtr func,
        ulong argc, IntPtr argv, IntPtr result);

    [DllImport(nameof(NodeApi), EntryPoint = "napi_new_instance", CallingConvention = CallingConvention.Cdecl)]
    internal static extern napi_status napi_new_instance(IntPtr env, IntPtr constructor,
        ulong argc, IntPtr argv, IntPtr result);

    [DllImport(nameof(NodeApi), EntryPoint = "napi_instanceof", CallingConvention = CallingConvention.Cdecl)]
    internal static extern napi_status napi_instanceof(IntPtr env, IntPtr @object,
        IntPtr constructor, bool* result);

    [DllImport(nameof(NodeApi), EntryPoint = "napi_get_cb_info", CallingConvention = CallingConvention.Cdecl)]
    internal static extern napi_status napi_get_cb_info(IntPtr env, IntPtr cbinfo, UIntPtr* argc,
        IntPtr argv, IntPtr this_arg, IntPtr data);

    [DllImport(nameof(NodeApi), EntryPoint = "napi_get_new_target", CallingConvention = CallingConvention.Cdecl)]
    internal static extern napi_status napi_get_new_target(IntPtr env, IntPtr cbinfo,
        IntPtr result);

    [DllImport(nameof(NodeApi), EntryPoint = "napi_define_class", CallingConvention = CallingConvention.Cdecl)]
    internal static extern napi_status napi_define_class(IntPtr env,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string utf8name, ulong length, IntPtr constructor, void* data,
        ulong property_count,
        IntPtr properties, IntPtr result);

    [DllImport(nameof(NodeApi), EntryPoint = "napi_wrap", CallingConvention = CallingConvention.Cdecl)]
    internal static extern napi_status napi_wrap(IntPtr env, IntPtr js_object,
        IntPtr native_object, IntPtr finalize_cb, IntPtr finalize_hint, IntPtr result);

    [DllImport(nameof(NodeApi), EntryPoint = "napi_unwrap", CallingConvention = CallingConvention.Cdecl)]
    internal static extern napi_status napi_unwrap(IntPtr env, IntPtr js_object, void** result);

    [DllImport(nameof(NodeApi), EntryPoint = "napi_remove_wrap", CallingConvention = CallingConvention.Cdecl)]
    internal static extern napi_status napi_remove_wrap(IntPtr env, IntPtr js_object,
        void** result);

    [DllImport(nameof(NodeApi), EntryPoint = "napi_create_external", CallingConvention = CallingConvention.Cdecl)]
    internal static extern napi_status napi_create_external(IntPtr env, IntPtr data,
        IntPtr finalize_cb, IntPtr finalize_hint, IntPtr result);

    [DllImport(nameof(NodeApi), EntryPoint = "napi_get_value_external", CallingConvention = CallingConvention.Cdecl)]
    internal static extern napi_status napi_get_value_external(IntPtr env, IntPtr value,
        void** result);

    [DllImport(nameof(NodeApi), EntryPoint = "napi_create_reference", CallingConvention = CallingConvention.Cdecl)]
    internal static extern napi_status napi_create_reference(IntPtr env, IntPtr value,
        uint initial_refcount, IntPtr result);

    [DllImport(nameof(NodeApi), EntryPoint = "napi_delete_reference", CallingConvention = CallingConvention.Cdecl)]
    internal static extern napi_status napi_delete_reference(IntPtr env, IntPtr @ref);

    [DllImport(nameof(NodeApi), EntryPoint = "napi_reference_ref", CallingConvention = CallingConvention.Cdecl)]
    internal static extern napi_status napi_reference_ref(IntPtr env, IntPtr @ref, uint* result);

    [DllImport(nameof(NodeApi), EntryPoint = "napi_reference_unref", CallingConvention = CallingConvention.Cdecl)]
    internal static extern napi_status napi_reference_unref(IntPtr env, IntPtr @ref, uint* result);

    [DllImport(nameof(NodeApi), EntryPoint = "napi_get_reference_value", CallingConvention = CallingConvention.Cdecl)]
    internal static extern napi_status napi_get_reference_value(IntPtr env, IntPtr @ref,
        IntPtr result);

    [DllImport(nameof(NodeApi), EntryPoint = "napi_open_handle_scope", CallingConvention = CallingConvention.Cdecl)]
    internal static extern napi_status napi_open_handle_scope(IntPtr env, IntPtr result);

    [DllImport(nameof(NodeApi), EntryPoint = "napi_close_handle_scope", CallingConvention = CallingConvention.Cdecl)]
    internal static extern napi_status napi_close_handle_scope(IntPtr env, IntPtr scope);

    [DllImport(nameof(NodeApi), EntryPoint = "napi_open_escapable_handle_scope",
         CallingConvention = CallingConvention.Cdecl)]
    internal static extern napi_status napi_open_escapable_handle_scope(IntPtr env, IntPtr result);

    [DllImport(nameof(NodeApi), EntryPoint = "napi_close_escapable_handle_scope",
         CallingConvention = CallingConvention.Cdecl)]
    internal static extern napi_status napi_close_escapable_handle_scope(IntPtr env, IntPtr scope);

    [DllImport(nameof(NodeApi), EntryPoint = "napi_escape_handle", CallingConvention = CallingConvention.Cdecl)]
    internal static extern napi_status napi_escape_handle(IntPtr env, IntPtr scope,
        IntPtr escapee, IntPtr result);

    [DllImport(nameof(NodeApi), EntryPoint = "napi_throw", CallingConvention = CallingConvention.Cdecl)]
    internal static extern napi_status napi_throw(IntPtr env, IntPtr error);

    [DllImport(nameof(NodeApi), EntryPoint = "napi_throw_error", CallingConvention = CallingConvention.Cdecl)]
    internal static extern napi_status napi_throw_error(IntPtr env,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string code, [MarshalAs(UnmanagedType.LPUTF8Str)] string msg);

    [DllImport(nameof(NodeApi), EntryPoint = "napi_throw_type_error", CallingConvention = CallingConvention.Cdecl)]
    internal static extern napi_status napi_throw_type_error(IntPtr env,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string code, [MarshalAs(UnmanagedType.LPUTF8Str)] string msg);

    [DllImport(nameof(NodeApi), EntryPoint = "napi_throw_range_error", CallingConvention = CallingConvention.Cdecl)]
    internal static extern napi_status napi_throw_range_error(IntPtr env,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string code, [MarshalAs(UnmanagedType.LPUTF8Str)] string msg);

    [DllImport(nameof(NodeApi), EntryPoint = "napi_is_error", CallingConvention = CallingConvention.Cdecl)]
    internal static extern napi_status napi_is_error(IntPtr env, IntPtr value, bool* result);

    [DllImport(nameof(NodeApi), EntryPoint = "napi_is_exception_pending", CallingConvention = CallingConvention.Cdecl)]
    internal static extern napi_status napi_is_exception_pending(IntPtr env, bool* result);

    [DllImport(nameof(NodeApi), EntryPoint = "napi_get_and_clear_last_exception",
         CallingConvention = CallingConvention.Cdecl)]
    internal static extern napi_status napi_get_and_clear_last_exception(IntPtr env, IntPtr result);

    [DllImport(nameof(NodeApi), EntryPoint = "napi_is_arraybuffer", CallingConvention = CallingConvention.Cdecl)]
    internal static extern napi_status napi_is_arraybuffer(IntPtr env, IntPtr value, bool* result);

    [DllImport(nameof(NodeApi), EntryPoint = "napi_create_arraybuffer", CallingConvention = CallingConvention.Cdecl)]
    internal static extern napi_status napi_create_arraybuffer(IntPtr env, ulong byte_length,
        void** data, IntPtr result);

    [DllImport(nameof(NodeApi), EntryPoint = "napi_create_external_arraybuffer", CallingConvention = CallingConvention.Cdecl)]
    internal static extern napi_status napi_create_external_arraybuffer(IntPtr env,
        IntPtr external_data, ulong byte_length, IntPtr finalize_cb, IntPtr finalize_hint, IntPtr result);

    [DllImport(nameof(NodeApi), EntryPoint = "napi_get_arraybuffer_info", CallingConvention = CallingConvention.Cdecl)]
    internal static extern napi_status napi_get_arraybuffer_info(IntPtr env, IntPtr arraybuffer,
        void** data, ulong* byte_length);

    [DllImport(nameof(NodeApi), EntryPoint = "napi_is_typedarray", CallingConvention = CallingConvention.Cdecl)]
    internal static extern napi_status napi_is_typedarray(IntPtr env, IntPtr value, bool* result);

    [DllImport(nameof(NodeApi), EntryPoint = "napi_create_typedarray", CallingConvention = CallingConvention.Cdecl)]
    internal static extern napi_status napi_create_typedarray(IntPtr env,
        napi_typedarray_type type, ulong length, IntPtr arraybuffer, ulong byte_offset,
        IntPtr result);

    [DllImport(nameof(NodeApi), EntryPoint = "napi_get_typedarray_info", CallingConvention = CallingConvention.Cdecl)]
    internal static extern napi_status napi_get_typedarray_info(IntPtr env, IntPtr typedarray,
        napi_typedarray_type* type, ulong* length, void** data, IntPtr arraybuffer,
        ulong* byte_offset);

    [DllImport(nameof(NodeApi), EntryPoint = "napi_create_dataview", CallingConvention = CallingConvention.Cdecl)]
    internal static extern napi_status napi_create_dataview(IntPtr env, ulong length,
        IntPtr arraybuffer, ulong byte_offset, IntPtr result);

    [DllImport(nameof(NodeApi), EntryPoint = "napi_is_dataview", CallingConvention = CallingConvention.Cdecl)]
    internal static extern napi_status napi_is_dataview(IntPtr env, IntPtr value, bool* result);

    [DllImport(nameof(NodeApi), EntryPoint = "napi_get_dataview_info", CallingConvention = CallingConvention.Cdecl)]
    internal static extern napi_status napi_get_dataview_info(IntPtr env, IntPtr dataview,
        ulong* bytelength, void** data, IntPtr arraybuffer, ulong* byte_offset);

    [DllImport(nameof(NodeApi), EntryPoint = "napi_get_version", CallingConvention = CallingConvention.Cdecl)]
    internal static extern napi_status napi_get_version(IntPtr env, uint* result);

    [DllImport(nameof(NodeApi), EntryPoint = "napi_create_promise", CallingConvention = CallingConvention.Cdecl)]
    internal static extern napi_status napi_create_promise(IntPtr env, IntPtr deferred,
        IntPtr promise);

    [DllImport(nameof(NodeApi), EntryPoint = "napi_resolve_deferred", CallingConvention = CallingConvention.Cdecl)]
    internal static extern napi_status napi_resolve_deferred(IntPtr env, IntPtr deferred,
        IntPtr resolution);

    [DllImport(nameof(NodeApi), EntryPoint = "napi_reject_deferred", CallingConvention = CallingConvention.Cdecl)]
    internal static extern napi_status napi_reject_deferred(IntPtr env, IntPtr deferred,
        IntPtr rejection);

    [DllImport(nameof(NodeApi), EntryPoint = "napi_is_promise", CallingConvention = CallingConvention.Cdecl)]
    internal static extern napi_status napi_is_promise(IntPtr env, IntPtr value, bool* is_promise);

    [DllImport(nameof(NodeApi), EntryPoint = "napi_run_script", CallingConvention = CallingConvention.Cdecl)]
    internal static extern napi_status napi_run_script(IntPtr env, IntPtr script, IntPtr result);

    [DllImport(nameof(NodeApi), EntryPoint = "napi_adjust_external_memory", CallingConvention = CallingConvention.Cdecl)]
    internal static extern napi_status napi_adjust_external_memory(IntPtr env, long change_in_bytes,
        long* adjusted_value);

    [DllImport(nameof(NodeApi), EntryPoint = "napi_create_date", CallingConvention = CallingConvention.Cdecl)]
    internal static extern napi_status napi_create_date(IntPtr env, double time, IntPtr result);

    [DllImport(nameof(NodeApi), EntryPoint = "napi_is_date", CallingConvention = CallingConvention.Cdecl)]
    internal static extern napi_status napi_is_date(IntPtr env, IntPtr value, bool* is_date);

    [DllImport(nameof(NodeApi), EntryPoint = "napi_get_date_value", CallingConvention = CallingConvention.Cdecl)]
    internal static extern napi_status
        napi_get_date_value(IntPtr env, IntPtr value, double* result);

    [DllImport(nameof(NodeApi), EntryPoint = "napi_add_finalizer", CallingConvention = CallingConvention.Cdecl)]
    internal static extern napi_status napi_add_finalizer(IntPtr env, IntPtr js_object,
        IntPtr native_object, delegate* unmanaged[Cdecl]<IntPtr, IntPtr, IntPtr, void> finalize_cb, IntPtr finalize_hint, IntPtr result);

    [DllImport(nameof(NodeApi), EntryPoint = "napi_create_bigint_int64", CallingConvention = CallingConvention.Cdecl)]
    internal static extern napi_status napi_create_bigint_int64(IntPtr env, long value,
        IntPtr result);

    [DllImport(nameof(NodeApi), EntryPoint = "napi_create_bigint_uint64", CallingConvention = CallingConvention.Cdecl)]
    internal static extern napi_status napi_create_bigint_uint64(IntPtr env, ulong value,
        IntPtr result);

    [DllImport(nameof(NodeApi), EntryPoint = "napi_create_bigint_words", CallingConvention = CallingConvention.Cdecl)]
    internal static extern napi_status napi_create_bigint_words(IntPtr env, int sign_bit,
        ulong word_count, ulong* words, IntPtr result);

    [DllImport(nameof(NodeApi), EntryPoint = "napi_get_value_bigint_int64", CallingConvention = CallingConvention.Cdecl)]
    internal static extern napi_status napi_get_value_bigint_int64(IntPtr env, IntPtr value,
        long* result, bool* lossless);

    [DllImport(nameof(NodeApi), EntryPoint = "napi_get_value_bigint_uint64", CallingConvention = CallingConvention.Cdecl)]
    internal static extern napi_status napi_get_value_bigint_uint64(IntPtr env, IntPtr value,
        ulong* result, bool* lossless);

    [DllImport(nameof(NodeApi), EntryPoint = "napi_get_value_bigint_words", CallingConvention = CallingConvention.Cdecl)]
    internal static extern napi_status napi_get_value_bigint_words(IntPtr env, IntPtr value,
        int* sign_bit, ulong* word_count, ulong* words);

    [DllImport(nameof(NodeApi), EntryPoint = "napi_get_all_property_names", CallingConvention = CallingConvention.Cdecl)]
    internal static extern napi_status napi_get_all_property_names(IntPtr env, IntPtr @object,
        napi_key_collection_mode key_mode, napi_key_filter key_filter,
        napi_key_conversion key_conversion, IntPtr result);

    [DllImport(nameof(NodeApi), EntryPoint = "napi_set_instance_data", CallingConvention = CallingConvention.Cdecl)]
    internal static extern napi_status napi_set_instance_data(IntPtr env, IntPtr data,
        IntPtr finalize_cb, IntPtr finalize_hint);

    [DllImport(nameof(NodeApi), EntryPoint = "napi_get_instance_data", CallingConvention = CallingConvention.Cdecl)]
    internal static extern napi_status napi_get_instance_data(IntPtr env, void** data);

    [DllImport(nameof(NodeApi), EntryPoint = "napi_detach_arraybuffer", CallingConvention = CallingConvention.Cdecl)]
    internal static extern napi_status napi_detach_arraybuffer(IntPtr env, IntPtr arraybuffer);

    [DllImport(nameof(NodeApi), EntryPoint = "napi_is_detached_arraybuffer", CallingConvention = CallingConvention.Cdecl)]
    internal static extern napi_status napi_is_detached_arraybuffer(IntPtr env, IntPtr value,
        bool* result);

    [DllImport(nameof(NodeApi), EntryPoint = "napi_type_tag_object", CallingConvention = CallingConvention.Cdecl)]
    internal static extern napi_status napi_type_tag_object(IntPtr env, IntPtr value,
        IntPtr type_tag);

    [DllImport(nameof(NodeApi), EntryPoint = "napi_check_object_type_tag", CallingConvention = CallingConvention.Cdecl)]
    internal static extern napi_status napi_check_object_type_tag(IntPtr env, IntPtr value,
        IntPtr type_tag, bool* result);

    [DllImport(nameof(NodeApi), EntryPoint = "napi_object_freeze", CallingConvention = CallingConvention.Cdecl)]
    internal static extern napi_status napi_object_freeze(IntPtr env, IntPtr @object);

    [DllImport(nameof(NodeApi), EntryPoint = "napi_object_seal", CallingConvention = CallingConvention.Cdecl)]
    internal static extern napi_status napi_object_seal(IntPtr env, IntPtr @object);
  }
}