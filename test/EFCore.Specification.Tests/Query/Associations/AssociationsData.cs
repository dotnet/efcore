// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Associations;

public class AssociationsData : ISetSource
{
    public AssociationsData()
    {
        RootEntities = CreateRootEntities();
        Associates = [];
        NestedAssociates = [];
        RootReferencingEntities = CreateRootReferencingEntities(RootEntities);

        ValueRootEntities = CreateValueRootEntities(RootEntities);
    }

    public List<RootEntity> RootEntities { get; }

    // TODO: Remove? Relevant only for non-owned navigations
    public List<AssociateType> Associates { get; }
    public List<NestedAssociateType> NestedAssociates { get; }

    public List<RootReferencingEntity> RootReferencingEntities { get; }

    public List<ValueRootEntity> ValueRootEntities { get; }

    private static List<RootEntity> CreateRootEntities()
    {
        var id = 1;

        List<RootEntity> rootEntities =
        [
            // First basic entity with all properties set
            CreateRootEntity(id++, description: null),

            // Second basic entity with all properties set to other values (but same across all properties)
            CreateRootEntity(
                id++, description: "With_other_values", e =>
                {
                    SetAssociateValues(e.RequiredAssociate);

                    if (e.OptionalAssociate is not null)
                    {
                        SetAssociateValues(e.OptionalAssociate);
                    }

                    foreach (var associate in e.AssociateCollection)
                    {
                        SetAssociateValues(associate);
                    }

                    void SetAssociateValues(AssociateType associate)
                    {
                        associate.Int = 9;
                        associate.String = "bar";
                        associate.Ints = [4, 5, 6, 6];
                        associate.RequiredNestedAssociate.Int = 9;
                        associate.RequiredNestedAssociate.String = "bar";
                        associate.RequiredNestedAssociate.Ints = [4, 5, 6, 6];
                        associate.OptionalNestedAssociate?.Int = 9;
                        associate.OptionalNestedAssociate?.String = "bar";
                        if (associate.OptionalNestedAssociate is not null)
                        {
                            associate.OptionalNestedAssociate.Ints = [4, 5, 6, 6];
                        }

                        foreach (var nested in associate.NestedCollection)
                        {
                            nested.Int = 9;
                            nested.String = "bar";
                            nested.Ints = [4, 5, 6, 6];
                        }
                    }
                }),

            // Third basic entity with all properties set to completely different values
            CreateRootEntity(
                id++, description: "With_different_values", e =>
                {
                    var intValue = 100;
                    var stringValue = 100;

                    SetAssociateValues(e.RequiredAssociate);

                    if (e.OptionalAssociate is not null)
                    {
                        SetAssociateValues(e.OptionalAssociate);
                    }

                    foreach (var associate in e.AssociateCollection)
                    {
                        SetAssociateValues(associate);
                    }

                    void SetAssociateValues(AssociateType associate)
                    {
                        associate.Int = intValue++;
                        associate.String = $"foo{stringValue++}";
                        associate.Ints = [8, 9, intValue++];
                        associate.RequiredNestedAssociate.Int = intValue++;
                        associate.RequiredNestedAssociate.String = $"foo{stringValue++}";
                        associate.RequiredNestedAssociate.Ints = [8, 9, intValue++];
                        associate.OptionalNestedAssociate?.Int = intValue++;
                        associate.OptionalNestedAssociate?.String = $"foo{stringValue++}";

                        if (associate.OptionalNestedAssociate is not null)
                        {
                            associate.OptionalNestedAssociate.Ints = [8, 9, intValue++];
                        }

                        foreach (var nested in associate.NestedCollection)
                        {
                            nested.Int = intValue++;
                            nested.String = $"foo{stringValue++}";
                            nested.Ints = [8, 9, intValue++];
                        }
                    }
                }),

            // Entity where values are referentially identical to each other across a given entity, to test various equality sceanarios.
            // Note that this gets overridden for owned navigations.
            CreateRootEntity(
                id++, description: "With_referential_identity", e =>
                {
                    e.OptionalAssociate = e.RequiredAssociate;
                    e.RequiredAssociate.OptionalNestedAssociate = e.RequiredAssociate.RequiredNestedAssociate;
                    e.OptionalAssociate.OptionalNestedAssociate = e.RequiredAssociate.RequiredNestedAssociate;

                    e.AssociateCollection.Clear();
                    e.RequiredAssociate.NestedCollection = [e.RequiredAssociate.RequiredNestedAssociate];
                    e.OptionalAssociate.NestedCollection.Clear();
                }),

            // Entity where everything optional is null
            CreateRootEntity(
                id++, description: "All_optionals_null", e =>
                {
                    e.RequiredAssociate.OptionalNestedAssociate = null;
                    e.OptionalAssociate = null;

                    foreach (var associate in e.AssociateCollection)
                    {
                        associate.OptionalNestedAssociate = null;
                    }
                }),

            // Entity where all collections are empty
            CreateRootEntity(
                id++, description: "All_collections_empty", e =>
                {
                    e.AssociateCollection.Clear();
                    e.RequiredAssociate.NestedCollection.Clear();
                    e.OptionalAssociate!.NestedCollection.Clear();
                }),

            // Entity with all string properties set to a value with special characters
            CreateRootEntity(
                id++, description: "With_special_characters", e =>
                {
                    SetAssociateValues(e.RequiredAssociate);

                    if (e.OptionalAssociate is not null)
                    {
                        SetAssociateValues(e.OptionalAssociate);
                    }

                    foreach (var associate in e.AssociateCollection)
                    {
                        SetAssociateValues(associate);
                    }

                    void SetAssociateValues(AssociateType associate)
                    {
                        associate.String = "{ this may/look:like JSON but it [isn't]: ממש ממש לאéèéè }";
                        associate.RequiredNestedAssociate.String = "{ this may/look:like JSON but it [isn't]: ממש ממש לאéèéè }";
                        associate.OptionalNestedAssociate?.String = "{ this may/look:like JSON but it [isn't]: ממש ממש לאéèéè }";

                        foreach (var nested in associate.NestedCollection)
                        {
                            nested.String = "{ this may/look:like JSON but it [isn't]: ממש ממש לאéèéè }";
                        }
                    }
                })
        ];

        return rootEntities;

        RootEntity CreateRootEntity(int id, string? description, Action<RootEntity>? customizer = null)
        {
            var shortName = $"Root{id}";
            var associateId = id * 100;
            var nestedId = id * 1000;

            const int intValue = 8;
            const string stringValue = "foo";
            List<int> intsValue = [1, 2, 3];

            var rootEntity = new RootEntity
            {
                Id = id,
                Name = description is null ? shortName : $"{shortName}_{description}",
                RequiredAssociate = new AssociateType
                {
                    Id = associateId++,
                    Name = $"{shortName}_RequiredAssociate",
                    Int = intValue,
                    String = stringValue,
                    Ints = intsValue,

                    RequiredNestedAssociate = new NestedAssociateType
                    {
                        Id = nestedId++,
                        Name = $"{shortName}_RequiredAssociate_RequiredNestedAssociate",
                        Int = intValue,
                        String = stringValue,
                        Ints = intsValue
                    },
                    OptionalNestedAssociate = new NestedAssociateType
                    {
                        Id = nestedId++,
                        Name = $"{shortName}_RequiredAssociate_OptionalNestedAssociate",
                        Int = intValue,
                        String = stringValue,
                        Ints = intsValue
                    },
                    NestedCollection =
                    [
                        new NestedAssociateType
                        {
                            Id = nestedId++,
                            Name = $"{shortName}_RequiredAssociate_NestedCollection_1",
                            Int = intValue,
                            String = stringValue,
                            Ints = intsValue
                        },
                        new NestedAssociateType
                        {
                            Id = nestedId++,
                            Name = $"{shortName}_RequiredAssociate_NestedCollection_2",
                            Int = intValue,
                            String = stringValue,
                            Ints = intsValue
                        }
                    ]
                },
                OptionalAssociate = new AssociateType
                {
                    Id = associateId++,
                    Name = $"{shortName}_OptionalAssociate",
                    Int = intValue,
                    String = stringValue,
                    Ints = [1, 2, 3],

                    RequiredNestedAssociate = new NestedAssociateType
                    {
                        Id = nestedId++,
                        Name = $"{shortName}_OptionalAssociate_RequiredNestedAssociate",
                        Int = intValue,
                        String = stringValue,
                        Ints = intsValue
                    },
                    OptionalNestedAssociate = new NestedAssociateType
                    {
                        Id = nestedId++,
                        Name = $"{shortName}_OptionalAssociate_OptionalNestedAssociate",
                        Int = intValue,
                        String = stringValue,
                        Ints = [1, 2, 3]
                    },
                    NestedCollection =
                    [
                        new NestedAssociateType
                        {
                            Id = nestedId++,
                            Name = $"{shortName}_OptionalAssociate_NestedCollection_1",
                            Int = intValue,
                            String = stringValue,
                            Ints = intsValue.ToList()
                        },
                        new NestedAssociateType
                        {
                            Id = nestedId++,
                            Name = $"{shortName}_OptionalAssociate_NestedCollection_2",
                            Int = intValue,
                            String = stringValue,
                            Ints = intsValue.ToList()
                        }
                    ]
                },
                AssociateCollection =
                [
                    new AssociateType
                    {
                        Id = associateId++,
                        Name = $"{shortName}_AssociateCollection_1",
                        Int = intValue,
                        String = stringValue,
                        Ints = [1, 2, 3],

                        RequiredNestedAssociate = new NestedAssociateType
                        {
                            Id = nestedId++,
                            Name = $"{shortName}_AssociateCollection_1_RequiredNestedAssociate",
                            Int = intValue,
                            String = stringValue,
                            Ints = [1, 2, 3]
                        },
                        OptionalNestedAssociate = new NestedAssociateType
                        {
                            Id = nestedId++,
                            Name = $"{shortName}_AssociateCollection_1_OptionalNestedAssociate",
                            Int = intValue,
                            String = stringValue,
                            Ints = [1, 2, 3]
                        },
                        NestedCollection =
                        [
                            new NestedAssociateType
                            {
                                Id = nestedId++,
                                Name = $"{shortName}_AssociateCollection_1_NestedCollection_1",
                                Int = intValue,
                                String = stringValue,
                                Ints = [1, 2, 3]
                            },
                            new NestedAssociateType
                            {
                                Id = nestedId++,
                                Name = $"{shortName}_AssociateCollection_1_NestedCollection_2",
                                Int = intValue,
                                String = stringValue,
                                Ints = [1, 2, 3]
                            }
                        ]
                    },
                    new AssociateType
                    {
                        Id = associateId++,
                        Name = $"{shortName}_AssociateCollection_2",
                        Int = intValue,
                        String = stringValue,
                        Ints = [1, 2, 3],

                        RequiredNestedAssociate = new NestedAssociateType
                        {
                            Id = nestedId++,
                            Name = $"{shortName}_AssociateCollection_2_RequiredNestedAssociate",
                            Int = intValue,
                            String = stringValue,
                            Ints = [1, 2, 3]
                        },
                        OptionalNestedAssociate = new NestedAssociateType
                        {
                            Id = nestedId++,
                            Name = $"{shortName}_AssociateCollection_2_OptionalNestedAssociate",
                            Int = intValue,
                            String = stringValue,
                            Ints = [1, 2, 3]
                        },
                        NestedCollection =
                        [
                            new NestedAssociateType
                            {
                                Id = nestedId++,
                                Name = $"{shortName}_AssociateCollection_2_NestedCollection_1",
                                Int = intValue,
                                String = stringValue,
                                Ints = [1, 2, 3]
                            },
                            new NestedAssociateType
                            {
                                Id = nestedId++,
                                Name = $"{shortName}_Root1_AssociateCollection_2_NestedCollection_2",
                                Int = intValue,
                                String = stringValue,
                                Ints = [1, 2, 3]
                            }
                        ]
                    }
                ]
            };

            customizer?.Invoke(rootEntity);

            return rootEntity;
        }
    }

    private static List<RootReferencingEntity> CreateRootReferencingEntities(IEnumerable<RootEntity> rootEntities)
    {
        var rootReferencingEntities = new List<RootReferencingEntity>();

        var id = 1;

        rootReferencingEntities.Add(new RootReferencingEntity { Id = id++, Root = null });
        foreach (var rootEntity in rootEntities.Take(2))
        {
            var rootReferencingEntity = new RootReferencingEntity { Id = id++, Root = rootEntity };
            rootEntity.RootReferencingEntity = rootReferencingEntity;
            rootReferencingEntities.Add(rootReferencingEntity);
        }

        return rootReferencingEntities;
    }

    private static List<ValueRootEntity> CreateValueRootEntities(List<RootEntity> rootEntities)
        => rootEntities.Select(ValueRootEntity.FromRootEntity).ToList();

    public IQueryable<TEntity> Set<TEntity>()
        where TEntity : class
        => typeof(TEntity) switch
        {
            var t when t == typeof(RootEntity) => (IQueryable<TEntity>)RootEntities.AsQueryable(),
            var t when t == typeof(AssociateType) => (IQueryable<TEntity>)Associates.AsQueryable(),
            var t when t == typeof(NestedAssociateType) => (IQueryable<TEntity>)NestedAssociates.AsQueryable(),
            var t when t == typeof(RootReferencingEntity) => (IQueryable<TEntity>)RootReferencingEntities.AsQueryable(),

            var t when t == typeof(ValueRootEntity) => (IQueryable<TEntity>)ValueRootEntities.AsQueryable(),

            _ => throw new InvalidOperationException("Invalid entity type: " + typeof(TEntity))
        };
}
