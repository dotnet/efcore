// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Storage;

namespace Microsoft.Data.Entity.Relational
{
    public interface IRelationalConnection : IDataStoreConnection, IDisposable
    {
        string ConnectionString { get; }

        DbConnection DbConnection { get; }

        RelationalTransaction Transaction { get; }

        int? CommandTimeout { get; set; }

        DbTransaction DbTransaction { get; }

        RelationalTransaction BeginTransaction();

        Task<RelationalTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default(CancellationToken));

        RelationalTransaction BeginTransaction(IsolationLevel isolationLevel);

        Task<RelationalTransaction> BeginTransactionAsync(IsolationLevel isolationLevel, CancellationToken cancellationToken = default(CancellationToken));

        RelationalTransaction UseTransaction([CanBeNull] DbTransaction transaction);

        void Open();

        Task OpenAsync(CancellationToken cancellationToken = default(CancellationToken));

        void Close();
    }
}
