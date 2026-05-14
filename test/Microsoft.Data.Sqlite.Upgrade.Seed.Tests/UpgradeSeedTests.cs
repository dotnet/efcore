// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Globalization;
using System.IO;
using System.Linq;
using Microsoft.Data.Sqlite;
using Xunit;

namespace Microsoft.Data.Sqlite.Upgrade.Tests;

public class UpgradeSeedTests
{
    static UpgradeSeedTests()
    {
        SQLitePCL.Batteries_V2.Init();
        UpgradeDbFiles.EnsureDirectory();
    }

    private static SqliteConnection OpenFresh(string fileName)
    {
        UpgradeDbFiles.Delete(fileName);
        var connection = new SqliteConnection("Data Source=" + UpgradeDbFiles.GetPath(fileName));
        connection.Open();
        return connection;
    }

    [Fact]
    public void Default_encoding_seed()
    {
        using var connection = OpenFresh("default-encoding.db");

        connection.ExecuteNonQuery(
            """
            CREATE TABLE T (Id INTEGER PRIMARY KEY, V TEXT NOT NULL);
            INSERT INTO T (V) VALUES ('héllo — 你好');
            """);

        var encoding = connection.ExecuteScalar<string>("PRAGMA encoding;");
        Assert.Equal("UTF-8", encoding);
    }

    [Fact]
    public void Wal_journal_mode_seed()
    {
        using var connection = OpenFresh("wal.db");

        var mode = connection.ExecuteScalar<string>("PRAGMA journal_mode=WAL;");
        Assert.Equal("wal", mode, ignoreCase: true);

        connection.ExecuteNonQuery(
            """
            CREATE TABLE T (Id INTEGER PRIMARY KEY, V TEXT NOT NULL);
            INSERT INTO T (V) VALUES ('walrus');
            """);

        // Force a checkpoint so the journal_mode persists predictably.
        connection.ExecuteNonQuery("PRAGMA wal_checkpoint(TRUNCATE);");
    }

    [Fact]
    public void Fts5_seed()
    {
        using var connection = OpenFresh("fts5.db");

        connection.ExecuteNonQuery(
            """
            CREATE VIRTUAL TABLE Docs USING fts5(body);
            INSERT INTO Docs (rowid, body) VALUES
                (1, 'the quick brown fox'),
                (2, 'jumps over the lazy dog'),
                (3, 'sphinx of black quartz');
            """);

        var hit = connection.ExecuteScalar<long>("SELECT rowid FROM Docs WHERE Docs MATCH 'fox';");
        Assert.Equal(1, hit);
    }

    [Fact]
    public void Json1_seed()
    {
        using var connection = OpenFresh("json1.db");

        connection.ExecuteNonQuery(
            """
            CREATE TABLE T (Id INTEGER PRIMARY KEY, Doc TEXT NOT NULL);
            INSERT INTO T (Doc) VALUES ('{"name":"Ada","age":36}');
            """);

        var name = connection.ExecuteScalar<string>("SELECT json_extract(Doc, '$.name') FROM T WHERE Id = 1;");
        Assert.Equal("Ada", name);
    }

    [Fact]
    public void Rtree_seed()
    {
        using var connection = OpenFresh("rtree.db");

        connection.ExecuteNonQuery(
            """
            CREATE VIRTUAL TABLE Boxes USING rtree(id, minX, maxX, minY, maxY);
            INSERT INTO Boxes VALUES (1, 0, 1, 0, 1);
            INSERT INTO Boxes VALUES (2, 5, 6, 5, 6);
            INSERT INTO Boxes VALUES (3, 0.5, 1.5, 0.5, 1.5);
            """);

        var count = connection.ExecuteScalar<long>(
            "SELECT COUNT(*) FROM Boxes WHERE minX <= 1 AND maxX >= 0 AND minY <= 1 AND maxY >= 0;");
        Assert.Equal(2, count);
    }

    [Fact]
    public void Math_functions_seed()
    {
        using var connection = OpenFresh("math.db");

        connection.ExecuteNonQuery(
            """
            CREATE TABLE T (Id INTEGER PRIMARY KEY, Cos REAL NOT NULL, Sin REAL NOT NULL, Log REAL NOT NULL);
            INSERT INTO T (Cos, Sin, Log) VALUES (cos(0), sin(0), log(1));
            """);

        var cos = connection.ExecuteScalar<double>("SELECT Cos FROM T WHERE Id = 1;");
        var sin = connection.ExecuteScalar<double>("SELECT Sin FROM T WHERE Id = 1;");
        var log = connection.ExecuteScalar<double>("SELECT Log FROM T WHERE Id = 1;");
        Assert.Equal(1.0, cos);
        Assert.Equal(0.0, sin);
        Assert.Equal(0.0, log);
    }

    [Fact]
    public void Foreign_keys_seed()
    {
        using var connection = OpenFresh("foreign-keys.db");

        connection.ExecuteNonQuery(
            """
            PRAGMA foreign_keys = ON;
            CREATE TABLE Parent (Id INTEGER PRIMARY KEY);
            CREATE TABLE Child (
                Id INTEGER PRIMARY KEY,
                ParentId INTEGER NOT NULL REFERENCES Parent(Id));
            INSERT INTO Parent (Id) VALUES (1);
            INSERT INTO Child (Id, ParentId) VALUES (10, 1);
            """);

        var parentId = connection.ExecuteScalar<long>("SELECT ParentId FROM Child WHERE Id = 10;");
        Assert.Equal(1, parentId);
    }

    [Fact]
    public void User_version_seed()
    {
        using var connection = OpenFresh("user-version.db");

        connection.ExecuteNonQuery(
            """
            PRAGMA user_version = 42;
            PRAGMA application_id = 51966;
            """);

        Assert.Equal(42L, connection.ExecuteScalar<long>("PRAGMA user_version;"));
        Assert.Equal(51966L, connection.ExecuteScalar<long>("PRAGMA application_id;"));
    }

    [Fact]
    public void Generated_columns_seed()
    {
        using var connection = OpenFresh("generated-columns.db");

        connection.ExecuteNonQuery(
            """
            CREATE TABLE T (
                Id INTEGER PRIMARY KEY,
                A INTEGER NOT NULL,
                B INTEGER NOT NULL,
                SumVirtual INTEGER GENERATED ALWAYS AS (A + B) VIRTUAL,
                SumStored INTEGER GENERATED ALWAYS AS (A + B) STORED);
            INSERT INTO T (A, B) VALUES (2, 3);
            """);

        var sumV = connection.ExecuteScalar<long>("SELECT SumVirtual FROM T WHERE Id = 1;");
        var sumS = connection.ExecuteScalar<long>("SELECT SumStored FROM T WHERE Id = 1;");
        Assert.Equal(5, sumV);
        Assert.Equal(5, sumS);
    }

    [Fact]
    public void Attach_database_seed()
    {
        UpgradeDbFiles.Delete("attach-aux.db");

        using var connection = OpenFresh("attach-main.db");

        connection.ExecuteNonQuery(
            """
            CREATE TABLE Main (Id INTEGER PRIMARY KEY, V TEXT NOT NULL);
            INSERT INTO Main (V) VALUES ('main-row');
            """);

        var auxPath = UpgradeDbFiles.GetPath("attach-aux.db").Replace("'", "''");
        connection.ExecuteNonQuery(
            $"""
             ATTACH DATABASE '{auxPath}' AS aux;
             CREATE TABLE aux.Aux (Id INTEGER PRIMARY KEY, V TEXT NOT NULL);
             INSERT INTO aux.Aux (V) VALUES ('aux-row');
             DETACH DATABASE aux;
             """);
    }

    [Fact]
    public void Collation_nocase_seed()
    {
        using var connection = OpenFresh("collation-nocase.db");

        connection.ExecuteNonQuery(
            """
            CREATE TABLE T (Id INTEGER PRIMARY KEY, V TEXT NOT NULL COLLATE NOCASE);
            INSERT INTO T (V) VALUES ('Apple'), ('banana'), ('CARROT');
            """);

        var count = connection.ExecuteScalar<long>("SELECT COUNT(*) FROM T WHERE V = 'apple';");
        Assert.Equal(1, count);
    }

    [Fact]
    public void Triggers_seed()
    {
        using var connection = OpenFresh("triggers.db");

        connection.ExecuteNonQuery(
            """
            CREATE TABLE T (Id INTEGER PRIMARY KEY, V TEXT NOT NULL);
            CREATE TABLE Audit (Id INTEGER PRIMARY KEY AUTOINCREMENT, Op TEXT NOT NULL, V TEXT NOT NULL);
            CREATE TRIGGER T_ai AFTER INSERT ON T BEGIN
                INSERT INTO Audit (Op, V) VALUES ('INSERT', NEW.V);
            END;
            INSERT INTO T (V) VALUES ('one');
            """);

        var auditCount = connection.ExecuteScalar<long>("SELECT COUNT(*) FROM Audit;");
        Assert.Equal(1, auditCount);
    }
}
