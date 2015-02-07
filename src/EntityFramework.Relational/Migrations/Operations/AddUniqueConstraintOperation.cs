// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Relational.Migrations.Operations
{
    public class AddUniqueConstraintOperation : MigrationOperation
    {
        public AddUniqueConstraintOperation(
            [NotNull] string table,
            [CanBeNull] string schema,
            [CanBeNull] string name,
            [NotNull] IReadOnlyList<string> columns,
            [CanBeNull] IReadOnlyDictionary<string, string> annotations = null)
            : base(annotations)
        {
            Check.NotEmpty(table, nameof(table));
            Check.NotNull(columns, nameof(columns));

            Table = table;
            Schema = schema;
            Name = name;
            Columns = columns;
        }

        public virtual string Table { get;[param: NotNull] set; }
        public virtual string Schema { get;[param: CanBeNull] set; }
        public virtual string Name { get;[param: CanBeNull] set; }
        public virtual IReadOnlyList<string> Columns { get;[param: NotNull] set; }
    }
}
