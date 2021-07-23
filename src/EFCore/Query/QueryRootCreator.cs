﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore.Query
{
    /// <inheritdoc/>
    public class QueryRootCreator : IQueryRootCreator
    {
        /// <summary>
        ///     Creates a new instance of the <see cref="QueryRootCreator" /> class.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this class. </param>
        public QueryRootCreator(QueryRootCreatorDependencies dependencies)
        {
            Dependencies = dependencies;
        }

        /// <summary>
        ///     Parameter object containing service dependencies.
        /// </summary>
        protected virtual QueryRootCreatorDependencies Dependencies { get; }

        /// <inheritdoc/>
        public virtual QueryRootExpression CreateQueryRoot(IEntityType entityType, QueryRootExpression? source)
            => source?.QueryProvider != null
                ? new QueryRootExpression(source.QueryProvider, entityType)
                : new QueryRootExpression(entityType);

        /// <inheritdoc/>
        public virtual bool AreCompatible(QueryRootExpression? first, QueryRootExpression? second)
        {
            if (first is null && second is null)
            {
                return true;
            }

            if (first is not null && second is not null)
            {
                return first.EntityType.GetRootType() == second.EntityType.GetRootType();
            }

            return false;
        }
    }
}
