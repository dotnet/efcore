// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using SQLitePCL;
using Xunit;

namespace Microsoft.Data.Sqlite;

[Collection(nameof(SqliteConnectionFactoryTest))]
[CollectionDefinition(nameof(SqliteConnectionFactoryTest), DisableParallelization = true)]
public class SqliteConnectionFactoryTest : IDisposable
{
    private const string FileName = "pooled.db";
    private const string ConnectionString = "Data Source=" + FileName + ";Cache=Shared;Pooling=True";

    [Fact]
    public void Internal_connections_are_reused_after_reopen()
    {
        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();
        var db = connection.Handle;

        connection.Close();

        Assert.Null(connection.Handle);

        connection.Open();

        Assert.Same(db, connection.Handle);
    }

    [Fact]
    public void Internal_connections_are_reused_across_connections()
    {
        sqlite3 db;
        using (var connection1 = new SqliteConnection(ConnectionString))
        {
            connection1.Open();
            db = connection1.Handle!;
        }

        using var connection2 = new SqliteConnection(ConnectionString);
        connection2.Open();

        Assert.Same(db, connection2.Handle);
    }

    [Fact]
    public void Internal_connections_are_not_reused_when_pooling_is_disabled()
    {
        using var connection = new SqliteConnection(new SqliteConnectionStringBuilder(ConnectionString) { Pooling = false }.ToString());
        connection.Open();
        var db = connection.Handle;

        connection.Close();
        connection.Open();

        Assert.NotSame(db, connection.Handle);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Internal_connections_are_not_reused_after_clearing_pool(bool allPools)
    {
        using var connection = new SqliteConnection(ConnectionString);

        connection.Open();
        var db = connection.Handle;

        connection.Close();

        if (allPools)
        {
            SqliteConnection.ClearAllPools();
        }
        else
        {
            SqliteConnection.ClearPool(connection);
        }

        connection.Open();

        Assert.NotSame(db, connection.Handle);
    }

    [Fact]
    public void Can_clear_pools_while_connections_are_being_used()
    {
        const int threadCount = 20;
        var connectionStrings = Enumerable.Range(30, 30 + threadCount - 1)
            .Select(i => $"Data Source={FileName};Cache=Shared;Pooling=True;Command Timeout={i}").ToArray();

        var usingTasks = new Action[threadCount];
        for (var i = 0; i < threadCount; i++)
        {
            var captured = i;
            usingTasks[i] = () =>
            {
                for (var j = 0; j < 10000; j++)
                {
                    using (var connection = new SqliteConnection(connectionStrings[captured]))
                    {
                        connection.Open();
                        Task.Yield();
                        connection.Close();
                    }
                }
            };
        }

        for (int j = 0; j < 30; j++)
        {
            var runningTasks = usingTasks.Select(Task.Run).ToArray();

            for (var i = 0; i < 10000; i++)
            {
                SqliteConnection.ClearAllPools();
                Task.Yield();
            }

#pragma warning disable xUnit1031
            Task.WaitAll(runningTasks);
#pragma warning restore xUnit1031
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Internal_connections_are_not_reused_after_clearing_pool_when_open(bool allPools)
    {
        using var connection = new SqliteConnection(ConnectionString);

        connection.Open();
        var db = connection.Handle;

        if (allPools)
        {
            SqliteConnection.ClearAllPools();
        }
        else
        {
            SqliteConnection.ClearPool(connection);
        }

        Assert.Same(db, connection.Handle);

        connection.Close();
        connection.Open();

        Assert.NotSame(db, connection.Handle);
    }

    [Fact]
    public void ReadUncommitted_doesnt_bleed_across_connections()
    {
        sqlite3 db;
        using (var connection1 = new SqliteConnection(ConnectionString))
        {
            connection1.Open();
            db = connection1.Handle!;

            using (connection1.BeginTransaction(IsolationLevel.ReadUncommitted))
            {
            }
        }

        using var connection2 = new SqliteConnection(ConnectionString);
        connection2.Open();

        Assert.Same(db, connection2.Handle);
        var readUncommitted = connection2.ExecuteScalar<long>("PRAGMA read_uncommitted;");
        Assert.Equal(0L, readUncommitted);
    }

    [Fact]
    public void Functions_dont_bleed_across_connections()
    {
        sqlite3 db;
        using (var connection1 = new SqliteConnection(ConnectionString))
        {
            if (new Version(connection1.ServerVersion) < new Version(3, 31, 0))
            {
                // Skip. SQLite returns deleted functions
                return;
            }

            connection1.CreateFunction("function1", () => 1L, isDeterministic: true);
            connection1.CreateAggregate("aggregate1", 0L, x => x, isDeterministic: true);
            connection1.Open();
            db = connection1.Handle!;
        }

        using var connection2 = new SqliteConnection(ConnectionString);
        connection2.Open();

        Assert.Same(db, connection2.Handle);
        var functions = connection2.ExecuteScalar<string>("SELECT group_concat(name) FROM pragma_function_list;")
            .Split(',');
        Assert.DoesNotContain("function1", functions);
        Assert.DoesNotContain("aggregate1", functions);
    }

    [Fact(Skip = "ericsink/SQLitePCL.raw#421")]
    public void Collations_dont_bleed_across_connections()
    {
        sqlite3 db;
        using (var connection1 = new SqliteConnection(ConnectionString))
        {
            connection1.CreateCollation("COLLATION1", string.CompareOrdinal);
            connection1.Open();
            db = connection1.Handle!;
        }

        using var connection2 = new SqliteConnection(ConnectionString);
        connection2.Open();

        Assert.Same(db, connection2.Handle);
        var collations = connection2.ExecuteScalar<string>("SELECT group_concat(name) FROM pragma_collation_list;")
            .Split(',');
        Assert.DoesNotContain("COLLATION1", collations);
    }

    [Fact]
    public void EnableExtensions_doesnt_bleed_across_connections()
    {
        sqlite3 db;
        SqliteException ex;
        string disabledMessage;
        using (var connection1 = new SqliteConnection(ConnectionString))
        {
            connection1.Open();
            db = connection1.Handle!;

            var loadExtensionOmitted = connection1.ExecuteScalar<long>(
                "SELECT COUNT(*) FROM pragma_compile_options WHERE compile_options = 'OMIT_LOAD_EXTENSION';");
            if (loadExtensionOmitted != 0L)
            {
                return;
            }

            ex = Assert.Throws<SqliteException>(
                () => connection1.ExecuteNonQuery("SELECT load_extension('unknown');"));
            disabledMessage = ex.Message;

            connection1.EnableExtensions();
        }

        using var connection2 = new SqliteConnection(ConnectionString);
        connection2.Open();
        Assert.Same(db, connection2.Handle);

        ex = Assert.Throws<SqliteException>(
            () => connection2.ExecuteNonQuery("SELECT load_extension('unknown');"));
        Assert.Equal(disabledMessage, ex.Message);
    }

    [Fact]
    public void Clear_works_when_connection_leaked()
    {
        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();

        LeakConnection();
        GC.Collect();

        SqliteConnection.ClearPool(connection);

        [MethodImpl(MethodImplOptions.NoInlining)]
        static void LeakConnection()
        {
            // Don't add using!
            var connection = new SqliteConnection(ConnectionString);
            connection.Open();
        }
    }

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
        File.Delete(FileName);
    }
}
