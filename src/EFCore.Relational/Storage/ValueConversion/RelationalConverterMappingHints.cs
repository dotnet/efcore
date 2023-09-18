// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Data;

namespace Microsoft.EntityFrameworkCore.Storage.ValueConversion;

/// <summary>
///     Specifies hints used by the type mapper when mapping using a <see cref="ValueConverter" />.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-value-converters">EF Core value converters</see> for more information and examples.
/// </remarks>
public class RelationalConverterMappingHints : ConverterMappingHints
{
    /// <summary>
    ///     Creates a new <see cref="ConverterMappingHints" /> instance. Any hint contained in the instance
    ///     can be <see langword="null" /> to indicate it has not been specified.
    /// </summary>
    /// <param name="size">The suggested size of the mapped data type.</param>
    /// <param name="precision">The suggested precision of the mapped data type.</param>
    /// <param name="scale">The suggested scale of the mapped data type.</param>
    /// <param name="unicode">Whether or not the mapped data type should support Unicode.</param>
    /// <param name="fixedLength">Whether or not the mapped data type is fixed length.</param>
    /// <param name="valueGeneratorFactory">An optional factory for creating a specific <see cref="ValueGenerator" />.</param>
    /// <param name="dbType">The suggested <see cref="DbType" />.</param>
    public RelationalConverterMappingHints(
        int? size = null,
        int? precision = null,
        int? scale = null,
        bool? unicode = null,
        bool? fixedLength = null,
        Func<IProperty, IEntityType, ValueGenerator>? valueGeneratorFactory = null,
        DbType? dbType = null)
        : base(size, precision, scale, unicode, valueGeneratorFactory)
    {
        IsFixedLength = fixedLength;
        DbType = dbType;
    }

    /// <summary>
    ///     Creates a new <see cref="ConverterMappingHints" /> instance. Any hint contained in the instance
    ///     can be <see langword="null" /> to indicate it has not been specified.
    /// </summary>
    /// <param name="size">The suggested size of the mapped data type.</param>
    /// <param name="precision">The suggested precision of the mapped data type.</param>
    /// <param name="scale">The suggested scale of the mapped data type.</param>
    /// <param name="unicode">Whether or not the mapped data type should support Unicode.</param>
    /// <param name="fixedLength">Whether or not the mapped data type is fixed length.</param>
    /// <param name="valueGeneratorFactory">An optional factory for creating a specific <see cref="ValueGenerator" />.</param>
    [Obsolete("Use the overload with more parameters.")]
    public RelationalConverterMappingHints(
        int? size,
        int? precision,
        int? scale,
        bool? unicode,
        bool? fixedLength,
        Func<IProperty, ITypeBase, ValueGenerator>? valueGeneratorFactory)
        : base(size, precision, scale, unicode, valueGeneratorFactory)
    {
        IsFixedLength = fixedLength;
    }

    /// <inheritdoc />
    public override ConverterMappingHints With(ConverterMappingHints? hints)
        => hints == null
            ? this
            : new RelationalConverterMappingHints(
                hints.Size ?? Size,
                hints.Precision ?? Precision,
                hints.Scale ?? Scale,
                hints.IsUnicode ?? IsUnicode,
                (hints as RelationalConverterMappingHints)?.IsFixedLength ?? IsFixedLength,
#pragma warning disable CS0612 // Type or member is obsolete
                hints.ValueGeneratorFactory ?? ValueGeneratorFactory,
#pragma warning restore CS0612 // Type or member is obsolete
                (hints as RelationalConverterMappingHints)?.DbType ?? DbType);

    /// <inheritdoc />
    public override ConverterMappingHints OverrideWith(ConverterMappingHints? hints)
        => hints == null
            ? this
            : new RelationalConverterMappingHints(
                Size ?? hints.Size,
                Precision ?? hints.Precision,
                Scale ?? hints.Scale,
                IsUnicode ?? hints.IsUnicode,
                IsFixedLength ?? (hints as RelationalConverterMappingHints)?.IsFixedLength,
#pragma warning disable CS0612 // Type or member is obsolete
                ValueGeneratorFactory ?? hints.ValueGeneratorFactory,
#pragma warning restore CS0612 // Type or member is obsolete
                DbType ?? (hints as RelationalConverterMappingHints)?.DbType);

    /// <summary>
    ///     Whether or not the mapped data type is fixed length.
    /// </summary>
    public virtual bool? IsFixedLength { get; }

    /// <summary>
    ///     The suggested <see cref="DbType" />
    /// </summary>
    public virtual DbType? DbType { get; }
}
