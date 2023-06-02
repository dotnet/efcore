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
    {
        Dependencies = dependencies;
    }

    /// <summary>
    ///     Dependencies for this service.
    /// </summary>
    protected virtual JsonValueReaderWriterSourceDependencies Dependencies { get; }

    /// <inheritdoc />
    public virtual JsonValueReaderWriter? FindReaderWriter(Type type)
    {
        if (type == typeof(int))
        {
            return JsonInt32ReaderWriter.Instance;
        }

        if (type == typeof(string))
        {
            return JsonStringReaderWriter.Instance;
        }

        if (type == typeof(Guid))
        {
            return JsonGuidReaderWriter.Instance;
        }

        if (type == typeof(bool))
        {
            return JsonBoolReaderWriter.Instance;
        }

        if (type == typeof(DateTime))
        {
            return JsonDateTimeReaderWriter.Instance;
        }

        if (type == typeof(DateTimeOffset))
        {
            return JsonDateTimeOffsetReaderWriter.Instance;
        }

        if (type == typeof(decimal))
        {
            return JsonDecimalReaderWriter.Instance;
        }

        if (type == typeof(double))
        {
            return JsonDoubleReaderWriter.Instance;
        }

        if (type == typeof(long))
        {
            return JsonInt64ReaderWriter.Instance;
        }

        if (type == typeof(DateOnly))
        {
            return JsonDateOnlyReaderWriter.Instance;
        }

        if (type == typeof(TimeOnly))
        {
            return JsonTimeOnlyReaderWriter.Instance;
        }

        if (type == typeof(byte[]))
        {
            return JsonByteArrayReaderWriter.Instance;
        }

        if (type == typeof(ulong))
        {
            return JsonUInt64ReaderWriter.Instance;
        }

        if (type == typeof(uint))
        {
            return JsonUInt32ReaderWriter.Instance;
        }

        if (type == typeof(byte))
        {
            return JsonByteReaderWriter.Instance;
        }

        if (type == typeof(char))
        {
            return JsonCharReaderWriter.Instance;
        }

        if (type == typeof(float))
        {
            return JsonFloatReaderWriter.Instance;
        }

        if (type == typeof(short))
        {
            return JsonInt16ReaderWriter.Instance;
        }

        if (type == typeof(sbyte))
        {
            return JsonSByteReaderWriter.Instance;
        }

        if (type == typeof(ushort))
        {
            return JsonUInt16ReaderWriter.Instance;
        }

        if (type == typeof(TimeSpan))
        {
            return JsonTimeSpanReaderWriter.Instance;
        }

        if (type.IsEnum)
        {
            var readerWriterType =
                (type.GetEnumUnderlyingType().IsSignedInteger()
                    ? typeof(JsonSignedEnumReaderWriter<>)
                    : typeof(JsonUnsignedEnumReaderWriter<>))
                .MakeGenericType(type);
            return (JsonValueReaderWriter?)readerWriterType.GetAnyProperty("Instance")!.GetValue(null);
        }

        return null;
    }
}
