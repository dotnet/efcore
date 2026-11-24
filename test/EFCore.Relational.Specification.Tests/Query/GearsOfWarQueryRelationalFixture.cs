// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.GearsOfWarModel;

namespace Microsoft.EntityFrameworkCore.Query;

public abstract class GearsOfWarQueryRelationalFixture : GearsOfWarQueryFixtureBase, ITestSqlLoggerFactory
{
    public override Dictionary<(Type, string), Func<object, object?>> GetShadowPropertyMappings()
    {
        var discriminatorMapping = new Dictionary<(Type, string), Func<object, object?>>
        {
            {
                (typeof(Gear), "Discriminator"), e => (((Gear)e)?.Nickname)switch
                {
                    "Baird" or "Marcus" => "Officer", "Cole Train" or "Dom" or "Paduk" => "Gear", _ => null,
                }
            },
            {
                (typeof(Faction), "Discriminator"), e => (((Faction)e)?.Id)switch
                {
                    1 or 2 or 3 => "LocustHorde", _ => null,
                }
            },
            {
                (typeof(LocustLeader), "Discriminator"), e => (((LocustLeader)e)?.Name)switch
                {
                    "General Karn" or "General RAAM" or "High Priest Skorge" or "The Speaker" => "LocustLeader",
                    "Queen Myrrah" or "Unknown" or "Reyna Diaz" => "LocustCommander", _ => null,
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
