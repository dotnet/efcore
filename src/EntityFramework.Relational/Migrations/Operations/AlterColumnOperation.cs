// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Relational.Migrations.Operations
{
    public class AlterColumnOperation : MigrationOperation
    {
        public AlterColumnOperation(
            [NotNull] string table,
            [CanBeNull] string schema,
            [NotNull] ColumnModel column,
            [CanBeNull] IReadOnlyDictionary<string, string> annotations = null,
            bool isDestructiveChange = false)
            : base(annotations)
        {
            Check.NotEmpty(table, nameof(table));
            Check.NotNull(column, nameof(column));

            Table = table;
            Schema = schema;
            Column = column;
            IsDestructiveChange = isDestructiveChange;
        }

        public virtual string Table { get; }
        public virtual string Schema { get; }
        public virtual ColumnModel Column { get; }
        public override bool IsDestructiveChange { get; }
    }
}
