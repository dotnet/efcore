// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Storage.ValueConversion;

/// <summary>
///     Converts <typeparamref name="TModel" /> to and from <typeparamref name="TProvider" /> using simple casts from one type
///     to the other.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-value-converters">EF Core value converters</see> for more information and examples.
/// </remarks>
public class CastingConverter<TModel, TProvider> : ValueConverter<TModel, TProvider>
{
    // ReSharper disable once StaticMemberInGenericType
    private static readonly ConverterMappingHints? DefaultHints = CreateDefaultHints();

    private static ConverterMappingHints? CreateDefaultHints()
    {
        if (typeof(TProvider).UnwrapNullableType() == typeof(decimal))
        {
            var underlyingModelType = typeof(TModel).UnwrapNullableType().UnwrapEnumType();

            if (underlyingModelType == typeof(long)
                || underlyingModelType == typeof(ulong))
            {
                return new ConverterMappingHints(precision: 20, scale: 0);
            }

            if (underlyingModelType == typeof(float)
                || underlyingModelType == typeof(double))
            {
                return new ConverterMappingHints(precision: 38, scale: 17);
            }
        }

        return default;
    }

    /// <summary>
    ///     Creates a new instance of this converter.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-value-converters">EF Core value converters</see> for more information and examples.
    /// </remarks>
    public CastingConverter()
        : this(null)
    {
    }

    /// <summary>
    ///     Creates a new instance of this converter.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-value-converters">EF Core value converters</see> for more information and examples.
    /// </remarks>
    public CastingConverter(ConverterMappingHints? mappingHints)
        : base(
            Convert<TModel, TProvider>(),
            Convert<TProvider, TModel>(),
            DefaultHints?.With(mappingHints) ?? mappingHints)
    {
    }

    /// <summary>
    ///     A <see cref="ValueConverterInfo" /> for the default use of this converter.
    /// </summary>
    public static ValueConverterInfo DefaultInfo { get; }
        = new(typeof(TModel), typeof(TProvider), i => new CastingConverter<TModel, TProvider>(i.MappingHints), DefaultHints);

    private static Expression<Func<TIn, TOut>> Convert<TIn, TOut>()
    {
        var param = Expression.Parameter(typeof(TIn), "v");
        return Expression.Lambda<Func<TIn, TOut>>(
            Expression.Convert(param, typeof(TOut)),
            param);
    }
}
