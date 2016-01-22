// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Migrations
{
    public interface IMigrationsSqlGenerator
    {
        IReadOnlyList<IRelationalCommand> Generate(
            [NotNull] IReadOnlyList<MigrationOperation> operations,
            [CanBeNull] IModel model = null);
    }
}
