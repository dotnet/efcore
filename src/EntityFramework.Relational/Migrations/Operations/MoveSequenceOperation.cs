// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Relational.Migrations.Operations
{
    public class MoveSequenceOperation : MigrationOperation
    {
        public MoveSequenceOperation(
            [NotNull] string name,
            [CanBeNull] string schema,
            [NotNull] string newSchema,
            [CanBeNull] IReadOnlyDictionary<string, string> annotations = null)
            : base(annotations)
        {
            Check.NotEmpty(name, nameof(name));
            Check.NotEmpty(newSchema, nameof(newSchema));

            Name = name;
            Schema = schema;
            NewSchema = newSchema;
        }

        public virtual string Name { get; }
        public virtual string Schema { get; }
        public virtual string NewSchema { get; }
    }
}
