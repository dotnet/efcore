// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore.Query
{
    /// <summary>
    ///     Service which helps with various aspects of navigation expansion extensibility. 
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-providers">Implementation of database providers and extensions</see>
    ///     and <see href="https://aka.ms/efcore-how-queries-work">How EF Core queries work</see> for more information.
    /// </remarks>
    public interface INavigationExpansionExtensibilityHelper
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
        bool AreQueryRootsCompatible(QueryRootExpression? first, QueryRootExpression? second);
    }
}
