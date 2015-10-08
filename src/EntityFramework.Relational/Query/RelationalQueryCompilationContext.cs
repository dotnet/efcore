// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Query.Expressions;
using Microsoft.Data.Entity.Query.ExpressionVisitors;
using Microsoft.Data.Entity.Utilities;
using Remotion.Linq.Clauses;
using System.Diagnostics.Tracing;

namespace Microsoft.Data.Entity.Query
{
    public class RelationalQueryCompilationContext : QueryCompilationContext
    {
        private readonly List<RelationalQueryModelVisitor> _relationalQueryModelVisitors
            = new List<RelationalQueryModelVisitor>();

#pragma warning disable 0618
        public RelationalQueryCompilationContext(
            [NotNull] ISensitiveDataLogger logger,
            [NotNull] IEntityQueryModelVisitorFactory entityQueryModelVisitorFactory,
            [NotNull] IRequiresMaterializationExpressionVisitorFactory requiresMaterializationExpressionVisitorFactory,
            [NotNull] ILinqOperatorProvider linqOperatorProvider,
            [NotNull] IQueryMethodProvider queryMethodProvider,
            [NotNull] Type contextType,
            [NotNull] TelemetrySource telemetrySource)
            : base(
                Check.NotNull(logger, nameof(logger)),
                Check.NotNull(entityQueryModelVisitorFactory, nameof(entityQueryModelVisitorFactory)),
                Check.NotNull(requiresMaterializationExpressionVisitorFactory, nameof(requiresMaterializationExpressionVisitorFactory)),
                Check.NotNull(linqOperatorProvider, nameof(linqOperatorProvider)),
                Check.NotNull(contextType, nameof(contextType)))
        {
            Check.NotNull(queryMethodProvider, nameof(queryMethodProvider));
            Check.NotNull(telemetrySource, nameof(telemetrySource));

            QueryMethodProvider = queryMethodProvider;
            TelemetrySource = telemetrySource;
        }

        public virtual IQueryMethodProvider QueryMethodProvider { get; }
        public virtual TelemetrySource TelemetrySource { get; }
#pragma warning restore 0618

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
    }
}
