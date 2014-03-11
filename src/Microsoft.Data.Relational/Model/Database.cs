// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Data.Relational.Utilities;

namespace Microsoft.Data.Relational.Model
{
    public class Database
    {
        private readonly string _name;
        private readonly List<Table> _tables = new List<Table>();

        public Database([NotNull] string name)
        {
            Check.NotEmpty(name, "name");

            _name = name;
        }

        public virtual string Name
        {
            get { return _name; }
        }

        public virtual IReadOnlyList<Table> Tables
        {
            get { return _tables; }
        }

        public virtual void AddTable([NotNull] Table table)
        {
            Check.NotNull(table, "table");

            _tables.Add(table);
            table.Database = this;
        }
    }
}
