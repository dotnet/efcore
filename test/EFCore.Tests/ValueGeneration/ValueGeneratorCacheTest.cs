// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.ValueGeneration;

public class ValueGeneratorCacheTest
{
    [ConditionalFact]
    public void Uses_single_generator_per_property()
    {
        var model = CreateModel();
        var entityType = model.FindEntityType("Led");
        var property1 = entityType.FindProperty("Zeppelin");
        var property2 = entityType.FindProperty("Stairway");
        var cache = InMemoryTestHelpers.Instance.CreateContextServices(model)
            .GetRequiredService<IValueGeneratorCache>();

        var generator1 = cache.GetOrAdd(property1, entityType, (p, et) => new GuidValueGenerator());
        Assert.NotNull(generator1);
        Assert.Same(generator1, cache.GetOrAdd(property1, entityType, (p, et) => new GuidValueGenerator()));

        var generator2 = cache.GetOrAdd(property2, entityType, (p, et) => new GuidValueGenerator());
        Assert.NotNull(generator2);
        Assert.Same(generator2, cache.GetOrAdd(property2, entityType, (p, et) => new GuidValueGenerator()));
        Assert.NotSame(generator1, generator2);
    }

    private static IModel CreateModel(bool generateValues = true)
    {
        var modelBuilder = InMemoryTestHelpers.Instance.CreateConventionBuilder();
        modelBuilder.Entity(
            "Led", eb =>
            {
                eb.Property<int>("Id");
                eb.Property<Guid>("Zeppelin");
                var property = eb.Property<Guid>("Stairway");
                if (generateValues)
                {
                    property.ValueGeneratedOnAdd();
                }
            });

        return modelBuilder.FinalizeModel();
    }
}
