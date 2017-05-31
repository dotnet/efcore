// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations.Operations;

namespace Microsoft.EntityFrameworkCore.Migrations.Design
{
    public interface IMigrationsCodeGenerator
    {
        string GenerateMetadata(
            [NotNull] string migrationNamespace,
            [NotNull] Type contextType,
            [NotNull] string migrationName,
            [NotNull] string migrationId,
            [NotNull] IModel targetModel);

        string GenerateMigration(
            [NotNull] string migrationNamespace,
            [NotNull] string migrationName,
            [NotNull] IReadOnlyList<MigrationOperation> upOperations,
            [NotNull] IReadOnlyList<MigrationOperation> downOperations);

        string GenerateSnapshot(
            [NotNull] string modelSnapshotNamespace,
            [NotNull] Type contextType,
            [NotNull] string modelSnapshotName,
            [NotNull] IModel model);

        string FileExtension { get; }
    }
}
