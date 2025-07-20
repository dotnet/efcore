// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Relationships.ComplexJson;

public class ComplexJsonSqlServerFixture : ComplexJsonRelationalFixtureBase
{
    protected override ITestStoreFactory TestStoreFactory
        => SqlServerTestStoreFactory.Instance;

    protected override async Task SeedAsync(PoolableDbContext context)
    {
        // TODO: Temporary, until we have update pipeline support for complex JSON
        await context.Database.ExecuteSqlAsync($$$"""
INSERT INTO RootEntity (Id, Name, RequiredRelated, OptionalRelated, RelatedCollection) VALUES
(
    1,
    'Root1',
    -- RequiredRelated:
    '{
        "Id": 100,
        "Name": "Root1_RequiredRelated",
        "Int": 8,
        "String": "foo",
        "RequiredNested": { "Id": 1000, "Name": "Root1_RequiredRelated_RequiredNested", "Int": 50, "String": "foo_foo" },
        "OptionalNested": { "Id": 1001, "Name": "Root1_RequiredRelated_OptionalNested", "Int": 51, "String": "foo_bar" },
        "NestedCollection":
        [
            { "Id": 1002, "Name": "Root1_RequiredRelated_NestedCollection_1", "Int": 52, "String": "foo_baz1" },
            { "Id": 1003, "Name": "Root1_RequiredRelated_NestedCollection_2", "Int": 53, "String": "foo_baz2" }
        ]
    }',
    -- OptionalRelated:
    '{
        "Id": 101,
        "Name": "Root1_OptionalRelated",
        "Int": 9,
        "String": "bar",
        "RequiredNested": { "Id": 1010, "Name": "Root1_OptionalRelated_RequiredNested", "Int": 52, "String": "bar_foo" },
        "OptionalNested": { "Id": 1011, "Name": "Root1_OptionalRelated_OptionalNested", "Int": 53, "String": "bar_bar" },
        "NestedCollection":
        [
            { "Id": 1012, "Name": "Root1_OptionalRelated_NestedCollection_1", "Int": 54, "String": "bar_baz1" },
            { "Id": 1013, "Name": "Root1_OptionalRelated_NestedCollection_2", "Int": 55, "String": "bar_baz2" }
        ]
    }',
    -- RelatedCollection:
    '[
        {
            "Id": 102,
            "Name": "Root1_RelatedCollection_1",
            "Int": 21,
            "String": "foo",
            "RequiredNested": { "Id": 1020, "Name": "Root1_RelatedCollection_1_RequiredNested", "Int": 50, "String": "foo_foo" },
            "OptionalNested": { "Id": 1021, "Name": "Root1_RelatedCollection_1_OptionalNested", "Int": 51, "String": "foo_bar" },
            "NestedCollection":
            [
                { "Id": 1022, "Name": "Root1_RelatedCollection_1_NestedCollection_1", "Int": 53, "String": "foo_bar" },
                { "Id": 1023, "Name": "Root1_RelatedCollection_1_NestedCollection_2", "Int": 51, "String": "foo_bar" }
            ]
        },
        {
            "Id": 103,
            "Name": "Root1_RelatedCollection_2",
            "Int": 22,
            "String": "foo",
            "RequiredNested": { "Id": 1030, "Name": "Root1_RelatedCollection_2_RequiredNested", "Int": 50, "String": "foo_foo" },
            "OptionalNested": { "Id": 1031, "Name": "Root1_RelatedCollection_2_OptionalNested", "Int": 51, "String": "foo_bar" },
            "NestedCollection":
            [
                { "Id": 1032, "Name": "Root1_RelatedCollection_2_NestedCollection_1", "Int": 53, "String": "foo_bar" },
                { "Id": 1033, "Name": "Root1_RelatedCollection_2_NestedCollection_2", "Int": 51, "String": "foo_bar" }
            ]
        }
    ]'
),
(
    2,
    'Root2',
    -- RequiredRelated:
    '{
        "Id": 200,
        "Name": "Root2_RequiredRelated",
        "Int": 10,
        "String": "aaa",
        "RequiredNested": { "Id": 2000, "Name": "Root2_RequiredRelated_RequiredNested", "Int": 54, "String": "aaa_xxx" },
        "OptionalNested": { "Id": 2001, "Name": "Root2_RequiredRelated_OptionalNested", "Int": 55, "String": "aaa_yyy" },
        "NestedCollection": []
    }',
    -- OptionalRelated:
    '{
        "Id": 201,
        "Name": "Root2_OptionalRelated",
        "Int": 11,
        "String": "bbb",
        "RequiredNested": { "Id": 2010, "Name": "Root2_OptionalRelated_RequiredNested", "Int": 56, "String": "bbb_xxx" },
        "OptionalNested": { "Id": 2011, "Name": "Root2_OptionalRelated_OptionalNested", "Int": 57, "String": "bbb_yyy" },
        "NestedCollection": []
    }',
    -- RelatedCollection:
    '[]'
)
""");
    }
}
