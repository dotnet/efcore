// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Data.Relational.Utilities;

namespace Microsoft.Data.Relational.Model
{
    public class Table
    {
        private readonly SchemaQualifiedName _name;

        private readonly List<Column> _columns = new List<Column>();
        private PrimaryKey _primaryKey;

        public Table(SchemaQualifiedName name)
        {
            _name = name;
        }

        public virtual SchemaQualifiedName Name
        {
            get { return _name; }
        }

        public virtual List<Column> Columns
        {
            get { return _columns; }
        }

        public virtual PrimaryKey PrimaryKey
        {
            get { return _primaryKey; }
            [param: NotNull]
            set
            {
                Check.NotNull(value, "value");

                _primaryKey = value;
            }
        }
    }
}
