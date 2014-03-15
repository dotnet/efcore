// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Diagnostics.Contracts;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Relational.Utilities;
using System;

namespace Microsoft.Data.Relational.Model
{
    public class PrimaryKey
    {
        private readonly SchemaQualifiedName _name;
        private object _columns;
        private readonly bool _isClustered;

        public PrimaryKey(SchemaQualifiedName name, [NotNull] IReadOnlyList<Column> columns, bool isClustered)
        {
            Check.NotNull(columns, "columns");

            // TODO: Validate input. Must have at least one column. All columns must not be null. 
            // All columns must be part of the same table.

            _name = name;
            _columns = columns;
            _isClustered = isClustered;
        }

        public PrimaryKey(SchemaQualifiedName name, [NotNull] IReadOnlyList<Column> columns)
            : this(name, columns, false)
        {
        }

        public PrimaryKey(SchemaQualifiedName name, [NotNull] IReadOnlyList<string> columnNames, bool isClustered)
        {
            Check.NotNull(columnNames, "columnNames");

            // TODO: Validate input. Must have at least one column. All column names must not be empty.

            _name = name;
            _columns = columnNames;
            _isClustered = isClustered;
        }

        public PrimaryKey(SchemaQualifiedName name, [NotNull] IReadOnlyList<string> columnNames)
            : this(name, columnNames, false)
        {
        }

        public virtual SchemaQualifiedName Name
        {
            get { return _name; }
        }

        public virtual IReadOnlyList<Column> Columns
        {
            get
            {
                var columns = _columns as IReadOnlyList<Column>;
                if (columns != null)
                {
                    return columns;
                }

                // TODO: Use exception message that indicates that 
                // the primary key was not associated with a table.

                throw new InvalidOperationException();
            }
        }

        public virtual Table Table
        {
            get { return Columns[0].Table; }
        }

        public virtual bool IsClustered
        {
            get { return _isClustered; }
        }

        internal void FixupColumns(Table table)
        {
            var columnNames = _columns as IReadOnlyList<string>;
            if (columnNames != null)
            {
                _columns = columnNames.Select(n => table.GetColumn(n)).ToArray();
            }
        }
    }
}
