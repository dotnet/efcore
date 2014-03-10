// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using JetBrains.Annotations;
using Microsoft.Data.Relational.Utilities;

namespace Microsoft.Data.Relational.Model
{
    public class PrimaryKey
    {
        private readonly SchemaQualifiedName _name;
        private readonly List<Column> _columns;

        public PrimaryKey(SchemaQualifiedName name, [NotNull] Column column)
        {
            Check.NotNull(column, "column");
            Debug.Assert(column.Table != null);

            _name = name;
            _columns = new List<Column> { column };
        }

        public virtual SchemaQualifiedName Name
        {
            get { return _name; }
        }

        public virtual bool IsClustered { get; set; }

        public virtual IReadOnlyList<Column> Columns
        {
            get { return _columns; }
        }

        public virtual void AddColumn([NotNull] Column column)
        {
            Check.NotNull(column, "column");
            Debug.Assert(ReferenceEquals(_columns[0].Table, column.Table));

            _columns.Add(column);
        }

        public virtual bool RemoveColumn([NotNull] Column column)
        {
            Check.NotNull(column, "column");

            return (_columns.Count > 1) ? _columns.Remove(column) : false;
        }
    }
}
