// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;

namespace Microsoft.EntityFrameworkCore.Metadata.Builders;

/// <summary>
///     Provides a simple API for configuring a <see cref="IMutableProperty" />.
/// </summary>
/// <remarks>
///     <para>
///         Instances of this class are returned from methods when using the <see cref="ModelBuilder" /> API
///         and it is not designed to be directly constructed in your application code.
///     </para>
///     <para>
///         See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and
///         examples.
///     </para>
/// </remarks>
public class PrimitiveCollectionBuilder<TProperty> : PrimitiveCollectionBuilder
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public PrimitiveCollectionBuilder(IMutableProperty property)
        : base(property)
    {
    }

    /// <summary>
    ///     Adds or updates an annotation on the property. If an annotation with the key specified in
    ///     <paramref name="annotation" /> already exists its value will be updated.
    /// </summary>
    /// <param name="annotation">The key of the annotation to be added or updated.</param>
    /// <param name="value">The value to be stored in the annotation.</param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public new virtual PrimitiveCollectionBuilder<TProperty> HasAnnotation(string annotation, object? value)
        => (PrimitiveCollectionBuilder<TProperty>)base.HasAnnotation(annotation, value);

    /// <summary>
    ///     Configures whether this property must have a value assigned or whether null is a valid value.
    ///     A property can only be configured as non-required if it is based on a CLR type that can be
    ///     assigned <see langword="null" />.
    /// </summary>
    /// <param name="required">A value indicating whether the property is required.</param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public new virtual PrimitiveCollectionBuilder<TProperty> IsRequired(bool required = true)
        => (PrimitiveCollectionBuilder<TProperty>)base.IsRequired(required);

    /// <summary>
    ///     Configures the maximum length of data that can be stored in this property.
    ///     Maximum length can only be set on array properties (including <see cref="string" /> properties).
    /// </summary>
    /// <param name="maxLength">
    ///     The maximum length of data allowed in the property. A value of <c>-1</c> indicates that the property has no maximum length.
    /// </param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public new virtual PrimitiveCollectionBuilder<TProperty> HasMaxLength(int maxLength)
        => (PrimitiveCollectionBuilder<TProperty>)base.HasMaxLength(maxLength);

    /// <summary>
    ///     Configures the value that will be used to determine if the property has been set or not. If the property is set to the
    ///     sentinel value, then it is considered not set. By default, the sentinel value is the CLR default value for the type of
    ///     the property.
    /// </summary>
    /// <param name="sentinel">The sentinel value.</param>
    /// <returns>The same builder instance if the configuration was applied, <see langword="null" /> otherwise.</returns>
    public new virtual PrimitiveCollectionBuilder<TProperty> HasSentinel(object? sentinel)
        => (PrimitiveCollectionBuilder<TProperty>)base.HasSentinel(sentinel);

    /// <summary>
    ///     Configures the value that will be used to determine if the property has been set or not. If the property is set to the
    ///     sentinel value, then it is considered not set. By default, the sentinel value is the CLR default value for the type of
    ///     the property.
    /// </summary>
    /// <param name="sentinel">The sentinel value.</param>
    /// <returns>The same builder instance if the configuration was applied, <see langword="null" /> otherwise.</returns>
    public virtual PrimitiveCollectionBuilder<TProperty> HasSentinel(TProperty? sentinel)
        => (PrimitiveCollectionBuilder<TProperty>)base.HasSentinel(sentinel);

    /// <summary>
    ///     Configures the property as capable of persisting unicode characters.
    ///     Can only be set on <see cref="string" /> properties.
    /// </summary>
    /// <param name="unicode">A value indicating whether the property can contain unicode characters.</param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public new virtual PrimitiveCollectionBuilder<TProperty> IsUnicode(bool unicode = true)
        => (PrimitiveCollectionBuilder<TProperty>)base.IsUnicode(unicode);

    /// <summary>
    ///     Configures the <see cref="ValueGenerator" /> that will generate values for this property.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Values are generated when the entity is added to the context using, for example,
    ///         <see cref="DbContext.Add{TEntity}" />. Values are generated only when the property is assigned
    ///         the CLR default value (<see langword="null" /> for <c>string</c>, <c>0</c> for <c>int</c>,
    ///         <c>Guid.Empty</c> for <c>Guid</c>, etc.).
    ///     </para>
    ///     <para>
    ///         A single instance of this type will be created and used to generate values for this property in all
    ///         instances of the entity type. The type must be instantiable and have a parameterless constructor.
    ///     </para>
    ///     <para>
    ///         This method is intended for use with custom value generation. Value generation for common cases is
    ///         usually handled automatically by the database provider.
    ///     </para>
    /// </remarks>
    /// <typeparam name="TGenerator">A type that inherits from <see cref="ValueGenerator" />.</typeparam>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public new virtual PrimitiveCollectionBuilder<TProperty> HasValueGenerator
        <[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] TGenerator>()
        where TGenerator : ValueGenerator
        => (PrimitiveCollectionBuilder<TProperty>)base.HasValueGenerator<TGenerator>();

    /// <summary>
    ///     Configures the <see cref="ValueGenerator" /> that will generate values for this property.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Values are generated when the entity is added to the context using, for example,
    ///         <see cref="DbContext.Add{TEntity}" />. Values are generated only when the property is assigned
    ///         the CLR default value (<see langword="null" /> for <c>string</c>, <c>0</c> for <c>int</c>,
    ///         <c>Guid.Empty</c> for <c>Guid</c>, etc.).
    ///     </para>
    ///     <para>
    ///         A single instance of this type will be created and used to generate values for this property in all
    ///         instances of the entity type. The type must be instantiable and have a parameterless constructor.
    ///     </para>
    ///     <para>
    ///         This method is intended for use with custom value generation. Value generation for common cases is
    ///         usually handled automatically by the database provider.
    ///     </para>
    ///     <para>
    ///         Setting null does not disable value generation for this property, it just clears any generator explicitly
    ///         configured for this property. The database provider may still have a value generator for the property type.
    ///     </para>
    /// </remarks>
    /// <param name="valueGeneratorType">A type that inherits from <see cref="ValueGenerator" />.</param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public new virtual PrimitiveCollectionBuilder<TProperty> HasValueGenerator(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
        Type? valueGeneratorType)
        => (PrimitiveCollectionBuilder<TProperty>)base.HasValueGenerator(valueGeneratorType);

    /// <summary>
    ///     Configures the <see cref="ValueGeneratorFactory" /> for creating a <see cref="ValueGenerator" />
    ///     to use to generate values for this property.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Values are generated when the entity is added to the context using, for example,
    ///         <see cref="DbContext.Add{TEntity}" />. Values are generated only when the property is assigned
    ///         the CLR default value (<see langword="null" /> for <c>string</c>, <c>0</c> for <c>int</c>,
    ///         <c>Guid.Empty</c> for <c>Guid</c>, etc.).
    ///     </para>
    ///     <para>
    ///         A single instance of this type will be created and used to generate values for this property in all
    ///         instances of the entity type. The type must be instantiable and have a parameterless constructor.
    ///     </para>
    ///     <para>
    ///         This method is intended for use with custom value generation. Value generation for common cases is
    ///         usually handled automatically by the database provider.
    ///     </para>
    ///     <para>
    ///         Setting <see langword="null" /> does not disable value generation for this property, it just clears any generator explicitly
    ///         configured for this property. The database provider may still have a value generator for the property type.
    ///     </para>
    /// </remarks>
    /// <typeparam name="TFactory">A type that inherits from <see cref="ValueGeneratorFactory" />.</typeparam>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public new virtual PrimitiveCollectionBuilder<TProperty> HasValueGeneratorFactory
        <[DynamicallyAccessedMembers(ValueGeneratorFactory.DynamicallyAccessedMemberTypes)] TFactory>()
        where TFactory : ValueGeneratorFactory
        => (PrimitiveCollectionBuilder<TProperty>)base.HasValueGeneratorFactory<TFactory>();

    /// <summary>
    ///     Configures the <see cref="ValueGeneratorFactory" /> for creating a <see cref="ValueGenerator" />
    ///     to use to generate values for this property.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Values are generated when the entity is added to the context using, for example,
    ///         <see cref="DbContext.Add{TEntity}" />. Values are generated only when the property is assigned
    ///         the CLR default value (<see langword="null" /> for <c>string</c>, <c>0</c> for <c>int</c>,
    ///         <c>Guid.Empty</c> for <c>Guid</c>, etc.).
    ///     </para>
    ///     <para>
    ///         A single instance of this type will be created and used to generate values for this property in all
    ///         instances of the entity type. The type must be instantiable and have a parameterless constructor.
    ///     </para>
    ///     <para>
    ///         This method is intended for use with custom value generation. Value generation for common cases is
    ///         usually handled automatically by the database provider.
    ///     </para>
    ///     <para>
    ///         Setting <see langword="null" /> does not disable value generation for this property, it just clears any generator explicitly
    ///         configured for this property. The database provider may still have a value generator for the property type.
    ///     </para>
    /// </remarks>
    /// <param name="valueGeneratorFactoryType">A type that inherits from <see cref="ValueGeneratorFactory" />.</param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public new virtual PrimitiveCollectionBuilder<TProperty> HasValueGeneratorFactory(
        [DynamicallyAccessedMembers(ValueGeneratorFactory.DynamicallyAccessedMemberTypes)]
        Type? valueGeneratorFactoryType)
        => (PrimitiveCollectionBuilder<TProperty>)base.HasValueGeneratorFactory(valueGeneratorFactoryType);

    /// <summary>
    ///     Configures whether this property should be used as a concurrency token. When a property is configured
    ///     as a concurrency token the value in the database will be checked when an instance of this entity type
    ///     is updated or deleted during <see cref="DbContext.SaveChanges()" /> to ensure it has not changed since
    ///     the instance was retrieved from the database. If it has changed, an exception will be thrown and the
    ///     changes will not be applied to the database.
    /// </summary>
    /// <param name="concurrencyToken">A value indicating whether this property is a concurrency token.</param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public new virtual PrimitiveCollectionBuilder<TProperty> IsConcurrencyToken(bool concurrencyToken = true)
        => (PrimitiveCollectionBuilder<TProperty>)base.IsConcurrencyToken(concurrencyToken);

    /// <summary>
    ///     Configures a property to never have a value generated when an instance of this
    ///     entity type is saved.
    /// </summary>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    /// <remarks>
    ///     Note that temporary values may still be generated for use internally before a
    ///     new entity is saved.
    /// </remarks>
    public new virtual PrimitiveCollectionBuilder<TProperty> ValueGeneratedNever()
        => (PrimitiveCollectionBuilder<TProperty>)base.ValueGeneratedNever();

    /// <summary>
    ///     Configures a property to have a value generated only when saving a new entity, unless a non-null,
    ///     non-temporary value has been set, in which case the set value will be saved instead. The value
    ///     may be generated by a client-side value generator or may be generated by the database as part
    ///     of saving the entity.
    /// </summary>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public new virtual PrimitiveCollectionBuilder<TProperty> ValueGeneratedOnAdd()
        => (PrimitiveCollectionBuilder<TProperty>)base.ValueGeneratedOnAdd();

    /// <summary>
    ///     Configures a property to have a value generated when saving a new or existing entity.
    /// </summary>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public new virtual PrimitiveCollectionBuilder<TProperty> ValueGeneratedOnAddOrUpdate()
        => (PrimitiveCollectionBuilder<TProperty>)base.ValueGeneratedOnAddOrUpdate();

    /// <summary>
    ///     Configures a property to have a value generated when saving an existing entity.
    /// </summary>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public new virtual PrimitiveCollectionBuilder<TProperty> ValueGeneratedOnUpdate()
        => (PrimitiveCollectionBuilder<TProperty>)base.ValueGeneratedOnUpdate();

    /// <summary>
    ///     Configures a property to have a value generated under certain conditions when saving an existing entity.
    /// </summary>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public new virtual PrimitiveCollectionBuilder<TProperty> ValueGeneratedOnUpdateSometimes()
        => (PrimitiveCollectionBuilder<TProperty>)base.ValueGeneratedOnUpdateSometimes();

    /// <summary>
    ///     Sets the backing field to use for this property.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Backing fields are normally found by convention.
    ///         This method is useful for setting backing fields explicitly in cases where the
    ///         correct field is not found by convention.
    ///     </para>
    ///     <para>
    ///         By default, the backing field, if one is found or has been specified, is used when
    ///         new objects are constructed, typically when entities are queried from the database.
    ///         Properties are used for all other accesses. This can be changed by calling
    ///         <see cref="UsePropertyAccessMode" />.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-backing-fields">Backing fields</see> for more information and examples.
    ///     </para>
    /// </remarks>
    /// <param name="fieldName">The field name.</param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public new virtual PrimitiveCollectionBuilder<TProperty> HasField(string fieldName)
        => (PrimitiveCollectionBuilder<TProperty>)base.HasField(fieldName);

    /// <summary>
    ///     Configures the elements of this collection.
    /// </summary>
    /// <param name="builderAction">An action that performs configuration of the collection element type.</param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public new virtual PrimitiveCollectionBuilder<TProperty> ElementType(Action<ElementTypeBuilder> builderAction)
    {
        builderAction(ElementType());

        return this;
    }

    /// <summary>
    ///     Sets the <see cref="PropertyAccessMode" /> to use for this property.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         By default, the backing field, if one is found by convention or has been specified, is used when
    ///         new objects are constructed, typically when entities are queried from the database.
    ///         Properties are used for all other accesses. Calling this method will change that behavior
    ///         for this property as described in the <see cref="PropertyAccessMode" /> enum.
    ///     </para>
    ///     <para>
    ///         Calling this method overrides for this property any access mode that was set on the
    ///         entity type or model.
    ///     </para>
    /// </remarks>
    /// <param name="propertyAccessMode">The <see cref="PropertyAccessMode" /> to use for this property.</param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public new virtual PrimitiveCollectionBuilder<TProperty> UsePropertyAccessMode(PropertyAccessMode propertyAccessMode)
        => (PrimitiveCollectionBuilder<TProperty>)base.UsePropertyAccessMode(propertyAccessMode);
}
