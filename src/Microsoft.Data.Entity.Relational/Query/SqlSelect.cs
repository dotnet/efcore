// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Relational.Utilities;
using Remotion.Linq.Clauses;

namespace Microsoft.Data.Entity.Relational.Query
{
    public class SqlSelect
    {
        private readonly IList<string> _selectList = new List<string>();

        private readonly IList<Tuple<string, OrderingDirection>> _orderByList
            = new List<Tuple<string, OrderingDirection>>();

        private SchemaQualifiedName _table;

        public virtual SchemaQualifiedName Table
        {
            get { return _table; }
            [param: NotNull]
            set
            {
                Check.NotNull(value, "value");

                _table = value;
            }
        }

        public virtual int? TopN { get; set; }

        public virtual int AddToSelectList([NotNull] string expression)
        {
            Check.NotNull(expression, "expression");

            _selectList.Add(expression);

            return _selectList.Count - 1;
        }

        public virtual int AddToOrderByList([NotNull] string expression, OrderingDirection orderingDirection)
        {
            Check.NotNull(expression, "expression");

            _orderByList.Add(Tuple.Create(expression, orderingDirection));

            return _orderByList.Count - 1;
        }

        public virtual bool IsEmptyProjection
        {
            get { return _selectList.Count == 0; }
        }

        public override string ToString()
        {
            var selectSql = new StringBuilder();

            selectSql.Append("SELECT ");

            if (TopN != null)
            {
                selectSql
                    .Append("TOP ")
                    .Append(TopN)
                    .Append(" ");
            }

            selectSql
                .AppendJoin(!IsEmptyProjection ? _selectList : new[] { "1" })
                .AppendLine()
                .Append("FROM ")
                .Append(Table);

            if (_orderByList.Count > 0)
            {
                selectSql
                    .AppendLine()
                    .Append("ORDER BY ")
                    .AppendJoin(
                        _orderByList
                            .Select(o => o.Item2 == OrderingDirection.Asc ? o.Item1 : o.Item1 + " DESC"));
            }

            return selectSql.ToString();
        }
    }
}
