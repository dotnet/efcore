// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Relational.Utilities;

namespace Microsoft.Data.Entity.Relational.Model
{
    // TODO: Consider adding more validation.    
    public class Table
    {
        private SchemaQualifiedName _name;
        private readonly List<Column> _columns = new List<Column>();
        private PrimaryKey _primaryKey;
        private readonly List<ForeignKey> _foreignKeys = new List<ForeignKey>();
        private readonly List<Index> _indexes = new List<Index>();

        public Table(SchemaQualifiedName name)
        {
            _name = name;
        }

        public Table(SchemaQualifiedName name, [NotNull] IEnumerable<Column> columns)
        {
            Check.NotNull(columns, "columns");

            _name = name;

            foreach (var column in columns)
            {
                AddColumn(column);
            }
        }

        public virtual SchemaQualifiedName Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public virtual IReadOnlyList<Column> Columns
        {
            get { return _columns; }
        }

        public virtual PrimaryKey PrimaryKey
        {
            get { return _primaryKey; }

            [param: CanBeNull] set { _primaryKey = value; }
        }

        public virtual IReadOnlyList<ForeignKey> ForeignKeys
        {
            get { return _foreignKeys; }
        }

        public virtual IReadOnlyList<Index> Indexes
        {
            get { return _indexes; }
        }

        public virtual Column GetColumn([NotNull] string columnName)
        {
            Check.NotEmpty(columnName, "columnName");

            return _columns.First(c => c.Name.Equals(columnName, StringComparison.Ordinal));
        }

        public virtual void AddColumn([NotNull] Column column)
        {
            Check.NotNull(column, "column");

            _columns.Add(column);
            column.Table = this;
        }

        public virtual void RemoveColumn([NotNull] string columnName)
        {
            Check.NotEmpty(columnName, "columnName");

            var i = _columns.FindIndex(c => c.Name == columnName);
            var column = _columns[i];

            _columns.RemoveAt(i);
            column.Table = null;
        }

        public virtual ForeignKey GetForeignKey([NotNull] string foreignKeyName)
        {
            Check.NotEmpty(foreignKeyName, "foreignKeyName");

            return _foreignKeys.First(c => c.Name.Equals(foreignKeyName, StringComparison.Ordinal));
        }

        public virtual void AddForeignKey([NotNull] ForeignKey foreignKey)
        {
            Check.NotNull(foreignKey, "foreignKey");

            _foreignKeys.Add(foreignKey);
        }

        public virtual void RemoveForeignKey([NotNull] string foreignKeyName)
        {
            Check.NotEmpty(foreignKeyName, "foreignKeyName");

            var i = _foreignKeys.FindIndex(fk => fk.Name == foreignKeyName);

            _foreignKeys.RemoveAt(i);
        }

        public virtual Index GetIndex([NotNull] string indexName)
        {
            Check.NotEmpty(indexName, "indexName");

            return _indexes.First(c => c.Name.Equals(indexName, StringComparison.Ordinal));
        }

        public virtual void AddIndex([NotNull] Index index)
        {
            Check.NotNull(index, "index");

            _indexes.Add(index);
        }

        public virtual void RemoveIndex([NotNull] string indexName)
        {
            Check.NotEmpty(indexName, "indexName");

            var i = _indexes.FindIndex(ix => ix.Name == indexName);

            _indexes.RemoveAt(i);
        }

        public virtual Table Clone([NotNull] CloneContext cloneContext)
        {
            Check.NotNull(cloneContext, "cloneContext");

            var clone = new Table(Name, Columns.Select(c => c.Clone(cloneContext)));

            if (PrimaryKey != null)
            {
                clone._primaryKey = PrimaryKey.Clone(cloneContext);
            }

            clone._foreignKeys.AddRange(ForeignKeys.Select(fk => fk.Clone(cloneContext)));
            clone._indexes.AddRange(Indexes.Select(ix => ix.Clone(cloneContext)));

            return clone;
        }
    }
}
