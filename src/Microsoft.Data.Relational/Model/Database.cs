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
