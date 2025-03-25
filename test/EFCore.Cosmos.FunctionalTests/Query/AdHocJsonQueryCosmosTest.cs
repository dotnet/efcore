// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Cosmos.Storage.Internal;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.EntityFrameworkCore.Query;

public class AdHocJsonQueryCosmosTest(NonSharedFixture fixture) : AdHocJsonQueryTestBase(fixture)
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
        var entitiesContainer = singletonWrapper.Client.GetContainer(context.Database.GetCosmosDatabaseId(), containerId: "Entities");

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

    protected override async Task Seed29219(DbContext context)
    {
        await base.Seed29219(context);

        var wrapper = (CosmosClientWrapper)context.GetService<ICosmosClientWrapper>();
        var singletonWrapper = context.GetService<ISingletonCosmosClientWrapper>();
        var entitiesContainer = singletonWrapper.Client.GetContainer(context.Database.GetCosmosDatabaseId(), containerId: "Entities");

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

    // missing array comes out as empty on Cosmos
    public override Task Accessing_missing_navigation_works()
        => Task.CompletedTask;

    protected override void OnModelCreating30028(ModelBuilder modelBuilder)
    {
        base.OnModelCreating30028(modelBuilder);

        modelBuilder.Entity<Context30028.MyEntity>().ToContainer("Entities");
    }

    protected override async Task Seed30028(DbContext context)
    {
        var wrapper = (CosmosClientWrapper)context.GetService<ICosmosClientWrapper>();
        var singletonWrapper = context.GetService<ISingletonCosmosClientWrapper>();
        var entitiesContainer = singletonWrapper.Client.GetContainer(context.Database.GetCosmosDatabaseId(), containerId: "Entities");

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

    #region 33046

    protected override void OnModelCreating33046(ModelBuilder modelBuilder)
    {
        base.OnModelCreating33046(modelBuilder);

        modelBuilder.Entity<Context33046.Review>().ToContainer("Reviews");
    }

    protected override async Task Seed33046(DbContext context)
    {
        var wrapper = (CosmosClientWrapper)context.GetService<ICosmosClientWrapper>();
        var singletonWrapper = context.GetService<ISingletonCosmosClientWrapper>();
        var entitiesContainer = singletonWrapper.Client.GetContainer(context.Database.GetCosmosDatabaseId(), containerId: "Reviews");

        var json =
$$$$"""
{
    "Id": 1,
    "$type": "Review",
    "id": "1",
    "Rounds":
    [
        {
            "RoundNumber":11,
            "SubRounds":
            [
                {
                    "SubRoundNumber":111
                },
                {
                    "SubRoundNumber":112
                }
            ]
        }
    ]
}
""";

        await AdHocCosmosTestHelpers.CreateCustomEntityHelperAsync(
            entitiesContainer,
            json,
            CancellationToken.None);
    }

    #endregion

    #region 34960


    public override async Task Try_project_collection_but_JSON_is_entity()
    {
        var message = (await Assert.ThrowsAsync<JsonSerializationException>(
            () => base.Try_project_collection_but_JSON_is_entity())).Message;

        Assert.Equal(
            $"Deserialized JSON type '{typeof(JObject).FullName}' is not compatible with expected type '{typeof(JArray).FullName}'. Path 'Collection'.",
            message);
    }

    public override async Task Try_project_reference_but_JSON_is_collection()
    {
        var message = (await Assert.ThrowsAsync<JsonSerializationException>(
            () => base.Try_project_reference_but_JSON_is_collection())).Message;

        Assert.Equal(
            $"Deserialized JSON type '{typeof(JArray).FullName}' is not compatible with expected type '{typeof(JObject).FullName}'. Path 'Reference'.",
            message);
    }

    protected override void OnModelCreating34960(ModelBuilder modelBuilder)
    {
        base.OnModelCreating34960(modelBuilder);
        modelBuilder.Entity<Context34960.Entity>().ToContainer("Entities");
        modelBuilder.Entity<Context34960.JunkEntity>().ToContainer("Junk");
    }

    protected async override Task Seed34960(Context34960 context)
    {
        await base.Seed34960(context);

        var wrapper = (CosmosClientWrapper)context.GetService<ICosmosClientWrapper>();
        var singletonWrapper = context.GetService<ISingletonCosmosClientWrapper>();
        var entitiesContainer = singletonWrapper.Client.GetContainer(context.Database.GetCosmosDatabaseId(), containerId: "Entities");

        var json =
$$$"""
{
    "Id": 4,
    "$type": "Entity",
    "id": "4",
    "Collection": null,
    "Reference": null
}
""";

        await AdHocCosmosTestHelpers.CreateCustomEntityHelperAsync(
            entitiesContainer,
            json,
            CancellationToken.None);

        var junkContainer = singletonWrapper.Client.GetContainer(context.Database.GetCosmosDatabaseId(), containerId: "Junk");

        var objectWhereCollectionShouldBe =
$$$"""
{
    "Id": 1,
    "$type": "JunkEntity",
    "id": "1",
    "Collection": { "DoB":"2000-01-01T00:00:00","Text":"junk" },
    "Reference": null
}
""";

        await AdHocCosmosTestHelpers.CreateCustomEntityHelperAsync(
            junkContainer,
            objectWhereCollectionShouldBe,
            CancellationToken.None);

        var collectionWhereEntityShouldBe =
$$$"""
{
    "Id": 2,
    "$type": "JunkEntity",
    "id": "2",
    "Collection": null,
    "Reference": [{ "DoB":"2000-01-01T00:00:00","Text":"junk" }]
}
""";

        await AdHocCosmosTestHelpers.CreateCustomEntityHelperAsync(
            junkContainer,
            collectionWhereEntityShouldBe,
            CancellationToken.None);
    }

    #endregion

    #region ArrayOfPrimitives

    protected override void OnModelCreatingArrayOfPrimitives(ModelBuilder modelBuilder)
    {
        base.OnModelCreatingArrayOfPrimitives(modelBuilder);
    }

    #endregion

    #region JunkInJson

    protected override void OnModelCreatingJunkInJson(ModelBuilder modelBuilder)
    {
        base.OnModelCreatingJunkInJson(modelBuilder);

        modelBuilder.Entity<ContextJunkInJson.MyEntity>().ToContainer("Entities");
    }

    protected override async Task SeedJunkInJson(DbContext context)
    {
        var wrapper = (CosmosClientWrapper)context.GetService<ICosmosClientWrapper>();
        var singletonWrapper = context.GetService<ISingletonCosmosClientWrapper>();
        var entitiesContainer = singletonWrapper.Client.GetContainer(context.Database.GetCosmosDatabaseId(), containerId: "Entities");

        var json =
$$$"""
{
    "Id": 1,
    "$type": "MyEntity",
    "id": "1",
    "Collection":
    [
        {
            "JunkReference": {
                "Something":"SomeValue"
            },
            "Name":"c11",
            "JunkProperty1":50,
            "Number":11.5,
            "JunkCollection1":[],
            "JunkCollection2":
            [
                {
                    "Foo":"junk value"
                }
            ],
            "NestedCollection":
            [
                {
                    "DoB":"2002-04-01T00:00:00",
                    "DummyProp":"Dummy value"
                },
                {
                    "DoB":"2002-04-02T00:00:00",
                    "DummyReference":{
                        "Foo":5
                    }
                }
            ],
            "NestedReference":{
                "DoB":"2002-03-01T00:00:00"
            }
        },
        {
            "Name":"c12",
            "Number":12.5,
            "NestedCollection":
            [
                {
                    "DoB":"2002-06-01T00:00:00"
                },
                {
                    "DoB":"2002-06-02T00:00:00"
                }
            ],
            "NestedDummy":59,
            "NestedReference":{
                "DoB":"2002-05-01T00:00:00"
            }
        }
    ],
    "CollectionWithCtor":
    [
        {
            "MyBool":true,
            "Name":"c11 ctor",
            "JunkReference":{
                "Something":"SomeValue",
                "JunkCollection":
                [
                    {
                        "Foo":"junk value"
                    }
                ]
            },
            "NestedCollection":
            [
                {
                    "DoB":"2002-08-01T00:00:00"
                },
                {
                    "DoB":"2002-08-02T00:00:00"
                }
            ],
            "NestedReference":{
                "DoB":"2002-07-01T00:00:00"
            }
        },
        {
            "MyBool":false,
            "Name":"c12 ctor",
            "NestedCollection":
            [
                {
                    "DoB":"2002-10-01T00:00:00"
                },
                {
                    "DoB":"2002-10-02T00:00:00"
                }
            ],
            "JunkCollection":
            [
                {
                    "Foo":"junk value"
                }
            ],
            "NestedReference":{
                "DoB":"2002-09-01T00:00:00"
            }
        }
    ],
    "Reference": {
        "Name":"r1",
        "JunkCollection":
        [
            {
                "Foo":"junk value"
            }
        ],
        "JunkReference":{
            "Something":"SomeValue"
        },
        "Number":1.5,
        "NestedCollection":
        [
            {
                "DoB":"2000-02-01T00:00:00",
                "JunkReference":{
                    "Something":"SomeValue"
                }
            },
            {
                "DoB":"2000-02-02T00:00:00"
            }
        ],
        "NestedReference":{
            "DoB":"2000-01-01T00:00:00"
        }
    },
    "ReferenceWithCtor":{
        "MyBool":true,
        "JunkCollection":
        [
            {
                "Foo":"junk value"
            }
        ],
        "Name":"r1 ctor",
        "JunkReference":{
            "Something":"SomeValue"
        },
        "NestedCollection":
        [
            {
                "DoB":"2001-02-01T00:00:00"
            },
            {
                "DoB":"2001-02-02T00:00:00"
            }
        ],
        "NestedReference":{
            "JunkCollection":
            [
                {
                    "Foo":"junk value"
                }
            ],
            "DoB":"2001-01-01T00:00:00"
        }
    }
}
""";

        await AdHocCosmosTestHelpers.CreateCustomEntityHelperAsync(
            entitiesContainer,
            json,
            CancellationToken.None);
    }

    #endregion

    #region TrickyBuffering

    protected override void OnModelCreatingTrickyBuffering(ModelBuilder modelBuilder)
    {
        base.OnModelCreatingTrickyBuffering(modelBuilder);

        modelBuilder.Entity<ContextTrickyBuffering.MyEntity>().ToContainer("Entities");
    }

    protected override async Task SeedTrickyBuffering(DbContext context)
    {
        var wrapper = (CosmosClientWrapper)context.GetService<ICosmosClientWrapper>();
        var singletonWrapper = context.GetService<ISingletonCosmosClientWrapper>();
        var entitiesContainer = singletonWrapper.Client.GetContainer(context.Database.GetCosmosDatabaseId(), containerId: "Entities");

        var json =
$$$"""
{
    "Id": 1,
    "$type": "MyEntity",
    "id": "1",
    "Reference": {
        "Name": "r1",
        "Number": 7,
        "JunkReference": {
            "Something": "SomeValue"
        },
        "JunkCollection":
        [
            {
                "Foo": "junk value"
            }
        ],
        "NestedReference": {
            "DoB": "2000-01-01T00:00:00"
        },
        "NestedCollection":
        [
            {
                "DoB": "2000-02-01T00:00:00",
                "JunkReference": {
                    "Something": "SomeValue"
                }
            },
            {
                "DoB": "2000-02-02T00:00:00"
            }
        ]
    }
}
""";

        await AdHocCosmosTestHelpers.CreateCustomEntityHelperAsync(
            entitiesContainer,
            json,
            CancellationToken.None);
    }

    #endregion

    #region ShadowProperties

    protected override void OnModelCreatingShadowProperties(ModelBuilder modelBuilder)
    {
        base.OnModelCreatingShadowProperties(modelBuilder);

        modelBuilder.Entity<ContextShadowProperties.MyEntity>(b =>
            {
                b.ToContainer("Entities");

                //b.OwnsOne(x => x.Reference, b =>
                //{
                //    //      b.ToJson().HasColumnType(JsonColumnType);
                //    b.Property<string>("ShadowString");
                //});

                b.OwnsOne(x => x.ReferenceWithCtor, b =>
                {
                    //    b.ToJson().HasColumnType(JsonColumnType);
                    b.Property<int>("Shadow_Int").ToJsonProperty("ShadowInt");
                });

                //b.OwnsMany(
                //    x => x.Collection, b =>
                //    {
                //        //  b.ToJson().HasColumnType(JsonColumnType);
                //        b.Property<double>("ShadowDouble");
                //    });

                //b.OwnsMany(
                //    x => x.CollectionWithCtor, b =>
                //    {
                //        //b.ToJson().HasColumnType(JsonColumnType);
                //        b.Property<byte?>("ShadowNullableByte");
                //    });
            });
    }

    protected override async Task SeedShadowProperties(DbContext context)
    {
        var wrapper = (CosmosClientWrapper)context.GetService<ICosmosClientWrapper>();
        var singletonWrapper = context.GetService<ISingletonCosmosClientWrapper>();
        var entitiesContainer = singletonWrapper.Client.GetContainer(context.Database.GetCosmosDatabaseId(), containerId: "Entities");

        var json =
$$$"""
{
    "Id": 1,
    "$type": "MyEntity",
    "id": "1",
    "Name": "e1",
    "Collection":
    [
        {
            "Name":"e1_c1","ShadowDouble":5.5
        },
        {
            "ShadowDouble":20.5,"Name":"e1_c2"
        }
    ],
    "CollectionWithCtor":
    [
        {
            "Name":"e1_c1 ctor","ShadowNullableByte":6
        },
        {
            "ShadowNullableByte":null,"Name":"e1_c2 ctor"
        }
    ],
    "Reference": { "Name":"e1_r", "ShadowString":"Foo" },
    "ReferenceWithCtor": { "ShadowInt":143,"Name":"e1_r ctor" }
}
""";

        await AdHocCosmosTestHelpers.CreateCustomEntityHelperAsync(
            entitiesContainer,
            json,
            CancellationToken.None);
    }

    #endregion

    #region LazyLoadingProxies

    protected override void OnModelCreatingLazyLoadingProxies(ModelBuilder modelBuilder)
    {
        base.OnModelCreatingLazyLoadingProxies(modelBuilder);
    }

    #endregion

    #region NotICollection

    protected override void OnModelCreatingNotICollection(ModelBuilder modelBuilder)
    {
        base.OnModelCreatingNotICollection(modelBuilder);

        modelBuilder.Entity<ContextNotICollection.MyEntity>().ToContainer("Entities");
    }

    protected override async Task SeedNotICollection(DbContext context)
    {
        var wrapper = (CosmosClientWrapper)context.GetService<ICosmosClientWrapper>();
        var singletonWrapper = context.GetService<ISingletonCosmosClientWrapper>();
        var entitiesContainer = singletonWrapper.Client.GetContainer(context.Database.GetCosmosDatabaseId(), containerId: "Entities");

        var json1 =
$$$"""
{
    "Id": 1,
    "$type": "MyEntity",
    "id": "1",
    "Json":
    {
        "Collection":
        [
            {
                "Bar":11,"Foo":"c11"
            },
            {
                "Bar":12,"Foo":"c12"
            },
            {
                "Bar":13,"Foo":"c13"
            }
        ]
    }
}
""";

        await AdHocCosmosTestHelpers.CreateCustomEntityHelperAsync(
            entitiesContainer,
            json1,
            CancellationToken.None);

        var json2 =
$$$"""
{
    "Id": 2,
    "$type": "MyEntity",
    "id": "2",
    "Json": {
        "Collection":
        [
            {
                "Bar":21,"Foo":"c21"
            },
            {
                "Bar":22,"Foo":"c22"
            }
        ]
    }
}
""";

        await AdHocCosmosTestHelpers.CreateCustomEntityHelperAsync(
            entitiesContainer,
            json2,
            CancellationToken.None);
    }

    #endregion

    #region BadJsonProperties

    // missing collection comes back as empty on Cosmos
    public override Task Bad_json_properties_empty_navigations(bool noTracking)
        => Task.CompletedTask;

    protected override void OnModelCreatingBadJsonProperties(ModelBuilder modelBuilder)
    {
        base.OnModelCreatingBadJsonProperties(modelBuilder);

        modelBuilder.Entity<ContextBadJsonProperties.Entity>().ToContainer("Entities");
    }

    protected override async Task SeedBadJsonProperties(ContextBadJsonProperties context)
    {
        var wrapper = (CosmosClientWrapper)context.GetService<ICosmosClientWrapper>();
        var singletonWrapper = context.GetService<ISingletonCosmosClientWrapper>();
        var entitiesContainer = singletonWrapper.Client.GetContainer(context.Database.GetCosmosDatabaseId(), containerId: "Entities");

        var baseline =
$$$"""
{
    "Id": 1,
    "$type": "Entity",
    "id": "1",
    "Scenario": "baseline",
    "OptionalReference": {"NestedOptional": { "Text":"or no" }, "NestedRequired": { "Text":"or nr" }, "NestedCollection": [ { "Text":"or nc 1" }, { "Text":"or nc 2" } ] },
    "RequiredReference": {"NestedOptional": { "Text":"rr no" }, "NestedRequired": { "Text":"rr nr" }, "NestedCollection": [ { "Text":"rr nc 1" }, { "Text":"rr nc 2" } ] },
    "Collection":
    [
        {"NestedOptional": { "Text":"c 1 no" }, "NestedRequired": { "Text":"c 1 nr" }, "NestedCollection": [ { "Text":"c 1 nc 1" }, { "Text":"c 1 nc 2" } ] },
        {"NestedOptional": { "Text":"c 2 no" }, "NestedRequired": { "Text":"c 2 nr" }, "NestedCollection": [ { "Text":"c 2 nc 1" }, { "Text":"c 2 nc 2" } ] }
    ]
}
""";

        await AdHocCosmosTestHelpers.CreateCustomEntityHelperAsync(
            entitiesContainer,
            baseline,
            CancellationToken.None);

        var duplicatedNavigations =
$$$"""
{
    "Id": 2,
    "$type": "Entity",
    "id": "2",
    "Scenario": "duplicated navigations",
    "OptionalReference": {"NestedOptional": { "Text":"or no" }, "NestedOptional": { "Text":"or no dupnav" }, "NestedRequired": { "Text":"or nr" }, "NestedCollection": [ { "Text":"or nc 1" }, { "Text":"or nc 2" } ], "NestedCollection": [ { "Text":"or nc 1 dupnav" }, { "Text":"or nc 2 dupnav" } ], "NestedRequired": { "Text":"or nr dupnav" } },
    "RequiredReference": {"NestedOptional": { "Text":"rr no" }, "NestedOptional": { "Text":"rr no dupnav" }, "NestedRequired": { "Text":"rr nr" }, "NestedCollection": [ { "Text":"rr nc 1" }, { "Text":"rr nc 2" } ], "NestedCollection": [ { "Text":"rr nc 1 dupnav" }, { "Text":"rr nc 2 dupnav" } ], "NestedRequired": { "Text":"rr nr dupnav" } },
    "Collection":
    [
        {"NestedOptional": { "Text":"c 1 no" }, "NestedOptional": { "Text":"c 1 no dupnav" }, "NestedRequired": { "Text":"c 1 nr" }, "NestedCollection": [ { "Text":"c 1 nc 1" }, { "Text":"c 1 nc 2" } ], "NestedCollection": [ { "Text":"c 1 nc 1 dupnav" }, { "Text":"c 1 nc 2 dupnav" } ], "NestedRequired": { "Text":"c 1 nr dupnav" } },
        {"NestedOptional": { "Text":"c 2 no" }, "NestedOptional": { "Text":"c 2 no dupnav" }, "NestedRequired": { "Text":"c 2 nr" }, "NestedCollection": [ { "Text":"c 2 nc 1" }, { "Text":"c 2 nc 2" } ], "NestedCollection": [ { "Text":"c 2 nc 1 dupnav" }, { "Text":"c 2 nc 2 dupnav" } ], "NestedRequired": { "Text":"c 2 nr dupnav" } }
    ]
}
""";

        await AdHocCosmosTestHelpers.CreateCustomEntityHelperAsync(
            entitiesContainer,
            duplicatedNavigations,
            CancellationToken.None);

        var duplicatedScalars =
$$$"""
{
    "Id": 3,
    "$type": "Entity",
    "id": "3",
    "Scenario": "duplicated scalars",
    "OptionalReference": {"NestedOptional": { "Text":"or no", "Text":"or no dupprop" }, "NestedRequired": { "Text":"or nr", "Text":"or nr dupprop" }, "NestedCollection": [ { "Text":"or nc 1", "Text":"or nc 1 dupprop" }, { "Text":"or nc 2", "Text":"or nc 2 dupprop" } ] },
    "RequiredReference": {"NestedOptional": { "Text":"rr no", "Text":"rr no dupprop" }, "NestedRequired": { "Text":"rr nr", "Text":"rr nr dupprop" }, "NestedCollection": [ { "Text":"rr nc 1", "Text":"rr nc 1 dupprop" }, { "Text":"rr nc 2", "Text":"rr nc 2 dupprop" } ] },
    "Collection":
    [
        {"NestedOptional": { "Text":"c 1 no", "Text":"c 1 no dupprop" }, "NestedRequired": { "Text":"c 1 nr", "Text":"c 1 nr dupprop" }, "NestedCollection": [ { "Text":"c 1 nc 1", "Text":"c 1 nc 1 dupprop" }, { "Text":"c 1 nc 2", "Text":"c 1 nc 2 dupprop" } ] },
        {"NestedOptional": { "Text":"c 2 no", "Text":"c 2 no dupprop" }, "NestedRequired": { "Text":"c 2 nr", "Text":"c 2 nr dupprop" }, "NestedCollection": [ { "Text":"c 2 nc 1", "Text":"c 2 nc 1 dupprop" }, { "Text":"c 2 nc 2", "Text":"c 2 nc 2 dupprop" } ] }
    ]
}
""";

        await AdHocCosmosTestHelpers.CreateCustomEntityHelperAsync(
            entitiesContainer,
            duplicatedScalars,
            CancellationToken.None);

        var emptyNavs =
$$$"""
{
    "Id": 4,
    "$type": "Entity",
    "id": "4",
    "Scenario": "empty navigation property names",
    "OptionalReference": {"": { "Text":"or no" }, "": { "Text":"or nr" }, "": [ { "Text":"or nc 1" }, { "Text":"or nc 2" } ] },
    "RequiredReference": {"": { "Text":"rr no" }, "": { "Text":"rr nr" }, "": [ { "Text":"rr nc 1" }, { "Text":"rr nc 2" } ] },
    "Collection":
    [
        {"": { "Text":"c 1 no" }, "": { "Text":"c 1 nr" }, "": [ { "Text":"c 1 nc 1" }, { "Text":"c 1 nc 2" } ] },
        {"": { "Text":"c 2 no" }, "": { "Text":"c 2 nr" }, "": [ { "Text":"c 2 nc 1" }, { "Text":"c 2 nc 2" } ] }
    ]
}
""";

        await AdHocCosmosTestHelpers.CreateCustomEntityHelperAsync(
            entitiesContainer,
            emptyNavs,
            CancellationToken.None);

        var emptyScalars =
$$$"""
{
    "Id": 5,
    "$type": "Entity",
    "id": "5",
    "Scenario": "empty scalar property names",
    "OptionalReference": {"NestedOptional": { "":"or no" }, "NestedRequired": { "":"or nr" }, "NestedCollection": [ { "":"or nc 1" }, { "":"or nc 2" } ] },
    "RequiredReference": {"NestedOptional": { "":"rr no" }, "NestedRequired": { "":"rr nr" }, "NestedCollection": [ { "":"rr nc 1" }, { "":"rr nc 2" } ] },
    "Collection":
    [
        {"NestedOptional": { "":"c 1 no" }, "NestedRequired": { "":"c 1 nr" }, "NestedCollection": [ { "":"c 1 nc 1" }, { "":"c 1 nc 2" } ] },
        {"NestedOptional": { "":"c 2 no" }, "NestedRequired": { "":"c 2 nr" }, "NestedCollection": [ { "":"c 2 nc 1" }, { "":"c 2 nc 2" } ] }
    ]
}
""";

        await AdHocCosmosTestHelpers.CreateCustomEntityHelperAsync(
            entitiesContainer,
            emptyScalars,
            CancellationToken.None);

        var nullNavs =
$$$"""
{
    "Id": 10,
    "$type": "Entity",
    "id": "10",
    "Scenario": "null navigation property names",
    "OptionalReference": {null: { "Text":"or no" }, null: { "Text":"or nr" }, null: [ { "Text":"or nc 1" }, { "Text":"or nc 2" } ] },
    "RequiredReference": {null: { "Text":"rr no" }, null: { "Text":"rr nr" }, null: [ { "Text":"rr nc 1" }, { "Text":"rr nc 2" } ] },
    "Collection":
    [
        {null: { "Text":"c 1 no" }, null: { "Text":"c 1 nr" }, null: [ { "Text":"c 1 nc 1" }, { "Text":"c 1 nc 2" } ] },
        {null: { "Text":"c 2 no" }, null: { "Text":"c 2 nr" }, null: [ { "Text":"c 2 nc 1" }, { "Text":"c 2 nc 2" } ] }
    ]
}
""";

        await AdHocCosmosTestHelpers.CreateCustomEntityHelperAsync(
            entitiesContainer,
            nullNavs,
            CancellationToken.None);

        var nullScalars =
$$$"""
{
    "Id": 11,
    "$type": "Entity",
    "id": "11",
    "Scenario": "null scalar property names",
    "OptionalReference": {"NestedOptional": { null:"or no", "Text":"or no nonnull" }, "NestedRequired": { null:"or nr", "Text":"or nr nonnull" }, "NestedCollection": [ { null:"or nc 1", "Text":"or nc 1 nonnull" }, { null:"or nc 2", "Text":"or nc 2 nonnull" } ] },
    "RequiredReference": {"NestedOptional": { null:"rr no", "Text":"rr no nonnull" }, "NestedRequired": { null:"rr nr", "Text":"rr nr nonnull" }, "NestedCollection": [ { null:"rr nc 1", "Text":"rr nc 1 nonnull" }, { null:"rr nc 2", "Text":"rr nc 2 nonnull" } ] },
    "Collection":
    [
        {"NestedOptional": { null:"c 1 no", "Text":"c 1 no nonnull" }, "NestedRequired": { null:"c 1 nr", "Text":"c 1 nr nonnull" }, "NestedCollection": [ { null:"c 1 nc 1", "Text":"c 1 nc 1 nonnull" }, { null:"c 1 nc 2", "Text":"c 1 nc 2 nonnull" } ] },
        {"NestedOptional": { null:"c 2 no", "Text":"c 2 no nonnull" }, "NestedRequired": { null:"c 2 nr", "Text":"c 2 nr nonnull" }, "NestedCollection": [ { null:"c 2 nc 1", "Text":"c 2 nc 1 nonnull" }, { null:"c 2 nc 2", "Text":"c 2 nc 2 nonnull" } ] }
    ]
}
""";

        await AdHocCosmosTestHelpers.CreateCustomEntityHelperAsync(
            entitiesContainer,
            nullScalars,
            CancellationToken.None);
    }

    #endregion

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
