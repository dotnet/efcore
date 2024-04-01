// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Globalization;
using Microsoft.EntityFrameworkCore.TestModels.EntitySplitting;

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public abstract class EntitySplittingQueryTestBase : NonSharedModelTestBase
{
    protected EntitySplittingQueryTestBase()
    {
        _setSourceCreator = GetSetSourceCreator();
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Can_query_entity_which_is_split_in_two(bool async)
    {
        await InitializeContextFactoryAsync(
            mb =>
            {
                mb.Entity<EntityOne>().SplitToTable(
                    "SplitEntityOnePart",
                    tb =>
                    {
                        tb.Property(e => e.IntValue3);
                        tb.Property(e => e.IntValue4);
                        tb.Property(e => e.StringValue3);
                        tb.Property(e => e.StringValue4);
                    });
            });

        await AssertQuery(
            async,
            ss => ss.Set<EntityOne>(),
            entryCount: 5);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Can_query_entity_which_is_split_in_three(bool async)
    {
        await InitializeContextFactoryAsync(
            mb =>
            {
                mb.Entity<EntityOne>(
                    b =>
                    {
                        b.SplitToTable(
                            "SplitEntityOnePart2",
                            tb =>
                            {
                                tb.Property(e => e.IntValue3);
                                tb.Property(e => e.StringValue3);
                            });

                        b.SplitToTable(
                            "SplitEntityOnePart3",
                            tb =>
                            {
                                tb.Property(e => e.IntValue4);
                                tb.Property(e => e.StringValue4);
                            });
                    });
            });

        await AssertQuery(
            async,
            ss => ss.Set<EntityOne>(),
            entryCount: 5);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Can_query_entity_which_is_split_selecting_only_main_properties(bool async)
    {
        await InitializeContextFactoryAsync(
            mb =>
            {
                mb.Entity<EntityOne>(
                    b =>
                    {
                        b.SplitToTable(
                            "SplitEntityOnePart2",
                            tb =>
                            {
                                tb.Property(e => e.IntValue3);
                                tb.Property(e => e.StringValue3);
                            });

                        b.SplitToTable(
                            "SplitEntityOnePart3",
                            tb =>
                            {
                                tb.Property(e => e.IntValue4);
                                tb.Property(e => e.StringValue4);
                            });
                    });
            });

        await AssertQuery(
            async,
            ss => ss.Set<EntityOne>().Select(
                e => new
                {
                    e.Id,
                    e.IntValue1,
                    e.StringValue1
                }),
            elementSorter: e => e.Id);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Can_query_entity_which_is_split_selecting_only_part_2_properties(bool async)
    {
        await InitializeContextFactoryAsync(
            mb =>
            {
                mb.Entity<EntityOne>(
                    b =>
                    {
                        b.SplitToTable(
                            "SplitEntityOnePart2",
                            tb =>
                            {
                                tb.Property(e => e.IntValue3);
                                tb.Property(e => e.StringValue3);
                            });

                        b.SplitToTable(
                            "SplitEntityOnePart3",
                            tb =>
                            {
                                tb.Property(e => e.IntValue4);
                                tb.Property(e => e.StringValue4);
                            });
                    });
            });

        await AssertQuery(
            async,
            ss => ss.Set<EntityOne>().Select(
                e => new
                {
                    e.Id,
                    e.IntValue3,
                    e.StringValue3
                }),
            elementSorter: e => e.Id);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Can_query_entity_which_is_split_selecting_only_part_3_properties(bool async)
    {
        await InitializeContextFactoryAsync(
            mb =>
            {
                mb.Entity<EntityOne>(
                    b =>
                    {
                        b.SplitToTable(
                            "SplitEntityOnePart2",
                            tb =>
                            {
                                tb.Property(e => e.IntValue3);
                                tb.Property(e => e.StringValue3);
                            });

                        b.SplitToTable(
                            "SplitEntityOnePart3",
                            tb =>
                            {
                                tb.Property(e => e.IntValue4);
                                tb.Property(e => e.StringValue4);
                            });
                    });
            });

        await AssertQuery(
            async,
            ss => ss.Set<EntityOne>().Select(
                e => new
                {
                    e.Id,
                    e.IntValue4,
                    e.StringValue4
                }),
            elementSorter: e => e.Id);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Include_reference_to_split_entity(bool async)
    {
        await InitializeContextFactoryAsync(
            mb =>
            {
                mb.Entity<EntityOne>(
                    b =>
                    {
                        b.SplitToTable(
                            "SplitEntityOnePart2",
                            tb =>
                            {
                                tb.Property(e => e.IntValue3);
                                tb.Property(e => e.StringValue3);
                            });

                        b.SplitToTable(
                            "SplitEntityOnePart3",
                            tb =>
                            {
                                tb.Property(e => e.IntValue4);
                                tb.Property(e => e.StringValue4);
                            });
                    });
            });

        await AssertQuery(
            async,
            ss => ss.Set<EntityTwo>().Include(e => e.EntityOne),
            elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<EntityTwo>(i => i.EntityOne)),
            entryCount: 8);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Include_collection_to_split_entity(bool async)
    {
        await InitializeContextFactoryAsync(
            mb =>
            {
                mb.Entity<EntityOne>().SplitToTable(
                    "SplitEntityOnePart2",
                    tb =>
                    {
                        tb.Property(e => e.IntValue3);
                        tb.Property(e => e.StringValue3);
                    });

                mb.Entity<EntityOne>().SplitToTable(
                    "SplitEntityOnePart3",
                    tb =>
                    {
                        tb.Property(e => e.IntValue4);
                        tb.Property(e => e.StringValue4);
                    });
            });

        await AssertQuery(
            async,
            ss => ss.Set<EntityThree>().Include(e => e.EntityOnes),
            elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<EntityThree>(i => i.EntityOnes)),
            entryCount: 10);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Include_reference_to_split_entity_including_reference(bool async)
    {
        await InitializeContextFactoryAsync(
            mb =>
            {
                mb.Entity<EntityOne>(
                    b =>
                    {
                        b.SplitToTable(
                            "SplitEntityOnePart2",
                            tb =>
                            {
                                tb.Property(e => e.IntValue3);
                                tb.Property(e => e.StringValue3);
                            });

                        b.SplitToTable(
                            "SplitEntityOnePart3",
                            tb =>
                            {
                                tb.Property(e => e.IntValue4);
                                tb.Property(e => e.StringValue4);
                            });
                    });
            });

        await AssertQuery(
            async,
            ss => ss.Set<EntityTwo>().Include(e => e.EntityOne.EntityThree),
            elementAsserter: (e, a) => AssertInclude(
                e, a,
                new ExpectedInclude<EntityTwo>(i => i.EntityOne),
                new ExpectedInclude<EntityOne>(i => i.EntityThree)),
            entryCount: 10);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Include_collection_to_split_entity_including_collection(bool async)
    {
        await InitializeContextFactoryAsync(
            mb =>
            {
                mb.Entity<EntityOne>(
                    b =>
                    {
                        b.SplitToTable(
                            "SplitEntityOnePart2",
                            tb =>
                            {
                                tb.Property(e => e.IntValue3);
                                tb.Property(e => e.StringValue3);
                            });

                        b.SplitToTable(
                            "SplitEntityOnePart3",
                            tb =>
                            {
                                tb.Property(e => e.IntValue4);
                                tb.Property(e => e.StringValue4);
                            });
                    });
            });

        await AssertQuery(
            async,
            ss => ss.Set<EntityThree>().Include(e => e.EntityOnes).ThenInclude(e => e.EntityTwos),
            elementAsserter: (e, a) => AssertInclude(
                e, a,
                new ExpectedInclude<EntityThree>(i => i.EntityOnes),
                new ExpectedInclude<EntityOne>(i => i.EntityTwos)),
            entryCount: 15);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Include_reference_on_split_entity(bool async)
    {
        await InitializeContextFactoryAsync(
            mb =>
            {
                mb.Entity<EntityOne>(
                    b =>
                    {
                        b.SplitToTable(
                            "SplitEntityOnePart2",
                            tb =>
                            {
                                tb.Property(e => e.IntValue3);
                                tb.Property(e => e.StringValue3);
                            });

                        b.SplitToTable(
                            "SplitEntityOnePart3",
                            tb =>
                            {
                                tb.Property(e => e.IntValue4);
                                tb.Property(e => e.StringValue4);
                            });
                    });
            });

        await AssertQuery(
            async,
            ss => ss.Set<EntityOne>().Include(e => e.EntityThree),
            elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<EntityOne>(i => i.EntityThree)),
            entryCount: 8);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Include_collection_on_split_entity(bool async)
    {
        await InitializeContextFactoryAsync(
            mb =>
            {
                mb.Entity<EntityOne>(
                    b =>
                    {
                        b.SplitToTable(
                            "SplitEntityOnePart2",
                            tb =>
                            {
                                tb.Property(e => e.IntValue3);
                                tb.Property(e => e.StringValue3);
                            });

                        b.SplitToTable(
                            "SplitEntityOnePart3",
                            tb =>
                            {
                                tb.Property(e => e.IntValue4);
                                tb.Property(e => e.StringValue4);
                            });
                    });
            });

        await AssertQuery(
            async,
            ss => ss.Set<EntityOne>().Include(e => e.EntityTwos),
            elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<EntityOne>(i => i.EntityTwos)),
            entryCount: 10);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Custom_projection_trim_when_multiple_tables(bool async)
    {
        await InitializeContextFactoryAsync(
            mb =>
            {
                mb.Entity<EntityOne>(
                    b =>
                    {
                        b.SplitToTable(
                            "SplitEntityOnePart2",
                            tb =>
                            {
                                tb.Property(e => e.IntValue3);
                                tb.Property(e => e.StringValue3);
                            });

                        b.SplitToTable(
                            "SplitEntityOnePart3",
                            tb =>
                            {
                                tb.Property(e => e.IntValue4);
                                tb.Property(e => e.StringValue4);
                            });
                    });
            });

        await AssertQuery(
            async,
            ss => ss.Set<EntityOne>().Select(
                e => new
                {
                    e.IntValue1,
                    e.IntValue3,
                    e.EntityThree
                }),
            elementSorter: e => e.IntValue1,
            elementAsserter: (e, a) =>
            {
                AssertEqual(e.IntValue1, a.IntValue1);
                AssertEqual(e.IntValue3, a.IntValue3);
                AssertEqual(e.EntityThree, a.EntityThree);
            },
            entryCount: 3);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Normal_entity_owning_a_split_reference_with_main_fragment_sharing(bool async)
    {
        await InitializeContextFactoryAsync(
            mb =>
            {
                mb.Entity<EntityOne>(
                    b =>
                    {
                        b.OwnsOne(
                            e => e.OwnedReference,
                            o =>
                            {
                                o.SplitToTable(
                                    "OwnedReferenceExtras1",
                                    t =>
                                    {
                                        t.Property(e => e.OwnedIntValue3);
                                        t.Property(e => e.OwnedStringValue3);
                                    });

                                o.SplitToTable(
                                    "OwnedReferenceExtras2",
                                    t =>
                                    {
                                        t.Property(e => e.OwnedIntValue4);
                                        t.Property(e => e.OwnedStringValue4);
                                    });
                            });
                    });
            });

        await AssertQuery(
            async,
            ss => ss.Set<EntityOne>(),
            elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<EntityOne>(i => i.OwnedReference)),
            entryCount: 10);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Normal_entity_owning_a_split_reference_with_main_fragment_sharing_custom_projection(bool async)
    {
        await InitializeContextFactoryAsync(
            mb =>
            {
                mb.Entity<EntityOne>(
                    b =>
                    {
                        b.ToTable("EntityOnes");

                        b.OwnsOne(
                            e => e.OwnedReference,
                            o =>
                            {
                                o.SplitToTable(
                                    "OwnedReferenceExtras1",
                                    t =>
                                    {
                                        t.Property(e => e.OwnedIntValue3);
                                        t.Property(e => e.OwnedStringValue3);
                                    });

                                o.SplitToTable(
                                    "OwnedReferenceExtras2",
                                    t =>
                                    {
                                        t.Property(e => e.OwnedIntValue4);
                                        t.Property(e => e.OwnedStringValue4);
                                    });
                            });
                    });
            });

        await AssertQuery(
            async,
            ss => ss.Set<EntityOne>().Select(
                e => new
                {
                    e.Id,
                    e.OwnedReference.OwnedIntValue4,
                    e.OwnedReference.OwnedStringValue4
                }),
            elementSorter: e => e.Id,
            elementAsserter: (e, a) =>
            {
                AssertEqual(e.Id, a.Id);
                AssertEqual(e.OwnedIntValue4, e.OwnedIntValue4);
                AssertEqual(e.OwnedStringValue4, e.OwnedStringValue4);
            },
            entryCount: 0);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Normal_entity_owning_a_split_reference_with_main_fragment_not_sharing(bool async)
    {
        await InitializeContextFactoryAsync(
            mb =>
            {
                mb.Entity<EntityOne>(
                    b =>
                    {
                        b.ToTable("EntityOnes");

                        b.OwnsOne(
                            e => e.OwnedReference,
                            o =>
                            {
                                o.ToTable("OwnedReferences");

                                o.SplitToTable(
                                    "OwnedReferenceExtras1",
                                    t =>
                                    {
                                        t.Property(e => e.OwnedIntValue3);
                                        t.Property(e => e.OwnedStringValue3);
                                    });

                                o.SplitToTable(
                                    "OwnedReferenceExtras2",
                                    t =>
                                    {
                                        t.Property(e => e.OwnedIntValue4);
                                        t.Property(e => e.OwnedStringValue4);
                                    });
                            });
                    });
            });

        await AssertQuery(
            async,
            ss => ss.Set<EntityOne>(),
            elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<EntityOne>(i => i.OwnedReference)),
            entryCount: 10);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Normal_entity_owning_a_split_reference_with_main_fragment_not_sharing_custom_projection(bool async)
    {
        await InitializeContextFactoryAsync(
            mb =>
            {
                mb.Entity<EntityOne>(
                    b =>
                    {
                        b.ToTable("EntityOnes");

                        b.OwnsOne(
                            e => e.OwnedReference,
                            o =>
                            {
                                o.ToTable("OwnedReferences");

                                o.SplitToTable(
                                    "OwnedReferenceExtras1",
                                    t =>
                                    {
                                        t.Property(e => e.OwnedIntValue3);
                                        t.Property(e => e.OwnedStringValue3);
                                    });

                                o.SplitToTable(
                                    "OwnedReferenceExtras2",
                                    t =>
                                    {
                                        t.Property(e => e.OwnedIntValue4);
                                        t.Property(e => e.OwnedStringValue4);
                                    });
                            });
                    });
            });

        await AssertQuery(
            async,
            ss => ss.Set<EntityOne>().Select(
                e => new
                {
                    e.Id,
                    e.OwnedReference.OwnedIntValue4,
                    e.OwnedReference.OwnedStringValue4
                }),
            elementSorter: e => e.Id,
            elementAsserter: (e, a) =>
            {
                AssertEqual(e.Id, a.Id);
                AssertEqual(e.OwnedIntValue4, e.OwnedIntValue4);
                AssertEqual(e.OwnedStringValue4, e.OwnedStringValue4);
            },
            entryCount: 0);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Normal_entity_owning_a_split_collection(bool async)
    {
        await InitializeContextFactoryAsync(
            mb =>
            {
                mb.Entity<EntityOne>(
                    b =>
                    {
                        b.ToTable("EntityOnes");

                        b.OwnsMany(
                            e => e.OwnedCollection,
                            o =>
                            {
                                o.ToTable("OwnedCollection");

                                o.SplitToTable(
                                    "OwnedCollectionExtras1",
                                    t =>
                                    {
                                        t.Property(e => e.OwnedIntValue3);
                                        t.Property(e => e.OwnedStringValue3);
                                    });

                                o.SplitToTable(
                                    "OwnedCollectionExtras2",
                                    t =>
                                    {
                                        t.Property(e => e.OwnedIntValue4);
                                        t.Property(e => e.OwnedStringValue4);
                                    });
                            });
                    });
            });

        await AssertQuery(
            async,
            ss => ss.Set<EntityOne>(),
            elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<EntityOne>(i => i.OwnedCollection)),
            entryCount: 15);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Normal_entity_owning_a_split_reference_with_main_fragment_sharing_multiple_level(bool async)
    {
        await InitializeContextFactoryAsync(
            mb =>
            {
                mb.Entity<EntityOne>(
                    b =>
                    {
                        b.ToTable("EntityOnes");

                        b.OwnsOne(
                            e => e.OwnedReference,
                            o =>
                            {
                                o.SplitToTable(
                                    "OwnedReferenceExtras1",
                                    t =>
                                    {
                                        t.Property(e => e.OwnedIntValue3);
                                        t.Property(e => e.OwnedStringValue3);
                                    });

                                o.SplitToTable(
                                    "OwnedReferenceExtras2",
                                    t =>
                                    {
                                        t.Property(e => e.OwnedIntValue4);
                                        t.Property(e => e.OwnedStringValue4);
                                    });

                                o.OwnsOne(
                                    e => e.OwnedNestedReference,
                                    oo =>
                                    {
                                        oo.SplitToTable(
                                            "OwnedNestedReferenceExtras1",
                                            t =>
                                            {
                                                t.Property(e => e.OwnedNestedIntValue3);
                                                t.Property(e => e.OwnedNestedStringValue3);
                                            });

                                        oo.SplitToTable(
                                            "OwnedNestedReferenceExtras2",
                                            t =>
                                            {
                                                t.Property(e => e.OwnedNestedIntValue4);
                                                t.Property(e => e.OwnedNestedStringValue4);
                                            });
                                    });
                            });
                    });
            });

        await AssertQuery(
            async,
            ss => ss.Set<EntityOne>(),
            elementAsserter: (e, a) => AssertInclude(
                e, a,
                new ExpectedInclude<EntityOne>(i => i.OwnedReference),
                new ExpectedInclude<OwnedReference>(i => i.OwnedNestedReference)),
            entryCount: 15);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Split_entity_owning_a_reference(bool async)
    {
        await InitializeContextFactoryAsync(
            mb =>
            {
                mb.Entity<EntityOne>(
                    b =>
                    {
                        b.SplitToTable(
                            "SplitEntityOnePart2",
                            tb =>
                            {
                                tb.Property(e => e.IntValue3);
                                tb.Property(e => e.StringValue3);
                            });

                        b.SplitToTable(
                            "SplitEntityOnePart3",
                            tb =>
                            {
                                tb.Property(e => e.IntValue4);
                                tb.Property(e => e.StringValue4);
                            });
                    });

                mb.Entity<EntityOne>().OwnsOne(e => e.OwnedReference);
            });

        await AssertQuery(
            async,
            ss => ss.Set<EntityOne>(),
            elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<EntityOne>(i => i.OwnedReference)),
            entryCount: 10);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Split_entity_owning_a_collection(bool async)
    {
        await InitializeContextFactoryAsync(
            mb =>
            {
                mb.Entity<EntityOne>(
                    b =>
                    {
                        b.SplitToTable(
                            "SplitEntityOnePart2",
                            tb =>
                            {
                                tb.Property(e => e.IntValue3);
                                tb.Property(e => e.StringValue3);
                            });

                        b.SplitToTable(
                            "SplitEntityOnePart3",
                            tb =>
                            {
                                tb.Property(e => e.IntValue4);
                                tb.Property(e => e.StringValue4);
                            });
                    });

                mb.Entity<EntityOne>().OwnsMany(e => e.OwnedCollection);
            });

        await AssertQuery(
            async,
            ss => ss.Set<EntityOne>(),
            elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<EntityOne>(i => i.OwnedCollection)),
            entryCount: 15);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Split_entity_owning_a_split_reference_without_table_sharing(bool async)
    {
        await InitializeContextFactoryAsync(
            mb =>
            {
                mb.Entity<EntityOne>(
                    b =>
                    {
                        b.SplitToTable(
                            "SplitEntityOnePart2",
                            tb =>
                            {
                                tb.Property(e => e.IntValue3);
                                tb.Property(e => e.StringValue3);
                            });

                        b.SplitToTable(
                            "SplitEntityOnePart3",
                            tb =>
                            {
                                tb.Property(e => e.IntValue4);
                                tb.Property(e => e.StringValue4);
                            });

                        b.OwnsOne(
                            e => e.OwnedReference,
                            o =>
                            {
                                o.ToTable("OwnedReferences");

                                o.SplitToTable(
                                    "OwnedReferenceExtras1",
                                    t =>
                                    {
                                        t.Property(e => e.OwnedIntValue3);
                                        t.Property(e => e.OwnedStringValue3);
                                    });

                                o.SplitToTable(
                                    "OwnedReferenceExtras2",
                                    t =>
                                    {
                                        t.Property(e => e.OwnedIntValue4);
                                        t.Property(e => e.OwnedStringValue4);
                                    });
                            });
                    });
            });

        await AssertQuery(
            async,
            ss => ss.Set<EntityOne>(),
            elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<EntityOne>(i => i.OwnedReference)),
            entryCount: 10);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Split_entity_owning_a_split_collection(bool async)
    {
        await InitializeContextFactoryAsync(
            mb =>
            {
                mb.Entity<EntityOne>(
                    b =>
                    {
                        b.SplitToTable(
                            "SplitEntityOnePart2",
                            tb =>
                            {
                                tb.Property(e => e.IntValue3);
                                tb.Property(e => e.StringValue3);
                            });

                        b.SplitToTable(
                            "SplitEntityOnePart3",
                            tb =>
                            {
                                tb.Property(e => e.IntValue4);
                                tb.Property(e => e.StringValue4);
                            });

                        b.OwnsMany(
                            e => e.OwnedCollection,
                            o =>
                            {
                                o.ToTable("OwnedCollection");

                                o.SplitToTable(
                                    "OwnedCollectionExtras1",
                                    t =>
                                    {
                                        t.Property(e => e.OwnedIntValue3);
                                        t.Property(e => e.OwnedStringValue3);
                                    });

                                o.SplitToTable(
                                    "OwnedCollectionExtras2",
                                    t =>
                                    {
                                        t.Property(e => e.OwnedIntValue4);
                                        t.Property(e => e.OwnedStringValue4);
                                    });
                            });
                    });
            });

        await AssertQuery(
            async,
            ss => ss.Set<EntityOne>(),
            elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<EntityOne>(i => i.OwnedCollection)),
            entryCount: 15);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Split_entity_owning_a_split_reference_with_table_sharing_1(bool async)
    {
        await InitializeContextFactoryAsync(
            mb =>
            {
                mb.Entity<EntityOne>(
                    b =>
                    {
                        b.ToTable("SplitEntityOnePart1");

                        b.SplitToTable(
                            "SplitEntityOnePart2",
                            tb =>
                            {
                                tb.Property(e => e.IntValue3);
                                tb.Property(e => e.StringValue3);
                            });

                        b.SplitToTable(
                            "SplitEntityOnePart3",
                            tb =>
                            {
                                tb.Property(e => e.IntValue4);
                                tb.Property(e => e.StringValue4);
                            });

                        b.OwnsOne(
                            e => e.OwnedReference,
                            o =>
                            {
                                o.ToTable("SplitEntityOnePart1");

                                o.SplitToTable(
                                    "SplitEntityOnePart2",
                                    t =>
                                    {
                                        t.Property(e => e.OwnedIntValue3);
                                        t.Property(e => e.OwnedStringValue3);
                                    });

                                o.SplitToTable(
                                    "SplitEntityOnePart3",
                                    t =>
                                    {
                                        t.Property(e => e.OwnedIntValue4);
                                        t.Property(e => e.OwnedStringValue4);
                                    });
                            });
                    });
            });

        await AssertQuery(
            async,
            ss => ss.Set<EntityOne>(),
            elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<EntityOne>(i => i.OwnedReference)),
            entryCount: 10);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Split_entity_owning_a_split_reference_with_table_sharing_4(bool async)
    {
        await InitializeContextFactoryAsync(
            mb =>
            {
                mb.Entity<EntityOne>(
                    b =>
                    {
                        b.ToTable("SplitEntityOnePart1");

                        b.SplitToTable(
                            "SplitEntityOnePart2",
                            tb =>
                            {
                                tb.Property(e => e.IntValue3);
                                tb.Property(e => e.StringValue3);
                            });

                        b.SplitToTable(
                            "SplitEntityOnePart3",
                            tb =>
                            {
                                tb.Property(e => e.IntValue4);
                                tb.Property(e => e.StringValue4);
                            });

                        b.OwnsOne(
                            e => e.OwnedReference,
                            o =>
                            {
                                o.ToTable("SplitEntityOnePart1");

                                o.SplitToTable(
                                    "SplitEntityOnePart2",
                                    t =>
                                    {
                                        t.Property(e => e.OwnedIntValue3);
                                        t.Property(e => e.OwnedStringValue3);
                                    });

                                o.SplitToTable(
                                    "OwnedReferencePart3",
                                    t =>
                                    {
                                        t.Property(e => e.OwnedIntValue4);
                                        t.Property(e => e.OwnedStringValue4);
                                    });
                            });
                    });
            });

        await AssertQuery(
            async,
            ss => ss.Set<EntityOne>(),
            elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<EntityOne>(i => i.OwnedReference)),
            entryCount: 10);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Split_entity_owning_a_split_reference_with_table_sharing_6(bool async)
    {
        await InitializeContextFactoryAsync(
            mb =>
            {
                mb.Entity<EntityOne>(
                    b =>
                    {
                        b.ToTable("SplitEntityOnePart1");

                        b.SplitToTable(
                            "SplitEntityOnePart2",
                            tb =>
                            {
                                tb.Property(e => e.IntValue3);
                                tb.Property(e => e.StringValue3);
                            });

                        b.SplitToTable(
                            "SplitEntityOnePart3",
                            tb =>
                            {
                                tb.Property(e => e.IntValue4);
                                tb.Property(e => e.StringValue4);
                            });

                        b.OwnsOne(
                            e => e.OwnedReference,
                            o =>
                            {
                                o.ToTable("SplitEntityOnePart2");

                                o.SplitToTable(
                                    "OwnedReferencePart2",
                                    t =>
                                    {
                                        t.Property(e => e.OwnedIntValue3);
                                        t.Property(e => e.OwnedStringValue3);
                                    });

                                o.SplitToTable(
                                    "OwnedReferencePart3",
                                    t =>
                                    {
                                        t.Property(e => e.OwnedIntValue4);
                                        t.Property(e => e.OwnedStringValue4);
                                    });
                            });
                    });
            });

        await AssertQuery(
            async,
            ss => ss.Set<EntityOne>(),
            elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<EntityOne>(i => i.OwnedReference)),
            entryCount: 10);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Tph_entity_owning_a_split_reference_on_base_with_table_sharing(bool async)
    {
        await InitializeContextFactoryAsync(
            mb =>
            {
                mb.Entity<BaseEntity>()
                    .OwnsOne(
                        e => e.OwnedReference,
                        o =>
                        {
                            o.SplitToTable(
                                "OwnedReferencePart3",
                                t =>
                                {
                                    t.Property(e => e.OwnedIntValue3);
                                    t.Property(e => e.OwnedStringValue3);
                                });

                            o.SplitToTable(
                                "OwnedReferencePart4",
                                t =>
                                {
                                    t.Property(e => e.OwnedIntValue4);
                                    t.Property(e => e.OwnedStringValue4);
                                });
                        });
            });

        await AssertQuery(
            async,
            ss => ss.Set<BaseEntity>(),
            elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<BaseEntity>(i => i.OwnedReference)),
            entryCount: 8);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Tpt_entity_owning_a_split_reference_on_base_with_table_sharing(bool async)
    {
        await InitializeContextFactoryAsync(
            mb =>
            {
                mb.Entity<BaseEntity>().UseTptMappingStrategy();

                mb.Entity<BaseEntity>()
                    .OwnsOne(
                        e => e.OwnedReference,
                        o =>
                        {
                            o.SplitToTable(
                                "OwnedReferencePart3",
                                t =>
                                {
                                    t.Property(e => e.OwnedIntValue3);
                                    t.Property(e => e.OwnedStringValue3);
                                });

                            o.SplitToTable(
                                "OwnedReferencePart4",
                                t =>
                                {
                                    t.Property(e => e.OwnedIntValue4);
                                    t.Property(e => e.OwnedStringValue4);
                                });
                        });
            });

        await AssertQuery(
            async,
            ss => ss.Set<BaseEntity>(),
            elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<BaseEntity>(i => i.OwnedReference)),
            entryCount: 8);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Tph_entity_owning_a_split_reference_on_middle_with_table_sharing(bool async)
    {
        await InitializeContextFactoryAsync(
            mb =>
            {
                mb.Entity<MiddleEntity>()
                    .OwnsOne(
                        e => e.OwnedReference,
                        o =>
                        {
                            o.SplitToTable(
                                "OwnedReferencePart3",
                                t =>
                                {
                                    t.Property(e => e.OwnedIntValue3);
                                    t.Property(e => e.OwnedStringValue3);
                                });

                            o.SplitToTable(
                                "OwnedReferencePart4",
                                t =>
                                {
                                    t.Property(e => e.OwnedIntValue4);
                                    t.Property(e => e.OwnedStringValue4);
                                });
                        });
            });

        await AssertQuery(
            async,
            ss => ss.Set<BaseEntity>(),
            elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<MiddleEntity>(i => i.OwnedReference)),
            entryCount: 6);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Tpt_entity_owning_a_split_reference_on_middle_with_table_sharing(bool async)
    {
        await InitializeContextFactoryAsync(
            mb =>
            {
                mb.Entity<BaseEntity>().UseTptMappingStrategy();

                mb.Entity<MiddleEntity>()
                    .OwnsOne(
                        e => e.OwnedReference,
                        o =>
                        {
                            o.SplitToTable(
                                "OwnedReferencePart3",
                                t =>
                                {
                                    t.Property(e => e.OwnedIntValue3);
                                    t.Property(e => e.OwnedStringValue3);
                                });

                            o.SplitToTable(
                                "OwnedReferencePart4",
                                t =>
                                {
                                    t.Property(e => e.OwnedIntValue4);
                                    t.Property(e => e.OwnedStringValue4);
                                });
                        });
            });

        await AssertQuery(
            async,
            ss => ss.Set<BaseEntity>(),
            elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<MiddleEntity>(i => i.OwnedReference)),
            entryCount: 6);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Tph_entity_owning_a_split_reference_on_leaf_with_table_sharing(bool async)
    {
        await InitializeContextFactoryAsync(
            mb =>
            {
                mb.Entity<LeafEntity>()
                    .OwnsOne(
                        e => e.OwnedReference,
                        o =>
                        {
                            o.SplitToTable(
                                "OwnedReferencePart3",
                                t =>
                                {
                                    t.Property(e => e.OwnedIntValue3);
                                    t.Property(e => e.OwnedStringValue3);
                                });

                            o.SplitToTable(
                                "OwnedReferencePart4",
                                t =>
                                {
                                    t.Property(e => e.OwnedIntValue4);
                                    t.Property(e => e.OwnedStringValue4);
                                });
                        });
            });

        await AssertQuery(
            async,
            ss => ss.Set<BaseEntity>(),
            elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<LeafEntity>(i => i.OwnedReference)),
            entryCount: 5);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Tpt_entity_owning_a_split_reference_on_leaf_with_table_sharing(bool async)
    {
        await InitializeContextFactoryAsync(
            mb =>
            {
                mb.Entity<BaseEntity>().UseTptMappingStrategy();

                mb.Entity<LeafEntity>()
                    .OwnsOne(
                        e => e.OwnedReference,
                        o =>
                        {
                            o.SplitToTable(
                                "OwnedReferencePart3",
                                t =>
                                {
                                    t.Property(e => e.OwnedIntValue3);
                                    t.Property(e => e.OwnedStringValue3);
                                });

                            o.SplitToTable(
                                "OwnedReferencePart4",
                                t =>
                                {
                                    t.Property(e => e.OwnedIntValue4);
                                    t.Property(e => e.OwnedStringValue4);
                                });
                        });
            });

        await AssertQuery(
            async,
            ss => ss.Set<BaseEntity>(),
            elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<LeafEntity>(i => i.OwnedReference)),
            entryCount: 5);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Tpc_entity_owning_a_split_reference_on_leaf_with_table_sharing(bool async)
    {
        await InitializeContextFactoryAsync(
            mb =>
            {
                mb.Entity<BaseEntity>().UseTpcMappingStrategy();

                mb.Entity<LeafEntity>()
                    .OwnsOne(
                        e => e.OwnedReference,
                        o =>
                        {
                            o.SplitToTable(
                                "OwnedReferencePart3",
                                t =>
                                {
                                    t.Property(e => e.OwnedIntValue3);
                                    t.Property(e => e.OwnedStringValue3);
                                });

                            o.SplitToTable(
                                "OwnedReferencePart4",
                                t =>
                                {
                                    t.Property(e => e.OwnedIntValue4);
                                    t.Property(e => e.OwnedStringValue4);
                                });
                        });
            });

        await AssertQuery(
            async,
            ss => ss.Set<BaseEntity>(),
            elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<LeafEntity>(i => i.OwnedReference)),
            entryCount: 5);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Tph_entity_owning_a_split_reference_on_base_with_table_sharing_querying_sibling(bool async)
    {
        await InitializeContextFactoryAsync(
            mb =>
            {
                mb.Entity<BaseEntity>()
                    .OwnsOne(
                        e => e.OwnedReference,
                        o =>
                        {
                            o.SplitToTable(
                                "OwnedReferencePart3",
                                t =>
                                {
                                    t.Property(e => e.OwnedIntValue3);
                                    t.Property(e => e.OwnedStringValue3);
                                });

                            o.SplitToTable(
                                "OwnedReferencePart4",
                                t =>
                                {
                                    t.Property(e => e.OwnedIntValue4);
                                    t.Property(e => e.OwnedStringValue4);
                                });
                        });
            });

        await AssertQuery(
            async,
            ss => ss.Set<SiblingEntity>(),
            elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<SiblingEntity>(i => i.OwnedReference)),
            entryCount: 2);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Tpt_entity_owning_a_split_reference_on_base_with_table_sharing_querying_sibling(bool async)
    {
        await InitializeContextFactoryAsync(
            mb =>
            {
                mb.Entity<BaseEntity>().UseTptMappingStrategy();

                mb.Entity<BaseEntity>()
                    .OwnsOne(
                        e => e.OwnedReference,
                        o =>
                        {
                            o.SplitToTable(
                                "OwnedReferencePart3",
                                t =>
                                {
                                    t.Property(e => e.OwnedIntValue3);
                                    t.Property(e => e.OwnedStringValue3);
                                });

                            o.SplitToTable(
                                "OwnedReferencePart4",
                                t =>
                                {
                                    t.Property(e => e.OwnedIntValue4);
                                    t.Property(e => e.OwnedStringValue4);
                                });
                        });
            });

        await AssertQuery(
            async,
            ss => ss.Set<SiblingEntity>(),
            elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<SiblingEntity>(i => i.OwnedReference)),
            entryCount: 2);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Tph_entity_owning_a_split_reference_on_middle_with_table_sharing_querying_sibling(bool async)
    {
        await InitializeContextFactoryAsync(
            mb =>
            {
                mb.Entity<MiddleEntity>()
                    .OwnsOne(
                        e => e.OwnedReference,
                        o =>
                        {
                            o.SplitToTable(
                                "OwnedReferencePart3",
                                t =>
                                {
                                    t.Property(e => e.OwnedIntValue3);
                                    t.Property(e => e.OwnedStringValue3);
                                });

                            o.SplitToTable(
                                "OwnedReferencePart4",
                                t =>
                                {
                                    t.Property(e => e.OwnedIntValue4);
                                    t.Property(e => e.OwnedStringValue4);
                                });
                        });
            });

        await AssertQuery(
            async,
            ss => ss.Set<SiblingEntity>(),
            elementAsserter: (e, a) => AssertInclude(e, a),
            entryCount: 1);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Tpt_entity_owning_a_split_reference_on_middle_with_table_sharing_querying_sibling(bool async)
    {
        await InitializeContextFactoryAsync(
            mb =>
            {
                mb.Entity<BaseEntity>().UseTptMappingStrategy();

                mb.Entity<MiddleEntity>()
                    .OwnsOne(
                        e => e.OwnedReference,
                        o =>
                        {
                            o.SplitToTable(
                                "OwnedReferencePart3",
                                t =>
                                {
                                    t.Property(e => e.OwnedIntValue3);
                                    t.Property(e => e.OwnedStringValue3);
                                });

                            o.SplitToTable(
                                "OwnedReferencePart4",
                                t =>
                                {
                                    t.Property(e => e.OwnedIntValue4);
                                    t.Property(e => e.OwnedStringValue4);
                                });
                        });
            });

        await AssertQuery(
            async,
            ss => ss.Set<SiblingEntity>(),
            elementAsserter: (e, a) => AssertInclude(e, a),
            entryCount: 1);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Tph_entity_owning_a_split_reference_on_leaf_with_table_sharing_querying_sibling(bool async)
    {
        await InitializeContextFactoryAsync(
            mb =>
            {
                mb.Entity<LeafEntity>()
                    .OwnsOne(
                        e => e.OwnedReference,
                        o =>
                        {
                            o.SplitToTable(
                                "OwnedReferencePart3",
                                t =>
                                {
                                    t.Property(e => e.OwnedIntValue3);
                                    t.Property(e => e.OwnedStringValue3);
                                });

                            o.SplitToTable(
                                "OwnedReferencePart4",
                                t =>
                                {
                                    t.Property(e => e.OwnedIntValue4);
                                    t.Property(e => e.OwnedStringValue4);
                                });
                        });
            });

        await AssertQuery(
            async,
            ss => ss.Set<SiblingEntity>(),
            elementAsserter: (e, a) => AssertInclude(e, a),
            entryCount: 1);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Tpt_entity_owning_a_split_reference_on_leaf_with_table_sharing_querying_sibling(bool async)
    {
        await InitializeContextFactoryAsync(
            mb =>
            {
                mb.Entity<BaseEntity>().UseTptMappingStrategy();

                mb.Entity<LeafEntity>()
                    .OwnsOne(
                        e => e.OwnedReference,
                        o =>
                        {
                            o.SplitToTable(
                                "OwnedReferencePart3",
                                t =>
                                {
                                    t.Property(e => e.OwnedIntValue3);
                                    t.Property(e => e.OwnedStringValue3);
                                });

                            o.SplitToTable(
                                "OwnedReferencePart4",
                                t =>
                                {
                                    t.Property(e => e.OwnedIntValue4);
                                    t.Property(e => e.OwnedStringValue4);
                                });
                        });
            });

        await AssertQuery(
            async,
            ss => ss.Set<SiblingEntity>(),
            elementAsserter: (e, a) => AssertInclude(e, a),
            entryCount: 1);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Tpc_entity_owning_a_split_reference_on_leaf_with_table_sharing_querying_sibling(bool async)
    {
        await InitializeContextFactoryAsync(
            mb =>
            {
                mb.Entity<BaseEntity>().UseTpcMappingStrategy();

                mb.Entity<LeafEntity>()
                    .OwnsOne(
                        e => e.OwnedReference,
                        o =>
                        {
                            o.SplitToTable(
                                "OwnedReferencePart3",
                                t =>
                                {
                                    t.Property(e => e.OwnedIntValue3);
                                    t.Property(e => e.OwnedStringValue3);
                                });

                            o.SplitToTable(
                                "OwnedReferencePart4",
                                t =>
                                {
                                    t.Property(e => e.OwnedIntValue4);
                                    t.Property(e => e.OwnedStringValue4);
                                });
                        });
            });

        await AssertQuery(
            async,
            ss => ss.Set<SiblingEntity>(),
            elementAsserter: (e, a) => AssertInclude(e, a),
            entryCount: 1);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Tph_entity_owning_a_split_reference_on_base_without_table_sharing(bool async)
    {
        await InitializeContextFactoryAsync(
            mb =>
            {
                mb.Entity<BaseEntity>()
                    .OwnsOne(
                        e => e.OwnedReference,
                        o =>
                        {
                            o.ToTable("OwnedReferencePart1");

                            o.SplitToTable(
                                "OwnedReferencePart3",
                                t =>
                                {
                                    t.Property(e => e.OwnedIntValue3);
                                    t.Property(e => e.OwnedStringValue3);
                                });

                            o.SplitToTable(
                                "OwnedReferencePart4",
                                t =>
                                {
                                    t.Property(e => e.OwnedIntValue4);
                                    t.Property(e => e.OwnedStringValue4);
                                });
                        });
            });

        await AssertQuery(
            async,
            ss => ss.Set<BaseEntity>(),
            elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<BaseEntity>(i => i.OwnedReference)),
            entryCount: 8);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Tpt_entity_owning_a_split_reference_on_base_without_table_sharing(bool async)
    {
        await InitializeContextFactoryAsync(
            mb =>
            {
                mb.Entity<BaseEntity>().UseTptMappingStrategy();

                mb.Entity<BaseEntity>()
                    .OwnsOne(
                        e => e.OwnedReference,
                        o =>
                        {
                            o.ToTable("OwnedReferencePart1");

                            o.SplitToTable(
                                "OwnedReferencePart3",
                                t =>
                                {
                                    t.Property(e => e.OwnedIntValue3);
                                    t.Property(e => e.OwnedStringValue3);
                                });

                            o.SplitToTable(
                                "OwnedReferencePart4",
                                t =>
                                {
                                    t.Property(e => e.OwnedIntValue4);
                                    t.Property(e => e.OwnedStringValue4);
                                });
                        });
            });

        await AssertQuery(
            async,
            ss => ss.Set<BaseEntity>(),
            elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<BaseEntity>(i => i.OwnedReference)),
            entryCount: 8);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Tpc_entity_owning_a_split_reference_on_base_without_table_sharing(bool async)
    {
        await InitializeContextFactoryAsync(
            mb =>
            {
                mb.Entity<BaseEntity>().UseTpcMappingStrategy();

                mb.Entity<BaseEntity>()
                    .OwnsOne(
                        e => e.OwnedReference,
                        o =>
                        {
                            o.ToTable("OwnedReferencePart1");

                            o.SplitToTable(
                                "OwnedReferencePart3",
                                t =>
                                {
                                    t.Property(e => e.OwnedIntValue3);
                                    t.Property(e => e.OwnedStringValue3);
                                });

                            o.SplitToTable(
                                "OwnedReferencePart4",
                                t =>
                                {
                                    t.Property(e => e.OwnedIntValue4);
                                    t.Property(e => e.OwnedStringValue4);
                                });
                        });
            });

        await AssertQuery(
            async,
            ss => ss.Set<BaseEntity>(),
            elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<BaseEntity>(i => i.OwnedReference)),
            entryCount: 8);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Tph_entity_owning_a_split_reference_on_middle_without_table_sharing(bool async)
    {
        await InitializeContextFactoryAsync(
            mb =>
            {
                mb.Entity<MiddleEntity>()
                    .OwnsOne(
                        e => e.OwnedReference,
                        o =>
                        {
                            o.ToTable("OwnedReferencePart1");

                            o.SplitToTable(
                                "OwnedReferencePart3",
                                t =>
                                {
                                    t.Property(e => e.OwnedIntValue3);
                                    t.Property(e => e.OwnedStringValue3);
                                });

                            o.SplitToTable(
                                "OwnedReferencePart4",
                                t =>
                                {
                                    t.Property(e => e.OwnedIntValue4);
                                    t.Property(e => e.OwnedStringValue4);
                                });
                        });
            });

        await AssertQuery(
            async,
            ss => ss.Set<BaseEntity>(),
            elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<MiddleEntity>(i => i.OwnedReference)),
            entryCount: 6);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Tpt_entity_owning_a_split_reference_on_middle_without_table_sharing(bool async)
    {
        await InitializeContextFactoryAsync(
            mb =>
            {
                mb.Entity<BaseEntity>().UseTptMappingStrategy();

                mb.Entity<MiddleEntity>()
                    .OwnsOne(
                        e => e.OwnedReference,
                        o =>
                        {
                            o.ToTable("OwnedReferencePart1");

                            o.SplitToTable(
                                "OwnedReferencePart3",
                                t =>
                                {
                                    t.Property(e => e.OwnedIntValue3);
                                    t.Property(e => e.OwnedStringValue3);
                                });

                            o.SplitToTable(
                                "OwnedReferencePart4",
                                t =>
                                {
                                    t.Property(e => e.OwnedIntValue4);
                                    t.Property(e => e.OwnedStringValue4);
                                });
                        });
            });

        await AssertQuery(
            async,
            ss => ss.Set<BaseEntity>(),
            elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<MiddleEntity>(i => i.OwnedReference)),
            entryCount: 6);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Tpc_entity_owning_a_split_reference_on_middle_without_table_sharing(bool async)
    {
        await InitializeContextFactoryAsync(
            mb =>
            {
                mb.Entity<BaseEntity>().UseTpcMappingStrategy();

                mb.Entity<MiddleEntity>()
                    .OwnsOne(
                        e => e.OwnedReference,
                        o =>
                        {
                            o.ToTable("OwnedReferencePart1");

                            o.SplitToTable(
                                "OwnedReferencePart3",
                                t =>
                                {
                                    t.Property(e => e.OwnedIntValue3);
                                    t.Property(e => e.OwnedStringValue3);
                                });

                            o.SplitToTable(
                                "OwnedReferencePart4",
                                t =>
                                {
                                    t.Property(e => e.OwnedIntValue4);
                                    t.Property(e => e.OwnedStringValue4);
                                });
                        });
            });

        await AssertQuery(
            async,
            ss => ss.Set<BaseEntity>(),
            elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<MiddleEntity>(i => i.OwnedReference)),
            entryCount: 6);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Tph_entity_owning_a_split_reference_on_leaf_without_table_sharing(bool async)
    {
        await InitializeContextFactoryAsync(
            mb =>
            {
                mb.Entity<LeafEntity>()
                    .OwnsOne(
                        e => e.OwnedReference,
                        o =>
                        {
                            o.ToTable("OwnedReferencePart1");

                            o.SplitToTable(
                                "OwnedReferencePart3",
                                t =>
                                {
                                    t.Property(e => e.OwnedIntValue3);
                                    t.Property(e => e.OwnedStringValue3);
                                });

                            o.SplitToTable(
                                "OwnedReferencePart4",
                                t =>
                                {
                                    t.Property(e => e.OwnedIntValue4);
                                    t.Property(e => e.OwnedStringValue4);
                                });
                        });
            });

        await AssertQuery(
            async,
            ss => ss.Set<BaseEntity>(),
            elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<LeafEntity>(i => i.OwnedReference)),
            entryCount: 5);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Tpt_entity_owning_a_split_reference_on_leaf_without_table_sharing(bool async)
    {
        await InitializeContextFactoryAsync(
            mb =>
            {
                mb.Entity<BaseEntity>().UseTptMappingStrategy();

                mb.Entity<LeafEntity>()
                    .OwnsOne(
                        e => e.OwnedReference,
                        o =>
                        {
                            o.ToTable("OwnedReferencePart1");

                            o.SplitToTable(
                                "OwnedReferencePart3",
                                t =>
                                {
                                    t.Property(e => e.OwnedIntValue3);
                                    t.Property(e => e.OwnedStringValue3);
                                });

                            o.SplitToTable(
                                "OwnedReferencePart4",
                                t =>
                                {
                                    t.Property(e => e.OwnedIntValue4);
                                    t.Property(e => e.OwnedStringValue4);
                                });
                        });
            });

        await AssertQuery(
            async,
            ss => ss.Set<BaseEntity>(),
            elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<LeafEntity>(i => i.OwnedReference)),
            entryCount: 5);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Tpc_entity_owning_a_split_reference_on_leaf_without_table_sharing(bool async)
    {
        await InitializeContextFactoryAsync(
            mb =>
            {
                mb.Entity<BaseEntity>().UseTpcMappingStrategy();

                mb.Entity<LeafEntity>()
                    .OwnsOne(
                        e => e.OwnedReference,
                        o =>
                        {
                            o.ToTable("OwnedReferencePart1");

                            o.SplitToTable(
                                "OwnedReferencePart3",
                                t =>
                                {
                                    t.Property(e => e.OwnedIntValue3);
                                    t.Property(e => e.OwnedStringValue3);
                                });

                            o.SplitToTable(
                                "OwnedReferencePart4",
                                t =>
                                {
                                    t.Property(e => e.OwnedIntValue4);
                                    t.Property(e => e.OwnedStringValue4);
                                });
                        });
            });

        await AssertQuery(
            async,
            ss => ss.Set<BaseEntity>(),
            elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<LeafEntity>(i => i.OwnedReference)),
            entryCount: 5);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Tph_entity_owning_a_split_collection_on_base(bool async)
    {
        await InitializeContextFactoryAsync(
            mb =>
            {
                mb.Entity<BaseEntity>()
                    .OwnsMany(
                        e => e.OwnedCollection,
                        o =>
                        {
                            o.ToTable("OwnedReferencePart1");

                            o.SplitToTable(
                                "OwnedReferencePart3",
                                t =>
                                {
                                    t.Property(e => e.OwnedIntValue3);
                                    t.Property(e => e.OwnedStringValue3);
                                });

                            o.SplitToTable(
                                "OwnedReferencePart4",
                                t =>
                                {
                                    t.Property(e => e.OwnedIntValue4);
                                    t.Property(e => e.OwnedStringValue4);
                                });
                        });
            });

        await AssertQuery(
            async,
            ss => ss.Set<BaseEntity>(),
            elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<BaseEntity>(i => i.OwnedCollection)),
            entryCount: 10);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Tpt_entity_owning_a_split_collection_on_base(bool async)
    {
        await InitializeContextFactoryAsync(
            mb =>
            {
                mb.Entity<BaseEntity>().UseTptMappingStrategy();

                mb.Entity<BaseEntity>()
                    .OwnsMany(
                        e => e.OwnedCollection,
                        o =>
                        {
                            o.ToTable("OwnedReferencePart1");

                            o.SplitToTable(
                                "OwnedReferencePart3",
                                t =>
                                {
                                    t.Property(e => e.OwnedIntValue3);
                                    t.Property(e => e.OwnedStringValue3);
                                });

                            o.SplitToTable(
                                "OwnedReferencePart4",
                                t =>
                                {
                                    t.Property(e => e.OwnedIntValue4);
                                    t.Property(e => e.OwnedStringValue4);
                                });
                        });
            });

        await AssertQuery(
            async,
            ss => ss.Set<BaseEntity>(),
            elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<BaseEntity>(i => i.OwnedCollection)),
            entryCount: 10);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Tpc_entity_owning_a_split_collection_on_base(bool async)
    {
        await InitializeContextFactoryAsync(
            mb =>
            {
                mb.Entity<BaseEntity>().UseTpcMappingStrategy();

                mb.Entity<BaseEntity>()
                    .OwnsMany(
                        e => e.OwnedCollection,
                        o =>
                        {
                            o.ToTable("OwnedReferencePart1");

                            o.SplitToTable(
                                "OwnedReferencePart3",
                                t =>
                                {
                                    t.Property(e => e.OwnedIntValue3);
                                    t.Property(e => e.OwnedStringValue3);
                                });

                            o.SplitToTable(
                                "OwnedReferencePart4",
                                t =>
                                {
                                    t.Property(e => e.OwnedIntValue4);
                                    t.Property(e => e.OwnedStringValue4);
                                });
                        });
            });

        await AssertQuery(
            async,
            ss => ss.Set<BaseEntity>(),
            elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<BaseEntity>(i => i.OwnedCollection)),
            entryCount: 10);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Tph_entity_owning_a_split_collection_on_middle(bool async)
    {
        await InitializeContextFactoryAsync(
            mb =>
            {
                mb.Entity<MiddleEntity>()
                    .OwnsMany(
                        e => e.OwnedCollection,
                        o =>
                        {
                            o.ToTable("OwnedReferencePart1");

                            o.SplitToTable(
                                "OwnedReferencePart3",
                                t =>
                                {
                                    t.Property(e => e.OwnedIntValue3);
                                    t.Property(e => e.OwnedStringValue3);
                                });

                            o.SplitToTable(
                                "OwnedReferencePart4",
                                t =>
                                {
                                    t.Property(e => e.OwnedIntValue4);
                                    t.Property(e => e.OwnedStringValue4);
                                });
                        });
            });

        await AssertQuery(
            async,
            ss => ss.Set<BaseEntity>(),
            elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<MiddleEntity>(i => i.OwnedCollection)),
            entryCount: 8);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Tpt_entity_owning_a_split_collection_on_middle(bool async)
    {
        await InitializeContextFactoryAsync(
            mb =>
            {
                mb.Entity<BaseEntity>().UseTptMappingStrategy();

                mb.Entity<MiddleEntity>()
                    .OwnsMany(
                        e => e.OwnedCollection,
                        o =>
                        {
                            o.ToTable("OwnedReferencePart1");

                            o.SplitToTable(
                                "OwnedReferencePart3",
                                t =>
                                {
                                    t.Property(e => e.OwnedIntValue3);
                                    t.Property(e => e.OwnedStringValue3);
                                });

                            o.SplitToTable(
                                "OwnedReferencePart4",
                                t =>
                                {
                                    t.Property(e => e.OwnedIntValue4);
                                    t.Property(e => e.OwnedStringValue4);
                                });
                        });
            });

        await AssertQuery(
            async,
            ss => ss.Set<BaseEntity>(),
            elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<MiddleEntity>(i => i.OwnedCollection)),
            entryCount: 8);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Tpc_entity_owning_a_split_collection_on_middle(bool async)
    {
        await InitializeContextFactoryAsync(
            mb =>
            {
                mb.Entity<BaseEntity>().UseTpcMappingStrategy();

                mb.Entity<MiddleEntity>()
                    .OwnsMany(
                        e => e.OwnedCollection,
                        o =>
                        {
                            o.ToTable("OwnedReferencePart1");

                            o.SplitToTable(
                                "OwnedReferencePart3",
                                t =>
                                {
                                    t.Property(e => e.OwnedIntValue3);
                                    t.Property(e => e.OwnedStringValue3);
                                });

                            o.SplitToTable(
                                "OwnedReferencePart4",
                                t =>
                                {
                                    t.Property(e => e.OwnedIntValue4);
                                    t.Property(e => e.OwnedStringValue4);
                                });
                        });
            });

        await AssertQuery(
            async,
            ss => ss.Set<BaseEntity>(),
            elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<MiddleEntity>(i => i.OwnedCollection)),
            entryCount: 8);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Tph_entity_owning_a_split_collection_on_leaf(bool async)
    {
        await InitializeContextFactoryAsync(
            mb =>
            {
                mb.Entity<LeafEntity>()
                    .OwnsMany(
                        e => e.OwnedCollection,
                        o =>
                        {
                            o.ToTable("OwnedReferencePart1");

                            o.SplitToTable(
                                "OwnedReferencePart3",
                                t =>
                                {
                                    t.Property(e => e.OwnedIntValue3);
                                    t.Property(e => e.OwnedStringValue3);
                                });

                            o.SplitToTable(
                                "OwnedReferencePart4",
                                t =>
                                {
                                    t.Property(e => e.OwnedIntValue4);
                                    t.Property(e => e.OwnedStringValue4);
                                });
                        });
            });

        await AssertQuery(
            async,
            ss => ss.Set<BaseEntity>(),
            elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<LeafEntity>(i => i.OwnedCollection)),
            entryCount: 7);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Tpt_entity_owning_a_split_collection_on_leaf(bool async)
    {
        await InitializeContextFactoryAsync(
            mb =>
            {
                mb.Entity<BaseEntity>().UseTptMappingStrategy();

                mb.Entity<LeafEntity>()
                    .OwnsMany(
                        e => e.OwnedCollection,
                        o =>
                        {
                            o.ToTable("OwnedReferencePart1");

                            o.SplitToTable(
                                "OwnedReferencePart3",
                                t =>
                                {
                                    t.Property(e => e.OwnedIntValue3);
                                    t.Property(e => e.OwnedStringValue3);
                                });

                            o.SplitToTable(
                                "OwnedReferencePart4",
                                t =>
                                {
                                    t.Property(e => e.OwnedIntValue4);
                                    t.Property(e => e.OwnedStringValue4);
                                });
                        });
            });

        await AssertQuery(
            async,
            ss => ss.Set<BaseEntity>(),
            elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<LeafEntity>(i => i.OwnedCollection)),
            entryCount: 7);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Tpc_entity_owning_a_split_collection_on_leaf(bool async)
    {
        await InitializeContextFactoryAsync(
            mb =>
            {
                mb.Entity<BaseEntity>().UseTpcMappingStrategy();

                mb.Entity<LeafEntity>()
                    .OwnsMany(
                        e => e.OwnedCollection,
                        o =>
                        {
                            o.ToTable("OwnedReferencePart1");

                            o.SplitToTable(
                                "OwnedReferencePart3",
                                t =>
                                {
                                    t.Property(e => e.OwnedIntValue3);
                                    t.Property(e => e.OwnedStringValue3);
                                });

                            o.SplitToTable(
                                "OwnedReferencePart4",
                                t =>
                                {
                                    t.Property(e => e.OwnedIntValue4);
                                    t.Property(e => e.OwnedStringValue4);
                                });
                        });
            });

        await AssertQuery(
            async,
            ss => ss.Set<BaseEntity>(),
            elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<LeafEntity>(i => i.OwnedCollection)),
            entryCount: 7);
    }

    #region TestHelpers

    protected async Task AssertQuery<TResult>(
        bool async,
        Func<ISetSource, IQueryable<TResult>> queryCreator,
        Func<TResult, object> elementSorter = null,
        Action<TResult, TResult> elementAsserter = null,
        bool assertOrder = false,
        int entryCount = 0)
        where TResult : class
    {
        using var context = CreateContext();
        var query = queryCreator(_setSourceCreator(context));

        OrderingSettingsVerifier(assertOrder, query.Expression.Type, elementSorter);

        var actual = async
            ? await query.ToListAsync()
            : query.ToList();

        var expectedData = GetExpectedData();
        var expected = queryCreator(expectedData).ToList();

        if (!assertOrder
            && elementSorter == null)
        {
            EntitySorters.TryGetValue(typeof(TResult), out var sorter);
            elementSorter = (Func<TResult, object>)sorter;
        }

        if (elementAsserter == null)
        {
            EntityAsserters.TryGetValue(typeof(TResult), out var asserter);
            elementAsserter = (Action<TResult, TResult>)asserter;
        }

        TestHelpers.AssertResults(
            expected,
            actual,
            elementSorter,
            elementAsserter,
            assertOrder);

        Assert.Equal(entryCount, context.ChangeTracker.Entries().Count());
    }

    protected void AssertEqual<T>(T expected, T actual, Action<T, T> asserter = null)
    {
        if (asserter == null
            && expected != null)
        {
            EntityAsserters.TryGetValue(typeof(T), out var entityAsserter);
            asserter ??= (Action<T, T>)entityAsserter;
        }

        asserter ??= Assert.Equal;
        asserter(expected, actual);
    }

    protected void AssertCollection<TElement>(
        IEnumerable<TElement> expected,
        IEnumerable<TElement> actual,
        bool ordered = false,
        Func<TElement, object> elementSorter = null,
        Action<TElement, TElement> elementAsserter = null)

    {
        if (expected == null
            && actual == null)
        {
            return;
        }

        if (expected == null != (actual == null))
        {
            throw new InvalidOperationException(
                $"Nullability doesn't match. Expected: {(expected == null ? "NULL" : "NOT NULL")}. Actual: {(actual == null ? "NULL." : "NOT NULL.")}.");
        }

        EntitySorters.TryGetValue(typeof(TElement), out var sorter);
        EntityAsserters.TryGetValue(typeof(TElement), out var asserter);

        elementSorter ??= (Func<TElement, object>)sorter;
        elementAsserter ??= (Action<TElement, TElement>)asserter ?? Assert.Equal;

        if (!ordered)
        {
            if (elementSorter != null)
            {
                var sortedActual = actual.OrderBy(elementSorter).ToList();
                var sortedExpected = expected.OrderBy(elementSorter).ToList();

                Assert.Equal(sortedExpected.Count, sortedActual.Count);
                for (var i = 0; i < sortedExpected.Count; i++)
                {
                    elementAsserter(sortedExpected[i], sortedActual[i]);
                }
            }
            else
            {
                var sortedActual = actual.OrderBy(e => e).ToList();
                var sortedExpected = expected.OrderBy(e => e).ToList();

                Assert.Equal(sortedExpected.Count, sortedActual.Count);
                for (var i = 0; i < sortedExpected.Count; i++)
                {
                    elementAsserter(sortedExpected[i], sortedActual[i]);
                }
            }
        }
        else
        {
            var expectedList = expected.ToList();
            var actualList = actual.ToList();

            Assert.Equal(expectedList.Count, actualList.Count);
            for (var i = 0; i < expectedList.Count; i++)
            {
                elementAsserter(expectedList[i], actualList[i]);
            }
        }
    }

    private static readonly MethodInfo _assertIncludeEntity =
        typeof(EntitySplittingQueryTestBase).GetTypeInfo().GetDeclaredMethod(nameof(AssertIncludeEntity));

    private static readonly MethodInfo _assertIncludeCollectionMethodInfo =
        typeof(EntitySplittingQueryTestBase).GetTypeInfo().GetDeclaredMethod(nameof(AssertIncludeCollection));

    private static readonly MethodInfo _filteredIncludeMethodInfo =
        typeof(EntitySplittingQueryTestBase).GetTypeInfo().GetDeclaredMethod(nameof(FilteredInclude));

    private readonly List<string> _includePath = [];

    protected void AssertInclude<TEntity>(
        TEntity expected,
        TEntity actual,
        params IExpectedInclude[] expectedIncludes)
        => AssertIncludeInternal(expected, actual, expectedIncludes);

    private void AssertIncludeInternal<TEntity>(TEntity expected, TEntity actual, IExpectedInclude[] expectedIncludes)
    {
        _includePath.Clear();

        AssertIncludeObject(expected, actual, expectedIncludes, assertOrder: false);
    }

    private void AssertIncludeObject(object expected, object actual, IEnumerable<IExpectedInclude> expectedIncludes, bool assertOrder)
    {
        if (expected == null
            && actual == null)
        {
            return;
        }

        Assert.Equal(expected == null, actual == null);

        var expectedType = expected.GetType();
        if (expectedType.IsGenericType
            && expectedType.GetTypeInfo().ImplementedInterfaces.Any(
                i => i.IsConstructedGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>)))
        {
            _assertIncludeCollectionMethodInfo.MakeGenericMethod(expectedType.GenericTypeArguments[0])
                .Invoke(this, [expected, actual, expectedIncludes, assertOrder]);
        }
        else
        {
            _assertIncludeEntity.MakeGenericMethod(expectedType).Invoke(this, [expected, actual, expectedIncludes]);
        }
    }

    private void AssertIncludeEntity<TElement>(TElement expected, TElement actual, IEnumerable<IExpectedInclude> expectedIncludes)
    {
        Assert.Equal(expected.GetType(), actual.GetType());

        if (EntityAsserters.TryGetValue(typeof(TElement), out var asserter))
        {
            ((Action<TElement, TElement>)asserter)(expected, actual);
            ProcessIncludes(expected, actual, expectedIncludes);
        }
        else
        {
            throw new InvalidOperationException($"Couldn't find entity asserter for entity type: '{typeof(TElement).Name}'.");
        }
    }

    private void AssertIncludeCollection<TElement>(
        IEnumerable<TElement> expected,
        IEnumerable<TElement> actual,
        IEnumerable<IExpectedInclude> expectedIncludes,
        bool assertOrder)
    {
        var expectedList = expected.ToList();
        var actualList = actual.ToList();

        if (!assertOrder && EntitySorters.TryGetValue(typeof(TElement), out var sorter))
        {
            var actualSorter = (Func<TElement, object>)sorter;
            expectedList = expectedList.OrderBy(actualSorter).ToList();
            actualList = actualList.OrderBy(actualSorter).ToList();
        }

        Assert.Equal(expectedList.Count, actualList.Count);

        for (var i = 0; i < expectedList.Count; i++)
        {
            var elementType = expectedList[i].GetType();
            _assertIncludeEntity.MakeGenericMethod(elementType)
                .Invoke(this, [expectedList[i], actualList[i], expectedIncludes]);
        }
    }

    private void ProcessIncludes<TEntity>(TEntity expected, TEntity actual, IEnumerable<IExpectedInclude> expectedIncludes)
    {
        var currentPath = string.Join(".", _includePath);

        foreach (var expectedInclude in expectedIncludes.OfType<ExpectedInclude<TEntity>>().Where(i => i.NavigationPath == currentPath))
        {
            var expectedIncludedNavigation = GetIncluded(expected, expectedInclude.IncludeMember);
            var assertOrder = false;
            if (expectedInclude.GetType().BaseType != typeof(object))
            {
                var includedType = expectedInclude.GetType().GetGenericArguments()[1];
                var filterTypedMethod = _filteredIncludeMethodInfo.MakeGenericMethod(typeof(TEntity), includedType);
                expectedIncludedNavigation = filterTypedMethod.Invoke(
                    this,
                    BindingFlags.NonPublic,
                    null,
                    [expectedIncludedNavigation, expectedInclude],
                    CultureInfo.CurrentCulture);

                assertOrder = (bool)expectedInclude.GetType()
                    .GetProperty(nameof(ExpectedFilteredInclude<object, object>.AssertOrder))
                    .GetValue(expectedInclude);
            }

            var actualIncludedNavigation = GetIncluded(actual, expectedInclude.IncludeMember);

            _includePath.Add(expectedInclude.IncludeMember.Name);

            AssertIncludeObject(expectedIncludedNavigation, actualIncludedNavigation, expectedIncludes, assertOrder);

            _includePath.RemoveAt(_includePath.Count - 1);
        }
    }

    private IEnumerable<TIncluded> FilteredInclude<TEntity, TIncluded>(
        IEnumerable<TIncluded> expected,
        ExpectedFilteredInclude<TEntity, TIncluded> expectedFilteredInclude)
        => expectedFilteredInclude.IncludeFilter(expected);

    private object GetIncluded<TEntity>(TEntity entity, MemberInfo includeMember)
        => includeMember switch
        {
            FieldInfo fieldInfo => fieldInfo.GetValue(entity),
            PropertyInfo propertyInfo => propertyInfo.GetValue(entity),
            _ => throw new InvalidOperationException(),
        };

    protected void AssertGrouping<TKey, TElement>(
        IGrouping<TKey, TElement> expected,
        IGrouping<TKey, TElement> actual,
        bool ordered = false,
        Func<TElement, object> elementSorter = null,
        Action<TKey, TKey> keyAsserter = null,
        Action<TElement, TElement> elementAsserter = null)
    {
        keyAsserter ??= Assert.Equal;
        keyAsserter(expected.Key, actual.Key);
        AssertCollection(expected, actual, ordered, elementSorter, elementAsserter);
    }

    private void OrderingSettingsVerifier(bool assertOrder, Type type, object elementSorter)
    {
        if (!assertOrder
            && type.IsGenericType
            && (type.GetGenericTypeDefinition() == typeof(IOrderedEnumerable<>)
                || type.GetGenericTypeDefinition() == typeof(IOrderedQueryable<>)))
        {
            throw new InvalidOperationException(
                "Query result is OrderedQueryable - you need to set AssertQuery option: 'assertOrder' to 'true'. If the resulting order is non-deterministic by design, add identity projection to the top of the query to disable this check.");
        }

        if (assertOrder && elementSorter != null)
        {
            throw new InvalidOperationException("You shouldn't apply element sorter when 'assertOrder' is set to 'true'.");
        }
    }

    private readonly Func<DbContext, ISetSource> _setSourceCreator;

    private Func<DbContext, ISetSource> GetSetSourceCreator()
        => context => new DefaultSetSource(context);

    protected void AssertSql(params string[] expected)
        => TestSqlLoggerFactory.AssertBaseline(expected);

    // These are static so that they are shared across tests
    private static IReadOnlyDictionary<Type, object> EntityAsserters { get; }
        = new Dictionary<Type, Action<object, object>>
        {
            {
                typeof(EntityOne), (e, a) =>
                {
                    Assert.Equal(e == null, a == null);
                    if (a != null)
                    {
                        var ee = (EntityOne)e;
                        var aa = (EntityOne)a;

                        Assert.Equal(ee.Id, aa.Id);
                        Assert.Equal(ee.IntValue1, aa.IntValue1);
                        Assert.Equal(ee.IntValue2, aa.IntValue2);
                        Assert.Equal(ee.IntValue3, aa.IntValue3);
                        Assert.Equal(ee.IntValue4, aa.IntValue4);
                        Assert.Equal(ee.StringValue1, aa.StringValue1);
                        Assert.Equal(ee.StringValue2, aa.StringValue2);
                        Assert.Equal(ee.StringValue3, aa.StringValue3);
                        Assert.Equal(ee.StringValue4, aa.StringValue4);
                    }
                }
            },
            {
                typeof(EntityTwo), (e, a) =>
                {
                    Assert.Equal(e == null, a == null);
                    if (a != null)
                    {
                        var ee = (EntityTwo)e;
                        var aa = (EntityTwo)a;

                        Assert.Equal(ee.Id, aa.Id);
                        Assert.Equal(ee.Name, aa.Name);
                    }
                }
            },
            {
                typeof(EntityThree), (e, a) =>
                {
                    Assert.Equal(e == null, a == null);
                    if (a != null)
                    {
                        var ee = (EntityThree)e;
                        var aa = (EntityThree)a;

                        Assert.Equal(ee.Id, aa.Id);
                        Assert.Equal(ee.Name, aa.Name);
                    }
                }
            },
            {
                typeof(BaseEntity), (e, a) =>
                {
                    Assert.Equal(e == null, a == null);
                    if (a != null)
                    {
                        var ee = (BaseEntity)e;
                        var aa = (BaseEntity)a;

                        Assert.Equal(ee.Id, aa.Id);
                        Assert.Equal(ee.BaseValue, aa.BaseValue);
                        if (ee is MiddleEntity me)
                        {
                            var ma = (MiddleEntity)aa;
                            Assert.Equal(me.MiddleValue, ma.MiddleValue);

                            if (ee is LeafEntity le)
                            {
                                var la = (LeafEntity)aa;
                                Assert.Equal(le.LeafValue, la.LeafValue);
                            }
                        }

                        if (ee is SiblingEntity se)
                        {
                            var sa = (SiblingEntity)aa;
                            Assert.Equal(se.SiblingValue, sa.SiblingValue);
                        }
                    }
                }
            },
            {
                typeof(MiddleEntity), (e, a) =>
                {
                    Assert.Equal(e == null, a == null);
                    if (a != null)
                    {
                        var ee = (MiddleEntity)e;
                        var aa = (MiddleEntity)a;

                        Assert.Equal(ee.Id, aa.Id);
                        Assert.Equal(ee.BaseValue, aa.BaseValue);
                        Assert.Equal(ee.MiddleValue, aa.MiddleValue);

                        if (ee is LeafEntity le)
                        {
                            var la = (LeafEntity)aa;
                            Assert.Equal(le.LeafValue, la.LeafValue);
                        }
                    }
                }
            },
            {
                typeof(SiblingEntity), (e, a) =>
                {
                    Assert.Equal(e == null, a == null);
                    if (a != null)
                    {
                        var ee = (SiblingEntity)e;
                        var aa = (SiblingEntity)a;

                        Assert.Equal(ee.Id, aa.Id);
                        Assert.Equal(ee.BaseValue, aa.BaseValue);
                        Assert.Equal(ee.SiblingValue, aa.SiblingValue);
                    }
                }
            },
            {
                typeof(LeafEntity), (e, a) =>
                {
                    Assert.Equal(e == null, a == null);
                    if (a != null)
                    {
                        var ee = (LeafEntity)e;
                        var aa = (LeafEntity)a;

                        Assert.Equal(ee.Id, aa.Id);
                        Assert.Equal(ee.BaseValue, aa.BaseValue);
                        Assert.Equal(ee.MiddleValue, aa.MiddleValue);
                        Assert.Equal(ee.LeafValue, aa.LeafValue);
                    }
                }
            },
            {
                typeof(OwnedReference), (e, a) =>
                {
                    Assert.Equal(e == null, a == null);
                    if (a != null)
                    {
                        var ee = (OwnedReference)e;
                        var aa = (OwnedReference)a;

                        Assert.Equal(ee.Id, aa.Id);
                        Assert.Equal(ee.OwnedIntValue1, aa.OwnedIntValue1);
                        Assert.Equal(ee.OwnedIntValue2, aa.OwnedIntValue2);
                        Assert.Equal(ee.OwnedIntValue3, aa.OwnedIntValue3);
                        Assert.Equal(ee.OwnedIntValue4, aa.OwnedIntValue4);
                        Assert.Equal(ee.OwnedStringValue1, aa.OwnedStringValue1);
                        Assert.Equal(ee.OwnedStringValue2, aa.OwnedStringValue2);
                        Assert.Equal(ee.OwnedStringValue3, aa.OwnedStringValue3);
                        Assert.Equal(ee.OwnedStringValue4, aa.OwnedStringValue4);
                    }
                }
            },
            {
                typeof(OwnedNestedReference), (e, a) =>
                {
                    Assert.Equal(e == null, a == null);
                    if (a != null)
                    {
                        var ee = (OwnedNestedReference)e;
                        var aa = (OwnedNestedReference)a;

                        Assert.Equal(ee.Id, aa.Id);
                        Assert.Equal(ee.OwnedNestedIntValue1, aa.OwnedNestedIntValue1);
                        Assert.Equal(ee.OwnedNestedIntValue2, aa.OwnedNestedIntValue2);
                        Assert.Equal(ee.OwnedNestedIntValue3, aa.OwnedNestedIntValue3);
                        Assert.Equal(ee.OwnedNestedIntValue4, aa.OwnedNestedIntValue4);
                        Assert.Equal(ee.OwnedNestedStringValue1, aa.OwnedNestedStringValue1);
                        Assert.Equal(ee.OwnedNestedStringValue2, aa.OwnedNestedStringValue2);
                        Assert.Equal(ee.OwnedNestedStringValue3, aa.OwnedNestedStringValue3);
                        Assert.Equal(ee.OwnedNestedStringValue4, aa.OwnedNestedStringValue4);
                    }
                }
            },
            {
                typeof(OwnedCollection), (e, a) =>
                {
                    Assert.Equal(e == null, a == null);
                    if (a != null)
                    {
                        var ee = (OwnedCollection)e;
                        var aa = (OwnedCollection)a;

                        Assert.Equal(ee.Id, aa.Id);
                        Assert.Equal(ee.OwnedIntValue1, aa.OwnedIntValue1);
                        Assert.Equal(ee.OwnedIntValue2, aa.OwnedIntValue2);
                        Assert.Equal(ee.OwnedStringValue1, aa.OwnedStringValue1);
                        Assert.Equal(ee.OwnedStringValue2, aa.OwnedStringValue2);
                    }
                }
            },
        }.ToDictionary(e => e.Key, e => (object)e.Value);

    private static IReadOnlyDictionary<Type, object> EntitySorters { get; }
        = new Dictionary<Type, Func<object, object>>
        {
            { typeof(EntityOne), e => ((EntityOne)e)?.Id },
            { typeof(EntityTwo), e => ((EntityTwo)e)?.Id },
            { typeof(EntityThree), e => ((EntityThree)e)?.Id },
            { typeof(BaseEntity), e => ((BaseEntity)e)?.Id },
            { typeof(OwnedReference), e => ((OwnedReference)e)?.Id },
            { typeof(OwnedCollection), e => ((OwnedCollection)e)?.Id },
            { typeof(OwnedNestedReference), e => ((OwnedNestedReference)e)?.Id },
        }.ToDictionary(e => e.Key, e => (object)e.Value);

    protected virtual ISetSource GetExpectedData()
        => EntitySplittingData.Instance;

    private class DefaultSetSource(DbContext context) : ISetSource
    {
        private readonly DbContext _context = context;

        public IQueryable<TEntity> Set<TEntity>()
            where TEntity : class
            => _context.Set<TEntity>();
    }

    #endregion

    #region Fixture

    protected async Task InitializeContextFactoryAsync(Action<ModelBuilder> onModelCreating)
        => ContextFactory = await InitializeAsync<EntitySplittingContext>(
            mb =>
            {
                OnModelCreating(mb);
                onModelCreating(mb);
            },
            onConfiguring: e => e.ConfigureWarnings(
                wc =>
                {
                    wc.Log(RelationalEventId.ForeignKeyTpcPrincipalWarning);
                }),
            shouldLogCategory: _ => true, seed: c => SeedAsync(c));

    protected virtual EntitySplittingContext CreateContext()
        => ContextFactory.CreateContext();

    public void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
        => facade.UseTransaction(transaction.GetDbTransaction());

    protected override string StoreName
        => "EntitySplittingQueryTest";

    protected TestSqlLoggerFactory TestSqlLoggerFactory
        => (TestSqlLoggerFactory)ListLoggerFactory;

    protected ContextFactory<EntitySplittingContext> ContextFactory { get; private set; }

    protected virtual void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<EntityOne>().Property(e => e.Id).ValueGeneratedNever();
        modelBuilder.Entity<EntityTwo>().Property(e => e.Id).ValueGeneratedNever();
        modelBuilder.Entity<EntityThree>().Property(e => e.Id).ValueGeneratedNever();
        modelBuilder.Entity<BaseEntity>().Property(e => e.Id).ValueGeneratedNever();
        modelBuilder.Entity<MiddleEntity>();
        modelBuilder.Entity<SiblingEntity>();
        modelBuilder.Entity<LeafEntity>();
    }

    protected virtual Task SeedAsync(EntitySplittingContext context)
        => EntitySplittingData.Instance.Seed(context);

    public override async Task DisposeAsync()
    {
        await base.DisposeAsync();

        ContextFactory = null;
    }

    #endregion
}
