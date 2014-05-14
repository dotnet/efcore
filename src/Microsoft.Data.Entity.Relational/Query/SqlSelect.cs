// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Text;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Relational.Utilities;

namespace Microsoft.Data.Entity.Relational.Query
{
    public class SqlSelect
    {
        private readonly IList<string> _selectList = new List<string>();
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

        public virtual int AddToSelectList([NotNull] string expression)
        {
            Check.NotNull(expression, "expression");

            _selectList.Add(expression);

            return _selectList.Count - 1;
        }

        public virtual bool IsEmptyProjection
        {
            get { return _selectList.Count == 0; }
        }

        public override string ToString()
        {
            var sql = new StringBuilder();

            sql.Append("SELECT ")
                .AppendJoin(!IsEmptyProjection ? _selectList : new[] { "1" })
                .AppendLine()
                .Append("FROM ")
                .Append(Table);

            return sql.ToString();
        }
    }
}
