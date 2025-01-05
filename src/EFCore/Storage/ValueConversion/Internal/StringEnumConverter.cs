// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class StringEnumConverter<TModel, TProvider, TEnum> : ValueConverter<TModel, TProvider>
    where TEnum : struct, Enum
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public StringEnumConverter(
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
    protected static new Expression<Func<TEnum, string>> ToString()
        => v => v.ToString()!;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected static Expression<Func<string, TEnum>> ToEnum()
    {
        if (!typeof(TEnum).UnwrapNullableType().IsEnum)
        {
            throw new InvalidOperationException(
                CoreStrings.ConverterBadType(
                    typeof(StringEnumConverter<TModel, TProvider, TEnum>).ShortDisplayName(),
                    typeof(TEnum).ShortDisplayName(),
                    "enum types"));
        }

        return v => ConvertToEnum(v);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static TEnum ConvertToEnum(string value)
        => Enum.TryParse<TEnum>(value, out var result)
            ? result
            : Enum.TryParse(value, true, out result)
                ? result
                : ulong.TryParse(value, out var ulongValue)
                    ? (TEnum)(object)ulongValue
                    : long.TryParse(value, out var longValue)
                        ? (TEnum)(object)longValue
                        : value == ""
                            ? default
                            : value == null
                                ? throw new ArgumentNullException(nameof(value))
                                : throw new InvalidOperationException(
                                    CoreStrings.CannotConvertEnumValue(value, typeof(TEnum).ShortDisplayName()));
}
