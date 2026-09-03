// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Cosmos.Storage.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class ReadOnlyMemoryConverter<T> : ValueConverter<ReadOnlyMemory<T>, T[]>
{
    private static readonly ConverterMappingHints DefaultHints = new();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public ReadOnlyMemoryConverter()
        : this(null)
    {
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public ReadOnlyMemoryConverter(ConverterMappingHints? mappingHints)
        : base(
            v => ToArray(v),
            v => ToMemory(v),
            DefaultHints.With(mappingHints))
    {
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static T[] ToArray(ReadOnlyMemory<T> memory)
        => memory.ToArray();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static ReadOnlyMemory<T> ToMemory(T[] array)
        // If the array is empty, then return the default ReadOnlyMemory instance because this will compare the same as other empty
        // ReadOnlyMemory instances, while the instance created with an empty array is considered not equal to the default.
        => array.Length == 0 ? default : new ReadOnlyMemory<T>(array);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static ValueConverterInfo DefaultInfo { get; }
        = new(typeof(ReadOnlyMemory<T>), typeof(T[]), i => new ReadOnlyMemoryConverter<T>(i.MappingHints), DefaultHints);
}
