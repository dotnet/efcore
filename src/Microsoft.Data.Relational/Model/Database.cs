// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Relational.Utilities;

namespace Microsoft.Data.Relational.Model
{
    public class Database
    {
        private readonly List<Sequence> _sequences = new List<Sequence>();
        private readonly List<Table> _tables = new List<Table>();

        public virtual IReadOnlyList<Sequence> Sequences
        {
            get { return _sequences; }
        }

        public virtual IReadOnlyList<Table> Tables
        {
            get { return _tables; }
        }

        public virtual Sequence GetSequence(SchemaQualifiedName sequenceName)
        {
            return _sequences.First(s => s.Name.Equals(sequenceName));
        }

        public virtual Table GetTable(SchemaQualifiedName tableName)
        {
            return _tables.First(t => t.Name.Equals(tableName));
        }

        public virtual PrimaryKey GetPrimaryKey(SchemaQualifiedName primaryKeyName)
        {
            return _tables.First(t => t.PrimaryKey.Name.Equals(primaryKeyName)).PrimaryKey;
        }

        public virtual void AddSequence([NotNull] Sequence sequence)
        {
            Check.NotNull(sequence, "sequence");

            // TODO: Validate sequence.

            _sequences.Add(sequence);
            sequence.Database = this;
        }

        public virtual void AddTable([NotNull] Table table)
        {
            Check.NotNull(table, "table");

            // TODO: Validate table.

            _tables.Add(table);
            table.Database = this;
        }
    }
}
