// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.EntityFrameworkCore.Storage.Json;

namespace Microsoft.EntityFrameworkCore.Storage;

/// <summary>
///     <para>
///         Represents the mapping between a .NET type and a database type.
///     </para>
///     <para>
///         This type is typically used by database providers (and other extensions). It is generally
///         not used in application code.
///     </para>
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-providers">Implementation of database providers and extensions</see>
///     for more information and examples.
/// </remarks>
public abstract class CoreTypeMapping
{
    /// <summary>
    ///     Parameter object for use in the <see cref="CoreTypeMapping" /> hierarchy.
    /// </summary>
    protected readonly record struct CoreTypeMappingParameters
    {
        /// <summary>
        ///     Creates a new <see cref="CoreTypeMappingParameters" /> parameter object.
        /// </summary>
        /// <param name="clrType">The .NET type used in the EF model.</param>
        /// <param name="converter">Converts types to and from the store whenever this mapping is used.</param>
        /// <param name="comparer">Supports custom value snapshotting and comparisons.</param>
        /// <param name="keyComparer">Supports custom comparisons between keys--e.g. PK to FK comparison.</param>
        /// <param name="providerValueComparer">Supports custom comparisons between converted provider values.</param>
        /// <param name="valueGeneratorFactory">An optional factory for creating a specific <see cref="ValueGenerator" />.</param>
        /// <param name="elementMapping">
        ///     If this type mapping represents a primitive collection, this holds the element's type mapping.
        /// </param>
        /// <param name="jsonValueReaderWriter">Handles reading and writing JSON values for instances of the mapped type.</param>
        public CoreTypeMappingParameters(
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods)] Type clrType,
            ValueConverter? converter = null,
            ValueComparer? comparer = null,
            ValueComparer? keyComparer = null,
            ValueComparer? providerValueComparer = null,
            Func<IProperty, ITypeBase, ValueGenerator>? valueGeneratorFactory = null,
            CoreTypeMapping? elementMapping = null,
            JsonValueReaderWriter? jsonValueReaderWriter = null)
        {
            ClrType = clrType;
            Converter = converter;
            Comparer = comparer;
            KeyComparer = keyComparer;
            ProviderValueComparer = providerValueComparer;
            ValueGeneratorFactory = valueGeneratorFactory;
            ElementTypeMapping = elementMapping;
            JsonValueReaderWriter = jsonValueReaderWriter;
        }

        /// <summary>
        ///     The mapping CLR type.
        /// </summary>
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods)]
        public Type ClrType { get; init; }

        /// <summary>
        ///     The mapping converter.
        /// </summary>
        public ValueConverter? Converter { get; init; }

        /// <summary>
        ///     The mapping comparer.
        /// </summary>
        public ValueComparer? Comparer { get; init; }

        /// <summary>
        ///     The mapping key comparer.
        /// </summary>
        public ValueComparer? KeyComparer { get; init; }

        /// <summary>
        ///     The provider comparer.
        /// </summary>
        public ValueComparer? ProviderValueComparer { get; init; }

        /// <summary>
        ///     An optional factory for creating a specific <see cref="ValueGenerator" /> to use with
        ///     this mapping.
        /// </summary>
        public Func<IProperty, ITypeBase, ValueGenerator>? ValueGeneratorFactory { get; }

        /// <summary>
        ///     If this type mapping represents a primitive collection, this holds the element's type mapping.
        /// </summary>
        public CoreTypeMapping? ElementTypeMapping { get; init; }

        /// <summary>
        ///     Handles reading and writing JSON values for instances of the mapped type.
        /// </summary>
        public JsonValueReaderWriter? JsonValueReaderWriter { get; init; }

        /// <summary>
        ///     Creates a new <see cref="CoreTypeMappingParameters" /> parameter object with the given
        ///     converter composed with any existing converter and set on the new parameter object.
        /// </summary>
        /// <param name="converter">The converter.</param>
        /// <param name="comparer">The comparer.</param>
        /// <param name="keyComparer">The key comparer.</param>
        /// <param name="elementMapping">The element mapping, or <see langword="null" /> for non-collection mappings.</param>
        /// <param name="jsonValueReaderWriter">The JSON reader/writer, or <see langword="null" /> to leave unchanged.</param>
        /// <returns>The new parameter object.</returns>
        public CoreTypeMappingParameters WithComposedConverter(
            ValueConverter? converter,
            ValueComparer? comparer,
            ValueComparer? keyComparer,
            CoreTypeMapping? elementMapping,
            JsonValueReaderWriter? jsonValueReaderWriter)
        {
            converter = converter?.ComposeWith(Converter);

            return new CoreTypeMappingParameters(
                ClrType,
                converter ?? Converter,
                comparer ?? Comparer,
                keyComparer ?? KeyComparer,
                ProviderValueComparer,
                ValueGeneratorFactory,
                elementMapping ?? ElementTypeMapping,
                jsonValueReaderWriter ?? CreateReaderWriter(converter, JsonValueReaderWriter));

            static JsonValueReaderWriter? CreateReaderWriter(ValueConverter? converter, JsonValueReaderWriter? readerWriter)
            {
                if (converter == null || readerWriter == null)
                {
                    return readerWriter;
                }

                if (!RuntimeFeature.IsDynamicCodeSupported)
                {
                    throw new InvalidOperationException(CoreStrings.NativeAotNoCompiledModel);
                }

                if (readerWriter is IJsonConvertedValueReaderWriter convertedValueReaderWriter)
                {
                    readerWriter = convertedValueReaderWriter.InnerReaderWriter;
                }

                return (JsonValueReaderWriter)Activator.CreateInstance(
                    typeof(JsonConvertedValueReaderWriter<,>).MakeGenericType(
                        converter.ModelClrType, readerWriter.ValueType),
                    readerWriter, converter)!;
            }
        }
    }

    private ValueComparer? _comparer;
    private ValueComparer? _keyComparer;
    private ValueComparer? _providerValueComparer;

    /// <summary>
    ///     Initializes a new instance of the <see cref="CoreTypeMapping" /> class.
    /// </summary>
    /// <param name="parameters">The parameters for this mapping.</param>
    protected CoreTypeMapping(CoreTypeMappingParameters parameters)
    {
        Parameters = parameters;

        var converter = parameters.Converter;

        var clrType = converter?.ModelClrType ?? parameters.ClrType;
        ClrType = clrType;

        Check.DebugAssert(
            parameters.Comparer == null
            || converter != null
            || parameters.Comparer.Type.IsAssignableFrom(clrType),
            $"Expected {clrType}, got {parameters.Comparer?.Type}");
        if (parameters.Comparer?.Type.IsAssignableFrom(clrType) == true)
        {
            _comparer = parameters.Comparer;
        }

        Check.DebugAssert(
            parameters.KeyComparer == null
            || converter != null
            || parameters.KeyComparer.Type.IsAssignableFrom(parameters.ClrType),
            $"Expected {parameters.ClrType}, got {parameters.KeyComparer?.Type}");
        if (parameters.KeyComparer?.Type.IsAssignableFrom(clrType) == true)
        {
            _keyComparer = parameters.KeyComparer;
        }

        Check.DebugAssert(
            parameters.ProviderValueComparer == null
            || parameters.ProviderValueComparer.Type.IsAssignableFrom(converter?.ProviderClrType ?? clrType),
            $"Expected {converter?.ProviderClrType ?? clrType}, got {parameters.ProviderValueComparer?.Type}");
        if (parameters.ProviderValueComparer?.Type.IsAssignableFrom(converter?.ProviderClrType ?? clrType) == true)
        {
            _providerValueComparer = parameters.ProviderValueComparer;
        }

#pragma warning disable CS0612 // Type or member is obsolete
        ValueGeneratorFactory = parameters.ValueGeneratorFactory
            ?? converter?.MappingHints?.ValueGeneratorFactory;
#pragma warning restore CS0612 // Type or member is obsolete
    }

    /// <summary>
    ///     Returns the parameters used to create this type mapping.
    /// </summary>
    protected virtual CoreTypeMappingParameters Parameters { get; }

    /// <summary>
    ///     Gets the .NET type used in the EF model.
    /// </summary>
    [DynamicallyAccessedMembers(
        DynamicallyAccessedMemberTypes.PublicMethods
        | DynamicallyAccessedMemberTypes.NonPublicMethods
        | DynamicallyAccessedMemberTypes.PublicProperties)]
    public virtual Type ClrType { get; }

    /// <summary>
    ///     Converts types to and from the store whenever this mapping is used.
    ///     May be null if no conversion is needed.
    /// </summary>
    public virtual ValueConverter? Converter
        => Parameters.Converter;

    /// <summary>
    ///     An optional factory for creating a specific <see cref="ValueGenerator" /> to use with
    ///     this mapping.
    /// </summary>
    [Obsolete]
    public virtual Func<IProperty, IEntityType, ValueGenerator>? ValueGeneratorFactory { get; }

    /// <summary>
    ///     A <see cref="ValueComparer" /> adds custom value snapshotting and comparison for
    ///     CLR types that cannot be compared with <see cref="object.Equals(object, object)" />
    ///     and/or need a deep copy when taking a snapshot.
    /// </summary>
    public virtual ValueComparer Comparer
        => NonCapturingLazyInitializer.EnsureInitialized(
            ref _comparer,
            this,
            static c => ValueComparer.CreateDefault(c.ClrType, favorStructuralComparisons: false));

    /// <summary>
    ///     A <see cref="ValueComparer" /> adds custom value comparison for use when
    ///     comparing key values to each other. For example, when comparing a PK to and FK.
    /// </summary>
    public virtual ValueComparer KeyComparer
        => NonCapturingLazyInitializer.EnsureInitialized(
            ref _keyComparer,
            this,
            static c => ValueComparer.CreateDefault(c.ClrType, favorStructuralComparisons: true));

    /// <summary>
    ///     A <see cref="ValueComparer" /> for the provider CLR type values.
    /// </summary>
    public virtual ValueComparer ProviderValueComparer
        => NonCapturingLazyInitializer.EnsureInitialized(
            ref _providerValueComparer,
            this,
            static c => (c.Converter?.ProviderClrType ?? c.ClrType) == c.ClrType
                ? c.KeyComparer
                : ValueComparer.CreateDefault(c.Converter!.ProviderClrType, favorStructuralComparisons: true));

    /// <summary>
    ///     Creates a copy of this mapping.
    /// </summary>
    /// <param name="parameters">The parameters for this mapping.</param>
    /// <returns>The newly created mapping.</returns>
    protected abstract CoreTypeMapping Clone(CoreTypeMappingParameters parameters);

    /// <summary>
    ///     Returns a new copy of this type mapping with the given <see cref="ValueConverter" />
    ///     added.
    /// </summary>
    /// <param name="converter">The converter to use.</param>
    /// <param name="comparer">The comparer to use, or <see langword="null" /> for to keep the default.</param>
    /// <param name="keyComparer">The comparer to use when the value is a key, or <see langword="null" /> for to keep the default.</param>
    /// <param name="elementMapping">The element mapping, or <see langword="null" /> for non-collection mappings.</param>
    /// <param name="jsonValueReaderWriter">The JSON reader/writer, or <see langword="null" /> to leave unchanged.</param>
    /// <returns>A new type mapping</returns>
    public abstract CoreTypeMapping WithComposedConverter(
        ValueConverter? converter,
        ValueComparer? comparer = null,
        ValueComparer? keyComparer = null,
        CoreTypeMapping? elementMapping = null,
        JsonValueReaderWriter? jsonValueReaderWriter = null);

    /// <summary>
    ///     Clones the type mapping to update any parameter if needed.
    /// </summary>
    /// <param name="mappingInfo">The mapping info containing the facets to use.</param>
    /// <param name="clrType">The .NET type used in the EF model, or <see langword="null" /> to leave unchanged.</param>
    /// <param name="converter">The value converter, or <see langword="null" /> to leave unchanged.</param>
    /// <param name="comparer">The value comparer, or <see langword="null" /> to leave unchanged.</param>
    /// <param name="keyComparer">The key value comparer, or <see langword="null" /> to leave unchanged.</param>
    /// <param name="providerValueComparer">The provider value comparer, or <see langword="null" /> to leave unchanged.</param>
    /// <param name="elementMapping">The element mapping, or <see langword="null" /> to leave unchanged.</param>
    /// <param name="jsonValueReaderWriter">The JSON reader/writer, or <see langword="null" /> to leave unchanged.</param>
    /// <returns>The cloned mapping, or the original mapping if no clone was needed.</returns>
    public virtual CoreTypeMapping Clone(
        in TypeMappingInfo? mappingInfo = null,
        Type? clrType = null,
        ValueConverter? converter = null,
        ValueComparer? comparer = null,
        ValueComparer? keyComparer = null,
        ValueComparer? providerValueComparer = null,
        CoreTypeMapping? elementMapping = null,
        JsonValueReaderWriter? jsonValueReaderWriter = null)
        => Clone(
            new CoreTypeMappingParameters(
                clrType ?? Parameters.ClrType,
                converter ?? Parameters.Converter,
                comparer ?? Parameters.Comparer,
                keyComparer ?? Parameters.KeyComparer,
                providerValueComparer ?? Parameters.ProviderValueComparer,
                Parameters.ValueGeneratorFactory,
                elementMapping ?? Parameters.ElementTypeMapping,
                jsonValueReaderWriter ?? Parameters.JsonValueReaderWriter));

    /// <summary>
    ///     Creates a an expression tree that can be used to generate code for the literal value.
    ///     Currently, only very basic expressions such as constructor calls and factory methods taking
    ///     simple constants are supported.
    /// </summary>
    /// <param name="value">The value for which a literal is needed.</param>
    /// <returns>An expression tree that can be used to generate code for the literal value.</returns>
    public virtual Expression GenerateCodeLiteral(object value)
        => throw new NotSupportedException(CoreStrings.LiteralGenerationNotSupported(ClrType.ShortDisplayName()));

    /// <summary>
    ///     If this type mapping represents a primitive collection, this holds the element's type mapping.
    /// </summary>
    public virtual CoreTypeMapping? ElementTypeMapping
        => Parameters.ElementTypeMapping;

    /// <summary>
    ///     Handles reading and writing JSON values for instances of the mapped type.
    /// </summary>
    public virtual JsonValueReaderWriter? JsonValueReaderWriter
        => Parameters.JsonValueReaderWriter;
}
