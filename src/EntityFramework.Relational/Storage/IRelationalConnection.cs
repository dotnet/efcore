// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Storage
{
    public interface IRelationalConnection : IDisposable
    {
        string ConnectionString { get; }

        DbConnection DbConnection { get; }

        IRelationalTransaction Transaction { get; }

        int? CommandTimeout { get; set; }

        DbTransaction DbTransaction { get; }

        IRelationalTransaction BeginTransaction();

        Task<IRelationalTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default(CancellationToken));

        IRelationalTransaction BeginTransaction(IsolationLevel isolationLevel);

        Task<IRelationalTransaction> BeginTransactionAsync(IsolationLevel isolationLevel, CancellationToken cancellationToken = default(CancellationToken));

        IRelationalTransaction UseTransaction([CanBeNull] DbTransaction transaction);

        void Open();

        Task OpenAsync(CancellationToken cancellationToken = default(CancellationToken));

        void Close();

        bool IsMultipleActiveResultSetsEnabled { get; }
    }
}
