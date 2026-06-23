// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Storage.Json;

/// <summary>
///     <para>
///         Attempts to find a <see cref="JsonValueReaderWriter" /> for a given CLR type.
///     </para>
/// </summary>
/// <remarks>
///     <para>
///         The service lifetime is <see cref="ServiceLifetime.Singleton" />. This means a single instance
///         is used by many <see cref="DbContext" /> instances. The implementation must be thread-safe.
///         This service cannot depend on services registered as <see cref="ServiceLifetime.Scoped" />.
///     </para>
///     <para>
///         See <see href="https://aka.ms/efcore-docs-providers">Implementation of database providers and extensions</see>
///         for more information and examples.
///     </para>
/// </remarks>
public class JsonValueReaderWriterSource : IJsonValueReaderWriterSource
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="JsonValueReaderWriterSource" /> class.
    /// </summary>
    /// <param name="dependencies">Parameter object containing dependencies for this service.</param>
    public JsonValueReaderWriterSource(JsonValueReaderWriterSourceDependencies dependencies)
        => Dependencies = dependencies;

    /// <summary>
    ///     Dependencies for this service.
    /// </summary>
    protected virtual JsonValueReaderWriterSourceDependencies Dependencies { get; }

    private static readonly MethodInfo FindReaderWriterMethod
        = typeof(JsonValueReaderWriterSource).GetMethod(
            nameof(FindReaderWriter), genericParameterCount: 1, BindingFlags.Public | BindingFlags.Static, Type.EmptyTypes)!;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public static JsonValueReaderWriter? FindReaderWriter<T>()
        => typeof(T) switch
        {
            var t when t == typeof(int) => JsonInt32ReaderWriter.Instance,
            var t when t == typeof(string) => JsonStringReaderWriter.Instance,
            var t when t == typeof(Guid) => JsonGuidReaderWriter.Instance,
            var t when t == typeof(bool) => JsonBoolReaderWriter.Instance,
            var t when t == typeof(DateTime) => JsonDateTimeReaderWriter.Instance,
            var t when t == typeof(DateTimeOffset) => JsonDateTimeOffsetReaderWriter.Instance,
            var t when t == typeof(decimal) => JsonDecimalReaderWriter.Instance,
            var t when t == typeof(double) => JsonDoubleReaderWriter.Instance,
            var t when t == typeof(long) => JsonInt64ReaderWriter.Instance,
            var t when t == typeof(DateOnly) => JsonDateOnlyReaderWriter.Instance,
            var t when t == typeof(TimeOnly) => JsonTimeOnlyReaderWriter.Instance,
            var t when t == typeof(byte[]) => JsonByteArrayReaderWriter.Instance,
            var t when t == typeof(ulong) => JsonUInt64ReaderWriter.Instance,
            var t when t == typeof(uint) => JsonUInt32ReaderWriter.Instance,
            var t when t == typeof(byte) => JsonByteReaderWriter.Instance,
            var t when t == typeof(char) => JsonCharReaderWriter.Instance,
            var t when t == typeof(float) => JsonFloatReaderWriter.Instance,
            var t when t == typeof(short) => JsonInt16ReaderWriter.Instance,
            var t when t == typeof(sbyte) => JsonSByteReaderWriter.Instance,
            var t when t == typeof(ushort) => JsonUInt16ReaderWriter.Instance,
            var t when t == typeof(TimeSpan) => JsonTimeSpanReaderWriter.Instance,
            var t when t.IsEnum => typeof(T).GetEnumUnderlyingType().IsSignedInteger()
                ? JsonSignedEnumReaderWriter<T>.Instance
                : JsonUnsignedEnumReaderWriter<T>.Instance,
            _ => null
        };

    /// <inheritdoc />
    [RequiresDynamicCode("This method uses reflection to invoke the generic FindReaderWriter<T> method.")]
    public virtual JsonValueReaderWriter? FindReaderWriter(Type type)
        => (JsonValueReaderWriter?)FindReaderWriterMethod
            .MakeGenericMethod(type)
            .Invoke(null, null);
}
