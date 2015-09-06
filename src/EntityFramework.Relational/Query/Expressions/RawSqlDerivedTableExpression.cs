// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Query.Sql;
using Microsoft.Data.Entity.Utilities;
using Remotion.Linq.Clauses;

namespace Microsoft.Data.Entity.Query.Expressions
{
    public class RawSqlDerivedTableExpression : TableExpressionBase
    {
        public RawSqlDerivedTableExpression(
            [NotNull] string sql,
            [NotNull] object[] parameters,
            [NotNull] string alias,
            [NotNull] IQuerySource querySource)
            : base(
                Check.NotNull(querySource, nameof(querySource)),
                Check.NotEmpty(alias, nameof(alias)))
        {
            Check.NotEmpty(sql, nameof(sql));
            Check.NotNull(parameters, nameof(parameters));

            Sql = sql;
            Parameters = parameters;
        }

        public virtual string Sql { get; }

        public virtual object[] Parameters { get; }

        protected override Expression Accept(ExpressionVisitor visitor)
        {
            Check.NotNull(visitor, nameof(visitor));

            var specificVisitor = visitor as ISqlExpressionVisitor;

            return specificVisitor != null
                ? specificVisitor.VisitRawSqlDerivedTable(this)
                : base.Accept(visitor);
        }

        public override string ToString() => Sql + " " + Alias;
    }
}
