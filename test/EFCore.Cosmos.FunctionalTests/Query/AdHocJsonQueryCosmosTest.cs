// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.EntityFrameworkCore.Cosmos.Internal;
using Microsoft.EntityFrameworkCore.Cosmos.Storage.Internal;

namespace Microsoft.EntityFrameworkCore.Query;

public class AdHocJsonQueryCosmosTest(NonSharedFixture fixture) : AdHocJsonQueryTestBase(fixture)
{
    #region 21006

    public override Task Project_root_with_missing_scalars(bool async)
    => CosmosTestHelpers.Instance.NoSyncTest(async, async async =>
    {
        await base.Project_root_with_missing_scalars(async);

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["Id"] < 4)
""");
    });

    [Theory(Skip = "issue #35702")]
    public override Task Project_top_level_json_entity_with_missing_scalars(bool async)
    => CosmosTestHelpers.Instance.NoSyncTest(async, async async =>
    {
        await base.Project_top_level_json_entity_with_missing_scalars(async);

        AssertSql();
    });

    public override async Task Project_nested_json_entity_with_missing_scalars(bool async)
    {
        // Throws sync not supported exception for sync
        if (async)
        {
            await AssertTranslationFailed(() => base.Project_nested_json_entity_with_missing_scalars(async));

            AssertSql();
        }
    }

    public override Task Project_top_level_entity_with_null_value_required_scalars(bool async)
    => CosmosTestHelpers.Instance.NoSyncTest(async, async async =>
    {
        await base.Project_top_level_entity_with_null_value_required_scalars(async);

        AssertSql(
            """
SELECT c["Id"], c["RequiredReference"]
FROM root c
WHERE (c["Id"] = 4)
""");
    });

    [Fact]
    public virtual async Task Project_entity_with_null_value_required_scalars()
    {
        var contextFactory = await InitializeNonSharedTest<Context21006>(
            onConfiguring: b => b.ConfigureWarnings(ConfigureWarnings),
            onModelCreating: OnModelCreating21006,
            seed: Seed21006);

        await using var context = contextFactory.CreateDbContext();

        var result = await context.Set<Context21006.Entity>().Where(x => x.Id == 4).AsNoTracking().ToListAsync();

        var nullScalars = result.Single();

        Assert.Equal(default, nullScalars.RequiredReference.Number);
    }

    [Fact]
    public virtual async Task Project_null_value_required_scalar()
    {
        var contextFactory = await InitializeNonSharedTest<Context21006>(
            onConfiguring: b => b.ConfigureWarnings(ConfigureWarnings),
            onModelCreating: OnModelCreating21006,
            seed: Seed21006);

        await using var context = contextFactory.CreateDbContext();

        // Same as in 10.0
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => context.Set<Context21006.Entity>().Where(x => x.Id == 4).Select(x => x.RequiredReference.Number).ToListAsync());
        Assert.Equal("Nullable object must have a value.", ex.Message);
    }

    public override Task Project_root_entity_with_missing_required_navigation(bool async)
    => CosmosTestHelpers.Instance.NoSyncTest(async, async async =>
    {
        await base.Project_root_entity_with_missing_required_navigation(async);

        AssertSql(
            """
ReadItem(?, ?)
""");
    });

    public override async Task Project_missing_required_navigation(bool async)
    {
        // Throws sync not supported exception for sync
        if (async)
        {
            // Cosmos will filter out undefined result
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => base.Project_missing_required_navigation(async));
            Assert.Equal("Sequence contains no elements", ex.Message);

            AssertSql(
                """
SELECT VALUE c["RequiredReference"]["NestedRequiredReference"]
FROM root c
WHERE (c["Id"] = 5)
""");
        }
    }

    public override Task Project_root_entity_with_null_required_navigation(bool async)
    => CosmosTestHelpers.Instance.NoSyncTest(async, async async =>
    {
        await base.Project_root_entity_with_null_required_navigation(async);

        AssertSql(
            """
ReadItem(?, ?)
""");
    });

    public override Task Project_null_required_navigation(bool async)
    => CosmosTestHelpers.Instance.NoSyncTest(async, async async =>
    {
        await base.Project_null_required_navigation(async);

        AssertSql(
            """
SELECT VALUE c["RequiredReference"]
FROM root c
WHERE (c["Id"] = 6)
""");
    });

    public override async Task Project_missing_required_scalar(bool async)
    {
        // Throws sync not supported exception for sync
        if (async)
        {
            // https://github.com/dotnet/efcore/issues/38298#issuecomment-4726236589
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => base.Project_missing_required_scalar(async));

            Assert.Equal(CosmosStrings.ProjectionUndefined, exception.Message);

            AssertSql(
                """
SELECT c["Id"], c["RequiredReference"]["Number"]
FROM root c
WHERE (c["Id"] = 2)
""");
        }
    }

    public override Task Project_null_required_scalar(bool async)
    => CosmosTestHelpers.Instance.NoSyncTest(async, async async =>
    {
        await base.Project_null_required_scalar(async);

        AssertSql(
            """
SELECT c["Id"], c["RequiredReference"]["Number"]
FROM root c
WHERE (c["Id"] = 4)
""");
    });

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
            """
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
            """
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
            """
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
            """
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
            }
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
            }
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
        }
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
        }
    }
}
""";

        await AdHocCosmosTestHelpers.CreateCustomEntityHelperAsync(
            entitiesContainer,
            missingRequiredNav,
            CancellationToken.None);

        var nullRequiredNav =
            """
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
            """
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

    [Theory(Skip = "issue #35702")]
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
            """
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
            """
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
            """
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
            """
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
            """
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
        var message = (await Assert.ThrowsAsync<InvalidOperationException>(() => base.Try_project_collection_but_JSON_is_entity()))
            .Message;

        Assert.Equal(CoreStrings.JsonReaderInvalidTokenType(JsonTokenType.StartObject), message);
    }

    public override async Task Try_project_reference_but_JSON_is_collection()
    {
        var message = (await Assert.ThrowsAsync<InvalidOperationException>(() => base.Try_project_reference_but_JSON_is_collection()))
            .Message;

        Assert.Equal(CoreStrings.JsonReaderInvalidTokenType(JsonTokenType.StartArray), message);
    }

    protected override void OnModelCreating34960(ModelBuilder modelBuilder)
    {
        base.OnModelCreating34960(modelBuilder);
        modelBuilder.Entity<Context34960.Entity>().ToContainer("Entities");
        modelBuilder.Entity<Context34960.JunkEntity>().ToContainer("Junk");
    }

    protected override async Task Seed34960(Context34960 context)
    {
        await base.Seed34960(context);

        var wrapper = (CosmosClientWrapper)context.GetService<ICosmosClientWrapper>();
        var singletonWrapper = context.GetService<ISingletonCosmosClientWrapper>();
        var entitiesContainer = singletonWrapper.Client.GetContainer(context.Database.GetCosmosDatabaseId(), containerId: "Entities");

        var json =
            """
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
            """
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
            """
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

    public override async Task Project_element_of_json_array_of_primitives()
    {
        // https://github.com/Azure/azure-cosmos-db-emulator-docker/issues/335
        CosmosTestEnvironment.SkipOnLinuxEmulator();

        await base.Project_element_of_json_array_of_primitives();
    }

    protected override void OnModelCreatingArrayOfPrimitives(ModelBuilder modelBuilder)
        => base.OnModelCreatingArrayOfPrimitives(modelBuilder);

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
            """
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
            """
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

    public override async Task Project_shadow_properties_from_json_entity()
    {
        // https://github.com/Azure/azure-cosmos-db-emulator-docker/issues/335
        CosmosTestEnvironment.SkipOnLinuxEmulator();

        await base.Project_shadow_properties_from_json_entity();
    }

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

            b.OwnsOne(
                x => x.ReferenceWithCtor, b =>
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
            """
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
        => base.OnModelCreatingLazyLoadingProxies(modelBuilder);

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
            """
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
            """
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

    // Insertion of the bad data fails thanks to json validation by Cosmos DB
    public override Task Bad_json_properties_null_navigations(bool noTracking)
       => Task.CompletedTask;

    public override Task Bad_json_properties_null_scalars(bool noTracking)
       => Task.CompletedTask;

    // Insertion of the bad data is deduplicated by Cosmos DB
    public override Task Bad_json_properties_duplicated_navigations(bool noTracking)
        => Task.CompletedTask;

    public override Task Bad_json_properties_duplicated_scalars(bool noTracking)
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
            """
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
            """
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
            """
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
            """
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
            """
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

        // Insertion of the bad data fails thanks to json validation by Cosmos DB
        // nullNavs or "Scenario": "null navigation property names" can not be tested
        // nullScalars or "Scenario": "null scalar property names" can not be tested
    }

    #endregion

    #region Bad primary keys

    // TODO: This only asserts the current behavior of the provider, but the behavior is probably not ideal
    // Consider:
    // - Never storing foreign key properties in json, For projections when the foreign key is a member, we would need to translate to the owner id in the query pipeline
    // - Not allowing (explicit?) primary keys on embedded entity types
    // - Deprecating owned entity types as a whole

    // The workaround for old providers is:
    // For missing implicit PK properties on embedded collection entities, use ordinal

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Primary_key_baseline(bool tracking)
    {
        var contextFactory = await InitializeNonSharedTest<ContextBadPrimaryKeyJsonProperties>(
            onConfiguring: b => b.ConfigureWarnings(ConfigureWarnings),
            seed: async context =>
            {
                var entity = new ContextBadPrimaryKeyJsonProperties.Entity
                {
                    Id = 1,
                    Associate = new() { Id = 2 },
                    KeyedAssociate = new() { Id = 3 }, // Overwritten to 1 (entity id)
                    AssociateCollection =
                    [
                        new() { Id = 4 },
                        new() { Id = 5 }
                    ],
                    KeyedAssociateCollection =
                    [
                        new() { Id = 6 },
                        new() { Id = 7 }
                    ],
                    ForeignKeyAssociate = new() { Id = 8 },
                    KeyedForeignKeyAssociate = new() { Id = 9 },
                    ForeignKeyAssociateCollection =
                    [
                        new() { Id = 10 },
                        new() { Id = 11 }
                    ],
                    KeyedForeignKeyAssociateCollection =
                    [
                        new() { Id = 12 },
                        new() { Id = 13 }
                    ]
                };

                context.Add(entity);
                await context.SaveChangesAsync();
            });

        using var context = contextFactory.CreateDbContext();
        var client = context.Database.GetCosmosClient();
        var container = client.GetContainer(context.Database.GetCosmosDatabaseId(), containerId: "Entities");

        var entityJson = JsonNode.Parse("""
{
  "$type": "Entity",
  "Id": 1,
  "id": "1",
  "Associate": {
    "Id": 2
  },
  "KeyedAssociate": {},
  "AssociateCollection": [
    {
      "Id": 4
    },
    {
      "Id": 5
    }
  ],
  "KeyedAssociateCollection": [
    {
      "Id": 6,
      "EntityId": 1
    },
    {
      "Id": 7,
      "EntityId": 1
    }
  ],
  "ForeignKeyAssociate": {
    "Id": 8
  },
  "KeyedForeignKeyAssociate": {
    "Id": 9,
    "EntityId": 1
  },
  "ForeignKeyAssociateCollection": [
    {
      "Id": 10
    },
    {
      "Id": 11
    }
  ],
  "KeyedForeignKeyAssociateCollection": [
    {
      "Id": 12,
      "EntityId": 1
    },
    {
      "Id": 13,
      "EntityId": 1
    }
  ]
}
""")!.AsObject();

        var dbJson = JsonNode.Parse(
                new StreamReader(
                    (await container.ReadItemStreamAsync("1", Azure.Cosmos.PartitionKey.None)).Content).ReadToEnd())!.AsObject();
        foreach (var property in dbJson.Where(x => x.Key.StartsWith("_")).ToList())
        {
            dbJson.Remove(property.Key);
        }

        var text = dbJson.ToString();

        Assert.True(JsonNode.DeepEquals(entityJson, dbJson));

        var entity = await context.Entities.SingleAsync();
        Assert.Equal(2, entity.Associate.Id);
        Assert.Equal(1, entity.KeyedAssociate.Id); // Is overwritten..

        Assert.Equal(4, entity.AssociateCollection.First().Id);
        Assert.Equal(6, entity.KeyedAssociateCollection.First().Id);

        Assert.Equal(8, entity.ForeignKeyAssociate.Id);
        Assert.Equal(9, entity.KeyedForeignKeyAssociate.Id);

        Assert.Equal(10, entity.ForeignKeyAssociateCollection.First().Id);
        Assert.Equal(12, entity.KeyedForeignKeyAssociateCollection.First().Id);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Root_null_primary_key_throws(bool tracking)
    {
        var contextFactory = await InitializeNonSharedTest<ContextBadPrimaryKeyJsonProperties>(
            onConfiguring: b => b.ConfigureWarnings(ConfigureWarnings),
            seed: async context =>
            {
                var client = context.Database.GetCosmosClient();
                var container = client.GetContainer(context.Database.GetCosmosDatabaseId(), containerId: "Entities");

                var entity = """
{
  "$type": "Entity",
  "Id": null,
  "id": "1",
  "Associate": {
    "Id": 2
  },
  "KeyedAssociate": {},
  "AssociateCollection": [
    {
      "Id": 4
    },
    {
      "Id": 5
    }
  ],
  "KeyedAssociateCollection": [
    {
      "Id": 6,
      "EntityId": 1
    },
    {
      "Id": 7,
      "EntityId": 1
    }
  ],
  "ForeignKeyAssociate": {
    "Id": 8
  },
  "KeyedForeignKeyAssociate": {
    "Id": 9,
    "EntityId": 1
  },
  "ForeignKeyAssociateCollection": [
    {
      "Id": 10
    },
    {
      "Id": 11
    }
  ],
  "KeyedForeignKeyAssociateCollection": [
    {
      "Id": 12,
      "EntityId": 1
    },
    {
      "Id": 13,
      "EntityId": 1
    }
  ]
}
""";
                await AdHocCosmosTestHelpers.CreateCustomEntityHelperAsync(
                    container,
                    entity,
                    CancellationToken.None);
            });

        using var context = contextFactory.CreateDbContext();
        IQueryable<ContextBadPrimaryKeyJsonProperties.Entity> query = context.Entities;
        if (!tracking)
        {
            query = query.AsNoTracking();
        }

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => query.SingleOrDefaultAsync());
        Assert.Equal(CoreStrings.InvalidKeyValue("Entity", "Id"), ex.Message);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Root_missing_primary_key_throws(bool tracking)
    {
        var contextFactory = await InitializeNonSharedTest<ContextBadPrimaryKeyJsonProperties>(
            onConfiguring: b => b.ConfigureWarnings(ConfigureWarnings),
            seed: async context =>
            {
                var client = context.Database.GetCosmosClient();
                var container = client.GetContainer(context.Database.GetCosmosDatabaseId(), containerId: "Entities");

                var entity = """
{
  "$type": "Entity",
  "id": "1",
  "Associate": {
    "Id": 2
  },
  "KeyedAssociate": {},
  "AssociateCollection": [
    {
      "Id": 4
    },
    {
      "Id": 5
    }
  ],
  "KeyedAssociateCollection": [
    {
      "Id": 6,
      "EntityId": 1
    },
    {
      "Id": 7,
      "EntityId": 1
    }
  ],
  "ForeignKeyAssociate": {
    "Id": 8
  },
  "KeyedForeignKeyAssociate": {
    "Id": 9,
    "EntityId": 1
  },
  "ForeignKeyAssociateCollection": [
    {
      "Id": 10
    },
    {
      "Id": 11
    }
  ],
  "KeyedForeignKeyAssociateCollection": [
    {
      "Id": 12,
      "EntityId": 1
    },
    {
      "Id": 13,
      "EntityId": 1
    }
  ]
}
""";
                await AdHocCosmosTestHelpers.CreateCustomEntityHelperAsync(
                    container,
                    entity,
                    CancellationToken.None);
            });

        using var context = contextFactory.CreateDbContext();
        IQueryable<ContextBadPrimaryKeyJsonProperties.Entity> query = context.Entities;
        if (!tracking)
        {
            query = query.AsNoTracking();
        }

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => query.SingleOrDefaultAsync());

        Assert.Equal(CoreStrings.InvalidKeyValue("Entity", "Id"), ex.Message);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Associate_null_non_primary_key_doesnt_throw(bool tracking)
    {
        var contextFactory = await InitializeNonSharedTest<ContextBadPrimaryKeyJsonProperties>(
            onConfiguring: b => b.ConfigureWarnings(ConfigureWarnings),
            seed: async context =>
            {
                var client = context.Database.GetCosmosClient();
                var container = client.GetContainer(context.Database.GetCosmosDatabaseId(), containerId: "Entities");

                var entity = """
{
  "$type": "Entity",
  "Id": 1,
  "id": "1",
  "Associate": {
    "Id": null
  },
  "KeyedAssociate": {},
  "AssociateCollection": [
    {
      "Id": 4
    },
    {
      "Id": 5
    }
  ],
  "KeyedAssociateCollection": [
    {
      "Id": 6,
      "EntityId": 1
    },
    {
      "Id": 7,
      "EntityId": 1
    }
  ],
  "ForeignKeyAssociate": {
    "Id": 8
  },
  "KeyedForeignKeyAssociate": {
    "Id": 9,
    "EntityId": 1
  },
  "ForeignKeyAssociateCollection": [
    {
      "Id": 10
    },
    {
      "Id": 11
    }
  ],
  "KeyedForeignKeyAssociateCollection": [
    {
      "Id": 12,
      "EntityId": 1
    },
    {
      "Id": 13,
      "EntityId": 1
    }
  ]
}
""";
                await AdHocCosmosTestHelpers.CreateCustomEntityHelperAsync(
                    container,
                    entity,
                    CancellationToken.None);
            });

        using var context = contextFactory.CreateDbContext();
        IQueryable<ContextBadPrimaryKeyJsonProperties.Entity> query = context.Entities;
        if (!tracking)
        {
            query = query.AsNoTracking();
        }

        var result = await query.SingleAsync();
        Assert.Equal(0, result.Associate.Id);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Associate_missing_non_primary_key_doesnt_throw(bool tracking)
    {
        var contextFactory = await InitializeNonSharedTest<ContextBadPrimaryKeyJsonProperties>(
            onConfiguring: b => b.ConfigureWarnings(ConfigureWarnings),
            seed: async context =>
            {
                var client = context.Database.GetCosmosClient();
                var container = client.GetContainer(context.Database.GetCosmosDatabaseId(), containerId: "Entities");

                var entity = """
{
  "$type": "Entity",
  "Id": 1,
  "id": "1",
  "Associate": {
  },
  "KeyedAssociate": {},
  "AssociateCollection": [
    {
      "Id": 4
    },
    {
      "Id": 5
    }
  ],
  "KeyedAssociateCollection": [
    {
      "Id": 6,
      "EntityId": 1
    },
    {
      "Id": 7,
      "EntityId": 1
    }
  ],
  "ForeignKeyAssociate": {
    "Id": 8
  },
  "KeyedForeignKeyAssociate": {
    "Id": 9,
    "EntityId": 1
  },
  "ForeignKeyAssociateCollection": [
    {
      "Id": 10
    },
    {
      "Id": 11
    }
  ],
  "KeyedForeignKeyAssociateCollection": [
    {
      "Id": 12,
      "EntityId": 1
    },
    {
      "Id": 13,
      "EntityId": 1
    }
  ]
}
""";
                await AdHocCosmosTestHelpers.CreateCustomEntityHelperAsync(
                    container,
                    entity,
                    CancellationToken.None);
            });

        using var context = contextFactory.CreateDbContext();
        IQueryable<ContextBadPrimaryKeyJsonProperties.Entity> query = context.Entities;
        if (!tracking)
        {
            query = query.AsNoTracking();
        }

        var result = await query.SingleAsync();
        Assert.Equal(0, result.Associate.Id);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task KeyedAssociate_null_primary_key_uses_implicit(bool tracking)
    {
        var contextFactory = await InitializeNonSharedTest<ContextBadPrimaryKeyJsonProperties>(
            onConfiguring: b => b.ConfigureWarnings(ConfigureWarnings),
            seed: async context =>
            {
                var client = context.Database.GetCosmosClient();
                var container = client.GetContainer(context.Database.GetCosmosDatabaseId(), containerId: "Entities");

                var entity = """
{
  "$type": "Entity",
  "Id": 1,
  "id": "1",
  "Associate": {
    "Id": 2
  },
  "KeyedAssociate": {
    "Id": null
  },
  "AssociateCollection": [
    {
      "Id": 4
    },
    {
      "Id": 5
    }
  ],
  "KeyedAssociateCollection": [
    {
      "Id": 6,
      "EntityId": 1
    },
    {
      "Id": 7,
      "EntityId": 1
    }
  ],
  "ForeignKeyAssociate": {
    "Id": 8
  },
  "KeyedForeignKeyAssociate": {
    "Id": 9,
    "EntityId": 1
  },
  "ForeignKeyAssociateCollection": [
    {
      "Id": 10
    },
    {
      "Id": 11
    }
  ],
  "KeyedForeignKeyAssociateCollection": [
    {
      "Id": 12,
      "EntityId": 1
    },
    {
      "Id": 13,
      "EntityId": 1
    }
  ]
}
""";
                await AdHocCosmosTestHelpers.CreateCustomEntityHelperAsync(
                    container,
                    entity,
                    CancellationToken.None);
            });

        using var context = contextFactory.CreateDbContext();
        IQueryable<ContextBadPrimaryKeyJsonProperties.Entity> query = context.Entities;
        if (!tracking)
        {
            query = query.AsNoTracking();
        }

        var result = await query.SingleAsync();
        Assert.Equal(1, result.KeyedAssociate.Id);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task KeyedAssociate_incorrect_primary_key_uses_implicit(bool tracking)
    {
        var contextFactory = await InitializeNonSharedTest<ContextBadPrimaryKeyJsonProperties>(
            onConfiguring: b => b.ConfigureWarnings(ConfigureWarnings),
            seed: async context =>
            {
                var client = context.Database.GetCosmosClient();
                var container = client.GetContainer(context.Database.GetCosmosDatabaseId(), containerId: "Entities");

                var entity = """
{
  "$type": "Entity",
  "Id": 1,
  "id": "1",
  "Associate": {
    "Id": 2
  },
  "KeyedAssociate": {
    "Id": 99
  },
  "AssociateCollection": [
    {
      "Id": 4
    },
    {
      "Id": 5
    }
  ],
  "KeyedAssociateCollection": [
    {
      "Id": 6,
      "EntityId": 1
    },
    {
      "Id": 7,
      "EntityId": 1
    }
  ],
  "ForeignKeyAssociate": {
    "Id": 8
  },
  "KeyedForeignKeyAssociate": {
    "Id": 9,
    "EntityId": 1
  },
  "ForeignKeyAssociateCollection": [
    {
      "Id": 10
    },
    {
      "Id": 11
    }
  ],
  "KeyedForeignKeyAssociateCollection": [
    {
      "Id": 12,
      "EntityId": 1
    },
    {
      "Id": 13,
      "EntityId": 1
    }
  ]
}
""";
                await AdHocCosmosTestHelpers.CreateCustomEntityHelperAsync(
                    container,
                    entity,
                    CancellationToken.None);
            });

        using var context = contextFactory.CreateDbContext();
        IQueryable<ContextBadPrimaryKeyJsonProperties.Entity> query = context.Entities;
        if (!tracking)
        {
            query = query.AsNoTracking();
        }

        var result = await query.SingleAsync();
        Assert.Equal(1, result.KeyedAssociate.Id);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task AssociateCollection_null_primary_key_throws(bool tracking)
    {
        var contextFactory = await InitializeNonSharedTest<ContextBadPrimaryKeyJsonProperties>(
            onConfiguring: b => b.ConfigureWarnings(ConfigureWarnings),
            seed: async context =>
            {
                var client = context.Database.GetCosmosClient();
                var container = client.GetContainer(context.Database.GetCosmosDatabaseId(), containerId: "Entities");

                var entity = """
{
  "$type": "Entity",
  "Id": 1,
  "id": "1",
  "Associate": {
    "Id": 2
  },
  "KeyedAssociate": {},
  "AssociateCollection": [
    {
      "Id": null
    },
    {
      "Id": null
    }
  ],
  "KeyedAssociateCollection": [
    {
      "Id": 6,
      "EntityId": 1
    },
    {
      "Id": 7,
      "EntityId": 1
    }
  ],
  "ForeignKeyAssociate": {
    "Id": 8
  },
  "KeyedForeignKeyAssociate": {
    "Id": 9,
    "EntityId": 1
  },
  "ForeignKeyAssociateCollection": [
    {
      "Id": 10
    },
    {
      "Id": 11
    }
  ],
  "KeyedForeignKeyAssociateCollection": [
    {
      "Id": 12,
      "EntityId": 1
    },
    {
      "Id": 13,
      "EntityId": 1
    }
  ]
}
""";
                await AdHocCosmosTestHelpers.CreateCustomEntityHelperAsync(
                    container,
                    entity,
                    CancellationToken.None);
            });

        using var context = contextFactory.CreateDbContext();
        IQueryable<ContextBadPrimaryKeyJsonProperties.Entity> query = context.Entities;
        if (!tracking)
        {
            query = query.AsNoTracking();
        }

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => query.SingleAsync());
        Assert.Equal(CoreStrings.InvalidKeyValue("Entity.AssociateCollection#AssociateEntity", "Id"), ex.Message);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task AssociateCollection_missing_primary_key_doesnt_throw_and_uses_ordinal_workaround(bool tracking)
    {
        var contextFactory = await InitializeNonSharedTest<ContextBadPrimaryKeyJsonProperties>(
            onConfiguring: b => b.ConfigureWarnings(ConfigureWarnings),
            seed: async context =>
            {
                var client = context.Database.GetCosmosClient();
                var container = client.GetContainer(context.Database.GetCosmosDatabaseId(), containerId: "Entities");

                var entity = """
{
  "$type": "Entity",
  "Id": 1,
  "id": "1",
  "Associate": {
    "Id": 2
  },
  "KeyedAssociate": {},
  "AssociateCollection": [
    {
    },
    {
    }
  ],
  "KeyedAssociateCollection": [
    {
      "Id": 6,
      "EntityId": 1
    },
    {
      "Id": 7,
      "EntityId": 1
    }
  ],
  "ForeignKeyAssociate": {
    "Id": 8
  },
  "KeyedForeignKeyAssociate": {
    "Id": 9,
    "EntityId": 1
  },
  "ForeignKeyAssociateCollection": [
    {
      "Id": 10
    },
    {
      "Id": 11
    }
  ],
  "KeyedForeignKeyAssociateCollection": [
    {
      "Id": 12,
      "EntityId": 1
    },
    {
      "Id": 13,
      "EntityId": 1
    }
  ]
}
""";
                await AdHocCosmosTestHelpers.CreateCustomEntityHelperAsync(
                    container,
                    entity,
                    CancellationToken.None);
            });

        using var context = contextFactory.CreateDbContext();
        IQueryable<ContextBadPrimaryKeyJsonProperties.Entity> query = context.Entities;
        if (!tracking)
        {
            query = query.AsNoTracking();
        }

        var result = await query.SingleAsync();
        Assert.Equal(1, result.AssociateCollection.First().Id);
        Assert.Equal(2, result.AssociateCollection.Last().Id);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task KeyedAssociateCollection_null_primary_key_throws(bool tracking)
    {
        var contextFactory = await InitializeNonSharedTest<ContextBadPrimaryKeyJsonProperties>(
            onConfiguring: b => b.ConfigureWarnings(ConfigureWarnings),
            seed: async context =>
            {
                var client = context.Database.GetCosmosClient();
                var container = client.GetContainer(context.Database.GetCosmosDatabaseId(), containerId: "Entities");

                var entity = """
{
  "$type": "Entity",
  "Id": 1,
  "id": "1",
  "Associate": {
    "Id": 2
  },
  "KeyedAssociate": {},
  "AssociateCollection": [
    {
        "Id": 4
    },
    {
        "Id": 5
    }
  ],
  "KeyedAssociateCollection": [
    {
      "Id": null,
      "EntityId": 1
    },
    {
      "Id": null,
      "EntityId": 1
    }
  ],
  "ForeignKeyAssociate": {
    "Id": 8
  },
  "KeyedForeignKeyAssociate": {
    "Id": 9,
    "EntityId": 1
  },
  "ForeignKeyAssociateCollection": [
    {
      "Id": 10
    },
    {
      "Id": 11
    }
  ],
  "KeyedForeignKeyAssociateCollection": [
    {
      "Id": 12,
      "EntityId": 1
    },
    {
      "Id": 13,
      "EntityId": 1
    }
  ]
}
""";
                await AdHocCosmosTestHelpers.CreateCustomEntityHelperAsync(
                    container,
                    entity,
                    CancellationToken.None);
            });

        using var context = contextFactory.CreateDbContext();
        IQueryable<ContextBadPrimaryKeyJsonProperties.Entity> query = context.Entities;
        if (!tracking)
        {
            query = query.AsNoTracking();
        }

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => query.SingleAsync());
        Assert.Equal(CoreStrings.InvalidKeyValue("Entity.KeyedAssociateCollection#AssociateEntity", "Id"), ex.Message);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task KeyedAssociateCollection_missing_primary_key_throws(bool tracking)
    {
        var contextFactory = await InitializeNonSharedTest<ContextBadPrimaryKeyJsonProperties>(
            onConfiguring: b => b.ConfigureWarnings(ConfigureWarnings),
            seed: async context =>
            {
                var client = context.Database.GetCosmosClient();
                var container = client.GetContainer(context.Database.GetCosmosDatabaseId(), containerId: "Entities");

                var entity = """
{
  "$type": "Entity",
  "Id": 1,
  "id": "1",
  "Associate": {
    "Id": 2
  },
  "KeyedAssociate": {},
  "AssociateCollection": [
    {
        "Id": 4
    },
    {
        "Id": 5
    }
  ],
  "KeyedAssociateCollection": [
    {
      "EntityId": 1
    },
    {
      "EntityId": 1
    }
  ],
  "ForeignKeyAssociate": {
    "Id": 8
  },
  "KeyedForeignKeyAssociate": {
    "Id": 9,
    "EntityId": 1
  },
  "ForeignKeyAssociateCollection": [
    {
      "Id": 10
    },
    {
      "Id": 11
    }
  ],
  "KeyedForeignKeyAssociateCollection": [
    {
      "Id": 12,
      "EntityId": 1
    },
    {
      "Id": 13,
      "EntityId": 1
    }
  ]
}
""";
                await AdHocCosmosTestHelpers.CreateCustomEntityHelperAsync(
                    container,
                    entity,
                    CancellationToken.None);
            });

        using var context = contextFactory.CreateDbContext();
        IQueryable<ContextBadPrimaryKeyJsonProperties.Entity> query = context.Entities;
        if (!tracking)
        {
            query = query.AsNoTracking();
        }

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => query.SingleAsync());
        Assert.Equal(CoreStrings.InvalidKeyValue("Entity.KeyedAssociateCollection#AssociateEntity", "Id"), ex.Message);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task KeyedAssociateCollection_null_foreign_key_doesnt_throw(bool tracking)
    {
        var contextFactory = await InitializeNonSharedTest<ContextBadPrimaryKeyJsonProperties>(
            onConfiguring: b => b.ConfigureWarnings(ConfigureWarnings),
            seed: async context =>
            {
                var client = context.Database.GetCosmosClient();
                var container = client.GetContainer(context.Database.GetCosmosDatabaseId(), containerId: "Entities");

                var entity = """
{
  "$type": "Entity",
  "Id": 1,
  "id": "1",
  "Associate": {
    "Id": 2
  },
  "KeyedAssociate": {},
  "AssociateCollection": [
    {
        "Id": 4
    },
    {
        "Id": 5
    }
  ],
  "KeyedAssociateCollection": [
    {
      "Id": 6,
      "EntityId": null
    },
    {
      "Id": 7,
      "EntityId": null
    }
  ],
  "ForeignKeyAssociate": {
    "Id": 8
  },
  "KeyedForeignKeyAssociate": {
    "Id": 9,
    "EntityId": 1
  },
  "ForeignKeyAssociateCollection": [
    {
      "Id": 10
    },
    {
      "Id": 11
    }
  ],
  "KeyedForeignKeyAssociateCollection": [
    {
      "Id": 12,
      "EntityId": 1
    },
    {
      "Id": 13,
      "EntityId": 1
    }
  ]
}
""";
                await AdHocCosmosTestHelpers.CreateCustomEntityHelperAsync(
                    container,
                    entity,
                    CancellationToken.None);
            });

        using var context = contextFactory.CreateDbContext();
        IQueryable<ContextBadPrimaryKeyJsonProperties.Entity> query = context.Entities;
        if (!tracking)
        {
            query = query.AsNoTracking();
        }

        var result = await query.SingleAsync();
        if (!tracking)
        {
            Assert.Equal(6, result.KeyedAssociateCollection.First().Id);
            Assert.Equal(7, result.KeyedAssociateCollection.Last().Id);
        }
        else
        {
            Assert.Empty(result.KeyedAssociateCollection);
        }
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task KeyedAssociateCollection_missing_foreign_key_doesnt_throw(bool tracking)
    {
        var contextFactory = await InitializeNonSharedTest<ContextBadPrimaryKeyJsonProperties>(
            onConfiguring: b => b.ConfigureWarnings(ConfigureWarnings),
            seed: async context =>
            {
                var client = context.Database.GetCosmosClient();
                var container = client.GetContainer(context.Database.GetCosmosDatabaseId(), containerId: "Entities");

                var entity = """
{
  "$type": "Entity",
  "Id": 1,
  "id": "1",
  "Associate": {
    "Id": 2
  },
  "KeyedAssociate": {},
  "AssociateCollection": [
    {
        "Id": 4
    },
    {
        "Id": 5
    }
  ],
  "KeyedAssociateCollection": [
    {
      "Id": 6
    },
    {
      "Id": 7
    }
  ],
  "ForeignKeyAssociate": {
    "Id": 8
  },
  "KeyedForeignKeyAssociate": {
    "Id": 9,
    "EntityId": 1
  },
  "ForeignKeyAssociateCollection": [
    {
      "Id": 10
    },
    {
      "Id": 11
    }
  ],
  "KeyedForeignKeyAssociateCollection": [
    {
      "Id": 12,
      "EntityId": 1
    },
    {
      "Id": 13,
      "EntityId": 1
    }
  ]
}
""";
                await AdHocCosmosTestHelpers.CreateCustomEntityHelperAsync(
                    container,
                    entity,
                    CancellationToken.None);
            });

        using var context = contextFactory.CreateDbContext();
        IQueryable<ContextBadPrimaryKeyJsonProperties.Entity> query = context.Entities;
        if (!tracking)
        {
            query = query.AsNoTracking();
        }

        var result = await query.SingleAsync();
        if (!tracking)
        {
            Assert.Equal(6, result.KeyedAssociateCollection.First().Id);
            Assert.Equal(7, result.KeyedAssociateCollection.Last().Id);
        }
        else
        {
            Assert.Empty(result.KeyedAssociateCollection);
        }
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task KeyedAssociateCollection_incorrect_foreign_key_doesnt_throw(bool tracking)
    {
        var contextFactory = await InitializeNonSharedTest<ContextBadPrimaryKeyJsonProperties>(
            onConfiguring: b => b.ConfigureWarnings(ConfigureWarnings),
            seed: async context =>
            {
                var client = context.Database.GetCosmosClient();
                var container = client.GetContainer(context.Database.GetCosmosDatabaseId(), containerId: "Entities");

                var entity = """
{
  "$type": "Entity",
  "Id": 1,
  "id": "1",
  "Associate": {
    "Id": 2
  },
  "KeyedAssociate": {},
  "AssociateCollection": [
    {
        "Id": 4
    },
    {
        "Id": 5
    }
  ],
  "KeyedAssociateCollection": [
    {
      "Id": 6,
      "EntityId": 99
    },
    {
      "Id": 7,
      "EntityId": 99
    }
  ],
  "ForeignKeyAssociate": {
    "Id": 8
  },
  "KeyedForeignKeyAssociate": {
    "Id": 9,
    "EntityId": 1
  },
  "ForeignKeyAssociateCollection": [
    {
      "Id": 10
    },
    {
      "Id": 11
    }
  ],
  "KeyedForeignKeyAssociateCollection": [
    {
      "Id": 12,
      "EntityId": 1
    },
    {
      "Id": 13,
      "EntityId": 1
    }
  ]
}
""";
                await AdHocCosmosTestHelpers.CreateCustomEntityHelperAsync(
                    container,
                    entity,
                    CancellationToken.None);
            });

        using var context = contextFactory.CreateDbContext();
        IQueryable<ContextBadPrimaryKeyJsonProperties.Entity> query = context.Entities;
        if (!tracking)
        {
            query = query.AsNoTracking();
        }

        var result = await query.SingleAsync();
        if (!tracking)
        {
            Assert.Equal(6, result.KeyedAssociateCollection.First().Id);
            Assert.Equal(7, result.KeyedAssociateCollection.Last().Id);
        }
        else
        {
            Assert.Empty(result.KeyedAssociateCollection);
        }
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task ForeignKeyAssociate_null_non_primary_key_doesnt_throw(bool tracking)
    {
        var contextFactory = await InitializeNonSharedTest<ContextBadPrimaryKeyJsonProperties>(
            onConfiguring: b => b.ConfigureWarnings(ConfigureWarnings),
            seed: async context =>
            {
                var client = context.Database.GetCosmosClient();
                var container = client.GetContainer(context.Database.GetCosmosDatabaseId(), containerId: "Entities");

                var entity = """
{
  "$type": "Entity",
  "Id": 1,
  "id": "1",
  "Associate": {
    "Id": 2
  },
  "KeyedAssociate": {},
  "AssociateCollection": [
    {
        "Id": 3
    },
    {
        "Id": 4
    }
  ],
  "KeyedAssociateCollection": [
    {
      "Id": 5,
      "EntityId": 1
    },
    {
      "Id": 6,
      "EntityId": 1
    }
  ],
  "ForeignKeyAssociate": {
    "Id": null
  },
  "KeyedForeignKeyAssociate": {
    "Id": 9,
    "EntityId": 1
  },
  "ForeignKeyAssociateCollection": [
    {
      "Id": 10
    },
    {
      "Id": 11
    }
  ],
  "KeyedForeignKeyAssociateCollection": [
    {
      "Id": 12,
      "EntityId": 1
    },
    {
      "Id": 13,
      "EntityId": 1
    }
  ]
}
""";
                await AdHocCosmosTestHelpers.CreateCustomEntityHelperAsync(
                    container,
                    entity,
                    CancellationToken.None);
            });

        using var context = contextFactory.CreateDbContext();
        IQueryable<ContextBadPrimaryKeyJsonProperties.Entity> query = context.Entities;
        if (!tracking)
        {
            query = query.AsNoTracking();
        }

        var result = await query.SingleAsync();
        Assert.Equal(0, result.ForeignKeyAssociate.Id);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task ForeignKeyAssociate_missing_non_primary_key_doesnt_throw(bool tracking)
    {
        var contextFactory = await InitializeNonSharedTest<ContextBadPrimaryKeyJsonProperties>(
            onConfiguring: b => b.ConfigureWarnings(ConfigureWarnings),
            seed: async context =>
            {
                var client = context.Database.GetCosmosClient();
                var container = client.GetContainer(context.Database.GetCosmosDatabaseId(), containerId: "Entities");

                var entity = """
{
  "$type": "Entity",
  "Id": 1,
  "id": "1",
  "Associate": {
    "Id": 2
  },
  "KeyedAssociate": {},
  "AssociateCollection": [
    {
        "Id": 3
    },
    {
        "Id": 4
    }
  ],
  "KeyedAssociateCollection": [
    {
      "Id": 5,
      "EntityId": 1
    },
    {
      "Id": 6,
      "EntityId": 1
    }
  ],
  "ForeignKeyAssociate": {
  },
  "KeyedForeignKeyAssociate": {
    "Id": 9,
    "EntityId": 1
  },
  "ForeignKeyAssociateCollection": [
    {
      "Id": 10
    },
    {
      "Id": 11
    }
  ],
  "KeyedForeignKeyAssociateCollection": [
    {
      "Id": 12,
      "EntityId": 1
    },
    {
      "Id": 13,
      "EntityId": 1
    }
  ]
}
""";
                await AdHocCosmosTestHelpers.CreateCustomEntityHelperAsync(
                    container,
                    entity,
                    CancellationToken.None);
            });

        using var context = contextFactory.CreateDbContext();

        IQueryable<ContextBadPrimaryKeyJsonProperties.Entity> query = context.Entities;
        if (!tracking)
        {
            query = query.AsNoTracking();
        }

        var result = await query.SingleAsync();
        Assert.Equal(0, result.ForeignKeyAssociate.Id);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task KeyedForeignKeyAssociate_null_primary_key_throws(bool tracking)
    {
        var contextFactory = await InitializeNonSharedTest<ContextBadPrimaryKeyJsonProperties>(
            onConfiguring: b => b.ConfigureWarnings(ConfigureWarnings),
            seed: async context =>
            {
                var client = context.Database.GetCosmosClient();
                var container = client.GetContainer(context.Database.GetCosmosDatabaseId(), containerId: "Entities");

                var entity = """
{
  "$type": "Entity",
  "Id": 1,
  "id": "1",
  "Associate": {
    "Id": 2
  },
  "KeyedAssociate": {},
  "AssociateCollection": [
    {
        "Id": 4
    },
    {
        "Id": 5
    }
  ],
  "KeyedAssociateCollection": [
    {
      "Id": 6,
      "EntityId": 1
    },
    {
      "Id": 7,
      "EntityId": 1
    }
  ],
  "ForeignKeyAssociate": {
    "Id": 8
  },
  "KeyedForeignKeyAssociate": {
    "Id": null,
    "EntityId": 1
  },
  "ForeignKeyAssociateCollection": [
    {
      "Id": 10
    },
    {
      "Id": 11
    }
  ],
  "KeyedForeignKeyAssociateCollection": [
    {
      "Id": 12,
      "EntityId": 1
    },
    {
      "Id": 13,
      "EntityId": 1
    }
  ]
}
""";
                await AdHocCosmosTestHelpers.CreateCustomEntityHelperAsync(
                    container,
                    entity,
                    CancellationToken.None);
            });

        using var context = contextFactory.CreateDbContext();
        IQueryable<ContextBadPrimaryKeyJsonProperties.Entity> query = context.Entities;
        if (!tracking)
        {
            query = query.AsNoTracking();
        }

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => query.SingleOrDefaultAsync());
        Assert.Equal(CoreStrings.InvalidKeyValue("Entity.KeyedForeignKeyAssociate#ForeignKeyAssociateEntity", "Id"), ex.Message);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task KeyedForeignKeyAssociate_missing_primary_key_throws(bool tracking)
    {
        var contextFactory = await InitializeNonSharedTest<ContextBadPrimaryKeyJsonProperties>(
            onConfiguring: b => b.ConfigureWarnings(ConfigureWarnings),
            seed: async context =>
            {
                var client = context.Database.GetCosmosClient();
                var container = client.GetContainer(context.Database.GetCosmosDatabaseId(), containerId: "Entities");

                var entity = """
{
  "$type": "Entity",
  "Id": 1,
  "id": "1",
  "Associate": {
    "Id": 2
  },
  "KeyedAssociate": {},
  "AssociateCollection": [
    {
        "Id": 4
    },
    {
        "Id": 5
    }
  ],
  "KeyedAssociateCollection": [
    {
      "Id": 6,
      "EntityId": 1
    },
    {
      "Id": 7,
      "EntityId": 1
    }
  ],
  "ForeignKeyAssociate": {
    "Id": 8
  },
  "KeyedForeignKeyAssociate": {
    "EntityId": 1
  },
  "ForeignKeyAssociateCollection": [
    {
      "Id": 10
    },
    {
      "Id": 11
    }
  ],
  "KeyedForeignKeyAssociateCollection": [
    {
      "Id": 12,
      "EntityId": 1
    },
    {
      "Id": 13,
      "EntityId": 1
    }
  ]
}
""";
                await AdHocCosmosTestHelpers.CreateCustomEntityHelperAsync(
                    container,
                    entity,
                    CancellationToken.None);
            });

        using var context = contextFactory.CreateDbContext();
        IQueryable<ContextBadPrimaryKeyJsonProperties.Entity> query = context.Entities;
        if (!tracking)
        {
            query = query.AsNoTracking();
        }

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => query.SingleOrDefaultAsync());
        Assert.Equal(CoreStrings.InvalidKeyValue("Entity.KeyedForeignKeyAssociate#ForeignKeyAssociateEntity", "Id"), ex.Message);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task KeyedForeignKeyAssociate_null_foreign_key_doesnt_throw(bool tracking)
    {
        var contextFactory = await InitializeNonSharedTest<ContextBadPrimaryKeyJsonProperties>(
            onConfiguring: b => b.ConfigureWarnings(ConfigureWarnings),
            seed: async context =>
            {
                var client = context.Database.GetCosmosClient();
                var container = client.GetContainer(context.Database.GetCosmosDatabaseId(), containerId: "Entities");

                var entity = """
{
  "$type": "Entity",
  "Id": 1,
  "id": "1",
  "Associate": {
    "Id": 2
  },
  "KeyedAssociate": {},
  "AssociateCollection": [
    {
        "Id": 4
    },
    {
        "Id": 5
    }
  ],
  "KeyedAssociateCollection": [
    {
      "Id": 6,
      "EntityId": 1
    },
    {
      "Id": 7,
      "EntityId": 1
    }
  ],
  "ForeignKeyAssociate": {
    "Id": 8
  },
  "KeyedForeignKeyAssociate": {
    "Id": 9,
    "EntityId": null
  },
  "ForeignKeyAssociateCollection": [
    {
      "Id": 10
    },
    {
      "Id": 11
    }
  ],
  "KeyedForeignKeyAssociateCollection": [
    {
      "Id": 12,
      "EntityId": 1
    },
    {
      "Id": 13,
      "EntityId": 1
    }
  ]
}
""";
                await AdHocCosmosTestHelpers.CreateCustomEntityHelperAsync(
                    container,
                    entity,
                    CancellationToken.None);
            });

        using var context = contextFactory.CreateDbContext();
        IQueryable<ContextBadPrimaryKeyJsonProperties.Entity> query = context.Entities;
        if (!tracking)
        {
            query = query.AsNoTracking();
        }

        var result = await query.SingleAsync();
        if (tracking)
        {
            Assert.Null(result.KeyedForeignKeyAssociate);
        }
        else
        {
            Assert.NotNull(result.KeyedForeignKeyAssociate);
            Assert.Equal(9, result.KeyedForeignKeyAssociate.Id);
            Assert.Equal(0, result.KeyedForeignKeyAssociate.EntityId);
        }
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task KeyedForeignKeyAssociate_missing_foreign_key_doesnt_throw(bool tracking)
    {
        var contextFactory = await InitializeNonSharedTest<ContextBadPrimaryKeyJsonProperties>(
            onConfiguring: b => b.ConfigureWarnings(ConfigureWarnings),
            seed: async context =>
            {
                var client = context.Database.GetCosmosClient();
                var container = client.GetContainer(context.Database.GetCosmosDatabaseId(), containerId: "Entities");

                var entity = """
{
  "$type": "Entity",
  "Id": 1,
  "id": "1",
  "Associate": {
    "Id": 2
  },
  "KeyedAssociate": {},
  "AssociateCollection": [
    {
        "Id": 4
    },
    {
        "Id": 5
    }
  ],
  "KeyedAssociateCollection": [
    {
      "Id": 6,
      "EntityId": 1
    },
    {
      "Id": 7,
      "EntityId": 1
    }
  ],
  "ForeignKeyAssociate": {
    "Id": 8
  },
  "KeyedForeignKeyAssociate": {
    "Id": 9,
    "EntityId": null
  },
  "ForeignKeyAssociateCollection": [
    {
      "Id": 10
    },
    {
      "Id": 11
    }
  ],
  "KeyedForeignKeyAssociateCollection": [
    {
      "Id": 12,
      "EntityId": 1
    },
    {
      "Id": 13,
      "EntityId": 1
    }
  ]
}
""";
                await AdHocCosmosTestHelpers.CreateCustomEntityHelperAsync(
                    container,
                    entity,
                    CancellationToken.None);
            });

        using var context = contextFactory.CreateDbContext();
        IQueryable<ContextBadPrimaryKeyJsonProperties.Entity> query = context.Entities;
        if (!tracking)
        {
            query = query.AsNoTracking();
        }

        var result = await query.SingleAsync();
        if (tracking)
        {
            Assert.Null(result.KeyedForeignKeyAssociate);
        }
        else
        {
            Assert.NotNull(result.KeyedForeignKeyAssociate);
            Assert.Equal(9, result.KeyedForeignKeyAssociate.Id);
            Assert.Equal(0, result.KeyedForeignKeyAssociate.EntityId);
        }
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task KeyedForeignKeyAssociate_incorrect_foreign_key_doesnt_throw(bool tracking)
    {
        var contextFactory = await InitializeNonSharedTest<ContextBadPrimaryKeyJsonProperties>(
            onConfiguring: b => b.ConfigureWarnings(ConfigureWarnings),
            seed: async context =>
            {
                var client = context.Database.GetCosmosClient();
                var container = client.GetContainer(context.Database.GetCosmosDatabaseId(), containerId: "Entities");

                var entity = """
{
  "$type": "Entity",
  "Id": 1,
  "id": "1",
  "Associate": {
    "Id": 2
  },
  "KeyedAssociate": {},
  "AssociateCollection": [
    {
        "Id": 4
    },
    {
        "Id": 5
    }
  ],
  "KeyedAssociateCollection": [
    {
      "Id": 6,
      "EntityId": 1
    },
    {
      "Id": 7,
      "EntityId": 1
    }
  ],
  "ForeignKeyAssociate": {
    "Id": 8
  },
  "KeyedForeignKeyAssociate": {
    "Id": 9,
    "EntityId": 99
  },
  "ForeignKeyAssociateCollection": [
    {
      "Id": 10
    },
    {
      "Id": 11
    }
  ],
  "KeyedForeignKeyAssociateCollection": [
    {
      "Id": 12,
      "EntityId": 1
    },
    {
      "Id": 13,
      "EntityId": 1
    }
  ]
}
""";
                await AdHocCosmosTestHelpers.CreateCustomEntityHelperAsync(
                    container,
                    entity,
                    CancellationToken.None);
            });

        using var context = contextFactory.CreateDbContext();
        IQueryable<ContextBadPrimaryKeyJsonProperties.Entity> query = context.Entities;
        if (!tracking)
        {
            query = query.AsNoTracking();
        }

        var result = await query.SingleAsync();
        if (tracking)
        {
            Assert.Null(result.KeyedForeignKeyAssociate);
        }
        else
        {
            Assert.NotNull(result.KeyedForeignKeyAssociate);
            Assert.Equal(9, result.KeyedForeignKeyAssociate.Id);
            Assert.Equal(99, result.KeyedForeignKeyAssociate.EntityId);
        }
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task ForeignKeyAssociateCollection_null_primary_key_throws(bool tracking)
    {
        var contextFactory = await InitializeNonSharedTest<ContextBadPrimaryKeyJsonProperties>(
            onConfiguring: b => b.ConfigureWarnings(ConfigureWarnings),
            seed: async context =>
            {
                var client = context.Database.GetCosmosClient();
                var container = client.GetContainer(context.Database.GetCosmosDatabaseId(), containerId: "Entities");

                var entity = """
{
  "$type": "Entity",
  "Id": 1,
  "id": "1",
  "Associate": {
    "Id": 2
  },
  "KeyedAssociate": {},
  "AssociateCollection": [
    {
        "Id": 4
    },
    {
        "Id": 5
    }
  ],
  "KeyedAssociateCollection": [
    {
      "Id": 6,
      "EntityId": 1
    },
    {
      "Id": 7,
      "EntityId": 1
    }
  ],
  "ForeignKeyAssociate": {
    "Id": 8
  },
  "KeyedForeignKeyAssociate": {
    "Id": 9,
    "EntityId": 1
  },
  "ForeignKeyAssociateCollection": [
    {
      "Id": null
    },
    {
      "Id": null
    }
  ],
  "KeyedForeignKeyAssociateCollection": [
    {
      "Id": 12,
      "EntityId": 1
    },
    {
      "Id": 13,
      "EntityId": 1
    }
  ]
}
""";
                await AdHocCosmosTestHelpers.CreateCustomEntityHelperAsync(
                    container,
                    entity,
                    CancellationToken.None);
            });

        using var context = contextFactory.CreateDbContext();
        IQueryable<ContextBadPrimaryKeyJsonProperties.Entity> query = context.Entities;
        if (!tracking)
        {
            query = query.AsNoTracking();
        }

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => query.SingleOrDefaultAsync());
        Assert.Equal(CoreStrings.InvalidKeyValue("Entity.ForeignKeyAssociateCollection#ForeignKeyAssociateEntity", "Id"), ex.Message);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task ForeignKeyAssociateCollection_missing_primary_key_doesnt_throw_and_uses_ordinal_workaround(bool tracking)
    {
        var contextFactory = await InitializeNonSharedTest<ContextBadPrimaryKeyJsonProperties>(
            onConfiguring: b => b.ConfigureWarnings(ConfigureWarnings),
            seed: async context =>
            {
                var client = context.Database.GetCosmosClient();
                var container = client.GetContainer(context.Database.GetCosmosDatabaseId(), containerId: "Entities");

                var entity = """
{
  "$type": "Entity",
  "Id": 1,
  "id": "1",
  "Associate": {
    "Id": 2
  },
  "KeyedAssociate": {},
  "AssociateCollection": [
    {
        "Id": 4
    },
    {
        "Id": 5
    }
  ],
  "KeyedAssociateCollection": [
    {
      "Id": 6,
      "EntityId": 1
    },
    {
      "Id": 7,
      "EntityId": 1
    }
  ],
  "ForeignKeyAssociate": {
    "Id": 8
  },
  "KeyedForeignKeyAssociate": {
    "Id": 9,
    "EntityId": 1
  },
  "ForeignKeyAssociateCollection": [
    {
    },
    {
    }
  ],
  "KeyedForeignKeyAssociateCollection": [
    {
      "Id": 12,
      "EntityId": 1
    },
    {
      "Id": 13,
      "EntityId": 1
    }
  ]
}
""";
                await AdHocCosmosTestHelpers.CreateCustomEntityHelperAsync(
                    container,
                    entity,
                    CancellationToken.None);
            });

        using var context = contextFactory.CreateDbContext();
        IQueryable<ContextBadPrimaryKeyJsonProperties.Entity> query = context.Entities;
        if (!tracking)
        {
            query = query.AsNoTracking();
        }

        var result = await query.SingleAsync();
        Assert.Equal(1, result.ForeignKeyAssociateCollection.First().Id);
        Assert.Equal(2, result.ForeignKeyAssociateCollection.Last().Id);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task KeyedForeignKeyAssociateCollection_null_primary_key_throws(bool tracking)
    {
        var contextFactory = await InitializeNonSharedTest<ContextBadPrimaryKeyJsonProperties>(
            onConfiguring: b => b.ConfigureWarnings(ConfigureWarnings),
            seed: async context =>
            {
                var client = context.Database.GetCosmosClient();
                var container = client.GetContainer(context.Database.GetCosmosDatabaseId(), containerId: "Entities");

                var entity = """
{
  "$type": "Entity",
  "Id": 1,
  "id": "1",
  "Associate": {
    "Id": 2
  },
  "KeyedAssociate": {},
  "AssociateCollection": [
    {
        "Id": 4
    },
    {
        "Id": 5
    }
  ],
  "KeyedAssociateCollection": [
    {
      "Id": 6,
      "EntityId": 1
    },
    {
      "Id": 7,
      "EntityId": 1
    }
  ],
  "ForeignKeyAssociate": {
    "Id": 8
  },
  "KeyedForeignKeyAssociate": {
    "Id": 9,
    "EntityId": 1
  },
  "ForeignKeyAssociateCollection": [
    {
      "Id": 10
    },
    {
      "Id": 11
    }
  ],
  "KeyedForeignKeyAssociateCollection": [
    {
      "Id": null,
      "EntityId": 1
    },
    {
      "Id": null,
      "EntityId": 1
    }
  ]
}
""";
                await AdHocCosmosTestHelpers.CreateCustomEntityHelperAsync(
                    container,
                    entity,
                    CancellationToken.None);
            });

        using var context = contextFactory.CreateDbContext();
        IQueryable<ContextBadPrimaryKeyJsonProperties.Entity> query = context.Entities;
        if (!tracking)
        {
            query = query.AsNoTracking();
        }

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => query.SingleOrDefaultAsync());
        Assert.Equal(CoreStrings.InvalidKeyValue("Entity.KeyedForeignKeyAssociateCollection#ForeignKeyAssociateEntity", "Id"), ex.Message);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task KeyedForeignKeyAssociateCollection_missing_primary_key_throws(bool tracking)
    {
        var contextFactory = await InitializeNonSharedTest<ContextBadPrimaryKeyJsonProperties>(
            onConfiguring: b => b.ConfigureWarnings(ConfigureWarnings),
            seed: async context =>
            {
                var client = context.Database.GetCosmosClient();
                var container = client.GetContainer(context.Database.GetCosmosDatabaseId(), containerId: "Entities");

                var entity = """
{
  "$type": "Entity",
  "Id": 1,
  "id": "1",
  "Associate": {
    "Id": 2
  },
  "KeyedAssociate": {},
  "AssociateCollection": [
    {
        "Id": 4
    },
    {
        "Id": 5
    }
  ],
  "KeyedAssociateCollection": [
    {
      "Id": 6,
      "EntityId": 1
    },
    {
      "Id": 7,
      "EntityId": 1
    }
  ],
  "ForeignKeyAssociate": {
    "Id": 8
  },
  "KeyedForeignKeyAssociate": {
    "Id": 9,
    "EntityId": 1
  },
  "ForeignKeyAssociateCollection": [
    {
      "Id": 10
    },
    {
      "Id": 11
    }
  ],
  "KeyedForeignKeyAssociateCollection": [
    {
      "EntityId": 1
    },
    {
      "EntityId": 1
    }
  ]
}
""";
                await AdHocCosmosTestHelpers.CreateCustomEntityHelperAsync(
                    container,
                    entity,
                    CancellationToken.None);
            });

        using var context = contextFactory.CreateDbContext();
        IQueryable<ContextBadPrimaryKeyJsonProperties.Entity> query = context.Entities;
        if (!tracking)
        {
            query = query.AsNoTracking();
        }

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => query.SingleOrDefaultAsync());
        Assert.Equal(CoreStrings.InvalidKeyValue("Entity.KeyedForeignKeyAssociateCollection#ForeignKeyAssociateEntity", "Id"), ex.Message);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task KeyedForeignKeyAssociateCollection_null_foreign_key_doesnt_throw(bool tracking)
    {
        var contextFactory = await InitializeNonSharedTest<ContextBadPrimaryKeyJsonProperties>(
            onConfiguring: b => b.ConfigureWarnings(ConfigureWarnings),
            seed: async context =>
            {
                var client = context.Database.GetCosmosClient();
                var container = client.GetContainer(context.Database.GetCosmosDatabaseId(), containerId: "Entities");

                var entity = """
{
  "$type": "Entity",
  "Id": 1,
  "id": "1",
  "Associate": {
    "Id": 2
  },
  "KeyedAssociate": {},
  "AssociateCollection": [
    {
        "Id": 4
    },
    {
        "Id": 5
    }
  ],
  "KeyedAssociateCollection": [
    {
      "Id": 6,
      "EntityId": 1
    },
    {
      "Id": 7,
      "EntityId": 1
    }
  ],
  "ForeignKeyAssociate": {
    "Id": 8
  },
  "KeyedForeignKeyAssociate": {
    "Id": 9,
    "EntityId": 1
  },
  "ForeignKeyAssociateCollection": [
    {
      "Id": 10
    },
    {
      "Id": 11
    }
  ],
  "KeyedForeignKeyAssociateCollection": [
    {
      "Id": 12,
      "EntityId": null
    },
    {
      "Id": 13,
      "EntityId": null
    }
  ]
}
""";
                await AdHocCosmosTestHelpers.CreateCustomEntityHelperAsync(
                    container,
                    entity,
                    CancellationToken.None);
            });

        using var context = contextFactory.CreateDbContext();
        IQueryable<ContextBadPrimaryKeyJsonProperties.Entity> query = context.Entities;
        if (!tracking)
        {
            query = query.AsNoTracking();
        }

        var result = await query.SingleAsync();
        if (tracking)
        {
            Assert.Empty(result.KeyedForeignKeyAssociateCollection);
        }
        else
        {
            foreach (var item in result.KeyedForeignKeyAssociateCollection)
            {
                Assert.Equal(0, item.EntityId);
            }
        }
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task KeyedForeignKeyAssociateCollection_missing_foreign_key_doesnt_throw(bool tracking)
    {
        var contextFactory = await InitializeNonSharedTest<ContextBadPrimaryKeyJsonProperties>(
            onConfiguring: b => b.ConfigureWarnings(ConfigureWarnings),
            seed: async context =>
            {
                var client = context.Database.GetCosmosClient();
                var container = client.GetContainer(context.Database.GetCosmosDatabaseId(), containerId: "Entities");

                var entity = """
{
  "$type": "Entity",
  "Id": 1,
  "id": "1",
  "Associate": {
    "Id": 2
  },
  "KeyedAssociate": {},
  "AssociateCollection": [
    {
        "Id": 4
    },
    {
        "Id": 5
    }
  ],
  "KeyedAssociateCollection": [
    {
      "Id": 6,
      "EntityId": 1
    },
    {
      "Id": 7,
      "EntityId": 1
    }
  ],
  "ForeignKeyAssociate": {
    "Id": 8
  },
  "KeyedForeignKeyAssociate": {
    "Id": 9,
    "EntityId": 1
  },
  "ForeignKeyAssociateCollection": [
    {
      "Id": 10
    },
    {
      "Id": 11
    }
  ],
  "KeyedForeignKeyAssociateCollection": [
    {
      "Id": 12
    },
    {
      "Id": 13
    }
  ]
}
""";
                await AdHocCosmosTestHelpers.CreateCustomEntityHelperAsync(
                    container,
                    entity,
                    CancellationToken.None);
            });

        using var context = contextFactory.CreateDbContext();
        IQueryable<ContextBadPrimaryKeyJsonProperties.Entity> query = context.Entities;
        if (!tracking)
        {
            query = query.AsNoTracking();
        }

        var result = await query.SingleAsync();
        if (tracking)
        {
            Assert.Empty(result.KeyedForeignKeyAssociateCollection);
        }
        else
        {
            foreach (var item in result.KeyedForeignKeyAssociateCollection)
            {
                Assert.Equal(0, item.EntityId);
            }
        }
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task KeyedForeignKeyAssociateCollection_incorrect_foreign_key_doesnt_throw(bool tracking)
    {
        var contextFactory = await InitializeNonSharedTest<ContextBadPrimaryKeyJsonProperties>(
            onConfiguring: b => b.ConfigureWarnings(ConfigureWarnings),
            seed: async context =>
            {
                var client = context.Database.GetCosmosClient();
                var container = client.GetContainer(context.Database.GetCosmosDatabaseId(), containerId: "Entities");

                var entity = """
{
  "$type": "Entity",
  "Id": 1,
  "id": "1",
  "Associate": {
    "Id": 2
  },
  "KeyedAssociate": {},
  "AssociateCollection": [
    {
        "Id": 4
    },
    {
        "Id": 5
    }
  ],
  "KeyedAssociateCollection": [
    {
      "Id": 6,
      "EntityId": 1
    },
    {
      "Id": 7,
      "EntityId": 1
    }
  ],
  "ForeignKeyAssociate": {
    "Id": 8
  },
  "KeyedForeignKeyAssociate": {
    "Id": 9,
    "EntityId": 1
  },
  "ForeignKeyAssociateCollection": [
    {
      "Id": 10
    },
    {
      "Id": 11
    }
  ],
  "KeyedForeignKeyAssociateCollection": [
    {
      "Id": 12,
      "EntityId": 99
    },
    {
      "Id": 13,
      "EntityId": 99
    }
  ]
}
""";
                await AdHocCosmosTestHelpers.CreateCustomEntityHelperAsync(
                    container,
                    entity,
                    CancellationToken.None);
            });

        using var context = contextFactory.CreateDbContext();
        IQueryable<ContextBadPrimaryKeyJsonProperties.Entity> query = context.Entities;
        if (!tracking)
        {
            query = query.AsNoTracking();
        }

        var result = await query.SingleAsync();
        if (tracking)
        {
            Assert.Empty(result.KeyedForeignKeyAssociateCollection);
        }
        else
        {
            foreach (var item in result.KeyedForeignKeyAssociateCollection)
            {
                Assert.Equal(99, item.EntityId);
            }
        }
    }

    private class ContextBadPrimaryKeyJsonProperties : DbContext
    {
        public ContextBadPrimaryKeyJsonProperties(DbContextOptions options)
            : base(options)
        {
        }

        public DbSet<Entity> Entities { get; set; } = null!;

        public class Entity
        {
            public int Id { get; set; }

            public AssociateEntity Associate { get; set; } = null!;
            public AssociateEntity KeyedAssociate { get; set; } = null!;

            public List<AssociateEntity> AssociateCollection { get; set; } = new();
            public List<AssociateEntity> KeyedAssociateCollection { get; set; } = new();

            public ForeignKeyAssociateEntity ForeignKeyAssociate { get; set; } = null!;
            public ForeignKeyAssociateEntity KeyedForeignKeyAssociate { get; set; } = null!;

            public List<ForeignKeyAssociateEntity> ForeignKeyAssociateCollection { get; set; } = new();
            public List<ForeignKeyAssociateEntity> KeyedForeignKeyAssociateCollection { get; set; } = new();
        }

        public class AssociateEntity
        {
            public int Id { get; set; }
        }

        public class ForeignKeyAssociateEntity
        {
            public int Id { get; set; }

            public int EntityId { get; set; }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var entity = modelBuilder.Entity<Entity>();
            entity.ToContainer("Entities");
            entity.OwnsOne(x => x.Associate);
            entity.OwnsOne(x => x.KeyedAssociate, b => b.HasKey(x => x.Id));
            entity.OwnsMany(x => x.AssociateCollection);
            entity.OwnsMany(x => x.KeyedAssociateCollection, b => b.HasKey(x => x.Id));

            entity.OwnsOne(x => x.ForeignKeyAssociate);
            entity.OwnsOne(x => x.KeyedForeignKeyAssociate, b => b.HasKey(x => x.Id));
            entity.OwnsMany(x => x.ForeignKeyAssociateCollection);
            entity.OwnsMany(x => x.KeyedForeignKeyAssociateCollection, b => b.HasKey(x => x.Id));
        }
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

    protected override DbContextOptionsBuilder AddNonSharedOptions(DbContextOptionsBuilder builder)
        => builder.ConfigureWarnings(b => b.Ignore(CosmosEventId.NoPartitionKeyDefined));

    protected override ITestStoreFactory NonSharedTestStoreFactory
        => CosmosTestStoreFactory.Instance;
}
