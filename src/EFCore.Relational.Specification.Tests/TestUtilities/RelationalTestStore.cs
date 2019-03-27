// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;

namespace Microsoft.EntityFrameworkCore.TestUtilities
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

        protected RelationalTestStore(string name, bool shared)
            : base(name, shared)
        {
        }

        public override TestStore Initialize(IServiceProvider serviceProvider, Func<DbContext> createContext, Action<DbContext> seed)
        {
            base.Initialize(serviceProvider, createContext, seed);

            if (ConnectionState != ConnectionState.Open)
            {
                OpenConnection();
            }

            return this;
        }

        public override void Dispose()
        {
            Connection?.Dispose();
            base.Dispose();
        }

        public virtual RawSqlString NormalizeDelimeters(RawSqlString sql)
            => NormalizeDelimeters(sql.Format);

        public virtual FormattableString NormalizeDelimeters(FormattableString sql)
            => new TestFormattableString(NormalizeDelimeters(sql.Format), sql.GetArguments());

        private string NormalizeDelimeters(string sql)
            => sql.Replace("[", OpenDelimeter).Replace("]", CloseDelimeter);

        protected virtual string OpenDelimeter => "\"";

        protected virtual string CloseDelimeter => "\"";
    }
}
