// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     SharePoint specific extension methods for <see cref="ModelBuilder" />.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
///     <see href="https://aka.ms/efcore-docs-sharepoint">Accessing SharePoint lists with EF Core</see>
///     for more information and examples.
/// </remarks>
public static class SharePointModelBuilderExtensions
{
    /// <summary>
    ///     Configures the model to use a SharePoint list as the data source for the specified entity type.
    /// </summary>
    /// <param name="modelBuilder">The model builder.</param>
    /// <param name="listName">The name of the SharePoint list.</param>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static ModelBuilder UseSharePointList<TEntity>(
        this ModelBuilder modelBuilder,
        string listName)
        where TEntity : class
    {
        Check.NotNull(listName, nameof(listName));

        modelBuilder.Entity<TEntity>().ToTable(listName, t => t.IsTemporal(false));
        return modelBuilder;
    }

    /// <summary>
    ///     Configures the model to map a SharePoint list column to a property.
    /// </summary>
    /// <param name="entityTypeBuilder">The entity type builder.</param>
    /// <param name="propertyName">The name of the property.</param>
    /// <param name="columnName">The name of the SharePoint list column.</param>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static EntityTypeBuilder<TEntity> MapSharePointColumn<TEntity>(
        this EntityTypeBuilder<TEntity> entityTypeBuilder,
        string propertyName,
        string columnName)
        where TEntity : class
    {
        Check.NotNull(propertyName, nameof(propertyName));
        Check.NotNull(columnName, nameof(columnName));

        entityTypeBuilder.Property(propertyName).HasColumnName(columnName);
        return entityTypeBuilder;
    }

    /// <summary>
    ///     Configures the model to use a specific SharePoint list view for querying data.
    /// </summary>
    /// <param name="modelBuilder">The model builder.</param>
    /// <param name="viewName">The name of the SharePoint list view.</param>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static ModelBuilder UseSharePointListView<TEntity>(
        this ModelBuilder modelBuilder,
        string viewName)
        where TEntity : class
    {
        Check.NotNull(viewName, nameof(viewName));

        modelBuilder.Entity<TEntity>().ToTable(viewName);
        return modelBuilder;
    }

    /// <summary>
    ///     Configures the model to use a specific SharePoint list content type for the entity.
    /// </summary>
    /// <param name="modelBuilder">The model builder.</param>
    /// <param name="contentTypeId">The ID of the SharePoint content type.</param>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static ModelBuilder UseSharePointContentType<TEntity>(
        this ModelBuilder modelBuilder,
        string contentTypeId)
        where TEntity : class
    {
        Check.NotNull(contentTypeId, nameof(contentTypeId));

        modelBuilder.Entity<TEntity>().HasAnnotation("SharePoint:ContentTypeId", contentTypeId);
        return modelBuilder;
    }
}
