// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Update;

#nullable disable

public class NonSharedModelUpdatesSqliteTest : NonSharedModelUpdatesTestBase
{
    public override async Task Principal_and_dependent_roundtrips_with_cycle_breaking(bool async)
    {
        await base.Principal_and_dependent_roundtrips_with_cycle_breaking(async);

        AssertSql(
            """
@p0='AC South' (Size = 8)

INSERT INTO "AuthorsClub" ("Name")
VALUES (@p0)
RETURNING "Id";
""",
            //
            """
@p1='1'
@p2='Alice' (Size = 5)

INSERT INTO "Author" ("AuthorsClubId", "Name")
VALUES (@p1, @p2)
RETURNING "Id";
""",
            //
            """
@p3='1'
@p4=NULL

INSERT INTO "Book" ("AuthorId", "Title")
VALUES (@p3, @p4)
RETURNING "Id";
""",
            //
            """
SELECT "b"."Id", "b"."AuthorId", "b"."Title", "a"."Id", "a"."AuthorsClubId", "a"."Name"
FROM "Book" AS "b"
INNER JOIN "Author" AS "a" ON "b"."AuthorId" = "a"."Id"
LIMIT 2
""",
            //
            """
@p0='AC North' (Size = 8)

INSERT INTO "AuthorsClub" ("Name")
VALUES (@p0)
RETURNING "Id";
""",
            //
            """
@p1='2'
@p2='Author of the year 2023' (Size = 23)

INSERT INTO "Author" ("AuthorsClubId", "Name")
VALUES (@p1, @p2)
RETURNING "Id";
""",
            //
            """
@p4='1'
@p3='2'

UPDATE "Book" SET "AuthorId" = @p3
WHERE "Id" = @p4
RETURNING 1;
""",
            //
            """
@p0='1'

DELETE FROM "Author"
WHERE "Id" = @p0
RETURNING 1;
""");
    }

    public override async Task DbUpdateException_Entries_is_correct_with_multiple_inserts(bool async)
    {
        await base.DbUpdateException_Entries_is_correct_with_multiple_inserts(async);

        AssertSql(
            """
@p0='Blog2' (Size = 5)

INSERT INTO "Blog" ("Name")
VALUES (@p0)
RETURNING "Id";
""",
            //
            """
@p0='Blog1' (Size = 5)

INSERT INTO "Blog" ("Name")
VALUES (@p0)
RETURNING "Id";
""",
            //
            """
@p0='Blog2' (Size = 5)

INSERT INTO "Blog" ("Name")
VALUES (@p0)
RETURNING "Id";
""");
    }

    private void AssertSql(params string[] expected)
        => TestSqlLoggerFactory.AssertBaseline(expected);

    protected override ITestStoreFactory TestStoreFactory
        => SqliteTestStoreFactory.Instance;
}
