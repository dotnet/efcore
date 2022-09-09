// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable once CheckNamespace

namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     Relational database specific extension methods for <see cref="EntityTypeBuilder" />.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
/// </remarks>
public static partial class RelationalEntityTypeBuilderExtensions
{
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

        buildAction(new TableBuilder(null, entityTypeBuilder));

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
        string name,
        Action<TableBuilder> buildAction)
    {
        Check.NotNull(name, nameof(name));
        Check.NotNull(buildAction, nameof(buildAction));

        entityTypeBuilder.Metadata.SetTableName(name);
        entityTypeBuilder.Metadata.SetSchema(null);
        buildAction(new TableBuilder(StoreObjectIdentifier.Table(name), entityTypeBuilder));

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

        var entityTypeConventionBuilder = entityTypeBuilder.GetInfrastructure();
        if (entityTypeConventionBuilder.Metadata[RelationalAnnotationNames.TableName] == null)
        {
            entityTypeConventionBuilder.ToTable(entityTypeBuilder.Metadata.GetDefaultTableName());
        }

        buildAction(new TableBuilder<TEntity>(null, entityTypeBuilder));

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
        string name,
        Action<TableBuilder<TEntity>> buildAction)
        where TEntity : class
        => ToTable(entityTypeBuilder, name, null, buildAction);

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
        buildAction(new TableBuilder(StoreObjectIdentifier.Table(name, entityTypeBuilder.Metadata.GetSchema()), entityTypeBuilder));

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
        buildAction(
            new TableBuilder<TEntity>(StoreObjectIdentifier.Table(name, entityTypeBuilder.Metadata.GetSchema()), entityTypeBuilder));

        return entityTypeBuilder;
    }

    /// <summary>
    ///     Configures the table that the entity type maps to when targeting a relational database.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
    /// </remarks>
    /// <param name="ownedNavigationBuilder">The builder for the entity type being configured.</param>
    /// <param name="name">The name of the table.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static OwnedNavigationBuilder ToTable(
        this OwnedNavigationBuilder ownedNavigationBuilder,
        string? name)
    {
        Check.NullButNotEmpty(name, nameof(name));

        ownedNavigationBuilder.OwnedEntityType.SetTableName(name);
        ownedNavigationBuilder.OwnedEntityType.SetSchema(null);

        return ownedNavigationBuilder;
    }

    /// <summary>
    ///     Configures the table that the entity type maps to when targeting a relational database.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
    /// </remarks>
    /// <param name="ownedNavigationBuilder">The builder for the entity type being configured.</param>
    /// <param name="buildAction">An action that performs configuration of the table.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static OwnedNavigationBuilder ToTable(
        this OwnedNavigationBuilder ownedNavigationBuilder,
        Action<OwnedNavigationTableBuilder> buildAction)
    {
        Check.NotNull(buildAction, nameof(buildAction));

        buildAction(new OwnedNavigationTableBuilder(null, ownedNavigationBuilder));

        return ownedNavigationBuilder;
    }

    /// <summary>
    ///     Configures the table that the entity type maps to when targeting a relational database.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
    /// </remarks>
    /// <typeparam name="TOwnerEntity">The entity type owning the relationship.</typeparam>
    /// <typeparam name="TDependentEntity">The dependent entity type of the relationship.</typeparam>
    /// <param name="ownedNavigationBuilder">The builder for the entity type being configured.</param>
    /// <param name="buildAction">An action that performs configuration of the table.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static OwnedNavigationBuilder<TOwnerEntity, TDependentEntity> ToTable<TOwnerEntity, TDependentEntity>(
        this OwnedNavigationBuilder<TOwnerEntity, TDependentEntity> ownedNavigationBuilder,
        Action<OwnedNavigationTableBuilder<TOwnerEntity, TDependentEntity>> buildAction)
        where TOwnerEntity : class
        where TDependentEntity : class
    {
        Check.NotNull(buildAction, nameof(buildAction));

        buildAction(new OwnedNavigationTableBuilder<TOwnerEntity, TDependentEntity>(null, ownedNavigationBuilder));

        return ownedNavigationBuilder;
    }

    /// <summary>
    ///     Configures the table that the entity type maps to when targeting a relational database.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
    /// </remarks>
    /// <typeparam name="TOwnerEntity">The entity type owning the relationship.</typeparam>
    /// <typeparam name="TDependentEntity">The dependent entity type of the relationship.</typeparam>
    /// <param name="ownedNavigationBuilder">The builder for the entity type being configured.</param>
    /// <param name="name">The name of the table.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static OwnedNavigationBuilder<TOwnerEntity, TDependentEntity> ToTable<TOwnerEntity, TDependentEntity>(
        this OwnedNavigationBuilder<TOwnerEntity, TDependentEntity> ownedNavigationBuilder,
        string? name)
        where TOwnerEntity : class
        where TDependentEntity : class
        => (OwnedNavigationBuilder<TOwnerEntity, TDependentEntity>)((OwnedNavigationBuilder)ownedNavigationBuilder).ToTable(name);

    /// <summary>
    ///     Configures the table that the entity type maps to when targeting a relational database.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
    /// </remarks>
    /// <param name="ownedNavigationBuilder">The builder for the entity type being configured.</param>
    /// <param name="name">The name of the table.</param>
    /// <param name="buildAction">An action that performs configuration of the table.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static OwnedNavigationBuilder ToTable(
        this OwnedNavigationBuilder ownedNavigationBuilder,
        string name,
        Action<OwnedNavigationTableBuilder> buildAction)
    {
        Check.NotNull(name, nameof(name));
        Check.NotNull(buildAction, nameof(buildAction));

        ownedNavigationBuilder.OwnedEntityType.SetTableName(name);
        ownedNavigationBuilder.OwnedEntityType.SetSchema(null);
        buildAction(new OwnedNavigationTableBuilder(StoreObjectIdentifier.Table(name), ownedNavigationBuilder));

        return ownedNavigationBuilder;
    }

    /// <summary>
    ///     Configures the table that the entity type maps to when targeting a relational database.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
    /// </remarks>
    /// <typeparam name="TOwnerEntity">The entity type owning the relationship.</typeparam>
    /// <typeparam name="TDependentEntity">The dependent entity type of the relationship.</typeparam>
    /// <param name="ownedNavigationBuilder">The builder for the entity type being configured.</param>
    /// <param name="name">The name of the table.</param>
    /// <param name="buildAction">An action that performs configuration of the table.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static OwnedNavigationBuilder<TOwnerEntity, TDependentEntity> ToTable<TOwnerEntity, TDependentEntity>(
        this OwnedNavigationBuilder<TOwnerEntity, TDependentEntity> ownedNavigationBuilder,
        string name,
        Action<OwnedNavigationTableBuilder<TOwnerEntity, TDependentEntity>> buildAction)
        where TOwnerEntity : class
        where TDependentEntity : class
        => ToTable(ownedNavigationBuilder, name, null, buildAction);

    /// <summary>
    ///     Configures the table that the entity type maps to when targeting a relational database.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
    /// </remarks>
    /// <param name="ownedNavigationBuilder">The builder for the entity type being configured.</param>
    /// <param name="name">The name of the table.</param>
    /// <param name="schema">The schema of the table.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static OwnedNavigationBuilder ToTable(
        this OwnedNavigationBuilder ownedNavigationBuilder,
        string name,
        string? schema)
    {
        Check.NotNull(name, nameof(name));
        Check.NullButNotEmpty(schema, nameof(schema));

        ownedNavigationBuilder.OwnedEntityType.SetTableName(name);
        ownedNavigationBuilder.OwnedEntityType.SetSchema(schema);

        return ownedNavigationBuilder;
    }

    /// <summary>
    ///     Configures the table that the entity type maps to when targeting a relational database.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
    /// </remarks>
    /// <param name="ownedNavigationBuilder">The builder for the entity type being configured.</param>
    /// <param name="name">The name of the table.</param>
    /// <param name="schema">The schema of the table.</param>
    /// <param name="buildAction">An action that performs configuration of the table.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static OwnedNavigationBuilder ToTable(
        this OwnedNavigationBuilder ownedNavigationBuilder,
        string name,
        string? schema,
        Action<OwnedNavigationTableBuilder> buildAction)
    {
        Check.NotNull(name, nameof(name));
        Check.NullButNotEmpty(schema, nameof(schema));
        Check.NotNull(buildAction, nameof(buildAction));

        ownedNavigationBuilder.OwnedEntityType.SetTableName(name);
        ownedNavigationBuilder.OwnedEntityType.SetSchema(schema);
        buildAction(
            new OwnedNavigationTableBuilder(
                StoreObjectIdentifier.Table(name, ownedNavigationBuilder.OwnedEntityType.GetSchema()), ownedNavigationBuilder));

        return ownedNavigationBuilder;
    }

    /// <summary>
    ///     Configures the table that the entity type maps to when targeting a relational database.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
    /// </remarks>
    /// <typeparam name="TOwnerEntity">The entity type owning the relationship.</typeparam>
    /// <typeparam name="TDependentEntity">The dependent entity type of the relationship.</typeparam>
    /// <param name="ownedNavigationBuilder">The builder for the entity type being configured.</param>
    /// <param name="name">The name of the table.</param>
    /// <param name="schema">The schema of the table.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static OwnedNavigationBuilder<TOwnerEntity, TDependentEntity> ToTable<TOwnerEntity, TDependentEntity>(
        this OwnedNavigationBuilder<TOwnerEntity, TDependentEntity> ownedNavigationBuilder,
        string name,
        string? schema)
        where TOwnerEntity : class
        where TDependentEntity : class
        => (OwnedNavigationBuilder<TOwnerEntity, TDependentEntity>)((OwnedNavigationBuilder)ownedNavigationBuilder).ToTable(
            name, schema);

    /// <summary>
    ///     Configures the table that the entity type maps to when targeting a relational database.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
    /// </remarks>
    /// <typeparam name="TOwnerEntity">The entity type owning the relationship.</typeparam>
    /// <typeparam name="TDependentEntity">The dependent entity type of the relationship.</typeparam>
    /// <param name="ownedNavigationBuilder">The builder for the entity type being configured.</param>
    /// <param name="name">The name of the table.</param>
    /// <param name="schema">The schema of the table.</param>
    /// <param name="buildAction">An action that performs configuration of the table.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static OwnedNavigationBuilder<TOwnerEntity, TDependentEntity> ToTable<TOwnerEntity, TDependentEntity>(
        this OwnedNavigationBuilder<TOwnerEntity, TDependentEntity> ownedNavigationBuilder,
        string name,
        string? schema,
        Action<OwnedNavigationTableBuilder<TOwnerEntity, TDependentEntity>> buildAction)
        where TOwnerEntity : class
        where TDependentEntity : class
    {
        Check.NotNull(name, nameof(name));
        Check.NullButNotEmpty(schema, nameof(schema));
        Check.NotNull(buildAction, nameof(buildAction));

        ownedNavigationBuilder.OwnedEntityType.SetTableName(name);
        ownedNavigationBuilder.OwnedEntityType.SetSchema(schema);
        buildAction(
            new OwnedNavigationTableBuilder<TOwnerEntity, TDependentEntity>(
                StoreObjectIdentifier.Table(name, ownedNavigationBuilder.OwnedEntityType.GetSchema()), ownedNavigationBuilder));

        return ownedNavigationBuilder;
    }

    /// <summary>
    ///     Configures some of the properties on this entity type to be mapped to a different table.
    ///     The primary key properties are mapped to all tables, other properties must be explicitly mapped.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
    /// </remarks>
    /// <param name="entityTypeBuilder">The builder for the entity type being configured.</param>
    /// <param name="name">The name of the table.</param>
    /// <param name="buildAction">An action that performs configuration of the table.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static EntityTypeBuilder SplitToTable(
        this EntityTypeBuilder entityTypeBuilder,
        string name,
        Action<SplitTableBuilder> buildAction)
        => entityTypeBuilder.SplitToTable(name, null, buildAction);

    /// <summary>
    ///     Configures some of the properties on this entity type to be mapped to a different table.
    ///     The primary key properties are mapped to all tables, other properties must be explicitly mapped.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
    /// </remarks>
    /// <typeparam name="TEntity">The entity type being configured.</typeparam>
    /// <param name="entityTypeBuilder">The builder for the entity type being configured.</param>
    /// <param name="name">The name of the table.</param>
    /// <param name="buildAction">An action that performs configuration of the table.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static EntityTypeBuilder<TEntity> SplitToTable<TEntity>(
        this EntityTypeBuilder<TEntity> entityTypeBuilder,
        string name,
        Action<SplitTableBuilder<TEntity>> buildAction)
        where TEntity : class
        => entityTypeBuilder.SplitToTable(name, null, buildAction);

    /// <summary>
    ///     Configures some of the properties on this entity type to be mapped to a different table.
    ///     The primary key properties are mapped to all tables, other properties must be explicitly mapped.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
    /// </remarks>
    /// <param name="entityTypeBuilder">The builder for the entity type being configured.</param>
    /// <param name="name">The name of the table.</param>
    /// <param name="schema">The schema of the table.</param>
    /// <param name="buildAction">An action that performs configuration of the table.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static EntityTypeBuilder SplitToTable(
        this EntityTypeBuilder entityTypeBuilder,
        string name,
        string? schema,
        Action<SplitTableBuilder> buildAction)
    {
        Check.NotNull(name, nameof(name));
        Check.NullButNotEmpty(schema, nameof(schema));
        Check.NotNull(buildAction, nameof(buildAction));

        buildAction(
            new SplitTableBuilder(
                StoreObjectIdentifier.Table(name, schema ?? entityTypeBuilder.Metadata.GetDefaultSchema()), entityTypeBuilder));

        return entityTypeBuilder;
    }

    /// <summary>
    ///     Configures some of the properties on this entity type to be mapped to a different table.
    ///     The primary key properties are mapped to all tables, other properties must be explicitly mapped.
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
    public static EntityTypeBuilder<TEntity> SplitToTable<TEntity>(
        this EntityTypeBuilder<TEntity> entityTypeBuilder,
        string name,
        string? schema,
        Action<SplitTableBuilder<TEntity>> buildAction)
        where TEntity : class
    {
        Check.NotNull(name, nameof(name));
        Check.NullButNotEmpty(schema, nameof(schema));
        Check.NotNull(buildAction, nameof(buildAction));

        buildAction(
            new SplitTableBuilder<TEntity>(
                StoreObjectIdentifier.Table(name, schema ?? entityTypeBuilder.Metadata.GetDefaultSchema()), entityTypeBuilder));

        return entityTypeBuilder;
    }

    /// <summary>
    ///     Configures some of the properties on this entity type to be mapped to a different table.
    ///     The primary key properties are mapped to all tables, other properties must be explicitly mapped.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
    /// </remarks>
    /// <param name="ownedNavigationBuilder">The builder for the entity type being configured.</param>
    /// <param name="name">The name of the table.</param>
    /// <param name="buildAction">An action that performs configuration of the table.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static OwnedNavigationBuilder SplitToTable(
        this OwnedNavigationBuilder ownedNavigationBuilder,
        string name,
        Action<OwnedNavigationSplitTableBuilder> buildAction)
        => ownedNavigationBuilder.SplitToTable(name, null, buildAction);

    /// <summary>
    ///     Configures some of the properties on this entity type to be mapped to a different table.
    ///     The primary key properties are mapped to all tables, other properties must be explicitly mapped.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
    /// </remarks>
    /// <typeparam name="TOwnerEntity">The entity type owning the relationship.</typeparam>
    /// <typeparam name="TDependentEntity">The dependent entity type of the relationship.</typeparam>
    /// <param name="ownedNavigationBuilder">The builder for the entity type being configured.</param>
    /// <param name="name">The name of the table.</param>
    /// <param name="buildAction">An action that performs configuration of the table.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static OwnedNavigationBuilder<TOwnerEntity, TDependentEntity> SplitToTable<TOwnerEntity, TDependentEntity>(
        this OwnedNavigationBuilder<TOwnerEntity, TDependentEntity> ownedNavigationBuilder,
        string name,
        Action<OwnedNavigationSplitTableBuilder<TOwnerEntity, TDependentEntity>> buildAction)
        where TOwnerEntity : class
        where TDependentEntity : class
        => ownedNavigationBuilder.SplitToTable(name, null, buildAction);

    /// <summary>
    ///     Configures some of the properties on this entity type to be mapped to a different table.
    ///     The primary key properties are mapped to all tables, other properties must be explicitly mapped.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
    /// </remarks>
    /// <param name="ownedNavigationBuilder">The builder for the entity type being configured.</param>
    /// <param name="name">The name of the table.</param>
    /// <param name="schema">The schema of the table.</param>
    /// <param name="buildAction">An action that performs configuration of the table.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static OwnedNavigationBuilder SplitToTable(
        this OwnedNavigationBuilder ownedNavigationBuilder,
        string name,
        string? schema,
        Action<OwnedNavigationSplitTableBuilder> buildAction)
    {
        Check.NotNull(name, nameof(name));
        Check.NullButNotEmpty(schema, nameof(schema));
        Check.NotNull(buildAction, nameof(buildAction));

        buildAction(
            new OwnedNavigationSplitTableBuilder(
                StoreObjectIdentifier.Table(name, schema ?? ownedNavigationBuilder.OwnedEntityType.GetDefaultSchema()),
                ownedNavigationBuilder));

        return ownedNavigationBuilder;
    }

    /// <summary>
    ///     Configures some of the properties on this entity type to be mapped to a different table.
    ///     The primary key properties are mapped to all tables, other properties must be explicitly mapped.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
    /// </remarks>
    /// <typeparam name="TOwnerEntity">The entity type owning the relationship.</typeparam>
    /// <typeparam name="TDependentEntity">The dependent entity type of the relationship.</typeparam>
    /// <param name="ownedNavigationBuilder">The builder for the entity type being configured.</param>
    /// <param name="name">The name of the table.</param>
    /// <param name="schema">The schema of the table.</param>
    /// <param name="buildAction">An action that performs configuration of the table.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static OwnedNavigationBuilder<TOwnerEntity, TDependentEntity> SplitToTable<TOwnerEntity, TDependentEntity>(
        this OwnedNavigationBuilder<TOwnerEntity, TDependentEntity> ownedNavigationBuilder,
        string name,
        string? schema,
        Action<OwnedNavigationSplitTableBuilder<TOwnerEntity, TDependentEntity>> buildAction)
        where TOwnerEntity : class
        where TDependentEntity : class
    {
        Check.NotNull(name, nameof(name));
        Check.NullButNotEmpty(schema, nameof(schema));
        Check.NotNull(buildAction, nameof(buildAction));

        buildAction(
            new OwnedNavigationSplitTableBuilder<TOwnerEntity, TDependentEntity>(
                StoreObjectIdentifier.Table(name, schema ?? ownedNavigationBuilder.OwnedEntityType.GetDefaultSchema()),
                ownedNavigationBuilder));

        return ownedNavigationBuilder;
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
}
