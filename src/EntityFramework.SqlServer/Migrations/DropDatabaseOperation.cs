// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Relational.Migrations.Operations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.SqlServer.Migrations
{
    public class DropDatabaseOperation : MigrationOperation
    {
        public DropDatabaseOperation(
            [NotNull] string name,
            [CanBeNull] IReadOnlyDictionary<string, string> annotations = null)
            : base(annotations)
        {
            Check.NotEmpty(name, nameof(name));

            Name = name;
        }

        public virtual string Name { get; }
    }
}
