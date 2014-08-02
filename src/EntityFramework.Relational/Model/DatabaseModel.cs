// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Relational.Utilities;

namespace Microsoft.Data.Entity.Relational.Model
{
    // TODO: Consider adding more validation.
    public class DatabaseModel
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

        public virtual void AddSequence([NotNull] Sequence sequence)
        {
            Check.NotNull(sequence, "sequence");

            _sequences.Add(sequence);
        }

        public virtual void RemoveSequence(SchemaQualifiedName sequenceName)
        {
            _sequences.RemoveAt(_sequences.FindIndex(s => s.Name.Equals(sequenceName)));
        }

        public virtual void AddTable([NotNull] Table table)
        {
            Check.NotNull(table, "table");

            _tables.Add(table);
        }

        public virtual void RemoveTable(SchemaQualifiedName tableName)
        {
            _tables.RemoveAt(_tables.FindIndex(t => t.Name.Equals(tableName)));
        }

        public virtual DatabaseModel Clone()
        {            
            var clone = new DatabaseModel();
            var cloneContext = new CloneContext();

            clone._sequences.AddRange(Sequences.Select(s => s.Clone(cloneContext)));
            clone._tables.AddRange(Tables.Select(t => t.Clone(cloneContext)));

            return clone;
        }
    }
}
