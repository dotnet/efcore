// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics.Tracing;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Query.ExpressionVisitors;
using Microsoft.Data.Entity.Query.Internal;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Query
{
    public class RelationalQueryCompilationContextFactory : QueryCompilationContextFactory
    {
        public RelationalQueryCompilationContextFactory(
            [NotNull] ISensitiveDataLogger<RelationalQueryCompilationContextFactory> logger,
            [NotNull] IEntityQueryModelVisitorFactory entityQueryModelVisitorFactory,
            [NotNull] IRequiresMaterializationExpressionVisitorFactory requiresMaterializationExpressionVisitorFactory,
            [NotNull] DbContext context,
            [NotNull] TelemetrySource telemetrySource)
            : base(
                Check.NotNull(logger, nameof(logger)),
                Check.NotNull(entityQueryModelVisitorFactory, nameof(entityQueryModelVisitorFactory)),
                Check.NotNull(requiresMaterializationExpressionVisitorFactory, nameof(requiresMaterializationExpressionVisitorFactory)),
                Check.NotNull(context, nameof(context)))
        {
            TelemetrySource = telemetrySource;
        }

        protected virtual TelemetrySource TelemetrySource { get; }

        public override QueryCompilationContext Create(bool async)
            => async
                ? new RelationalQueryCompilationContext(
                    (ISensitiveDataLogger)Logger,
                    EntityQueryModelVisitorFactory,
                    RequiresMaterializationExpressionVisitorFactory,
                    new AsyncLinqOperatorProvider(),
                    new AsyncQueryMethodProvider(),
                    ContextType,
                    TelemetrySource)
                : new RelationalQueryCompilationContext(
                    (ISensitiveDataLogger)Logger,
                    EntityQueryModelVisitorFactory,
                    RequiresMaterializationExpressionVisitorFactory,
                    new LinqOperatorProvider(),
                    new QueryMethodProvider(),
                    ContextType,
                    TelemetrySource);
    }
}
