// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
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
        private readonly List<Ordering> _orderBy = new List<Ordering>();

        private TableExpression _tableSource;

        private int? _limit;

        public SelectExpression([NotNull] Type type)
            : base(Check.NotNull(type, "type"))
        {
        }

        public virtual TableExpression TableSource
        {
            get { return _tableSource; }
            [param: NotNull]
            set
            {
                Check.NotNull(value, "value");

                _tableSource = value;
            }
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

        public virtual IReadOnlyList<Expression> Projection
        {
            get { return _projection; }
        }

        public virtual void AddToProjection([NotNull] IProperty property)
        {
            Check.NotNull(property, "property");

            if (GetProjectionIndex(property) == -1)
            {
                _projection.Add(new ColumnExpression(property, _tableSource.Alias));
            }
        }

        public virtual bool IsEmptyProjection
        {
            get { return _projection.Count == 0; }
        }

        public virtual int GetProjectionIndex([NotNull] IProperty property)
        {
            Check.NotNull(property, "property");

            return _projection.FindIndex(ce => ce.Property == property);
        }

        public virtual Expression Predicate { get; [param: CanBeNull] set; }
        
        public virtual void AddToOrderBy([NotNull] IProperty property, OrderingDirection orderingDirection)
        {
            Check.NotNull(property, "property");

            var columnExpression = new ColumnExpression(property, _tableSource.Alias);

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
