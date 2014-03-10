// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics;
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
                Debug.Assert((value == null) != (_database == null));
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

            _columns.Add(column);
            column.Table = this;
        }

        public virtual bool RemoveColumn([NotNull] Column column)
        {
            Check.NotNull(column, "column");

            if (_primaryKey == null)
            {
                return RemoveColumnFromList(column, _columns);
            }

            if (_primaryKey.Columns.Count > 1)
            {
                _primaryKey.RemoveColumn(column);
                return RemoveColumnFromList(column, _columns);
            }

            if (!ReferenceEquals(_primaryKey.Columns[0], column))
            {
                return RemoveColumnFromList(column, _columns);
            }

            return false;
        }

        private static bool RemoveColumnFromList(Column column, List<Column> list)
        {
            if (list.Remove(column))
            {
                column.Table = null;
                return true;
            }

            return false;
        }

        public virtual PrimaryKey PrimaryKey
        {
            get { return _primaryKey; }

            [param: NotNull]
            set
            {
                Check.NotNull(value, "value");
                Debug.Assert(ReferenceEquals(this, value.Columns[0].Table));

                _primaryKey = value;
            }
        }
    }
}
