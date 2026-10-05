// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Migrations.Design;

/// <summary>
///     The parameter object that flows context (the output builder and the set of identifiers
///     currently in scope) through a <see cref="CSharpSnapshotGenerator" /> call tree.
/// </summary>
public sealed record CSharpSnapshotGeneratorParameters
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public CSharpSnapshotGeneratorParameters(
        IndentedStringBuilder stringBuilder,
        ICollection<string> scope)
    {
        StringBuilder = stringBuilder;
        Scope = scope;
    }

    /// <summary>
    ///     The builder code is added to.
    /// </summary>
    public IndentedStringBuilder StringBuilder { get; init; }

    /// <summary>
    ///     The set of identifiers currently in scope. Generators that introduce new locals
    ///     (e.g. <c>var index = b.HasIndex(...);</c>) should pick names from
    ///     <see cref="ICSharpHelper.Identifier(string, ICollection{string}?, bool?)" /> with this scope
    ///     so that the chosen name doesn't collide with another identifier in the same lambda body.
    /// </summary>
    public ICollection<string> Scope { get; init; }
}
