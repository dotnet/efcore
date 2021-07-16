// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore.Query
{
    /// <summary>
    ///     Service which can create a new <see cref="QueryRootExpression"/> given the entity type and source expression.
    /// </summary>
    public interface IQueryRootCreator
    {
        /// <summary>
        ///     Creates a new <see cref="QueryRootExpression"/>.
        /// </summary>
        /// <param name="entityType">Entity type of the new <see cref="QueryRootExpression"/>.</param>
        /// <param name="source">Source expression.</param>
        QueryRootExpression CreateQueryRoot(IEntityType entityType, QueryRootExpression? source);

        /// <summary>
        ///     Checks whether two query roots are compatible for a set operation to combine them.
        /// </summary>
        /// <param name="first">The first query root.</param>
        /// <param name="second">The second query root.</param>
        bool AreCompatible(QueryRootExpression? first, QueryRootExpression? second);
    }
}
