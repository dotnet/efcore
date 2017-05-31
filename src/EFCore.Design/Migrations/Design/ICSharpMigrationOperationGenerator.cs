// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Migrations.Operations;

namespace Microsoft.EntityFrameworkCore.Migrations.Design
{
    public interface ICSharpMigrationOperationGenerator
    {
        void Generate(
            [NotNull] string builderName,
            [NotNull] IReadOnlyList<MigrationOperation> operations,
            [NotNull] IndentedStringBuilder builder);
    }
}
