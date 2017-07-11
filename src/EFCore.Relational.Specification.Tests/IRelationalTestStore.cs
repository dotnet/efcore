// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data.Common;
using System.Threading.Tasks;

namespace Microsoft.EntityFrameworkCore
{
    public interface IRelationalTestStore<out TConnection> : IDisposable
        where TConnection : DbConnection
    {
        TConnection Connection { get; }
        string ConnectionString { get; }
        DbTransaction Transaction { get; }
        void OpenConnection();
        Task OpenConnectionAsync();
    }
}
