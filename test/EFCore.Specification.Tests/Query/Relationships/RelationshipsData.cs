// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Relationships;

public class RelationshipsData : ISetSource
{
    public RelationshipsData(bool withCollections = true)
    {
        RootEntities = CreateRootEntities(withCollections);
        MainTypes = [];
        NestedTypes = [];
        PreRootEntities = [];
    }

    // TODO: Remove? Relevant only for non-owned navigations
    public List<RootEntity> RootEntities { get; }
    public List<RelatedType> MainTypes { get; }
    public List<NestedType> NestedTypes { get; }
    public List<RootReferencingEntity> PreRootEntities { get; }

    public static List<RootEntity> CreateRootEntities(bool withCollections = true)
    {
        List<RootEntity> rootEntities =
        [
            new RootEntity
            {
                Id = 1,
                Name = "Root1",

                RequiredRelated = new RelatedType
                {
                    Id = 100,
                    Name = "Root1_RequiredRelated",

                    Int = 8,
                    String = "foo",

                    RequiredNested = new NestedType
                    {
                        Id = 1000,
                        Name = "Root1_RequiredRelated_RequiredNested",

                        Int = 50,
                        String = "foo_foo"
                    },
                    OptionalNested = new NestedType
                    {
                        Id = 1001,
                        Name = "Root1_RequiredRelated_OptionalNested",

                        Int = 51,
                        String = "foo_bar"
                    },
                    NestedCollection =
                    [
                        new NestedType
                        {
                            Id = 1002,
                            Name = "Root1_RequiredRelated_NestedCollection_1",

                            Int = 52,
                            String = "foo_baz1"
                        },
                        new NestedType
                        {
                            Id = 1003,
                            Name = "Root1_RequiredRelated_NestedCollection_2",

                            Int = 53,
                            String = "foo_baz2"
                        }
                    ]
                },
                OptionalRelated = new RelatedType
                {
                    Id = 101,
                    Name = "Root1_OptionalRelated",

                    Int = 9,
                    String = "bar",

                    RequiredNested = new NestedType
                    {
                        Id = 1010,
                        Name = "Root1_OptionalRelated_RequiredNested",

                        Int = 52,
                        String = "bar_foo"
                    },
                    OptionalNested = new NestedType
                    {
                        Id = 1011,
                        Name = "Root1_OptionalRelated_OptionalNested",

                        Int = 53,
                        String = "bar_bar"
                    },
                    NestedCollection =
                    [
                        new NestedType
                        {
                            Id = 1012,
                            Name = "Root1_OptionalRelated_NestedCollection_1",

                            Int = 54,
                            String = "bar_baz1"
                        },
                        new NestedType
                        {
                            Id = 1013,
                            Name = "Root1_OptionalRelated_NestedCollection_2",

                            Int = 55,
                            String = "bar_baz2"
                        }
                    ]
                },
                RelatedCollection =
                [
                    new RelatedType
                    {
                        Id = 102,
                        Name = "Root1_RelatedCollection_1",

                        Int = 21,
                        String = "foo",

                        RequiredNested = new NestedType
                        {
                            Id = 1020,
                            Name = "Root1_RelatedCollection_1_RequiredNested",

                            Int = 50,
                            String = "foo_foo"
                        },
                        OptionalNested = new NestedType
                        {
                            Id = 1021,
                            Name = "Root1_RelatedCollection_1_OptionalNested",

                            Int = 51,
                            String = "foo_bar"
                        },
                        NestedCollection =
                        [
                            new NestedType
                            {
                                Id = 1022,
                                Name = "Root1_RelatedCollection_1_NestedCollection_1",

                                Int = 53,
                                String = "foo_bar"
                            },
                            new NestedType
                            {
                                Id = 1023,
                                Name = "Root1_RelatedCollection_1_NestedCollection_2",

                                Int = 51,
                                String = "foo_bar"
                            }
                        ]
                    },
                    new RelatedType
                    {
                        Id = 103,
                        Name = "Root1_RelatedCollection_2",

                        Int = 22,
                        String = "foo",

                        RequiredNested = new NestedType
                        {
                            Id = 1030,
                            Name = "Root1_RelatedCollection_2_RequiredNested",

                            Int = 50,
                            String = "foo_foo"
                        },
                        OptionalNested = new NestedType
                        {
                            Id = 1031,
                            Name = "Root1_RelatedCollection_2_OptionalNested",

                            Int = 51,
                            String = "foo_bar"
                        },
                        NestedCollection =
                        [
                            new NestedType
                            {
                                Id = 1032,
                                Name = "Root1_RelatedCollection_2_NestedCollection_1",

                                Int = 53,
                                String = "foo_bar"
                            },
                            new NestedType
                            {
                                Id = 1033,
                                Name = "Root1_RelatedCollection_2_NestedCollection_2",

                                Int = 51,
                                String = "foo_bar"
                            }
                        ]
                    }
                ]
            },
            new RootEntity
            {
                Id = 2,
                Name = "Root2",

                RequiredRelated = new RelatedType
                {
                    Id = 200,
                    Name = "Root2_RequiredRelated",

                    Int = 10,
                    String = "aaa",

                    RequiredNested = new NestedType
                    {
                        Id = 2000,
                        Name = "Root2_RequiredRelated_RequiredNested",

                        Int = 54,
                        String = "aaa_xxx"
                    },
                    OptionalNested = new NestedType
                    {
                        Id = 2001,
                        Name = "Root2_RequiredRelated_OptionalNested",

                        Int = 55,
                        String = "aaa_yyy"
                    },
                    NestedCollection =
                    [
                        // TODO
                    ]
                },
                OptionalRelated = new RelatedType
                {
                    Id = 201,
                    Name = "Root2_OptionalRelated",

                    Int = 11,
                    String = "bbb",

                    RequiredNested = new NestedType
                    {
                        Id = 2010,
                        Name = "Root2_OptionalRelated_RequiredNested",

                        Int = 56,
                        String = "bbb_xxx"
                    },
                    OptionalNested = new NestedType
                    {
                        Id = 2011,
                        Name = "Root2_OptionalRelated_OptionalNested",

                        Int = 57,
                        String = "bbb_yyy"
                    },
                    NestedCollection =
                    [
                        // TODO
                    ]
                },
                RelatedCollection =
                [
                ]
            }
        ];

        if (!withCollections)
        {
            foreach (var rootEntity in rootEntities)
            {
                rootEntity.RelatedCollection = [];
                rootEntity.RequiredRelated.NestedCollection = [];
                rootEntity.OptionalRelated?.NestedCollection = [];
            }
        }

        return rootEntities;
    }

    public IQueryable<TEntity> Set<TEntity>()
        where TEntity : class
        => typeof(TEntity) switch
        {
            var t when t == typeof(RootEntity) => (IQueryable<TEntity>)RootEntities.AsQueryable(),
            var t when t == typeof(RelatedType) => (IQueryable<TEntity>)MainTypes.AsQueryable(),
            var t when t == typeof(NestedType) => (IQueryable<TEntity>)NestedTypes.AsQueryable(),
            var t when t == typeof(RootReferencingEntity) => (IQueryable<TEntity>)PreRootEntities.AsQueryable(),

            _ => throw new InvalidOperationException("Invalid entity type: " + typeof(TEntity))
        };
}
