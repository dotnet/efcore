// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     Provides a simple API surface for setting defaults and configuring conventions before they run.
/// </summary>
/// <remarks>
///     <para>
///         You can use <see cref="ModelConfigurationBuilder" /> to configure the conventions for a context by overriding
///         <see cref="DbContext.ConfigureConventions(ModelConfigurationBuilder)" /> on your derived context.
///         Alternatively you can create the model externally and set it on a <see cref="DbContextOptions" /> instance
///         that is passed to the context constructor.
///     </para>
///     <para>
///         See <see href="https://aka.ms/efcore-docs-pre-convention">Pre-convention model building in EF Core</see> for more information and
///         examples.
///     </para>
/// </remarks>
public class ModelConfigurationBuilder
{
    private readonly ModelConfiguration _modelConfiguration = new();
    private readonly ConventionSet _conventions;
    private readonly ConventionSetBuilder _conventionSetBuilder;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public ModelConfigurationBuilder(ConventionSet conventions, IServiceProvider serviceProvider)
    {
        Check.NotNull(conventions, nameof(conventions));

        _conventions = conventions;
        _conventionSetBuilder = new ConventionSetBuilder(conventions, serviceProvider);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    protected virtual ModelConfiguration ModelConfiguration
        => _modelConfiguration;

    /// <summary>
    ///     Gets the builder for the conventions that will be used in the model.
    /// </summary>
    public virtual ConventionSetBuilder Conventions
        => _conventionSetBuilder;

    /// <summary>
    ///     Prevents the conventions from the given type from discovering properties of the given or derived types.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-pre-convention">Pre-convention model building in EF Core</see> for more information and
    ///     examples.
    /// </remarks>
    /// <typeparam name="T">The type to be ignored.</typeparam>
    /// <returns>
    ///     The same <see cref="ModelConfigurationBuilder" /> instance so that additional configuration calls can be chained.
    /// </returns>
    public virtual ModelConfigurationBuilder IgnoreAny<T>()
        => IgnoreAny(typeof(T));

    /// <summary>
    ///     Prevents the conventions from the given type from discovering properties of the given or derived types.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-pre-convention">Pre-convention model building in EF Core</see> for more information and
    ///     examples.
    /// </remarks>
    /// <param name="type">The type to be ignored.</param>
    /// <returns>
    ///     The same <see cref="ModelConfigurationBuilder" /> instance so that additional configuration calls can be chained.
    /// </returns>
    public virtual ModelConfigurationBuilder IgnoreAny(Type type)
    {
        Check.NotNull(type, nameof(type));

        _modelConfiguration.AddIgnored(type);

        return this;
    }

    /// <summary>
    ///     Marks the given and derived types as corresponding to entity type properties.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This can also be called on an interface to apply the configuration to all properties of implementing types.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-pre-convention">Pre-convention model building in EF Core</see> for more information and
    ///         examples.
    ///     </para>
    /// </remarks>
    /// <typeparam name="TProperty">The property type to be configured.</typeparam>
    /// <returns>An object that can be used to configure the properties.</returns>
    public virtual PropertiesConfigurationBuilder<TProperty> Properties<TProperty>()
    {
        var property = _modelConfiguration.GetOrAddProperty(typeof(TProperty));

        return new PropertiesConfigurationBuilder<TProperty>(property);
    }

    /// <summary>
    ///     Marks the given and derived types as corresponding to entity type properties.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This can also be called on an interface to apply the configuration to all properties of implementing types.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-pre-convention">Pre-convention model building in EF Core</see> for more information and
    ///         examples.
    ///     </para>
    /// </remarks>
    /// <typeparam name="TProperty">The property type to be configured.</typeparam>
    /// <param name="buildAction">An action that performs configuration of the property.</param>
    /// <returns>
    ///     The same <see cref="ModelConfigurationBuilder" /> instance so that additional configuration calls can be chained.
    /// </returns>
    public virtual ModelConfigurationBuilder Properties<TProperty>(
        Action<PropertiesConfigurationBuilder<TProperty>> buildAction)
    {
        Check.NotNull(buildAction, nameof(buildAction));

        var propertyBuilder = Properties<TProperty>();
        buildAction(propertyBuilder);

        return this;
    }

    /// <summary>
    ///     Marks the given and derived types as corresponding to entity type properties.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This can also be called on an interface or an unbound generic type to apply the configuration to all
    ///         properties of implementing and constructed types.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-pre-convention">Pre-convention model building in EF Core</see> for more information and
    ///         examples.
    ///     </para>
    /// </remarks>
    /// <param name="propertyType">The property type to be configured.</param>
    /// <returns>An object that can be used to configure the property.</returns>
    public virtual PropertiesConfigurationBuilder Properties(Type propertyType)
    {
        Check.NotNull(propertyType, nameof(propertyType));

        var property = _modelConfiguration.GetOrAddProperty(propertyType);

        return new PropertiesConfigurationBuilder(property);
    }

    /// <summary>
    ///     Marks the given and derived types as corresponding to entity type properties.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This can also be called on an interface or an unbound generic type to apply the configuration to all
    ///         properties of implementing and constructed types.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-pre-convention">Pre-convention model building in EF Core</see> for more information and
    ///         examples.
    ///     </para>
    /// </remarks>
    /// <param name="propertyType">The property type to be configured.</param>
    /// <param name="buildAction">An action that performs configuration of the property.</param>
    /// <returns>
    ///     The same <see cref="ModelConfigurationBuilder" /> instance so that additional configuration calls can be chained.
    /// </returns>
    public virtual ModelConfigurationBuilder Properties(
        Type propertyType,
        Action<PropertiesConfigurationBuilder> buildAction)
    {
        Check.NotNull(propertyType, nameof(propertyType));
        Check.NotNull(buildAction, nameof(buildAction));

        var propertyBuilder = Properties(propertyType);
        buildAction(propertyBuilder);

        return this;
    }

    /// <summary>
    ///     Marks the given type as a scalar, even when used outside of entity types. This allows values of this type
    ///     to be used in queries that are not referencing property of this type.
    ///     Calling this won't affect whether properties of this type are discovered.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Unlike <see cref="Properties{TProperty}()" /> this method should only be called on a non-nullable concrete type.
    ///         Calling it on a base type will not apply the configuration to the derived types.
    ///     </para>
    ///     <para>
    ///         Calling this is rarely needed. If there are properties of the given type calling <see cref="Properties{TProperty}()" />
    ///         should be enough in most cases.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-pre-convention">Pre-convention model building in EF Core</see> for more information and
    ///         examples.
    ///     </para>
    /// </remarks>
    /// <typeparam name="TScalar">The scalar type to be configured.</typeparam>
    /// <returns>An object that can be used to configure the scalars.</returns>
    public virtual TypeMappingConfigurationBuilder<TScalar> DefaultTypeMapping<TScalar>()
    {
        var scalar = _modelConfiguration.GetOrAddTypeMapping(typeof(TScalar));

        return new TypeMappingConfigurationBuilder<TScalar>(scalar);
    }

    /// <summary>
    ///     Marks the given type as a scalar, even when used outside of entity types. This allows values of this type
    ///     to be used in queries that are not referencing property of this type.
    ///     Calling this won't affect whether properties of this type are discovered.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Unlike <see cref="Properties{TProperty}()" /> this method should only be called on a non-nullable concrete type.
    ///         Calling it on a base type will not apply the configuration to the derived types.
    ///     </para>
    ///     <para>
    ///         Calling this is rarely needed. If there are properties of the given type calling <see cref="Properties{TProperty}()" />
    ///         should be enough in most cases.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-pre-convention">Pre-convention model building in EF Core</see> for more information and
    ///         examples.
    ///     </para>
    /// </remarks>
    /// <typeparam name="TScalar">The scalar type to be configured.</typeparam>
    /// <param name="buildAction">An action that performs configuration for the scalars.</param>
    /// <returns>
    ///     The same <see cref="ModelConfigurationBuilder" /> instance so that additional configuration calls can be chained.
    /// </returns>
    public virtual ModelConfigurationBuilder DefaultTypeMapping<TScalar>(
        Action<TypeMappingConfigurationBuilder<TScalar>> buildAction)
    {
        Check.NotNull(buildAction, nameof(buildAction));

        var scalarBuilder = DefaultTypeMapping<TScalar>();
        buildAction(scalarBuilder);

        return this;
    }

    /// <summary>
    ///     Marks the given type as a scalar, even when used outside of entity types. This allows values of this type
    ///     to be used in queries that are not referencing property of this type.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Unlike <see cref="Properties(Type)" /> this method should only be called on a non-nullable concrete type.
    ///         Calling it on a base type will not apply the configuration to the derived types.
    ///     </para>
    ///     <para>
    ///         Calling this is rarely needed. If there are properties of the given type calling <see cref="Properties(Type)" />
    ///         should be enough in most cases.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-pre-convention">Pre-convention model building in EF Core</see> for more information and
    ///         examples.
    ///     </para>
    /// </remarks>
    /// <param name="scalarType">The scalar type to be configured.</param>
    /// <returns>An object that can be used to configure the scalars.</returns>
    public virtual TypeMappingConfigurationBuilder DefaultTypeMapping(Type scalarType)
    {
        Check.NotNull(scalarType, nameof(scalarType));

        var scalar = _modelConfiguration.GetOrAddTypeMapping(scalarType);

        return new TypeMappingConfigurationBuilder(scalar);
    }

    /// <summary>
    ///     Marks the given type as a scalar, even when used outside of entity types. This allows values of this type
    ///     to be used in queries that are not referencing property of this type.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Unlike <see cref="Properties(Type)" /> this method should only be called on a non-nullable concrete type.
    ///         Calling it on a base type will not apply the configuration to the derived types.
    ///     </para>
    ///     <para>
    ///         Calling this is rarely needed. If there are properties of the given type calling <see cref="Properties(Type)" />
    ///         should be enough in most cases.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-pre-convention">Pre-convention model building in EF Core</see> for more information and
    ///         examples.
    ///     </para>
    /// </remarks>
    /// <param name="scalarType">The scalar type to be configured.</param>
    /// <param name="buildAction">An action that performs configuration for the scalars.</param>
    /// <returns>
    ///     The same <see cref="ModelConfigurationBuilder" /> instance so that additional configuration calls can be chained.
    /// </returns>
    public virtual ModelConfigurationBuilder DefaultTypeMapping(
        Type scalarType,
        Action<TypeMappingConfigurationBuilder> buildAction)
    {
        Check.NotNull(scalarType, nameof(scalarType));
        Check.NotNull(buildAction, nameof(buildAction));

        var scalarBuilder = DefaultTypeMapping(scalarType);
        buildAction(scalarBuilder);

        return this;
    }

    /// <summary>
    ///     Marks the given and derived types as corresponding to complex properties.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This can also be called on an interface to apply the configuration to all properties of implementing types.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-pre-convention">Pre-convention model building in EF Core</see> for more information and
    ///         examples.
    ///     </para>
    /// </remarks>
    /// <typeparam name="TProperty">The property type to be configured.</typeparam>
    /// <returns>An object that can be used to configure the properties.</returns>
    public virtual ComplexPropertiesConfigurationBuilder<TProperty> ComplexProperties<TProperty>()
    {
        var property = _modelConfiguration.GetOrAddComplexProperty(typeof(TProperty));

        return new ComplexPropertiesConfigurationBuilder<TProperty>(property);
    }

    /// <summary>
    ///     Marks the given and derived types as corresponding to complex properties.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This can also be called on an interface or an unbound generic type to apply the configuration to all
    ///         properties of implementing and constructed types.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-pre-convention">Pre-convention model building in EF Core</see> for more information and
    ///         examples.
    ///     </para>
    /// </remarks>
    /// <param name="propertyType">The property type to be configured.</param>
    /// <returns>An object that can be used to configure the property.</returns>
    public virtual ComplexPropertiesConfigurationBuilder ComplexProperties(Type propertyType)
    {
        Check.NotNull(propertyType, nameof(propertyType));

        var property = _modelConfiguration.GetOrAddComplexProperty(propertyType);

        return new ComplexPropertiesConfigurationBuilder(property);
    }

    /// <summary>
    ///     Creates the configured <see cref="ModelBuilder" /> used to create the model. This is done automatically when using
    ///     <see cref="DbContext.OnModelCreating" />; this method allows it to be run
    ///     explicitly in cases where the automatic execution is not possible.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-pre-convention">Pre-convention model building in EF Core</see> for more information and
    ///     examples.
    /// </remarks>
    /// <param name="modelDependencies">The dependencies object used during model building.</param>
    /// <returns>The configured <see cref="ModelBuilder" />.</returns>
    public virtual ModelBuilder CreateModelBuilder(ModelDependencies? modelDependencies)
        => new(_conventions, modelDependencies, _modelConfiguration.IsEmpty() ? null : _modelConfiguration.Validate());

    #region Hidden System.Object members

    /// <summary>
    ///     Returns a string that represents the current object.
    /// </summary>
    /// <returns>A string that represents the current object.</returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public override string? ToString()
        => base.ToString();

    /// <summary>
    ///     Determines whether the specified object is equal to the current object.
    /// </summary>
    /// <param name="obj">The object to compare with the current object.</param>
    /// <returns><see langword="true" /> if the specified object is equal to the current object; otherwise, <see langword="false" />.</returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public override bool Equals(object? obj)
        => base.Equals(obj);

    /// <summary>
    ///     Serves as the default hash function.
    /// </summary>
    /// <returns>A hash code for the current object.</returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public override int GetHashCode()
        => base.GetHashCode();

    #endregion
}
