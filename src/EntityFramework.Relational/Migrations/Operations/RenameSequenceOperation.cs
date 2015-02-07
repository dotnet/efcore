// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Relational.Migrations.Operations
{
    public class RenameSequenceOperation : MigrationOperation
    {
        public RenameSequenceOperation(
            [NotNull] string name,
            [CanBeNull] string schema,
            [NotNull] string newName,
            [CanBeNull] IReadOnlyDictionary<string, string> annotations = null)
            : base(annotations)
        {
            Check.NotEmpty(name, nameof(name));
            Check.NotEmpty(newName, nameof(newName));

            Name = name;
            Schema = schema;
            NewName = newName;
        }

        public virtual string Name { get; }
        public virtual string Schema { get; }
        public virtual string NewName { get; }
    }
}
