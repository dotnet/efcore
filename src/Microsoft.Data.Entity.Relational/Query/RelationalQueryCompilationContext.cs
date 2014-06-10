// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Query;
using Microsoft.Data.Entity.Relational.Query.Sql;
using Microsoft.Data.Entity.Relational.Utilities;

namespace Microsoft.Data.Entity.Relational.Query
{
    public class RelationalQueryCompilationContext : QueryCompilationContext
    {
        private readonly IEnumerableMethodProvider _enumerableMethodProvider;
        private readonly ISqlGeneratingExpressionTreeVisitorFactory _sqlGeneratingExpressionTreeVisitor;

        public RelationalQueryCompilationContext(
            [NotNull] IModel model,
            [NotNull] ILinqOperatorProvider linqOperatorProvider,
            [NotNull] IResultOperatorHandler resultOperatorHandler,
            [NotNull] IEnumerableMethodProvider enumerableMethodProvider)
            : base(
                Check.NotNull(model, "model"),
                Check.NotNull(linqOperatorProvider, "linqOperatorProvider"),
                Check.NotNull(resultOperatorHandler, "resultOperatorHandler"))
        {
            Check.NotNull(enumerableMethodProvider, "enumerableMethodProvider");

            _enumerableMethodProvider = enumerableMethodProvider;
            _sqlGeneratingExpressionTreeVisitor = new SqlGeneratingExpressionTreeVisitorFactory();
        }

        public override EntityQueryModelVisitor CreateVisitor()
        {
            return new RelationalQueryModelVisitor(this);
        }

        public virtual IEnumerableMethodProvider EnumerableMethodProvider
        {
            get { return _enumerableMethodProvider; }
        }

        public virtual ISqlGeneratingExpressionTreeVisitorFactory SqlGeneratingExpressionTreeVisitor
        {
            get { return _sqlGeneratingExpressionTreeVisitor; }
        }
    }
}
