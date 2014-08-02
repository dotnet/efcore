// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Relational.Utilities;

namespace Microsoft.Data.Entity.Relational.Model
{
    // TODO: Consider adding more validation.    
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

        protected internal virtual ForeignKey Clone(CloneContext cloneContext)
        {
            return 
                new ForeignKey(
                    Name,
                    Columns.Select(column => column.Clone(cloneContext)).ToArray(),
                    ReferencedColumns.Select(column => column.Clone(cloneContext)).ToArray(),
                    CascadeDelete);
        }
    }
}
