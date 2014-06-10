// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.Utilities;
using Remotion.Linq.Clauses;

namespace Microsoft.Data.Entity.Relational.Query.Sql
{
    public class SqlSelect : IParameterFactory
    {
        public class Parameter
        {
            private readonly string _name;
            private readonly object _value;

            public Parameter([NotNull] string name, [NotNull] object value)
            {
                Check.NotNull(name, "name");
                Check.NotNull(value, "value");

                _name = name;
                _value = value;
            }

            public virtual string Name
            {
                get { return _name; }
            }

            public virtual object Value
            {
                get { return _value; }
            }
        }

        private readonly List<IProperty> _selectList = new List<IProperty>();

        private readonly List<Tuple<IProperty, OrderingDirection>> _orderByList
            = new List<Tuple<IProperty, OrderingDirection>>();

        private readonly List<Parameter> _parameters = new List<Parameter>();

        private readonly ISqlGeneratingExpressionTreeVisitorFactory _sqlGeneratingExpressionTreeVisitorFactory;

        private object _tableSource;
        private int? _limit;
        private int _aliasCount;
        private bool _projectStar;
        private bool _distinct;
        private Expression _predicate;

        public SqlSelect([NotNull] ISqlGeneratingExpressionTreeVisitorFactory sqlGeneratingExpressionTreeVisitorFactory)
        {
            Check.NotNull(sqlGeneratingExpressionTreeVisitorFactory, "sqlGeneratingExpressionTreeVisitorFactory");

            _sqlGeneratingExpressionTreeVisitorFactory = sqlGeneratingExpressionTreeVisitorFactory;
        }
        
        public virtual SqlSelect SetTableSource([NotNull] object tableSource)
        {
            Check.NotNull(tableSource, "tableSource");

            _tableSource = tableSource;

            return this;
        }

        public virtual bool TryMakeDistinct()
        {
            _distinct
                = _orderByList
                    .Select(t => t.Item1)
                    .All(p => !_selectList.Contains(p));

            return _distinct;
        }

        public virtual void AddLimit(int limit)
        {
            if (_limit != null)
            {
                _tableSource
                    = new SqlSelect(_sqlGeneratingExpressionTreeVisitorFactory)
                        {
                            _tableSource = _tableSource,
                            _limit = _limit,
                            _projectStar = true
                        };
            }

            _limit = limit;
        }

        public virtual void AddToProjection([NotNull] IProperty property)
        {
            Check.NotNull(property, "property");

            if (!_selectList.Contains(property))
            {
                _selectList.Add(property);
            }
        }

        public virtual int GetProjectionIndex([NotNull] IProperty property)
        {
            Check.NotNull(property, "property");

            return _selectList.IndexOf(property);
        }

        public virtual void AddToOrderBy([NotNull] IProperty property, OrderingDirection orderingDirection)
        {
            Check.NotNull(property, "property");

            _orderByList.Add(Tuple.Create(property, orderingDirection));
        }

        public virtual bool IsEmptyProjection
        {
            get { return _selectList.Count == 0; }
        }

        public virtual string CreateParameter(object value)
        {
            Check.NotNull(value, "value");

            var parameter
                = _parameters.SingleOrDefault(kv => Equals(kv.Value, value));

            if (parameter == null)
            {
                _parameters.Add(parameter = new Parameter("@p" + _parameters.Count, value));
            }

            return parameter.Name;
        }

        public virtual IEnumerable<Parameter> Parameters
        {
            get { return _parameters; }
        }

        public virtual void SetPredicate([CanBeNull] Expression predicate)
        {
            _predicate = predicate;
        }

        public override string ToString()
        {
            var sql = new StringBuilder();

            sql
                .Append("SELECT ");

            if (_distinct)
            {
                sql
                    .Append("DISTINCT ");
            }

            if (_limit != null)
            {
                sql
                    .Append("TOP ")
                    .Append(_limit)
                    .Append(" ");
            }

            if (_projectStar)
            {
                sql
                    .Append("*");
            }
            else
            {
                sql
                    .AppendJoin(
                        !IsEmptyProjection
                            ? _selectList.Select(p => p.StorageName)
                            : new[] { "1" });
            }

            sql
                .AppendLine()
                .Append("FROM ");

            // HACK: this is temporary
            var isSubquery = _tableSource is SqlSelect;

            if (isSubquery)
            {
                sql
                    .Append("(")
                    .Append(_tableSource)
                    .Append(") AS ")
                    .Append("t")
                    .Append(_aliasCount++.ToString(CultureInfo.InvariantCulture));
            }
            else
            {
                sql
                    .Append(_tableSource);
            }

            if (_predicate != null)
            {
                sql
                    .AppendLine()
                    .Append("WHERE ");

                _sqlGeneratingExpressionTreeVisitorFactory
                    .Create(sql, this)
                    .VisitExpression(_predicate);
            }

            if (_orderByList.Count > 0)
            {
                sql
                    .AppendLine()
                    .Append("ORDER BY ")
                    .AppendJoin(
                        _orderByList
                            .Select(o => o.Item2 == OrderingDirection.Asc
                                ? o.Item1.StorageName
                                : o.Item1.StorageName + " DESC"));
            }

            return sql.ToString();
        }
    }
}
