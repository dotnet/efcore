// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.SharePoint.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     SharePoint specific extension methods for <see cref="PropertyBuilder" />.
/// </summary>
/// <remarks>
///     See SharePoint EF Core provider documentation for more information and examples.
/// </remarks>
public static class SharePointPropertyBuilderExtensions
{
    /// <summary>
    ///     Configures the SharePoint field type for the property.
    /// </summary>
    /// <param name="propertyBuilder">The builder for the property being configured.</param>
    /// <param name="fieldType">The SharePoint field type (e.g., "Text", "Number", "DateTime").</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static PropertyBuilder HasFieldType(
        this PropertyBuilder propertyBuilder,
        string fieldType)
    {
        propertyBuilder.Metadata.SetAnnotation(SharePointAnnotationNames.FieldType, fieldType);
        return propertyBuilder;
    }

    /// <summary>
    ///     Configures the SharePoint field type for the property.
    /// </summary>
    /// <typeparam name="TProperty">The type of the property being configured.</typeparam>
    /// <param name="propertyBuilder">The builder for the property being configured.</param>
    /// <param name="fieldType">The SharePoint field type.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static PropertyBuilder<TProperty> HasFieldType<TProperty>(
        this PropertyBuilder<TProperty> propertyBuilder,
        string fieldType)
        => (PropertyBuilder<TProperty>)HasFieldType((PropertyBuilder)propertyBuilder, fieldType);

    /// <summary>
    ///     Configures the SharePoint internal field name for the property.
    /// </summary>
    /// <param name="propertyBuilder">The builder for the property being configured.</param>
    /// <param name="internalName">The SharePoint internal field name.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static PropertyBuilder HasFieldInternalName(
        this PropertyBuilder propertyBuilder,
        string internalName)
    {
        propertyBuilder.Metadata.SetAnnotation(SharePointAnnotationNames.FieldInternalName, internalName);
        return propertyBuilder;
    }

    /// <summary>
    ///     Configures the SharePoint internal field name for the property.
    /// </summary>
    /// <typeparam name="TProperty">The type of the property being configured.</typeparam>
    /// <param name="propertyBuilder">The builder for the property being configured.</param>
    /// <param name="internalName">The SharePoint internal field name.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static PropertyBuilder<TProperty> HasFieldInternalName<TProperty>(
        this PropertyBuilder<TProperty> propertyBuilder,
        string internalName)
        => (PropertyBuilder<TProperty>)HasFieldInternalName((PropertyBuilder)propertyBuilder, internalName);

    /// <summary>
    ///     Configures whether the property is a SharePoint lookup field.
    /// </summary>
    /// <param name="propertyBuilder">The builder for the property being configured.</param>
    /// <param name="isLookup">A value indicating whether the property is a lookup field.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static PropertyBuilder IsLookupField(
        this PropertyBuilder propertyBuilder,
        bool isLookup = true)
    {
        propertyBuilder.Metadata.SetAnnotation(SharePointAnnotationNames.IsLookupField, isLookup);
        return propertyBuilder;
    }

    /// <summary>
    ///     Configures whether the property is a SharePoint lookup field.
    /// </summary>
    /// <typeparam name="TProperty">The type of the property being configured.</typeparam>
    /// <param name="propertyBuilder">The builder for the property being configured.</param>
    /// <param name="isLookup">A value indicating whether the property is a lookup field.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static PropertyBuilder<TProperty> IsLookupField<TProperty>(
        this PropertyBuilder<TProperty> propertyBuilder,
        bool isLookup = true)
        => (PropertyBuilder<TProperty>)IsLookupField((PropertyBuilder)propertyBuilder, isLookup);

    /// <summary>
    ///     Configures the SharePoint column name for the property.
    /// </summary>
    /// <param name="propertyBuilder">The builder for the property being configured.</param>
    /// <param name="columnName">The SharePoint column name.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static PropertyBuilder HasSharePointColumnName(
        this PropertyBuilder propertyBuilder,
        string columnName)
    {
        propertyBuilder.Metadata.SetAnnotation(SharePointAnnotationNames.ColumnName, columnName);
        return propertyBuilder;
    }

    /// <summary>
    ///     Configures the SharePoint column name for the property.
    /// </summary>
    /// <typeparam name="TProperty">The type of the property being configured.</typeparam>
    /// <param name="propertyBuilder">The builder for the property being configured.</param>
    /// <param name="columnName">The SharePoint column name.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static PropertyBuilder<TProperty> HasSharePointColumnName<TProperty>(
        this PropertyBuilder<TProperty> propertyBuilder,
        string columnName)
        => (PropertyBuilder<TProperty>)HasSharePointColumnName((PropertyBuilder)propertyBuilder, columnName);
}
