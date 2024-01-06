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
public class StringNumberConverter<TModel, TProvider, TNumber> : ValueConverter<TModel, TProvider>
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    // ReSharper disable once StaticMemberInGenericType
    protected static readonly ConverterMappingHints DefaultHints = new(size: 64);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public StringNumberConverter(
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
    protected static Expression<Func<string, TNumber>> ToNumber()
    {
        var type = typeof(TNumber).UnwrapNullableType();

        CheckTypeSupported(
            type,
            typeof(StringNumberConverter<TModel, TProvider, TNumber>),
            typeof(int), typeof(long), typeof(short), typeof(byte),
            typeof(uint), typeof(ulong), typeof(ushort), typeof(sbyte),
            typeof(decimal), typeof(float), typeof(double));

        var parseMethod = type.GetMethod(
            nameof(double.Parse),
            [typeof(string), typeof(NumberStyles), typeof(IFormatProvider)])!;

        var param = Expression.Parameter(typeof(string), "v");

        Expression expression = Expression.Call(
            parseMethod,
            param,
            Expression.Constant(NumberStyles.Any),
            Expression.Constant(CultureInfo.InvariantCulture, typeof(IFormatProvider)));

        if (typeof(TNumber).IsNullableType())
        {
            expression = Expression.Condition(
                Expression.ReferenceEqual(param, Expression.Constant(null, typeof(string))),
                Expression.Constant(null, typeof(TNumber)),
                Expression.Convert(expression, typeof(TNumber)));
        }

        return Expression.Lambda<Func<string, TNumber>>(expression, param);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected static new Expression<Func<TNumber, string>> ToString()
    {
        var type = typeof(TNumber).UnwrapNullableType();

        CheckTypeSupported(
            type,
            typeof(StringNumberConverter<TModel, TProvider, TNumber>),
            typeof(int), typeof(long), typeof(short), typeof(byte),
            typeof(uint), typeof(ulong), typeof(ushort), typeof(sbyte),
            typeof(decimal), typeof(float), typeof(double));

        var formatMethod = typeof(string).GetMethod(
            nameof(string.Format),
            [typeof(IFormatProvider), typeof(string), typeof(object)])!;

        var param = Expression.Parameter(typeof(TNumber), "v");

        Expression expression = Expression.Call(
            formatMethod,
            Expression.Constant(CultureInfo.InvariantCulture),
            Expression.Constant(type == typeof(float) || type == typeof(double) ? "{0:R}" : "{0}"),
            Expression.Convert(param, typeof(object)));

        if (typeof(TNumber).IsNullableType())
        {
            expression = Expression.Condition(
                Expression.MakeMemberAccess(param, typeof(TNumber).GetProperty("HasValue")!),
                expression,
                Expression.Constant(null, typeof(string)));
        }

        return Expression.Lambda<Func<TNumber, string>>(expression, param);
    }
}
