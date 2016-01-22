// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Storage
{
    public interface IRelationalTransactionManager : IDbContextTransactionManager
    {
        IDbContextTransaction BeginTransaction(IsolationLevel isolationLevel);

        Task<IDbContextTransaction> BeginTransactionAsync(IsolationLevel isolationLevel, CancellationToken cancellationToken = default(CancellationToken));

        IDbContextTransaction UseTransaction([CanBeNull] DbTransaction transaction);
    }
}
