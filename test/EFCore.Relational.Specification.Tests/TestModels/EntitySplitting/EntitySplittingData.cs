// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.EntitySplitting;

#nullable disable

public class EntitySplittingData : ISetSource
{
    public static readonly EntitySplittingData Instance = new();

    private readonly EntityOne[] _entityOnes;
    private readonly EntityTwo[] _entityTwos;
    private readonly EntityThree[] _entityThrees;
    private readonly BaseEntity[] _baseEntities;

    private EntitySplittingData()
    {
        _entityOnes = CreateEntityOnes();
        _entityTwos = CreateEntityTwos();
        _entityThrees = CreateEntityThrees();
        _baseEntities = CreateHierarchyEntities();

        WireUp();
    }

    public IQueryable<TEntity> Set<TEntity>()
        where TEntity : class
    {
        if (typeof(TEntity) == typeof(EntityOne))
        {
            return (IQueryable<TEntity>)_entityOnes.AsQueryable();
        }

        if (typeof(TEntity) == typeof(EntityTwo))
        {
            return (IQueryable<TEntity>)_entityTwos.AsQueryable();
        }

        if (typeof(TEntity) == typeof(EntityThree))
        {
            return (IQueryable<TEntity>)_entityThrees.AsQueryable();
        }

        if (typeof(TEntity) == typeof(BaseEntity))
        {
            return (IQueryable<TEntity>)_baseEntities.AsQueryable();
        }

        if (typeof(TEntity) == typeof(MiddleEntity))
        {
            return (IQueryable<TEntity>)_baseEntities.OfType<MiddleEntity>().AsQueryable();
        }

        if (typeof(TEntity) == typeof(SiblingEntity))
        {
            return (IQueryable<TEntity>)_baseEntities.OfType<SiblingEntity>().AsQueryable();
        }

        if (typeof(TEntity) == typeof(LeafEntity))
        {
            return (IQueryable<TEntity>)_baseEntities.OfType<LeafEntity>().AsQueryable();
        }

        throw new InvalidOperationException("Invalid entity type: " + typeof(TEntity));
    }

    private static EntityOne[] CreateEntityOnes()
        =>
        [
            new()
            {
                Id = 1,
                IntValue1 = 11,
                IntValue2 = 12,
                IntValue3 = 13,
                IntValue4 = 14,
                StringValue1 = "V11",
                StringValue2 = "V12",
                StringValue3 = "V13",
                StringValue4 = "V14"
            },
            new()
            {
                Id = 2,
                IntValue1 = 21,
                IntValue2 = 22,
                IntValue3 = 23,
                IntValue4 = 24,
                StringValue1 = "V21",
                StringValue2 = "V22",
                StringValue3 = "V23",
                StringValue4 = "V24"
            },
            new()
            {
                Id = 3,
                IntValue1 = 31,
                IntValue2 = 32,
                IntValue3 = 33,
                IntValue4 = 34,
                StringValue1 = "V31",
                StringValue2 = "V32",
                StringValue3 = "V33",
                StringValue4 = "V34"
            },
            new()
            {
                Id = 4,
                IntValue1 = 41,
                IntValue2 = 42,
                IntValue3 = 43,
                IntValue4 = 44,
                StringValue1 = "V41",
                StringValue2 = "V42",
                StringValue3 = "V43",
                StringValue4 = "V44"
            },
            new()
            {
                Id = 5,
                IntValue1 = 51,
                IntValue2 = 52,
                IntValue3 = 53,
                IntValue4 = 54,
                StringValue1 = "V51",
                StringValue2 = "V52",
                StringValue3 = "V53",
                StringValue4 = "V54"
            }
        ];

    private static EntityTwo[] CreateEntityTwos()
        =>
        [
            new() { Id = 1, Name = "Two1" },
            new() { Id = 2, Name = "Two2" },
            new() { Id = 3, Name = "Two3" },
            new() { Id = 4, Name = "Two4" },
            new() { Id = 5, Name = "Two5" }
        ];

    private static EntityThree[] CreateEntityThrees()
        =>
        [
            new() { Id = 1, Name = "Three1" },
            new() { Id = 2, Name = "Three2" },
            new() { Id = 3, Name = "Three3" },
            new() { Id = 4, Name = "Three4" },
            new() { Id = 5, Name = "Three5" }
        ];

    private static BaseEntity[] CreateHierarchyEntities()
        =>
        [
            new() { Id = 1, BaseValue = 1 },
            new MiddleEntity
            {
                Id = 2,
                BaseValue = 2,
                MiddleValue = 21
            },
            new SiblingEntity
            {
                Id = 3,
                BaseValue = 3,
                SiblingValue = 21
            },
            new LeafEntity
            {
                Id = 4,
                BaseValue = 4,
                MiddleValue = 22,
                LeafValue = 301
            }
        ];

    private void WireUp()
    {
        _entityTwos[0].EntityOne = _entityOnes[0];
        _entityTwos[1].EntityOne = _entityOnes[0];
        _entityTwos[2].EntityOne = _entityOnes[1];
        _entityTwos[3].EntityOne = _entityOnes[2];
        _entityTwos[4].EntityOne = _entityOnes[2];

        _entityOnes[0].EntityThree = _entityThrees[0];
        _entityOnes[1].EntityThree = _entityThrees[0];
        _entityOnes[2].EntityThree = _entityThrees[1];
        _entityOnes[3].EntityThree = _entityThrees[2];
        _entityOnes[4].EntityThree = _entityThrees[2];

        for (var i = 0; i < _entityOnes.Length; i++)
        {
            _entityOnes[i].OwnedReference = new OwnedReference
            {
                OwnedIntValue1 = i * 10 + 1,
                OwnedIntValue2 = i * 10 + 2,
                OwnedIntValue3 = i * 10 + 3,
                OwnedIntValue4 = i * 10 + 4,
                OwnedStringValue1 = "O" + i + "1",
                OwnedStringValue2 = "O" + i + "2",
                OwnedStringValue3 = "O" + i + "3",
                OwnedStringValue4 = "O" + i + "4",
                OwnedNestedReference = new OwnedNestedReference
                {
                    OwnedNestedIntValue1 = i * 100 + 1,
                    OwnedNestedIntValue2 = i * 100 + 2,
                    OwnedNestedIntValue3 = i * 100 + 3,
                    OwnedNestedIntValue4 = i * 100 + 4,
                    OwnedNestedStringValue1 = "ON" + i + "1",
                    OwnedNestedStringValue2 = "ON" + i + "2",
                    OwnedNestedStringValue3 = "ON" + i + "3",
                    OwnedNestedStringValue4 = "ON" + i + "4"
                }
            };

            for (var j = 0; j < i; j++)
            {
                _entityOnes[i].OwnedCollection.Add(
                    new OwnedCollection
                    {
                        Id = i * 100 + j,
                        OwnedIntValue1 = i * 10 + 1,
                        OwnedIntValue2 = i * 10 + 2,
                        OwnedIntValue3 = i * 10 + 3,
                        OwnedIntValue4 = i * 10 + 4,
                        OwnedStringValue1 = "O" + i + "1",
                        OwnedStringValue2 = "O" + i + "2",
                        OwnedStringValue3 = "O" + i + "3",
                        OwnedStringValue4 = "O" + i + "4"
                    });
            }
        }

        for (var i = 0; i < _baseEntities.Length; i++)
        {
            _baseEntities[i].OwnedReference = new OwnedReference
            {
                OwnedIntValue1 = i * 10 + 1,
                OwnedIntValue2 = i * 10 + 2,
                OwnedIntValue3 = i * 10 + 3,
                OwnedIntValue4 = i * 10 + 4,
                OwnedStringValue1 = "O" + i + "1",
                OwnedStringValue2 = "O" + i + "2",
                OwnedStringValue3 = "O" + i + "3",
                OwnedStringValue4 = "O" + i + "4",
                OwnedNestedReference = new OwnedNestedReference
                {
                    OwnedNestedIntValue1 = i * 100 + 1,
                    OwnedNestedIntValue2 = i * 100 + 2,
                    OwnedNestedIntValue3 = i * 100 + 3,
                    OwnedNestedIntValue4 = i * 100 + 4,
                    OwnedNestedStringValue1 = "ON" + i + "1",
                    OwnedNestedStringValue2 = "ON" + i + "2",
                    OwnedNestedStringValue3 = "ON" + i + "3",
                    OwnedNestedStringValue4 = "ON" + i + "4"
                }
            };

            for (var j = 0; j < i; j++)
            {
                _baseEntities[i].OwnedCollection.Add(
                    new OwnedCollection
                    {
                        Id = i * 100 + j,
                        OwnedIntValue1 = i * 10 + 1,
                        OwnedIntValue2 = i * 10 + 2,
                        OwnedIntValue3 = i * 10 + 3,
                        OwnedIntValue4 = i * 10 + 4,
                        OwnedStringValue1 = "O" + i + "1",
                        OwnedStringValue2 = "O" + i + "2",
                        OwnedStringValue3 = "O" + i + "3",
                        OwnedStringValue4 = "O" + i + "4"
                    });
            }
        }
    }

    public Task Seed(EntitySplittingContext context)
    {
        // Seed data cannot contain any store generated value,
        // or recreate instances when calling AddRange
        context.AddRange(_entityOnes);
        context.AddRange(_entityTwos);
        context.AddRange(_entityThrees);
        context.AddRange(_baseEntities);

        return context.SaveChangesAsync();
    }
}
