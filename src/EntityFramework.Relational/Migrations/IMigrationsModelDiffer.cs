// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Migrations.Operations;

namespace Microsoft.Data.Entity.Migrations
{
    public interface IMigrationsModelDiffer
    {
        bool HasDifferences([CanBeNull] IModel source, [CanBeNull] IModel target);

        IReadOnlyList<MigrationOperation> GetDifferences([CanBeNull] IModel source, [CanBeNull] IModel target);
    }
}
