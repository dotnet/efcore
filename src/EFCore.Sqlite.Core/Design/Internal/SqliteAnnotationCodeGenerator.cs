// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Sqlite.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Sqlite.Design.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class SqliteAnnotationCodeGenerator : AnnotationCodeGenerator
{
    #region MethodInfos

    private static readonly MethodInfo PropertyUseAutoincrementMethodInfo
        = typeof(SqlitePropertyBuilderExtensions).GetRuntimeMethod(
            nameof(SqlitePropertyBuilderExtensions.UseAutoincrement), [typeof(PropertyBuilder)])!;

    private static readonly MethodInfo ComplexTypePropertyUseAutoincrementMethodInfo
        = typeof(SqliteComplexTypePropertyBuilderExtensions).GetRuntimeMethod(
            nameof(SqliteComplexTypePropertyBuilderExtensions.UseAutoincrement), [typeof(ComplexTypePropertyBuilder)])!;

    #endregion MethodInfos

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SqliteAnnotationCodeGenerator(AnnotationCodeGeneratorDependencies dependencies)
        : base(dependencies)
    {
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override IReadOnlyList<MethodCallCodeFragment> GenerateFluentApiCalls(
        IProperty property,
        IDictionary<string, IAnnotation> annotations)
    {
        var fragments = new List<MethodCallCodeFragment>(base.GenerateFluentApiCalls(property, annotations));

        if (TryGetAndRemove<SqliteValueGenerationStrategy>(annotations, SqliteAnnotationNames.ValueGenerationStrategy, out var strategy)
            && strategy == SqliteValueGenerationStrategy.Autoincrement)
        {
            var methodInfo = property.DeclaringType is IComplexType
                ? ComplexTypePropertyUseAutoincrementMethodInfo
                : PropertyUseAutoincrementMethodInfo;
            fragments.Add(new MethodCallCodeFragment(methodInfo));
        }

        return fragments;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override bool IsHandledByConvention(IProperty property, IAnnotation annotation)
    {
        if (annotation.Name == SqliteAnnotationNames.ValueGenerationStrategy)
        {
            return (SqliteValueGenerationStrategy)annotation.Value! == SqlitePropertyExtensions.GetDefaultValueGenerationStrategy(property);
        }

        return base.IsHandledByConvention(property, annotation);
    }

    private static bool TryGetAndRemove<T>(
        IDictionary<string, IAnnotation> annotations,
        string annotationName,
        [NotNullWhen(true)] out T? annotationValue)
    {
        if (annotations.TryGetValue(annotationName, out var annotation)
            && annotation.Value != null)
        {
            annotations.Remove(annotationName);
            annotationValue = (T)annotation.Value;
            return true;
        }

        annotationValue = default;
        return false;
    }
}