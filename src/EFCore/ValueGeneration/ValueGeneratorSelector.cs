// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.ValueGeneration;

/// <summary>
///     <para>
///         Selects value generators to be used to generate values for properties of entities.
///     </para>
///     <para>
///         This type is typically used by database providers (and other extensions). It is generally
///         not used in application code.
///     </para>
/// </summary>
/// <remarks>
///     <para>
///         The service lifetime is <see cref="ServiceLifetime.Scoped" />. This means that each
///         <see cref="DbContext" /> instance will use its own instance of this service.
///         The implementation may depend on other services registered with any lifetime.
///         The implementation does not need to be thread-safe.
///     </para>
///     <para>
///         See <see href="https://aka.ms/efcore-docs-providers">Implementation of database providers and extensions</see>
///         for more information and examples.
///     </para>
/// </remarks>
public class ValueGeneratorSelector : IValueGeneratorSelector
{
    /// <summary>
    ///     The cache being used to store value generator instances.
    /// </summary>
    public virtual IValueGeneratorCache Cache
        => Dependencies.Cache;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ValueGeneratorSelector" /> class.
    /// </summary>
    /// <param name="dependencies">Parameter object containing dependencies for this service.</param>
    public ValueGeneratorSelector(ValueGeneratorSelectorDependencies dependencies)
    {
        Dependencies = dependencies;
    }

    /// <summary>
    ///     Dependencies for this service.
    /// </summary>
    protected virtual ValueGeneratorSelectorDependencies Dependencies { get; }

    /// <summary>
    ///     Selects the appropriate value generator for a given property.
    /// </summary>
    /// <param name="property">The property to get the value generator for.</param>
    /// <param name="typeBase">
    ///     The entity type that the value generator will be used for. When called on inherited properties on derived entity types,
    ///     this entity type may be different from the declared entity type on <paramref name="property" />
    /// </param>
    /// <returns>The value generator to be used.</returns>
    public virtual ValueGenerator Select(IProperty property, ITypeBase typeBase)
        => Cache.GetOrAdd(property, typeBase, (p, t) => CreateFromFactory(p, t) ?? Create(p, t));

    private static ValueGenerator? CreateFromFactory(IProperty property, ITypeBase structuralType)
    {
        var factory = property.GetValueGeneratorFactory();
        if (factory == null)
        {
            var mapping = property.GetTypeMapping();
#pragma warning disable CS0612 // Type or member is obsolete
            if (mapping.ValueGeneratorFactory != null
                && structuralType is IEntityType)
            {
                factory = (p, t) => mapping.ValueGeneratorFactory.Invoke(p, (IEntityType)t);
            }
#pragma warning restore CS0612 // Type or member is obsolete
        }

        return factory?.Invoke(property, structuralType);
    }

    /// <summary>
    ///     Creates a new value generator for the given property.
    /// </summary>
    /// <param name="property">The property to get the value generator for.</param>
    /// <param name="typeBase">
    ///     The entity type that the value generator will be used for. When called on inherited properties on derived entity types,
    ///     this entity type may be different from the declared entity type on <paramref name="property" />
    /// </param>
    /// <returns>The newly created value generator.</returns>
    public virtual ValueGenerator Create(IProperty property, ITypeBase typeBase)
    {
        var propertyType = property.ClrType.UnwrapNullableType().UnwrapEnumType();
        var generator = FindForType(property, typeBase, propertyType);
        if (generator != null)
        {
            return generator;
        }

        var converter = property.GetTypeMapping().Converter;
        if (converter != null
            && converter.ProviderClrType != propertyType)
        {
            generator = FindForType(property, typeBase, converter.ProviderClrType);
            if (generator != null)
            {
                return generator.WithConverter(converter);
            }
        }

        throw new NotSupportedException(
            CoreStrings.NoValueGenerator(property.Name, property.DeclaringType.DisplayName(), propertyType.ShortDisplayName()));
    }

    /// <summary>
    ///     Creates a new value generator for the given property and type, where the property may have a <see cref="ValueConverter" />.
    /// </summary>
    /// <param name="property">The property to get the value generator for.</param>
    /// <param name="typeBase">
    ///     The entity type that the value generator will be used for. When called on inherited properties on derived entity types,
    ///     this entity type may be different from the declared entity type on <paramref name="property" />
    /// </param>
    /// <param name="clrType">The type, which may be the provider type after conversion, rather than the property type.</param>
    /// <returns>The newly created value generator.</returns>
    protected virtual ValueGenerator? FindForType(IProperty property, ITypeBase typeBase, Type clrType)
        => clrType == typeof(Guid)
            ? new GuidValueGenerator()
            : clrType == typeof(string)
                ? new StringValueGenerator()
                : clrType == typeof(byte[])
                    ? new BinaryValueGenerator()
                    : null;
}
