// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;

namespace Microsoft.EntityFrameworkCore.Scaffolding
{
    public interface IDatabaseModelFactory
    {
        DatabaseModel Create([NotNull] string connectionString, [NotNull] TableSelectionSet tableSelectionSet);
    }
}
