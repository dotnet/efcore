// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Data;

namespace Microsoft.EntityFrameworkCore.TestUtilities;

public abstract class RelationalTestStore : TestStore
{
    public virtual string ConnectionString { get; protected set; }

    public ConnectionState ConnectionState
        => Connection.State;

    public void CloseConnection()
        => Connection.Close();

    public virtual void OpenConnection()
        => Connection.Open();

    public virtual Task OpenConnectionAsync()
        => Connection.OpenAsync();

    public DbTransaction BeginTransaction()
        => Connection.BeginTransaction();

    protected virtual DbConnection Connection { get; set; }

    protected RelationalTestStore(string name, bool shared)
        : base(name, shared)
    {
    }

    public override TestStore Initialize(
        IServiceProvider serviceProvider,
        Func<DbContext> createContext,
        Action<DbContext> seed = null,
        Action<DbContext> clean = null)
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

    public virtual string NormalizeDelimitersInRawString(string sql)
        => sql.Replace("[", OpenDelimiter).Replace("]", CloseDelimiter);

    public virtual FormattableString NormalizeDelimitersInInterpolatedString(FormattableString sql)
        => new TestFormattableString(NormalizeDelimitersInRawString(sql.Format), sql.GetArguments());

    protected virtual string OpenDelimiter
        => "\"";

    protected virtual string CloseDelimiter
        => "\"";
}
