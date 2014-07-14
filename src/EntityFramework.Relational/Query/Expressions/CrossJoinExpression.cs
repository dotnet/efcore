// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Relational.Query.Sql;
using Microsoft.Data.Entity.Relational.Utilities;
using Remotion.Linq.Clauses;
using Remotion.Linq.Parsing;

namespace Microsoft.Data.Entity.Relational.Query.Expressions
{
    public class CrossJoinExpression : TableExpression
    {
        public CrossJoinExpression(
            [NotNull] string table,
            [CanBeNull] string schema,
            [NotNull] string alias,
            [NotNull] IQuerySource querySource)
            : base(
                Check.NotEmpty(table, "table"),
                schema,
                Check.NotEmpty(alias, "alias"),
                Check.NotNull(querySource, "querySource"))
        {
        }

        public override Expression Accept(ExpressionTreeVisitor visitor)
        {
            Check.NotNull(visitor, "visitor");

            var specificVisitor = visitor as ISqlExpressionVisitor;

            if (specificVisitor != null)
            {
                return specificVisitor.VisitCrossJoinExpression(this);
            }

            return base.Accept(visitor);
        }

        public override string ToString()
        {
            return "CROSS JOIN " + base.ToString();
        }
    }
}
