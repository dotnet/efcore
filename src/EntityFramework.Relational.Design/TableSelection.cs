// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Scaffolding
{
    public class TableSelection
    {
        public const string Any = "*";
        public static readonly TableSelection InclusiveAll = new TableSelection();
        public static readonly TableSelection ExclusiveAll = new TableSelection() { Exclude = true };

        public virtual string Schema { get;[param: NotNull] set; } = Any;
        public virtual string Table { get;[param: NotNull] set; } = Any;
        public virtual bool Exclude { get; set; }

        public virtual bool Matches([NotNull] string schemaName, [NotNull] string tableName)
        {
            Check.NotEmpty(schemaName, nameof(schemaName));
            Check.NotEmpty(tableName, nameof(tableName));

            return (Schema == Any || Schema == schemaName)
                && (Table == Any || Table == tableName);
        }
    }
}
