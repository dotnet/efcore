// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.GearsOfWarModel;

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public abstract class GearsOfWarQueryRelationalFixture : GearsOfWarQueryFixtureBase, ITestSqlLoggerFactory
{
    public override Dictionary<(Type, string), Func<object, object>> GetShadowPropertyMappings()
    {
        var discriminatorMapping = new Dictionary<(Type, string), Func<object, object>>
        {
            {
                (typeof(Gear), "Discriminator"), e =>
                {
                    switch (((Gear)e)?.Nickname)
                    {
                        case "Baird":
                        case "Marcus":
                            return "Officer";

                        case "Cole Train":
                        case "Dom":
                        case "Paduk":
                            return "Gear";

                        default:
                            return null;
                    }
                }
            },
            {
                (typeof(Faction), "Discriminator"), e =>
                {
                    switch (((Faction)e)?.Id)
                    {
                        case 1:
                        case 2:
                            return "LocustHorde";

                        default:
                            return null;
                    }
                }
            },
            {
                (typeof(LocustLeader), "Discriminator"), e =>
                {
                    switch (((LocustLeader)e)?.Name)
                    {
                        case "General Karn":
                        case "General RAAM":
                        case "High Priest Skorge":
                        case "The Speaker":
                            return "LocustLeader";

                        case "Queen Myrrah":
                        case "Unknown":
                            return "LocustCommander";

                        default:
                            return null;
                    }
                }
            },
        };

        foreach (var shadowPropertyMappingElement in base.GetShadowPropertyMappings())
        {
            discriminatorMapping.Add(shadowPropertyMappingElement.Key, shadowPropertyMappingElement.Value);
        }

        return discriminatorMapping;
    }

    public new RelationalTestStore TestStore
        => (RelationalTestStore)base.TestStore;

    public TestSqlLoggerFactory TestSqlLoggerFactory
        => (TestSqlLoggerFactory)ListLoggerFactory;

    protected override bool ShouldLogCategory(string logCategory)
        => logCategory == DbLoggerCategory.Query.Name;
}
