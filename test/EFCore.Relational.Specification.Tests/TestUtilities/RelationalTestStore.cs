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

        public override TestStore Initialize(
            IServiceProvider serviceProvider, Func<DbContext> createContext, Action<DbContext> seed = null, Action<DbContext> clean = null)
        {
            base.Initialize(serviceProvider, createContext, seed, clean);

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

        public virtual string NormalizeDelimetersInRawString(string sql)
            => sql.Replace("[", OpenDelimeter).Replace("]", CloseDelimeter);

        public virtual FormattableString NormalizeDelimetersInInterpolatedString(FormattableString sql)
            => new TestFormattableString(NormalizeDelimetersInRawString(sql.Format), sql.GetArguments());

        protected virtual string OpenDelimeter => "\"";

        protected virtual string CloseDelimeter => "\"";
    }
}
