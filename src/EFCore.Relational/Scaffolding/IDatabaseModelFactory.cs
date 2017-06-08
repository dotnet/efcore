// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Data.Common;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;

namespace Microsoft.EntityFrameworkCore.Scaffolding
{
    public interface IDatabaseModelFactory
    {
        DatabaseModel Create([NotNull] string connectionString, [NotNull] IEnumerable<string> tables, [NotNull] IEnumerable<string> schemas);
        DatabaseModel Create([NotNull] DbConnection connection, [NotNull] IEnumerable<string> tables, [NotNull] IEnumerable<string> schemas);
    }
}
