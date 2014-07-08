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
        private readonly IQueryMethodProvider _queryMethodProvider;

        public RelationalQueryCompilationContext(
            [NotNull] IModel model,
            [NotNull] ILinqOperatorProvider linqOperatorProvider,
            [NotNull] IResultOperatorHandler resultOperatorHandler,
            [NotNull] IQueryMethodProvider queryMethodProvider)
            : base(
                Check.NotNull(model, "model"),
                Check.NotNull(linqOperatorProvider, "linqOperatorProvider"),
                Check.NotNull(resultOperatorHandler, "resultOperatorHandler"))
        {
            Check.NotNull(queryMethodProvider, "queryMethodProvider");

            _queryMethodProvider = queryMethodProvider;
        }

        public override EntityQueryModelVisitor CreateQueryModelVisitor(
            EntityQueryModelVisitor parentEntityQueryModelVisitor)
        {
            return new RelationalQueryModelVisitor(
                this, (RelationalQueryModelVisitor)parentEntityQueryModelVisitor);
        }

        public virtual IQueryMethodProvider QueryMethodProvider
        {
            get { return _queryMethodProvider; }
        }

        public virtual ISqlQueryGenerator CreateSqlQueryGenerator()
        {
            return new DefaultSqlQueryGenerator();
        }
    }
}
