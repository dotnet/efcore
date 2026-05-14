// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Globalization;
using System.IO;
using Microsoft.Data.Sqlite;
using Xunit;

namespace Microsoft.Data.Sqlite.Upgrade.Tests;

public class UpgradeReadTests
{
    static UpgradeReadTests()
    {
        SQLitePCL.Batteries_V2.Init();
    }

    private static SqliteConnection OpenExisting(string fileName)
    {
        UpgradeDbFiles.EnsureExists(fileName);
        var connection = new SqliteConnection("Data Source=" + UpgradeDbFiles.GetPath(fileName));
        connection.Open();
        return connection;
    }

    [Fact]
    public void Default_encoding_seed()
    {
        using var connection = OpenExisting("default-encoding.db");

        var encoding = connection.ExecuteScalar<string>("PRAGMA encoding;");
        Assert.Equal("UTF-8", encoding);

        var value = connection.ExecuteScalar<string>("SELECT V FROM T WHERE Id = 1;");
        Assert.Equal("héllo — 你好", value);
    }

    [Fact]
    public void Wal_journal_mode_seed()
    {
        using var connection = OpenExisting("wal.db");

        var mode = connection.ExecuteScalar<string>("PRAGMA journal_mode;");
        Assert.Equal("wal", mode, ignoreCase: true);

        var value = connection.ExecuteScalar<string>("SELECT V FROM T WHERE Id = 1;");
        Assert.Equal("walrus", value);

        // Write through the upgraded bundle to ensure WAL is still usable.
        connection.ExecuteNonQuery("INSERT INTO T (V) VALUES ('post-upgrade');");
        var count = connection.ExecuteScalar<long>("SELECT COUNT(*) FROM T;");
        Assert.Equal(2, count);
    }

    [Fact]
    public void Fts5_seed()
    {
        using var connection = OpenExisting("fts5.db");

        var hit = connection.ExecuteScalar<long>("SELECT rowid FROM Docs WHERE Docs MATCH 'fox';");
        Assert.Equal(1, hit);

        var sphinx = connection.ExecuteScalar<long>("SELECT rowid FROM Docs WHERE Docs MATCH 'sphinx';");
        Assert.Equal(3, sphinx);
    }

    [Fact]
    public void Json1_seed()
    {
        using var connection = OpenExisting("json1.db");

        var name = connection.ExecuteScalar<string>("SELECT json_extract(Doc, '$.name') FROM T WHERE Id = 1;");
        Assert.Equal("Ada", name);

        var age = connection.ExecuteScalar<long>("SELECT json_extract(Doc, '$.age') FROM T WHERE Id = 1;");
        Assert.Equal(36, age);
    }

    [Fact]
    public void Rtree_seed()
    {
        using var connection = OpenExisting("rtree.db");

        var count = connection.ExecuteScalar<long>(
            "SELECT COUNT(*) FROM Boxes WHERE minX <= 1 AND maxX >= 0 AND minY <= 1 AND maxY >= 0;");
        Assert.Equal(2, count);
    }

    [Fact]
    public void Math_functions_seed()
    {
        using var connection = OpenExisting("math.db");

        // Stored values from the e_sqlite3 build.
        var cos = connection.ExecuteScalar<double>("SELECT Cos FROM T WHERE Id = 1;");
        var sin = connection.ExecuteScalar<double>("SELECT Sin FROM T WHERE Id = 1;");
        var log = connection.ExecuteScalar<double>("SELECT Log FROM T WHERE Id = 1;");
        Assert.Equal(1.0, cos);
        Assert.Equal(0.0, sin);
        Assert.Equal(0.0, log);

        // Recompute under SQLite3MC to confirm math functions are still exposed.
        var cosNow = connection.ExecuteScalar<double>("SELECT cos(0);");
        var sinNow = connection.ExecuteScalar<double>("SELECT sin(0);");
        var logNow = connection.ExecuteScalar<double>("SELECT log(1);");
        Assert.Equal(1.0, cosNow);
        Assert.Equal(0.0, sinNow);
        Assert.Equal(0.0, logNow);
    }

    [Fact]
    public void Foreign_keys_seed()
    {
        using var connection = OpenExisting("foreign-keys.db");

        // PRAGMA foreign_keys is a per-connection setting; it must be re-enabled on each open.
        connection.ExecuteNonQuery("PRAGMA foreign_keys = ON;");
        Assert.Equal(1L, connection.ExecuteScalar<long>("PRAGMA foreign_keys;"));

        var ex = Assert.Throws<SqliteException>(
            () => connection.ExecuteNonQuery("INSERT INTO Child (Id, ParentId) VALUES (11, 999);"));
        Assert.Equal(787 /* SQLITE_CONSTRAINT_FOREIGNKEY */, ex.SqliteExtendedErrorCode);
    }

    [Fact]
    public void User_version_seed()
    {
        using var connection = OpenExisting("user-version.db");

        Assert.Equal(42L, connection.ExecuteScalar<long>("PRAGMA user_version;"));
        Assert.Equal(51966L, connection.ExecuteScalar<long>("PRAGMA application_id;"));
    }

    [Fact]
    public void Generated_columns_seed()
    {
        using var connection = OpenExisting("generated-columns.db");

        var sumV = connection.ExecuteScalar<long>("SELECT SumVirtual FROM T WHERE Id = 1;");
        var sumS = connection.ExecuteScalar<long>("SELECT SumStored FROM T WHERE Id = 1;");
        Assert.Equal(5, sumV);
        Assert.Equal(5, sumS);

        // Recompute on update.
        connection.ExecuteNonQuery("UPDATE T SET A = 10, B = 20 WHERE Id = 1;");
        Assert.Equal(30L, connection.ExecuteScalar<long>("SELECT SumVirtual FROM T WHERE Id = 1;"));
        Assert.Equal(30L, connection.ExecuteScalar<long>("SELECT SumStored FROM T WHERE Id = 1;"));
    }

    [Fact]
    public void Attach_database_seed()
    {
        UpgradeDbFiles.EnsureExists("attach-aux.db");
        using var connection = OpenExisting("attach-main.db");

        Assert.Equal("main-row", connection.ExecuteScalar<string>("SELECT V FROM Main WHERE Id = 1;"));

        var auxPath = UpgradeDbFiles.GetPath("attach-aux.db").Replace("'", "''");
        connection.ExecuteNonQuery($"ATTACH DATABASE '{auxPath}' AS aux;");
        try
        {
            Assert.Equal("aux-row", connection.ExecuteScalar<string>("SELECT V FROM aux.Aux WHERE Id = 1;"));
        }
        finally
        {
            connection.ExecuteNonQuery("DETACH DATABASE aux;");
        }
    }

    [Fact]
    public void Collation_nocase_seed()
    {
        using var connection = OpenExisting("collation-nocase.db");

        Assert.Equal(1L, connection.ExecuteScalar<long>("SELECT COUNT(*) FROM T WHERE V = 'apple';"));
        Assert.Equal(1L, connection.ExecuteScalar<long>("SELECT COUNT(*) FROM T WHERE V = 'BANANA';"));
        Assert.Equal(1L, connection.ExecuteScalar<long>("SELECT COUNT(*) FROM T WHERE V = 'carrot';"));
    }

    [Fact]
    public void Triggers_seed()
    {
        using var connection = OpenExisting("triggers.db");

        Assert.Equal(1L, connection.ExecuteScalar<long>("SELECT COUNT(*) FROM Audit;"));

        // New insert under SQLite3MC should still fire the trigger persisted by the seed stage.
        connection.ExecuteNonQuery("INSERT INTO T (V) VALUES ('two');");
        Assert.Equal(2L, connection.ExecuteScalar<long>("SELECT COUNT(*) FROM Audit;"));

        var lastOp = connection.ExecuteScalar<string>("SELECT Op FROM Audit ORDER BY Id DESC LIMIT 1;");
        var lastV = connection.ExecuteScalar<string>("SELECT V FROM Audit ORDER BY Id DESC LIMIT 1;");
        Assert.Equal("INSERT", lastOp);
        Assert.Equal("two", lastV);
    }
}
