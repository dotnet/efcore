// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.SqlServer.TestModels;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore.SqlServer;

[SqlServerCondition(SqlServerCondition.SupportsSqlClr)]
public class QueryTests : IDisposable
{
    private readonly AbrahamicContext _db;

    public QueryTests()
    {
        _db = new AbrahamicContext();
        _db.Database.EnsureDeleted();
        _db.Database.EnsureCreated();
        _db.ClearSql();
    }

    [ConditionalFact]
    public void GetLevel_can_translate()
    {
        var results = (from p in _db.Patriarchy
                       where p.Id.GetLevel() == 0
                       select p.Name).ToList();

        Assert.Equal(
            Condense(@"SELECT [p].[Name] FROM [Patriarchy] AS [p] WHERE [p].[Id].GetLevel() = CAST(0 AS smallint)"),
            Condense(_db.Sql));

        Assert.Equal(new[] { "Abraham" }, results);
    }

    [ConditionalFact]
    public void IsDescendantOf_can_translate()
    {
        var results = (from p in _db.Patriarchy
                       where p.Id.GetLevel() == 3
                       select p.Id.IsDescendantOf(p.Id.GetAncestor(1))).ToList();

        Assert.Equal(
            Condense(
                @"SELECT [p].[Id].IsDescendantOf([p].[Id].GetAncestor(1)) FROM [Patriarchy] AS [p] WHERE [p].[Id].GetLevel() = CAST(3 AS smallint)"),
            Condense(_db.Sql));

        Assert.All(results, b => Assert.True(b));
    }

    [ConditionalFact]
    public void IsDescendantOf_can_translate_when_constant()
    {
        var results = (from p in _db.Patriarchy
                       where p.Id.GetLevel() == 1
                       select new HierarchyId("/1/1/11.1/").IsDescendantOf(p.Id)).ToList();

        Assert.Equal(
            Condense(
                @"SELECT hierarchyid::Parse('/1/1/11.1/').IsDescendantOf([p].[Id]) FROM [Patriarchy] AS [p] WHERE [p].[Id].GetLevel() = CAST(1 AS smallint)"),
            Condense(_db.Sql));

        Assert.All(results, b => Assert.True(b));
    }

    [ConditionalFact]
    public void GetAncestor_0_can_translate()
    {
        var results = (from p in _db.Patriarchy
                       where p.Id.GetLevel() == 0
                       select p.Id.GetAncestor(0)).ToList();

        Assert.Equal(
            Condense(@"SELECT [p].[Id].GetAncestor(0) FROM [Patriarchy] AS [p] WHERE [p].[Id].GetLevel() = CAST(0  AS smallint)"),
            Condense(_db.Sql));

        Assert.All(results, h => Assert.Equal(HierarchyId.GetRoot(), h));
    }

    [ConditionalFact]
    public void GetAncestor_1_can_translate()
    {
        var results = (from p in _db.Patriarchy
                       where p.Id.GetLevel() == 1
                       select p.Id.GetAncestor(1)).ToList();

        Assert.Equal(
            Condense(@"SELECT [p].[Id].GetAncestor(1) FROM [Patriarchy] AS [p] WHERE [p].[Id].GetLevel() = CAST(1 AS smallint)"),
            Condense(_db.Sql));

        Assert.All(results, h => Assert.Equal(HierarchyId.GetRoot(), h));
    }

    [ConditionalFact]
    public void GetAncestor_2_can_translate()
    {
        var results = (from p in _db.Patriarchy
                       where p.Id.GetLevel() == 2
                       select p.Id.GetAncestor(2)).ToList();

        Assert.Equal(
            Condense(@"SELECT [p].[Id].GetAncestor(2) FROM [Patriarchy] AS [p] WHERE [p].[Id].GetLevel() = CAST(2 AS smallint)"),
            Condense(_db.Sql));

        Assert.All(results, h => Assert.Equal(HierarchyId.GetRoot(), h));
    }

    [ConditionalFact]
    public void GetAncestor_3_can_translate()
    {
        var results = (from p in _db.Patriarchy
                       where p.Id.GetLevel() == 3
                       select p.Id.GetAncestor(3)).ToList();

        Assert.Equal(
            Condense(@"SELECT [p].[Id].GetAncestor(3) FROM [Patriarchy] AS [p] WHERE [p].[Id].GetLevel() = CAST(3 AS smallint)"),
            Condense(_db.Sql));

        Assert.All(results, h => Assert.Equal(HierarchyId.GetRoot(), h));
    }

    [ConditionalFact]
    public void GetAncestor_of_root_returns_null()
    {
        var results = (from p in _db.Patriarchy
                       where p.Id.GetLevel() == 0
                       select p.Id.GetAncestor(1)).ToList();

        Assert.Equal(
            Condense(@"SELECT [p].[Id].GetAncestor(1) FROM [Patriarchy] AS [p] WHERE [p].[Id].GetLevel() = CAST(0 AS smallint)"),
            Condense(_db.Sql));

        Assert.Equal(new HierarchyId[] { null }, results);
    }

    [ConditionalFact]
    public void GetDescendent_can_translate()
    {
        var results = (from p in _db.Patriarchy
                       where p.Id.GetLevel() == 0
                       select p.Id.GetDescendant(null, null)).ToList();

        Assert.Equal(
            Condense(@"SELECT [p].[Id].GetDescendant(NULL, NULL) FROM [Patriarchy] AS [p] WHERE [p].[Id].GetLevel() = CAST(0 AS smallint)"),
            Condense(_db.Sql));

        Assert.Equal(new[] { HierarchyId.Parse("/1/") }, results);
    }

    [ConditionalFact]
    public void GetDescendent_can_translate_when_one_argument()
    {
        var results = (from p in _db.Patriarchy
                       where p.Id.GetLevel() == 0
                       select p.Id.GetDescendant(null)).ToList();

        Assert.Equal(
            Condense(@"SELECT [p].[Id].GetDescendant(NULL, NULL) FROM [Patriarchy] AS [p] WHERE [p].[Id].GetLevel() = CAST(0 AS smallint)"),
            Condense(_db.Sql));

        Assert.Equal(new[] { HierarchyId.Parse("/1/") }, results);
    }

    [ConditionalFact]
    public void HierarchyId_can_be_sent_as_parameter()
    {
        var results = (from p in _db.Patriarchy
                       where p.Id == HierarchyId.Parse("/1/")
                       select p.Name).ToList();

        Assert.Equal(
            Condense(@"SELECT [p].[Name] FROM [Patriarchy] AS [p] WHERE [p].[Id] = hierarchyid::Parse('/1/')"),
            Condense(_db.Sql));

        Assert.Equal(new[] { "Isaac" }, results);
    }

    [ConditionalFact]
    public void Converted_HierarchyId_can_be_sent_as_parameter()
    {
        var results = (from p in _db.ConvertedPatriarchy
                       where p.HierarchyId == HierarchyId.Parse("/1/").ToString()
                       select p.Name).ToList();

        Assert.Equal(
            Condense(@"SELECT [c].[Name] FROM [ConvertedPatriarchy] AS [c] WHERE [c].[HierarchyId] = hierarchyid::Parse('/1/')"),
            Condense(_db.Sql));

        Assert.Equal(new[] { "Isaac" }, results);
    }

    [ConditionalFact]
    public void Can_insert_HierarchyId()
    {
        using (_db.Database.BeginTransaction())
        {
            var entities = new List<Patriarch>
            {
                new() { Id = HierarchyId.Parse("/2/1/"), Name = "Thrór" },
                new() { Id = HierarchyId.Parse("/2/2/"), Name = "Thráin II" },
                new() { Id = HierarchyId.Parse("/3/"), Name = "Thorin Oakenshield" }
            };

            _db.AddRange(entities);
            _db.SaveChanges();
            _db.ChangeTracker.Clear();

            var queried = _db.Patriarchy.Where(e => e.Name.StartsWith("Th")).OrderBy(e => e.Id).ToList();

            Assert.Equal(3, queried.Count);

            Assert.Equal(HierarchyId.Parse("/2/1/"), queried[0].Id);
            Assert.Equal("Thrór", queried[0].Name);

            Assert.Equal(HierarchyId.Parse("/2/2/"), queried[1].Id);
            Assert.Equal("Thráin II", queried[1].Name);

            Assert.Equal(HierarchyId.Parse("/3/"), queried[2].Id);
            Assert.Equal("Thorin Oakenshield", queried[2].Name);
        }
    }

    [ConditionalFact]
    public void Can_insert_and_update_converted_HierarchyId()
    {
        using (_db.Database.BeginTransaction())
        {
            var entities = new List<ConvertedPatriarch>
            {
                new() { HierarchyId = HierarchyId.Parse("/2/1/").ToString(), Name = "Thrór" },
                new() { HierarchyId = HierarchyId.Parse("/2/2/").ToString(), Name = "Thráin II" },
                new() { HierarchyId = HierarchyId.Parse("/3/").ToString(), Name = "Thorin Oakenshield" }
            };

            _db.AddRange(entities);
            _db.SaveChanges();
            _db.ChangeTracker.Clear();

            var queried = _db.ConvertedPatriarchy.Where(e => e.Name.StartsWith("Th")).OrderBy(e => e.Id).ToList();

            Assert.Equal(3, queried.Count);

            Assert.Equal(HierarchyId.Parse("/2/1/").ToString(), queried[0].HierarchyId);
            Assert.Equal("Thrór", queried[0].Name);

            Assert.Equal(HierarchyId.Parse("/2/2/").ToString(), queried[1].HierarchyId);
            Assert.Equal("Thráin II", queried[1].Name);

            Assert.Equal(HierarchyId.Parse("/3/").ToString(), queried[2].HierarchyId);
            Assert.Equal("Thorin Oakenshield", queried[2].Name);

            queried[2].HierarchyId = "/3/1/";

            _db.SaveChanges();
            _db.ChangeTracker.Clear();

            queried = _db.ConvertedPatriarchy.Where(e => e.Name.StartsWith("Th")).OrderBy(e => e.Id).ToList();

            Assert.Equal(3, queried.Count);

            Assert.Equal(HierarchyId.Parse("/2/1/").ToString(), queried[0].HierarchyId);
            Assert.Equal("Thrór", queried[0].Name);

            Assert.Equal(HierarchyId.Parse("/2/2/").ToString(), queried[1].HierarchyId);
            Assert.Equal("Thráin II", queried[1].Name);

            Assert.Equal(HierarchyId.Parse("/3/1/").ToString(), queried[2].HierarchyId);
            Assert.Equal("Thorin Oakenshield", queried[2].Name);
        }
    }

    [ConditionalFact]
    public void HierarchyId_get_ancestor_of_level_is_root()
    {
        var results = (from p in _db.Patriarchy
                       where
                           p.Id.GetAncestor(p.Id.GetLevel())
                           == HierarchyId
                               .GetRoot() // HierarchyId.Parse("/1/") // HierarchyId.Parse(p.Id.ToString()).GetAncestor(HierarchyId.Parse(p.Id.ToString()).GetLevel())
                       select p.Name).ToList();

        Assert.Equal(
            Condense(
                @"SELECT [p].[Name] FROM [Patriarchy] AS [p] WHERE [p].[Id].GetAncestor(CAST([p].[Id].GetLevel() AS int)) = hierarchyid::Parse('/')"),
            Condense(_db.Sql));

        var all = (from p in _db.Patriarchy
                   select p.Name).ToList();

        Assert.Equal(all, results);
    }

    [ConditionalFact]
    public void HierarchyId_can_call_method_on_parameter()
    {
        var isaac = HierarchyId.Parse("/1/");

        var results = (from p in _db.Patriarchy
                       where isaac.IsDescendantOf(p.Id)
                       select p.Name).ToList();

        Assert.Equal(
            """
            @__isaac_0='?' (DbType = Object)

            SELECT [p].[Name]
            FROM [Patriarchy] AS [p]
            WHERE @__isaac_0.IsDescendantOf([p].[Id]) = CAST(1 AS bit)
            """,
            _db.Sql,
            ignoreLineEndingDifferences: true);

        Assert.Equal(new[] { "Abraham", "Isaac" }, results);
    }

    [ConditionalFact]
    public void ToString_can_translate()
    {
        var results = (from p in _db.Patriarchy
                       where p.Id.GetLevel() == 1
                       select p.Id.ToString()).ToList();

        Assert.Equal(
            Condense(@"SELECT [p].[Id].ToString() FROM [Patriarchy] AS [p] WHERE [p].[Id].GetLevel() = CAST(1 AS smallint)"),
            Condense(_db.Sql));

        Assert.Equal(new[] { "/1/" }, results);
    }

    [ConditionalFact]
    public void ToString_can_translate_redux()
    {
        var results = (from p in _db.Patriarchy
                       where EF.Functions.Like(p.Id.ToString(), "%/1/")
                       select p.Name).ToList();

        Assert.Equal(
            Condense(@"SELECT [p].[Name] FROM [Patriarchy] AS [p] WHERE [p].[Id].ToString() LIKE N'%/1/'"),
            Condense(_db.Sql));

        Assert.Equal(new[] { "Isaac", "Jacob", "Reuben" }, results);
    }

    [ConditionalFact]
    public void Parse_can_translate()
    {
        var results = (from p in _db.Patriarchy
                       where p.Id == HierarchyId.GetRoot()
                       select HierarchyId.Parse(p.Id.ToString())).ToList();

        Assert.Equal(
            Condense(@"SELECT hierarchyid::Parse([p].[Id].ToString()) FROM [Patriarchy] AS [p] WHERE [p].[Id] = hierarchyid::Parse('/')"),
            Condense(_db.Sql));

        Assert.Equal(new[] { HierarchyId.Parse("/") }, results);
    }

    [ConditionalFact]
    public void Contains_with_parameter_list_can_translate()
    {
        var ids = new[] { HierarchyId.Parse("/1/1/7/"), HierarchyId.Parse("/1/1/99/") };
        var result = (from p in _db.Patriarchy
                       where ids.Contains(p.Id)
                       select p.Name).Single();

        Assert.Equal(
            """
@__ids_0='?' (Size = 4000)

SELECT TOP(2) [p].[Name]
FROM [Patriarchy] AS [p]
WHERE [p].[Id] IN (
    SELECT CAST([i].[value] AS hierarchyid) AS [value]
    FROM OPENJSON(@__ids_0) AS [i]
)
""",
            _db.Sql,
            ignoreLineEndingDifferences: true);

        Assert.Equal("Dan", result);
    }

    public void Dispose()
        => _db.Dispose();

    // replace whitespace with a single space
    private static string Condense(string str)
    {
        var split = str.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
        return string.Join(" ", split);
    }
}
