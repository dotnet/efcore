// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     Relational database specific extension methods for <see cref="OwnedNavigationBuilder" />.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
/// </remarks>
public static class RelationalOwnedNavigationBuilderExtensions
{
    /// <summary>
    ///     Configures a relationship where this entity type and the entities that it owns are mapped to a JSON column in the database.
    /// </summary>
    /// <remarks>
    ///     This method should only be specified for the outer-most owned entity in the given ownership structure.
    ///     All entities owned by this will be automatically mapped to the same JSON column.
    ///     The ownerships must still be explicitly defined.
    ///     Name of the navigation will be used as the JSON column name.
    /// </remarks>
    /// <param name="builder">The builder for the owned navigation being configured.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static OwnedNavigationBuilder ToJson(this OwnedNavigationBuilder builder)
    {
        var navigationName = builder.Metadata.GetNavigation(pointsToPrincipal: false)!.Name;
        builder.ToJson(navigationName);

        return builder;
    }

    /// <summary>
    ///     Configures a relationship where this entity type and the entities that it owns are mapped to a JSON column in the database.
    /// </summary>
    /// <remarks>
    ///     This method should only be specified for the outer-most owned entity in the given ownership structure.
    ///     All entities owned by this will be automatically mapped to the same JSON column.
    ///     The ownerships must still be explicitly defined.
    ///     Name of the navigation will be used as the JSON column name.
    /// </remarks>
    /// <param name="builder">The builder for the owned navigation being configured.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static OwnedNavigationBuilder<TOwnerEntity, TDependentEntity> ToJson<TOwnerEntity, TDependentEntity>(
        this OwnedNavigationBuilder<TOwnerEntity, TDependentEntity> builder)
        where TOwnerEntity : class
        where TDependentEntity : class
    {
        var navigationName = builder.Metadata.GetNavigation(pointsToPrincipal: false)!.Name;
        builder.ToJson(navigationName);

        return builder;
    }

    /// <summary>
    ///     Configures a relationship where this entity type and the entities that it owns are mapped to a JSON column in the database.
    /// </summary>
    /// <remarks>
    ///     This method should only be specified for the outer-most owned entity in the given ownership structure.
    ///     All entities owned by this will be automatically mapped to the same JSON column.
    ///     The ownerships must still be explicitly defined.
    /// </remarks>
    /// <param name="builder">The builder for the owned navigation being configured.</param>
    /// <param name="jsonColumnName">JSON column name to use.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static OwnedNavigationBuilder<TOwnerEntity, TDependentEntity> ToJson<TOwnerEntity, TDependentEntity>(
        this OwnedNavigationBuilder<TOwnerEntity, TDependentEntity> builder,
        string? jsonColumnName)
        where TOwnerEntity : class
        where TDependentEntity : class
    {
        builder.OwnedEntityType.SetContainerColumnName(jsonColumnName);

        return builder;
    }

    /// <summary>
    ///     Configures a relationship where this entity type and the entities that it owns are mapped to a JSON column in the database.
    /// </summary>
    /// <remarks>
    ///     This method should only be specified for the outer-most owned entity in the given ownership structure.
    ///     All entities owned by this will be automatically mapped to the same JSON column.
    ///     The ownerships must still be explicitly defined.
    /// </remarks>
    /// <param name="builder">The builder for the owned navigation being configured.</param>
    /// <param name="jsonColumnName">JSON column name to use.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static OwnedNavigationBuilder ToJson(
        this OwnedNavigationBuilder builder,
        string? jsonColumnName)
    {
        builder.OwnedEntityType.SetContainerColumnName(jsonColumnName);

        return builder;
    }

    /// <summary>
    ///     Configures the navigation of an entity mapped to a JSON column, mapping the navigation to a specific JSON property,
    ///     rather than using the navigation name.
    /// </summary>
    /// <param name="navigationBuilder">The builder for the navigation being configured.</param>
    /// <param name="name">JSON property name to be used.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static OwnedNavigationBuilder HasJsonPropertyName(
        this OwnedNavigationBuilder navigationBuilder,
        string? name)
    {
        Check.NullButNotEmpty(name, nameof(name));

        if (!navigationBuilder.Metadata.PrincipalEntityType.IsOwned())
        {
            throw new InvalidOperationException(
                RelationalStrings.JsonPropertyNameShouldBeConfiguredOnNestedNavigation);
        }

        navigationBuilder.Metadata.DeclaringEntityType.SetJsonPropertyName(name);

        return navigationBuilder;
    }

    /// <summary>
    ///     Configures the navigation of an entity mapped to a JSON column, mapping the navigation to a specific JSON property,
    ///     rather than using the navigation name.
    /// </summary>
    /// <param name="navigationBuilder">The builder for the navigation being configured.</param>
    /// <param name="name">JSON property name to be used.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static OwnedNavigationBuilder<TSource, TTarget> HasJsonPropertyName<TSource, TTarget>(
        this OwnedNavigationBuilder<TSource, TTarget> navigationBuilder,
        string? name)
        where TSource : class
        where TTarget : class
        => (OwnedNavigationBuilder<TSource, TTarget>)HasJsonPropertyName((OwnedNavigationBuilder)navigationBuilder, name);
}
