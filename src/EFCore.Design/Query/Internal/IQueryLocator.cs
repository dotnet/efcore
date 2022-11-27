// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.CodeAnalysis;

namespace Microsoft.EntityFrameworkCore.Query.Internal;

/// <summary>
///     Statically analyzes user code and locates EF LINQ queries within it, by identifying well-known terminating operators
///     (e.g. <c>ToList</c>, <c>Single</c>).
/// </summary>
/// <remarks>
///     After a <see cref="Compilation" /> is loaded via <see cref="LoadCompilation" />, <see cref="LocateQueries" /> is called repeatedly
///     for all syntax trees in the compilation.
/// </remarks>
/// <remarks>
///     <para>
///         In some cases, the provided <see cref="SyntaxTree" /> must be rewritten (since async invocations such as <c>SingleAsync</c>
///         inject a sync <c>Single</c> node). As a result, <see cref="LocateQueries" /> returns a possibly-rewritten
///         <see cref="SyntaxTree" />.
///     </para>
///     <para>
///         This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///         the same compatibility standards as public APIs. It may be changed or removed without notice in
///         any release. You should only use it directly in your code with extreme caution and knowing that
///         doing so can result in application failures when updating to a new Entity Framework Core release.
///     </para>
/// </remarks>
public interface IQueryLocator
{
    /// <summary>
    ///     The <see cref="SyntaxAnnotation.Kind" /> of the the <see cref="SyntaxAnnotation" /> added to nodes which represent EF query
    ///     LINQ candidates.
    /// </summary>
    /// <remarks>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </remarks>
    const string EfQueryCandidateAnnotationKind = "EfQueryCandidate";

    /// <summary>
    ///     A list of syntax trees in which EF LINQ query candidates were located.
    /// </summary>
    /// <remarks>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </remarks>
    IReadOnlyList<SyntaxTree> SyntaxTreesWithQueryCandidates { get; }

    /// <summary>
    ///     Loads a new <see cref="Compilation" />, representing a user project in which to locate queries.
    /// </summary>
    /// <param name="compilation">A <see cref="Compilation" /> representing a user project.</param>
    /// <remarks>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </remarks>
    void LoadCompilation(Compilation compilation);

    /// <summary>
    ///     Locates EF LINQ queries within the given <see cref="SyntaxTree" />, which represents user code.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         In some cases, the provided <see cref="SyntaxTree" /> must be rewritten (since async invocations such as <c>SingleAsync</c>
    ///         inject a sync <c>Single</c> node). As a result, this method returns a possibly-rewritten <see cref="SyntaxTree" />.
    ///     </para>
    ///     <para>
    ///         This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///         the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///         any release. You should only use it directly in your code with extreme caution and knowing that
    ///         doing so can result in application failures when updating to a new Entity Framework Core release.
    ///     </para>
    /// </remarks>
    /// <param name="syntaxTree">A <see cref="SyntaxTree" /> in which to locate EF LINQ queries.</param>
    /// <returns>A possibly rewritten <see cref="SyntaxTree" />.</returns>
    SyntaxTree LocateQueries(SyntaxTree syntaxTree);
}
