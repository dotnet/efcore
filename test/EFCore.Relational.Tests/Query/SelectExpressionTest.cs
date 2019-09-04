// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class SelectExpressionTest
    {
        [ConditionalFact]
        public void Table_referential_integrity_is_preserved()
        {
            var model = CreateModel();
            var property = model.FindEntityType(typeof(Foo)).FindProperty("Id");
            var table = new TableExpression("SomeTable", null, "t");
            var ordering = new OrderingExpression(new ColumnExpression(property, table, false), true);

            var select = new SelectExpression(
                "s",
                new List<ProjectionExpression>(),
                new List<TableExpressionBase> { table },
                new List<SqlExpression>(),
                new List<OrderingExpression> { ordering });

            var visitor = new TableSwitchingExpressionVisitor();
            var visitedSelect = (SelectExpression)visitor.Visit(select);
            Assert.Same(visitedSelect.Tables[0], ((ColumnExpression)visitedSelect.Orderings[0].Expression).Table);
        }

        private class TableSwitchingExpressionVisitor : ExpressionVisitor
        {
            protected override Expression VisitExtension(Expression node)
            {
                if (node is TableExpression tableExpression)
                {
                    return tableExpression.VisitedExpression ?? new TableExpression(
                               tableExpression.Name + "2", tableExpression.Schema, tableExpression.Alias);
                }
                return base.VisitExtension(node);
            }
        }

        protected IMutableModel CreateModel()
        {
            var builder = RelationalTestHelpers.Instance.CreateConventionBuilder();
            builder.Entity<Foo>();
            builder.FinalizeModel();
            return builder.Model;
        }

        private class Foo
        {
            public int Id { get; set; }
        }
    }
}
