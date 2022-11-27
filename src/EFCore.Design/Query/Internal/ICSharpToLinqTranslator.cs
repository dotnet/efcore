// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.CodeAnalysis;

namespace Microsoft.EntityFrameworkCore.Query.Internal;

/// <summary>
///     Translates a Roslyn syntax tree into a LINQ expression tree.
/// </summary>
/// <remarks>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </remarks>
public interface ICSharpToLinqTranslator
{
    /// <summary>
    ///     Loads the given <see cref="Compilation" /> and prepares to translate queries using the given <see cref="DbContext" />.
    /// </summary>
    /// <param name="compilation">A <see cref="Compilation" /> containing the syntax nodes to be translated.</param>
    /// <param name="userDbContext">An instance of the user's <see cref="DbContext" />.</param>
    /// <remarks>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </remarks>
    void Load(Compilation compilation, DbContext userDbContext);

    /// <summary>
    ///     Translates a Roslyn syntax tree into a LINQ expression tree.
    /// </summary>
    /// <param name="node">The Roslyn syntax node to be translated.</param>
    /// <param name="semanticModel">
    /// The <see cref="SemanticModel" /> for the Roslyn <see cref="SyntaxTree" /> of which <paramref name="node" /> is a part.
    /// </param>
    /// <returns>A LINQ expression tree translated from the provided <paramref name="node"/>.</returns>
    /// <remarks>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </remarks>
    Expression Translate(SyntaxNode node, SemanticModel semanticModel);
}
