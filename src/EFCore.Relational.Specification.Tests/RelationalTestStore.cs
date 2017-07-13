// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Data.Common;
using System.Threading.Tasks;

namespace Microsoft.EntityFrameworkCore
{
    public abstract class RelationalTestStore<TConnection> : TestStore, IRelationalTestStore<TConnection>
        where TConnection : DbConnection
    {
        public virtual string ConnectionString { get; protected set; }
        public virtual void OpenConnection() => Connection.Open();
        public virtual Task OpenConnectionAsync() => Connection.OpenAsync();
        // TODO: Make protected
        public virtual TConnection Connection { get; protected set; }
        // TODO: Remove
        public virtual DbTransaction Transaction { get; }

        protected RelationalTestStore(string name)
            : base(name)
        {
        }

        public override void Dispose()
        {
            Transaction?.Dispose();
            Connection?.Dispose();
            base.Dispose();
        }
    }
}
