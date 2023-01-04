// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Internal;

/// <summary>
///     Given a SQL expression tree, generates a LINQ expression tree that instantiates a copy of that tree. Can be used to render a C#
///     representation of the SQL tree.
/// </summary>
/// <remarks>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </remarks>
public interface ISqlTreeQuoter
{
    /// <summary>
    ///     Returns a LINQ expression tree that instantiates a faithful copy of <paramref name="expression" />.
    /// </summary>
    /// <param name="expression">The SQL tree to be quoted.</param>
    /// <param name="rootSelectVariableName">The variable name to give to the top-most select expression.</param>
    /// <param name="variableNames">A set of variable names already defined in the context, for uniquification.</param>
    /// <returns>A LINQ expression tree that instantiates a faithful copy of <paramref name="expression" />.</returns>
    /// <remarks>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </remarks>
    public BlockExpression Quote(Expression expression, string rootSelectVariableName, HashSet<string> variableNames);
}
