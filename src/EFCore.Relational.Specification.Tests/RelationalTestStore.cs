// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Data;
using System.Data.Common;
using System.Threading.Tasks;

namespace Microsoft.EntityFrameworkCore
{
    public abstract class RelationalTestStore : TestStore
    {
        public virtual string ConnectionString { get; protected set; }
        public ConnectionState ConnectionState => Connection.State;
        public void CloseConnection() => Connection.Close();
        public virtual void OpenConnection() => Connection.Open();
        public virtual Task OpenConnectionAsync() => Connection.OpenAsync();
        public DbTransaction BeginTransaction() => Connection.BeginTransaction();

        protected virtual DbConnection Connection { get; set; }

        protected RelationalTestStore(string name)
            : base(name)
        {
        }

        public override void Dispose()
        {
            Connection?.Dispose();
            base.Dispose();
        }
    }
}
