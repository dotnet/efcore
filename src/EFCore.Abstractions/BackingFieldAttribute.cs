// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     Names the backing field associated with this property or navigation property.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
/// </remarks>
[AttributeUsage(AttributeTargets.Property)]
public sealed class BackingFieldAttribute : Attribute
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="BackingFieldAttribute" /> class.
    /// </summary>
    /// <param name="name">The name of the backing field.</param>
    public BackingFieldAttribute(string name)
    {
        Check.NotEmpty(name, nameof(name));

        Name = name;
    }

    /// <summary>
    ///     The name of the backing field.
    /// </summary>
    public string Name { get; }
}
