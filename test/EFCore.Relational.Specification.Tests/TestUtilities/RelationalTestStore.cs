// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Data;

namespace Microsoft.EntityFrameworkCore.TestUtilities;

public abstract class RelationalTestStore(string name, bool shared, DbConnection connection) : TestStore(name, shared)
{
    public virtual string ConnectionString { get; } = connection.ConnectionString;

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

    protected virtual DbConnection Connection { get; } = connection;

    public override async Task<TestStore> InitializeAsync(
        IServiceProvider? serviceProvider,
        Func<DbContext>? createContext,
        Func<DbContext, Task>? seed = null,
        Func<DbContext, Task>? clean = null)
    {
        await base.InitializeAsync(serviceProvider, createContext, seed, clean);

        if (ConnectionState != ConnectionState.Open)
        {
            await OpenConnectionAsync();
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
