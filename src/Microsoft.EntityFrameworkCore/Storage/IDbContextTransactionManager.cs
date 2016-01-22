// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.EntityFrameworkCore.Storage
{
    public interface IDbContextTransactionManager
    {
        IDbContextTransaction BeginTransaction();

        Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default(CancellationToken));

        void CommitTransaction();

        void RollbackTransaction();
    }
}
