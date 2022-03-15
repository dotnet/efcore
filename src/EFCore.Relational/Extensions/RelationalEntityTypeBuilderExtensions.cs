// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     Relational database specific extension methods for <see cref="EntityTypeBuilder" />.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
/// </remarks>
public static class RelationalEntityTypeBuilderExtensions
{
    /// <summary>
    ///     Configures TPC as the mapping strategy for the derived types. Each type will be mapped to a different database object.
    ///     All properties will be mapped to columns on the corresponding object.
    /// </summary>
    /// <param name="entityTypeBuilder">The builder for the entity type being configured.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static EntityTypeBuilder UseTpcMappingStrategy(this EntityTypeBuilder entityTypeBuilder)
    {
        entityTypeBuilder.Metadata.SetMappingStrategy(RelationalAnnotationNames.TpcMappingStrategy);

        return entityTypeBuilder;
    }

    /// <summary>
    ///     Configures TPH as the mapping strategy for the derived types. All types will be mapped to the same database object.
    ///     This is the default mapping strategy.
    /// </summary>
    /// <param name="entityTypeBuilder">The builder for the entity type being configured.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static EntityTypeBuilder UseTphMappingStrategy(this EntityTypeBuilder entityTypeBuilder)
    {
        entityTypeBuilder.Metadata.SetMappingStrategy(RelationalAnnotationNames.TphMappingStrategy);

        return entityTypeBuilder;
    }

    /// <summary>
    ///     Configures TPT as the mapping strategy for the derived types. Each type will be mapped to a different database object.
    ///     Only the declared properties will be mapped to columns on the corresponding object.
    /// </summary>
    /// <param name="entityTypeBuilder">The builder for the entity type being configured.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static EntityTypeBuilder UseTptMappingStrategy(this EntityTypeBuilder entityTypeBuilder)
    {
        entityTypeBuilder.Metadata.SetMappingStrategy(RelationalAnnotationNames.TptMappingStrategy);

        return entityTypeBuilder;
    }

    /// <summary>
    ///     Configures TPC as the mapping strategy for the derived types. Each type will be mapped to a different database object.
    ///     All properties will be mapped to columns on the corresponding object.
    /// </summary>
    /// <param name="entityTypeBuilder">The builder for the entity type being configured.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static EntityTypeBuilder<TEntity> UseTpcMappingStrategy<TEntity>(this EntityTypeBuilder<TEntity> entityTypeBuilder)
        where TEntity : class
        => (EntityTypeBuilder<TEntity>)((EntityTypeBuilder)entityTypeBuilder).UseTpcMappingStrategy();

    /// <summary>
    ///     Configures TPH as the mapping strategy for the derived types. All types will be mapped to the same database object.
    ///     This is the default mapping strategy.
    /// </summary>
    /// <param name="entityTypeBuilder">The builder for the entity type being configured.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static EntityTypeBuilder<TEntity> UseTphMappingStrategy<TEntity>(this EntityTypeBuilder<TEntity> entityTypeBuilder)
        where TEntity : class
        => (EntityTypeBuilder<TEntity>)((EntityTypeBuilder)entityTypeBuilder).UseTphMappingStrategy();

    /// <summary>
    ///     Configures TPT as the mapping strategy for the derived types. Each type will be mapped to a different database object.
    ///     Only the declared properties will be mapped to columns on the corresponding object.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-migrations">Database migrations</see> for more information and examples.
    /// </remarks>
    /// <typeparam name="TEntity">The entity type being configured.</typeparam>
    /// <param name="entityTypeBuilder">The builder for the entity type being configured.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static EntityTypeBuilder<TEntity> UseTptMappingStrategy<TEntity>(this EntityTypeBuilder<TEntity> entityTypeBuilder)
        where TEntity : class
        => (EntityTypeBuilder<TEntity>)((EntityTypeBuilder)entityTypeBuilder).UseTptMappingStrategy();

    /// <summary>
    ///     Configures the table that the entity type maps to when targeting a relational database.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
    /// </remarks>
    /// <param name="entityTypeBuilder">The builder for the entity type being configured.</param>
    /// <param name="name">The name of the table.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static EntityTypeBuilder ToTable(
        this EntityTypeBuilder entityTypeBuilder,
        string? name)
    {
        Check.NullButNotEmpty(name, nameof(name));

        entityTypeBuilder.Metadata.SetTableName(name);
        entityTypeBuilder.Metadata.SetSchema(null);

        return entityTypeBuilder;
    }

    /// <summary>
    ///     Configures the table that the entity type maps to when targeting a relational database.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
    /// </remarks>
    /// <param name="entityTypeBuilder">The builder for the entity type being configured.</param>
    /// <param name="buildAction">An action that performs configuration of the table.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static EntityTypeBuilder ToTable(
        this EntityTypeBuilder entityTypeBuilder,
        Action<TableBuilder> buildAction)
    {
        Check.NotNull(buildAction, nameof(buildAction));

        buildAction(new TableBuilder(null, null, entityTypeBuilder));

        return entityTypeBuilder;
    }

    /// <summary>
    ///     Configures the table that the entity type maps to when targeting a relational database.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
    /// </remarks>
    /// <param name="entityTypeBuilder">The builder for the entity type being configured.</param>
    /// <param name="name">The name of the table.</param>
    /// <param name="buildAction">An action that performs configuration of the table.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static EntityTypeBuilder ToTable(
        this EntityTypeBuilder entityTypeBuilder,
        string? name,
        Action<TableBuilder> buildAction)
    {
        Check.NullButNotEmpty(name, nameof(name));
        Check.NotNull(buildAction, nameof(buildAction));

        entityTypeBuilder.Metadata.SetTableName(name);
        entityTypeBuilder.Metadata.SetSchema(null);
        buildAction(new TableBuilder(name, null, entityTypeBuilder));

        return entityTypeBuilder;
    }

    /// <summary>
    ///     Configures the table that the entity type maps to when targeting a relational database.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
    /// </remarks>
    /// <typeparam name="TEntity">The entity type being configured.</typeparam>
    /// <param name="entityTypeBuilder">The builder for the entity type being configured.</param>
    /// <param name="name">The name of the table.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static EntityTypeBuilder<TEntity> ToTable<TEntity>(
        this EntityTypeBuilder<TEntity> entityTypeBuilder,
        string? name)
        where TEntity : class
        => (EntityTypeBuilder<TEntity>)((EntityTypeBuilder)entityTypeBuilder).ToTable(name);

    /// <summary>
    ///     Configures the table that the entity type maps to when targeting a relational database.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
    /// </remarks>
    /// <typeparam name="TEntity">The entity type being configured.</typeparam>
    /// <param name="entityTypeBuilder">The builder for the entity type being configured.</param>
    /// <param name="buildAction">An action that performs configuration of the table.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static EntityTypeBuilder<TEntity> ToTable<TEntity>(
        this EntityTypeBuilder<TEntity> entityTypeBuilder,
        Action<TableBuilder<TEntity>> buildAction)
        where TEntity : class
    {
        Check.NotNull(buildAction, nameof(buildAction));

        buildAction(new TableBuilder<TEntity>(null, null, entityTypeBuilder));

        return entityTypeBuilder;
    }

    /// <summary>
    ///     Configures the table that the entity type maps to when targeting a relational database.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
    /// </remarks>
    /// <typeparam name="TEntity">The entity type being configured.</typeparam>
    /// <param name="entityTypeBuilder">The builder for the entity type being configured.</param>
    /// <param name="name">The name of the table.</param>
    /// <param name="buildAction">An action that performs configuration of the table.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static EntityTypeBuilder<TEntity> ToTable<TEntity>(
        this EntityTypeBuilder<TEntity> entityTypeBuilder,
        string? name,
        Action<TableBuilder<TEntity>> buildAction)
        where TEntity : class
    {
        Check.NullButNotEmpty(name, nameof(name));
        Check.NotNull(buildAction, nameof(buildAction));

        entityTypeBuilder.Metadata.SetTableName(name);
        entityTypeBuilder.Metadata.SetSchema(null);
        buildAction(new TableBuilder<TEntity>(name, null, entityTypeBuilder));

        return entityTypeBuilder;
    }

    /// <summary>
    ///     Configures the table that the entity type maps to when targeting a relational database.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
    /// </remarks>
    /// <param name="entityTypeBuilder">The builder for the entity type being configured.</param>
    /// <param name="name">The name of the table.</param>
    /// <param name="schema">The schema of the table.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static EntityTypeBuilder ToTable(
        this EntityTypeBuilder entityTypeBuilder,
        string name,
        string? schema)
    {
        Check.NotNull(name, nameof(name));
        Check.NullButNotEmpty(schema, nameof(schema));

        entityTypeBuilder.Metadata.SetTableName(name);
        entityTypeBuilder.Metadata.SetSchema(schema);
        return entityTypeBuilder;
    }

    /// <summary>
    ///     Configures the table that the entity type maps to when targeting a relational database.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
    /// </remarks>
    /// <param name="entityTypeBuilder">The builder for the entity type being configured.</param>
    /// <param name="name">The name of the table.</param>
    /// <param name="schema">The schema of the table.</param>
    /// <param name="buildAction">An action that performs configuration of the table.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static EntityTypeBuilder ToTable(
        this EntityTypeBuilder entityTypeBuilder,
        string name,
        string? schema,
        Action<TableBuilder> buildAction)
    {
        Check.NotNull(name, nameof(name));
        Check.NullButNotEmpty(schema, nameof(schema));
        Check.NotNull(buildAction, nameof(buildAction));

        entityTypeBuilder.Metadata.SetTableName(name);
        entityTypeBuilder.Metadata.SetSchema(schema);
        buildAction(new TableBuilder(name, schema, entityTypeBuilder));

        return entityTypeBuilder;
    }

    /// <summary>
    ///     Configures the table that the entity type maps to when targeting a relational database.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
    /// </remarks>
    /// <typeparam name="TEntity">The entity type being configured.</typeparam>
    /// <param name="entityTypeBuilder">The builder for the entity type being configured.</param>
    /// <param name="name">The name of the table.</param>
    /// <param name="schema">The schema of the table.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static EntityTypeBuilder<TEntity> ToTable<TEntity>(
        this EntityTypeBuilder<TEntity> entityTypeBuilder,
        string name,
        string? schema)
        where TEntity : class
        => (EntityTypeBuilder<TEntity>)((EntityTypeBuilder)entityTypeBuilder).ToTable(name, schema);

    /// <summary>
    ///     Configures the table that the entity type maps to when targeting a relational database.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
    /// </remarks>
    /// <typeparam name="TEntity">The entity type being configured.</typeparam>
    /// <param name="entityTypeBuilder">The builder for the entity type being configured.</param>
    /// <param name="name">The name of the table.</param>
    /// <param name="schema">The schema of the table.</param>
    /// <param name="buildAction">An action that performs configuration of the table.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static EntityTypeBuilder<TEntity> ToTable<TEntity>(
        this EntityTypeBuilder<TEntity> entityTypeBuilder,
        string name,
        string? schema,
        Action<TableBuilder<TEntity>> buildAction)
        where TEntity : class
    {
        Check.NotNull(name, nameof(name));
        Check.NullButNotEmpty(schema, nameof(schema));
        Check.NotNull(buildAction, nameof(buildAction));

        entityTypeBuilder.Metadata.SetTableName(name);
        entityTypeBuilder.Metadata.SetSchema(schema);
        buildAction(new TableBuilder<TEntity>(name, schema, entityTypeBuilder));

        return entityTypeBuilder;
    }

    /// <summary>
    ///     Configures the table that the entity type maps to when targeting a relational database.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
    /// </remarks>
    /// <param name="referenceOwnershipBuilder">The builder for the entity type being configured.</param>
    /// <param name="name">The name of the table.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static OwnedNavigationBuilder ToTable(
        this OwnedNavigationBuilder referenceOwnershipBuilder,
        string? name)
    {
        Check.NullButNotEmpty(name, nameof(name));

        referenceOwnershipBuilder.OwnedEntityType.SetTableName(name);
        referenceOwnershipBuilder.OwnedEntityType.SetSchema(null);

        return referenceOwnershipBuilder;
    }

    /// <summary>
    ///     Configures the table that the entity type maps to when targeting a relational database.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
    /// </remarks>
    /// <param name="referenceOwnershipBuilder">The builder for the entity type being configured.</param>
    /// <param name="buildAction">An action that performs configuration of the table.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static OwnedNavigationBuilder ToTable(
        this OwnedNavigationBuilder referenceOwnershipBuilder,
        Action<OwnedNavigationTableBuilder> buildAction)
    {
        Check.NotNull(buildAction, nameof(buildAction));

        buildAction(new OwnedNavigationTableBuilder(null, null, referenceOwnershipBuilder));

        return referenceOwnershipBuilder;
    }

    /// <summary>
    ///     Configures the table that the entity type maps to when targeting a relational database.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
    /// </remarks>
    /// <param name="referenceOwnershipBuilder">The builder for the entity type being configured.</param>
    /// <param name="buildAction">An action that performs configuration of the table.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static OwnedNavigationBuilder<TOwnerEntity, TRelatedEntity> ToTable<TOwnerEntity, TRelatedEntity>(
        this OwnedNavigationBuilder<TOwnerEntity, TRelatedEntity> referenceOwnershipBuilder,
        Action<OwnedNavigationTableBuilder<TRelatedEntity>> buildAction)
        where TOwnerEntity : class
        where TRelatedEntity : class
    {
        Check.NotNull(buildAction, nameof(buildAction));

        buildAction(new OwnedNavigationTableBuilder<TRelatedEntity>(null, null, referenceOwnershipBuilder));

        return referenceOwnershipBuilder;
    }

    /// <summary>
    ///     Configures the table that the entity type maps to when targeting a relational database.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
    /// </remarks>
    /// <param name="referenceOwnershipBuilder">The builder for the entity type being configured.</param>
    /// <param name="name">The name of the table.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static OwnedNavigationBuilder<TOwnerEntity, TRelatedEntity> ToTable<TOwnerEntity, TRelatedEntity>(
        this OwnedNavigationBuilder<TOwnerEntity, TRelatedEntity> referenceOwnershipBuilder,
        string? name)
        where TOwnerEntity : class
        where TRelatedEntity : class
        => (OwnedNavigationBuilder<TOwnerEntity, TRelatedEntity>)((OwnedNavigationBuilder)referenceOwnershipBuilder).ToTable(name);

    /// <summary>
    ///     Configures the table that the entity type maps to when targeting a relational database.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
    /// </remarks>
    /// <param name="referenceOwnershipBuilder">The builder for the entity type being configured.</param>
    /// <param name="name">The name of the table.</param>
    /// <param name="buildAction">An action that performs configuration of the table.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static OwnedNavigationBuilder ToTable(
        this OwnedNavigationBuilder referenceOwnershipBuilder,
        string? name,
        Action<OwnedNavigationTableBuilder> buildAction)
    {
        Check.NullButNotEmpty(name, nameof(name));
        Check.NotNull(buildAction, nameof(buildAction));

        referenceOwnershipBuilder.OwnedEntityType.SetTableName(name);
        referenceOwnershipBuilder.OwnedEntityType.SetSchema(null);
        buildAction(new OwnedNavigationTableBuilder(name, null, referenceOwnershipBuilder));

        return referenceOwnershipBuilder;
    }

    /// <summary>
    ///     Configures the table that the entity type maps to when targeting a relational database.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
    /// </remarks>
    /// <param name="referenceOwnershipBuilder">The builder for the entity type being configured.</param>
    /// <param name="name">The name of the table.</param>
    /// <param name="buildAction">An action that performs configuration of the table.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static OwnedNavigationBuilder<TOwnerEntity, TRelatedEntity> ToTable<TOwnerEntity, TRelatedEntity>(
        this OwnedNavigationBuilder<TOwnerEntity, TRelatedEntity> referenceOwnershipBuilder,
        string? name,
        Action<OwnedNavigationTableBuilder<TRelatedEntity>> buildAction)
        where TOwnerEntity : class
        where TRelatedEntity : class
    {
        Check.NullButNotEmpty(name, nameof(name));
        Check.NotNull(buildAction, nameof(buildAction));

        referenceOwnershipBuilder.OwnedEntityType.SetTableName(name);
        referenceOwnershipBuilder.OwnedEntityType.SetSchema(null);
        buildAction(new OwnedNavigationTableBuilder<TRelatedEntity>(name, null, referenceOwnershipBuilder));

        return referenceOwnershipBuilder;
    }

    /// <summary>
    ///     Configures the table that the entity type maps to when targeting a relational database.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
    /// </remarks>
    /// <param name="referenceOwnershipBuilder">The builder for the entity type being configured.</param>
    /// <param name="name">The name of the table.</param>
    /// <param name="schema">The schema of the table.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static OwnedNavigationBuilder ToTable(
        this OwnedNavigationBuilder referenceOwnershipBuilder,
        string name,
        string? schema)
    {
        Check.NotNull(name, nameof(name));
        Check.NullButNotEmpty(schema, nameof(schema));

        referenceOwnershipBuilder.OwnedEntityType.SetTableName(name);
        referenceOwnershipBuilder.OwnedEntityType.SetSchema(schema);

        return referenceOwnershipBuilder;
    }

    /// <summary>
    ///     Configures the table that the entity type maps to when targeting a relational database.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
    /// </remarks>
    /// <param name="referenceOwnershipBuilder">The builder for the entity type being configured.</param>
    /// <param name="name">The name of the table.</param>
    /// <param name="schema">The schema of the table.</param>
    /// <param name="buildAction">An action that performs configuration of the table.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static OwnedNavigationBuilder ToTable(
        this OwnedNavigationBuilder referenceOwnershipBuilder,
        string name,
        string? schema,
        Action<OwnedNavigationTableBuilder> buildAction)
    {
        Check.NotNull(name, nameof(name));
        Check.NullButNotEmpty(schema, nameof(schema));
        Check.NotNull(buildAction, nameof(buildAction));

        referenceOwnershipBuilder.OwnedEntityType.SetTableName(name);
        referenceOwnershipBuilder.OwnedEntityType.SetSchema(schema);
        buildAction(new OwnedNavigationTableBuilder(name, schema, referenceOwnershipBuilder));

        return referenceOwnershipBuilder;
    }

    /// <summary>
    ///     Configures the table that the entity type maps to when targeting a relational database.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
    /// </remarks>
    /// <param name="referenceOwnershipBuilder">The builder for the entity type being configured.</param>
    /// <param name="name">The name of the table.</param>
    /// <param name="schema">The schema of the table.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static OwnedNavigationBuilder<TOwnerEntity, TRelatedEntity> ToTable<TOwnerEntity, TRelatedEntity>(
        this OwnedNavigationBuilder<TOwnerEntity, TRelatedEntity> referenceOwnershipBuilder,
        string name,
        string? schema)
        where TOwnerEntity : class
        where TRelatedEntity : class
        => (OwnedNavigationBuilder<TOwnerEntity, TRelatedEntity>)((OwnedNavigationBuilder)referenceOwnershipBuilder).ToTable(
            name, schema);

    /// <summary>
    ///     Configures the table that the entity type maps to when targeting a relational database.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
    /// </remarks>
    /// <param name="referenceOwnershipBuilder">The builder for the entity type being configured.</param>
    /// <param name="name">The name of the table.</param>
    /// <param name="schema">The schema of the table.</param>
    /// <param name="buildAction">An action that performs configuration of the table.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static OwnedNavigationBuilder<TOwnerEntity, TRelatedEntity> ToTable<TOwnerEntity, TRelatedEntity>(
        this OwnedNavigationBuilder<TOwnerEntity, TRelatedEntity> referenceOwnershipBuilder,
        string name,
        string? schema,
        Action<OwnedNavigationTableBuilder<TRelatedEntity>> buildAction)
        where TOwnerEntity : class
        where TRelatedEntity : class
    {
        Check.NotNull(name, nameof(name));
        Check.NullButNotEmpty(schema, nameof(schema));
        Check.NotNull(buildAction, nameof(buildAction));

        referenceOwnershipBuilder.OwnedEntityType.SetTableName(name);
        referenceOwnershipBuilder.OwnedEntityType.SetSchema(schema);
        buildAction(new OwnedNavigationTableBuilder<TRelatedEntity>(name, schema, referenceOwnershipBuilder));

        return referenceOwnershipBuilder;
    }

    /// <summary>
    ///     Configures the table that the entity type maps to when targeting a relational database.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
    /// </remarks>
    /// <param name="entityTypeBuilder">The builder for the entity type being configured.</param>
    /// <param name="name">The name of the table.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     The same builder instance if the configuration was applied, <see langword="null" /> otherwise.
    /// </returns>
    public static IConventionEntityTypeBuilder? ToTable(
        this IConventionEntityTypeBuilder entityTypeBuilder,
        string? name,
        bool fromDataAnnotation = false)
    {
        if (!entityTypeBuilder.CanSetTable(name, fromDataAnnotation))
        {
            return null;
        }

        entityTypeBuilder.Metadata.SetTableName(name, fromDataAnnotation);
        return entityTypeBuilder;
    }

    /// <summary>
    ///     Configures the table that the entity type maps to when targeting a relational database.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
    /// </remarks>
    /// <param name="entityTypeBuilder">The builder for the entity type being configured.</param>
    /// <param name="name">The name of the table.</param>
    /// <param name="schema">The schema of the table.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     The same builder instance if the configuration was applied, <see langword="null" /> otherwise.
    /// </returns>
    public static IConventionEntityTypeBuilder? ToTable(
        this IConventionEntityTypeBuilder entityTypeBuilder,
        string? name,
        string? schema,
        bool fromDataAnnotation = false)
    {
        if (!entityTypeBuilder.CanSetTable(name, fromDataAnnotation)
            || !entityTypeBuilder.CanSetSchema(schema, fromDataAnnotation))
        {
            return null;
        }

        entityTypeBuilder.Metadata.SetTableName(name, fromDataAnnotation);
        entityTypeBuilder.Metadata.SetSchema(schema, fromDataAnnotation);
        return entityTypeBuilder;
    }

    /// <summary>
    ///     Returns a value indicating whether the table name can be set for this entity type
    ///     using the specified configuration source.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
    /// </remarks>
    /// <param name="entityTypeBuilder">The builder for the entity type being configured.</param>
    /// <param name="name">The name of the table.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns><see langword="true" /> if the configuration can be applied.</returns>
    public static bool CanSetTable(
        this IConventionEntityTypeBuilder entityTypeBuilder,
        string? name,
        bool fromDataAnnotation = false)
    {
        Check.NullButNotEmpty(name, nameof(name));

        return entityTypeBuilder.CanSetAnnotation(RelationalAnnotationNames.TableName, name, fromDataAnnotation);
    }

    /// <summary>
    ///     Configures the schema of the table that the entity type maps to when targeting a relational database.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
    /// </remarks>
    /// <param name="entityTypeBuilder">The builder for the entity type being configured.</param>
    /// <param name="schema">The schema of the table.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     The same builder instance if the configuration was applied, <see langword="null" /> otherwise.
    /// </returns>
    public static IConventionEntityTypeBuilder? ToSchema(
        this IConventionEntityTypeBuilder entityTypeBuilder,
        string? schema,
        bool fromDataAnnotation = false)
    {
        if (!entityTypeBuilder.CanSetSchema(schema, fromDataAnnotation))
        {
            return null;
        }

        entityTypeBuilder.Metadata.SetSchema(schema, fromDataAnnotation);
        return entityTypeBuilder;
    }

    /// <summary>
    ///     Returns a value indicating whether the schema of the table name can be set for this entity type
    ///     using the specified configuration source.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
    /// </remarks>
    /// <param name="entityTypeBuilder">The builder for the entity type being configured.</param>
    /// <param name="schema">The schema of the table.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns><see langword="true" /> if the configuration can be applied.</returns>
    public static bool CanSetSchema(
        this IConventionEntityTypeBuilder entityTypeBuilder,
        string? schema,
        bool fromDataAnnotation = false)
    {
        Check.NullButNotEmpty(schema, nameof(schema));

        return entityTypeBuilder.CanSetAnnotation(RelationalAnnotationNames.Schema, schema, fromDataAnnotation);
    }

    /// <summary>
    ///     Mark the table that this entity type is mapped to as excluded from migrations.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-migrations">Database migrations</see> for more information and examples.
    /// </remarks>
    /// <param name="entityTypeBuilder">The builder for the entity type being configured.</param>
    /// <param name="excludedFromMigrations">A value indicating whether the table should be managed by migrations.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     The same builder instance if the configuration was applied,
    ///     <see langword="null" /> otherwise.
    /// </returns>
    public static IConventionEntityTypeBuilder? ExcludeTableFromMigrations(
        this IConventionEntityTypeBuilder entityTypeBuilder,
        bool? excludedFromMigrations,
        bool fromDataAnnotation = false)
    {
        if (!entityTypeBuilder.CanExcludeTableFromMigrations(excludedFromMigrations, fromDataAnnotation))
        {
            return null;
        }

        entityTypeBuilder.Metadata.SetIsTableExcludedFromMigrations(excludedFromMigrations, fromDataAnnotation);
        return entityTypeBuilder;
    }

    /// <summary>
    ///     Returns a value indicating whether the table that this entity type is mapped to can be excluded from migrations
    ///     using the specified configuration source.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-migrations">Database migrations</see> for more information and examples.
    /// </remarks>
    /// <param name="entityTypeBuilder">The builder for the entity type being configured.</param>
    /// <param name="excludedFromMigrations">A value indicating whether the table should be managed by migrations.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns><see langword="true" /> if the configuration can be applied.</returns>
    public static bool CanExcludeTableFromMigrations(
        this IConventionEntityTypeBuilder entityTypeBuilder,
        bool? excludedFromMigrations,
        bool fromDataAnnotation = false)
        => entityTypeBuilder.CanSetAnnotation
            (RelationalAnnotationNames.IsTableExcludedFromMigrations, excludedFromMigrations, fromDataAnnotation);

    /// <summary>
    ///     Configures the view that the entity type maps to when targeting a relational database.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
    /// </remarks>
    /// <param name="entityTypeBuilder">The builder for the entity type being configured.</param>
    /// <param name="name">The name of the view.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static EntityTypeBuilder ToView(
        this EntityTypeBuilder entityTypeBuilder,
        string? name)
        => entityTypeBuilder.ToView(name, null);

    /// <summary>
    ///     Configures the view that the entity type maps to when targeting a relational database.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
    /// </remarks>
    /// <typeparam name="TEntity">The entity type being configured.</typeparam>
    /// <param name="referenceOwnershipBuilder">The builder for the entity type being configured.</param>
    /// <param name="name">The name of the view.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static EntityTypeBuilder<TEntity> ToView<TEntity>(
        this EntityTypeBuilder<TEntity> referenceOwnershipBuilder,
        string? name)
        where TEntity : class
        => (EntityTypeBuilder<TEntity>)ToView((EntityTypeBuilder)referenceOwnershipBuilder, name);

    /// <summary>
    ///     Configures the view that the entity type maps to when targeting a relational database.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
    /// </remarks>
    /// <param name="entityTypeBuilder">The builder for the entity type being configured.</param>
    /// <param name="name">The name of the view.</param>
    /// <param name="schema">The schema of the view.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static EntityTypeBuilder ToView(
        this EntityTypeBuilder entityTypeBuilder,
        string? name,
        string? schema)
    {
        Check.NullButNotEmpty(name, nameof(name));
        Check.NullButNotEmpty(schema, nameof(schema));

        entityTypeBuilder.Metadata.SetViewName(name);
        entityTypeBuilder.Metadata.SetViewSchema(schema);
        entityTypeBuilder.Metadata.SetAnnotation(RelationalAnnotationNames.ViewDefinitionSql, null);

        return entityTypeBuilder;
    }

    /// <summary>
    ///     Configures the view that the entity type maps to when targeting a relational database.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
    /// </remarks>
    /// <typeparam name="TEntity">The entity type being configured.</typeparam>
    /// <param name="entityTypeBuilder">The builder for the entity type being configured.</param>
    /// <param name="name">The name of the view.</param>
    /// <param name="schema">The schema of the view.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static EntityTypeBuilder<TEntity> ToView<TEntity>(
        this EntityTypeBuilder<TEntity> entityTypeBuilder,
        string? name,
        string? schema)
        where TEntity : class
        => (EntityTypeBuilder<TEntity>)ToView((EntityTypeBuilder)entityTypeBuilder, name, schema);

    /// <summary>
    ///     Configures the view that the entity type maps to when targeting a relational database.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
    /// </remarks>
    /// <param name="referenceOwnershipBuilder">The builder for the entity type being configured.</param>
    /// <param name="name">The name of the view.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static OwnedNavigationBuilder ToView(
        this OwnedNavigationBuilder referenceOwnershipBuilder,
        string? name)
        => referenceOwnershipBuilder.ToView(name, null);

    /// <summary>
    ///     Configures the view that the entity type maps to when targeting a relational database.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
    /// </remarks>
    /// <param name="referenceOwnershipBuilder">The builder for the entity type being configured.</param>
    /// <param name="name">The name of the view.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static OwnedNavigationBuilder<TOwnerEntity, TRelatedEntity> ToView<TOwnerEntity, TRelatedEntity>(
        this OwnedNavigationBuilder<TOwnerEntity, TRelatedEntity> referenceOwnershipBuilder,
        string? name)
        where TOwnerEntity : class
        where TRelatedEntity : class
        => (OwnedNavigationBuilder<TOwnerEntity, TRelatedEntity>)ToView((OwnedNavigationBuilder)referenceOwnershipBuilder, name);

    /// <summary>
    ///     Configures the view that the entity type maps to when targeting a relational database.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
    /// </remarks>
    /// <param name="referenceOwnershipBuilder">The builder for the entity type being configured.</param>
    /// <param name="name">The name of the view.</param>
    /// <param name="schema">The schema of the view.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static OwnedNavigationBuilder ToView(
        this OwnedNavigationBuilder referenceOwnershipBuilder,
        string? name,
        string? schema)
    {
        Check.NullButNotEmpty(name, nameof(name));
        Check.NullButNotEmpty(schema, nameof(schema));

        referenceOwnershipBuilder.OwnedEntityType.SetViewName(name);
        referenceOwnershipBuilder.OwnedEntityType.SetViewSchema(schema);
        referenceOwnershipBuilder.OwnedEntityType.SetAnnotation(RelationalAnnotationNames.ViewDefinitionSql, null);

        return referenceOwnershipBuilder;
    }

    /// <summary>
    ///     Configures the view that the entity type maps to when targeting a relational database.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
    /// </remarks>
    /// <param name="referenceOwnershipBuilder">The builder for the entity type being configured.</param>
    /// <param name="name">The name of the view.</param>
    /// <param name="schema">The schema of the view.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static OwnedNavigationBuilder<TOwnerEntity, TRelatedEntity> ToView<TOwnerEntity, TRelatedEntity>(
        this OwnedNavigationBuilder<TOwnerEntity, TRelatedEntity> referenceOwnershipBuilder,
        string? name,
        string? schema)
        where TOwnerEntity : class
        where TRelatedEntity : class
        => (OwnedNavigationBuilder<TOwnerEntity, TRelatedEntity>)ToView(
            (OwnedNavigationBuilder)referenceOwnershipBuilder, name, schema);

    /// <summary>
    ///     Configures the view that the entity type maps to when targeting a relational database.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
    /// </remarks>
    /// <param name="entityTypeBuilder">The builder for the entity type being configured.</param>
    /// <param name="name">The name of the view.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     The same builder instance if the configuration was applied, <see langword="null" /> otherwise.
    /// </returns>
    public static IConventionEntityTypeBuilder? ToView(
        this IConventionEntityTypeBuilder entityTypeBuilder,
        string? name,
        bool fromDataAnnotation = false)
    {
        if (!entityTypeBuilder.CanSetView(name, fromDataAnnotation))
        {
            return null;
        }

        entityTypeBuilder.Metadata.SetViewName(name, fromDataAnnotation);
        return entityTypeBuilder;
    }

    /// <summary>
    ///     Configures the view that the entity type maps to when targeting a relational database.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
    /// </remarks>
    /// <param name="entityTypeBuilder">The builder for the entity type being configured.</param>
    /// <param name="name">The name of the view.</param>
    /// <param name="schema">The schema of the view.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     The same builder instance if the configuration was applied, <see langword="null" /> otherwise.
    /// </returns>
    public static IConventionEntityTypeBuilder? ToView(
        this IConventionEntityTypeBuilder entityTypeBuilder,
        string? name,
        string? schema,
        bool fromDataAnnotation = false)
    {
        if (!entityTypeBuilder.CanSetView(name, fromDataAnnotation)
            || !entityTypeBuilder.CanSetViewSchema(schema, fromDataAnnotation))
        {
            return null;
        }

        entityTypeBuilder.Metadata.SetViewName(name, fromDataAnnotation);
        entityTypeBuilder.Metadata.SetViewSchema(schema, fromDataAnnotation);
        return entityTypeBuilder;
    }

    /// <summary>
    ///     Returns a value indicating whether the view name can be set for this entity type
    ///     using the specified configuration source.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
    /// </remarks>
    /// <param name="entityTypeBuilder">The builder for the entity type being configured.</param>
    /// <param name="name">The name of the view.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns><see langword="true" /> if the configuration can be applied.</returns>
    public static bool CanSetView(
        this IConventionEntityTypeBuilder entityTypeBuilder,
        string? name,
        bool fromDataAnnotation = false)
    {
        Check.NullButNotEmpty(name, nameof(name));

        return entityTypeBuilder.CanSetAnnotation(RelationalAnnotationNames.ViewName, name, fromDataAnnotation);
    }

    /// <summary>
    ///     Configures the schema of the view that the entity type maps to when targeting a relational database.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
    /// </remarks>
    /// <param name="entityTypeBuilder">The builder for the entity type being configured.</param>
    /// <param name="schema">The schema of the view.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     The same builder instance if the configuration was applied, <see langword="null" /> otherwise.
    /// </returns>
    public static IConventionEntityTypeBuilder? ToViewSchema(
        this IConventionEntityTypeBuilder entityTypeBuilder,
        string? schema,
        bool fromDataAnnotation = false)
    {
        if (!entityTypeBuilder.CanSetSchema(schema, fromDataAnnotation))
        {
            return null;
        }

        entityTypeBuilder.Metadata.SetViewSchema(schema, fromDataAnnotation);
        return entityTypeBuilder;
    }

    /// <summary>
    ///     Returns a value indicating whether the schema of the view can be set for this entity type
    ///     using the specified configuration source.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
    /// </remarks>
    /// <param name="entityTypeBuilder">The builder for the entity type being configured.</param>
    /// <param name="schema">The schema of the view.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns><see langword="true" /> if the configuration can be applied.</returns>
    public static bool CanSetViewSchema(
        this IConventionEntityTypeBuilder entityTypeBuilder,
        string? schema,
        bool fromDataAnnotation = false)
    {
        Check.NullButNotEmpty(schema, nameof(schema));

        return entityTypeBuilder.CanSetAnnotation(RelationalAnnotationNames.ViewSchema, schema, fromDataAnnotation);
    }

    /// <summary>
    ///     Configures a SQL string used to provide data for the entity type.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
    /// </remarks>
    /// <param name="entityTypeBuilder">The builder for the entity type being configured.</param>
    /// <param name="query">The SQL query that will provide the underlying data for the entity type.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static EntityTypeBuilder ToSqlQuery(
        this EntityTypeBuilder entityTypeBuilder,
        string query)
    {
        Check.NotNull(query, nameof(query));

        entityTypeBuilder.Metadata.SetSqlQuery(query);

        return entityTypeBuilder;
    }

    /// <summary>
    ///     Configures a SQL string used to provide data for the entity type.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
    /// </remarks>
    /// <param name="entityTypeBuilder">The builder for the entity type being configured.</param>
    /// <param name="query">The SQL query that will provide the underlying data for the entity type.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static EntityTypeBuilder<TEntity> ToSqlQuery<TEntity>(
        this EntityTypeBuilder<TEntity> entityTypeBuilder,
        string query)
        where TEntity : class
        => (EntityTypeBuilder<TEntity>)ToSqlQuery((EntityTypeBuilder)entityTypeBuilder, query);

    /// <summary>
    ///     Configures a SQL string used to provide data for the entity type.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
    /// </remarks>
    /// <param name="entityTypeBuilder">The builder for the entity type being configured.</param>
    /// <param name="name">The SQL query that will provide the underlying data for the entity type.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     The same builder instance if the configuration was applied, <see langword="null" /> otherwise.
    /// </returns>
    public static IConventionEntityTypeBuilder? ToSqlQuery(
        this IConventionEntityTypeBuilder entityTypeBuilder,
        string? name,
        bool fromDataAnnotation = false)
    {
        if (!entityTypeBuilder.CanSetSqlQuery(name, fromDataAnnotation))
        {
            return null;
        }

        var entityType = entityTypeBuilder.Metadata;
        entityType.SetSqlQuery(name, fromDataAnnotation);

        return entityTypeBuilder;
    }

    /// <summary>
    ///     Returns a value indicating whether the query SQL string can be set for this entity type
    ///     using the specified configuration source.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
    /// </remarks>
    /// <param name="entityTypeBuilder">The builder for the entity type being configured.</param>
    /// <param name="name">The SQL query that will provide the underlying data for the entity type.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns><see langword="true" /> if the configuration can be applied.</returns>
    public static bool CanSetSqlQuery(
        this IConventionEntityTypeBuilder entityTypeBuilder,
        string? name,
        bool fromDataAnnotation = false)
    {
        Check.NullButNotEmpty(name, nameof(name));

        return entityTypeBuilder.CanSetAnnotation(RelationalAnnotationNames.SqlQuery, name, fromDataAnnotation);
    }

    /// <summary>
    ///     Configures the function that the entity type maps to when targeting a relational database.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
    /// </remarks>
    /// <param name="entityTypeBuilder">The builder for the entity type being configured.</param>
    /// <param name="name">The name of the function.</param>
    /// <returns>The function configuration builder.</returns>
    public static EntityTypeBuilder ToFunction(
        this EntityTypeBuilder entityTypeBuilder,
        string? name)
    {
        Check.NullButNotEmpty(name, nameof(name));

        ToFunction(name, entityTypeBuilder.Metadata);

        return entityTypeBuilder;
    }

    /// <summary>
    ///     Configures the function that the entity type maps to when targeting a relational database.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
    /// </remarks>
    /// <param name="entityTypeBuilder">The builder for the entity type being configured.</param>
    /// <param name="function">The method representing the function.</param>
    /// <returns>The function configuration builder.</returns>
    public static EntityTypeBuilder ToFunction(
        this EntityTypeBuilder entityTypeBuilder,
        MethodInfo? function)
    {
        ToFunction(function, entityTypeBuilder.Metadata);

        return entityTypeBuilder;
    }

    /// <summary>
    ///     Configures the function that the entity type maps to when targeting a relational database.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
    /// </remarks>
    /// <param name="entityTypeBuilder">The builder for the entity type being configured.</param>
    /// <param name="name">The name of the function.</param>
    /// <param name="configureFunction">The function configuration action.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static EntityTypeBuilder ToFunction(
        this EntityTypeBuilder entityTypeBuilder,
        string name,
        Action<TableValuedFunctionBuilder> configureFunction)
    {
        Check.NotNull(name, nameof(name));
        Check.NotNull(configureFunction, nameof(configureFunction));

        configureFunction(new TableValuedFunctionBuilder(ToFunction(name, entityTypeBuilder.Metadata)));

        return entityTypeBuilder;
    }

    /// <summary>
    ///     Configures the function that the entity type maps to when targeting a relational database.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
    /// </remarks>
    /// <param name="entityTypeBuilder">The builder for the entity type being configured.</param>
    /// <param name="function">The method representing the function.</param>
    /// <param name="configureFunction">The function configuration action.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static EntityTypeBuilder ToFunction(
        this EntityTypeBuilder entityTypeBuilder,
        MethodInfo function,
        Action<TableValuedFunctionBuilder> configureFunction)
    {
        Check.NotNull(function, nameof(function));
        Check.NotNull(configureFunction, nameof(configureFunction));

        configureFunction(new TableValuedFunctionBuilder(ToFunction(function, entityTypeBuilder.Metadata)));

        return entityTypeBuilder;
    }

    /// <summary>
    ///     Configures the function that the entity type maps to when targeting a relational database.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
    /// </remarks>
    /// <typeparam name="TEntity">The entity type being configured.</typeparam>
    /// <param name="entityTypeBuilder">The builder for the entity type being configured.</param>
    /// <param name="name">The name of the function.</param>
    /// <returns>The function configuration builder.</returns>
    public static EntityTypeBuilder<TEntity> ToFunction<TEntity>(
        this EntityTypeBuilder<TEntity> entityTypeBuilder,
        string? name)
        where TEntity : class
        => (EntityTypeBuilder<TEntity>)ToFunction((EntityTypeBuilder)entityTypeBuilder, name);

    /// <summary>
    ///     Configures the function that the entity type maps to when targeting a relational database.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
    /// </remarks>
    /// <typeparam name="TEntity">The entity type being configured.</typeparam>
    /// <param name="entityTypeBuilder">The builder for the entity type being configured.</param>
    /// <param name="function">The method representing the function.</param>
    /// <returns>The function configuration builder.</returns>
    public static EntityTypeBuilder<TEntity> ToFunction<TEntity>(
        this EntityTypeBuilder<TEntity> entityTypeBuilder,
        MethodInfo? function)
        where TEntity : class
        => (EntityTypeBuilder<TEntity>)ToFunction((EntityTypeBuilder)entityTypeBuilder, function);

    /// <summary>
    ///     Configures the function that the entity type maps to when targeting a relational database.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
    /// </remarks>
    /// <typeparam name="TEntity">The entity type being configured.</typeparam>
    /// <param name="entityTypeBuilder">The builder for the entity type being configured.</param>
    /// <param name="name">The name of the function.</param>
    /// <param name="configureFunction">The function configuration action.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static EntityTypeBuilder<TEntity> ToFunction<TEntity>(
        this EntityTypeBuilder<TEntity> entityTypeBuilder,
        string name,
        Action<TableValuedFunctionBuilder> configureFunction)
        where TEntity : class
        => (EntityTypeBuilder<TEntity>)ToFunction((EntityTypeBuilder)entityTypeBuilder, name, configureFunction);

    /// <summary>
    ///     Configures the function that the entity type maps to when targeting a relational database.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
    /// </remarks>
    /// <typeparam name="TEntity">The entity type being configured.</typeparam>
    /// <param name="entityTypeBuilder">The builder for the entity type being configured.</param>
    /// <param name="function">The method representing the function.</param>
    /// <param name="configureFunction">The function configuration action.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static EntityTypeBuilder<TEntity> ToFunction<TEntity>(
        this EntityTypeBuilder<TEntity> entityTypeBuilder,
        MethodInfo function,
        Action<TableValuedFunctionBuilder> configureFunction)
        where TEntity : class
        => (EntityTypeBuilder<TEntity>)ToFunction((EntityTypeBuilder)entityTypeBuilder, function, configureFunction);

    /// <summary>
    ///     Configures the function that the entity type maps to when targeting a relational database.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
    /// </remarks>
    /// <param name="ownedNavigationBuilder">The builder for the entity type being configured.</param>
    /// <param name="name">The name of the function.</param>
    /// <returns>The function configuration builder.</returns>
    public static OwnedNavigationBuilder ToFunction(
        this OwnedNavigationBuilder ownedNavigationBuilder,
        string? name)
    {
        Check.NullButNotEmpty(name, nameof(name));

        ToFunction(name, ownedNavigationBuilder.OwnedEntityType);

        return ownedNavigationBuilder;
    }

    /// <summary>
    ///     Configures the function that the entity type maps to when targeting a relational database.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
    /// </remarks>
    /// <param name="ownedNavigationBuilder">The builder for the entity type being configured.</param>
    /// <param name="function">The method representing the function.</param>
    /// <returns>The function configuration builder.</returns>
    public static OwnedNavigationBuilder ToFunction(
        this OwnedNavigationBuilder ownedNavigationBuilder,
        MethodInfo? function)
    {
        ToFunction(function, ownedNavigationBuilder.OwnedEntityType);

        return ownedNavigationBuilder;
    }

    /// <summary>
    ///     Configures the function that the entity type maps to when targeting a relational database.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
    /// </remarks>
    /// <param name="ownedNavigationBuilder">The builder for the entity type being configured.</param>
    /// <param name="name">The name of the function.</param>
    /// <param name="configureFunction">The function configuration action.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static OwnedNavigationBuilder ToFunction(
        this OwnedNavigationBuilder ownedNavigationBuilder,
        string name,
        Action<TableValuedFunctionBuilder> configureFunction)
    {
        Check.NullButNotEmpty(name, nameof(name));
        Check.NotNull(configureFunction, nameof(configureFunction));

        configureFunction(new TableValuedFunctionBuilder(ToFunction(name, ownedNavigationBuilder.OwnedEntityType)));

        return ownedNavigationBuilder;
    }

    /// <summary>
    ///     Configures the function that the entity type maps to when targeting a relational database.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
    /// </remarks>
    /// <param name="ownedNavigationBuilder">The builder for the entity type being configured.</param>
    /// <param name="function">The method representing the function.</param>
    /// <param name="configureFunction">The function configuration action.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static OwnedNavigationBuilder ToFunction(
        this OwnedNavigationBuilder ownedNavigationBuilder,
        MethodInfo function,
        Action<TableValuedFunctionBuilder> configureFunction)
    {
        Check.NotNull(function, nameof(function));
        Check.NotNull(configureFunction, nameof(configureFunction));

        configureFunction(new TableValuedFunctionBuilder(ToFunction(function, ownedNavigationBuilder.OwnedEntityType)));

        return ownedNavigationBuilder;
    }

    /// <summary>
    ///     Configures the function that the entity type maps to when targeting a relational database.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
    /// </remarks>
    /// <param name="referenceOwnershipBuilder">The builder for the entity type being configured.</param>
    /// <param name="name">The name of the function.</param>
    /// <returns>The function configuration builder.</returns>
    public static OwnedNavigationBuilder<TOwnerEntity, TRelatedEntity> ToFunction<TOwnerEntity, TRelatedEntity>(
        this OwnedNavigationBuilder<TOwnerEntity, TRelatedEntity> referenceOwnershipBuilder,
        string? name)
        where TOwnerEntity : class
        where TRelatedEntity : class
        => (OwnedNavigationBuilder<TOwnerEntity, TRelatedEntity>)ToFunction((OwnedNavigationBuilder)referenceOwnershipBuilder, name);

    /// <summary>
    ///     Configures the function that the entity type maps to when targeting a relational database.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
    /// </remarks>
    /// <param name="referenceOwnershipBuilder">The builder for the entity type being configured.</param>
    /// <param name="function">The method representing the function.</param>
    /// <returns>The function configuration builder.</returns>
    public static OwnedNavigationBuilder<TOwnerEntity, TRelatedEntity> ToFunction<TOwnerEntity, TRelatedEntity>(
        this OwnedNavigationBuilder<TOwnerEntity, TRelatedEntity> referenceOwnershipBuilder,
        MethodInfo? function)
        where TOwnerEntity : class
        where TRelatedEntity : class
        => (OwnedNavigationBuilder<TOwnerEntity, TRelatedEntity>)ToFunction(
            (OwnedNavigationBuilder)referenceOwnershipBuilder, function);

    /// <summary>
    ///     Configures the function that the entity type maps to when targeting a relational database.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
    /// </remarks>
    /// <param name="referenceOwnershipBuilder">The builder for the entity type being configured.</param>
    /// <param name="name">The name of the function.</param>
    /// <param name="configureFunction">The function configuration action.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static OwnedNavigationBuilder<TOwnerEntity, TRelatedEntity> ToFunction<TOwnerEntity, TRelatedEntity>(
        this OwnedNavigationBuilder<TOwnerEntity, TRelatedEntity> referenceOwnershipBuilder,
        string name,
        Action<TableValuedFunctionBuilder> configureFunction)
        where TOwnerEntity : class
        where TRelatedEntity : class
        => (OwnedNavigationBuilder<TOwnerEntity, TRelatedEntity>)ToFunction(
            (OwnedNavigationBuilder)referenceOwnershipBuilder, name, configureFunction);

    /// <summary>
    ///     Configures the function that the entity type maps to when targeting a relational database.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
    /// </remarks>
    /// <param name="referenceOwnershipBuilder">The builder for the entity type being configured.</param>
    /// <param name="function">The method representing the function.</param>
    /// <param name="configureFunction">The function configuration action.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static OwnedNavigationBuilder<TOwnerEntity, TRelatedEntity> ToFunction<TOwnerEntity, TRelatedEntity>(
        this OwnedNavigationBuilder<TOwnerEntity, TRelatedEntity> referenceOwnershipBuilder,
        MethodInfo function,
        Action<TableValuedFunctionBuilder> configureFunction)
        where TOwnerEntity : class
        where TRelatedEntity : class
        => (OwnedNavigationBuilder<TOwnerEntity, TRelatedEntity>)ToFunction(
            (OwnedNavigationBuilder)referenceOwnershipBuilder, function, configureFunction);

    [return: NotNullIfNotNull("name")]
    private static IMutableDbFunction? ToFunction(string? name, IMutableEntityType entityType)
    {
        entityType.SetFunctionName(name);

        if (name is null)
        {
            return null;
        }

        var model = entityType.Model;
        var function = model.FindDbFunction(name);
        if (function != null)
        {
            ((DbFunction)function).UpdateConfigurationSource(ConfigurationSource.Explicit);
        }
        else
        {
            function = model.AddDbFunction(name, typeof(IQueryable<>).MakeGenericType(entityType.ClrType));
        }

        return function;
    }

    [return: NotNullIfNotNull("method")]
    private static IMutableDbFunction? ToFunction(MethodInfo? method, IMutableEntityType entityType)
    {
        var name = method == null ? null : DbFunction.GetFunctionName(method);
        entityType.SetFunctionName(name);

        if (name is null)
        {
            return null;
        }

        var model = entityType.Model;
        var function = model.FindDbFunction(name);
        if (function != null)
        {
            ((DbFunction)function).UpdateConfigurationSource(ConfigurationSource.Explicit);
        }
        else
        {
            function = model.AddDbFunction(method!);
        }

        return function;
    }

    /// <summary>
    ///     Configures the function that the entity type maps to when targeting a relational database.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
    /// </remarks>
    /// <param name="entityTypeBuilder">The builder for the entity type being configured.</param>
    /// <param name="name">The name of the function.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     The same builder instance if the configuration was applied,
    ///     <see langword="null" /> otherwise.
    /// </returns>
    public static IConventionEntityTypeBuilder? ToFunction(
        this IConventionEntityTypeBuilder entityTypeBuilder,
        string? name,
        bool fromDataAnnotation = false)
    {
        if (!entityTypeBuilder.CanSetFunction(name, fromDataAnnotation))
        {
            return null;
        }

        var entityType = entityTypeBuilder.Metadata;
        entityType.SetFunctionName(name, fromDataAnnotation);

        if (name is not null)
        {
            entityType.Model.Builder.HasDbFunction(name, typeof(IQueryable<>).MakeGenericType(entityType.ClrType), fromDataAnnotation);
        }

        return entityTypeBuilder;
    }

    /// <summary>
    ///     Configures the function that the entity type maps to when targeting a relational database.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
    /// </remarks>
    /// <param name="entityTypeBuilder">The builder for the entity type being configured.</param>
    /// <param name="function">The method representing the function.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     The same builder instance if the configuration was applied,
    ///     <see langword="null" /> otherwise.
    /// </returns>
    public static IConventionEntityTypeBuilder? ToFunction(
        this IConventionEntityTypeBuilder entityTypeBuilder,
        MethodInfo? function,
        bool fromDataAnnotation = false)
    {
        var name = function == null ? null : DbFunction.GetFunctionName(function);
        if (!entityTypeBuilder.CanSetFunction(name, fromDataAnnotation))
        {
            return null;
        }

        var entityType = entityTypeBuilder.Metadata;
        entityType.SetFunctionName(name, fromDataAnnotation);

        if (function is not null)
        {
            entityType.Model.Builder.HasDbFunction(function, fromDataAnnotation);
        }

        return entityTypeBuilder;
    }

    /// <summary>
    ///     Returns a value indicating whether the function name can be set for this entity type
    ///     using the specified configuration source.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
    /// </remarks>
    /// <param name="entityTypeBuilder">The builder for the entity type being configured.</param>
    /// <param name="name">The name of the function.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns><see langword="true" /> if the configuration can be applied.</returns>
    public static bool CanSetFunction(
        this IConventionEntityTypeBuilder entityTypeBuilder,
        string? name,
        bool fromDataAnnotation = false)
    {
        Check.NullButNotEmpty(name, nameof(name));

        return entityTypeBuilder.CanSetAnnotation(RelationalAnnotationNames.FunctionName, name, fromDataAnnotation);
    }

    /// <summary>
    ///     Returns a value indicating whether the function name can be set for this entity type
    ///     using the specified configuration source.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
    /// </remarks>
    /// <param name="entityTypeBuilder">The builder for the entity type being configured.</param>
    /// <param name="function">The method representing the function.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns><see langword="true" /> if the configuration can be applied.</returns>
    public static bool CanSetFunction(
        this IConventionEntityTypeBuilder entityTypeBuilder,
        MethodInfo? function,
        bool fromDataAnnotation = false)
        => entityTypeBuilder.CanSetFunction(function == null ? null : DbFunction.GetFunctionName(function), fromDataAnnotation);

    /// <summary>
    ///     Configures a database check constraint when targeting a relational database.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-check-constraints">Database check constraints</see> for more information and examples.
    /// </remarks>
    /// <param name="entityTypeBuilder">The entity type builder.</param>
    /// <param name="name">The name of the check constraint.</param>
    /// <param name="sql">The logical constraint sql used in the check constraint.</param>
    /// <returns>A builder to further configure the entity type.</returns>
    public static EntityTypeBuilder HasCheckConstraint(
        this EntityTypeBuilder entityTypeBuilder,
        string name,
        string? sql)
    {
        InternalCheckConstraintBuilder.HasCheckConstraint(
            (IConventionEntityType)entityTypeBuilder.Metadata,
            name,
            sql,
            ConfigurationSource.Explicit);

        return entityTypeBuilder;
    }

    /// <summary>
    ///     Configures a database check constraint when targeting a relational database.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-check-constraints">Database check constraints</see> for more information and examples.
    /// </remarks>
    /// <param name="entityTypeBuilder">The entity type builder.</param>
    /// <param name="name">The name of the check constraint.</param>
    /// <param name="sql">The logical constraint sql used in the check constraint.</param>
    /// <param name="buildAction">An action that performs configuration of the check constraint.</param>
    /// <returns>A builder to further configure the entity type.</returns>
    public static EntityTypeBuilder HasCheckConstraint(
        this EntityTypeBuilder entityTypeBuilder,
        string name,
        string sql,
        Action<CheckConstraintBuilder> buildAction)
    {
        Check.NotEmpty(sql, nameof(sql));
        Check.NotNull(buildAction, nameof(buildAction));

        entityTypeBuilder.HasCheckConstraint(name, sql);

        buildAction(new CheckConstraintBuilder(entityTypeBuilder.Metadata.FindCheckConstraint(name)!));

        return entityTypeBuilder;
    }

    /// <summary>
    ///     Configures a database check constraint when targeting a relational database.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-check-constraints">Database check constraints</see> for more information and examples.
    /// </remarks>
    /// <typeparam name="TEntity">The entity type being configured.</typeparam>
    /// <param name="entityTypeBuilder">The entity type builder.</param>
    /// <param name="name">The name of the check constraint.</param>
    /// <param name="sql">The logical constraint sql used in the check constraint.</param>
    /// <returns>A builder to further configure the entity type.</returns>
    public static EntityTypeBuilder<TEntity> HasCheckConstraint<TEntity>(
        this EntityTypeBuilder<TEntity> entityTypeBuilder,
        string name,
        string? sql)
        where TEntity : class
        => (EntityTypeBuilder<TEntity>)HasCheckConstraint((EntityTypeBuilder)entityTypeBuilder, name, sql);

    /// <summary>
    ///     Configures a database check constraint when targeting a relational database.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-check-constraints">Database check constraints</see> for more information and examples.
    /// </remarks>
    /// <typeparam name="TEntity">The entity type being configured.</typeparam>
    /// <param name="entityTypeBuilder">The entity type builder.</param>
    /// <param name="name">The name of the check constraint.</param>
    /// <param name="sql">The logical constraint sql used in the check constraint.</param>
    /// <param name="buildAction">An action that performs configuration of the check constraint.</param>
    /// <returns>A builder to further configure the entity type.</returns>
    public static EntityTypeBuilder<TEntity> HasCheckConstraint<TEntity>(
        this EntityTypeBuilder<TEntity> entityTypeBuilder,
        string name,
        string sql,
        Action<CheckConstraintBuilder> buildAction)
        where TEntity : class
        => (EntityTypeBuilder<TEntity>)HasCheckConstraint((EntityTypeBuilder)entityTypeBuilder, name, sql, buildAction);

    /// <summary>
    ///     Configures a database check constraint when targeting a relational database.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-check-constraints">Database check constraints</see> for more information and examples.
    /// </remarks>
    /// <param name="ownedNavigationBuilder">The navigation builder for the owned type.</param>
    /// <param name="name">The name of the check constraint.</param>
    /// <param name="sql">The logical constraint sql used in the check constraint.</param>
    /// <returns>A builder to further configure the navigation.</returns>
    public static OwnedNavigationBuilder HasCheckConstraint(
        this OwnedNavigationBuilder ownedNavigationBuilder,
        string name,
        string? sql)
    {
        InternalCheckConstraintBuilder.HasCheckConstraint(
            (IConventionEntityType)ownedNavigationBuilder.OwnedEntityType,
            name,
            sql,
            ConfigurationSource.Explicit);

        return ownedNavigationBuilder;
    }

    /// <summary>
    ///     Configures a database check constraint when targeting a relational database.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-check-constraints">Database check constraints</see> for more information and examples.
    /// </remarks>
    /// <param name="ownedNavigationBuilder">The navigation builder for the owned type.</param>
    /// <param name="name">The name of the check constraint.</param>
    /// <param name="sql">The logical constraint sql used in the check constraint.</param>
    /// <returns>A builder to further configure the navigation.</returns>
    public static OwnedNavigationBuilder<TOwnerEntity, TDependentEntity> HasCheckConstraint<TOwnerEntity, TDependentEntity>(
        this OwnedNavigationBuilder<TOwnerEntity, TDependentEntity> ownedNavigationBuilder,
        string name,
        string? sql)
        where TOwnerEntity : class
        where TDependentEntity : class
        => (OwnedNavigationBuilder<TOwnerEntity, TDependentEntity>)
            HasCheckConstraint((OwnedNavigationBuilder)ownedNavigationBuilder, name, sql);

    /// <summary>
    ///     Configures a database check constraint when targeting a relational database.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-check-constraints">Database check constraints</see> for more information and examples.
    /// </remarks>
    /// <param name="ownedNavigationBuilder">The navigation builder for the owned type.</param>
    /// <param name="name">The name of the check constraint.</param>
    /// <param name="sql">The logical constraint sql used in the check constraint.</param>
    /// <param name="buildAction">An action that performs configuration of the check constraint.</param>
    /// <returns>A builder to further configure the navigation.</returns>
    public static OwnedNavigationBuilder HasCheckConstraint(
        this OwnedNavigationBuilder ownedNavigationBuilder,
        string name,
        string sql,
        Action<CheckConstraintBuilder> buildAction)
    {
        Check.NotEmpty(sql, nameof(sql));
        Check.NotNull(buildAction, nameof(buildAction));

        ownedNavigationBuilder.HasCheckConstraint(name, sql);

        buildAction(new CheckConstraintBuilder(ownedNavigationBuilder.OwnedEntityType.FindCheckConstraint(name)!));

        return ownedNavigationBuilder;
    }

    /// <summary>
    ///     Configures a database check constraint when targeting a relational database.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-check-constraints">Database check constraints</see> for more information and examples.
    /// </remarks>
    /// <typeparam name="TOwnerEntity">The entity type owning the relationship.</typeparam>
    /// <typeparam name="TDependentEntity">The dependent entity type of the relationship.</typeparam>
    /// <param name="ownedNavigationBuilder">The navigation builder for the owned type.</param>
    /// <param name="name">The name of the check constraint.</param>
    /// <param name="sql">The logical constraint sql used in the check constraint.</param>
    /// <param name="buildAction">An action that performs configuration of the check constraint.</param>
    /// <returns>A builder to further configure the navigation.</returns>
    public static OwnedNavigationBuilder<TOwnerEntity, TDependentEntity> HasCheckConstraint<TOwnerEntity, TDependentEntity>(
        this OwnedNavigationBuilder<TOwnerEntity, TDependentEntity> ownedNavigationBuilder,
        string name,
        string sql,
        Action<CheckConstraintBuilder> buildAction)
        where TOwnerEntity : class
        where TDependentEntity : class
        => (OwnedNavigationBuilder<TOwnerEntity, TDependentEntity>)
            HasCheckConstraint((OwnedNavigationBuilder)ownedNavigationBuilder, name, sql, buildAction);

    /// <summary>
    ///     Configures a database check constraint when targeting a relational database.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-check-constraints">Database check constraints</see> for more information and examples.
    /// </remarks>
    /// <param name="entityTypeBuilder">The entity type builder.</param>
    /// <param name="name">The name of the check constraint.</param>
    /// <param name="sql">The logical constraint sql used in the check constraint.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     The same builder instance if the check constraint was configured,
    ///     <see langword="null" /> otherwise.
    /// </returns>
    public static IConventionCheckConstraintBuilder? HasCheckConstraint(
        this IConventionEntityTypeBuilder entityTypeBuilder,
        string name,
        string? sql,
        bool fromDataAnnotation = false)
        => InternalCheckConstraintBuilder.HasCheckConstraint(
                entityTypeBuilder.Metadata,
                name,
                sql,
                fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention)
            ?.Builder;

    /// <summary>
    ///     Returns a value indicating whether the check constraint can be configured.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-check-constraints">Database check constraints</see> for more information and examples.
    /// </remarks>
    /// <param name="entityTypeBuilder">The builder for the entity type being configured.</param>
    /// <param name="name">The name of the check constraint.</param>
    /// <param name="sql">The logical constraint sql used in the check constraint.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns><see langword="true" /> if the configuration can be applied.</returns>
    public static bool CanHaveCheckConstraint(
        this IConventionEntityTypeBuilder entityTypeBuilder,
        string name,
        string? sql,
        bool fromDataAnnotation = false)
        => InternalCheckConstraintBuilder.CanHaveCheckConstraint(
            entityTypeBuilder.Metadata,
            name,
            sql,
            fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     Configures a comment to be applied to the table
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
    /// </remarks>
    /// <param name="entityTypeBuilder">The builder for the entity type being configured.</param>
    /// <param name="comment">The comment for the table.</param>
    /// <returns>A builder to further configure the entity type.</returns>
    public static EntityTypeBuilder HasComment(
        this EntityTypeBuilder entityTypeBuilder,
        string? comment)
    {
        entityTypeBuilder.Metadata.SetComment(comment);
        return entityTypeBuilder;
    }

    /// <summary>
    ///     Configures a comment to be applied to the table
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
    /// </remarks>
    /// <typeparam name="TEntity">The entity type being configured.</typeparam>
    /// <param name="entityTypeBuilder">The entity type builder.</param>
    /// <param name="comment">The comment for the table.</param>
    /// <returns>A builder to further configure the entity type.</returns>
    public static EntityTypeBuilder<TEntity> HasComment<TEntity>(
        this EntityTypeBuilder<TEntity> entityTypeBuilder,
        string? comment)
        where TEntity : class
        => (EntityTypeBuilder<TEntity>)HasComment((EntityTypeBuilder)entityTypeBuilder, comment);

    /// <summary>
    ///     Configures a comment to be applied to the table
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
    /// </remarks>
    /// <param name="entityTypeBuilder">The builder for the entity type being configured.</param>
    /// <param name="comment">The comment for the table.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     The same builder instance if the configuration was applied,
    ///     <see langword="null" /> otherwise.
    /// </returns>
    public static IConventionEntityTypeBuilder? HasComment(
        this IConventionEntityTypeBuilder entityTypeBuilder,
        string? comment,
        bool fromDataAnnotation = false)
    {
        if (!entityTypeBuilder.CanSetComment(comment, fromDataAnnotation))
        {
            return null;
        }

        entityTypeBuilder.Metadata.SetComment(comment, fromDataAnnotation);
        return entityTypeBuilder;
    }

    /// <summary>
    ///     Returns a value indicating whether a comment can be set for this entity type
    ///     using the specified configuration source.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
    /// </remarks>
    /// <param name="entityTypeBuilder">The builder for the entity type being configured.</param>
    /// <param name="comment">The comment for the table.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns><see langword="true" /> if the configuration can be applied.</returns>
    public static bool CanSetComment(
        this IConventionEntityTypeBuilder entityTypeBuilder,
        string? comment,
        bool fromDataAnnotation = false)
        => entityTypeBuilder.CanSetAnnotation(
            RelationalAnnotationNames.Comment,
            comment,
            fromDataAnnotation);
}
