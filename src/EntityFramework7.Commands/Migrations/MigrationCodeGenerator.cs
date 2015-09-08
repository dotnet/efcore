// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Migrations.Operations;

namespace Microsoft.Data.Entity.Commands.Migrations
{
    public abstract class MigrationCodeGenerator
    {
        public abstract string Language { get; }

        public abstract string Generate(
            [NotNull] string migrationNamespace,
            [NotNull] string migrationName,
            [NotNull] IReadOnlyList<MigrationOperation> upOperations,
            [NotNull] IReadOnlyList<MigrationOperation> downOperations);

        public abstract string GenerateMetadata(
            [NotNull] string migrationNamespace,
            [NotNull] Type contextType,
            [NotNull] string migrationName,
            [NotNull] string migrationId,
            [NotNull] string productVersion,
            [NotNull] IModel targetModel);

        public abstract string GenerateSnapshot(
            [NotNull] string modelSnapshotNamespace,
            [NotNull] Type contextType,
            [NotNull] string modelSnapshotName,
            [NotNull] IModel model);
    }
}
