// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Data.Common;
using Microsoft.Data.Sqlite;

namespace Microsoft.EntityFrameworkCore.Query;

public class BooleanPredicateIndexSqliteTest
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Sparse_boolean_count_uses_index(bool async)
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var context = CreateContext(connection);
        await context.Database.EnsureCreatedAsync();
        await context.Database.ExecuteSqlRawAsync(
            """
WITH RECURSIVE n(x) AS (SELECT 1 UNION ALL SELECT x+1 FROM n WHERE x<100000)
INSERT INTO Rows (Id, Flag, Enabled)
SELECT x, CASE WHEN x%100 = 0 THEN CASE x%3 WHEN 0 THEN 1 WHEN 1 THEN 2 ELSE -1 END ELSE 0 END, 0 FROM n;
ANALYZE;
""");

        var query = context.Rows.AsNoTracking().Where(r => r.Flag == true);
        Assert.Equal(1000, async ? await query.CountAsync() : query.Count());
        var sql = context.LastSql;

        await using var command = connection.CreateCommand();
        command.CommandText = "EXPLAIN QUERY PLAN " + sql;
        await using var reader = await command.ExecuteReaderAsync();
        var details = new List<string>();
        while (await reader.ReadAsync())
        {
            details.Add(reader.GetString(3));
        }

        Assert.Equal(2, details.Count(p => p.StartsWith("SEARCH r USING COVERING INDEX IX_Rows_Flag", StringComparison.Ordinal)));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Indexed_boolean_preserves_noncanonical_values(bool async)
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var context = CreateContext(connection);
        await Seed(context);

        var query = context.Rows.Where(r => r.Flag == true).Select(r => r.Id);
        Assert.Equal(new[] { 2, 3, 4, 6, 7, 10, 12 }, await ReadIds(query, async));
        Assert.Equal(
            """
SELECT "r"."Id"
FROM "Rows" AS "r"
WHERE "r"."Flag" > 0 AND "r"."Flag"
UNION ALL
SELECT "r"."Id"
FROM "Rows" AS "r"
WHERE "r"."Flag" < 0
""", query.ToQueryString(), ignoreLineEndingDifferences: true);

        var enabledQuery = context.Rows.Where(r => r.Enabled).Select(r => r.Id);
        Assert.Equal(new[] { 2, 3, 4, 6, 7, 10, 12 }, await ReadIds(enabledQuery, async));
        Assert.Contains("UNION ALL", enabledQuery.ToQueryString());
    }

    [Theory]
    [InlineData(false, 1)]
    [InlineData(true, 1)]
    [InlineData(false, 99)]
    [InlineData(true, 99)]
    public async Task Covering_entity_list_uses_index(bool async, int percent)
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var context = CreateContext(connection, "covering");
        await context.Database.EnsureCreatedAsync();
        await context.Database.ExecuteSqlRawAsync(
            """
WITH RECURSIVE n(x) AS (SELECT 1 UNION ALL SELECT x+1 FROM n WHERE x<100000)
INSERT INTO Rows (Id, Flag, Enabled)
SELECT x, CASE WHEN x%100 < {0} THEN CASE x%3 WHEN 0 THEN 1 WHEN 1 THEN 2 ELSE -1 END ELSE 0 END, 0 FROM n;
ANALYZE;
""", percent);
        var query = context.Rows.AsNoTracking().Where(r => r.Flag == true);
        var rows = async ? await query.ToListAsync() : query.ToList();
        Assert.Equal(Enumerable.Range(1, 100000).Where(i => i % 100 < percent), rows.Select(r => r.Id).Order());
        Assert.All(rows, r => Assert.True(r.Flag));
        var sql = context.LastSql;
        Assert.Contains("UNION ALL", sql);
        await using var command = connection.CreateCommand();
        command.CommandText = "EXPLAIN QUERY PLAN " + sql;
        await using var reader = await command.ExecuteReaderAsync();
        var details = new List<string>();
        while (await reader.ReadAsync())
        {
            details.Add(reader.GetString(3));
        }

        Assert.Equal(2, details.Count(p => p.StartsWith("SEARCH r USING COVERING INDEX IX_Rows_Flag_Enabled", StringComparison.Ordinal)));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Covering_entity_list_preserves_storage_values_and_aliases(bool async)
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var context = CreateContext(connection, "covering");
        await Seed(context);
        var baseline = context.Rows.FromSqlRaw("SELECT * FROM Rows WHERE Flag").AsNoTracking();
        var expected = async ? await baseline.ToListAsync() : baseline.ToList();
        var query = context.Rows.AsNoTracking().Where(r => r.Flag == true);
        var actual = async ? await query.ToListAsync() : query.ToList();
        Assert.Equal(expected.OrderBy(r => r.Id).Select(r => (r.Id, r.Flag, r.Enabled)),
            actual.OrderBy(r => r.Id).Select(r => (r.Id, r.Flag, r.Enabled)));
        Assert.Contains("UNION ALL", context.LastSql);
        var projected = query.Select(r => new { A = r.Flag, B = r.Id });
        var aliases = async ? await projected.ToArrayAsync() : projected.ToArray();
        Assert.Equal(expected.OrderBy(r => r.Id).Select(r => (r.Flag, r.Id)), aliases.OrderBy(r => r.B).Select(r => (r.A, r.B)));
        Assert.Contains("UNION ALL", context.LastSql);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Noncovering_list_preserves_values_and_bounded_list_keeps_order(bool async)
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var context = CreateContext(connection);
        await Seed(context);
        var query = context.Rows.Where(r => r.Flag == true);
        var scalarCommands = context.ScalarCommands;
        var rows = async ? await query.ToListAsync() : query.ToList();
        Assert.Equal(new[] { 2, 3, 4, 6, 7, 10, 12 }, rows.Select(r => r.Id).Order());
        Assert.Equal(scalarCommands + 1, context.ScalarCommands);
        Assert.DoesNotContain("CROSS JOIN", context.LastSql);
        Assert.Contains("CROSS JOIN", query.ToQueryString());
        var page = query.OrderBy(r => r.Id).Select(r => r.Id).Skip(2).Take(3);
        Assert.Equal(new[] { 4, 6, 7 }, async ? await page.ToArrayAsync() : page.ToArray());
        Assert.Equal(scalarCommands + 1, context.ScalarCommands);
        Assert.DoesNotContain("UNION ALL", context.LastSql);
    }

    [Theory]
    [InlineData(10000, "indexed", false)]
    [InlineData(200000, "indexed", false)]
    [InlineData(200000, "nonrowid", false)]
    [InlineData(200000, "indexed", true)]
    [InlineData(200000, "nonrowid", true)]
    public async Task Noncovering_stream_bounds_work_before_first_row(int rowCount, string shape, bool includeZeros)
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var context = CreateContext(connection, shape);
        await context.Database.EnsureCreatedAsync();
        await context.Database.ExecuteSqlRawAsync(
            """
WITH RECURSIVE n(x) AS (SELECT 1 UNION ALL SELECT x+1 FROM n WHERE x<{0})
INSERT INTO Rows (Id, Flag, Enabled) SELECT x, CASE WHEN {1} AND x%4 = 0 THEN 0 WHEN x%2 = 0 THEN 2 ELSE -1 END, 0 FROM n;
""", rowCount, includeZeros);

        // Include the index probe and the reader through early disposal.
        var steps = 0;
        SQLitePCL.raw.sqlite3_progress_handler(connection.Handle!, 10, _ =>
        {
            steps += 10;
            return 0;
        }, null);
        try
        {
            var query = context.Rows.AsNoTracking().Where(r => r.Flag == true);
            await using var enumerator = query.AsAsyncEnumerable().GetAsyncEnumerator();
            Assert.True(await enumerator.MoveNextAsync());
            Assert.True(enumerator.Current.Flag);
        }
        finally
        {
            SQLitePCL.raw.sqlite3_progress_handler(connection.Handle!, 0, null, null);
        }

        Assert.InRange(steps, 0, includeZeros ? 15000 : 500);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Noncovering_list_switches_access_path_after_data_changes(bool async)
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var context = CreateContext(connection);
        await Seed(context);
        await context.Database.ExecuteSqlRawAsync(
            """
WITH RECURSIVE n(x) AS (SELECT 14 UNION ALL SELECT x+1 FROM n WHERE x<10000)
INSERT INTO Rows (Id, Flag, Enabled) SELECT x, 0, x%2 FROM n;
ANALYZE;
""");

        var query = context.Rows.AsNoTracking().Where(r => r.Flag == true).TagWith("boolean index");
        var initialSql = query.ToQueryString();
        Assert.Contains("boolean index", initialSql);
        foreach (var dense in new[] { false, true })
        {
            if (dense)
            {
                await context.Database.ExecuteSqlRawAsync("UPDATE Rows SET Flag = CASE Id%2 WHEN 0 THEN 2 ELSE -1 END WHERE Id >= 14 AND Id%4 <> 0");
            }

            var reference = context.Rows.FromSqlRaw("SELECT * FROM Rows WHERE Flag").AsNoTracking();
            var expected = await reference.ToListAsync();
            var actual = async ? await query.ToListAsync() : query.ToList();
            Assert.Equal(expected.OrderBy(r => r.Id).Select(r => (r.Id, r.Flag, r.Enabled)),
                actual.OrderBy(r => r.Id).Select(r => (r.Id, r.Flag, r.Enabled)));
            Assert.Equal(initialSql.TrimEnd(), context.LastSql.TrimEnd());

            await using var command = connection.CreateCommand();
            command.CommandText = context.LastSql;
            await using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
            }

            var steps = SQLitePCL.raw.sqlite3_stmt_status(reader.Handle!, SQLitePCL.raw.SQLITE_STMTSTATUS_FULLSCAN_STEP, 0);
            // EXPLAIN lists both arms. The execution counter distinguishes the arm which actually read the table.
            Assert.InRange(steps, dense ? 9999 : 0, dense ? int.MaxValue : 100);
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Noncovering_boolean_with_quoted_column_and_colliding_table_name(bool async)
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var context = CreateContext(connection, "quoted");
        await context.Database.EnsureCreatedAsync();
        await context.Database.ExecuteSqlRawAsync(
            """
INSERT INTO "EF_DENSITY" ("Id", "some""flag", "Flag") VALUES (1, 2, 0), (2, -1, 1), (3, 0, 0);
""");
        var query = context.Rows.Where(r => r.Enabled);
        var actual = async ? await query.ToListAsync() : query.ToList();
        Assert.Equal(new[] { 1, 2 }, actual.Select(r => r.Id).Order());
        Assert.All(actual, r => Assert.True(r.Enabled));
        Assert.False(actual.Single(r => r.Id == 1).Flag);
        Assert.True(actual.Single(r => r.Id == 2).Flag);
        var standaloneSql = query.ToQueryString();
        Assert.StartsWith("WITH \"ef_density_\"", standaloneSql);
        Assert.Contains("\"some\"\"flag\"", context.LastSql);
        var standaloneRows = await context.Rows.FromSqlRaw(standaloneSql).AsNoTracking().ToListAsync();
        Assert.Equal(new[] { 1, 2 }, standaloneRows.Select(r => r.Id).Order());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Nonrowid_primary_key_is_not_treated_as_covering(bool async)
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var context = CreateContext(connection, "nonrowid");
        await Seed(context);
        var query = context.Rows.Where(r => r.Flag == true).Select(r => r.Id);
        Assert.Equal(new[] { 2, 3, 4, 6, 7, 10, 12 }, await ReadIds(query, async));
        Assert.Contains("CROSS JOIN", query.ToQueryString());
        Assert.DoesNotContain("CROSS JOIN", context.LastSql);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Noncovering_list_rechecks_zero_and_null_entries_across_transaction_and_rollback(bool async)
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var context = CreateContext(connection);
        await context.Database.EnsureCreatedAsync();
        await context.Database.ExecuteSqlRawAsync(
            """
WITH RECURSIVE n(x) AS (SELECT 1 UNION ALL SELECT x+1 FROM n WHERE x<1024)
INSERT INTO Rows (Id, Flag, Enabled) SELECT x, CASE WHEN x%2 = 0 THEN 0 END, 0 FROM n;
INSERT INTO Rows (Id, Flag, Enabled) VALUES (1025,-1,0);
""");

        var query = context.Rows.AsNoTracking().Where(r => r.Flag == true).TagWith("boundary");
        async Task Check(bool sampleDensity, int[] ids)
        {
            var rows = async ? await query.ToListAsync() : query.ToList();
            Assert.Equal(ids, rows.Select(r => r.Id).Order());
            Assert.Contains("boundary", context.LastSql);
            Assert.Contains("boundary", context.LastScalarSql);
            Assert.Equal(sampleDensity, context.LastSql.Contains("CROSS JOIN", StringComparison.Ordinal));
        }

        await Check(false, [1025]);
        await using (var transaction = await context.Database.BeginTransactionAsync())
        {
            await context.Database.ExecuteSqlRawAsync("INSERT INTO Rows (Id,Flag,Enabled) VALUES (1026,NULL,0), (1027,2,0)");
            await Check(true, [1025, 1027]);
            await transaction.RollbackAsync();
        }

        await Check(false, [1025]);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Index_probe_releases_connection_on_failure_and_next_query_can_retry(bool async)
    {
        var connectionString = $"Data Source={Guid.NewGuid():N};Mode=Memory;Cache=Shared;Pooling=False";
        await using var anchor = new SqliteConnection(connectionString);
        await anchor.OpenAsync();
        await using var connection = new SqliteConnection(connectionString);
        await using var context = CreateContext(connection);
        var query = context.Rows.AsNoTracking().Where(r => r.Flag == true);

        if (async)
        {
            await Assert.ThrowsAsync<SqliteException>(() => query.ToListAsync());
        }
        else
        {
            Assert.Throws<SqliteException>(() => query.ToList());
        }

        Assert.Equal(System.Data.ConnectionState.Closed, connection.State);
        await Seed(context);
        var rows = async ? await query.ToListAsync() : query.ToList();
        Assert.Equal(new[] { 2, 3, 4, 6, 7, 10, 12 }, rows.Select(r => r.Id).Order());
        Assert.Equal(System.Data.ConnectionState.Closed, connection.State);
    }

    [Fact]
    public async Task Compiled_covering_list_reads_changed_data()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var context = CreateContext(connection);
        await Seed(context);
        var query = EF.CompileQuery((IndexContext db) => db.Rows.Where(r => r.Flag == true).Select(r => r.Id));
        Assert.Equal(new[] { 2, 3, 4, 6, 7, 10, 12 }, query(context).Order());
        Assert.Contains("UNION ALL", context.LastSql);
        await context.Database.ExecuteSqlRawAsync("INSERT INTO Rows (Id,Flag,Enabled) VALUES (14,-2,-2)");
        Assert.Equal(new[] { 2, 3, 4, 6, 7, 10, 12, 14 }, query(context).Order());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Boolean_composition_and_negation_preserve_results(bool async)
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var context = CreateContext(connection);
        await Seed(context);

        Assert.Equal(new[] { 3, 4, 6, 7, 10, 12 }, await ReadIds(context.Rows.Where(r => r.Flag == true && r.Id > 2).Select(r => r.Id), async));
        Assert.Equal(new[] { 1, 2, 3, 4, 6, 7, 10, 12 }, await ReadIds(context.Rows.Where(r => r.Flag == true || r.Id == 1).Select(r => r.Id), async));
        Assert.Equal(new[] { 1, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13 }, await ReadIds(context.Rows.Where(r => r.Flag != true).Select(r => r.Id), async));
        Assert.Equal(new[] { 1, 11 }, await ReadIds(context.Rows.Where(r => r.Flag == false).Select(r => r.Id), async));
        Assert.Equal(new[] { 1, 5, 8, 9, 11, 13 }, await ReadIds(context.Rows.Where(r => !r.Enabled).Select(r => r.Id), async));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Join_predicate_preserves_cardinality(bool async)
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var context = CreateContext(connection);
        await Seed(context);

        var query = context.Rows.Join(context.Rows, r => true, r => r.Enabled, (left, right) => new { Left = left.Id, Right = right.Id });
        var rows = async ? await query.ToArrayAsync() : query.ToArray();
        Assert.Equal(
            Enumerable.Range(1, 13).SelectMany(left => new[] { 2, 3, 4, 6, 7, 10, 12 }.Select(right => (left, right))),
            rows.Select(r => (r.Left, r.Right)).Order());
        Assert.DoesNotContain(" > 0", query.ToQueryString());
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public async Task Count_preserves_noncanonical_values(bool async, bool longCount)
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var context = CreateContext(connection);
        await Seed(context);
        var query = context.Rows.Where(r => r.Flag == true);
        var count = longCount
            ? (async ? await query.LongCountAsync() : query.LongCount())
            : (async ? await query.CountAsync() : query.Count());
        Assert.Equal(7, count);
        Assert.Equal(
            """
SELECT (
    SELECT COUNT(*)
    FROM "Rows" AS "r"
    WHERE "r"."Flag" > 0 AND "r"."Flag") + (
    SELECT COUNT(*)
    FROM "Rows" AS "r"
    WHERE "r"."Flag" < 0)
""", context.LastSql, ignoreLineEndingDifferences: true);
        var enabled = context.Rows.Where(r => r.Enabled);
        Assert.Equal(7, async ? await enabled.CountAsync() : enabled.Count());
        Assert.Contains("\"Enabled\" > 0", context.LastSql);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Count_keeps_bounded_distinct_and_extra_predicates(bool async)
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var context = CreateContext(connection);
        await Seed(context);
        var query = context.Rows.Where(r => r.Flag == true);
        Assert.Equal(3, async ? await query.Take(3).CountAsync() : query.Take(3).Count());
        Assert.DoesNotContain(" > 0", context.LastSql);
        Assert.Equal(7, async ? await query.Select(r => r.Flag).Distinct().CountAsync() : query.Select(r => r.Flag).Distinct().Count());
        Assert.DoesNotContain(" > 0", context.LastSql);
        var filtered = query.Where(r => r.Id > 2);
        Assert.Equal(6, async ? await filtered.CountAsync() : filtered.Count());
        Assert.DoesNotContain(" > 0", context.LastSql);
        var negative = context.Rows.Where(r => r.Flag != true);
        Assert.Equal(12, async ? await negative.CountAsync() : negative.Count());
        Assert.DoesNotContain(" > 0", context.LastSql);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Count_keeps_grouped_and_joined_sources(bool async)
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var context = CreateContext(connection);
        await Seed(context);
        var groups = context.Rows.Where(r => r.Flag == true).GroupBy(r => r.Id).Select(g => g.Count());
        Assert.Equal(Enumerable.Repeat(1, 7), async ? await groups.ToArrayAsync() : groups.ToArray());
        Assert.DoesNotContain(" > 0", context.LastSql);
        var join = context.Rows.Join(context.Rows, r => true, r => r.Enabled, (left, right) => right.Id);
        Assert.Equal(91, async ? await join.CountAsync() : join.Count());
        Assert.DoesNotContain(" > 0", context.LastSql);
    }

    [Fact]
    public async Task Compiled_count_reads_changed_data()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var context = CreateContext(connection);
        await Seed(context);
        var count = EF.CompileQuery((IndexContext db) => db.Rows.Count(r => r.Flag == true));
        Assert.Equal(7, count(context));
        Assert.Contains("\"Flag\" > 0", context.LastSql);
        await context.Database.ExecuteSqlRawAsync("INSERT INTO Rows (Id,Flag,Enabled) VALUES (14,-2,-2)");
        Assert.Equal(8, count(context));
        var longCount = EF.CompileAsyncQuery((IndexContext db) => db.Rows.LongCount(r => r.Flag == true));
        Assert.Equal(8L, await longCount(context));
        Assert.Contains("\"Flag\" < 0", context.LastSql);
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public async Task Empty_and_all_null_sources(bool async, bool allNull)
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var context = CreateContext(connection);
        await context.Database.EnsureCreatedAsync();
        if (allNull)
        {
            await context.Database.ExecuteSqlRawAsync("INSERT INTO Rows (Id, Flag, Enabled) VALUES (1, NULL, 0), (2, NULL, 0)");
        }

        Assert.Empty(await ReadIds(context.Rows.Where(r => r.Flag == true).Select(r => r.Id), async));
        Assert.Equal(0, async ? await context.Rows.Where(r => r.Flag == true).CountAsync() : context.Rows.Where(r => r.Flag == true).Count());
    }

    [Theory]
    [InlineData("unindexed")]
    [InlineData("converted")]
    [InlineData("trailing")]
    [InlineData("partial")]
    public async Task Ineligible_columns_are_not_rewritten(string shape)
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var context = CreateContext(connection, shape);
        await Seed(context);

        var query = context.Rows.Where(r => r.Flag == true).Select(r => r.Id);
        Assert.DoesNotContain(" > 0", query.ToQueryString());
        Assert.Equal(shape == "converted" ? new[] { 2 } : new[] { 2, 3, 4, 6, 7, 10, 12 }, await ReadIds(query, async: true));
        Assert.Equal(shape == "converted" ? 1 : 7, await query.CountAsync());
        Assert.DoesNotContain(" > 0", context.LastSql);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Projection_is_unchanged(bool async)
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var context = CreateContext(connection);
        await context.Database.EnsureCreatedAsync();
        await context.Database.ExecuteSqlRawAsync("INSERT INTO Rows (Id, Flag, Enabled) VALUES (1, 0, 0), (2, 1, 1), (3, 2, 2), (4, -1, -1), (5, NULL, 0)");

        var query = context.Rows.OrderBy(r => r.Id).Select(r => r.Flag);
        Assert.DoesNotContain(" > 0", query.ToQueryString());
        Assert.Equal(new bool?[] { false, true, true, true, null }, async ? await query.ToArrayAsync() : query.ToArray());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Changing_boolean_parameter_preserves_cached_query_results(bool async)
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var context = CreateContext(connection);
        await Seed(context);

        foreach (var value in new[] { true, false, true })
        {
            var query = context.Rows.Where(r => r.Enabled == value).Select(r => r.Id);
            Assert.Equal(
                value ? new[] { 2 } : new[] { 1, 5, 11 },
                await ReadIds(query, async));
        }
    }

    [Fact]
    public async Task Compiled_query_preserves_changing_boolean_parameter_results()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var context = CreateContext(connection);
        await Seed(context);
        var query = EF.CompileQuery((IndexContext db, bool value) => db.Rows.Where(r => r.Enabled == value).Select(r => r.Id));

        foreach (var value in new[] { true, false, true })
        {
            Assert.Equal(
                value ? new[] { 2 } : new[] { 1, 5, 11 },
                query(context, value).Order());
        }
    }

    private static async Task<int[]> ReadIds(IQueryable<int> query, bool async)
        => (async ? await query.ToArrayAsync() : query.ToArray()).Order().ToArray();

    private static async Task Seed(IndexContext context)
    {
        await context.Database.EnsureCreatedAsync();
        await context.Database.ExecuteSqlRawAsync(
            """
INSERT INTO Rows (Id, Flag, Enabled) VALUES
(1,0,0),(2,1,1),(3,2,2),(4,-1,-1),(5,NULL,0),(6,0.5,0.5),(7,-0.5,-0.5),
(8,'abc','abc'),(9,'0abc','0abc'),(10,'1abc','1abc'),(11,'0','0'),(12,x'31',x'31'),(13,x'00',x'00')
""");
    }

    private static IndexContext CreateContext(SqliteConnection connection, string shape = "indexed")
    {
        var options = new DbContextOptionsBuilder().UseSqlite(connection).Options;
        return shape switch
        {
            "unindexed" => new UnindexedContext(options),
            "converted" => new ConvertedContext(options),
            "trailing" => new TrailingIndexContext(options),
            "partial" => new PartialIndexContext(options),
            "covering" => new CoveringContext(options),
            "nonrowid" => new NonrowidContext(options),
            "quoted" => new QuotedContext(options),
            _ => new IndexContext(options)
        };
    }

    public class IndexContext(DbContextOptions options) : DbContext(options)
    {
        private readonly CommandRecorder _commands = new();

        public DbSet<Row> Rows => Set<Row>();
        public int ScalarCommands => _commands.ScalarCommands;
        public string? LastScalarSql => _commands.LastScalarSql;
        public string LastSql => _commands.Sql ?? throw new InvalidOperationException("No query was executed.");

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.AddInterceptors(_commands);

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Row>().HasIndex(r => r.Flag);
            modelBuilder.Entity<Row>().HasIndex(r => r.Enabled);
        }
    }

    public class UnindexedContext(DbContextOptions options) : IndexContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<Row>();
    }

    public class ConvertedContext(DbContextOptions options) : IndexContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Row>().Property(r => r.Flag).HasConversion<int>();
        }
    }

    public class TrailingIndexContext(DbContextOptions options) : IndexContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<Row>().HasIndex(r => new { r.Id, r.Flag });
    }

    public class PartialIndexContext(DbContextOptions options) : IndexContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<Row>().HasIndex(r => r.Flag).HasFilter("Flag = 1");
    }

    public class CoveringContext(DbContextOptions options) : IndexContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<Row>().HasIndex(r => new { r.Flag, r.Enabled });
    }

    public class QuotedContext(DbContextOptions options) : IndexContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Row>().ToTable("EF_DENSITY").Property(r => r.Enabled).HasColumnName("some\"flag");
        }
    }

    public class NonrowidContext(DbContextOptions options) : IndexContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Row>().Property(r => r.Id).HasColumnType("INT").ValueGeneratedNever();
        }
    }

    public class Row
    {
        public int Id { get; set; }
        public bool? Flag { get; set; }
        public bool Enabled { get; set; }
    }

    private sealed class CommandRecorder : DbCommandInterceptor
    {
        public string? Sql { get; private set; }
        public int ScalarCommands { get; private set; }
        public string? LastScalarSql { get; private set; }

        public override InterceptionResult<object> ScalarExecuting(
            DbCommand command, CommandEventData eventData, InterceptionResult<object> result)
        {
            ScalarCommands++;
            LastScalarSql = command.CommandText;
            return result;
        }

        public override ValueTask<InterceptionResult<object>> ScalarExecutingAsync(
            DbCommand command, CommandEventData eventData, InterceptionResult<object> result, CancellationToken cancellationToken = default)
        {
            ScalarCommands++;
            LastScalarSql = command.CommandText;
            return ValueTask.FromResult(result);
        }

        public override InterceptionResult<DbDataReader> ReaderExecuting(
            DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result)
        {
            Sql = command.CommandText;
            return result;
        }

        public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
            DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result,
            CancellationToken cancellationToken = default)
        {
            Sql = command.CommandText;
            return ValueTask.FromResult(result);
        }
    }
}
