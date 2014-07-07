// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.Query.Sql;
using Microsoft.Data.Entity.Relational.Utilities;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Parsing;

namespace Microsoft.Data.Entity.Relational.Query.Expressions
{
    public class SelectExpression : ExtensionExpression
    {
        private readonly List<ColumnExpression> _projection = new List<ColumnExpression>();
        private readonly List<TableExpression> _tables = new List<TableExpression>();
        private readonly List<Ordering> _orderBy = new List<Ordering>();

        private int? _limit;

        public SelectExpression()
            : base(typeof(object))
        {
        }

        public virtual IReadOnlyList<TableExpression> Tables
        {
            get { return _tables; }
        }

        public virtual void AddTable([NotNull] TableExpression tableExpression)
        {
            Check.NotNull(tableExpression, "tableExpression");

            _tables.Add(tableExpression);
        }

        public virtual bool HandlesQuerySource([NotNull] IQuerySource querySource)
        {
            Check.NotNull(querySource, "querySource");

            return _tables.Any(tableExpression => tableExpression.QuerySource == querySource);
        }

        public virtual TableExpression FindTableForQuerySource([NotNull] IQuerySource querySource)
        {
            Check.NotNull(querySource, "querySource");

            return _tables.Single(t => t.QuerySource == querySource);
        }

        public virtual bool IsDistinct { get; set; }

        public virtual void AddLimit(int limit)
        {
            _limit = limit;
        }

        public virtual int? Limit
        {
            get { return _limit; }
        }

        public virtual IReadOnlyList<ColumnExpression> Projection
        {
            get { return _projection; }
        }

        public virtual void AddToProjection([NotNull] IProperty property, [NotNull] IQuerySource querySource)
        {
            Check.NotNull(property, "property");
            Check.NotNull(querySource, "querySource");

            if (GetProjectionIndex(property, querySource) == -1)
            {
                _projection.Add(new ColumnExpression(property, FindTableForQuerySource(querySource).Alias));
            }
        }

        public virtual int GetProjectionIndex([NotNull] IProperty property, [NotNull] IQuerySource querySource)
        {
            Check.NotNull(property, "property");
            Check.NotNull(querySource, "querySource");

            var table = FindTableForQuerySource(querySource);

            return _projection.FindIndex(ce => ce.Property == property && ce.Alias == table.Alias);
        }

        public virtual Expression Predicate { get; [param: CanBeNull] set; }

        public virtual void AddToOrderBy(
            [NotNull] IProperty property,
            [NotNull] IQuerySource querySource,
            OrderingDirection orderingDirection)
        {
            Check.NotNull(property, "property");
            Check.NotNull(property, "querySource");

            var columnExpression
                = new ColumnExpression(property, FindTableForQuerySource(querySource).Alias);

            _orderBy.Add(new Ordering(columnExpression, orderingDirection));
        }

        public virtual IReadOnlyList<Ordering> OrderBy
        {
            get { return _orderBy; }
        }

        public virtual void ClearOrderBy()
        {
            _orderBy.Clear();
        }

        public virtual void Merge([NotNull] SelectExpression selectExpression)
        {
            Check.NotNull(selectExpression, "selectExpression");
            Contract.Assert(!selectExpression.OrderBy.Any());

            _tables.InsertRange(0, selectExpression.Tables);
            _projection.InsertRange(0, selectExpression.Projection);
        }

        public override Expression Accept([NotNull] ExpressionTreeVisitor visitor)
        {
            Check.NotNull(visitor, "visitor");

            var specificVisitor = visitor as ISqlExpressionVisitor;

            if (specificVisitor != null)
            {
                return specificVisitor.VisitSelectExpression(this);
            }

            return base.Accept(visitor);
        }

        protected override Expression VisitChildren(ExpressionTreeVisitor visitor)
        {
            return this;
        }
    }
}
