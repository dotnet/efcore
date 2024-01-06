// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     Provides a simple API surface for configuring a <see cref="IMutableModel" /> that defines the shape of your
///     entities, the relationships between them, and how they map to the database.
/// </summary>
/// <remarks>
///     <para>
///         You can use <see cref="ModelBuilder" /> to construct a model for a context by overriding
///         <see cref="DbContext.OnModelCreating(ModelBuilder)" /> on your derived context. Alternatively you can create the
///         model externally and set it on a <see cref="DbContextOptions" /> instance that is passed to the context constructor.
///     </para>
///     <para>
///         See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships in EF Core</see> for more information
///         and examples.
///     </para>
/// </remarks>
public class ModelBuilder : IInfrastructure<IConventionModelBuilder>
{
    private readonly InternalModelBuilder _builder;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ModelBuilder" /> class that will
    ///     apply a set of conventions.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> and
    ///     <see href="https://aka.ms/efcore-docs-conventions">EF Core model-building conventions</see> for more information and examples.
    /// </remarks>
    /// <param name="conventions">The conventions to be applied to the model.</param>
    public ModelBuilder(ConventionSet conventions)
        : this(conventions, null, null)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ModelBuilder" /> class that will
    ///     apply a set of conventions.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> and
    ///     <see href="https://aka.ms/efcore-docs-conventions">EF Core model-building conventions</see> for more information and examples.
    /// </remarks>
    /// <param name="conventions">The conventions to be applied to the model.</param>
    /// <param name="modelDependencies">The dependencies object for the model.</param>
    public ModelBuilder(ConventionSet conventions, ModelDependencies modelDependencies)
        : this(conventions, modelDependencies, null)
    {
        Check.NotNull(modelDependencies, nameof(modelDependencies));
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public ModelBuilder(ConventionSet conventions, ModelDependencies? modelDependencies, ModelConfiguration? modelConfiguration)
    {
        Check.NotNull(conventions, nameof(conventions));

        _builder = new Model(conventions, modelDependencies, modelConfiguration).Builder;

        _builder.Metadata.SetProductVersion(ProductInfo.GetVersion());
    }

    /// <summary>
    ///     <para>
    ///         Initializes a new instance of the <see cref="ModelBuilder" /> class with no conventions.
    ///     </para>
    ///     <para>
    ///         Warning: conventions are needed to build a correct model.
    ///     </para>
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> and
    ///     <see href="https://aka.ms/efcore-docs-conventions">EF Core model-building conventions</see> for more information and examples.
    /// </remarks>
    public ModelBuilder()
    {
        _builder = new Model().Builder;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public ModelBuilder(IMutableModel model)
    {
        _builder = ((Model)model).Builder;
    }

    /// <summary>
    ///     The model being configured.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships in EF Core</see> for more information and
    ///     examples.
    /// </remarks>
    public virtual IMutableModel Model
        => Builder.Metadata;

    /// <summary>
    ///     Adds or updates an annotation on the model. If an annotation with the key specified in
    ///     <paramref name="annotation" /> already exists its value will be updated.
    /// </summary>
    /// <param name="annotation">The key of the annotation to be added or updated.</param>
    /// <param name="value">The value to be stored in the annotation.</param>
    /// <returns>The same <see cref="ModelBuilder" /> instance so that multiple configuration calls can be chained.</returns>
    public virtual ModelBuilder HasAnnotation(string annotation, object? value)
    {
        Check.NotEmpty(annotation, nameof(annotation));

        Builder.HasAnnotation(annotation, value, ConfigurationSource.Explicit);

        return this;
    }

    /// <summary>
    ///     The internal <see cref="ModelBuilder" /> being used to configure this model.
    /// </summary>
    /// <remarks>
    ///     This property is intended for use by extension methods to configure the model. It is not intended to be used in
    ///     application code.
    /// </remarks>
    IConventionModelBuilder IInfrastructure<IConventionModelBuilder>.Instance
        => _builder;

    /// <summary>
    ///     Returns an object that can be used to configure a given entity type in the model.
    ///     If the entity type is not already part of the model, it will be added to the model.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types</see> for more information and examples.
    /// </remarks>
    /// <typeparam name="TEntity">The entity type to be configured.</typeparam>
    /// <returns>An object that can be used to configure the entity type.</returns>
    public virtual EntityTypeBuilder<TEntity> Entity<
        [DynamicallyAccessedMembers(IEntityType.DynamicallyAccessedMemberTypes)] TEntity>()
        where TEntity : class
        => new(Builder.Entity(typeof(TEntity), ConfigurationSource.Explicit, shouldBeOwned: false)!.Metadata);

    /// <summary>
    ///     Returns an object that can be used to configure a given shared type entity type in the model.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         If an entity type with the provided name is not already part of the model, a new entity type with provided CLR
    ///         type will be added to the model as shared type entity type.
    ///     </para>
    ///     <para>
    ///         Shared type entity type is an entity type which can share CLR type with other types in the model but has
    ///         a unique name and always identified by the name.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types</see> and
    ///         <see href="https://aka.ms/efcore-docs-shared-types">Shared entity types</see> for more information and examples.
    ///     </para>
    /// </remarks>
    /// <typeparam name="TEntity">The CLR type of the entity type to be configured.</typeparam>
    /// <param name="name">The name of the entity type to be configured.</param>
    /// <returns>An object that can be used to configure the entity type.</returns>
    public virtual EntityTypeBuilder<TEntity> SharedTypeEntity<
        [DynamicallyAccessedMembers(IEntityType.DynamicallyAccessedMemberTypes)] TEntity>(
        string name)
        where TEntity : class
    {
        Check.NotEmpty(name, nameof(name));

        return new EntityTypeBuilder<TEntity>(Builder.SharedTypeEntity(name, typeof(TEntity), ConfigurationSource.Explicit)!.Metadata);
    }

    /// <summary>
    ///     Returns an object that can be used to configure a given entity type in the model.
    ///     If the entity type is not already part of the model, it will be added to the model.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types</see> for more information and examples.
    /// </remarks>
    /// <param name="type">The entity type to be configured.</param>
    /// <returns>An object that can be used to configure the entity type.</returns>
    public virtual EntityTypeBuilder Entity([DynamicallyAccessedMembers(IEntityType.DynamicallyAccessedMemberTypes)] Type type)
    {
        Check.NotNull(type, nameof(type));

        return new EntityTypeBuilder(Builder.Entity(type, ConfigurationSource.Explicit, shouldBeOwned: false)!.Metadata);
    }

    /// <summary>
    ///     Returns an object that can be used to configure a given entity type in the model.
    ///     If an entity type with the provided name is not already part of the model,
    ///     a new entity type that does not have a corresponding CLR type will be added to the model.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types</see> for more information and examples.
    /// </remarks>
    /// <param name="name">The name of the entity type to be configured.</param>
    /// <returns>An object that can be used to configure the entity type.</returns>
    public virtual EntityTypeBuilder Entity(string name)
    {
        Check.NotEmpty(name, nameof(name));

        return new EntityTypeBuilder(Builder.Entity(name, ConfigurationSource.Explicit, shouldBeOwned: false)!.Metadata);
    }

    /// <summary>
    ///     Returns an object that can be used to configure a given shared type entity type in the model.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         If an entity type with the provided name is not already part of the model, a new entity type with provided CLR
    ///         type will be added to the model as shared type entity type.
    ///     </para>
    ///     <para>
    ///         Shared type entity type is an entity type which can share CLR type with other types in the model but has
    ///         a unique name and always identified by the name.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types</see> and
    ///         <see href="https://aka.ms/efcore-docs-shared-types">Shared entity types</see> for more information and examples.
    ///     </para>
    /// </remarks>
    /// <param name="name">The name of the entity type to be configured.</param>
    /// <param name="type">The CLR type of the entity type to be configured.</param>
    /// <returns>An object that can be used to configure the entity type.</returns>
    public virtual EntityTypeBuilder SharedTypeEntity(
        string name,
        [DynamicallyAccessedMembers(IEntityType.DynamicallyAccessedMemberTypes)] Type type)
    {
        Check.NotEmpty(name, nameof(name));
        Check.NotNull(type, nameof(type));

        return new EntityTypeBuilder(
            Builder.SharedTypeEntity(name, type, ConfigurationSource.Explicit, shouldBeOwned: false)!.Metadata);
    }

    /// <summary>
    ///     Performs configuration of a given entity type in the model. If the entity type is not already part
    ///     of the model, it will be added to the model.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This overload allows configuration of the entity type to be done in line in the method call rather
    ///         than being chained after a call to <see cref="Entity{TEntity}()" />. This allows additional
    ///         configuration at the model level to be chained after configuration for the entity type.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types</see> for more information and examples.
    ///     </para>
    /// </remarks>
    /// <typeparam name="TEntity">The entity type to be configured.</typeparam>
    /// <param name="buildAction">An action that performs configuration of the entity type.</param>
    /// <returns>
    ///     The same <see cref="ModelBuilder" /> instance so that additional configuration calls can be chained.
    /// </returns>
    public virtual ModelBuilder Entity<[DynamicallyAccessedMembers(IEntityType.DynamicallyAccessedMemberTypes)] TEntity>(
        Action<EntityTypeBuilder<TEntity>> buildAction)
        where TEntity : class
    {
        Check.NotNull(buildAction, nameof(buildAction));

        buildAction(Entity<TEntity>());

        return this;
    }

    /// <summary>
    ///     Returns an object that can be used to configure a given shared type entity type in the model.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         If an entity type with the provided name is not already part of the model, a new entity type with provided CLR
    ///         type will be added to the model as shared type entity type.
    ///     </para>
    ///     <para>
    ///         Shared type entity type is an entity type which can share CLR type with other types in the model but has
    ///         a unique name and always identified by the name.
    ///     </para>
    ///     <para>
    ///         This overload allows configuration of the entity type to be done inline in the method call rather
    ///         than being chained after a call to <see cref="Entity{TEntity}()" />. This allows additional
    ///         configuration at the model level to be chained after configuration for the entity type.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types</see> and
    ///         <see href="https://aka.ms/efcore-docs-shared-types">Shared entity types</see> for more information and examples.
    ///     </para>
    /// </remarks>
    /// <typeparam name="TEntity">The CLR type of the entity type to be configured.</typeparam>
    /// <param name="name">The name of the entity type to be configured.</param>
    /// <param name="buildAction">An action that performs configuration of the entity type.</param>
    /// <returns>
    ///     The same <see cref="ModelBuilder" /> instance so that additional configuration calls can be chained.
    /// </returns>
    public virtual ModelBuilder SharedTypeEntity<[DynamicallyAccessedMembers(IEntityType.DynamicallyAccessedMemberTypes)] TEntity>(
        string name,
        Action<EntityTypeBuilder<TEntity>> buildAction)
        where TEntity : class
    {
        Check.NotNull(buildAction, nameof(buildAction));

        buildAction(SharedTypeEntity<TEntity>(name));

        return this;
    }

    /// <summary>
    ///     Performs configuration of a given entity type in the model. If the entity type is not already part
    ///     of the model, it will be added to the model.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This overload allows configuration of the entity type to be done in line in the method call rather
    ///         than being chained after a call to <see cref="Entity{TEntity}()" />. This allows additional
    ///         configuration at the model level to be chained after configuration for the entity type.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types</see> for more information and examples.
    ///     </para>
    /// </remarks>
    /// <param name="type">The entity type to be configured.</param>
    /// <param name="buildAction">An action that performs configuration of the entity type.</param>
    /// <returns>
    ///     The same <see cref="ModelBuilder" /> instance so that additional configuration calls can be chained.
    /// </returns>
    public virtual ModelBuilder Entity(
        [DynamicallyAccessedMembers(IEntityType.DynamicallyAccessedMemberTypes)] Type type,
        Action<EntityTypeBuilder> buildAction)
    {
        Check.NotNull(buildAction, nameof(buildAction));

        buildAction(Entity(type));

        return this;
    }

    /// <summary>
    ///     Performs configuration of a given entity type in the model.
    ///     If an entity type with the provided name is not already part of the model,
    ///     a new entity type that does not have a corresponding CLR type will be added to the model.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This overload allows configuration of the entity type to be done in line in the method call rather
    ///         than being chained after a call to <see cref="Entity(string)" />. This allows additional
    ///         configuration at the model level to be chained after configuration for the entity type.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types</see> for more information and examples.
    ///     </para>
    /// </remarks>
    /// <param name="name">The name of the entity type to be configured.</param>
    /// <param name="buildAction">An action that performs configuration of the entity type.</param>
    /// <returns>
    ///     The same <see cref="ModelBuilder" /> instance so that additional configuration calls can be chained.
    /// </returns>
    public virtual ModelBuilder Entity(string name, Action<EntityTypeBuilder> buildAction)
    {
        Check.NotNull(buildAction, nameof(buildAction));

        buildAction(Entity(name));

        return this;
    }

    /// <summary>
    ///     Returns an object that can be used to configure a given shared type entity type in the model.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         If an entity type with the provided name is not already part of the model, a new entity type with provided CLR
    ///         type will be added to the model as shared type entity type.
    ///     </para>
    ///     <para>
    ///         Shared type entity type is an entity type which can share CLR type with other types in the model but has
    ///         a unique name and always identified by the name.
    ///     </para>
    ///     <para>
    ///         This overload allows configuration of the entity type to be done in line in the method call rather
    ///         than being chained after a call to <see cref="Entity(string)" />. This allows additional
    ///         configuration at the model level to be chained after configuration for the entity type.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types</see> and
    ///         <see href="https://aka.ms/efcore-docs-shared-types">Shared entity types</see> for more information and examples.
    ///     </para>
    /// </remarks>
    /// <param name="name">The name of the entity type to be configured.</param>
    /// <param name="type">The CLR type of the entity type to be configured.</param>
    /// <param name="buildAction">An action that performs configuration of the entity type.</param>
    /// <returns>
    ///     The same <see cref="ModelBuilder" /> instance so that additional configuration calls can be chained.
    /// </returns>
    public virtual ModelBuilder SharedTypeEntity(
        string name,
        [DynamicallyAccessedMembers(IEntityType.DynamicallyAccessedMemberTypes)] Type type,
        Action<EntityTypeBuilder> buildAction)
    {
        Check.NotNull(type, nameof(type));
        Check.NotNull(buildAction, nameof(buildAction));

        buildAction(SharedTypeEntity(name, type));

        return this;
    }

    /// <summary>
    ///     Excludes the given entity type from the model. This method is typically used to remove types from
    ///     the model that were added by convention.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types</see> for more information and examples.
    /// </remarks>
    /// <typeparam name="TEntity">The entity type to be removed from the model.</typeparam>
    /// <returns>
    ///     The same <see cref="ModelBuilder" /> instance so that additional configuration calls can be chained.
    /// </returns>
    public virtual ModelBuilder Ignore<[DynamicallyAccessedMembers(IEntityType.DynamicallyAccessedMemberTypes)] TEntity>()
        where TEntity : class
        => Ignore(typeof(TEntity));

    /// <summary>
    ///     Excludes an entity type with given CLR type from the model. This method is typically used to remove types from
    ///     the model that were added by convention.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types</see> for more information and examples.
    /// </remarks>
    /// <param name="type">The entity type to be removed from the model.</param>
    /// <returns>
    ///     The same <see cref="ModelBuilder" /> instance so that additional configuration calls can be chained.
    /// </returns>
    public virtual ModelBuilder Ignore([DynamicallyAccessedMembers(IEntityType.DynamicallyAccessedMemberTypes)] Type type)
    {
        Check.NotNull(type, nameof(type));

        Builder.Ignore(type, ConfigurationSource.Explicit);

        return this;
    }

    /// <summary>
    ///     Excludes an entity type with the given name from the model. This method is typically used to remove types from
    ///     the model that were added by convention.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types</see> for more information and examples.
    /// </remarks>
    /// <param name="typeName">The name of the entity type to be removed from the model.</param>
    /// <returns>
    ///     The same <see cref="ModelBuilder" /> instance so that additional configuration calls can be chained.
    /// </returns>
    public virtual ModelBuilder Ignore(string typeName)
    {
        Check.NotEmpty(typeName, nameof(typeName));

        Builder.Ignore(typeName, ConfigurationSource.Explicit);

        return this;
    }

    /// <summary>
    ///     Applies configuration that is defined in an <see cref="IEntityTypeConfiguration{TEntity}" /> instance.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
    /// </remarks>
    /// <typeparam name="TEntity">The entity type to be configured.</typeparam>
    /// <param name="configuration">The configuration to be applied.</param>
    /// <returns>
    ///     The same <see cref="ModelBuilder" /> instance so that additional configuration calls can be chained.
    /// </returns>
    public virtual ModelBuilder ApplyConfiguration
        <[DynamicallyAccessedMembers(IEntityType.DynamicallyAccessedMemberTypes)] TEntity>(IEntityTypeConfiguration<TEntity> configuration)
        where TEntity : class
    {
        Check.NotNull(configuration, nameof(configuration));

        configuration.Configure(Entity<TEntity>());

        return this;
    }

    /// <summary>
    ///     Applies configuration from all <see cref="IEntityTypeConfiguration{TEntity}" />
    ///     instances that are defined in provided assembly.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
    /// </remarks>
    /// <param name="assembly">The assembly to scan.</param>
    /// <param name="predicate">Optional predicate to filter types within the assembly.</param>
    /// <returns>
    ///     The same <see cref="ModelBuilder" /> instance so that additional configuration calls can be chained.
    /// </returns>
    [RequiresUnreferencedCode("This API isn't safe for trimming, since it searches for types in an arbitrary assembly.")]
    public virtual ModelBuilder ApplyConfigurationsFromAssembly(
        Assembly assembly,
        Func<Type, bool>? predicate = null)
    {
        var applyEntityConfigurationMethod = typeof(ModelBuilder)
            .GetMethods()
            .Single(
                e => e is { Name: nameof(ApplyConfiguration), ContainsGenericParameters: true }
                    && e.GetParameters().SingleOrDefault()?.ParameterType.GetGenericTypeDefinition()
                    == typeof(IEntityTypeConfiguration<>));

        var logger = Builder.Metadata.ScopedModelDependencies?.Logger;
        var foundOne = false;
        foreach (var type in assembly.GetConstructibleTypes(logger).OrderBy(t => t.FullName))
        {
            // Only accept types that contain a parameterless constructor, are not abstract and satisfy a predicate if it was used.
            var hasConstructor
                = type.GetConstructor(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, Type.EmptyTypes) != null;

            if (hasConstructor && predicate != null && !predicate(type))
            {
                continue;
            }

            foreach (var @interface in type.GetInterfaces())
            {
                if (!@interface.IsGenericType)
                {
                    continue;
                }

                if (@interface.GetGenericTypeDefinition() == typeof(IEntityTypeConfiguration<>))
                {
                    if (hasConstructor)
                    {
                        var target = applyEntityConfigurationMethod.MakeGenericMethod(@interface.GenericTypeArguments[0]);
                        target.Invoke(this, [Activator.CreateInstance(type, nonPublic: true)]);
                        foundOne = true;
                    }
                    else
                    {
                        logger?.SkippedEntityTypeConfigurationWarning(type);
                    }
                }
            }
        }

        if (!foundOne)
        {
            logger?.NoEntityTypeConfigurationsWarning(assembly);
        }

        return this;
    }

    /// <summary>
    ///     Marks an entity type as owned. All references to this type will be configured as
    ///     separate owned type instances.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-owned">Owned types in EF Core</see> for more information and examples.
    /// </remarks>
    /// <typeparam name="T">The entity type to be configured.</typeparam>
    public virtual OwnedEntityTypeBuilder<T> Owned<[DynamicallyAccessedMembers(IEntityType.DynamicallyAccessedMemberTypes)] T>()
        where T : class
    {
        Builder.Owned(typeof(T), ConfigurationSource.Explicit);

        return new OwnedEntityTypeBuilder<T>();
    }

    /// <summary>
    ///     Marks an entity type as owned. All references to this type will be configured as
    ///     separate owned type instances.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-owned">Owned types in EF Core</see> for more information and examples.
    /// </remarks>
    /// <param name="type">The entity type to be configured.</param>
    public virtual OwnedEntityTypeBuilder Owned([DynamicallyAccessedMembers(IEntityType.DynamicallyAccessedMemberTypes)] Type type)
    {
        Check.NotNull(type, nameof(type));

        Builder.Owned(type, ConfigurationSource.Explicit);

        return new OwnedEntityTypeBuilder();
    }

    /// <summary>
    ///     Configures the default <see cref="ChangeTrackingStrategy" /> to be used for this model.
    ///     This strategy indicates how the context detects changes to properties for an instance of an entity type.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-change-detection">Change detection and notifications</see> for more information and examples.
    /// </remarks>
    /// <param name="changeTrackingStrategy">The change tracking strategy to be used.</param>
    /// <returns>
    ///     The same <see cref="ModelBuilder" /> instance so that additional configuration calls can be chained.
    /// </returns>
    public virtual ModelBuilder HasChangeTrackingStrategy(ChangeTrackingStrategy changeTrackingStrategy)
    {
        Builder.HasChangeTrackingStrategy(changeTrackingStrategy, ConfigurationSource.Explicit);

        return this;
    }

    /// <summary>
    ///     Sets the <see cref="PropertyAccessMode" /> to use for all properties of this entity type.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         By default, the backing field, if one is found by convention or has been specified, is used when
    ///         new objects are constructed, typically when entities are queried from the database.
    ///         Properties are used for all other accesses. Calling this method will change that behavior
    ///         for all properties in the model as described in the <see cref="PropertyAccessMode" /> enum.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-property-access">Property versus field access in EF Core</see> for more information and
    ///         examples.
    ///     </para>
    /// </remarks>
    /// <param name="propertyAccessMode">The <see cref="PropertyAccessMode" /> to use for properties of this model.</param>
    /// <returns>
    ///     The same <see cref="ModelBuilder" /> instance so that additional configuration calls can be chained.
    /// </returns>
    public virtual ModelBuilder UsePropertyAccessMode(PropertyAccessMode propertyAccessMode)
    {
        Builder.UsePropertyAccessMode(propertyAccessMode, ConfigurationSource.Explicit);

        return this;
    }

    /// <summary>
    ///     Forces post-processing on the model such that it is ready for use by the runtime. This post
    ///     processing happens automatically when using <see cref="DbContext.OnModelCreating" />; this method allows it to be run
    ///     explicitly in cases where the automatic execution is not possible.
    /// </summary>
    /// <returns>The finalized model.</returns>
    public virtual IModel FinalizeModel()
        => Builder.Metadata.FinalizeModel();

    private InternalModelBuilder Builder
        => (InternalModelBuilder)this.GetInfrastructure();

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
