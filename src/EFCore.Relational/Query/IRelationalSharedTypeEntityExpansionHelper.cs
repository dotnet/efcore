// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Query
{
    /// <summary>
    ///     Service which helps with various aspects of shared type entity expansion extensibility for relational providrers.
    /// </summary>
    /// <para>
    ///     The service lifetime is <see cref="ServiceLifetime.Scoped" />. This means that each
    ///     <see cref="DbContext" /> instance will use its own instance of this service.
    ///     The implementation may depend on other services registered with any lifetime.
    ///     The implementation does not need to be thread-safe.
    /// </para>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-providers">Implementation of database providers and extensions</see>
    ///     and <see href="https://aka.ms/efcore-how-queries-work">How EF Core queries work</see> for more information.
    /// </remarks>
    public interface IRelationalSharedTypeEntityExpansionHelper
    {
        /// <summary>
        /// Creates a SelectExpression representing owned type.
        /// </summary>
        public SelectExpression CreateInnerSelectExpression(
            TableExpressionBase sourceTable,
            IEntityType targetEntityType);

        /// <summary>
        /// Returns true if the given table expression matches table metadata, false otherwise.
        /// </summary>
        public bool TableMatchesMetadata(
            TableExpressionBase tableExpression,
            ITableBase tableMetadata);
    }
}
