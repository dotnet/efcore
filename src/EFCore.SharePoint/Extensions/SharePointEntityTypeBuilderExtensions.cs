// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.SharePoint.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     SharePoint specific extension methods for <see cref="EntityTypeBuilder" />.
/// </summary>
/// <remarks>
///     See SharePoint EF Core provider documentation for more information and examples.
/// </remarks>
public static class SharePointEntityTypeBuilderExtensions
{
    /// <summary>
    ///     Configures the SharePoint list that the entity maps to.
    /// </summary>
    /// <param name="entityTypeBuilder">The builder for the entity type being configured.</param>
    /// <param name="listName">The name of the SharePoint list.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static EntityTypeBuilder ToList(
        this EntityTypeBuilder entityTypeBuilder,
        string listName)
    {
        entityTypeBuilder.Metadata.SetAnnotation(SharePointAnnotationNames.ListName, listName);
        return entityTypeBuilder;
    }

    /// <summary>
    ///     Configures the SharePoint list that the entity maps to.
    /// </summary>
    /// <typeparam name="TEntity">The entity type being configured.</typeparam>
    /// <param name="entityTypeBuilder">The builder for the entity type being configured.</param>
    /// <param name="listName">The name of the SharePoint list.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static EntityTypeBuilder<TEntity> ToList<TEntity>(
        this EntityTypeBuilder<TEntity> entityTypeBuilder,
        string listName)
        where TEntity : class
    {
        entityTypeBuilder.Metadata.SetAnnotation(SharePointAnnotationNames.ListName, listName);
        return entityTypeBuilder;
    }

    /// <summary>
    ///     Configures the SharePoint view that the entity maps to.
    /// </summary>
    /// <param name="entityTypeBuilder">The builder for the entity type being configured.</param>
    /// <param name="viewName">The name of the SharePoint view.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static EntityTypeBuilder ToView(
        this EntityTypeBuilder entityTypeBuilder,
        string viewName)
    {
        entityTypeBuilder.Metadata.SetAnnotation(SharePointAnnotationNames.ViewName, viewName);
        return entityTypeBuilder;
    }

    /// <summary>
    ///     Configures the SharePoint view that the entity maps to.
    /// </summary>
    /// <typeparam name="TEntity">The entity type being configured.</typeparam>
    /// <param name="entityTypeBuilder">The builder for the entity type being configured.</param>
    /// <param name="viewName">The name of the SharePoint view.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static EntityTypeBuilder<TEntity> ToView<TEntity>(
        this EntityTypeBuilder<TEntity> entityTypeBuilder,
        string viewName)
        where TEntity : class
    {
        entityTypeBuilder.Metadata.SetAnnotation(SharePointAnnotationNames.ViewName, viewName);
        return entityTypeBuilder;
    }

    /// <summary>
    ///     Configures the SharePoint list that the entity maps to using a convention builder.
    /// </summary>
    /// <param name="entityTypeBuilder">The convention entity type builder.</param>
    /// <param name="listName">The name of the SharePoint list.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     The same builder instance if the configuration was applied,
    ///     <see langword="null" /> otherwise.
    /// </returns>
    public static IConventionEntityTypeBuilder? ToList(
        this IConventionEntityTypeBuilder entityTypeBuilder,
        string? listName,
        bool fromDataAnnotation = false)
    {
        if (entityTypeBuilder.CanSetAnnotation(SharePointAnnotationNames.ListName, listName, fromDataAnnotation))
        {
            entityTypeBuilder.Metadata.SetAnnotation(SharePointAnnotationNames.ListName, listName, fromDataAnnotation);
            return entityTypeBuilder;
        }
        return null;
    }

    /// <summary>
    ///     Configures the SharePoint view that the entity maps to using a convention builder.
    /// </summary>
    /// <param name="entityTypeBuilder">The convention entity type builder.</param>
    /// <param name="viewName">The name of the SharePoint view.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     The same builder instance if the configuration was applied,
    ///     <see langword="null" /> otherwise.
    /// </returns>
    public static IConventionEntityTypeBuilder? ToView(
        this IConventionEntityTypeBuilder entityTypeBuilder,
        string? viewName,
        bool fromDataAnnotation = false)
    {
        if (entityTypeBuilder.CanSetAnnotation(SharePointAnnotationNames.ViewName, viewName, fromDataAnnotation))
        {
            entityTypeBuilder.Metadata.SetAnnotation(SharePointAnnotationNames.ViewName, viewName, fromDataAnnotation);
            return entityTypeBuilder;
        }
        return null;
    }
}
