// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Reflection;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.SharePoint.Metadata.Internal;


namespace Microsoft.EntityFrameworkCore.SharePoint.Design.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class SharePointAnnotationCodeGenerator : AnnotationCodeGenerator
{
    private static readonly MethodInfo EntityTypeToListMethodInfo =
        typeof(SharePointEntityTypeBuilderExtensions).GetRuntimeMethod(
            nameof(SharePointEntityTypeBuilderExtensions.ToList), new[] { typeof(EntityTypeBuilder), typeof(string) })!;

    private static readonly MethodInfo EntityTypeToViewMethodInfo =
        typeof(SharePointEntityTypeBuilderExtensions).GetRuntimeMethod(
            nameof(SharePointEntityTypeBuilderExtensions.ToView), new[] { typeof(EntityTypeBuilder), typeof(string) })!;

    private static readonly MethodInfo PropertyFieldTypeMethodInfo =
        typeof(SharePointPropertyBuilderExtensions).GetRuntimeMethod(
            nameof(SharePointPropertyBuilderExtensions.HasFieldType), new[] { typeof(PropertyBuilder), typeof(string) })!;

    private static readonly MethodInfo PropertyFieldInternalNameMethodInfo =
        typeof(SharePointPropertyBuilderExtensions).GetRuntimeMethod(
            nameof(SharePointPropertyBuilderExtensions.HasFieldInternalName), new[] { typeof(PropertyBuilder), typeof(string) })!;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SharePointAnnotationCodeGenerator(AnnotationCodeGeneratorDependencies dependencies)
        : base(dependencies)
    {
    }

    /// <summary>
    /// Generates fluent API calls for the specified entity type and its annotations.
    /// </summary>
    /// <param name="entityType">The entity type for which to generate fluent API calls.</param>
    /// <param name="annotations">The annotations associated with the entity type.</param>
    /// <returns>A list of method call code fragments representing the fluent API calls.</returns>
    public override IReadOnlyList<MethodCallCodeFragment> GenerateFluentApiCalls(
        IEntityType entityType,
        IDictionary<string, IAnnotation> annotations)
    {
        var fragments = new List<MethodCallCodeFragment>(base.GenerateFluentApiCalls(entityType, annotations));

        if (annotations.Remove(SharePointAnnotationNames.ListName, out var listNameAnnotation) && listNameAnnotation.Value is string listName)
        {
            fragments.Add(new MethodCallCodeFragment(EntityTypeToListMethodInfo, listName));
        }

        if (annotations.Remove(SharePointAnnotationNames.ViewName, out var viewNameAnnotation) && viewNameAnnotation.Value is string viewName)
        {
            fragments.Add(new MethodCallCodeFragment(EntityTypeToViewMethodInfo, viewName));
        }

        return fragments;
    }

    /// <summary>
    /// Generates fluent API calls for the specified property and its annotations.
    /// </summary>
    /// <param name="property">The property for which to generate fluent API calls.</param>
    /// <param name="annotations">The annotations associated with the property.</param>
    /// <returns>A list of method call code fragments representing the fluent API calls.</returns>
    public override IReadOnlyList<MethodCallCodeFragment> GenerateFluentApiCalls(
        IProperty property,
        IDictionary<string, IAnnotation> annotations)
    {
        var fragments = new List<MethodCallCodeFragment>(base.GenerateFluentApiCalls(property, annotations));

        if (annotations.Remove(SharePointAnnotationNames.FieldType, out var fieldTypeAnnotation) && fieldTypeAnnotation.Value is string fieldType)
        {
            fragments.Add(new MethodCallCodeFragment(PropertyFieldTypeMethodInfo, fieldType));
        }

        if (annotations.Remove(SharePointAnnotationNames.FieldInternalName, out var fieldInternalNameAnnotation) && fieldInternalNameAnnotation.Value is string fieldInternalName)
        {
            fragments.Add(new MethodCallCodeFragment(PropertyFieldInternalNameMethodInfo, fieldInternalName));
        }

        return fragments;
    }


    /// <summary>
    /// Determines whether the given annotation is handled by convention for the specified entity type.
    /// </summary>
    /// <param name="entityType">The entity type to check.</param>
    /// <param name="annotation">The annotation to evaluate.</param>
    /// <returns>
    /// <see langword="true"/> if the annotation is handled by convention; otherwise, <see langword="false"/>.
    /// </returns>
    protected override bool IsHandledByConvention(IEntityType entityType, IAnnotation annotation)
    {
        // No SharePoint-specific conventions yet; fallback to base
        return base.IsHandledByConvention(entityType, annotation);
    }

    /// <summary>
    /// Determines whether the given annotation is handled by convention for the specified property.
    /// </summary>
    /// <param name="property">The property to check.</param>
    /// <param name="annotation">The annotation to evaluate.</param>
    /// <returns>
    /// <see langword="true"/> if the annotation is handled by convention; otherwise, <see langword="false"/>.
    /// </returns>
    protected override bool IsHandledByConvention(IProperty property, IAnnotation annotation)
    {
        // No SharePoint-specific conventions yet; fallback to base
        return base.IsHandledByConvention(property, annotation);
    }
}
