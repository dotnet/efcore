// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using JetBrains.Annotations;
using Microsoft.Data.Relational.Utilities;

namespace Microsoft.Data.Relational.Model
{
    public class Table
    {
        private Database _database;
        private readonly SchemaQualifiedName _name;
        private readonly List<Column> _columns = new List<Column>();
        private PrimaryKey _primaryKey;

        public Table(SchemaQualifiedName name)
        {
            _name = name;
        }

        public virtual Database Database
        {
            get { return _database; }

            [param: CanBeNull]
            internal set
            {
                Contract.Assert((value == null) != (_database == null));
                _database = value;
            }
        }

        public virtual SchemaQualifiedName Name
        {
            get { return _name; }
        }

        public virtual IReadOnlyList<Column> Columns
        {
            get { return _columns; }
        }

        public virtual void AddColumn([NotNull] Column column)
        {
            Check.NotNull(column, "column");

            // TODO: Validate input.

            _columns.Add(column);
            column.Table = this;
        }

        public virtual PrimaryKey PrimaryKey
        {
            get { return _primaryKey; }

            [param: NotNull]
            set
            {
                Check.NotNull(value, "value");

                // TODO: Validate input.

                _primaryKey = value;
            }
        }
    }
}
