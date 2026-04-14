// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Update;

#nullable disable

public class NonSharedModelUpdatesSqlServerTest(NonSharedFixture fixture) : NonSharedModelUpdatesTestBase(fixture)
{
    public override async Task Principal_and_dependent_roundtrips_with_cycle_breaking(bool async)
    {
        await base.Principal_and_dependent_roundtrips_with_cycle_breaking(async);

        AssertSql(
            """
@p0='AC South' (Size = 4000)

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
INSERT INTO [AuthorsClub] ([Name])
OUTPUT INSERTED.[Id]
VALUES (@p0);
""",
            //
            """
@p1='1'
@p2='Alice' (Size = 4000)

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
INSERT INTO [Author] ([AuthorsClubId], [Name])
OUTPUT INSERTED.[Id]
VALUES (@p1, @p2);
""",
            //
            """
@p3='1'
@p4=NULL (Size = 4000)

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
INSERT INTO [Book] ([AuthorId], [Title])
OUTPUT INSERTED.[Id]
VALUES (@p3, @p4);
""",
            //
            """
SELECT TOP(2) [b].[Id], [b].[AuthorId], [b].[Title], [a].[Id], [a].[AuthorsClubId], [a].[Name]
FROM [Book] AS [b]
INNER JOIN [Author] AS [a] ON [b].[AuthorId] = [a].[Id]
""",
            //
            """
@p0='AC North' (Size = 4000)

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
INSERT INTO [AuthorsClub] ([Name])
OUTPUT INSERTED.[Id]
VALUES (@p0);
""",
            //
            """
@p1='2'
@p2='Author of the year 2023' (Size = 4000)

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
INSERT INTO [Author] ([AuthorsClubId], [Name])
OUTPUT INSERTED.[Id]
VALUES (@p1, @p2);
""",
            //
            """
@p4='1'
@p3='2'
@p5='1'

SET NOCOUNT ON;
UPDATE [Book] SET [AuthorId] = @p3
OUTPUT 1
WHERE [Id] = @p4;
DELETE FROM [Author]
OUTPUT 1
WHERE [Id] = @p5;
""");
    }

    [ConditionalFact] // Issue #29502
    public virtual async Task Bulk_insert_result_set_mapping()
    {
        var contextFactory = await InitializeNonSharedTest<DbContext>(
            onModelCreating: mb =>
            {
                mb.Entity<User>().ToTable("Users");
                mb.Entity<DailyDigest>().ToTable("DailyDigests");
            },
            createTestStore: () => SqlServerTestStore.GetOrCreateWithScriptPath(
                "Issue29502",
                Path.Combine("Update", "Issue29502.sql"),
                shared: false));

        await ExecuteWithStrategyInTransactionAsync(
            contextFactory,
            async context =>
            {
                var digests = await context.Set<User>()
                    .OrderBy(u => u.TimeCreatedUtc)
                    .Take(23)
                    .Select(u => new DailyDigest { User = u })
                    .ToListAsync();

                foreach (var digest in digests)
                {
                    context.Set<DailyDigest>().Add(digest);
                }

                await context.SaveChangesAsync();
            });
    }

    public class User
    {
        public string Id { get; set; } = null!;
        public DateTime TimeCreatedUtc { get; set; }
        public ICollection<DailyDigest> DailyDigests { get; set; } = null!;
    }

    public class DailyDigest
    {
        public int Id { get; set; }
        public User User { get; set; }
    }

    public override async Task DbUpdateException_Entries_is_correct_with_multiple_inserts(bool async)
    {
        // SQL Server's bulk insert support makes it impossible to populate the entry which caused the exception, since the position
        // used to find the entry is returned as an output column, but the row is never received in case of an exception.
        // Instead we make sure Entries contains all entries.
        var contextFactory = await InitializeNonSharedTest<DbContext>(onModelCreating: mb => mb.Entity<Blog>().HasIndex(b => b.Name).IsUnique());

        await ExecuteWithStrategyInTransactionAsync(
            contextFactory,
            async context =>
            {
                context.Add(new Blog { Name = "Blog2" });
                await context.SaveChangesAsync();
            },
            async context =>
            {
                context.Add(new Blog { Name = "Blog1" });
                context.Add(new Blog { Name = "Blog2" });
                context.Add(new Blog { Name = "Blog3" });

                var exception = async
                    ? await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync())
                    : Assert.Throws<DbUpdateException>(() => context.SaveChanges());

                Assert.Collection(
                    exception.Entries.Select(e => (Blog)e.Entity).OrderBy(b => b.Name),
                    b => Assert.Equal("Blog1", b.Name),
                    b => Assert.Equal("Blog2", b.Name),
                    b => Assert.Equal("Blog3", b.Name));
            });

        AssertSql(
            """
@p0='Blog2' (Size = 450)

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
INSERT INTO [Blog] ([Name])
OUTPUT INSERTED.[Id]
VALUES (@p0);
""",
            //
            """
@p0='Blog1' (Size = 450)
@p1='Blog2' (Size = 450)
@p2='Blog3' (Size = 450)

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
MERGE [Blog] USING (
VALUES (@p0, 0),
(@p1, 1),
(@p2, 2)) AS i ([Name], _Position) ON 1=0
WHEN NOT MATCHED THEN
INSERT ([Name])
VALUES (i.[Name])
OUTPUT INSERTED.[Id], i._Position;
""");
    }

    private void AssertSql(params string[] expected)
        => TestSqlLoggerFactory.AssertBaseline(expected);

    public override async Task Update_entity_with_not_loaded_property_excludes_column_from_SQL(bool async)
    {
        await base.Update_entity_with_not_loaded_property_excludes_column_from_SQL(async);

        AssertSql(
            """
@p1='1'
@p0='Updated Blog' (Size = 4000)

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [BlogWithDescription] SET [Name] = @p0
OUTPUT 1
WHERE [Id] = @p1;
""",
            //
            """
SELECT TOP(2) [b].[Id], [b].[Name]
FROM [BlogWithDescription] AS [b]
""",
            //
            """
SELECT TOP(2) [b].[Description]
FROM [BlogWithDescription] AS [b]
WHERE [b].[Description] = N'Original description'
""");
    }

    public override async Task Save_and_query_with_partially_loaded_primitive_collection(bool async)
    {
        await base.Save_and_query_with_partially_loaded_primitive_collection(async);

        AssertSql(
            """
@p1='1'
@p0='Updated Blog' (Size = 4000)

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [BlogWithTags] SET [Name] = @p0
OUTPUT 1
WHERE [Id] = @p1;
""",
            //
            """
SELECT TOP(2) [b].[Id], [b].[Name]
FROM [BlogWithTags] AS [b]
""");
    }

    public override async Task Query_with_not_auto_loaded_property_tracked(bool async)
    {
        await base.Query_with_not_auto_loaded_property_tracked(async);

        AssertSql(
            """
SELECT TOP(2) [b].[Id], [b].[Name]
FROM [BlogWithDescription] AS [b]
""");
    }

    public override async Task Query_with_not_auto_loaded_property_no_tracking(bool async)
    {
        await base.Query_with_not_auto_loaded_property_no_tracking(async);

        AssertSql(
            """
SELECT TOP(2) [b].[Id], [b].[Name]
FROM [BlogWithDescription] AS [b]
""");
    }

    public override async Task Explicit_select_of_not_auto_loaded_property(bool async)
    {
        await base.Explicit_select_of_not_auto_loaded_property(async);

        AssertSql(
            """
SELECT TOP(2) [b].[Description]
FROM [BlogWithDescription] AS [b]
""");
    }

    public override async Task Where_on_not_auto_loaded_property(bool async)
    {
        await base.Where_on_not_auto_loaded_property(async);

        AssertSql(
            """
SELECT TOP(2) [b].[Id], [b].[Name]
FROM [BlogWithDescription] AS [b]
WHERE [b].[Description] = N'Some description'
""");
    }

    public override async Task Query_with_not_auto_loaded_primitive_collection(bool async)
    {
        await base.Query_with_not_auto_loaded_primitive_collection(async);

        AssertSql(
            """
SELECT TOP(2) [b].[Id], [b].[Name]
FROM [BlogWithTags] AS [b]
""");
    }

    protected override ITestStoreFactory NonSharedTestStoreFactory
        => SqlServerTestStoreFactory.Instance;
}
