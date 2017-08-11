// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Utilities;
using Remotion.Linq.Clauses;

namespace Microsoft.EntityFrameworkCore.Query
{
    /// <summary>
    ///     A relational query compilation context. The primary data structure representing the state/components
    ///     used during relational query compilation.
    /// </summary>
    public class RelationalQueryCompilationContext : QueryCompilationContext
    {
        private readonly List<RelationalQueryModelVisitor> _relationalQueryModelVisitors
            = new List<RelationalQueryModelVisitor>();

        private const string SystemAliasPrefix = "t";
        private readonly ISet<string> _tableAliasSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public RelationalQueryCompilationContext(
            [NotNull] QueryCompilationContextDependencies dependencies,
            [NotNull] ILinqOperatorProvider linqOperatorProvider,
            [NotNull] IQueryMethodProvider queryMethodProvider,
            bool trackQueryResults)
            : base(dependencies, linqOperatorProvider, trackQueryResults)
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
        ///     Creates a query model visitor.
        /// </summary>
        /// <returns>
        ///     The new query model visitor.
        /// </returns>
        public override EntityQueryModelVisitor CreateQueryModelVisitor()
        {
            var relationalQueryModelVisitor
                = (RelationalQueryModelVisitor)base.CreateQueryModelVisitor();

            _relationalQueryModelVisitors.Add(relationalQueryModelVisitor);

            return relationalQueryModelVisitor;
        }

        /// <summary>
        ///     True if the current provider supports SQL LATERAL JOIN.
        /// </summary>
        public virtual bool IsLateralJoinSupported => false;

        /// <summary>
        ///     Max length of the table alias supported by provider.
        /// </summary>
        public virtual int MaxTableAliasLength => 128;

        /// <summary>
        ///     Creates query model visitor.
        /// </summary>
        /// <param name="parentEntityQueryModelVisitor"> The parent entity query model visitor. </param>
        /// <returns>
        ///     The new query model visitor.
        /// </returns>
        public override EntityQueryModelVisitor CreateQueryModelVisitor(EntityQueryModelVisitor parentEntityQueryModelVisitor)
        {
            var relationalQueryModelVisitor
                = (RelationalQueryModelVisitor)base.CreateQueryModelVisitor(parentEntityQueryModelVisitor);

            _relationalQueryModelVisitors.Add(relationalQueryModelVisitor);

            return relationalQueryModelVisitor;
        }

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
                (from v in _relationalQueryModelVisitors
                 let selectExpression = v.TryGetQuery(querySource)
                 where selectExpression != null
                 select selectExpression)
                .First();
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

            while (currentAlias.Length > MaxTableAliasLength - 3)
            {
                var index = currentAlias.IndexOf(".", StringComparison.OrdinalIgnoreCase);
                currentAlias = index > 0 ? currentAlias.Substring(index + 1) : currentAlias.Substring(0, MaxTableAliasLength - 3);
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
    }
}
