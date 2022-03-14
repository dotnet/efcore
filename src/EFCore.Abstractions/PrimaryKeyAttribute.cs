// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     Specifies a primary key for the entity type mapped to this CLR type.
/// </summary>
/// <remarks>
///     <para>
///         This attribute can be used for both keys made up of a
///         single property, and for composite keys made up of multiple properties. <see cref="KeyAttribute" />
///         can be used instead for single-property keys, in which case the behavior is identical. If both attributes are used, then
///         this attribute takes precedence.
///     </para>
///     <para>
///         See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and
///         examples.
///     </para>
/// </remarks>
[AttributeUsage(AttributeTargets.Class)]
public sealed class PrimaryKeyAttribute : Attribute
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="PrimaryKeyAttribute" /> class.
    /// </summary>
    /// <param name="propertyName">The first (or only) property in the primary key.</param>
    /// <param name="additionalPropertyNames">The additional properties which constitute the primary key, if any, in order.</param>
    public PrimaryKeyAttribute(string propertyName, params string[] additionalPropertyNames)
    {
        Check.NotEmpty(propertyName, nameof(propertyName));
        Check.HasNoEmptyElements(additionalPropertyNames, nameof(additionalPropertyNames));

        PropertyNames = new List<string> { propertyName };
        ((List<string>)PropertyNames).AddRange(additionalPropertyNames);
    }

    /// <summary>
    ///     The properties which constitute the primary key, in order.
    /// </summary>
    public IReadOnlyList<string> PropertyNames { get; }
}
