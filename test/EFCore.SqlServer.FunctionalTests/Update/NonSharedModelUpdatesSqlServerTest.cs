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

    public override async Task Nullable_json_collection_changed_from_empty_to_null_is_saved(bool alsoModifyName)
    {
        await base.Nullable_json_collection_changed_from_empty_to_null_is_saved(alsoModifyName);

        if (alsoModifyName)
        {
            AssertSql(
                """
@p0='[]' (Nullable = false) (Size = 2)
@p1='A' (Size = 4000)

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
INSERT INTO [NullableJsonParent] ([Children], [Name])
OUTPUT INSERTED.[Id]
VALUES (@p0, @p1);
""",
                //
                """
SELECT TOP(2) [n].[Id], [n].[Name], [n].[Children]
FROM [NullableJsonParent] AS [n]
""",
                //
                """
@p0=NULL (Nullable = false)
@p2='1'
@p1='B' (Size = 4000)

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [NullableJsonParent] SET [Children] = @p0, [Name] = @p1
OUTPUT 1
WHERE [Id] = @p2;
""",
                //
                """
SELECT TOP(2) [n].[Id], [n].[Name], [n].[Children]
FROM [NullableJsonParent] AS [n]
""");
        }
        else
        {
            AssertSql(
                """
@p0='[]' (Nullable = false) (Size = 2)
@p1='A' (Size = 4000)

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
INSERT INTO [NullableJsonParent] ([Children], [Name])
OUTPUT INSERTED.[Id]
VALUES (@p0, @p1);
""",
                //
                """
SELECT TOP(2) [n].[Id], [n].[Name], [n].[Children]
FROM [NullableJsonParent] AS [n]
""",
                //
                """
@p0=NULL (Nullable = false)
@p1='1'

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [NullableJsonParent] SET [Children] = @p0
OUTPUT 1
WHERE [Id] = @p1;
""",
                //
                """
SELECT TOP(2) [n].[Id], [n].[Name], [n].[Children]
FROM [NullableJsonParent] AS [n]
""");
        }
    }

    public override async Task Nullable_json_collection_changed_from_null_to_empty_is_saved(bool alsoModifyName)
    {
        await base.Nullable_json_collection_changed_from_null_to_empty_is_saved(alsoModifyName);

        if (alsoModifyName)
        {
            AssertSql(
                """
@p0='A' (Size = 4000)

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
INSERT INTO [NullableJsonParent] ([Name])
OUTPUT INSERTED.[Id]
VALUES (@p0);
""",
                //
                """
SELECT TOP(2) [n].[Id], [n].[Name], [n].[Children]
FROM [NullableJsonParent] AS [n]
""",
                //
                """
@p0='[]' (Nullable = false) (Size = 2)
@p2='1'
@p1='B' (Size = 4000)

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [NullableJsonParent] SET [Children] = @p0, [Name] = @p1
OUTPUT 1
WHERE [Id] = @p2;
""",
                //
                """
SELECT TOP(2) [n].[Id], [n].[Name], [n].[Children]
FROM [NullableJsonParent] AS [n]
""");
        }
        else
        {
            AssertSql(
                """
@p0='A' (Size = 4000)

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
INSERT INTO [NullableJsonParent] ([Name])
OUTPUT INSERTED.[Id]
VALUES (@p0);
""",
                //
                """
SELECT TOP(2) [n].[Id], [n].[Name], [n].[Children]
FROM [NullableJsonParent] AS [n]
""",
                //
                """
@p0='[]' (Nullable = false) (Size = 2)
@p1='1'

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [NullableJsonParent] SET [Children] = @p0
OUTPUT 1
WHERE [Id] = @p1;
""",
                //
                """
SELECT TOP(2) [n].[Id], [n].[Name], [n].[Children]
FROM [NullableJsonParent] AS [n]
""");
        }
    }

    public override async Task Nullable_json_collection_with_elements_changed_to_null_is_saved()
    {
        await base.Nullable_json_collection_with_elements_changed_to_null_is_saved();

        AssertSql(
            """
@p0='[{"Name":"C1"}]' (Nullable = false) (Size = 15)
@p1='A' (Size = 4000)

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
INSERT INTO [NullableJsonParent] ([Children], [Name])
OUTPUT INSERTED.[Id]
VALUES (@p0, @p1);
""",
            //
            """
SELECT TOP(2) [n].[Id], [n].[Name], [n].[Children]
FROM [NullableJsonParent] AS [n]
""",
            //
            """
@p0=NULL (Nullable = false)
@p1='1'

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [NullableJsonParent] SET [Children] = @p0
OUTPUT 1
WHERE [Id] = @p1;
""",
            //
            """
SELECT TOP(2) [n].[Id], [n].[Name], [n].[Children]
FROM [NullableJsonParent] AS [n]
""");
    }

    public override async Task Nullable_json_collection_nested_in_json_changed_from_empty_to_null_is_saved(bool alsoModifyReference)
    {
        await base.Nullable_json_collection_nested_in_json_changed_from_empty_to_null_is_saved(alsoModifyReference);

        if (alsoModifyReference)
        {
            AssertSql(
                """
@p0='{"Name":"R","Items":[]}' (Nullable = false) (Size = 23)
@p1='A' (Size = 4000)

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
INSERT INTO [NullableJsonReferenceParent] ([Reference], [Name])
OUTPUT INSERTED.[Id]
VALUES (@p0, @p1);
""",
                //
                """
SELECT TOP(2) [n].[Id], [n].[Name], [n].[Reference]
FROM [NullableJsonReferenceParent] AS [n]
""",
                //
                """
@p0='{"Name":"R2","Items":null}' (Nullable = false) (Size = 26)
@p1='1'

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [NullableJsonReferenceParent] SET [Reference] = @p0
OUTPUT 1
WHERE [Id] = @p1;
""",
                //
                """
SELECT TOP(2) [n].[Id], [n].[Name], [n].[Reference]
FROM [NullableJsonReferenceParent] AS [n]
""");
        }
        else
        {
            AssertSql(
                """
@p0='{"Name":"R","Items":[]}' (Nullable = false) (Size = 23)
@p1='A' (Size = 4000)

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
INSERT INTO [NullableJsonReferenceParent] ([Reference], [Name])
OUTPUT INSERTED.[Id]
VALUES (@p0, @p1);
""",
                //
                """
SELECT TOP(2) [n].[Id], [n].[Name], [n].[Reference]
FROM [NullableJsonReferenceParent] AS [n]
""",
                //
                """
@p0='{"Name":"R","Items":null}' (Nullable = false) (Size = 25)
@p1='1'

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [NullableJsonReferenceParent] SET [Reference] = @p0
OUTPUT 1
WHERE [Id] = @p1;
""",
                //
                """
SELECT TOP(2) [n].[Id], [n].[Name], [n].[Reference]
FROM [NullableJsonReferenceParent] AS [n]
""");
        }
    }

    public override async Task Update_of_a_disconnected_entity_saves_the_null_ness_of_a_nullable_json_collection(bool toNull)
    {
        await base.Update_of_a_disconnected_entity_saves_the_null_ness_of_a_nullable_json_collection(toNull);

        if (toNull)
        {
            AssertSql(
                """
@p0='[]' (Nullable = false) (Size = 2)
@p1='A' (Size = 4000)

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
INSERT INTO [NullableJsonParent] ([Children], [Name])
OUTPUT INSERTED.[Id]
VALUES (@p0, @p1);
""",
                //
                """
@p0=NULL (Nullable = false)
@p2='1'
@p1='A' (Size = 4000)

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [NullableJsonParent] SET [Children] = @p0, [Name] = @p1
OUTPUT 1
WHERE [Id] = @p2;
""",
                //
                """
SELECT TOP(2) [n].[Id], [n].[Name], [n].[Children]
FROM [NullableJsonParent] AS [n]
""");
        }
        else
        {
            AssertSql(
                """
@p0='A' (Size = 4000)

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
INSERT INTO [NullableJsonParent] ([Name])
OUTPUT INSERTED.[Id]
VALUES (@p0);
""",
                //
                """
@p0='[]' (Nullable = false) (Size = 2)
@p2='1'
@p1='A' (Size = 4000)

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [NullableJsonParent] SET [Children] = @p0, [Name] = @p1
OUTPUT 1
WHERE [Id] = @p2;
""",
                //
                """
SELECT TOP(2) [n].[Id], [n].[Name], [n].[Children]
FROM [NullableJsonParent] AS [n]
""");
        }
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

    public override async Task Nullable_json_collection_of_a_notifying_entity_changed_from_empty_to_null_is_saved(
        ChangeTrackingStrategy strategy)
    {
        await base.Nullable_json_collection_of_a_notifying_entity_changed_from_empty_to_null_is_saved(strategy);

        AssertSql(
            """
@p0='[]' (Nullable = false) (Size = 2)
@p1='A' (Size = 4000)

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
INSERT INTO [NotifyingNullableJsonParent] ([Children], [Name])
OUTPUT INSERTED.[Id]
VALUES (@p0, @p1);
""",
            //
            """
SELECT TOP(2) [n].[Id], [n].[Name], [n].[Children]
FROM [NotifyingNullableJsonParent] AS [n]
""",
            //
            """
@p0=NULL (Nullable = false)
@p1='1'

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [NotifyingNullableJsonParent] SET [Children] = @p0
OUTPUT 1
WHERE [Id] = @p1;
""",
            //
            """
SELECT TOP(2) [n].[Id], [n].[Name], [n].[Children]
FROM [NotifyingNullableJsonParent] AS [n]
""");
    }

    public override async Task Replacing_an_owner_with_the_same_key_saves_the_null_ness_of_a_nullable_json_collection(bool toNull)
    {
        await base.Replacing_an_owner_with_the_same_key_saves_the_null_ness_of_a_nullable_json_collection(toNull);

        if (toNull)
        {
            AssertSql(
                """
@p0='[]' (Nullable = false) (Size = 2)
@p1='A' (Size = 4000)

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
INSERT INTO [NullableJsonParent] ([Children], [Name])
OUTPUT INSERTED.[Id]
VALUES (@p0, @p1);
""",
                //
                """
SELECT TOP(2) [n].[Id], [n].[Name], [n].[Children]
FROM [NullableJsonParent] AS [n]
""",
                //
                """
@p0=NULL (Nullable = false)
@p2='1'
@p1='B' (Size = 4000)

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [NullableJsonParent] SET [Children] = @p0, [Name] = @p1
OUTPUT 1
WHERE [Id] = @p2;
""",
                //
                """
SELECT TOP(2) [n].[Id], [n].[Name], [n].[Children]
FROM [NullableJsonParent] AS [n]
""");
        }
        else
        {
            AssertSql(
                """
@p0='A' (Size = 4000)

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
INSERT INTO [NullableJsonParent] ([Name])
OUTPUT INSERTED.[Id]
VALUES (@p0);
""",
                //
                """
SELECT TOP(2) [n].[Id], [n].[Name], [n].[Children]
FROM [NullableJsonParent] AS [n]
""",
                //
                """
@p0='[]' (Nullable = false) (Size = 2)
@p2='1'
@p1='B' (Size = 4000)

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [NullableJsonParent] SET [Children] = @p0, [Name] = @p1
OUTPUT 1
WHERE [Id] = @p2;
""",
                //
                """
SELECT TOP(2) [n].[Id], [n].[Name], [n].[Children]
FROM [NullableJsonParent] AS [n]
""");
        }
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
INSERT INTO [InitializedNotifyingNullableJsonParent] ([Children], [Name])
OUTPUT INSERTED.[Id]
VALUES (@p0, @p1);
""",
            //
            """
UPDATE [InitializedNotifyingNullableJsonParent] SET [Children] = NULL
""",
            //
            """
SELECT TOP(2) [i].[Id], [i].[Name], [i].[Children]
FROM [InitializedNotifyingNullableJsonParent] AS [i]
""");
    }

    public override async Task Querying_a_tracked_entity_keeps_the_in_memory_null_ness_of_a_nullable_json_collection(bool inMemoryNull)
    {
        await base.Querying_a_tracked_entity_keeps_the_in_memory_null_ness_of_a_nullable_json_collection(inMemoryNull);

        if (inMemoryNull)
        {
            AssertSql(
                """
@p0='A' (Size = 4000)

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
INSERT INTO [NullableJsonParent] ([Name])
OUTPUT INSERTED.[Id]
VALUES (@p0);
""",
                //
                """
SELECT TOP(2) [n].[Id], [n].[Name], [n].[Children]
FROM [NullableJsonParent] AS [n]
""",
                //
                """
UPDATE [NullableJsonParent] SET [Children] = '[]'
""",
                //
                """
SELECT TOP(2) [n].[Id], [n].[Name], [n].[Children]
FROM [NullableJsonParent] AS [n]
""");
        }
        else
        {
            AssertSql(
                """
@p0='[{"Name":"C1"}]' (Nullable = false) (Size = 15)
@p1='A' (Size = 4000)

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
INSERT INTO [NullableJsonParent] ([Children], [Name])
OUTPUT INSERTED.[Id]
VALUES (@p0, @p1);
""",
                //
                """
SELECT TOP(2) [n].[Id], [n].[Name], [n].[Children]
FROM [NullableJsonParent] AS [n]
""",
                //
                """
UPDATE [NullableJsonParent] SET [Children] = NULL
""",
                //
                """
SELECT TOP(2) [n].[Id], [n].[Name], [n].[Children]
FROM [NullableJsonParent] AS [n]
""");
        }
    }

    public override async Task Nullable_json_collection_declared_on_a_derived_type_changed_to_null_is_saved()
    {
        await base.Nullable_json_collection_declared_on_a_derived_type_changed_to_null_is_saved();

        AssertSql(
            """
@p0='NullableJsonBase' (Nullable = false) (Size = 21)
@p1='Base' (Size = 4000)
@p2='[]' (Nullable = false) (Size = 2)
@p3='NullableJsonDerived' (Nullable = false) (Size = 21)
@p4='Derived' (Size = 4000)

SET NOCOUNT ON;
INSERT INTO [NullableJsonBase] ([Discriminator], [Name])
OUTPUT INSERTED.[Id]
VALUES (@p0, @p1);
INSERT INTO [NullableJsonBase] ([Children], [Discriminator], [Name])
OUTPUT INSERTED.[Id]
VALUES (@p2, @p3, @p4);
""",
            //
            """
SELECT TOP(2) [n].[Id], [n].[Discriminator], [n].[Name], [n].[Children]
FROM [NullableJsonBase] AS [n]
WHERE [n].[Discriminator] = N'NullableJsonDerived'
""",
            //
            """
@p0=NULL (Nullable = false)
@p1='2'

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [NullableJsonBase] SET [Children] = @p0
OUTPUT 1
WHERE [Id] = @p1;
""",
            //
            """
SELECT [n].[Id], [n].[Discriminator], [n].[Name], [n].[Children]
FROM [NullableJsonBase] AS [n]
ORDER BY [n].[Name]
""");
    }

    public override async Task Nested_nullable_json_collection_changed_to_null_is_saved()
    {
        await base.Nested_nullable_json_collection_changed_to_null_is_saved();

        AssertSql(
            """
@p0='[{"Name":"O1","Inner":[]},{"Name":"O2","Inner":[]}]' (Nullable = false) (Size = 51)

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
INSERT INTO [NestedNullableJsonOwner] ([Items])
OUTPUT INSERTED.[Id]
VALUES (@p0);
""",
            //
            """
SELECT TOP(2) [n].[Id], [n].[Items]
FROM [NestedNullableJsonOwner] AS [n]
""",
            //
            """
@p0='{"Name":"O1","Inner":null}' (Nullable = false) (Size = 26)
@p1='1'

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [NestedNullableJsonOwner] SET [Items] = JSON_MODIFY([Items], 'strict $[0]', JSON_QUERY(@p0))
OUTPUT 1
WHERE [Id] = @p1;
""",
            //
            """
SELECT TOP(2) [n].[Id], [n].[Items]
FROM [NestedNullableJsonOwner] AS [n]
""");
    }

    public override async Task Nullable_json_collection_changed_to_null_is_saved_for_an_entity_with_a_concurrency_token()
    {
        await base.Nullable_json_collection_changed_to_null_is_saved_for_an_entity_with_a_concurrency_token();

        AssertSql(
            """
@p0='[]' (Nullable = false) (Size = 2)
@p1='T1' (Size = 4000)

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
INSERT INTO [NullableJsonParentWithToken] ([Children], [Token])
OUTPUT INSERTED.[Id]
VALUES (@p0, @p1);
""",
            //
            """
SELECT TOP(2) [n].[Id], [n].[Token], [n].[Children]
FROM [NullableJsonParentWithToken] AS [n]
""",
            //
            """
@p0=NULL (Nullable = false)
@p1='1'
@p2='T1' (Size = 4000)

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [NullableJsonParentWithToken] SET [Children] = @p0
OUTPUT 1
WHERE [Id] = @p1 AND [Token] = @p2;
""",
            //
            """
SELECT TOP(2) [n].[Id], [n].[Token], [n].[Children]
FROM [NullableJsonParentWithToken] AS [n]
""");
    }

    public override async Task Mixed_changes_of_nullable_json_collections_are_saved_in_one_batch()
    {
        await base.Mixed_changes_of_nullable_json_collections_are_saved_in_one_batch();

        AssertSql(
            """
@p0='[]' (Nullable = false) (Size = 2)
@p1='ToNull' (Size = 4000)
@p2='ToEmpty' (Size = 4000)
@p3='[]' (Nullable = false) (Size = 2)
@p4='OnlyName' (Size = 4000)
@p5='[{"Name":"C1"}]' (Nullable = false) (Size = 15)
@p6='Elements' (Size = 4000)

SET NOCOUNT ON;
INSERT INTO [NullableJsonParent] ([Children], [Name])
OUTPUT INSERTED.[Id]
VALUES (@p0, @p1);
INSERT INTO [NullableJsonParent] ([Name])
OUTPUT INSERTED.[Id]
VALUES (@p2);
MERGE [NullableJsonParent] USING (
VALUES (@p3, @p4, 0),
(@p5, @p6, 1)) AS i ([Children], [Name], _Position) ON 1=0
WHEN NOT MATCHED THEN
INSERT ([Children], [Name])
VALUES (i.[Children], i.[Name])
OUTPUT INSERTED.[Id], i._Position;
""",
            //
            """
SELECT [n].[Id], [n].[Name], [n].[Children]
FROM [NullableJsonParent] AS [n]
""",
            //
            """
@p0=NULL (Nullable = false)
@p1='1'
@p2='[]' (Nullable = false) (Size = 2)
@p3='2'
@p4='[]' (Nullable = false) (Size = 2)
@p6='3'
@p5='OnlyNameChanged' (Size = 4000)
@p7='[{"Name":"C1"},{"Name":"C2"}]' (Nullable = false) (Size = 29)
@p8='4'

SET NOCOUNT ON;
UPDATE [NullableJsonParent] SET [Children] = @p0
OUTPUT 1
WHERE [Id] = @p1;
UPDATE [NullableJsonParent] SET [Children] = @p2
OUTPUT 1
WHERE [Id] = @p3;
UPDATE [NullableJsonParent] SET [Children] = @p4, [Name] = @p5
OUTPUT 1
WHERE [Id] = @p6;
UPDATE [NullableJsonParent] SET [Children] = @p7
OUTPUT 1
WHERE [Id] = @p8;
""",
            //
            """
SELECT [n].[Id], [n].[Name], [n].[Children]
FROM [NullableJsonParent] AS [n]
""");
    }

    public override async Task Nullable_json_collection_loaded_without_tracking_keeps_null_and_empty()
    {
        await base.Nullable_json_collection_loaded_without_tracking_keeps_null_and_empty();

        AssertSql(
            """
@p0='Null' (Size = 4000)
@p1='[]' (Nullable = false) (Size = 2)
@p2='Empty' (Size = 4000)

SET NOCOUNT ON;
INSERT INTO [NullableJsonParentWithInitializer] ([Name])
OUTPUT INSERTED.[Id]
VALUES (@p0);
INSERT INTO [NullableJsonParentWithInitializer] ([Children], [Name])
OUTPUT INSERTED.[Id]
VALUES (@p1, @p2);
""",
            //
            """
SELECT [n].[Id], [n].[Name], [n].[Children]
FROM [NullableJsonParentWithInitializer] AS [n]
ORDER BY [n].[Name]
""",
            //
            """
SELECT [n].[Id], [n].[Name], [n].[Children]
FROM [NullableJsonParentWithInitializer] AS [n]
ORDER BY [n].[Name]
""");
    }

    public override async Task Nullable_json_collection_projected_with_its_entity_keeps_null_and_empty()
    {
        await base.Nullable_json_collection_projected_with_its_entity_keeps_null_and_empty();

        AssertSql(
            """
@p0='Null' (Size = 4000)
@p1='[]' (Nullable = false) (Size = 2)
@p2='Empty' (Size = 4000)

SET NOCOUNT ON;
INSERT INTO [NullableJsonParentWithInitializer] ([Name])
OUTPUT INSERTED.[Id]
VALUES (@p0);
INSERT INTO [NullableJsonParentWithInitializer] ([Children], [Name])
OUTPUT INSERTED.[Id]
VALUES (@p1, @p2);
""",
            //
            """
SELECT [n].[Id], [n].[Name], [n].[Children]
FROM [NullableJsonParentWithInitializer] AS [n]
ORDER BY [n].[Name]
""");
    }

    public override async Task Opting_out_of_a_nullable_json_collection_keeps_the_initializer_on_load()
    {
        await base.Opting_out_of_a_nullable_json_collection_keeps_the_initializer_on_load();

        AssertSql(
            """
@p0='Null' (Size = 4000)

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
INSERT INTO [NullableJsonParentWithInitializer] ([Name])
OUTPUT INSERTED.[Id]
VALUES (@p0);
""",
            //
            """
SELECT TOP(2) [n].[Id], [n].[Name], [n].[Children]
FROM [NullableJsonParentWithInitializer] AS [n]
""");
    }

    public override async Task Querying_a_tracked_entity_does_not_overwrite_a_pending_change_of_a_nullable_json_collection_to_empty()
    {
        await base.Querying_a_tracked_entity_does_not_overwrite_a_pending_change_of_a_nullable_json_collection_to_empty();

        AssertSql(
            """
@p0='A' (Size = 4000)

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
INSERT INTO [NullableJsonParent] ([Name])
OUTPUT INSERTED.[Id]
VALUES (@p0);
""",
            //
            """
SELECT TOP(2) [n].[Id], [n].[Name], [n].[Children]
FROM [NullableJsonParent] AS [n]
""",
            //
            """
SELECT TOP(2) [n].[Id], [n].[Name], [n].[Children]
FROM [NullableJsonParent] AS [n]
""",
            //
            """
@p0='[]' (Nullable = false) (Size = 2)
@p1='1'

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [NullableJsonParent] SET [Children] = @p0
OUTPUT 1
WHERE [Id] = @p1;
""",
            //
            """
SELECT TOP(2) [n].[Id], [n].[Name], [n].[Children]
FROM [NullableJsonParent] AS [n]
""");
    }

    protected override ITestStoreFactory NonSharedTestStoreFactory
        => SqlServerTestStoreFactory.Instance;
}
