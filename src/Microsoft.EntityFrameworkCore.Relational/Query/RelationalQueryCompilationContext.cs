// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Utilities;
using Remotion.Linq.Clauses;
using Remotion.Linq;

namespace Microsoft.EntityFrameworkCore.Query
{
    /// <summary>
    ///     A relational query compilation context. The primary data structure representing the state/components
    ///     used during relational query compilation.
    /// </summary>
    public class RelationalQueryCompilationContext : QueryCompilationContext
    {
        private const string SystemAliasPrefix = "t";
        private readonly ISet<string> _tableAliasSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public RelationalQueryCompilationContext(
            [NotNull] IModel model,
            [NotNull] ISensitiveDataLogger logger,
            [NotNull] IEntityQueryModelVisitorFactory entityQueryModelVisitorFactory,
            [NotNull] IRequiresMaterializationExpressionVisitorFactory requiresMaterializationExpressionVisitorFactory,
            [NotNull] ILinqOperatorProvider linqOperatorProvider,
            [NotNull] IQueryMethodProvider queryMethodProvider,
            [NotNull] Type contextType,
            bool trackQueryResults)
            : base(
                model,
                logger,
                entityQueryModelVisitorFactory,
                requiresMaterializationExpressionVisitorFactory,
                linqOperatorProvider,
                contextType,
                trackQueryResults)
        {
            Check.NotNull(queryMethodProvider, nameof(queryMethodProvider));

            QueryMethodProvider = queryMethodProvider;
            ParentQueryReferenceParameters = new List<string>();
        }

        /// <summary>
        ///     Gets the query method provider.
        /// </summary>
        /// <value>
        ///     The query method provider.
        /// </value>
        public virtual IQueryMethodProvider QueryMethodProvider { get; }

        /// <summary>
        ///     Gets the list of parameter names that represent reference to a parent query.
        /// </summary>
        /// <value>
        ///     The list of parameter names that represent reference to a parent query.
        /// </value>
        public virtual IList<string> ParentQueryReferenceParameters { get; }

        /// <summary>
        ///     True if the current provider supports SQL LATERAL joins.
        /// </summary>
        public virtual bool IsLateralJoinSupported => false;

        /// <summary>
        ///     Creates query model visitor.
        /// </summary>
        /// <param name="queryModel"> The query model to create the query model visitor for. </param>
        /// <returns>
        ///     The new query model visitor.
        /// </returns>
        public virtual new RelationalQueryModelVisitor CreateQueryModelVisitor(
            [NotNull] QueryModel queryModel)
            => (RelationalQueryModelVisitor)base.CreateQueryModelVisitor(queryModel);

        /// <summary>
        ///     Creates query model visitor.
        /// </summary>
        /// <param name="queryModel"> The query model to create the query model visitor for. </param>
        /// <param name="parentEntityQueryModelVisitor"> The parent entity query model visitor. </param>
        /// <returns>
        ///     The new query model visitor.
        /// </returns>
        public virtual new RelationalQueryModelVisitor CreateQueryModelVisitor(
            [NotNull] QueryModel queryModel,
            [CanBeNull] EntityQueryModelVisitor parentEntityQueryModelVisitor)
            => (RelationalQueryModelVisitor)base.CreateQueryModelVisitor(queryModel, parentEntityQueryModelVisitor);

        /// <summary>
        /// Gets the query model visitor that was created for a given query model.
        /// </summary>
        /// <param name="queryModel"> The query model to get the query model visitor for. </param>
        /// <returns> The query model visitor. </returns>
        public virtual new RelationalQueryModelVisitor GetQueryModelVisitor(
            [NotNull] QueryModel queryModel)
            => (RelationalQueryModelVisitor)base.GetQueryModelVisitor(queryModel);

        /// <summary>
        ///     Searches for a select expression corresponding to the passed query source.
        /// </summary>
        /// <param name="querySource"> The query source. </param>
        /// <returns>
        ///     The select expression.
        /// </returns>
        public virtual SelectExpression FindSelectExpression([NotNull] IQuerySource querySource)
        {
            Check.NotNull(querySource, nameof(querySource));

            return
                (from v in EntityQueryModelVisitors.Cast<RelationalQueryModelVisitor>()
                 let selectExpression = v.TryGetQuery(querySource)
                 where selectExpression != null
                 select selectExpression)
                    .FirstOrDefault();
        }

        /// <summary>
        ///     Creates a unique table alias.
        /// </summary>
        /// <returns>
        ///     A unique table alias.
        /// </returns>
        public virtual string CreateUniqueTableAlias()
            => CreateUniqueTableAlias(SystemAliasPrefix);

        /// <summary>
        ///     Creates a unique table alias.
        /// </summary>
        /// <param name="currentAlias"> The current alias. </param>
        /// <returns>
        ///     A unique table alias.
        /// </returns>
        public virtual string CreateUniqueTableAlias([NotNull] string currentAlias)
        {
            Check.NotNull(currentAlias, nameof(currentAlias));

            if (currentAlias.Length == 0)
            {
                return currentAlias;
            }

            var counter = 0;
            var uniqueAlias = currentAlias;

            while (_tableAliasSet.Contains(uniqueAlias))
            {
                uniqueAlias = currentAlias + counter++;
            }

            _tableAliasSet.Add(uniqueAlias);

            return uniqueAlias;
        }

        /// <summary>
        ///     Registers an in-use table alias.
        /// </summary>
        /// <param name="alias"> The alias. </param>
        /// <returns>
        ///     A value indicating whether the alias was registered.
        /// </returns>
        public virtual bool RegisterUsedTableAlias([NotNull] string alias)
        {
            Check.NotNull(alias, nameof(alias));

            if (alias.Length == 0)
            {
                return false;
            }

            return _tableAliasSet.Add(alias);
        }
    }
}
