// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Diagnostics.Contracts;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Data.Relational.Utilities;

namespace Microsoft.Data.Relational.Model
{
    public class PrimaryKey
    {
        private readonly SchemaQualifiedName _name;
        private readonly List<Column> _columns;
        private readonly bool _isClustered;

        public PrimaryKey(SchemaQualifiedName name, [NotNull] IReadOnlyList<Column> columns, bool isClustered)
        {
            Check.NotNull(columns, "columns");
            ValidateColumns(columns);

            _name = name;
            _columns = new List<Column>(columns);
            _isClustered = isClustered;
        }

        public PrimaryKey(SchemaQualifiedName name, [NotNull] IReadOnlyList<Column> columns)
            : this(name, columns, false)
        {
        }

        public virtual SchemaQualifiedName Name
        {
            get { return _name; }
        }

        public virtual bool IsClustered 
        {
            get { return _isClustered; }
        }

        public virtual IReadOnlyList<Column> Columns
        {
            get { return _columns; }
        }

        private static void ValidateColumns(IReadOnlyList<Column> columns)
        {
            Contract.Assert(columns.Count > 0);
            Contract.Assert(columns[0] != null);

            for (var i = 1; i < columns.Count; i++)
            {
                Contract.Assert(columns[i] != null);
                Contract.Assert(ReferenceEquals(columns[0].Table, columns[i].Table));
            }
        }
    }
}
