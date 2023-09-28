// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Update;

public class NonSharedModelUpdatesSqlServerTest : NonSharedModelUpdatesTestBase
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
        var contextFactory = await InitializeAsync<DbContext>(
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

    private void AssertSql(params string[] expected)
        => TestSqlLoggerFactory.AssertBaseline(expected);

    protected override ITestStoreFactory TestStoreFactory => SqlServerTestStoreFactory.Instance;
}
