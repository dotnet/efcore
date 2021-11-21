// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Migrations;

/// <summary>
///     Indicates that a class is a <see cref="Migration" /> and provides its identifier.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-migrations">Database migrations</see> for more information and examples.
/// </remarks>
[AttributeUsage(AttributeTargets.Class)]
public sealed class MigrationAttribute : Attribute
{
    /// <summary>
    ///     Creates a new instance of this attribute.
    /// </summary>
    /// <param name="id">The migration identifier.</param>
    public MigrationAttribute(string id)
    {
        Check.NotEmpty(id, nameof(id));

        Id = id;
    }

    /// <summary>
    ///     The migration identifier.
    /// </summary>
    public string Id { get; }
}
