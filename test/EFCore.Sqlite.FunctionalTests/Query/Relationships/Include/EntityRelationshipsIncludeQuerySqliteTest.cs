// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Relationships.Include;

public class EntityRelationshipsIncludeQuerySqliteTest
    : EntityRelationshipsIncludeQueryRelationalTestBase<EntityRelationshipsQuerySqliteFixture>
{
    public EntityRelationshipsIncludeQuerySqliteTest(EntityRelationshipsQuerySqliteFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }
}
