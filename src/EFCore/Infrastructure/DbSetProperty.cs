// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;

namespace Microsoft.EntityFrameworkCore.Infrastructure;

/// <summary>
///     A struct representing facets of <see cref="DbSet{TEntity}" /> property defined on DbContext derived type.
/// </summary>
public readonly struct DbSetProperty
{
    /// <summary>
    ///     Initializes new <see cref="DbSetProperty" /> with given values.
    /// </summary>
    /// <param name="name">The name of DbSet.</param>
    /// <param name="type">The entity clr type of DbSet.</param>
    /// <param name="setter">The setter for DbSet property.</param>
    public DbSetProperty(
        string name,
        [DynamicallyAccessedMembers(IEntityType.DynamicallyAccessedMemberTypes)] Type type,
        IClrPropertySetter? setter)
    {
        Name = name;
        Type = type;
        Setter = setter;
    }

    /// <summary>
    ///     Gets the name of this DbSet property.
    /// </summary>
    public string Name { get; }

    /// <summary>
    ///     Gets the clr type of entity type this DbSet property represent.
    /// </summary>
    [DynamicallyAccessedMembers(IEntityType.DynamicallyAccessedMemberTypes)]
    public Type Type { get; }

    /// <summary>
    ///     The property setter for this DbSet property.
    /// </summary>
    public IClrPropertySetter? Setter { get; }
}
