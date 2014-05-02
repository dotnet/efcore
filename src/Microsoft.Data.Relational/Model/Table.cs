// Copyright (c) Microsoft Open Technologies, Inc.
// All Rights Reserved
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// THIS CODE IS PROVIDED *AS IS* BASIS, WITHOUT WARRANTIES OR
// CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING
// WITHOUT LIMITATION ANY IMPLIED WARRANTIES OR CONDITIONS OF
// TITLE, FITNESS FOR A PARTICULAR PURPOSE, MERCHANTABLITY OR
// NON-INFRINGEMENT.
// See the Apache 2 License for the specific language governing
// permissions and limitations under the License.

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
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
        private readonly List<ForeignKey> _foreignKeys = new List<ForeignKey>();
        private readonly List<Index> _indexes = new List<Index>();

        public Table(SchemaQualifiedName name)
        {
            _name = name;
        }

        public Table(SchemaQualifiedName name, [NotNull] IReadOnlyList<Column> columns)
        {
            Check.NotNull(columns, "columns");

            _name = name;

            foreach (var column in columns)
            {
                AddColumn(column);
            }
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

        public virtual Column GetColumn([NotNull] string columnName)
        {
            Check.NotEmpty(columnName, "columnName");

            return _columns.First(c => c.Name.Equals(columnName, StringComparison.Ordinal));
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

        public virtual IReadOnlyList<ForeignKey> ForeignKeys
        {
            get { return _foreignKeys; }
        }

        public virtual void AddForeignKey([NotNull] ForeignKey foreignKey)
        {
            Check.NotNull(foreignKey, "foreignKey");

            // TODO: Validate input.

            _foreignKeys.Add(foreignKey);
        }

        public virtual IReadOnlyList<Index> Indexes
        {
            get { return _indexes; }
        }

        public virtual Index GetIndex([NotNull] string indexName)
        {
            Check.NotEmpty(indexName, "indexName");

            return _indexes.First(c => c.Name.Equals(indexName, StringComparison.Ordinal));
        }

        public virtual void AddIndex([NotNull] Index index)
        {
            Check.NotNull(index, "index");

            // TODO: Validate input.

            _indexes.Add(index);
        }
    }
}
