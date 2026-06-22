// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Storage.Json;

namespace Microsoft.EntityFrameworkCore.Cosmos.Storage.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class CosmosTypeMapping<
    [DynamicallyAccessedMembers(
        DynamicallyAccessedMemberTypes.PublicMethods
        | DynamicallyAccessedMemberTypes.PublicProperties)]
    T> : CosmosTypeMapping
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static new CosmosTypeMapping<T> Default { get; } = new(jsonValueReaderWriter: FindReaderWriter());

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public static JsonValueReaderWriter? FindReaderWriter()
    {
        if (typeof(T) == typeof(int))
        {
            return JsonInt32ReaderWriter.Instance;
        }

        if (typeof(T) == typeof(string))
        {
            return JsonStringReaderWriter.Instance;
        }

        if (typeof(T) == typeof(Guid))
        {
            return JsonGuidReaderWriter.Instance;
        }

        if (typeof(T) == typeof(bool))
        {
            return JsonBoolReaderWriter.Instance;
        }

        if (typeof(T) == typeof(DateTime))
        {
            return JsonDateTimeReaderWriter.Instance;
        }

        if (typeof(T) == typeof(DateTimeOffset))
        {
            return JsonDateTimeOffsetReaderWriter.Instance;
        }

        if (typeof(T) == typeof(decimal))
        {
            return JsonDecimalReaderWriter.Instance;
        }

        if (typeof(T) == typeof(double))
        {
            return JsonDoubleReaderWriter.Instance;
        }

        if (typeof(T) == typeof(long))
        {
            return JsonInt64ReaderWriter.Instance;
        }

        if (typeof(T) == typeof(DateOnly))
        {
            return JsonDateOnlyReaderWriter.Instance;
        }

        if (typeof(T) == typeof(TimeOnly))
        {
            return JsonTimeOnlyReaderWriter.Instance;
        }

        if (typeof(T) == typeof(byte[]))
        {
            return JsonByteArrayReaderWriter.Instance;
        }

        if (typeof(T) == typeof(ulong))
        {
            return JsonUInt64ReaderWriter.Instance;
        }

        if (typeof(T) == typeof(uint))
        {
            return JsonUInt32ReaderWriter.Instance;
        }

        if (typeof(T) == typeof(byte))
        {
            return JsonByteReaderWriter.Instance;
        }

        if (typeof(T) == typeof(char))
        {
            return JsonCharReaderWriter.Instance;
        }

        if (typeof(T) == typeof(float))
        {
            return JsonFloatReaderWriter.Instance;
        }

        if (typeof(T) == typeof(short))
        {
            return JsonInt16ReaderWriter.Instance;
        }

        if (typeof(T) == typeof(sbyte))
        {
            return JsonSByteReaderWriter.Instance;
        }

        if (typeof(T) == typeof(ushort))
        {
            return JsonUInt16ReaderWriter.Instance;
        }

        if (typeof(T) == typeof(TimeSpan))
        {
            return JsonTimeSpanReaderWriter.Instance;
        }

        return null;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public CosmosTypeMapping(
        ValueComparer? comparer = null,
        ValueComparer? keyComparer = null,
        CoreTypeMapping? elementMapping = null,
        JsonValueReaderWriter? jsonValueReaderWriter = null)
        : base(typeof(T), comparer, keyComparer, elementMapping, jsonValueReaderWriter)
    {
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected CosmosTypeMapping(CoreTypeMappingParameters parameters)
        : base(parameters)
    {
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override ValueComparer CreateDefaultComparer(bool favorStructuralComparisons)
        => ClrType == typeof(T)
            ? ValueComparer.CreateDefault<T>(favorStructuralComparisons)
            : base.CreateDefaultComparer(favorStructuralComparisons);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override CoreTypeMapping WithComposedConverter(
        ValueConverter? converter,
        ValueComparer? comparer = null,
        ValueComparer? keyComparer = null,
        CoreTypeMapping? elementMapping = null,
        JsonValueReaderWriter? jsonValueReaderWriter = null)
        => new CosmosTypeMapping<T>(
            Parameters.WithComposedConverter(converter, comparer, keyComparer, elementMapping, jsonValueReaderWriter));

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override CoreTypeMapping Clone(CoreTypeMappingParameters parameters)
        => new CosmosTypeMapping<T>(parameters);
}
