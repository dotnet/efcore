// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Query.Sql.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query.Sql
{
    /// <summary>
    ///     A base class for query SQL generators.
    /// </summary>
    public abstract class QuerySqlGeneratorFactoryBase : IQuerySqlGeneratorFactory
    {
        /// <summary>
        ///     Initializes a new instance of the this class.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this service. </param>
        protected QuerySqlGeneratorFactoryBase([NotNull] QuerySqlGeneratorDependencies dependencies)
        {
            Check.NotNull(dependencies, nameof(dependencies));

            Dependencies = dependencies;
        }

        /// <summary>
        ///     Parameter object containing service dependencies.
        /// </summary>
        protected virtual QuerySqlGeneratorDependencies Dependencies { get; }

        /// <summary>
        ///     Creates a default query SQL generator.
        /// </summary>
        /// <param name="selectExpression"> The select expression. </param>
        /// <returns>
        ///     The new default query SQL generator.
        /// </returns>
        public abstract IQuerySqlGenerator CreateDefault(SelectExpression selectExpression);

        /// <summary>
        ///     Creates a query SQL generator for a FromSql query.
        /// </summary>
        /// <param name="selectExpression"> The select expression. </param>
        /// <param name="sql"> The SQL. </param>
        /// <param name="arguments"> The arguments. </param>
        /// <returns>
        ///     The query SQL generator.
        /// </returns>
        public virtual IQuerySqlGenerator CreateFromSql(
            SelectExpression selectExpression,
            string sql,
            Expression arguments)
            => new FromSqlNonComposedQuerySqlGenerator(
                Dependencies,
                Check.NotNull(selectExpression, nameof(selectExpression)),
                Check.NotEmpty(sql, nameof(sql)),
                Check.NotNull(arguments, nameof(arguments)));
    }
}
