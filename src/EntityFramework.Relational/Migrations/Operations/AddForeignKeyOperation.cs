// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Relational.Migrations.Operations
{
    public class AddForeignKeyOperation : MigrationOperation
    {
        public AddForeignKeyOperation(
            [NotNull] string dependentTable,
            [CanBeNull] string dependentSchema,
            [CanBeNull] string name,
            [NotNull] IReadOnlyList<string> dependentColumns,
            [NotNull] string principalTable,
            [CanBeNull] string principalSchema,
            [CanBeNull] IReadOnlyList<string> principalColumns,
            bool cascadeDelete,
            [CanBeNull] IReadOnlyDictionary<string, string> annotations = null)
            : base(annotations)
        {
            Check.NotEmpty(dependentTable, nameof(dependentTable));
            Check.NotNull(dependentColumns, nameof(dependentColumns));
            Check.NotEmpty(principalTable, nameof(principalTable));

            DependentTable = dependentTable;
            DependentSchema = dependentSchema;
            Name = name;
            DependentColumns = dependentColumns;
            PrincipalTable = principalTable;
            PrincipalSchema = principalSchema;
            PrincipalColumns = principalColumns ?? new string[0];
            CascadeDelete = cascadeDelete;
        }

        public virtual string DependentTable { get; [param: NotNull] set; }
        public virtual string DependentSchema { get; [param: CanBeNull] set; }
        public virtual string Name { get; [param: CanBeNull] set; }
        public virtual IReadOnlyList<string> DependentColumns { get; [param: NotNull] set; }
        public virtual string PrincipalTable { get; [param: NotNull] set; }
        public virtual string PrincipalSchema { get; [param: CanBeNull] set; }
        public virtual IReadOnlyList<string> PrincipalColumns { get; [param: NotNull] set; }
        public virtual bool CascadeDelete { get; set; }
    }
}
