// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Storage.ValueConversion;

/// <summary>
///     Specifies hints used by the type mapper when mapping using a <see cref="ValueConverter" />.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-value-converters">EF Core value converters</see> for more information and examples.
/// </remarks>
public class ConverterMappingHints
{
    /// <summary>
    ///     Creates a new <see cref="ConverterMappingHints" /> instance. Any hint contained in the instance
    ///     can be <see langword="null" /> to indicate it has not been specified.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-value-converters">EF Core value converters</see> for more information and examples.
    /// </remarks>
    /// <param name="size">The suggested size of the mapped data type.</param>
    /// <param name="precision">The suggested precision of the mapped data type.</param>
    /// <param name="scale">The suggested scale of the mapped data type.</param>
    /// <param name="unicode">Whether or not the mapped data type should support Unicode.</param>
    /// <param name="valueGeneratorFactory">An optional factory for creating a specific <see cref="ValueGenerator" />.</param>
    public ConverterMappingHints(
        int? size = null,
        int? precision = null,
        int? scale = null,
        bool? unicode = null,
        Func<IProperty, IEntityType, ValueGenerator>? valueGeneratorFactory = null)
    {
        Size = size;
        Precision = precision;
        Scale = scale;
        IsUnicode = unicode;
#pragma warning disable CS0612 // Type or member is obsolete
        ValueGeneratorFactory = valueGeneratorFactory;
#pragma warning restore CS0612 // Type or member is obsolete
    }

    /// <summary>
    ///     Adds hints from the given object to this one. Hints that are already specified are not overridden.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-value-converters">EF Core value converters</see> for more information and examples.
    /// </remarks>
    /// <param name="hints">The hints to add.</param>
    /// <returns>The combined hints.</returns>
    public virtual ConverterMappingHints With(ConverterMappingHints? hints)
        => hints == null
            ? this
            : hints.GetType().IsAssignableFrom(GetType())
                ? new ConverterMappingHints(
                    hints.Size ?? Size,
                    hints.Precision ?? Precision,
                    hints.Scale ?? Scale,
                    hints.IsUnicode ?? IsUnicode,
#pragma warning disable CS0612 // Type or member is obsolete
                    hints.ValueGeneratorFactory ?? ValueGeneratorFactory)
#pragma warning restore CS0612 // Type or member is obsolete
                : hints.OverrideWith(this);

    /// <summary>
    ///     Adds hints from the given object to this one. Hints that are already specified are overridden.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-value-converters">EF Core value converters</see> for more information and examples.
    /// </remarks>
    /// <param name="hints">The hints to add.</param>
    /// <returns>The combined hints.</returns>
    public virtual ConverterMappingHints OverrideWith(ConverterMappingHints? hints)
        => hints == null
            ? this
            : GetType().IsAssignableFrom(hints.GetType())
                ? new ConverterMappingHints(
                    Size ?? hints.Size,
                    Precision ?? hints.Precision,
                    Scale ?? hints.Scale,
                    IsUnicode ?? hints.IsUnicode,
#pragma warning disable CS0612 // Type or member is obsolete
                    ValueGeneratorFactory ?? hints.ValueGeneratorFactory)
#pragma warning restore CS0612 // Type or member is obsolete
                : hints.With(this);

    /// <summary>
    ///     The suggested size of the mapped data type.
    /// </summary>
    public virtual int? Size { get; }

    /// <summary>
    ///     The suggested precision of the mapped data type.
    /// </summary>
    public virtual int? Precision { get; }

    /// <summary>
    ///     The suggested scale of the mapped data type.
    /// </summary>
    public virtual int? Scale { get; }

    /// <summary>
    ///     Whether or not the mapped data type should support Unicode.
    /// </summary>
    public virtual bool? IsUnicode { get; }

    /// <summary>
    ///     An optional factory for creating a specific <see cref="ValueGenerator" /> to use for model
    ///     values when this converter is being used.
    /// </summary>
    [Obsolete]
    public virtual Func<IProperty, IEntityType, ValueGenerator>? ValueGeneratorFactory { get; }
}
