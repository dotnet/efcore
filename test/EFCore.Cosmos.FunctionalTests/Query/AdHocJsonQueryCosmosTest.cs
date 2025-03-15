// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Cosmos.Storage.Internal;

namespace Microsoft.EntityFrameworkCore.Query;

public class AdHocJsonQueryCosmosTest : AdHocJsonQueryTestBase
{
    #region 21006

    public override async Task Project_root_with_missing_scalars(bool async)
    {
        if (async)
        {
            await base.Project_root_with_missing_scalars(async);

            AssertSql(
                """
SELECT VALUE c
FROM root c
WHERE (c["Id"] < 4)
""");
        }
    }

    [ConditionalTheory(Skip = "issue #35702")]
    public override async Task Project_top_level_json_entity_with_missing_scalars(bool async)
    {
        if (async)
        {
            await base.Project_top_level_json_entity_with_missing_scalars(async);

            AssertSql();
        }
    }

    public override async Task Project_nested_json_entity_with_missing_scalars(bool async)
    {
        if (async)
        {
            await AssertTranslationFailed(
                () => base.Project_nested_json_entity_with_missing_scalars(async));

            AssertSql();
        }
    }

    [ConditionalTheory(Skip = "issue #34067")]
    public override async Task Project_top_level_entity_with_null_value_required_scalars(bool async)
    {
        if (async)
        {
            await base.Project_top_level_entity_with_null_value_required_scalars(async);

            AssertSql(
                """
SELECT c["Id"], c
FROM root c
WHERE (c["Id"] = 4)
""");
        }
    }

    public override async Task Project_root_entity_with_missing_required_navigation(bool async)
    {
        if (async)
        {
            await base.Project_root_entity_with_missing_required_navigation(async);

            AssertSql(
                """
ReadItem(?, ?)
""");
        }
    }

    public override async Task Project_missing_required_navigation(bool async)
    {
        if (async)
        {
            await base.Project_missing_required_navigation(async);

            AssertSql(
                """
SELECT VALUE c
FROM root c
WHERE (c["Id"] = 5)
""");
        }
    }

    public override async Task Project_root_entity_with_null_required_navigation(bool async)
    {
        if (async)
        {
            await base.Project_root_entity_with_null_required_navigation(async);

            AssertSql(
                """
ReadItem(?, ?)
""");
        }
    }

    public override async Task Project_null_required_navigation(bool async)
    {
        if (async)
        {
            await base.Project_null_required_navigation(async);

            AssertSql(
                """
SELECT VALUE c
FROM root c
WHERE (c["Id"] = 6)
""");
        }
    }

    public override async Task Project_missing_required_scalar(bool async)
    {
        if (async)
        {
            await base.Project_missing_required_scalar(async);

            AssertSql(
                """
SELECT c["Id"], c["RequiredReference"]["Number"]
FROM root c
WHERE (c["Id"] = 2)
""");
        }
    }

    public override async Task Project_null_required_scalar(bool async)
    {
        if (async)
        {
            await base.Project_null_required_scalar(async);

            AssertSql(
                """
SELECT c["Id"], c["RequiredReference"]["Number"]
FROM root c
WHERE (c["Id"] = 4)
""");
        }
    }

    protected override void OnModelCreating21006(ModelBuilder modelBuilder)
    {
        base.OnModelCreating21006(modelBuilder);

        modelBuilder.Entity<Context21006.Entity>().ToContainer("Entities");
    }

    protected override async Task Seed21006(Context21006 context)
    {
        await base.Seed21006(context);

        var wrapper = (CosmosClientWrapper)context.GetService<ICosmosClientWrapper>();
        var singletonWrapper = context.GetService<ISingletonCosmosClientWrapper>();
        var entitiesContainer = singletonWrapper.Client.GetContainer(StoreName, containerId: "Entities");

        var missingTopLevel =
$$"""
{
    "Id": 2,
    "$type": "Entity",
    "Name": "e2",
    "id": "2",
    "Collection":
    [
        {
            "Text": "e2 c1",
            "NestedCollection": [
            {
                "DoB": "2000-01-01T00:00:00",
                "Text": "e2 c1 c1"
            },
            {
                "DoB": "2000-01-01T00:00:00",
                "Text": "e2 c1 c2"
            }
            ],
            "NestedOptionalReference": {
            "DoB": "2000-01-01T00:00:00",
            "Text": "e2 c1 nor"
            },
            "NestedRequiredReference": {
            "DoB": "2000-01-01T00:00:00",
            "Text": "e2 c1 nrr"
            }
        },
        {
            "Text": "e2 c2",
            "NestedCollection": [
            {
                "DoB": "2000-01-01T00:00:00",
                "Text": "e2 c2 c1"
            },
            {
                "DoB": "2000-01-01T00:00:00",
                "Text": "e2 c2 c2"
            }
            ],
            "NestedOptionalReference": {
            "DoB": "2000-01-01T00:00:00",
            "Text": "e2 c2 nor"
            },
            "NestedRequiredReference": {
            "DoB": "2000-01-01T00:00:00",
            "Text": "e2 c2 nrr"
            }
        }
    ],
    "OptionalReference": {
        "Text": "e2 or",
        "NestedCollection":
        [
            {
                "DoB": "2000-01-01T00:00:00",
                "Text": "e2 or c1"
            },
            {
                "DoB": "2000-01-01T00:00:00",
                "Text": "e2 or c2"
            }
        ],
        "NestedOptionalReference": {
            "DoB": "2000-01-01T00:00:00",
            "Text": "e2 or nor"
        },
        "NestedRequiredReference": {
            "DoB": "2000-01-01T00:00:00",
            "Text": "e2 or nrr"
        }
    },
    "RequiredReference": {
        "Text": "e2 rr",
        "NestedCollection":
        [
            {
                "DoB": "2000-01-01T00:00:00",
                "Text": "e2 rr c1"
            },
            {
                "DoB": "2000-01-01T00:00:00",
                "Text": "e2 rr c2"
            }
        ],
        "NestedOptionalReference": {
            "DoB": "2000-01-01T00:00:00",
            "Text": "e2 rr nor"
        },
        "NestedRequiredReference": {
            "DoB": "2000-01-01T00:00:00",
            "Text": "e2 rr nrr"
        }
    }
}
""";

        await AdHocCosmosTestHelpers.CreateCustomEntityHelperAsync(
            entitiesContainer,
            missingTopLevel,
            CancellationToken.None);

        var missingNested =
$$"""
{
    "Id": 3,
    "$type": "Entity",
    "Name": "e3",
    "id": "3",
    "Collection":
    [
        {
            "Number": 7.0,
            "Text": "e3 c1",
            "NestedCollection":
            [
                {
                  "Text": "e3 c1 c1"
                },
                {
                  "Text": "e3 c1 c2"
                }
            ],
            "NestedOptionalReference": {
            "Text": "e3 c1 nor"
            },
            "NestedRequiredReference": {
            "Text": "e3 c1 nrr"
            }
        },
        {
            "Number": 7.0,
            "Text": "e3 c2",
            "NestedCollection":
            [
                {
                  "Text": "e3 c2 c1"
                },
                {
                  "Text": "e3 c2 c2"
                }
            ],
            "NestedOptionalReference": {
                "Text": "e3 c2 nor"
            },
            "NestedRequiredReference": {
                "Text": "e3 c2 nrr"
            }
        }
    ],
    "OptionalReference": {
        "Number": 7.0,
        "Text": "e3 or",
        "NestedCollection":
        [
            {
                "Text": "e3 or c1"
            },
            {
                "Text": "e3 or c2"
            }
        ],
        "NestedOptionalReference": {
            "Text": "e3 or nor"
        },
        "NestedRequiredReference": {
            "Text": "e3 or nrr"
        }
    },
    "RequiredReference": {
        "Number": 7.0,
        "Text": "e3 rr",
        "NestedCollection":
        [
            {
                "Text": "e3 rr c1"
            },
            {
                "Text": "e3 rr c2"
            }
        ],
        "NestedOptionalReference": {
            "Text": "e3 rr nor"
        },
        "NestedRequiredReference": {
            "Text": "e3 rr nrr"
        }
    }
}
""";

        await AdHocCosmosTestHelpers.CreateCustomEntityHelperAsync(
            entitiesContainer,
            missingNested,
            CancellationToken.None);

        var nullTopLevel =
$$"""
{
    "Id": 4,
    "$type": "Entity",
    "Name": "e4",
    "id": "4",
    "Collection":
    [
        {
            "Number": null,
            "Text": "e4 c1",
            "NestedCollection":
            [
                {
                    "DoB": "2000-01-01T00:00:00",
                    "Text": "e4 c1 c1"
                },
                {
                    "DoB": "2000-01-01T00:00:00",
                    "Text": "e4 c1 c2"
                }
            ],
            "NestedOptionalReference": {
                "DoB": "2000-01-01T00:00:00",
                "Text": "e4 c1 nor"
            },
            "NestedRequiredReference": {
                "DoB": "2000-01-01T00:00:00",
                "Text": "e4 c1 nrr"
            }
        },
        {
            "Number": null,
            "Text": "e4 c2",
            "NestedCollection":
            [
                {
                    "DoB": "2000-01-01T00:00:00",
                    "Text": "e4 c2 c1"
                },
                {
                    "DoB": "2000-01-01T00:00:00",
                    "Text": "e4 c2 c2"
                }
            ],
            "NestedOptionalReference": {
                "DoB": "2000-01-01T00:00:00",
                "Text": "e4 c2 nor"
            },
            "NestedRequiredReference": {
                "DoB": "2000-01-01T00:00:00",
                "Text": "e4 c2 nrr"
            }
        }
    ],
    "OptionalReference": {
        "Number": null,
        "Text": "e4 or",
        "NestedCollection":
        [
            {
                "DoB": "2000-01-01T00:00:00",
                "Text": "e4 or c1"
            },
            {
                "DoB": "2000-01-01T00:00:00",
                "Text": "e4 or c2"
            }
        ],
        "NestedOptionalReference": {
            "DoB": "2000-01-01T00:00:00",
            "Text": "e4 or nor"
        },
        "NestedRequiredReference": {
            "DoB": "2000-01-01T00:00:00",
            "Text": "e4 or nrr"
        }
    },
    "RequiredReference": {
        "Number": null,
        "Text": "e4 rr",
        "NestedCollection":
        [
            {
                "DoB": "2000-01-01T00:00:00",
                "Text": "e4 rr c1"
            },
            {
                "DoB": "2000-01-01T00:00:00",
                "Text": "e4 rr c2"
            }
        ],
        "NestedOptionalReference": {
            "DoB": "2000-01-01T00:00:00",
            "Text": "e4 rr nor"
        },
        "NestedRequiredReference": {
            "DoB": "2000-01-01T00:00:00",
            "Text": "e4 rr nrr"
        }
    }
}
""";

        await AdHocCosmosTestHelpers.CreateCustomEntityHelperAsync(
            entitiesContainer,
            nullTopLevel,
            CancellationToken.None);

        var missingRequiredNav =
$$"""
{
    "Id": 5,
    "$type": "Entity",
    "Name": "e5",
    "id": "5",
    "Collection":
    [
        {
            "Number": 7.0,
            "Text": "e5 c1",
            "NestedCollection":
            [
                {
                    "DoB": "2000-01-01T00:00:00",
                    "Text": "e5 c1 c1"
                },
                {
                    "DoB": "2000-01-01T00:00:00",
                    "Text": "e5 c1 c2"
                }
            ],
            "NestedOptionalReference": {
                "DoB": "2000-01-01T00:00:00",
                "Text": "e5 c1 nor"
            },
        },
        {
            "Number": 7.0,
            "Text": "e5 c2",
            "NestedCollection":
            [
                {
                    "DoB": "2000-01-01T00:00:00",
                    "Text": "e5 c2 c1"
                },
                {
                    "DoB": "2000-01-01T00:00:00",
                    "Text": "e5 c2 c2"
                }
            ],
            "NestedOptionalReference": {
                "DoB": "2000-01-01T00:00:00",
                "Text": "e5 c2 nor"
            },
        }
    ],
    "OptionalReference": {
        "Number": 7.0,
        "Text": "e5 or",
        "NestedCollection":
        [
            {
                "DoB": "2000-01-01T00:00:00",
                "Text": "e5 or c1"
            },
            {
                "DoB": "2000-01-01T00:00:00",
                "Text": "e5 or c2"
            }
        ],
        "NestedOptionalReference": {
            "DoB": "2000-01-01T00:00:00",
            "Text": "e5 or nor"
        },
    },
    "RequiredReference": {
        "Number": 7.0,
        "Text": "e5 rr",
        "NestedCollection":
        [
            {
                "DoB": "2000-01-01T00:00:00",
                "Text": "e5 rr c1"
            },
            {
                "DoB": "2000-01-01T00:00:00",
                "Text": "e5 rr c2"
            }
        ],
        "NestedOptionalReference": {
            "DoB": "2000-01-01T00:00:00",
            "Text": "e5 rr nor"
        },
    }
}
""";

        await AdHocCosmosTestHelpers.CreateCustomEntityHelperAsync(
            entitiesContainer,
            missingRequiredNav,
            CancellationToken.None);

        var nullRequiredNav =
$$"""
{
    "Id": 6,
    "$type": "Entity",
    "Name": "e6",
    "id": "6",
    "Collection":
    [
        {
            "Number": 7.0,
            "Text": "e6 c1",
            "NestedCollection":
            [
                {
                    "DoB": "2000-01-01T00:00:00",
                    "Text": "e6 c1 c1"
                },
                {
                    "DoB": "2000-01-01T00:00:00",
                    "Text": "e6 c1 c2"
                }
            ],
            "NestedOptionalReference": {
                "DoB": "2000-01-01T00:00:00",
                "Text": "e6 c1 nor"
            },
            "NestedRequiredReference": null
        },
        {
            "Number": 7.0,
            "Text": "e6 c2",
            "NestedCollection":
            [
                {
                    "DoB": "2000-01-01T00:00:00",
                    "Text": "e6 c2 c1"
                },
                {
                    "DoB": "2000-01-01T00:00:00",
                    "Text": "e6 c2 c2"
                }
            ],
            "NestedOptionalReference": {
                "DoB": "2000-01-01T00:00:00",
                "Text": "e6 c2 nor"
            },
            "NestedRequiredReference": null
        }
    ],
    "OptionalReference": {
        "Number": 7.0,
        "Text": "e6 or",
        "NestedCollection":
        [
            {
                "DoB": "2000-01-01T00:00:00",
                "Text": "e6 or c1"
            },
            {
                "DoB": "2000-01-01T00:00:00",
                "Text": "e6 or c2"
            }
        ],
        "NestedOptionalReference": {
            "DoB": "2000-01-01T00:00:00",
            "Text": "e6 or nor"
        },
        "NestedRequiredReference": null
    },
    "RequiredReference": {
        "Number": 7.0,
        "Text": "e6 rr",
        "NestedCollection":
        [
            {
                "DoB": "2000-01-01T00:00:00",
                "Text": "e6 rr c1"
            },
            {
                "DoB": "2000-01-01T00:00:00",
                "Text": "e6 rr c2"
            }
        ],
        "NestedOptionalReference": {
            "DoB": "2000-01-01T00:00:00",
            "Text": "e6 rr nor"
        },
        "NestedRequiredReference": null
    }
}
""";

        await AdHocCosmosTestHelpers.CreateCustomEntityHelperAsync(
            entitiesContainer,
            nullRequiredNav,
            CancellationToken.None);
    }

    #endregion

    #region 29219

    // Cosmos returns unexpected number of results (i.e. not returning row with non-existent NullableScalar
    // this is by design behavior in Cosmos, so we just skip the test to avoid validation error
    public override Task Can_project_nullable_json_property_when_the_element_in_json_is_not_present()
        => Task.CompletedTask;

    protected override void OnModelCreating29219(ModelBuilder modelBuilder)
    {
        base.OnModelCreating29219(modelBuilder);

        modelBuilder.Entity<Context29219.MyEntity>().ToContainer("Entities");
    }

    protected override async Task Seed29219(DbContext ctx)
    {
        await base.Seed29219(ctx);

        var wrapper = (CosmosClientWrapper)ctx.GetService<ICosmosClientWrapper>();
        var singletonWrapper = ctx.GetService<ISingletonCosmosClientWrapper>();
        var entitiesContainer = singletonWrapper.Client.GetContainer(StoreName, containerId: "Entities");

        var missingNullableScalars =
$$"""
{
    "Id": 3,
    "$type": "MyEntity",
    "id": "3",
    "Collection":
    [
        {
            "NonNullableScalar" : 10001
        }
    ],
    "Reference": {
        "NonNullableScalar" : 30
    }
}
""";

        await AdHocCosmosTestHelpers.CreateCustomEntityHelperAsync(
            entitiesContainer,
            missingNullableScalars,
            CancellationToken.None);
    }

    #endregion

    #region 30028

    [ConditionalTheory(Skip = "issue #35702")]
    public override Task Missing_navigation_works_with_deduplication(bool async)
        => base.Missing_navigation_works_with_deduplication(async);

    protected override void OnModelCreating30028(ModelBuilder modelBuilder)
    {
        base.OnModelCreating30028(modelBuilder);

        modelBuilder.Entity<Context30028.MyEntity>().ToContainer("Entities");
    }

    protected override async Task Seed30028(DbContext ctx)
    {
        var wrapper = (CosmosClientWrapper)ctx.GetService<ICosmosClientWrapper>();
        var singletonWrapper = ctx.GetService<ISingletonCosmosClientWrapper>();
        var entitiesContainer = singletonWrapper.Client.GetContainer(StoreName, containerId: "Entities");

        var complete =
$$$$"""
{
    "Id": 1,
    "$type": "MyEntity",
    "id": "1",
    "Json": {
        "RootName":"e1",
        "Collection":
        [
            {
                "BranchName":"e1 c1",
                "Nested":{
                    "LeafName":"e1 c1 l"
                }
            },
            {
                "BranchName":"e1 c2",
                "Nested":{
                    "LeafName":"e1 c2 l"
                }
            }
        ],
        "OptionalReference":{
            "BranchName":"e1 or",
            "Nested":{
                "LeafName":"e1 or l"
            }
        },
        "RequiredReference":{
            "BranchName":"e1 rr",
            "Nested":{
                "LeafName":"e1 rr l"
            }
        }
    }
}
""";

        await AdHocCosmosTestHelpers.CreateCustomEntityHelperAsync(
            entitiesContainer,
            complete,
            CancellationToken.None);

        var missingCollection =
$$$$"""
{
    "Id": 2,
    "$type": "MyEntity",
    "id": "2",
    "Json": {
        "RootName":"e2",
        "OptionalReference":{
            "BranchName":"e2 or",
            "Nested":{
                "LeafName":"e2 or l"
            }
        },
        "RequiredReference":{
            "BranchName":"e2 rr",
            "Nested":{
                "LeafName":"e2 rr l"
            }
        }
    }
}
""";

        await AdHocCosmosTestHelpers.CreateCustomEntityHelperAsync(
            entitiesContainer,
            missingCollection,
            CancellationToken.None);

        var missingOptionalReference =
$$$$"""
{
    "Id": 3,
    "$type": "MyEntity",
    "id": "3",
    "Json": {
        "RootName":"e3",
        "Collection":
        [
            {
                "BranchName":"e3 c1",
                "Nested":{
                    "LeafName":"e3 c1 l"
                }
            },
            {
                "BranchName":"e3 c2",
                "Nested":{
                    "LeafName":"e3 c2 l"
                }
            }
        ],
        "RequiredReference":{
            "BranchName":"e3 rr",
            "Nested":{
                "LeafName":"e3 rr l"
            }
        }
    }
}
""";

        await AdHocCosmosTestHelpers.CreateCustomEntityHelperAsync(
            entitiesContainer,
            missingOptionalReference,
            CancellationToken.None);

        var missingRequiredReference =
$$$$"""
{
    "Id": 4,
    "$type": "MyEntity",
    "id": "4",
    "Json": {
        "RootName":"e4",
        "Collection":
        [
            {
                "BranchName":"e4 c1",
                "Nested":{
                    "LeafName":"e4 c1 l"
                }
            },
            {
                "BranchName":"e4 c2",
                "Nested":{
                    "LeafName":"e4 c2 l"
                }
            }
        ],
        "OptionalReference":{
            "BranchName":"e4 or",
            "Nested":{
                "LeafName":"e4 or l"
            }
        }
    } 
}
""";

        await AdHocCosmosTestHelpers.CreateCustomEntityHelperAsync(
            entitiesContainer,
            missingRequiredReference,
            CancellationToken.None);
    }

    #endregion

    protected override Task Seed33046(DbContext ctx) => throw new NotImplementedException();









    protected TestSqlLoggerFactory TestSqlLoggerFactory
        => (TestSqlLoggerFactory)ListLoggerFactory;

    private void AssertSql(params string[] expected)
        => TestSqlLoggerFactory.AssertBaseline(expected);

    protected static async Task AssertTranslationFailed(Func<Task> query)
        => Assert.Contains(
            CoreStrings.TranslationFailed("")[48..],
            (await Assert.ThrowsAsync<InvalidOperationException>(query))
            .Message);

    protected override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
        => builder.ConfigureWarnings(b => b.Ignore(CosmosEventId.NoPartitionKeyDefined));

    protected override ITestStoreFactory TestStoreFactory
        => CosmosTestStoreFactory.Instance;
}
