// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
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
        private readonly List<IProperty> _projection = new List<IProperty>();

        private readonly List<Tuple<IProperty, OrderingDirection>> _orderBy
            = new List<Tuple<IProperty, OrderingDirection>>();

        private object _tableSource;
        private int? _limit;
        private bool _star;
        private bool _distinct;

        public SelectExpression([NotNull] Type type)
            : base(Check.NotNull(type, "type"))
        {
        }

        public virtual object TableSource
        {
            get { return _tableSource; }
            [param: NotNull]
            set
            {
                Check.NotNull(value, "value");

                _tableSource = value;
            }
        }

        public virtual bool TryMakeDistinct()
        {
            _distinct
                = _orderBy
                    .Select(t => t.Item1)
                    .All(p => !_projection.Contains(p));

            return _distinct;
        }

        public virtual bool IsDistinct
        {
            get { return _distinct; }
        }

        public virtual void AddLimit(int limit)
        {
            if (_limit != null)
            {
                _tableSource
                    = new SelectExpression(Type)
                        {
                            _tableSource = _tableSource,
                            _limit = _limit,
                            _star = true
                        };
            }

            _limit = limit;
        }

        public virtual int? Limit
        {
            get { return _limit; }
        }

        public virtual IReadOnlyList<IProperty> Projection
        {
            get { return _projection; }
        }

        public virtual void AddToProjection([NotNull] IProperty property)
        {
            Check.NotNull(property, "property");

            if (!_projection.Contains(property))
            {
                _projection.Add(property);
            }
        }

        public virtual bool IsEmptyProjection
        {
            get { return _projection.Count == 0; }
        }

        public virtual int GetProjectionIndex([NotNull] IProperty property)
        {
            Check.NotNull(property, "property");

            return _projection.IndexOf(property);
        }

        public virtual bool IsStar
        {
            get { return _star; }
        }

        public virtual Expression Predicate { get; [param: CanBeNull] set; }

        public virtual void AddToOrderBy([NotNull] IProperty property, OrderingDirection orderingDirection)
        {
            Check.NotNull(property, "property");

            _orderBy.Add(Tuple.Create(property, orderingDirection));
        }

        public virtual IReadOnlyList<Tuple<IProperty, OrderingDirection>> OrderBy
        {
            get { return _orderBy; }
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
