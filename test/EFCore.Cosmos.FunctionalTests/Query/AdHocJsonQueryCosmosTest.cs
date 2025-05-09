// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Cosmos.Storage.Internal;

namespace Microsoft.EntityFrameworkCore.Query;

public class AdHocJsonQueryCosmosTest : AdHocJsonQueryTestBase
{
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
        var entitiesContainer = singletonWrapper.Client.GetContainer(TestStore.Name, containerId: "Entities");

        var missingTopLevel =
$$"""
{
  "Id": 2,
  "$type": "Entity",
  "Name": "e2",
  "id": "2",
  "Collection": [
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
    "NestedCollection": [
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
    "NestedCollection": [
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
  "Collection": [
    {
      "Number": 7.0,
      "Text": "e3 c1",
      "NestedCollection": [
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
      "NestedCollection": [
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
    "NestedCollection": [
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
    "NestedCollection": [
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
  "Collection": [
    {
      "Number": null,
      "Text": "e4 c1",
      "NestedCollection": [
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
      "NestedCollection": [
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
    "NestedCollection": [
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
    "NestedCollection": [
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
  "Collection": [
    {
      "Number": 7.0,
      "Text": "e5 c1",
      "NestedCollection": [
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
      "NestedCollection": [
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
    "NestedCollection": [
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
    "NestedCollection": [
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
  "Collection": [
    {
      "Number": 7.0,
      "Text": "e6 c1",
      "NestedCollection": [
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
      "NestedCollection": [
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
    "NestedCollection": [
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
    "NestedCollection": [
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
