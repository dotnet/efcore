// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Update;

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

    [Fact] // Issue #29502
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
        public User? User { get; set; }
    }

    public override async Task DbUpdateException_Entries_is_correct_with_multiple_inserts(bool async)
    {
        // SQL Server's bulk insert support makes it impossible to populate the entry which caused the exception, since the position
        // used to find the entry is returned as an output column, but the row is never received in case of an exception.
        // Instead we make sure Entries contains all entries.
        var contextFactory =
            await InitializeNonSharedTest<DbContext>(onModelCreating: mb => mb.Entity<Blog>().HasIndex(b => b.Name).IsUnique());

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

    public override async Task Nullable_json_collection_change_is_saved(
        NullableJsonCollectionValue from,
        NullableJsonCollectionValue to,
        bool alsoModifyName)
    {
        await base.Nullable_json_collection_change_is_saved(from, to, alsoModifyName);

        if (to == NullableJsonCollectionValue.WithElements)
        {
            AssertSql(
                """
@p0='[]' (Nullable = false) (Size = 2)
@p1='NullableJsonParent' (Nullable = false) (Size = 34)
@p2='A' (Size = 4000)
@p3='T1' (Size = 4000)

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
INSERT INTO [NullableJsonParent] ([InitializedChildren], [Discriminator], [Name], [Token])
OUTPUT INSERTED.[Id]
VALUES (@p0, @p1, @p2, @p3);
""",
                //
                """
SELECT TOP(2) [n].[Id], [n].[Discriminator], [n].[Name], [n].[Token], [n].[Children], [n].[InitializedChildren], [n].[Outer], [n].[Reference], [n].[DerivedChildren]
FROM [NullableJsonParent] AS [n]
""",
                //
                """
@p0='[{"Name":"C1"}]' (Nullable = false) (Size = 15)
@p1='[]' (Nullable = false) (Size = 2)
@p2='1'
@p3='T1' (Size = 4000)

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [NullableJsonParent] SET [Children] = @p0, [InitializedChildren] = @p1
OUTPUT 1
WHERE [Id] = @p2 AND [Token] = @p3;
""",
                //
                """
SELECT TOP(2) [n].[Id], [n].[Discriminator], [n].[Name], [n].[Token], [n].[Children], [n].[InitializedChildren], [n].[Outer], [n].[Reference], [n].[DerivedChildren]
FROM [NullableJsonParent] AS [n]
""");
        }
        else if (from == NullableJsonCollectionValue.Empty && !alsoModifyName)
        {
            AssertSql(
                """
@p0='[]' (Nullable = false) (Size = 2)
@p1='[]' (Nullable = false) (Size = 2)
@p2='NullableJsonParent' (Nullable = false) (Size = 34)
@p3='A' (Size = 4000)
@p4='T1' (Size = 4000)

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
INSERT INTO [NullableJsonParent] ([Children], [InitializedChildren], [Discriminator], [Name], [Token])
OUTPUT INSERTED.[Id]
VALUES (@p0, @p1, @p2, @p3, @p4);
""",
                //
                """
SELECT TOP(2) [n].[Id], [n].[Discriminator], [n].[Name], [n].[Token], [n].[Children], [n].[InitializedChildren], [n].[Outer], [n].[Reference], [n].[DerivedChildren]
FROM [NullableJsonParent] AS [n]
""",
                //
                """
@p0=NULL (Nullable = false)
@p1='[]' (Nullable = false) (Size = 2)
@p2='1'
@p3='T1' (Size = 4000)

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [NullableJsonParent] SET [Children] = @p0, [InitializedChildren] = @p1
OUTPUT 1
WHERE [Id] = @p2 AND [Token] = @p3;
""",
                //
                """
SELECT TOP(2) [n].[Id], [n].[Discriminator], [n].[Name], [n].[Token], [n].[Children], [n].[InitializedChildren], [n].[Outer], [n].[Reference], [n].[DerivedChildren]
FROM [NullableJsonParent] AS [n]
""");
        }
        else if (from == NullableJsonCollectionValue.Empty)
        {
            AssertSql(
                """
@p0='[]' (Nullable = false) (Size = 2)
@p1='[]' (Nullable = false) (Size = 2)
@p2='NullableJsonParent' (Nullable = false) (Size = 34)
@p3='A' (Size = 4000)
@p4='T1' (Size = 4000)

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
INSERT INTO [NullableJsonParent] ([Children], [InitializedChildren], [Discriminator], [Name], [Token])
OUTPUT INSERTED.[Id]
VALUES (@p0, @p1, @p2, @p3, @p4);
""",
                //
                """
SELECT TOP(2) [n].[Id], [n].[Discriminator], [n].[Name], [n].[Token], [n].[Children], [n].[InitializedChildren], [n].[Outer], [n].[Reference], [n].[DerivedChildren]
FROM [NullableJsonParent] AS [n]
""",
                //
                """
@p0=NULL (Nullable = false)
@p1='[]' (Nullable = false) (Size = 2)
@p3='1'
@p2='B' (Size = 4000)
@p4='T1' (Size = 4000)

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [NullableJsonParent] SET [Children] = @p0, [InitializedChildren] = @p1, [Name] = @p2
OUTPUT 1
WHERE [Id] = @p3 AND [Token] = @p4;
""",
                //
                """
SELECT TOP(2) [n].[Id], [n].[Discriminator], [n].[Name], [n].[Token], [n].[Children], [n].[InitializedChildren], [n].[Outer], [n].[Reference], [n].[DerivedChildren]
FROM [NullableJsonParent] AS [n]
""");
        }
        else if (from == NullableJsonCollectionValue.Null && !alsoModifyName)
        {
            AssertSql(
                """
@p0='[]' (Nullable = false) (Size = 2)
@p1='NullableJsonParent' (Nullable = false) (Size = 34)
@p2='A' (Size = 4000)
@p3='T1' (Size = 4000)

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
INSERT INTO [NullableJsonParent] ([InitializedChildren], [Discriminator], [Name], [Token])
OUTPUT INSERTED.[Id]
VALUES (@p0, @p1, @p2, @p3);
""",
                //
                """
SELECT TOP(2) [n].[Id], [n].[Discriminator], [n].[Name], [n].[Token], [n].[Children], [n].[InitializedChildren], [n].[Outer], [n].[Reference], [n].[DerivedChildren]
FROM [NullableJsonParent] AS [n]
""",
                //
                """
@p0='[]' (Nullable = false) (Size = 2)
@p1='[]' (Nullable = false) (Size = 2)
@p2='1'
@p3='T1' (Size = 4000)

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [NullableJsonParent] SET [Children] = @p0, [InitializedChildren] = @p1
OUTPUT 1
WHERE [Id] = @p2 AND [Token] = @p3;
""",
                //
                """
SELECT TOP(2) [n].[Id], [n].[Discriminator], [n].[Name], [n].[Token], [n].[Children], [n].[InitializedChildren], [n].[Outer], [n].[Reference], [n].[DerivedChildren]
FROM [NullableJsonParent] AS [n]
""");
        }
        else if (from == NullableJsonCollectionValue.Null)
        {
            AssertSql(
                """
@p0='[]' (Nullable = false) (Size = 2)
@p1='NullableJsonParent' (Nullable = false) (Size = 34)
@p2='A' (Size = 4000)
@p3='T1' (Size = 4000)

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
INSERT INTO [NullableJsonParent] ([InitializedChildren], [Discriminator], [Name], [Token])
OUTPUT INSERTED.[Id]
VALUES (@p0, @p1, @p2, @p3);
""",
                //
                """
SELECT TOP(2) [n].[Id], [n].[Discriminator], [n].[Name], [n].[Token], [n].[Children], [n].[InitializedChildren], [n].[Outer], [n].[Reference], [n].[DerivedChildren]
FROM [NullableJsonParent] AS [n]
""",
                //
                """
@p0='[]' (Nullable = false) (Size = 2)
@p1='[]' (Nullable = false) (Size = 2)
@p3='1'
@p2='B' (Size = 4000)
@p4='T1' (Size = 4000)

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [NullableJsonParent] SET [Children] = @p0, [InitializedChildren] = @p1, [Name] = @p2
OUTPUT 1
WHERE [Id] = @p3 AND [Token] = @p4;
""",
                //
                """
SELECT TOP(2) [n].[Id], [n].[Discriminator], [n].[Name], [n].[Token], [n].[Children], [n].[InitializedChildren], [n].[Outer], [n].[Reference], [n].[DerivedChildren]
FROM [NullableJsonParent] AS [n]
""");
        }
        else
        {
            AssertSql(
                """
@p0='[{"Name":"C1"}]' (Nullable = false) (Size = 15)
@p1='[]' (Nullable = false) (Size = 2)
@p2='NullableJsonParent' (Nullable = false) (Size = 34)
@p3='A' (Size = 4000)
@p4='T1' (Size = 4000)

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
INSERT INTO [NullableJsonParent] ([Children], [InitializedChildren], [Discriminator], [Name], [Token])
OUTPUT INSERTED.[Id]
VALUES (@p0, @p1, @p2, @p3, @p4);
""",
                //
                """
SELECT TOP(2) [n].[Id], [n].[Discriminator], [n].[Name], [n].[Token], [n].[Children], [n].[InitializedChildren], [n].[Outer], [n].[Reference], [n].[DerivedChildren]
FROM [NullableJsonParent] AS [n]
""",
                //
                """
@p0=NULL (Nullable = false)
@p1='[]' (Nullable = false) (Size = 2)
@p2='1'
@p3='T1' (Size = 4000)

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [NullableJsonParent] SET [Children] = @p0, [InitializedChildren] = @p1
OUTPUT 1
WHERE [Id] = @p2 AND [Token] = @p3;
""",
                //
                """
SELECT TOP(2) [n].[Id], [n].[Discriminator], [n].[Name], [n].[Token], [n].[Children], [n].[InitializedChildren], [n].[Outer], [n].[Reference], [n].[DerivedChildren]
FROM [NullableJsonParent] AS [n]
""");
        }
    }

    public override async Task Nullable_json_collection_nested_in_a_json_reference_changed_from_empty_to_null_is_saved(bool alsoModifyReference)
    {
        await base.Nullable_json_collection_nested_in_a_json_reference_changed_from_empty_to_null_is_saved(alsoModifyReference);

        if (alsoModifyReference)
        {
            AssertSql(
                """
@p0='{"Name":"R","Items":[]}' (Nullable = false) (Size = 23)
@p1='[]' (Nullable = false) (Size = 2)
@p2='NullableJsonParent' (Nullable = false) (Size = 34)
@p3='A' (Size = 4000)
@p4=NULL (Size = 4000)

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
INSERT INTO [NullableJsonParent] ([Reference], [InitializedChildren], [Discriminator], [Name], [Token])
OUTPUT INSERTED.[Id]
VALUES (@p0, @p1, @p2, @p3, @p4);
""",
                //
                """
SELECT TOP(2) [n].[Id], [n].[Discriminator], [n].[Name], [n].[Token], [n].[Children], [n].[InitializedChildren], [n].[Outer], [n].[Reference], [n].[DerivedChildren]
FROM [NullableJsonParent] AS [n]
""",
                //
                """
@p0='{"Name":"R2","Items":null}' (Nullable = false) (Size = 26)
@p1='[]' (Nullable = false) (Size = 2)
@p2='1'
@p3=NULL (Size = 4000)

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [NullableJsonParent] SET [Reference] = @p0, [InitializedChildren] = @p1
OUTPUT 1
WHERE [Id] = @p2 AND [Token] IS NULL;
""",
                //
                """
SELECT TOP(2) [n].[Id], [n].[Discriminator], [n].[Name], [n].[Token], [n].[Children], [n].[InitializedChildren], [n].[Outer], [n].[Reference], [n].[DerivedChildren]
FROM [NullableJsonParent] AS [n]
""");
        }
        else
        {
            AssertSql(
                """
@p0='{"Name":"R","Items":[]}' (Nullable = false) (Size = 23)
@p1='[]' (Nullable = false) (Size = 2)
@p2='NullableJsonParent' (Nullable = false) (Size = 34)
@p3='A' (Size = 4000)
@p4=NULL (Size = 4000)

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
INSERT INTO [NullableJsonParent] ([Reference], [InitializedChildren], [Discriminator], [Name], [Token])
OUTPUT INSERTED.[Id]
VALUES (@p0, @p1, @p2, @p3, @p4);
""",
                //
                """
SELECT TOP(2) [n].[Id], [n].[Discriminator], [n].[Name], [n].[Token], [n].[Children], [n].[InitializedChildren], [n].[Outer], [n].[Reference], [n].[DerivedChildren]
FROM [NullableJsonParent] AS [n]
""",
                //
                """
@p0='{"Name":"R","Items":null}' (Nullable = false) (Size = 25)
@p1='[]' (Nullable = false) (Size = 2)
@p2='1'
@p3=NULL (Size = 4000)

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [NullableJsonParent] SET [Reference] = @p0, [InitializedChildren] = @p1
OUTPUT 1
WHERE [Id] = @p2 AND [Token] IS NULL;
""",
                //
                """
SELECT TOP(2) [n].[Id], [n].[Discriminator], [n].[Name], [n].[Token], [n].[Children], [n].[InitializedChildren], [n].[Outer], [n].[Reference], [n].[DerivedChildren]
FROM [NullableJsonParent] AS [n]
""");
        }
    }

    public override async Task Nullable_json_collection_nested_in_a_json_reference_changed_from_null_is_saved(bool withElements)
    {
        await base.Nullable_json_collection_nested_in_a_json_reference_changed_from_null_is_saved(withElements);

        if (withElements)
        {
            AssertSql(
                """
@p0='{"Name":"R","Items":null}' (Nullable = false) (Size = 25)
@p1='[]' (Nullable = false) (Size = 2)
@p2='NullableJsonParent' (Nullable = false) (Size = 34)
@p3='A' (Size = 4000)
@p4=NULL (Size = 4000)

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
INSERT INTO [NullableJsonParent] ([Reference], [InitializedChildren], [Discriminator], [Name], [Token])
OUTPUT INSERTED.[Id]
VALUES (@p0, @p1, @p2, @p3, @p4);
""",
                //
                """
SELECT TOP(2) [n].[Id], [n].[Discriminator], [n].[Name], [n].[Token], [n].[Children], [n].[InitializedChildren], [n].[Outer], [n].[Reference], [n].[DerivedChildren]
FROM [NullableJsonParent] AS [n]
""",
                //
                """
@p0='{"Name":"R","Items":[{"Name":"C1"}]}' (Nullable = false) (Size = 36)
@p1='[]' (Nullable = false) (Size = 2)
@p2='1'
@p3=NULL (Size = 4000)

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [NullableJsonParent] SET [Reference] = @p0, [InitializedChildren] = @p1
OUTPUT 1
WHERE [Id] = @p2 AND [Token] IS NULL;
""",
                //
                """
SELECT TOP(2) [n].[Id], [n].[Discriminator], [n].[Name], [n].[Token], [n].[Children], [n].[InitializedChildren], [n].[Outer], [n].[Reference], [n].[DerivedChildren]
FROM [NullableJsonParent] AS [n]
""");
        }
        else
        {
            AssertSql(
                """
@p0='{"Name":"R","Items":null}' (Nullable = false) (Size = 25)
@p1='[]' (Nullable = false) (Size = 2)
@p2='NullableJsonParent' (Nullable = false) (Size = 34)
@p3='A' (Size = 4000)
@p4=NULL (Size = 4000)

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
INSERT INTO [NullableJsonParent] ([Reference], [InitializedChildren], [Discriminator], [Name], [Token])
OUTPUT INSERTED.[Id]
VALUES (@p0, @p1, @p2, @p3, @p4);
""",
                //
                """
SELECT TOP(2) [n].[Id], [n].[Discriminator], [n].[Name], [n].[Token], [n].[Children], [n].[InitializedChildren], [n].[Outer], [n].[Reference], [n].[DerivedChildren]
FROM [NullableJsonParent] AS [n]
""",
                //
                """
@p0='{"Name":"R","Items":[]}' (Nullable = false) (Size = 23)
@p1='[]' (Nullable = false) (Size = 2)
@p2='1'
@p3=NULL (Size = 4000)

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [NullableJsonParent] SET [Reference] = @p0, [InitializedChildren] = @p1
OUTPUT 1
WHERE [Id] = @p2 AND [Token] IS NULL;
""",
                //
                """
SELECT TOP(2) [n].[Id], [n].[Discriminator], [n].[Name], [n].[Token], [n].[Children], [n].[InitializedChildren], [n].[Outer], [n].[Reference], [n].[DerivedChildren]
FROM [NullableJsonParent] AS [n]
""");
        }
    }

    public override async Task Nullable_json_collection_nested_in_json_collection_elements_changed_from_empty_to_null_is_saved()
    {
        await base.Nullable_json_collection_nested_in_json_collection_elements_changed_from_empty_to_null_is_saved();

        AssertSql(
            """
@p0='[{"Name":"O1","Inner":[]},{"Name":"O2","Inner":[]}]' (Nullable = false) (Size = 51)
@p1='[]' (Nullable = false) (Size = 2)
@p2='NullableJsonParent' (Nullable = false) (Size = 34)
@p3='A' (Size = 4000)
@p4=NULL (Size = 4000)

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
INSERT INTO [NullableJsonParent] ([Outer], [InitializedChildren], [Discriminator], [Name], [Token])
OUTPUT INSERTED.[Id]
VALUES (@p0, @p1, @p2, @p3, @p4);
""",
            //
            """
SELECT TOP(2) [n].[Id], [n].[Discriminator], [n].[Name], [n].[Token], [n].[Children], [n].[InitializedChildren], [n].[Outer], [n].[Reference], [n].[DerivedChildren]
FROM [NullableJsonParent] AS [n]
""",
            //
            """
@p0='{"Name":"O1","Inner":null}' (Nullable = false) (Size = 26)
@p1='[]' (Nullable = false) (Size = 2)
@p2='1'
@p3=NULL (Size = 4000)

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [NullableJsonParent] SET [Outer] = JSON_MODIFY([Outer], 'strict $[0]', JSON_QUERY(@p0)), [InitializedChildren] = @p1
OUTPUT 1
WHERE [Id] = @p2 AND [Token] IS NULL;
""",
            //
            """
SELECT TOP(2) [n].[Id], [n].[Discriminator], [n].[Name], [n].[Token], [n].[Children], [n].[InitializedChildren], [n].[Outer], [n].[Reference], [n].[DerivedChildren]
FROM [NullableJsonParent] AS [n]
""");
    }

    public override async Task Nullable_json_collection_nested_in_json_collection_elements_changed_from_null_is_saved(bool withElements)
    {
        await base.Nullable_json_collection_nested_in_json_collection_elements_changed_from_null_is_saved(withElements);

        if (withElements)
        {
            AssertSql(
                """
@p0='[{"Name":"O1","Inner":null},{"Name":"O2","Inner":null}]' (Nullable = false) (Size = 55)
@p1='[]' (Nullable = false) (Size = 2)
@p2='NullableJsonParent' (Nullable = false) (Size = 34)
@p3='A' (Size = 4000)
@p4=NULL (Size = 4000)

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
INSERT INTO [NullableJsonParent] ([Outer], [InitializedChildren], [Discriminator], [Name], [Token])
OUTPUT INSERTED.[Id]
VALUES (@p0, @p1, @p2, @p3, @p4);
""",
                //
                """
SELECT TOP(2) [n].[Id], [n].[Discriminator], [n].[Name], [n].[Token], [n].[Children], [n].[InitializedChildren], [n].[Outer], [n].[Reference], [n].[DerivedChildren]
FROM [NullableJsonParent] AS [n]
""",
                //
                """
@p0='{"Name":"O1","Inner":[{"Name":"C1"}]}' (Nullable = false) (Size = 37)
@p1='[]' (Nullable = false) (Size = 2)
@p2='1'
@p3=NULL (Size = 4000)

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [NullableJsonParent] SET [Outer] = JSON_MODIFY([Outer], 'strict $[0]', JSON_QUERY(@p0)), [InitializedChildren] = @p1
OUTPUT 1
WHERE [Id] = @p2 AND [Token] IS NULL;
""",
                //
                """
SELECT TOP(2) [n].[Id], [n].[Discriminator], [n].[Name], [n].[Token], [n].[Children], [n].[InitializedChildren], [n].[Outer], [n].[Reference], [n].[DerivedChildren]
FROM [NullableJsonParent] AS [n]
""");
        }
        else
        {
            AssertSql(
                """
@p0='[{"Name":"O1","Inner":null},{"Name":"O2","Inner":null}]' (Nullable = false) (Size = 55)
@p1='[]' (Nullable = false) (Size = 2)
@p2='NullableJsonParent' (Nullable = false) (Size = 34)
@p3='A' (Size = 4000)
@p4=NULL (Size = 4000)

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
INSERT INTO [NullableJsonParent] ([Outer], [InitializedChildren], [Discriminator], [Name], [Token])
OUTPUT INSERTED.[Id]
VALUES (@p0, @p1, @p2, @p3, @p4);
""",
                //
                """
SELECT TOP(2) [n].[Id], [n].[Discriminator], [n].[Name], [n].[Token], [n].[Children], [n].[InitializedChildren], [n].[Outer], [n].[Reference], [n].[DerivedChildren]
FROM [NullableJsonParent] AS [n]
""",
                //
                """
@p0='{"Name":"O1","Inner":[]}' (Nullable = false) (Size = 24)
@p1='[]' (Nullable = false) (Size = 2)
@p2='1'
@p3=NULL (Size = 4000)

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [NullableJsonParent] SET [Outer] = JSON_MODIFY([Outer], 'strict $[0]', JSON_QUERY(@p0)), [InitializedChildren] = @p1
OUTPUT 1
WHERE [Id] = @p2 AND [Token] IS NULL;
""",
                //
                """
SELECT TOP(2) [n].[Id], [n].[Discriminator], [n].[Name], [n].[Token], [n].[Children], [n].[InitializedChildren], [n].[Outer], [n].[Reference], [n].[DerivedChildren]
FROM [NullableJsonParent] AS [n]
""");
        }
    }

    public override async Task Nullable_json_collection_declared_on_a_derived_type_changed_to_null_is_saved()
    {
        await base.Nullable_json_collection_declared_on_a_derived_type_changed_to_null_is_saved();

        AssertSql(
            """
@p0='[]' (Nullable = false) (Size = 2)
@p1='[]' (Nullable = false) (Size = 2)
@p2='NullableJsonDerivedParent' (Nullable = false) (Size = 34)
@p3='Derived' (Size = 4000)
@p4=NULL (Size = 4000)
@p5='[]' (Nullable = false) (Size = 2)
@p6='NullableJsonParent' (Nullable = false) (Size = 34)
@p7='Base' (Size = 4000)
@p8=NULL (Size = 4000)

SET NOCOUNT ON;
INSERT INTO [NullableJsonParent] ([InitializedChildren], [DerivedChildren], [Discriminator], [Name], [Token])
OUTPUT INSERTED.[Id]
VALUES (@p0, @p1, @p2, @p3, @p4);
INSERT INTO [NullableJsonParent] ([InitializedChildren], [Discriminator], [Name], [Token])
OUTPUT INSERTED.[Id]
VALUES (@p5, @p6, @p7, @p8);
""",
            //
            """
SELECT TOP(2) [n].[Id], [n].[Discriminator], [n].[Name], [n].[Token], [n].[Children], [n].[InitializedChildren], [n].[Outer], [n].[Reference], [n].[DerivedChildren]
FROM [NullableJsonParent] AS [n]
WHERE [n].[Discriminator] = N'NullableJsonDerivedParent'
""",
            //
            """
@p0='[]' (Nullable = false) (Size = 2)
@p1=NULL (Nullable = false)
@p2='1'
@p3=NULL (Size = 4000)

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [NullableJsonParent] SET [InitializedChildren] = @p0, [DerivedChildren] = @p1
OUTPUT 1
WHERE [Id] = @p2 AND [Token] IS NULL;
""",
            //
            """
SELECT [n].[Id], [n].[Discriminator], [n].[Name], [n].[Token], [n].[Children], [n].[InitializedChildren], [n].[Outer], [n].[Reference], [n].[DerivedChildren]
FROM [NullableJsonParent] AS [n]
ORDER BY [n].[Name]
""");
    }

    public override async Task Nullable_json_collection_stored_as_null_or_empty_is_loaded_as_null_or_empty(
        NullableJsonCollectionQuery queryKind)
    {
        await base.Nullable_json_collection_stored_as_null_or_empty_is_loaded_as_null_or_empty(queryKind);

        if (queryKind == NullableJsonCollectionQuery.Tracking)
        {
            AssertSql(
                """
@p0='NullableJsonParent' (Nullable = false) (Size = 34)
@p1='Null' (Size = 4000)
@p2=NULL (Size = 4000)
@p3='[]' (Nullable = false) (Size = 2)
@p4='[]' (Nullable = false) (Size = 2)
@p5='NullableJsonParent' (Nullable = false) (Size = 34)
@p6='Empty' (Size = 4000)
@p7=NULL (Size = 4000)

SET NOCOUNT ON;
INSERT INTO [NullableJsonParent] ([Discriminator], [Name], [Token])
OUTPUT INSERTED.[Id]
VALUES (@p0, @p1, @p2);
INSERT INTO [NullableJsonParent] ([Children], [InitializedChildren], [Discriminator], [Name], [Token])
OUTPUT INSERTED.[Id]
VALUES (@p3, @p4, @p5, @p6, @p7);
""",
                //
                """
SELECT [n].[Id], [n].[Discriminator], [n].[Name], [n].[Token], [n].[Children], [n].[InitializedChildren], [n].[Outer], [n].[Reference], [n].[DerivedChildren]
FROM [NullableJsonParent] AS [n]
ORDER BY [n].[Name]
""",
                //
                """
SELECT [n].[Id], [n].[Discriminator], [n].[Name], [n].[Token], [n].[Children], [n].[InitializedChildren], [n].[Outer], [n].[Reference], [n].[DerivedChildren]
FROM [NullableJsonParent] AS [n]
ORDER BY [n].[Name]
""");
        }
        else
        {
            AssertSql(
                """
@p0='NullableJsonParent' (Nullable = false) (Size = 34)
@p1='Null' (Size = 4000)
@p2=NULL (Size = 4000)
@p3='[]' (Nullable = false) (Size = 2)
@p4='[]' (Nullable = false) (Size = 2)
@p5='NullableJsonParent' (Nullable = false) (Size = 34)
@p6='Empty' (Size = 4000)
@p7=NULL (Size = 4000)

SET NOCOUNT ON;
INSERT INTO [NullableJsonParent] ([Discriminator], [Name], [Token])
OUTPUT INSERTED.[Id]
VALUES (@p0, @p1, @p2);
INSERT INTO [NullableJsonParent] ([Children], [InitializedChildren], [Discriminator], [Name], [Token])
OUTPUT INSERTED.[Id]
VALUES (@p3, @p4, @p5, @p6, @p7);
""",
                //
                """
SELECT [n].[Id], [n].[Discriminator], [n].[Name], [n].[Token], [n].[Children], [n].[InitializedChildren], [n].[Outer], [n].[Reference], [n].[DerivedChildren]
FROM [NullableJsonParent] AS [n]
ORDER BY [n].[Name]
""");
        }
    }

    public override async Task Update_of_a_disconnected_entity_saves_the_nullness_of_a_nullable_json_collection(bool toNull)
    {
        await base.Update_of_a_disconnected_entity_saves_the_nullness_of_a_nullable_json_collection(toNull);

        if (toNull)
        {
            AssertSql(
                """
@p0='[]' (Nullable = false) (Size = 2)
@p1='[]' (Nullable = false) (Size = 2)
@p2='NullableJsonParent' (Nullable = false) (Size = 34)
@p3='A' (Size = 4000)
@p4=NULL (Size = 4000)

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
INSERT INTO [NullableJsonParent] ([Children], [InitializedChildren], [Discriminator], [Name], [Token])
OUTPUT INSERTED.[Id]
VALUES (@p0, @p1, @p2, @p3, @p4);
""",
                //
                """
@p0=NULL (Nullable = false)
@p1='[]' (Nullable = false) (Size = 2)
@p2=NULL (Nullable = false)
@p5='1'
@p3='A' (Size = 4000)
@p4=NULL (Size = 4000)
@p6=NULL (Size = 4000)

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [NullableJsonParent] SET [Children] = @p0, [InitializedChildren] = @p1, [Outer] = @p2, [Name] = @p3, [Token] = @p4
OUTPUT 1
WHERE [Id] = @p5 AND [Token] IS NULL;
""",
                //
                """
SELECT TOP(2) [n].[Id], [n].[Discriminator], [n].[Name], [n].[Token], [n].[Children], [n].[InitializedChildren], [n].[Outer], [n].[Reference], [n].[DerivedChildren]
FROM [NullableJsonParent] AS [n]
""");
        }
        else
        {
            AssertSql(
                """
@p0='[]' (Nullable = false) (Size = 2)
@p1='NullableJsonParent' (Nullable = false) (Size = 34)
@p2='A' (Size = 4000)
@p3=NULL (Size = 4000)

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
INSERT INTO [NullableJsonParent] ([InitializedChildren], [Discriminator], [Name], [Token])
OUTPUT INSERTED.[Id]
VALUES (@p0, @p1, @p2, @p3);
""",
                //
                """
@p0='[]' (Nullable = false) (Size = 2)
@p1='[]' (Nullable = false) (Size = 2)
@p2=NULL (Nullable = false)
@p5='1'
@p3='A' (Size = 4000)
@p4=NULL (Size = 4000)
@p6=NULL (Size = 4000)

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [NullableJsonParent] SET [Children] = @p0, [InitializedChildren] = @p1, [Outer] = @p2, [Name] = @p3, [Token] = @p4
OUTPUT 1
WHERE [Id] = @p5 AND [Token] IS NULL;
""",
                //
                """
SELECT TOP(2) [n].[Id], [n].[Discriminator], [n].[Name], [n].[Token], [n].[Children], [n].[InitializedChildren], [n].[Outer], [n].[Reference], [n].[DerivedChildren]
FROM [NullableJsonParent] AS [n]
""");
        }
    }

    public override async Task Replacing_an_owner_with_the_same_key_saves_the_nullness_of_a_nullable_json_collection(bool toNull)
    {
        await base.Replacing_an_owner_with_the_same_key_saves_the_nullness_of_a_nullable_json_collection(toNull);

        if (toNull)
        {
            AssertSql(
                """
@p0='[]' (Nullable = false) (Size = 2)
@p1='[]' (Nullable = false) (Size = 2)
@p2='NullableJsonParent' (Nullable = false) (Size = 34)
@p3='A' (Size = 4000)
@p4=NULL (Size = 4000)

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
INSERT INTO [NullableJsonParent] ([Children], [InitializedChildren], [Discriminator], [Name], [Token])
OUTPUT INSERTED.[Id]
VALUES (@p0, @p1, @p2, @p3, @p4);
""",
                //
                """
SELECT TOP(2) [n].[Id], [n].[Discriminator], [n].[Name], [n].[Token], [n].[Children], [n].[InitializedChildren], [n].[Outer], [n].[Reference], [n].[DerivedChildren]
FROM [NullableJsonParent] AS [n]
""",
                //
                """
@p0=NULL (Nullable = false)
@p1='[]' (Nullable = false) (Size = 2)
@p2=NULL (Nullable = false)
@p4='1'
@p3='B' (Size = 4000)
@p5=NULL (Size = 4000)

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [NullableJsonParent] SET [Children] = @p0, [InitializedChildren] = @p1, [Outer] = @p2, [Name] = @p3
OUTPUT 1
WHERE [Id] = @p4 AND [Token] IS NULL;
""",
                //
                """
SELECT TOP(2) [n].[Id], [n].[Discriminator], [n].[Name], [n].[Token], [n].[Children], [n].[InitializedChildren], [n].[Outer], [n].[Reference], [n].[DerivedChildren]
FROM [NullableJsonParent] AS [n]
""");
        }
        else
        {
            AssertSql(
                """
@p0='[]' (Nullable = false) (Size = 2)
@p1='NullableJsonParent' (Nullable = false) (Size = 34)
@p2='A' (Size = 4000)
@p3=NULL (Size = 4000)

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
INSERT INTO [NullableJsonParent] ([InitializedChildren], [Discriminator], [Name], [Token])
OUTPUT INSERTED.[Id]
VALUES (@p0, @p1, @p2, @p3);
""",
                //
                """
SELECT TOP(2) [n].[Id], [n].[Discriminator], [n].[Name], [n].[Token], [n].[Children], [n].[InitializedChildren], [n].[Outer], [n].[Reference], [n].[DerivedChildren]
FROM [NullableJsonParent] AS [n]
""",
                //
                """
@p0='[]' (Nullable = false) (Size = 2)
@p1='[]' (Nullable = false) (Size = 2)
@p2=NULL (Nullable = false)
@p4='1'
@p3='B' (Size = 4000)
@p5=NULL (Size = 4000)

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [NullableJsonParent] SET [Children] = @p0, [InitializedChildren] = @p1, [Outer] = @p2, [Name] = @p3
OUTPUT 1
WHERE [Id] = @p4 AND [Token] IS NULL;
""",
                //
                """
SELECT TOP(2) [n].[Id], [n].[Discriminator], [n].[Name], [n].[Token], [n].[Children], [n].[InitializedChildren], [n].[Outer], [n].[Reference], [n].[DerivedChildren]
FROM [NullableJsonParent] AS [n]
""");
        }
    }

    public override async Task Mixed_changes_of_nullable_json_collections_are_saved_in_one_batch()
    {
        await base.Mixed_changes_of_nullable_json_collections_are_saved_in_one_batch();

        AssertSql(
            """
@p0='[]' (Nullable = false) (Size = 2)
@p1='[]' (Nullable = false) (Size = 2)
@p2='NullableJsonParent' (Nullable = false) (Size = 34)
@p3='ToNull' (Size = 4000)
@p4=NULL (Size = 4000)
@p5='[]' (Nullable = false) (Size = 2)
@p6='NullableJsonParent' (Nullable = false) (Size = 34)
@p7='ToEmpty' (Size = 4000)
@p8=NULL (Size = 4000)
@p9='[]' (Nullable = false) (Size = 2)
@p10='[]' (Nullable = false) (Size = 2)
@p11='NullableJsonParent' (Nullable = false) (Size = 34)
@p12='OnlyName' (Size = 4000)
@p13=NULL (Size = 4000)
@p14='[{"Name":"C1"}]' (Nullable = false) (Size = 15)
@p15='[]' (Nullable = false) (Size = 2)
@p16='NullableJsonParent' (Nullable = false) (Size = 34)
@p17='Elements' (Size = 4000)
@p18=NULL (Size = 4000)

SET NOCOUNT ON;
INSERT INTO [NullableJsonParent] ([Children], [InitializedChildren], [Discriminator], [Name], [Token])
OUTPUT INSERTED.[Id]
VALUES (@p0, @p1, @p2, @p3, @p4);
INSERT INTO [NullableJsonParent] ([InitializedChildren], [Discriminator], [Name], [Token])
OUTPUT INSERTED.[Id]
VALUES (@p5, @p6, @p7, @p8);
MERGE [NullableJsonParent] USING (
VALUES (@p9, @p10, @p11, @p12, @p13, 0),
(@p14, @p15, @p16, @p17, @p18, 1)) AS i ([Children], [InitializedChildren], [Discriminator], [Name], [Token], _Position) ON 1=0
WHEN NOT MATCHED THEN
INSERT ([Children], [InitializedChildren], [Discriminator], [Name], [Token])
VALUES (i.[Children], i.[InitializedChildren], i.[Discriminator], i.[Name], i.[Token])
OUTPUT INSERTED.[Id], i._Position;
""",
            //
            """
SELECT [n].[Id], [n].[Discriminator], [n].[Name], [n].[Token], [n].[Children], [n].[InitializedChildren], [n].[Outer], [n].[Reference], [n].[DerivedChildren]
FROM [NullableJsonParent] AS [n]
""",
            //
            """
@p0=NULL (Nullable = false)
@p1='[]' (Nullable = false) (Size = 2)
@p2='1'
@p3=NULL (Size = 4000)
@p4='[]' (Nullable = false) (Size = 2)
@p5='[]' (Nullable = false) (Size = 2)
@p6='2'
@p7=NULL (Size = 4000)
@p8='[]' (Nullable = false) (Size = 2)
@p9='[]' (Nullable = false) (Size = 2)
@p11='3'
@p10='OnlyNameChanged' (Size = 4000)
@p12=NULL (Size = 4000)
@p13='[{"Name":"C1"},{"Name":"C2"}]' (Nullable = false) (Size = 29)
@p14='[]' (Nullable = false) (Size = 2)
@p15='4'
@p16=NULL (Size = 4000)

SET NOCOUNT ON;
UPDATE [NullableJsonParent] SET [Children] = @p0, [InitializedChildren] = @p1
OUTPUT 1
WHERE [Id] = @p2 AND [Token] IS NULL;
UPDATE [NullableJsonParent] SET [Children] = @p4, [InitializedChildren] = @p5
OUTPUT 1
WHERE [Id] = @p6 AND [Token] IS NULL;
UPDATE [NullableJsonParent] SET [Children] = @p8, [InitializedChildren] = @p9, [Name] = @p10
OUTPUT 1
WHERE [Id] = @p11 AND [Token] IS NULL;
UPDATE [NullableJsonParent] SET [Children] = @p13, [InitializedChildren] = @p14
OUTPUT 1
WHERE [Id] = @p15 AND [Token] IS NULL;
""",
            //
            """
SELECT [n].[Id], [n].[Discriminator], [n].[Name], [n].[Token], [n].[Children], [n].[InitializedChildren], [n].[Outer], [n].[Reference], [n].[DerivedChildren]
FROM [NullableJsonParent] AS [n]
""");
    }

    public override async Task Querying_a_tracked_entity_does_not_overwrite_a_pending_change_of_a_nullable_json_collection(bool toNull)
    {
        await base.Querying_a_tracked_entity_does_not_overwrite_a_pending_change_of_a_nullable_json_collection(toNull);

        if (toNull)
        {
            AssertSql(
                """
@p0='[]' (Nullable = false) (Size = 2)
@p1='[]' (Nullable = false) (Size = 2)
@p2='NullableJsonParent' (Nullable = false) (Size = 34)
@p3='A' (Size = 4000)
@p4=NULL (Size = 4000)

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
INSERT INTO [NullableJsonParent] ([Children], [InitializedChildren], [Discriminator], [Name], [Token])
OUTPUT INSERTED.[Id]
VALUES (@p0, @p1, @p2, @p3, @p4);
""",
                //
                """
SELECT TOP(2) [n].[Id], [n].[Discriminator], [n].[Name], [n].[Token], [n].[Children], [n].[InitializedChildren], [n].[Outer], [n].[Reference], [n].[DerivedChildren]
FROM [NullableJsonParent] AS [n]
""",
                //
                """
SELECT TOP(2) [n].[Id], [n].[Discriminator], [n].[Name], [n].[Token], [n].[Children], [n].[InitializedChildren], [n].[Outer], [n].[Reference], [n].[DerivedChildren]
FROM [NullableJsonParent] AS [n]
""",
                //
                """
@p0=NULL (Nullable = false)
@p1='[]' (Nullable = false) (Size = 2)
@p2='1'
@p3=NULL (Size = 4000)

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [NullableJsonParent] SET [Children] = @p0, [InitializedChildren] = @p1
OUTPUT 1
WHERE [Id] = @p2 AND [Token] IS NULL;
""",
                //
                """
SELECT TOP(2) [n].[Id], [n].[Discriminator], [n].[Name], [n].[Token], [n].[Children], [n].[InitializedChildren], [n].[Outer], [n].[Reference], [n].[DerivedChildren]
FROM [NullableJsonParent] AS [n]
""");
        }
        else
        {
            AssertSql(
                """
@p0='[]' (Nullable = false) (Size = 2)
@p1='NullableJsonParent' (Nullable = false) (Size = 34)
@p2='A' (Size = 4000)
@p3=NULL (Size = 4000)

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
INSERT INTO [NullableJsonParent] ([InitializedChildren], [Discriminator], [Name], [Token])
OUTPUT INSERTED.[Id]
VALUES (@p0, @p1, @p2, @p3);
""",
                //
                """
SELECT TOP(2) [n].[Id], [n].[Discriminator], [n].[Name], [n].[Token], [n].[Children], [n].[InitializedChildren], [n].[Outer], [n].[Reference], [n].[DerivedChildren]
FROM [NullableJsonParent] AS [n]
""",
                //
                """
SELECT TOP(2) [n].[Id], [n].[Discriminator], [n].[Name], [n].[Token], [n].[Children], [n].[InitializedChildren], [n].[Outer], [n].[Reference], [n].[DerivedChildren]
FROM [NullableJsonParent] AS [n]
""",
                //
                """
@p0='[]' (Nullable = false) (Size = 2)
@p1='[]' (Nullable = false) (Size = 2)
@p2='1'
@p3=NULL (Size = 4000)

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [NullableJsonParent] SET [Children] = @p0, [InitializedChildren] = @p1
OUTPUT 1
WHERE [Id] = @p2 AND [Token] IS NULL;
""",
                //
                """
SELECT TOP(2) [n].[Id], [n].[Discriminator], [n].[Name], [n].[Token], [n].[Children], [n].[InitializedChildren], [n].[Outer], [n].[Reference], [n].[DerivedChildren]
FROM [NullableJsonParent] AS [n]
""");
        }
    }

    public override async Task Streamed_duplicate_of_a_tracked_entity_does_not_overwrite_a_change_to_a_nullable_json_collection()
    {
        await base.Streamed_duplicate_of_a_tracked_entity_does_not_overwrite_a_change_to_a_nullable_json_collection();

        AssertSql(
            """
@p0='[]' (Nullable = false) (Size = 2)
@p1='NullableJsonParent' (Nullable = false) (Size = 34)
@p2='A' (Size = 4000)
@p3=NULL (Size = 4000)
@p4='[]' (Nullable = false) (Size = 2)
@p5='[]' (Nullable = false) (Size = 2)
@p6='NullableJsonParent' (Nullable = false) (Size = 34)
@p7='B' (Size = 4000)
@p8=NULL (Size = 4000)

SET NOCOUNT ON;
INSERT INTO [NullableJsonParent] ([InitializedChildren], [Discriminator], [Name], [Token])
OUTPUT INSERTED.[Id]
VALUES (@p0, @p1, @p2, @p3);
INSERT INTO [NullableJsonParent] ([Children], [InitializedChildren], [Discriminator], [Name], [Token])
OUTPUT INSERTED.[Id]
VALUES (@p4, @p5, @p6, @p7, @p8);
""",
            //
            """
SELECT [n].[Id], [n].[Discriminator], [n].[Name], [n].[Token], [n].[Children], [n].[InitializedChildren], [n].[Outer], [n].[Reference], [n].[DerivedChildren]
FROM [NullableJsonParent] AS [n]
CROSS JOIN [NullableJsonParent] AS [n0]
WHERE [n].[Name] = N'A'
""",
            //
            """
@p0='[]' (Nullable = false) (Size = 2)
@p1='[]' (Nullable = false) (Size = 2)
@p2='1'
@p3=NULL (Size = 4000)

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [NullableJsonParent] SET [Children] = @p0, [InitializedChildren] = @p1
OUTPUT 1
WHERE [Id] = @p2 AND [Token] IS NULL;
""",
            //
            """
SELECT TOP(2) [n].[Id], [n].[Discriminator], [n].[Name], [n].[Token], [n].[Children], [n].[InitializedChildren], [n].[Outer], [n].[Reference], [n].[DerivedChildren]
FROM [NullableJsonParent] AS [n]
WHERE [n].[Name] = N'A'
""");
    }

    public override async Task Compiled_query_enumerated_twice_does_not_overwrite_a_pending_change_of_a_nullable_json_collection(bool async)
    {
        await base.Compiled_query_enumerated_twice_does_not_overwrite_a_pending_change_of_a_nullable_json_collection(async);

        AssertSql(
            """
@p0='[]' (Nullable = false) (Size = 2)
@p1='NullableJsonParent' (Nullable = false) (Size = 34)
@p2='A' (Size = 4000)
@p3=NULL (Size = 4000)
@p4='[]' (Nullable = false) (Size = 2)
@p5='[]' (Nullable = false) (Size = 2)
@p6='NullableJsonParent' (Nullable = false) (Size = 34)
@p7='B' (Size = 4000)
@p8=NULL (Size = 4000)

SET NOCOUNT ON;
INSERT INTO [NullableJsonParent] ([InitializedChildren], [Discriminator], [Name], [Token])
OUTPUT INSERTED.[Id]
VALUES (@p0, @p1, @p2, @p3);
INSERT INTO [NullableJsonParent] ([Children], [InitializedChildren], [Discriminator], [Name], [Token])
OUTPUT INSERTED.[Id]
VALUES (@p4, @p5, @p6, @p7, @p8);
""",
            //
            """
SELECT [n].[Id], [n].[Discriminator], [n].[Name], [n].[Token], [n].[Children], [n].[InitializedChildren], [n].[Outer], [n].[Reference], [n].[DerivedChildren]
FROM [NullableJsonParent] AS [n]
WHERE [n].[Name] = N'A'
""",
            //
            """
SELECT [n].[Id], [n].[Discriminator], [n].[Name], [n].[Token], [n].[Children], [n].[InitializedChildren], [n].[Outer], [n].[Reference], [n].[DerivedChildren]
FROM [NullableJsonParent] AS [n]
WHERE [n].[Name] = N'A'
""",
            //
            """
@p0='[]' (Nullable = false) (Size = 2)
@p1='[]' (Nullable = false) (Size = 2)
@p2='1'
@p3=NULL (Size = 4000)

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [NullableJsonParent] SET [Children] = @p0, [InitializedChildren] = @p1
OUTPUT 1
WHERE [Id] = @p2 AND [Token] IS NULL;
""",
            //
            """
SELECT TOP(2) [n].[Id], [n].[Discriminator], [n].[Name], [n].[Token], [n].[Children], [n].[InitializedChildren], [n].[Outer], [n].[Reference], [n].[DerivedChildren]
FROM [NullableJsonParent] AS [n]
WHERE [n].[Name] = N'A'
""");
    }

    public override async Task Querying_a_tracked_entity_keeps_a_pending_null_of_a_nullable_json_collection_that_is_stored_with_elements()
    {
        await base.Querying_a_tracked_entity_keeps_a_pending_null_of_a_nullable_json_collection_that_is_stored_with_elements();

        AssertSql(
            """
@p0='[{"Name":"C1"},{"Name":"C2"}]' (Nullable = false) (Size = 29)
@p1='[]' (Nullable = false) (Size = 2)
@p2='NullableJsonParent' (Nullable = false) (Size = 34)
@p3='A' (Size = 4000)
@p4=NULL (Size = 4000)

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
INSERT INTO [NullableJsonParent] ([Children], [InitializedChildren], [Discriminator], [Name], [Token])
OUTPUT INSERTED.[Id]
VALUES (@p0, @p1, @p2, @p3, @p4);
""",
            //
            """
SELECT TOP(2) [n].[Id], [n].[Discriminator], [n].[Name], [n].[Token], [n].[Children], [n].[InitializedChildren], [n].[Outer], [n].[Reference], [n].[DerivedChildren]
FROM [NullableJsonParent] AS [n]
""",
            //
            """
@p0=NULL (Nullable = false)
@p1='[]' (Nullable = false) (Size = 2)
@p2=NULL (Nullable = false)
@p5='1'
@p3='A' (Size = 4000)
@p4=NULL (Size = 4000)
@p6=NULL (Size = 4000)

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [NullableJsonParent] SET [Children] = @p0, [InitializedChildren] = @p1, [Outer] = @p2, [Name] = @p3, [Token] = @p4
OUTPUT 1
WHERE [Id] = @p5 AND [Token] IS NULL;
""",
            //
            """
SELECT TOP(2) [n].[Id], [n].[Discriminator], [n].[Name], [n].[Token], [n].[Children], [n].[InitializedChildren], [n].[Outer], [n].[Reference], [n].[DerivedChildren]
FROM [NullableJsonParent] AS [n]
""");
    }

    public override async Task Querying_a_tracked_entity_keeps_the_in_memory_nullness_of_a_nullable_json_collection(bool inMemoryNull)
    {
        await base.Querying_a_tracked_entity_keeps_the_in_memory_nullness_of_a_nullable_json_collection(inMemoryNull);

        if (inMemoryNull)
        {
            AssertSql(
                """
@p0='[]' (Nullable = false) (Size = 2)
@p1='NullableJsonParent' (Nullable = false) (Size = 34)
@p2='A' (Size = 4000)
@p3=NULL (Size = 4000)

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
INSERT INTO [NullableJsonParent] ([InitializedChildren], [Discriminator], [Name], [Token])
OUTPUT INSERTED.[Id]
VALUES (@p0, @p1, @p2, @p3);
""",
                //
                """
SELECT TOP(2) [n].[Id], [n].[Discriminator], [n].[Name], [n].[Token], [n].[Children], [n].[InitializedChildren], [n].[Outer], [n].[Reference], [n].[DerivedChildren]
FROM [NullableJsonParent] AS [n]
""",
                //
                """
UPDATE [NullableJsonParent] SET [Children] = '[]'
""",
                //
                """
SELECT TOP(2) [n].[Id], [n].[Discriminator], [n].[Name], [n].[Token], [n].[Children], [n].[InitializedChildren], [n].[Outer], [n].[Reference], [n].[DerivedChildren]
FROM [NullableJsonParent] AS [n]
""");
        }
        else
        {
            AssertSql(
                """
@p0='[{"Name":"C1"}]' (Nullable = false) (Size = 15)
@p1='[]' (Nullable = false) (Size = 2)
@p2='NullableJsonParent' (Nullable = false) (Size = 34)
@p3='A' (Size = 4000)
@p4=NULL (Size = 4000)

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
INSERT INTO [NullableJsonParent] ([Children], [InitializedChildren], [Discriminator], [Name], [Token])
OUTPUT INSERTED.[Id]
VALUES (@p0, @p1, @p2, @p3, @p4);
""",
                //
                """
SELECT TOP(2) [n].[Id], [n].[Discriminator], [n].[Name], [n].[Token], [n].[Children], [n].[InitializedChildren], [n].[Outer], [n].[Reference], [n].[DerivedChildren]
FROM [NullableJsonParent] AS [n]
""",
                //
                """
UPDATE [NullableJsonParent] SET [Children] = NULL
""",
                //
                """
SELECT TOP(2) [n].[Id], [n].[Discriminator], [n].[Name], [n].[Token], [n].[Children], [n].[InitializedChildren], [n].[Outer], [n].[Reference], [n].[DerivedChildren]
FROM [NullableJsonParent] AS [n]
""");
        }
    }

    public override async Task Json_collection_that_is_not_nullable_or_opted_out_stored_as_null_is_loaded_as_empty_with_initializer()
    {
        await base.Json_collection_that_is_not_nullable_or_opted_out_stored_as_null_is_loaded_as_empty_with_initializer();

        AssertSql(
            """
SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
INSERT INTO [NotNullableJsonParent]
OUTPUT INSERTED.[Id]
DEFAULT VALUES;
""",
            //
            """
SELECT TOP(2) [n].[Id], [n].[Children], [n].[OptedOutChildren], [n].[RequiredChildren]
FROM [NotNullableJsonParent] AS [n]
""");
    }

    public override async Task Nullable_json_collection_with_a_coercing_setter_loaded_as_null_has_no_changes()
    {
        await base.Nullable_json_collection_with_a_coercing_setter_loaded_as_null_has_no_changes();

        AssertSql(
            """
@p0='[]' (Nullable = false) (Size = 2)
@p1='A' (Size = 4000)

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
INSERT INTO [CoercingNullableJsonParent] ([Children], [Name])
OUTPUT INSERTED.[Id]
VALUES (@p0, @p1);
""",
            //
            """
UPDATE [CoercingNullableJsonParent] SET [Children] = NULL
""",
            //
            """
SELECT TOP(2) [c].[Id], [c].[Name], [c].[Children]
FROM [CoercingNullableJsonParent] AS [c]
""");
    }

    public override async Task Nullable_json_collection_of_a_notifying_entity_changed_from_empty_to_null_is_saved(
        ChangeTrackingStrategy strategy)
    {
        await base.Nullable_json_collection_of_a_notifying_entity_changed_from_empty_to_null_is_saved(strategy);

        AssertSql(
            """
@p0='[]' (Nullable = false) (Size = 2)
@p1='[]' (Nullable = false) (Size = 2)
@p2='A' (Size = 4000)

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
INSERT INTO [NotifyingNullableJsonParent] ([Children], [InitializedChildren], [Name])
OUTPUT INSERTED.[Id]
VALUES (@p0, @p1, @p2);
""",
            //
            """
SELECT TOP(2) [n].[Id], [n].[Name], [n].[Children], [n].[InitializedChildren], [n].[Outer]
FROM [NotifyingNullableJsonParent] AS [n]
""",
            //
            """
@p0=NULL (Nullable = false)
@p1='[]' (Nullable = false) (Size = 2)
@p2='1'

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [NotifyingNullableJsonParent] SET [Children] = @p0, [InitializedChildren] = @p1
OUTPUT 1
WHERE [Id] = @p2;
""",
            //
            """
SELECT TOP(2) [n].[Id], [n].[Name], [n].[Children], [n].[InitializedChildren], [n].[Outer]
FROM [NotifyingNullableJsonParent] AS [n]
""");
    }

    public override async Task Nullable_json_collection_of_a_notifying_entity_loaded_by_a_query_is_notified_of_added_and_removed_elements(
        ChangeTrackingStrategy strategy)
    {
        await base.Nullable_json_collection_of_a_notifying_entity_loaded_by_a_query_is_notified_of_added_and_removed_elements(strategy);

        AssertSql(
            """
@p0='[{"Name":"C1"},{"Name":"C2"}]' (Nullable = false) (Size = 29)
@p1='[]' (Nullable = false) (Size = 2)
@p2='A' (Size = 4000)
@p3='[]' (Nullable = false) (Size = 2)
@p4='[]' (Nullable = false) (Size = 2)
@p5='B' (Size = 4000)

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
MERGE [NotifyingNullableJsonParent] USING (
VALUES (@p0, @p1, @p2, 0),
(@p3, @p4, @p5, 1)) AS i ([Children], [InitializedChildren], [Name], _Position) ON 1=0
WHEN NOT MATCHED THEN
INSERT ([Children], [InitializedChildren], [Name])
VALUES (i.[Children], i.[InitializedChildren], i.[Name])
OUTPUT INSERTED.[Id], i._Position;
""",
            //
            """
SELECT [n].[Id], [n].[Name], [n].[Children], [n].[InitializedChildren], [n].[Outer]
FROM [NotifyingNullableJsonParent] AS [n]
ORDER BY [n].[Name]
""",
            //
            """
@p0='[{"Name":"C2"},{"Name":"C3"}]' (Nullable = false) (Size = 29)
@p1='[]' (Nullable = false) (Size = 2)
@p2='1'
@p3='[{"Name":"D1"}]' (Nullable = false) (Size = 15)
@p4='[]' (Nullable = false) (Size = 2)
@p5='2'

SET NOCOUNT ON;
UPDATE [NotifyingNullableJsonParent] SET [Children] = @p0, [InitializedChildren] = @p1
OUTPUT 1
WHERE [Id] = @p2;
UPDATE [NotifyingNullableJsonParent] SET [Children] = @p3, [InitializedChildren] = @p4
OUTPUT 1
WHERE [Id] = @p5;
""",
            //
            """
SELECT [n].[Id], [n].[Name], [n].[Children], [n].[InitializedChildren], [n].[Outer]
FROM [NotifyingNullableJsonParent] AS [n]
ORDER BY [n].[Name]
""");
    }

    public override async Task Nullable_json_collections_of_a_notifying_entity_stored_as_null_stay_null_when_tracked(
        ChangeTrackingStrategy strategy)
    {
        await base.Nullable_json_collections_of_a_notifying_entity_stored_as_null_stay_null_when_tracked(strategy);

        AssertSql(
            """
@p0='[{"Name":"I1","Inner":null},{"Name":"I2","Inner":[]}]' (Nullable = false) (Size = 53)
@p1='[]' (Nullable = false) (Size = 2)
@p2='A' (Size = 4000)

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
INSERT INTO [NotifyingNullableJsonParent] ([Outer], [InitializedChildren], [Name])
OUTPUT INSERTED.[Id]
VALUES (@p0, @p1, @p2);
""",
            //
            """
SELECT TOP(2) [n].[Id], [n].[Name], [n].[Children], [n].[InitializedChildren], [n].[Outer]
FROM [NotifyingNullableJsonParent] AS [n]
""",
            //
            """
@p0='[]' (Nullable = false) (Size = 2)
@p2='1'
@p1='B' (Size = 4000)

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [NotifyingNullableJsonParent] SET [InitializedChildren] = @p0, [Name] = @p1
OUTPUT 1
WHERE [Id] = @p2;
""",
            //
            """
SELECT TOP(2) [n].[Id], [n].[Name], [n].[Children], [n].[InitializedChildren], [n].[Outer]
FROM [NotifyingNullableJsonParent] AS [n]
""");
    }

    public override async Task Nullable_json_collection_of_a_notifying_entity_with_an_initializer_loaded_as_null_has_no_changes(
        ChangeTrackingStrategy strategy)
    {
        await base.Nullable_json_collection_of_a_notifying_entity_with_an_initializer_loaded_as_null_has_no_changes(strategy);

        AssertSql(
            """
@p0='[]' (Nullable = false) (Size = 2)
@p1='A' (Size = 4000)

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
INSERT INTO [NotifyingNullableJsonParent] ([InitializedChildren], [Name])
OUTPUT INSERTED.[Id]
VALUES (@p0, @p1);
""",
            //
            """
UPDATE [NotifyingNullableJsonParent] SET [InitializedChildren] = NULL
""",
            //
            """
SELECT TOP(2) [n].[Id], [n].[Name], [n].[Children], [n].[InitializedChildren], [n].[Outer]
FROM [NotifyingNullableJsonParent] AS [n]
""");
    }

    protected override ITestStoreFactory NonSharedTestStoreFactory
        => SqlServerTestStoreFactory.Instance;
}
