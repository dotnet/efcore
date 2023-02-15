// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Globalization;

namespace Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class StringTimeOnlyConverter<TModel, TProvider> : ValueConverter<TModel, TProvider>
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    // ReSharper disable once StaticMemberInGenericType
    protected static readonly ConverterMappingHints DefaultHints = new(size: 48);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public StringTimeOnlyConverter(
        Expression<Func<TModel, TProvider>> convertToProviderExpression,
        Expression<Func<TProvider, TModel>> convertFromProviderExpression,
        ConverterMappingHints? mappingHints = null)
        : base(convertToProviderExpression, convertFromProviderExpression, mappingHints)
    {
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected static new Expression<Func<TimeOnly, string>> ToString()
        => v => v.Ticks % 10000000 == 0
            ? string.Format(CultureInfo.InvariantCulture, @"{0:HH\:mm\:ss}", v)
            : v.ToString("o");

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected static Expression<Func<string, TimeOnly>> ToTimeOnly()
        => v => TimeOnly.Parse(v, CultureInfo.InvariantCulture, DateTimeStyles.None);
}
