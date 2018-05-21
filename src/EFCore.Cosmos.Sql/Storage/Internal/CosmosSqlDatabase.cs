// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Update;

namespace Microsoft.EntityFrameworkCore.Cosmos.Sql.Storage.Internal
{
    public class CosmosSqlDatabase : Database
    {
        public CosmosSqlDatabase([NotNull] DatabaseDependencies dependencies)
            : base(dependencies)
        {
        }

        public override int SaveChanges(IReadOnlyList<IUpdateEntry> entries)
        {
            throw new NotImplementedException();
        }

        public override Task<int> SaveChangesAsync(
            IReadOnlyList<IUpdateEntry> entries, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
