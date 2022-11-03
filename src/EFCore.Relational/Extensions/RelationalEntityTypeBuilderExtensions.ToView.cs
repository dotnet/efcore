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
        => entityTypeBuilder.ToView(name, (string?)null);

    /// <summary>
    ///     Configures the view that the entity type maps to when targeting a relational database.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
    /// </remarks>
    /// <typeparam name="TEntity">The entity type being configured.</typeparam>
    /// <param name="entityTypeBuilder">The builder for the entity type being configured.</param>
    /// <param name="name">The name of the view.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static EntityTypeBuilder<TEntity> ToView<TEntity>(
        this EntityTypeBuilder<TEntity> entityTypeBuilder,
        string? name)
        where TEntity : class
        => (EntityTypeBuilder<TEntity>)ToView((EntityTypeBuilder)entityTypeBuilder, name);

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
    /// <param name="entityTypeBuilder">The builder for the entity type being configured.</param>
    /// <param name="name">The name of the view.</param>
    /// <param name="buildAction">An action that performs configuration of the view.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static EntityTypeBuilder ToView(
        this EntityTypeBuilder entityTypeBuilder,
        string name,
        Action<ViewBuilder> buildAction)
        => entityTypeBuilder.ToView(name, null, buildAction);

    /// <summary>
    ///     Configures the view that the entity type maps to when targeting a relational database.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
    /// </remarks>
    /// <typeparam name="TEntity">The entity type being configured.</typeparam>
    /// <param name="entityTypeBuilder">The builder for the entity type being configured.</param>
    /// <param name="name">The name of the view.</param>
    /// <param name="buildAction">An action that performs configuration of the view.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static EntityTypeBuilder<TEntity> ToView<TEntity>(
        this EntityTypeBuilder<TEntity> entityTypeBuilder,
        string name,
        Action<ViewBuilder<TEntity>> buildAction)
        where TEntity : class
        => ToView(entityTypeBuilder, name, null, buildAction);

    /// <summary>
    ///     Configures the view that the entity type maps to when targeting a relational database.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
    /// </remarks>
    /// <param name="entityTypeBuilder">The builder for the entity type being configured.</param>
    /// <param name="name">The name of the view.</param>
    /// <param name="schema">The schema of the view.</param>
    /// <param name="buildAction">An action that performs configuration of the view.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static EntityTypeBuilder ToView(
        this EntityTypeBuilder entityTypeBuilder,
        string name,
        string? schema,
        Action<ViewBuilder> buildAction)
    {
        Check.NotNull(name, nameof(name));
        Check.NullButNotEmpty(schema, nameof(schema));

        entityTypeBuilder.Metadata.SetViewName(name);
        entityTypeBuilder.Metadata.SetViewSchema(schema);
        entityTypeBuilder.Metadata.SetAnnotation(RelationalAnnotationNames.ViewDefinitionSql, null);
        buildAction(new ViewBuilder(StoreObjectIdentifier.View(name, entityTypeBuilder.Metadata.GetViewSchema()), entityTypeBuilder));

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
    /// <param name="buildAction">An action that performs configuration of the view.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static EntityTypeBuilder<TEntity> ToView<TEntity>(
        this EntityTypeBuilder<TEntity> entityTypeBuilder,
        string name,
        string? schema,
        Action<ViewBuilder<TEntity>> buildAction)
        where TEntity : class
    {
        Check.NotNull(name, nameof(name));
        Check.NullButNotEmpty(schema, nameof(schema));

        entityTypeBuilder.Metadata.SetViewName(name);
        entityTypeBuilder.Metadata.SetViewSchema(schema);
        entityTypeBuilder.Metadata.SetAnnotation(RelationalAnnotationNames.ViewDefinitionSql, null);
        buildAction(
            new ViewBuilder<TEntity>(StoreObjectIdentifier.View(name, entityTypeBuilder.Metadata.GetViewSchema()), entityTypeBuilder));

        return entityTypeBuilder;
    }

    /// <summary>
    ///     Configures the view that the entity type maps to when targeting a relational database.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
    /// </remarks>
    /// <param name="ownedNavigationBuilder">The builder for the entity type being configured.</param>
    /// <param name="name">The name of the view.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static OwnedNavigationBuilder ToView(
        this OwnedNavigationBuilder ownedNavigationBuilder,
        string? name)
        => ownedNavigationBuilder.ToView(name, (string?)null);

    /// <summary>
    ///     Configures the view that the entity type maps to when targeting a relational database.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
    /// </remarks>
    /// <typeparam name="TOwnerEntity">The entity type owning the relationship.</typeparam>
    /// <typeparam name="TDependentEntity">The dependent entity type of the relationship.</typeparam>
    /// <param name="ownedNavigationBuilder">The builder for the entity type being configured.</param>
    /// <param name="name">The name of the view.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static OwnedNavigationBuilder<TOwnerEntity, TDependentEntity> ToView<TOwnerEntity, TDependentEntity>(
        this OwnedNavigationBuilder<TOwnerEntity, TDependentEntity> ownedNavigationBuilder,
        string? name)
        where TOwnerEntity : class
        where TDependentEntity : class
        => (OwnedNavigationBuilder<TOwnerEntity, TDependentEntity>)ToView((OwnedNavigationBuilder)ownedNavigationBuilder, name);

    /// <summary>
    ///     Configures the view that the entity type maps to when targeting a relational database.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
    /// </remarks>
    /// <param name="ownedNavigationBuilder">The builder for the entity type being configured.</param>
    /// <param name="name">The name of the view.</param>
    /// <param name="schema">The schema of the view.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static OwnedNavigationBuilder ToView(
        this OwnedNavigationBuilder ownedNavigationBuilder,
        string? name,
        string? schema)
    {
        Check.NullButNotEmpty(name, nameof(name));
        Check.NullButNotEmpty(schema, nameof(schema));

        ownedNavigationBuilder.OwnedEntityType.SetViewName(name);
        ownedNavigationBuilder.OwnedEntityType.SetViewSchema(schema);
        ownedNavigationBuilder.OwnedEntityType.SetAnnotation(RelationalAnnotationNames.ViewDefinitionSql, null);

        return ownedNavigationBuilder;
    }

    /// <summary>
    ///     Configures the view that the entity type maps to when targeting a relational database.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
    /// </remarks>
    /// <typeparam name="TOwnerEntity">The entity type owning the relationship.</typeparam>
    /// <typeparam name="TDependentEntity">The dependent entity type of the relationship.</typeparam>
    /// <param name="ownedNavigationBuilder">The builder for the entity type being configured.</param>
    /// <param name="name">The name of the view.</param>
    /// <param name="schema">The schema of the view.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static OwnedNavigationBuilder<TOwnerEntity, TDependentEntity> ToView<TOwnerEntity, TDependentEntity>(
        this OwnedNavigationBuilder<TOwnerEntity, TDependentEntity> ownedNavigationBuilder,
        string? name,
        string? schema)
        where TOwnerEntity : class
        where TDependentEntity : class
        => (OwnedNavigationBuilder<TOwnerEntity, TDependentEntity>)ToView(
            (OwnedNavigationBuilder)ownedNavigationBuilder, name, schema);

    /// <summary>
    ///     Configures the view that the entity type maps to when targeting a relational database.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
    /// </remarks>
    /// <param name="ownedNavigationBuilder">The builder for the entity type being configured.</param>
    /// <param name="name">The name of the view.</param>
    /// <param name="buildAction">An action that performs configuration of the view.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static OwnedNavigationBuilder ToView(
        this OwnedNavigationBuilder ownedNavigationBuilder,
        string name,
        Action<OwnedNavigationViewBuilder> buildAction)
        => ownedNavigationBuilder.ToView(name, null, buildAction);

    /// <summary>
    ///     Configures the view that the entity type maps to when targeting a relational database.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
    /// </remarks>
    /// <typeparam name="TOwnerEntity">The entity type owning the relationship.</typeparam>
    /// <typeparam name="TDependentEntity">The dependent entity type of the relationship.</typeparam>
    /// <param name="ownedNavigationBuilder">The builder for the entity type being configured.</param>
    /// <param name="name">The name of the view.</param>
    /// <param name="buildAction">An action that performs configuration of the view.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static OwnedNavigationBuilder<TOwnerEntity, TDependentEntity> ToView<TOwnerEntity, TDependentEntity>(
        this OwnedNavigationBuilder<TOwnerEntity, TDependentEntity> ownedNavigationBuilder,
        string name,
        Action<OwnedNavigationViewBuilder<TOwnerEntity, TDependentEntity>> buildAction)
        where TOwnerEntity : class
        where TDependentEntity : class
        => ToView(ownedNavigationBuilder, name, null, buildAction);

    /// <summary>
    ///     Configures the view that the entity type maps to when targeting a relational database.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
    /// </remarks>
    /// <param name="ownedNavigationBuilder">The builder for the entity type being configured.</param>
    /// <param name="name">The name of the view.</param>
    /// <param name="schema">The schema of the view.</param>
    /// <param name="buildAction">An action that performs configuration of the view.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static OwnedNavigationBuilder ToView(
        this OwnedNavigationBuilder ownedNavigationBuilder,
        string name,
        string? schema,
        Action<OwnedNavigationViewBuilder> buildAction)
    {
        Check.NotNull(name, nameof(name));
        Check.NullButNotEmpty(schema, nameof(schema));

        ownedNavigationBuilder.OwnedEntityType.SetViewName(name);
        ownedNavigationBuilder.OwnedEntityType.SetViewSchema(schema);
        ownedNavigationBuilder.OwnedEntityType.SetAnnotation(RelationalAnnotationNames.ViewDefinitionSql, null);
        buildAction(
            new OwnedNavigationViewBuilder(
                StoreObjectIdentifier.View(name, ownedNavigationBuilder.OwnedEntityType.GetViewSchema()), ownedNavigationBuilder));

        return ownedNavigationBuilder;
    }

    /// <summary>
    ///     Configures the view that the entity type maps to when targeting a relational database.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
    /// </remarks>
    /// <typeparam name="TOwnerEntity">The entity type owning the relationship.</typeparam>
    /// <typeparam name="TDependentEntity">The dependent entity type of the relationship.</typeparam>
    /// <param name="ownedNavigationBuilder">The builder for the entity type being configured.</param>
    /// <param name="name">The name of the view.</param>
    /// <param name="schema">The schema of the view.</param>
    /// <param name="buildAction">An action that performs configuration of the view.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static OwnedNavigationBuilder<TOwnerEntity, TDependentEntity> ToView<TOwnerEntity, TDependentEntity>(
        this OwnedNavigationBuilder<TOwnerEntity, TDependentEntity> ownedNavigationBuilder,
        string name,
        string? schema,
        Action<OwnedNavigationViewBuilder<TOwnerEntity, TDependentEntity>> buildAction)
        where TOwnerEntity : class
        where TDependentEntity : class
    {
        Check.NotNull(name, nameof(name));
        Check.NullButNotEmpty(schema, nameof(schema));

        ownedNavigationBuilder.OwnedEntityType.SetViewName(name);
        ownedNavigationBuilder.OwnedEntityType.SetViewSchema(schema);
        ownedNavigationBuilder.OwnedEntityType.SetAnnotation(RelationalAnnotationNames.ViewDefinitionSql, null);
        buildAction(
            new OwnedNavigationViewBuilder<TOwnerEntity, TDependentEntity>(
                StoreObjectIdentifier.View(name, ownedNavigationBuilder.OwnedEntityType.GetViewSchema()), ownedNavigationBuilder));

        return ownedNavigationBuilder;
    }

    /// <summary>
    ///     Configures some of the properties on this entity type to be mapped to a different view.
    ///     The primary key properties are mapped to all views, other properties must be explicitly mapped.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
    /// </remarks>
    /// <param name="entityTypeBuilder">The builder for the entity type being configured.</param>
    /// <param name="name">The name of the view.</param>
    /// <param name="buildAction">An action that performs configuration of the view.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static EntityTypeBuilder SplitToView(
        this EntityTypeBuilder entityTypeBuilder,
        string name,
        Action<SplitViewBuilder> buildAction)
        => entityTypeBuilder.SplitToView(name, null, buildAction);

    /// <summary>
    ///     Configures some of the properties on this entity type to be mapped to a different view.
    ///     The primary key properties are mapped to all views, other properties must be explicitly mapped.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
    /// </remarks>
    /// <typeparam name="TEntity">The entity type being configured.</typeparam>
    /// <param name="entityTypeBuilder">The builder for the entity type being configured.</param>
    /// <param name="name">The name of the view.</param>
    /// <param name="buildAction">An action that performs configuration of the view.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static EntityTypeBuilder<TEntity> SplitToView<TEntity>(
        this EntityTypeBuilder<TEntity> entityTypeBuilder,
        string name,
        Action<SplitViewBuilder<TEntity>> buildAction)
        where TEntity : class
        => entityTypeBuilder.SplitToView(name, null, buildAction);

    /// <summary>
    ///     Configures some of the properties on this entity type to be mapped to a different view.
    ///     The primary key properties are mapped to all views, other properties must be explicitly mapped.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
    /// </remarks>
    /// <param name="entityTypeBuilder">The builder for the entity type being configured.</param>
    /// <param name="name">The name of the view.</param>
    /// <param name="schema">The schema of the view.</param>
    /// <param name="buildAction">An action that performs configuration of the view.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static EntityTypeBuilder SplitToView(
        this EntityTypeBuilder entityTypeBuilder,
        string name,
        string? schema,
        Action<SplitViewBuilder> buildAction)
    {
        Check.NotNull(name, nameof(name));
        Check.NullButNotEmpty(schema, nameof(schema));
        Check.NotNull(buildAction, nameof(buildAction));

        buildAction(
            new SplitViewBuilder(
                StoreObjectIdentifier.View(name, schema ?? entityTypeBuilder.Metadata.GetDefaultViewSchema()), entityTypeBuilder));

        return entityTypeBuilder;
    }

    /// <summary>
    ///     Configures some of the properties on this entity type to be mapped to a different view.
    ///     The primary key properties are mapped to all views, other properties must be explicitly mapped.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
    /// </remarks>
    /// <typeparam name="TEntity">The entity type being configured.</typeparam>
    /// <param name="entityTypeBuilder">The builder for the entity type being configured.</param>
    /// <param name="name">The name of the view.</param>
    /// <param name="schema">The schema of the view.</param>
    /// <param name="buildAction">An action that performs configuration of the view.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static EntityTypeBuilder<TEntity> SplitToView<TEntity>(
        this EntityTypeBuilder<TEntity> entityTypeBuilder,
        string name,
        string? schema,
        Action<SplitViewBuilder<TEntity>> buildAction)
        where TEntity : class
    {
        Check.NotNull(name, nameof(name));
        Check.NullButNotEmpty(schema, nameof(schema));
        Check.NotNull(buildAction, nameof(buildAction));

        buildAction(
            new SplitViewBuilder<TEntity>(
                StoreObjectIdentifier.View(name, schema ?? entityTypeBuilder.Metadata.GetDefaultViewSchema()), entityTypeBuilder));

        return entityTypeBuilder;
    }

    /// <summary>
    ///     Configures some of the properties on this entity type to be mapped to a different view.
    ///     The primary key properties are mapped to all views, other properties must be explicitly mapped.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
    /// </remarks>
    /// <param name="ownedNavigationBuilder">The builder for the entity type being configured.</param>
    /// <param name="name">The name of the view.</param>
    /// <param name="buildAction">An action that performs configuration of the view.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static OwnedNavigationBuilder SplitToView(
        this OwnedNavigationBuilder ownedNavigationBuilder,
        string name,
        Action<OwnedNavigationSplitViewBuilder> buildAction)
        => ownedNavigationBuilder.SplitToView(name, null, buildAction);

    /// <summary>
    ///     Configures some of the properties on this entity type to be mapped to a different view.
    ///     The primary key properties are mapped to all views, other properties must be explicitly mapped.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
    /// </remarks>
    /// <typeparam name="TOwnerEntity">The entity type owning the relationship.</typeparam>
    /// <typeparam name="TDependentEntity">The dependent entity type of the relationship.</typeparam>
    /// <param name="ownedNavigationBuilder">The builder for the entity type being configured.</param>
    /// <param name="name">The name of the view.</param>
    /// <param name="buildAction">An action that performs configuration of the view.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static OwnedNavigationBuilder<TOwnerEntity, TDependentEntity> SplitToView<TOwnerEntity, TDependentEntity>(
        this OwnedNavigationBuilder<TOwnerEntity, TDependentEntity> ownedNavigationBuilder,
        string name,
        Action<OwnedNavigationSplitViewBuilder<TOwnerEntity, TDependentEntity>> buildAction)
        where TOwnerEntity : class
        where TDependentEntity : class
        => ownedNavigationBuilder.SplitToView(name, null, buildAction);

    /// <summary>
    ///     Configures some of the properties on this entity type to be mapped to a different view.
    ///     The primary key properties are mapped to all views, other properties must be explicitly mapped.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
    /// </remarks>
    /// <param name="ownedNavigationBuilder">The builder for the entity type being configured.</param>
    /// <param name="name">The name of the view.</param>
    /// <param name="schema">The schema of the view.</param>
    /// <param name="buildAction">An action that performs configuration of the view.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static OwnedNavigationBuilder SplitToView(
        this OwnedNavigationBuilder ownedNavigationBuilder,
        string name,
        string? schema,
        Action<OwnedNavigationSplitViewBuilder> buildAction)
    {
        Check.NotNull(name, nameof(name));
        Check.NullButNotEmpty(schema, nameof(schema));
        Check.NotNull(buildAction, nameof(buildAction));

        buildAction(
            new OwnedNavigationSplitViewBuilder(
                StoreObjectIdentifier.View(name, schema ?? ownedNavigationBuilder.OwnedEntityType.GetDefaultViewSchema()),
                ownedNavigationBuilder));

        return ownedNavigationBuilder;
    }

    /// <summary>
    ///     Configures some of the properties on this entity type to be mapped to a different view.
    ///     The primary key properties are mapped to all views, other properties must be explicitly mapped.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
    /// </remarks>
    /// <typeparam name="TOwnerEntity">The entity type owning the relationship.</typeparam>
    /// <typeparam name="TDependentEntity">The dependent entity type of the relationship.</typeparam>
    /// <param name="ownedNavigationBuilder">The builder for the entity type being configured.</param>
    /// <param name="name">The name of the view.</param>
    /// <param name="schema">The schema of the view.</param>
    /// <param name="buildAction">An action that performs configuration of the view.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static OwnedNavigationBuilder<TOwnerEntity, TDependentEntity> SplitToView<TOwnerEntity, TDependentEntity>(
        this OwnedNavigationBuilder<TOwnerEntity, TDependentEntity> ownedNavigationBuilder,
        string name,
        string? schema,
        Action<OwnedNavigationSplitViewBuilder<TOwnerEntity, TDependentEntity>> buildAction)
        where TOwnerEntity : class
        where TDependentEntity : class
    {
        Check.NotNull(name, nameof(name));
        Check.NullButNotEmpty(schema, nameof(schema));
        Check.NotNull(buildAction, nameof(buildAction));

        buildAction(
            new OwnedNavigationSplitViewBuilder<TOwnerEntity, TDependentEntity>(
                StoreObjectIdentifier.View(name, schema ?? ownedNavigationBuilder.OwnedEntityType.GetDefaultViewSchema()),
                ownedNavigationBuilder));

        return ownedNavigationBuilder;
    }

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
}
