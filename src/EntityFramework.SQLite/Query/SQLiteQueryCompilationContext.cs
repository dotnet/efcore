// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Query;
using Microsoft.Data.Entity.Relational.Query;
using Microsoft.Data.Entity.Relational.Query.Sql;

namespace Microsoft.Data.Entity.SQLite.Query
{
    public class SQLiteQueryCompilationContext : RelationalQueryCompilationContext
    {
        public SQLiteQueryCompilationContext(
            [NotNull] IModel model,
            [NotNull] ILinqOperatorProvider linqOperatorProvider,
            [NotNull] IResultOperatorHandler resultOperatorHandler,
            [NotNull] IQueryMethodProvider queryMethodProvider)
            : base(model, linqOperatorProvider, resultOperatorHandler, queryMethodProvider)
        {
        }

        public override ISqlQueryGenerator CreateSqlQueryGenerator()
        {
            return new SQLiteSqlQueryGenerator();
        }
    }
}
