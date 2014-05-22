// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.Utilities;
using Remotion.Linq.Clauses;

namespace Microsoft.Data.Entity.Relational.Query
{
    public class SqlSelect
    {
        private readonly IList<IProperty> _selectList = new List<IProperty>();

        private readonly IList<Tuple<IProperty, OrderingDirection>> _orderByList
            = new List<Tuple<IProperty, OrderingDirection>>();

        private object _tableSource;
        private int? _limit;
        private int _aliasCount;
        private bool _projectStar;
        private bool _distinct;

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
                    = new SqlSelect
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

        public override string ToString()
        {
            var selectSql = new StringBuilder();

            selectSql
                .Append("SELECT ");

            if (_distinct)
            {
                selectSql
                    .Append("DISTINCT ");
            }

            if (_limit != null)
            {
                selectSql
                    .Append("TOP ")
                    .Append(_limit)
                    .Append(" ");
            }

            if (_projectStar)
            {
                selectSql
                    .Append("*");
            }
            else
            {
                selectSql
                    .AppendJoin(
                        !IsEmptyProjection
                            ? _selectList.Select(p => p.StorageName)
                            : new[] { "1" });
            }

            selectSql
                .AppendLine()
                .Append("FROM ");

            // HACK: this is temporary
            var isSubquery = _tableSource is SqlSelect;

            if (isSubquery)
            {
                selectSql
                    .Append("(")
                    .Append(_tableSource)
                    .Append(") AS ")
                    .Append("t")
                    .Append(_aliasCount++.ToString(CultureInfo.InvariantCulture));
            }
            else
            {
                selectSql
                    .Append(_tableSource);
            }

            if (_orderByList.Count > 0)
            {
                selectSql
                    .AppendLine()
                    .Append("ORDER BY ")
                    .AppendJoin(
                        _orderByList
                            .Select(o => o.Item2 == OrderingDirection.Asc
                                ? o.Item1.StorageName
                                : o.Item1.StorageName + " DESC"));
            }

            return selectSql.ToString();
        }
    }
}
