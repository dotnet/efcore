﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Relationships;

public class RelationshipsData : ISetSource
{
    public RelationshipsData()
    {
        RootEntities = CreateRootEntities();
        RelatedTypes = [];
        NestedTypes = [];
        PreRootEntities = [];
    }

    public List<RootEntity> RootEntities { get; }

    // TODO: Remove? Relevant only for non-owned navigations
    public List<RelatedType> RelatedTypes { get; }
    public List<NestedType> NestedTypes { get; }

    public List<RootReferencingEntity> PreRootEntities { get; }

    public static List<RootEntity> CreateRootEntities()
    {
        var id = 1;

        List<RootEntity> rootEntities =
        [
            // First basic entity with all properties set
            CreateRootEntity(id++, description: null),

            // Second basic entity with all properties set to other values (but same across all properties)
            CreateRootEntity(id++, description: "With_other_values", e =>
            {
                SetRelatedValues(e.RequiredRelated);

                if (e.OptionalRelated is not null)
                {
                    SetRelatedValues(e.OptionalRelated);
                }

                foreach (var related in e.RelatedCollection)
                {
                    SetRelatedValues(related);
                }

                void SetRelatedValues(RelatedType related)
                {
                    related.Int = 9;
                    related.String = "bar";
                    related.RequiredNested.Int = 9;
                    related.RequiredNested.String = "bar";
                    related.OptionalNested?.Int = 9;
                    related.OptionalNested?.String = "bar";

                    foreach (var nested in related.NestedCollection)
                    {
                        nested.Int = 9;
                        nested.String = "bar";
                    }
                }
            }),

            // Third basic entity with all properties set to completely different values
            CreateRootEntity(id++, description: "With_different_values", e =>
            {
                var intValue = 100;
                var stringValue = 100;

                SetRelatedValues(e.RequiredRelated);

                if (e.OptionalRelated is not null)
                {
                    SetRelatedValues(e.OptionalRelated);
                }

                foreach (var related in e.RelatedCollection)
                {
                    SetRelatedValues(related);
                }

                void SetRelatedValues(RelatedType related)
                {
                    related.Int = intValue++;
                    related.String = $"foo{stringValue++}";
                    related.RequiredNested.Int = intValue++;
                    related.RequiredNested.String = $"foo{stringValue++}";
                    related.OptionalNested?.Int = intValue++;
                    related.OptionalNested?.String = $"foo{stringValue++}";

                    foreach (var nested in related.NestedCollection)
                    {
                        nested.Int = intValue++;
                        nested.String = $"foo{stringValue++}";
                    }
                }
            }),

            // Entity where values are referentially identical to each other across required/optional, to test various equality sceanarios.
            // Note that this gets overridden for owned navigations .
            CreateRootEntity(id++, description: "With_referential_identity", e =>
            {
                e.OptionalRelated = e.RequiredRelated;
                e.RequiredRelated.OptionalNested = e.RequiredRelated.RequiredNested;
                e.OptionalRelated.OptionalNested = e.RequiredRelated.RequiredNested;

                e.RelatedCollection.Clear();
                e.RequiredRelated.NestedCollection.Clear();
                e.OptionalRelated.NestedCollection.Clear();
            }),

            // Entity where everything optional is null
            CreateRootEntity(id++, description: "All_optionals_null", e =>
            {
                e.RequiredRelated.OptionalNested = null;
                e.OptionalRelated = null;

                foreach (var related in e.RelatedCollection)
                {
                    related.OptionalNested = null;
                }
            }),

            // Entity where all collections are empty
            CreateRootEntity(id++, description: "All_collections_empty", e =>
            {
                e.RelatedCollection.Clear();
                e.RequiredRelated.NestedCollection.Clear();
                e.OptionalRelated!.NestedCollection.Clear();
            })
        ];

        return rootEntities;

        RootEntity CreateRootEntity(int id, string? description, Action<RootEntity>? customizer = null)
        {
            var shortName = $"Root{id}";
            var relatedId = id * 100;
            var nestedId = id * 1000;

            const int intValue = 8;
            const string stringValue = "foo";

            var rootEntity = new RootEntity
            {
                Id = id,
                Name = description is null ? shortName : $"{shortName}_{description}",

                RequiredRelated = new RelatedType
                {
                    Id = relatedId++,
                    Name = $"{shortName}_RequiredRelated",

                    Int = intValue,
                    String = stringValue,

                    RequiredNested = new NestedType
                    {
                        Id = nestedId++,
                        Name = $"{shortName}_RequiredRelated_RequiredNested",

                        Int = intValue,
                        String = stringValue
                    },
                    OptionalNested = new NestedType
                    {
                        Id = nestedId++,
                        Name = $"{shortName}_RequiredRelated_OptionalNested",

                        Int = intValue,
                        String = stringValue
                    },
                    NestedCollection =
                    [
                        new NestedType
                        {
                            Id = nestedId++,
                            Name = $"{shortName}_RequiredRelated_NestedCollection_1",

                            Int = intValue,
                            String = stringValue
                        },
                        new NestedType
                        {
                            Id = nestedId++,
                            Name = $"{shortName}_RequiredRelated_NestedCollection_2",

                            Int = intValue,
                            String = stringValue
                        }
                    ]
                },
                OptionalRelated = new RelatedType
                {
                    Id = relatedId++,
                    Name = $"{shortName}_OptionalRelated",

                    Int = intValue,
                    String = stringValue,

                    RequiredNested = new NestedType
                    {
                        Id = nestedId++,
                        Name = $"{shortName}_OptionalRelated_RequiredNested",

                        Int = intValue,
                        String = stringValue
                    },
                    OptionalNested = new NestedType
                    {
                        Id = nestedId++,
                        Name = $"{shortName}_OptionalRelated_OptionalNested",

                        Int = intValue,
                        String = stringValue
                    },
                    NestedCollection =
                    [
                        new NestedType
                        {
                            Id = nestedId++,
                            Name = $"{shortName}_OptionalRelated_NestedCollection_1",

                            Int = intValue,
                            String = stringValue
                        },
                        new NestedType
                        {
                            Id = nestedId++,
                            Name = $"{shortName}_OptionalRelated_NestedCollection_2",

                            Int = intValue,
                            String = stringValue
                        }
                    ]
                },
                RelatedCollection =
                [
                    new RelatedType
                    {
                        Id = relatedId++,
                        Name = $"{shortName}_RelatedCollection_1",

                        Int = intValue,
                        String = stringValue,

                        RequiredNested = new NestedType
                        {
                            Id = nestedId++,
                            Name = $"{shortName}_RelatedCollection_1_RequiredNested",

                            Int = intValue,
                            String = stringValue
                        },
                        OptionalNested = new NestedType
                        {
                            Id = nestedId++,
                            Name = $"{shortName}_RelatedCollection_1_OptionalNested",

                            Int = intValue,
                            String = stringValue
                        },
                        NestedCollection =
                        [
                            new NestedType
                            {
                                Id = nestedId++,
                                Name = $"{shortName}_RelatedCollection_1_NestedCollection_1",

                                Int = intValue,
                                String = stringValue
                            },
                            new NestedType
                            {
                                Id = nestedId++,
                                Name = $"{shortName}_RelatedCollection_1_NestedCollection_2",

                                Int = intValue,
                                String = stringValue
                            }
                        ]
                    },
                    new RelatedType
                    {
                        Id = relatedId++,
                        Name = $"{shortName}_RelatedCollection_2",

                        Int = intValue,
                        String = stringValue,

                        RequiredNested = new NestedType
                        {
                            Id = nestedId++,
                            Name = $"{shortName}_RelatedCollection_2_RequiredNested",

                            Int = intValue,
                            String = stringValue
                        },
                        OptionalNested = new NestedType
                        {
                            Id = nestedId++,
                            Name = $"{shortName}_RelatedCollection_2_OptionalNested",

                            Int = intValue,
                            String = stringValue
                        },
                        NestedCollection =
                        [
                            new NestedType
                            {
                                Id = nestedId++,
                                Name = $"{shortName}_RelatedCollection_2_NestedCollection_1",

                                Int = intValue,
                                String = stringValue
                            },
                            new NestedType
                            {
                                Id = nestedId++,
                                Name = $"{shortName}_Root1_RelatedCollection_2_NestedCollection_2",

                                Int = intValue,
                                String = stringValue
                            }
                        ]
                    }
                ]
            };

            customizer?.Invoke(rootEntity);

            return rootEntity;
        }
    }

    public IQueryable<TEntity> Set<TEntity>()
        where TEntity : class
        => typeof(TEntity) switch
        {
            var t when t == typeof(RootEntity) => (IQueryable<TEntity>)RootEntities.AsQueryable(),
            var t when t == typeof(RelatedType) => (IQueryable<TEntity>)RelatedTypes.AsQueryable(),
            var t when t == typeof(NestedType) => (IQueryable<TEntity>)NestedTypes.AsQueryable(),
            var t when t == typeof(RootReferencingEntity) => (IQueryable<TEntity>)PreRootEntities.AsQueryable(),

            _ => throw new InvalidOperationException("Invalid entity type: " + typeof(TEntity))
        };
}
