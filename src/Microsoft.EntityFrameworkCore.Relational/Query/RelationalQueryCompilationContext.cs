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

namespace Microsoft.EntityFrameworkCore.Query
{
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
            [NotNull] IModel model,
            [NotNull] ISensitiveDataLogger logger,
            [NotNull] IEntityQueryModelVisitorFactory entityQueryModelVisitorFactory,
            [NotNull] IRequiresMaterializationExpressionVisitorFactory requiresMaterializationExpressionVisitorFactory,
            [NotNull] ILinqOperatorProvider linqOperatorProvider,
            [NotNull] IQueryMethodProvider queryMethodProvider,
            [NotNull] Type contextType,
            bool trackQueryResults)
            : base(
                Check.NotNull(model, nameof(model)),
                Check.NotNull(logger, nameof(logger)),
                Check.NotNull(entityQueryModelVisitorFactory, nameof(entityQueryModelVisitorFactory)),
                Check.NotNull(requiresMaterializationExpressionVisitorFactory, nameof(requiresMaterializationExpressionVisitorFactory)),
                Check.NotNull(linqOperatorProvider, nameof(linqOperatorProvider)),
                Check.NotNull(contextType, nameof(contextType)),
                trackQueryResults)
        {
            Check.NotNull(queryMethodProvider, nameof(queryMethodProvider));

            QueryMethodProvider = queryMethodProvider;
        }

        public virtual IQueryMethodProvider QueryMethodProvider { get; }

        public override EntityQueryModelVisitor CreateQueryModelVisitor()
        {
            var relationalQueryModelVisitor
                = (RelationalQueryModelVisitor)base.CreateQueryModelVisitor();

            _relationalQueryModelVisitors.Add(relationalQueryModelVisitor);

            return relationalQueryModelVisitor;
        }

        public virtual bool IsLateralJoinSupported => false;

        public override EntityQueryModelVisitor CreateQueryModelVisitor(EntityQueryModelVisitor parentEntityQueryModelVisitor)
        {
            var relationalQueryModelVisitor
                = (RelationalQueryModelVisitor)base.CreateQueryModelVisitor(parentEntityQueryModelVisitor);

            _relationalQueryModelVisitors.Add(relationalQueryModelVisitor);

            return relationalQueryModelVisitor;
        }

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

        public virtual string CreateUniqueTableAlias()
            => CreateUniqueTableAlias(SystemAliasPrefix);

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
    }
}
