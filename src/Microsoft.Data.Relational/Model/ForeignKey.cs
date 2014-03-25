// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Data.Relational.Utilities;

namespace Microsoft.Data.Relational.Model
{
    public class ForeignKey
    {
        private readonly string _name;
        private readonly IReadOnlyList<Column> _columns;
        private readonly IReadOnlyList<Column> _referencedColumns;
        private readonly bool _cascadeDelete;

        public ForeignKey(
            [NotNull] string name,
            [NotNull] IReadOnlyList<Column> columns,
            [NotNull] IReadOnlyList<Column> referencedColumns,
            bool cascadeDelete)
        {
            Check.NotEmpty(name, "name");
            Check.NotNull(columns, "columns");
            Check.NotNull(referencedColumns, "referencedColumns");

            // TODO: Validate input.

            _name = name;
            _columns = columns;
            _referencedColumns = referencedColumns;
            _cascadeDelete = cascadeDelete;
        }

        public ForeignKey(
            [NotNull] string name,
            [NotNull] IReadOnlyList<Column> columns,
            [NotNull] IReadOnlyList<Column> referencedColumns)
            : this(name, columns, referencedColumns, cascadeDelete: false)
        {
        }

        public virtual string Name
        {
            get { return _name; }
        }

        public virtual Table Table
        {
            get { return _columns[0].Table; }
        }

        public virtual Table ReferencedTable
        {
            get { return _referencedColumns[0].Table; }
        }

        public virtual IReadOnlyList<Column> Columns
        {
            get { return _columns; }
        }

        public virtual IReadOnlyList<Column> ReferencedColumns
        {
            get { return _referencedColumns; }
        }

        public virtual bool CascadeDelete
        {
            get { return _cascadeDelete; }
        }
    }
}
