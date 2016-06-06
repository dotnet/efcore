// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Query.Sql.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query.Sql
{
    /// <summary>
    ///     A base class for query SQL generators.
    /// </summary>
    public abstract class QuerySqlGeneratorFactoryBase : IQuerySqlGeneratorFactory
    {
        /// <summary>
        ///     Specialised constructor for use only by derived class.
        /// </summary>
        /// <param name="commandBuilderFactory"> The command builder factory. </param>
        /// <param name="sqlGenerationHelper"> The SQL generation helper. </param>
        /// <param name="parameterNameGeneratorFactory"> The parameter name generator factory. </param>
        /// <param name="relationalTypeMapper"> The relational type mapper. </param>
        protected QuerySqlGeneratorFactoryBase(
            [NotNull] IRelationalCommandBuilderFactory commandBuilderFactory,
            [NotNull] ISqlGenerationHelper sqlGenerationHelper,
            [NotNull] IParameterNameGeneratorFactory parameterNameGeneratorFactory,
            [NotNull] IRelationalTypeMapper relationalTypeMapper)
        {
            Check.NotNull(commandBuilderFactory, nameof(commandBuilderFactory));
            Check.NotNull(sqlGenerationHelper, nameof(sqlGenerationHelper));
            Check.NotNull(parameterNameGeneratorFactory, nameof(parameterNameGeneratorFactory));
            Check.NotNull(relationalTypeMapper, nameof(relationalTypeMapper));

            CommandBuilderFactory = commandBuilderFactory;
            SqlGenerationHelper = sqlGenerationHelper;
            ParameterNameGeneratorFactory = parameterNameGeneratorFactory;
            RelationalTypeMapper = relationalTypeMapper;
        }

        /// <summary>
        ///     Gets the command builder factory.
        /// </summary>
        /// <value>
        ///     The command builder factory.
        /// </value>
        protected virtual IRelationalCommandBuilderFactory CommandBuilderFactory { get; }

        /// <summary>
        ///     Gets the SQL generation helper.
        /// </summary>
        /// <value>
        ///     The SQL generation helper.
        /// </value>
        protected virtual ISqlGenerationHelper SqlGenerationHelper { get; }

        /// <summary>
        ///     Gets the parameter name generator factory.
        /// </summary>
        /// <value>
        ///     The parameter name generator factory.
        /// </value>
        protected virtual IParameterNameGeneratorFactory ParameterNameGeneratorFactory { get; }

        /// <summary>
        ///     Gets the relational type mapper.
        /// </summary>
        /// <value>
        ///     The relational type mapper.
        /// </value>
        protected virtual IRelationalTypeMapper RelationalTypeMapper { get; }

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
                CommandBuilderFactory,
                SqlGenerationHelper,
                ParameterNameGeneratorFactory,
                RelationalTypeMapper,
                Check.NotNull(selectExpression, nameof(selectExpression)),
                Check.NotEmpty(sql, nameof(sql)),
                Check.NotNull(arguments, nameof(arguments)));
    }
}
