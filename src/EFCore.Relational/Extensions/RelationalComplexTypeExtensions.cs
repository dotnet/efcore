// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable once CheckNamespace

namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     Complex type extension methods for relational database metadata.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
/// </remarks>
public static class RelationalComplexTypeExtensions
{
    /// <summary>
    ///     Gets the container column name to which the complex type is mapped.
    /// </summary>
    /// <param name="complexType">The complex type to get the container column name for.</param>
    /// <returns>The container column name to which the complex type is mapped.</returns>
    public static string? GetContainerColumnName(this IReadOnlyComplexType complexType)
        => complexType.FindAnnotation(RelationalAnnotationNames.ContainerColumnName)?.Value is string columnName
            ? columnName
            : complexType.ComplexProperty.DeclaringType.GetContainerColumnName();
}
