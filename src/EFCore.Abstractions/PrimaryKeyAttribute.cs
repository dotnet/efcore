// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     Specifies a primary key for the entity type mapped to this CLR type. This attribute can be used for both keys made up of a
///     single property, and for composite keys made up of multiple properties. `System.ComponentModel.DataAnnotations.KeyAttribute`
///     can be used instead for single-property keys, in which case the behavior is identical. If both attributes are used, then
///     this attribute takes precedence.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
/// </remarks>
[AttributeUsage(AttributeTargets.Class)]
public sealed class PrimaryKeyAttribute : Attribute
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="IndexAttribute" /> class.
    /// </summary>
    /// <param name="propertyNames">The properties which constitute the index, in order (there must be at least one).</param>
    public PrimaryKeyAttribute(params string[] propertyNames)
    {
        Check.NotEmpty(propertyNames, nameof(propertyNames));
        Check.HasNoEmptyElements(propertyNames, nameof(propertyNames));

        PropertyNames = propertyNames.ToList();
    }

    /// <summary>
    ///     The properties which constitute the index, in order.
    /// </summary>
    public IReadOnlyList<string> PropertyNames { get; }
}
