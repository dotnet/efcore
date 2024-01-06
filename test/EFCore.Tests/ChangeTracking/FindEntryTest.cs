// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable enable

using System.ComponentModel.DataAnnotations.Schema;

namespace Microsoft.EntityFrameworkCore.ChangeTracking;

public class FindEntryTest
{
    [ConditionalTheory]
    [InlineData(CompositeKeyType.Array)]
    [InlineData(CompositeKeyType.List)]
    [InlineData(CompositeKeyType.Enumerable)]
    public virtual void Find_int_key(CompositeKeyType keyType)
    {
        using var context = new FindContext();
        var entities = new[]
        {
            context.Attach(new IntKey { Id = 87 }).Entity,
            context.Attach(new IntKey { Id = 88 }).Entity,
            context.Attach(new IntKey { Id = 89 }).Entity,
        };

        AssertSingle(context.Set<IntKey>(), nameof(IntKey.Id), entities, 88, 99, keyType, isPk: true);
    }

    [ConditionalTheory]
    [InlineData(CompositeKeyType.Array)]
    [InlineData(CompositeKeyType.List)]
    [InlineData(CompositeKeyType.Enumerable)]
    public virtual void Find_int_alternate_key(CompositeKeyType keyType)
    {
        using var context = new FindContext();
        var entities = new[]
        {
            context.Attach(new AlternateIntKey { Id = Guid.NewGuid(), AlternateId = 87 }).Entity,
            context.Attach(new AlternateIntKey { Id = Guid.NewGuid(), AlternateId = 88 }).Entity,
            context.Attach(new AlternateIntKey { Id = Guid.NewGuid(), AlternateId = 89 }).Entity,
        };

        AssertSingle(context.Set<AlternateIntKey>(), nameof(AlternateIntKey.AlternateId), entities, 88, 99, keyType);
    }

    [ConditionalTheory]
    [InlineData(CompositeKeyType.Array, true)]
    [InlineData(CompositeKeyType.List, true)]
    [InlineData(CompositeKeyType.Enumerable, true)]
    [InlineData(CompositeKeyType.Array, false)]
    [InlineData(CompositeKeyType.List, false)]
    [InlineData(CompositeKeyType.Enumerable, false)]
    public virtual void Find_int_foreign_key(CompositeKeyType keyType, bool trackPrincipal)
    {
        using var context = new FindContext();

        if (trackPrincipal)
        {
            context.Attach(new IntKey { Id = 88 });
        }

        var entities = new[]
        {
            context.Attach(new ForeignIntKey { Id = Guid.NewGuid(), IntKeyId = 87 }).Entity,
            context.Attach(new ForeignIntKey { Id = Guid.NewGuid(), IntKeyId = 88 }).Entity,
            context.Attach(new ForeignIntKey { Id = Guid.NewGuid(), IntKeyId = 88 }).Entity,
            context.Attach(new ForeignIntKey { Id = Guid.NewGuid(), IntKeyId = 89 }).Entity,
        };

        AssertMultiple(context.Set<ForeignIntKey>(), nameof(ForeignIntKey.IntKeyId), entities, 88, 99, keyType);
    }

    [ConditionalTheory]
    [InlineData(CompositeKeyType.Array)]
    [InlineData(CompositeKeyType.List)]
    [InlineData(CompositeKeyType.Enumerable)]
    public virtual void Find_int_non_key(CompositeKeyType keyType)
    {
        using var context = new FindContext();
        var entities = new[]
        {
            context.Attach(new IntNonKey { Id = Guid.NewGuid(), Int = 87 }).Entity,
            context.Attach(new IntNonKey { Id = Guid.NewGuid(), Int = 88 }).Entity,
            context.Attach(new IntNonKey { Id = Guid.NewGuid(), Int = 88 }).Entity,
            context.Attach(new IntNonKey { Id = Guid.NewGuid(), Int = 89 }).Entity,
        };

        AssertMultiple(context.Set<IntNonKey>(), nameof(IntNonKey.Int), entities, 88, 99, keyType);
    }

    [ConditionalTheory]
    [InlineData(CompositeKeyType.Array)]
    [InlineData(CompositeKeyType.List)]
    [InlineData(CompositeKeyType.Enumerable)]
    public virtual void Find_shadow_int_key(CompositeKeyType keyType)
    {
        using var context = new FindContext();
        var entities = new[]
        {
            CreateShadowEntity<ShadowIntKey>(context, "Id", 87),
            CreateShadowEntity<ShadowIntKey>(context, "Id", 88),
            CreateShadowEntity<ShadowIntKey>(context, "Id", 89),
        };

        AssertSingle(context.Set<ShadowIntKey>(), "Id", entities, 88, 99, keyType, isPk: true);
    }

    [ConditionalTheory]
    [InlineData(CompositeKeyType.Array)]
    [InlineData(CompositeKeyType.List)]
    [InlineData(CompositeKeyType.Enumerable)]
    public virtual void Find_shadow_alternate_int_key(CompositeKeyType keyType)
    {
        using var context = new FindContext();
        var entities = new[]
        {
            CreateShadowEntity<ShadowAlternateIntKey>(context, "AlternateId", 87),
            CreateShadowEntity<ShadowAlternateIntKey>(context, "AlternateId", 88),
            CreateShadowEntity<ShadowAlternateIntKey>(context, "AlternateId", 89),
        };

        AssertSingle(context.Set<ShadowAlternateIntKey>(), "AlternateId", entities, 88, 99, keyType);
    }

    [ConditionalTheory]
    [InlineData(CompositeKeyType.Array, true)]
    [InlineData(CompositeKeyType.List, true)]
    [InlineData(CompositeKeyType.Enumerable, true)]
    [InlineData(CompositeKeyType.Array, false)]
    [InlineData(CompositeKeyType.List, false)]
    [InlineData(CompositeKeyType.Enumerable, false)]
    public virtual void Find_shadow_int_foreign_key(CompositeKeyType keyType, bool trackPrincipal)
    {
        using var context = new FindContext();

        if (trackPrincipal)
        {
            CreateShadowEntity<ShadowIntKey>(context, "Id", 88);
        }

        var entities = new[]
        {
            CreateShadowEntity<ShadowForeignIntKey>(context, "IntKeyId", 87),
            CreateShadowEntity<ShadowForeignIntKey>(context, "IntKeyId", 88),
            CreateShadowEntity<ShadowForeignIntKey>(context, "IntKeyId", 88),
            CreateShadowEntity<ShadowForeignIntKey>(context, "IntKeyId", 89),
        };

        AssertMultiple(context.Set<ShadowForeignIntKey>(), "IntKeyId", entities, (int?)88, 99, keyType);
    }

    [ConditionalTheory]
    [InlineData(CompositeKeyType.Array)]
    [InlineData(CompositeKeyType.List)]
    [InlineData(CompositeKeyType.Enumerable)]
    public virtual void Find_shadow_int_non_key(CompositeKeyType keyType)
    {
        using var context = new FindContext();
        var entities = new[]
        {
            CreateShadowEntity<ShadowIntNonKey>(context, "Int", 87),
            CreateShadowEntity<ShadowIntNonKey>(context, "Int", 88),
            CreateShadowEntity<ShadowIntNonKey>(context, "Int", 88),
            CreateShadowEntity<ShadowIntNonKey>(context, "Int", 89),
        };

        AssertMultiple(context.Set<ShadowIntNonKey>(), "Int", entities, 88, 99, keyType);
    }

    [ConditionalTheory]
    [InlineData(CompositeKeyType.Array)]
    [InlineData(CompositeKeyType.List)]
    [InlineData(CompositeKeyType.Enumerable)]
    public virtual void Find_int_key_shared(CompositeKeyType keyType)
    {
        using var context = new FindContextShared();
        var setA = context.Set<IntKey>("IntKeyA");
        var setB = context.Set<IntKey>("IntKeyB");

        var entities = new[]
        {
            setA.Attach(new IntKey { Id = 87 }).Entity,
            setA.Attach(new IntKey { Id = 88 }).Entity,
            setA.Attach(new IntKey { Id = 89 }).Entity,
            setB.Attach(new IntKey { Id = 87 }).Entity,
            setB.Attach(new IntKey { Id = 88 }).Entity,
            setB.Attach(new IntKey { Id = 89 }).Entity,
        };

        AssertSingle(setA, nameof(IntKey.Id), entities, 88, 99, keyType, isPk: true);
        AssertSingle(setB, nameof(IntKey.Id), entities, 88, 99, keyType, isPk: true, index: 4);
    }

    [ConditionalTheory]
    [InlineData(CompositeKeyType.Array)]
    [InlineData(CompositeKeyType.List)]
    [InlineData(CompositeKeyType.Enumerable)]
    public virtual void Find_int_alternate_key_shared(CompositeKeyType keyType)
    {
        using var context = new FindContextShared();
        var setA = context.Set<AlternateIntKey>("AlternateIntKeyA");
        var setB = context.Set<AlternateIntKey>("AlternateIntKeyB");

        var entities = new[]
        {
            setA.Attach(new AlternateIntKey { Id = Guid.NewGuid(), AlternateId = 87 }).Entity,
            setA.Attach(new AlternateIntKey { Id = Guid.NewGuid(), AlternateId = 88 }).Entity,
            setA.Attach(new AlternateIntKey { Id = Guid.NewGuid(), AlternateId = 89 }).Entity,
            setB.Attach(new AlternateIntKey { Id = Guid.NewGuid(), AlternateId = 87 }).Entity,
            setB.Attach(new AlternateIntKey { Id = Guid.NewGuid(), AlternateId = 88 }).Entity,
            setB.Attach(new AlternateIntKey { Id = Guid.NewGuid(), AlternateId = 89 }).Entity,
        };

        AssertSingle(setA, nameof(AlternateIntKey.AlternateId), entities, 88, 99, keyType);
        AssertSingle(setB, nameof(AlternateIntKey.AlternateId), entities, 88, 99, keyType, index: 4);
    }

    [ConditionalTheory]
    [InlineData(CompositeKeyType.Array, true)]
    [InlineData(CompositeKeyType.List, true)]
    [InlineData(CompositeKeyType.Enumerable, true)]
    [InlineData(CompositeKeyType.Array, false)]
    [InlineData(CompositeKeyType.List, false)]
    [InlineData(CompositeKeyType.Enumerable, false)]
    public virtual void Find_int_foreign_key_shared(CompositeKeyType keyType, bool trackPrincipal)
    {
        using var context = new FindContextShared();

        if (trackPrincipal)
        {
            context.Set<IntKey>("IntKeyA").Attach(new IntKey { Id = 88 });
            context.Set<IntKey>("IntKeyB").Attach(new IntKey { Id = 88 });
        }

        var setA = context.Set<ForeignIntKey>("ForeignIntKeyA");
        var setB = context.Set<ForeignIntKey>("ForeignIntKeyB");
        var entities = new[]
        {
            setA.Attach(new ForeignIntKey { Id = Guid.NewGuid(), IntKeyId = 87 }).Entity,
            setA.Attach(new ForeignIntKey { Id = Guid.NewGuid(), IntKeyId = 88 }).Entity,
            setA.Attach(new ForeignIntKey { Id = Guid.NewGuid(), IntKeyId = 88 }).Entity,
            setA.Attach(new ForeignIntKey { Id = Guid.NewGuid(), IntKeyId = 89 }).Entity,
            setB.Attach(new ForeignIntKey { Id = Guid.NewGuid(), IntKeyId = 87 }).Entity,
            setB.Attach(new ForeignIntKey { Id = Guid.NewGuid(), IntKeyId = 88 }).Entity,
            setB.Attach(new ForeignIntKey { Id = Guid.NewGuid(), IntKeyId = 88 }).Entity,
            setB.Attach(new ForeignIntKey { Id = Guid.NewGuid(), IntKeyId = 89 }).Entity,
        };

        AssertMultiple(setA, nameof(ForeignIntKey.IntKeyId), entities, 88, 99, keyType);
        AssertMultiple(setB, nameof(ForeignIntKey.IntKeyId), entities, 88, 99, keyType, index: 5);
    }

    [ConditionalTheory]
    [InlineData(CompositeKeyType.Array)]
    [InlineData(CompositeKeyType.List)]
    [InlineData(CompositeKeyType.Enumerable)]
    public virtual void Find_int_non_key_shared(CompositeKeyType keyType)
    {
        using var context = new FindContextShared();
        var setA = context.Set<IntNonKey>("IntNonKeyA");
        var setB = context.Set<IntNonKey>("IntNonKeyB");

        var entities = new[]
        {
            setA.Attach(new IntNonKey { Id = Guid.NewGuid(), Int = 87 }).Entity,
            setA.Attach(new IntNonKey { Id = Guid.NewGuid(), Int = 88 }).Entity,
            setA.Attach(new IntNonKey { Id = Guid.NewGuid(), Int = 88 }).Entity,
            setA.Attach(new IntNonKey { Id = Guid.NewGuid(), Int = 89 }).Entity,
            setB.Attach(new IntNonKey { Id = Guid.NewGuid(), Int = 87 }).Entity,
            setB.Attach(new IntNonKey { Id = Guid.NewGuid(), Int = 88 }).Entity,
            setB.Attach(new IntNonKey { Id = Guid.NewGuid(), Int = 88 }).Entity,
            setB.Attach(new IntNonKey { Id = Guid.NewGuid(), Int = 89 }).Entity,
        };

        AssertMultiple(setA, nameof(IntNonKey.Int), entities, 88, 99, keyType);
        AssertMultiple(setB, nameof(IntNonKey.Int), entities, 88, 99, keyType, index: 5);
    }

    [ConditionalTheory]
    [InlineData(CompositeKeyType.Array)]
    [InlineData(CompositeKeyType.List)]
    [InlineData(CompositeKeyType.Enumerable)]
    public virtual void Find_shadow_int_key_shared(CompositeKeyType keyType)
    {
        using var context = new FindContextShared();
        var entities = new[]
        {
            CreateShadowEntityShared<ShadowIntKey>(context, "ShadowIntKeyA", "Id", 87),
            CreateShadowEntityShared<ShadowIntKey>(context, "ShadowIntKeyA", "Id", 88),
            CreateShadowEntityShared<ShadowIntKey>(context, "ShadowIntKeyA", "Id", 89),
            CreateShadowEntityShared<ShadowIntKey>(context, "ShadowIntKeyB", "Id", 87),
            CreateShadowEntityShared<ShadowIntKey>(context, "ShadowIntKeyB", "Id", 88),
            CreateShadowEntityShared<ShadowIntKey>(context, "ShadowIntKeyB", "Id", 89),
        };

        AssertSingle(context.Set<ShadowIntKey>("ShadowIntKeyA"), "Id", entities, 88, 99, keyType, isPk: true);
        AssertSingle(context.Set<ShadowIntKey>("ShadowIntKeyB"), "Id", entities, 88, 99, keyType, isPk: true, index: 4);
    }

    [ConditionalTheory]
    [InlineData(CompositeKeyType.Array)]
    [InlineData(CompositeKeyType.List)]
    [InlineData(CompositeKeyType.Enumerable)]
    public virtual void Find_shadow_alternate_int_key_shared(CompositeKeyType keyType)
    {
        using var context = new FindContextShared();
        var entities = new[]
        {
            CreateShadowEntityShared<ShadowAlternateIntKey>(context, "ShadowAlternateIntKeyA", "AlternateId", 87),
            CreateShadowEntityShared<ShadowAlternateIntKey>(context, "ShadowAlternateIntKeyA", "AlternateId", 88),
            CreateShadowEntityShared<ShadowAlternateIntKey>(context, "ShadowAlternateIntKeyA", "AlternateId", 89),
            CreateShadowEntityShared<ShadowAlternateIntKey>(context, "ShadowAlternateIntKeyB", "AlternateId", 87),
            CreateShadowEntityShared<ShadowAlternateIntKey>(context, "ShadowAlternateIntKeyB", "AlternateId", 88),
            CreateShadowEntityShared<ShadowAlternateIntKey>(context, "ShadowAlternateIntKeyB", "AlternateId", 89),
        };

        AssertSingle(context.Set<ShadowAlternateIntKey>("ShadowAlternateIntKeyA"), "AlternateId", entities, 88, 99, keyType);
        AssertSingle(context.Set<ShadowAlternateIntKey>("ShadowAlternateIntKeyB"), "AlternateId", entities, 88, 99, keyType, index: 4);
    }

    [ConditionalTheory]
    [InlineData(CompositeKeyType.Array, true)]
    [InlineData(CompositeKeyType.List, true)]
    [InlineData(CompositeKeyType.Enumerable, true)]
    [InlineData(CompositeKeyType.Array, false)]
    [InlineData(CompositeKeyType.List, false)]
    [InlineData(CompositeKeyType.Enumerable, false)]
    public virtual void Find_shadow_int_foreign_key_shared(CompositeKeyType keyType, bool trackPrincipal)
    {
        using var context = new FindContextShared();

        if (trackPrincipal)
        {
            CreateShadowEntityShared<ShadowIntKey>(context, "ShadowIntKeyA", "Id", 88);
            CreateShadowEntityShared<ShadowIntKey>(context, "ShadowIntKeyB", "Id", 88);
        }

        var entities = new[]
        {
            CreateShadowEntityShared<ShadowForeignIntKey>(context, "ShadowForeignIntKeyA", "IntKeyId", 87),
            CreateShadowEntityShared<ShadowForeignIntKey>(context, "ShadowForeignIntKeyA", "IntKeyId", 88),
            CreateShadowEntityShared<ShadowForeignIntKey>(context, "ShadowForeignIntKeyA", "IntKeyId", 88),
            CreateShadowEntityShared<ShadowForeignIntKey>(context, "ShadowForeignIntKeyA", "IntKeyId", 89),
            CreateShadowEntityShared<ShadowForeignIntKey>(context, "ShadowForeignIntKeyB", "IntKeyId", 87),
            CreateShadowEntityShared<ShadowForeignIntKey>(context, "ShadowForeignIntKeyB", "IntKeyId", 88),
            CreateShadowEntityShared<ShadowForeignIntKey>(context, "ShadowForeignIntKeyB", "IntKeyId", 88),
            CreateShadowEntityShared<ShadowForeignIntKey>(context, "ShadowForeignIntKeyB", "IntKeyId", 89),
        };

        AssertMultiple(context.Set<ShadowForeignIntKey>("ShadowForeignIntKeyA"), "IntKeyId", entities, (int?)88, 99, keyType);
        AssertMultiple(context.Set<ShadowForeignIntKey>("ShadowForeignIntKeyB"), "IntKeyId", entities, (int?)88, 99, keyType, index: 5);
    }

    [ConditionalTheory]
    [InlineData(CompositeKeyType.Array)]
    [InlineData(CompositeKeyType.List)]
    [InlineData(CompositeKeyType.Enumerable)]
    public virtual void Find_shadow_int_non_key_shared(CompositeKeyType keyType)
    {
        using var context = new FindContextShared();
        var entities = new[]
        {
            CreateShadowEntityShared<ShadowIntNonKey>(context, "ShadowIntNonKeyA", "Int", 87),
            CreateShadowEntityShared<ShadowIntNonKey>(context, "ShadowIntNonKeyA", "Int", 88),
            CreateShadowEntityShared<ShadowIntNonKey>(context, "ShadowIntNonKeyA", "Int", 88),
            CreateShadowEntityShared<ShadowIntNonKey>(context, "ShadowIntNonKeyA", "Int", 89),
            CreateShadowEntityShared<ShadowIntNonKey>(context, "ShadowIntNonKeyB", "Int", 87),
            CreateShadowEntityShared<ShadowIntNonKey>(context, "ShadowIntNonKeyB", "Int", 88),
            CreateShadowEntityShared<ShadowIntNonKey>(context, "ShadowIntNonKeyB", "Int", 88),
            CreateShadowEntityShared<ShadowIntNonKey>(context, "ShadowIntNonKeyB", "Int", 89),
        };

        AssertMultiple(context.Set<ShadowIntNonKey>("ShadowIntNonKeyA"), "Int", entities, 88, 99, keyType);
        AssertMultiple(context.Set<ShadowIntNonKey>("ShadowIntNonKeyB"), "Int", entities, 88, 99, keyType, index: 5);
    }

    [ConditionalTheory]
    [InlineData(CompositeKeyType.Array)]
    [InlineData(CompositeKeyType.List)]
    [InlineData(CompositeKeyType.Enumerable)]
    public virtual void Find_nullable_int_key(CompositeKeyType keyType)
    {
        using var context = new FindContext();
        var entities = new[]
        {
            context.Attach(new NullableIntKey { Id = 87 }).Entity,
            context.Attach(new NullableIntKey { Id = 88 }).Entity,
            context.Attach(new NullableIntKey { Id = 89 }).Entity,
        };

        AssertSingle(context.Set<NullableIntKey>(), nameof(IntKey.Id), entities, (int?)88, 99, keyType, isPk: true);
    }

    [ConditionalTheory]
    [InlineData(CompositeKeyType.Array)]
    [InlineData(CompositeKeyType.List)]
    [InlineData(CompositeKeyType.Enumerable)]
    public virtual void Find_nullable_int_alternate_key(CompositeKeyType keyType)
    {
        using var context = new FindContext();
        var entities = new[]
        {
            context.Attach(new AlternateNullableIntKey { Id = Guid.NewGuid(), AlternateId = 87 }).Entity,
            context.Attach(new AlternateNullableIntKey { Id = Guid.NewGuid(), AlternateId = 88 }).Entity,
            context.Attach(new AlternateNullableIntKey { Id = Guid.NewGuid(), AlternateId = 89 }).Entity,
        };

        AssertSingle(context.Set<AlternateNullableIntKey>(), nameof(AlternateNullableIntKey.AlternateId), entities, (int?)88, 99, keyType);
    }

    [ConditionalTheory]
    [InlineData(CompositeKeyType.Array, true)]
    [InlineData(CompositeKeyType.List, true)]
    [InlineData(CompositeKeyType.Enumerable, true)]
    [InlineData(CompositeKeyType.Array, false)]
    [InlineData(CompositeKeyType.List, false)]
    [InlineData(CompositeKeyType.Enumerable, false)]
    public virtual void Find_nullable_int_foreign_key(CompositeKeyType keyType, bool trackPrincipal)
    {
        using var context = new FindContext();

        if (trackPrincipal)
        {
            context.Attach(new NullableIntKey { Id = 88 });
        }

        var entities = new[]
        {
            context.Attach(new ForeignNullableIntKey { Id = Guid.NewGuid(), NullableIntKeyId = 87 }).Entity,
            context.Attach(new ForeignNullableIntKey { Id = Guid.NewGuid(), NullableIntKeyId = 88 }).Entity,
            context.Attach(new ForeignNullableIntKey { Id = Guid.NewGuid(), NullableIntKeyId = 88 }).Entity,
            context.Attach(new ForeignNullableIntKey { Id = Guid.NewGuid(), NullableIntKeyId = 89 }).Entity,
        };

        AssertMultiple(
            context.Set<ForeignNullableIntKey>(), nameof(ForeignNullableIntKey.NullableIntKeyId), entities, (int?)88, 99, keyType);
    }

    [ConditionalTheory]
    [InlineData(CompositeKeyType.Array)]
    [InlineData(CompositeKeyType.List)]
    [InlineData(CompositeKeyType.Enumerable)]
    public virtual void Find_nullable_int_non_key(CompositeKeyType keyType)
    {
        using var context = new FindContext();
        var entities = new[]
        {
            context.Attach(new NullableIntNonKey { Id = Guid.NewGuid(), NullableInt = 87 }).Entity,
            context.Attach(new NullableIntNonKey { Id = Guid.NewGuid(), NullableInt = 88 }).Entity,
            context.Attach(new NullableIntNonKey { Id = Guid.NewGuid(), NullableInt = 88 }).Entity,
            context.Attach(new NullableIntNonKey { Id = Guid.NewGuid(), NullableInt = 89 }).Entity,
        };

        AssertMultiple(context.Set<NullableIntNonKey>(), nameof(NullableIntNonKey.NullableInt), entities, (int?)88, 99, keyType);
    }

    [ConditionalTheory]
    [InlineData(CompositeKeyType.Array)]
    [InlineData(CompositeKeyType.List)]
    [InlineData(CompositeKeyType.Enumerable)]
    public virtual void Find_shadow_nullable_int_key(CompositeKeyType keyType)
    {
        using var context = new FindContext();
        var entities = new[]
        {
            CreateShadowEntity<ShadowNullableIntKey>(context, "Id", 87),
            CreateShadowEntity<ShadowNullableIntKey>(context, "Id", 88),
            CreateShadowEntity<ShadowNullableIntKey>(context, "Id", 89),
        };

        AssertSingle(context.Set<ShadowNullableIntKey>(), "Id", entities, (int?)88, 99, keyType, isPk: true);
    }

    [ConditionalTheory]
    [InlineData(CompositeKeyType.Array)]
    [InlineData(CompositeKeyType.List)]
    [InlineData(CompositeKeyType.Enumerable)]
    public virtual void Find_shadow_alternate_nullable_int_key(CompositeKeyType keyType)
    {
        using var context = new FindContext();
        var entities = new[]
        {
            CreateShadowEntity<ShadowAlternateNullableIntKey>(context, "AlternateId", 87),
            CreateShadowEntity<ShadowAlternateNullableIntKey>(context, "AlternateId", 88),
            CreateShadowEntity<ShadowAlternateNullableIntKey>(context, "AlternateId", 89),
        };

        AssertSingle(context.Set<ShadowAlternateNullableIntKey>(), "AlternateId", entities, (int?)88, 99, keyType);
    }

    [ConditionalTheory]
    [InlineData(CompositeKeyType.Array, true)]
    [InlineData(CompositeKeyType.List, true)]
    [InlineData(CompositeKeyType.Enumerable, true)]
    [InlineData(CompositeKeyType.Array, false)]
    [InlineData(CompositeKeyType.List, false)]
    [InlineData(CompositeKeyType.Enumerable, false)]
    public virtual void Find_shadow_nullable_int_foreign_key(CompositeKeyType keyType, bool trackPrincipal)
    {
        using var context = new FindContext();

        if (trackPrincipal)
        {
            context.Attach(new NullableIntKey { Id = 88 });
        }

        var entities = new[]
        {
            CreateShadowEntity<ShadowForeignNullableIntKey>(context, "NullableIntKeyId", 87),
            CreateShadowEntity<ShadowForeignNullableIntKey>(context, "NullableIntKeyId", 88),
            CreateShadowEntity<ShadowForeignNullableIntKey>(context, "NullableIntKeyId", 88),
            CreateShadowEntity<ShadowForeignNullableIntKey>(context, "NullableIntKeyId", 89),
        };

        AssertMultiple(context.Set<ShadowForeignNullableIntKey>(), "NullableIntKeyId", entities, (int?)88, 99, keyType);
    }

    [ConditionalTheory]
    [InlineData(CompositeKeyType.Array)]
    [InlineData(CompositeKeyType.List)]
    [InlineData(CompositeKeyType.Enumerable)]
    public virtual void Find_shadow_nullable_int_non_key(CompositeKeyType keyType)
    {
        using var context = new FindContext();
        var entities = new[]
        {
            CreateShadowEntity<ShadowNullableIntNonKey>(context, "NullableInt", 87),
            CreateShadowEntity<ShadowNullableIntNonKey>(context, "NullableInt", 88),
            CreateShadowEntity<ShadowNullableIntNonKey>(context, "NullableInt", 88),
            CreateShadowEntity<ShadowNullableIntNonKey>(context, "NullableInt", 89),
        };

        AssertMultiple(context.Set<ShadowNullableIntNonKey>(), "NullableInt", entities, (int?)88, 99, keyType);
    }

    [ConditionalTheory]
    [InlineData(CompositeKeyType.Array)]
    [InlineData(CompositeKeyType.List)]
    [InlineData(CompositeKeyType.Enumerable)]
    public virtual void Find_nullable_int_foreign_key_with_nulls(CompositeKeyType keyType)
    {
        using var context = new FindContext();

        var entities = new[]
        {
            context.Attach(new ForeignNullableIntKey { Id = Guid.NewGuid(), NullableIntKeyId = 87 }).Entity,
            context.Attach(new ForeignNullableIntKey { Id = Guid.NewGuid(), NullableIntKeyId = null }).Entity,
            context.Attach(new ForeignNullableIntKey { Id = Guid.NewGuid(), NullableIntKeyId = null }).Entity,
            context.Attach(new ForeignNullableIntKey { Id = Guid.NewGuid(), NullableIntKeyId = 89 }).Entity,
        };

        AssertMultiple(
            context.Set<ForeignNullableIntKey>(), nameof(ForeignNullableIntKey.NullableIntKeyId), entities, (int?)null, 99, keyType);
    }

    [ConditionalTheory]
    [InlineData(CompositeKeyType.Array)]
    [InlineData(CompositeKeyType.List)]
    [InlineData(CompositeKeyType.Enumerable)]
    public virtual void Find_nullable_int_non_key_with_nulls(CompositeKeyType keyType)
    {
        using var context = new FindContext();
        var entities = new[]
        {
            context.Attach(new NullableIntNonKey { Id = Guid.NewGuid(), NullableInt = 87 }).Entity,
            context.Attach(new NullableIntNonKey { Id = Guid.NewGuid(), NullableInt = null }).Entity,
            context.Attach(new NullableIntNonKey { Id = Guid.NewGuid(), NullableInt = null }).Entity,
            context.Attach(new NullableIntNonKey { Id = Guid.NewGuid(), NullableInt = 89 }).Entity,
        };

        AssertMultiple(context.Set<NullableIntNonKey>(), nameof(NullableIntNonKey.NullableInt), entities, (int?)null, 99, keyType);
    }

    [ConditionalTheory]
    [InlineData(CompositeKeyType.Array)]
    [InlineData(CompositeKeyType.List)]
    [InlineData(CompositeKeyType.Enumerable)]
    public virtual void Find_shadow_nullable_int_foreign_key_with_nulls(CompositeKeyType keyType)
    {
        using var context = new FindContext();

        var entities = new[]
        {
            CreateShadowEntity<ShadowForeignNullableIntKey>(context, "NullableIntKeyId", 87),
            CreateShadowEntity<ShadowForeignNullableIntKey>(context, "NullableIntKeyId", null),
            CreateShadowEntity<ShadowForeignNullableIntKey>(context, "NullableIntKeyId", null),
            CreateShadowEntity<ShadowForeignNullableIntKey>(context, "NullableIntKeyId", 89),
        };

        AssertMultiple(context.Set<ShadowForeignNullableIntKey>(), "NullableIntKeyId", entities, (int?)null, 99, keyType);
    }

    [ConditionalTheory]
    [InlineData(CompositeKeyType.Array)]
    [InlineData(CompositeKeyType.List)]
    [InlineData(CompositeKeyType.Enumerable)]
    public virtual void Find_shadow_nullable_int_non_key_with_nulls(CompositeKeyType keyType)
    {
        using var context = new FindContext();
        var entities = new[]
        {
            CreateShadowEntity<ShadowNullableIntNonKey>(context, "NullableInt", 87),
            CreateShadowEntity<ShadowNullableIntNonKey>(context, "NullableInt", null),
            CreateShadowEntity<ShadowNullableIntNonKey>(context, "NullableInt", null),
            CreateShadowEntity<ShadowNullableIntNonKey>(context, "NullableInt", 89),
        };

        AssertMultiple(context.Set<ShadowNullableIntNonKey>(), "NullableInt", entities, (int?)null, 99, keyType);
    }

    [ConditionalTheory]
    [InlineData(CompositeKeyType.Array)]
    [InlineData(CompositeKeyType.List)]
    [InlineData(CompositeKeyType.Enumerable)]
    public virtual void Find_nullable_int_key_shared(CompositeKeyType keyType)
    {
        using var context = new FindContextShared();
        var setA = context.Set<NullableIntKey>("NullableIntKeyA");
        var setB = context.Set<NullableIntKey>("NullableIntKeyB");

        var entities = new[]
        {
            setA.Attach(new NullableIntKey { Id = 87 }).Entity,
            setA.Attach(new NullableIntKey { Id = 88 }).Entity,
            setA.Attach(new NullableIntKey { Id = 89 }).Entity,
            setB.Attach(new NullableIntKey { Id = 87 }).Entity,
            setB.Attach(new NullableIntKey { Id = 88 }).Entity,
            setB.Attach(new NullableIntKey { Id = 89 }).Entity,
        };

        AssertSingle(setA, nameof(NullableIntKey.Id), entities, (int?)88, 99, keyType, isPk: true);
        AssertSingle(setB, nameof(NullableIntKey.Id), entities, (int?)88, 99, keyType, isPk: true, index: 4);
    }

    [ConditionalTheory]
    [InlineData(CompositeKeyType.Array)]
    [InlineData(CompositeKeyType.List)]
    [InlineData(CompositeKeyType.Enumerable)]
    public virtual void Find_nullable_int_alternate_key_shared(CompositeKeyType keyType)
    {
        using var context = new FindContextShared();
        var setA = context.Set<AlternateNullableIntKey>("AlternateNullableIntKeyA");
        var setB = context.Set<AlternateNullableIntKey>("AlternateNullableIntKeyB");

        var entities = new[]
        {
            setA.Attach(new AlternateNullableIntKey { Id = Guid.NewGuid(), AlternateId = 87 }).Entity,
            setA.Attach(new AlternateNullableIntKey { Id = Guid.NewGuid(), AlternateId = 88 }).Entity,
            setA.Attach(new AlternateNullableIntKey { Id = Guid.NewGuid(), AlternateId = 89 }).Entity,
            setB.Attach(new AlternateNullableIntKey { Id = Guid.NewGuid(), AlternateId = 87 }).Entity,
            setB.Attach(new AlternateNullableIntKey { Id = Guid.NewGuid(), AlternateId = 88 }).Entity,
            setB.Attach(new AlternateNullableIntKey { Id = Guid.NewGuid(), AlternateId = 89 }).Entity,
        };

        AssertSingle(setA, nameof(AlternateNullableIntKey.AlternateId), entities, (int?)88, 99, keyType);
        AssertSingle(setB, nameof(AlternateNullableIntKey.AlternateId), entities, (int?)88, 99, keyType, index: 4);
    }

    [ConditionalTheory]
    [InlineData(CompositeKeyType.Array, true)]
    [InlineData(CompositeKeyType.List, true)]
    [InlineData(CompositeKeyType.Enumerable, true)]
    [InlineData(CompositeKeyType.Array, false)]
    [InlineData(CompositeKeyType.List, false)]
    [InlineData(CompositeKeyType.Enumerable, false)]
    public virtual void Find_nullable_int_foreign_key_shared(CompositeKeyType keyType, bool trackPrincipal)
    {
        using var context = new FindContextShared();

        if (trackPrincipal)
        {
            context.Set<NullableIntKey>("NullableIntKeyA").Attach(new NullableIntKey { Id = 88 });
            context.Set<NullableIntKey>("NullableIntKeyB").Attach(new NullableIntKey { Id = 88 });
        }

        var setA = context.Set<ForeignNullableIntKey>("ForeignNullableIntKeyA");
        var setB = context.Set<ForeignNullableIntKey>("ForeignNullableIntKeyB");
        var entities = new[]
        {
            setA.Attach(new ForeignNullableIntKey { Id = Guid.NewGuid(), NullableIntKeyId = 87 }).Entity,
            setA.Attach(new ForeignNullableIntKey { Id = Guid.NewGuid(), NullableIntKeyId = 88 }).Entity,
            setA.Attach(new ForeignNullableIntKey { Id = Guid.NewGuid(), NullableIntKeyId = 88 }).Entity,
            setA.Attach(new ForeignNullableIntKey { Id = Guid.NewGuid(), NullableIntKeyId = 89 }).Entity,
            setB.Attach(new ForeignNullableIntKey { Id = Guid.NewGuid(), NullableIntKeyId = 87 }).Entity,
            setB.Attach(new ForeignNullableIntKey { Id = Guid.NewGuid(), NullableIntKeyId = 88 }).Entity,
            setB.Attach(new ForeignNullableIntKey { Id = Guid.NewGuid(), NullableIntKeyId = 88 }).Entity,
            setB.Attach(new ForeignNullableIntKey { Id = Guid.NewGuid(), NullableIntKeyId = 89 }).Entity,
        };

        AssertMultiple(setA, nameof(ForeignNullableIntKey.NullableIntKeyId), entities, (int?)88, 99, keyType);
        AssertMultiple(setB, nameof(ForeignNullableIntKey.NullableIntKeyId), entities, (int?)88, 99, keyType, index: 5);
    }

    [ConditionalTheory]
    [InlineData(CompositeKeyType.Array)]
    [InlineData(CompositeKeyType.List)]
    [InlineData(CompositeKeyType.Enumerable)]
    public virtual void Find_nullable_int_non_key_shared(CompositeKeyType keyType)
    {
        using var context = new FindContextShared();
        var setA = context.Set<NullableIntNonKey>("NullableIntNonKeyA");
        var setB = context.Set<NullableIntNonKey>("NullableIntNonKeyB");

        var entities = new[]
        {
            setA.Attach(new NullableIntNonKey { Id = Guid.NewGuid(), NullableInt = 87 }).Entity,
            setA.Attach(new NullableIntNonKey { Id = Guid.NewGuid(), NullableInt = 88 }).Entity,
            setA.Attach(new NullableIntNonKey { Id = Guid.NewGuid(), NullableInt = 88 }).Entity,
            setA.Attach(new NullableIntNonKey { Id = Guid.NewGuid(), NullableInt = 89 }).Entity,
            setB.Attach(new NullableIntNonKey { Id = Guid.NewGuid(), NullableInt = 87 }).Entity,
            setB.Attach(new NullableIntNonKey { Id = Guid.NewGuid(), NullableInt = 88 }).Entity,
            setB.Attach(new NullableIntNonKey { Id = Guid.NewGuid(), NullableInt = 88 }).Entity,
            setB.Attach(new NullableIntNonKey { Id = Guid.NewGuid(), NullableInt = 89 }).Entity,
        };

        AssertMultiple(setA, nameof(NullableIntNonKey.NullableInt), entities, (int?)88, 99, keyType);
        AssertMultiple(setB, nameof(NullableIntNonKey.NullableInt), entities, (int?)88, 99, keyType, index: 5);
    }

    [ConditionalTheory]
    [InlineData(CompositeKeyType.Array)]
    [InlineData(CompositeKeyType.List)]
    [InlineData(CompositeKeyType.Enumerable)]
    public virtual void Find_shadow_nullable_int_key_shared(CompositeKeyType keyType)
    {
        using var context = new FindContextShared();
        var entities = new[]
        {
            CreateShadowEntityShared<ShadowNullableIntKey>(context, "ShadowNullableIntKeyA", "Id", 87),
            CreateShadowEntityShared<ShadowNullableIntKey>(context, "ShadowNullableIntKeyA", "Id", 88),
            CreateShadowEntityShared<ShadowNullableIntKey>(context, "ShadowNullableIntKeyA", "Id", 89),
            CreateShadowEntityShared<ShadowNullableIntKey>(context, "ShadowNullableIntKeyB", "Id", 87),
            CreateShadowEntityShared<ShadowNullableIntKey>(context, "ShadowNullableIntKeyB", "Id", 88),
            CreateShadowEntityShared<ShadowNullableIntKey>(context, "ShadowNullableIntKeyB", "Id", 89),
        };

        AssertSingle(context.Set<ShadowNullableIntKey>("ShadowNullableIntKeyA"), "Id", entities, (int?)88, 99, keyType, isPk: true);
        AssertSingle(
            context.Set<ShadowNullableIntKey>("ShadowNullableIntKeyB"), "Id", entities, (int?)88, 99, keyType, isPk: true, index: 4);
    }

    [ConditionalTheory]
    [InlineData(CompositeKeyType.Array)]
    [InlineData(CompositeKeyType.List)]
    [InlineData(CompositeKeyType.Enumerable)]
    public virtual void Find_shadow_alternate_nullable_int_key_shared(CompositeKeyType keyType)
    {
        using var context = new FindContextShared();
        var entities = new[]
        {
            CreateShadowEntityShared<ShadowAlternateNullableIntKey>(context, "ShadowAlternateNullableIntKeyA", "AlternateId", 87),
            CreateShadowEntityShared<ShadowAlternateNullableIntKey>(context, "ShadowAlternateNullableIntKeyA", "AlternateId", 88),
            CreateShadowEntityShared<ShadowAlternateNullableIntKey>(context, "ShadowAlternateNullableIntKeyA", "AlternateId", 89),
            CreateShadowEntityShared<ShadowAlternateNullableIntKey>(context, "ShadowAlternateNullableIntKeyB", "AlternateId", 87),
            CreateShadowEntityShared<ShadowAlternateNullableIntKey>(context, "ShadowAlternateNullableIntKeyB", "AlternateId", 88),
            CreateShadowEntityShared<ShadowAlternateNullableIntKey>(context, "ShadowAlternateNullableIntKeyB", "AlternateId", 89),
        };

        AssertSingle(
            context.Set<ShadowAlternateNullableIntKey>("ShadowAlternateNullableIntKeyA"), "AlternateId", entities, (int?)88, 99, keyType);

        AssertSingle(
            context.Set<ShadowAlternateNullableIntKey>("ShadowAlternateNullableIntKeyB"), "AlternateId", entities, (int?)88, 99, keyType,
            index: 4);
    }

    [ConditionalTheory]
    [InlineData(CompositeKeyType.Array, true)]
    [InlineData(CompositeKeyType.List, true)]
    [InlineData(CompositeKeyType.Enumerable, true)]
    [InlineData(CompositeKeyType.Array, false)]
    [InlineData(CompositeKeyType.List, false)]
    [InlineData(CompositeKeyType.Enumerable, false)]
    public virtual void Find_shadow_nullable_int_foreign_key_shared(CompositeKeyType keyType, bool trackPrincipal)
    {
        using var context = new FindContextShared();

        if (trackPrincipal)
        {
            CreateShadowEntityShared<ShadowNullableIntKey>(context, "ShadowNullableIntKeyA", "Id", 88);
            CreateShadowEntityShared<ShadowNullableIntKey>(context, "ShadowNullableIntKeyB", "Id", 88);
        }

        var entities = new[]
        {
            CreateShadowEntityShared<ShadowForeignNullableIntKey>(context, "ShadowForeignNullableIntKeyA", "NullableIntKeyId", 87),
            CreateShadowEntityShared<ShadowForeignNullableIntKey>(context, "ShadowForeignNullableIntKeyA", "NullableIntKeyId", 88),
            CreateShadowEntityShared<ShadowForeignNullableIntKey>(context, "ShadowForeignNullableIntKeyA", "NullableIntKeyId", 88),
            CreateShadowEntityShared<ShadowForeignNullableIntKey>(context, "ShadowForeignNullableIntKeyA", "NullableIntKeyId", 89),
            CreateShadowEntityShared<ShadowForeignNullableIntKey>(context, "ShadowForeignNullableIntKeyB", "NullableIntKeyId", 87),
            CreateShadowEntityShared<ShadowForeignNullableIntKey>(context, "ShadowForeignNullableIntKeyB", "NullableIntKeyId", 88),
            CreateShadowEntityShared<ShadowForeignNullableIntKey>(context, "ShadowForeignNullableIntKeyB", "NullableIntKeyId", 88),
            CreateShadowEntityShared<ShadowForeignNullableIntKey>(context, "ShadowForeignNullableIntKeyB", "NullableIntKeyId", 89),
        };

        AssertMultiple(
            context.Set<ShadowForeignNullableIntKey>("ShadowForeignNullableIntKeyA"), "NullableIntKeyId", entities, (int?)88, 99, keyType);

        AssertMultiple(
            context.Set<ShadowForeignNullableIntKey>("ShadowForeignNullableIntKeyB"), "NullableIntKeyId", entities, (int?)88, 99, keyType,
            index: 5);
    }

    [ConditionalTheory]
    [InlineData(CompositeKeyType.Array)]
    [InlineData(CompositeKeyType.List)]
    [InlineData(CompositeKeyType.Enumerable)]
    public virtual void Find_shadow_nullable_int_non_key_shared(CompositeKeyType keyType)
    {
        using var context = new FindContextShared();
        var entities = new[]
        {
            CreateShadowEntityShared<ShadowNullableIntNonKey>(context, "ShadowNullableIntNonKeyA", "NullableInt", 87),
            CreateShadowEntityShared<ShadowNullableIntNonKey>(context, "ShadowNullableIntNonKeyA", "NullableInt", 88),
            CreateShadowEntityShared<ShadowNullableIntNonKey>(context, "ShadowNullableIntNonKeyA", "NullableInt", 88),
            CreateShadowEntityShared<ShadowNullableIntNonKey>(context, "ShadowNullableIntNonKeyA", "NullableInt", 89),
            CreateShadowEntityShared<ShadowNullableIntNonKey>(context, "ShadowNullableIntNonKeyB", "NullableInt", 87),
            CreateShadowEntityShared<ShadowNullableIntNonKey>(context, "ShadowNullableIntNonKeyB", "NullableInt", 88),
            CreateShadowEntityShared<ShadowNullableIntNonKey>(context, "ShadowNullableIntNonKeyB", "NullableInt", 88),
            CreateShadowEntityShared<ShadowNullableIntNonKey>(context, "ShadowNullableIntNonKeyB", "NullableInt", 89),
        };

        AssertMultiple(
            context.Set<ShadowNullableIntNonKey>("ShadowNullableIntNonKeyA"), "NullableInt", entities, (int?)88, 99, keyType);

        AssertMultiple(
            context.Set<ShadowNullableIntNonKey>("ShadowNullableIntNonKeyB"), "NullableInt", entities, (int?)88, 99, keyType, index: 5);
    }

    [ConditionalTheory]
    [InlineData(CompositeKeyType.Array)]
    [InlineData(CompositeKeyType.List)]
    [InlineData(CompositeKeyType.Enumerable)]
    public virtual void Find_string_key(CompositeKeyType keyType)
    {
        using var context = new FindContext();
        var entities = new[]
        {
            context.Attach(new StringKey { Id = "87" }).Entity,
            context.Attach(new StringKey { Id = "88" }).Entity,
            context.Attach(new StringKey { Id = "89" }).Entity,
        };

        AssertSingle(context.Set<StringKey>(), nameof(StringKey.Id), entities, "88", "99", keyType, isPk: true);
    }

    [ConditionalTheory]
    [InlineData(CompositeKeyType.Array)]
    [InlineData(CompositeKeyType.List)]
    [InlineData(CompositeKeyType.Enumerable)]
    public virtual void Find_string_alternate_key(CompositeKeyType keyType)
    {
        using var context = new FindContext();
        var entities = new[]
        {
            context.Attach(new AlternateStringKey { Id = Guid.NewGuid(), AlternateId = "87" }).Entity,
            context.Attach(new AlternateStringKey { Id = Guid.NewGuid(), AlternateId = "88" }).Entity,
            context.Attach(new AlternateStringKey { Id = Guid.NewGuid(), AlternateId = "89" }).Entity,
        };

        AssertSingle(context.Set<AlternateStringKey>(), nameof(AlternateStringKey.AlternateId), entities, "88", "99", keyType);
    }

    [ConditionalTheory]
    [InlineData(CompositeKeyType.Array, true)]
    [InlineData(CompositeKeyType.List, true)]
    [InlineData(CompositeKeyType.Enumerable, true)]
    [InlineData(CompositeKeyType.Array, false)]
    [InlineData(CompositeKeyType.List, false)]
    [InlineData(CompositeKeyType.Enumerable, false)]
    public virtual void Find_string_foreign_key(CompositeKeyType keyType, bool trackPrincipal)
    {
        using var context = new FindContext();

        if (trackPrincipal)
        {
            context.Attach(new StringKey { Id = "88" });
        }

        var entities = new[]
        {
            context.Attach(new ForeignStringKey { Id = Guid.NewGuid(), StringKeyId = "87" }).Entity,
            context.Attach(new ForeignStringKey { Id = Guid.NewGuid(), StringKeyId = "88" }).Entity,
            context.Attach(new ForeignStringKey { Id = Guid.NewGuid(), StringKeyId = "88" }).Entity,
            context.Attach(new ForeignStringKey { Id = Guid.NewGuid(), StringKeyId = "89" }).Entity,
        };

        AssertMultiple(context.Set<ForeignStringKey>(), nameof(ForeignStringKey.StringKeyId), entities, "88", "99", keyType);
    }

    [ConditionalTheory]
    [InlineData(CompositeKeyType.Array)]
    [InlineData(CompositeKeyType.List)]
    [InlineData(CompositeKeyType.Enumerable)]
    public virtual void Find_string_non_key(CompositeKeyType keyType)
    {
        using var context = new FindContext();
        var entities = new[]
        {
            context.Attach(new StringNonKey { Id = Guid.NewGuid(), String = "87" }).Entity,
            context.Attach(new StringNonKey { Id = Guid.NewGuid(), String = "88" }).Entity,
            context.Attach(new StringNonKey { Id = Guid.NewGuid(), String = "88" }).Entity,
            context.Attach(new StringNonKey { Id = Guid.NewGuid(), String = "89" }).Entity,
        };

        AssertMultiple(context.Set<StringNonKey>(), nameof(StringNonKey.String), entities, "88", "99", keyType);
    }

    [ConditionalTheory]
    [InlineData(CompositeKeyType.Array)]
    [InlineData(CompositeKeyType.List)]
    [InlineData(CompositeKeyType.Enumerable)]
    public virtual void Find_shadow_string_key(CompositeKeyType keyType)
    {
        using var context = new FindContext();
        var entities = new[]
        {
            CreateShadowEntity<ShadowStringKey>(context, "Id", "87"),
            CreateShadowEntity<ShadowStringKey>(context, "Id", "88"),
            CreateShadowEntity<ShadowStringKey>(context, "Id", "89"),
        };

        AssertSingle(context.Set<ShadowStringKey>(), "Id", entities, "88", "99", keyType, isPk: true);
    }

    [ConditionalTheory]
    [InlineData(CompositeKeyType.Array)]
    [InlineData(CompositeKeyType.List)]
    [InlineData(CompositeKeyType.Enumerable)]
    public virtual void Find_shadow_alternate_string_key(CompositeKeyType keyType)
    {
        using var context = new FindContext();
        var entities = new[]
        {
            CreateShadowEntity<ShadowAlternateStringKey>(context, "AlternateId", "87"),
            CreateShadowEntity<ShadowAlternateStringKey>(context, "AlternateId", "88"),
            CreateShadowEntity<ShadowAlternateStringKey>(context, "AlternateId", "89"),
        };

        AssertSingle(context.Set<ShadowAlternateStringKey>(), "AlternateId", entities, "88", "99", keyType);
    }

    [ConditionalTheory]
    [InlineData(CompositeKeyType.Array, true)]
    [InlineData(CompositeKeyType.List, true)]
    [InlineData(CompositeKeyType.Enumerable, true)]
    [InlineData(CompositeKeyType.Array, false)]
    [InlineData(CompositeKeyType.List, false)]
    [InlineData(CompositeKeyType.Enumerable, false)]
    public virtual void Find_shadow_string_foreign_key(CompositeKeyType keyType, bool trackPrincipal)
    {
        using var context = new FindContext();

        if (trackPrincipal)
        {
            context.Attach(new StringKey { Id = "88" });
        }

        var entities = new[]
        {
            CreateShadowEntity<ShadowForeignStringKey>(context, "StringKeyId", "87"),
            CreateShadowEntity<ShadowForeignStringKey>(context, "StringKeyId", "88"),
            CreateShadowEntity<ShadowForeignStringKey>(context, "StringKeyId", "88"),
            CreateShadowEntity<ShadowForeignStringKey>(context, "StringKeyId", "89"),
        };

        AssertMultiple(context.Set<ShadowForeignStringKey>(), "StringKeyId", entities, "88", "99", keyType);
    }

    [ConditionalTheory]
    [InlineData(CompositeKeyType.Array)]
    [InlineData(CompositeKeyType.List)]
    [InlineData(CompositeKeyType.Enumerable)]
    public virtual void Find_shadow_string_non_key(CompositeKeyType keyType)
    {
        using var context = new FindContext();
        var entities = new[]
        {
            CreateShadowEntity<ShadowStringNonKey>(context, "String", "87"),
            CreateShadowEntity<ShadowStringNonKey>(context, "String", "88"),
            CreateShadowEntity<ShadowStringNonKey>(context, "String", "88"),
            CreateShadowEntity<ShadowStringNonKey>(context, "String", "89"),
        };

        AssertMultiple(context.Set<ShadowStringNonKey>(), "String", entities, "88", "99", keyType);
    }

    [ConditionalTheory]
    [InlineData(CompositeKeyType.Array)]
    [InlineData(CompositeKeyType.List)]
    [InlineData(CompositeKeyType.Enumerable)]
    public virtual void Find_string_foreign_key_with_nulls(CompositeKeyType keyType)
    {
        using var context = new FindContext();

        var entities = new[]
        {
            context.Attach(new ForeignStringKey { Id = Guid.NewGuid(), StringKeyId = "87" }).Entity,
            context.Attach(new ForeignStringKey { Id = Guid.NewGuid(), StringKeyId = null }).Entity,
            context.Attach(new ForeignStringKey { Id = Guid.NewGuid(), StringKeyId = null }).Entity,
            context.Attach(new ForeignStringKey { Id = Guid.NewGuid(), StringKeyId = "89" }).Entity,
        };

        AssertMultiple(context.Set<ForeignStringKey>(), nameof(ForeignStringKey.StringKeyId), entities, null, "99", keyType);
    }

    [ConditionalTheory]
    [InlineData(CompositeKeyType.Array)]
    [InlineData(CompositeKeyType.List)]
    [InlineData(CompositeKeyType.Enumerable)]
    public virtual void Find_string_non_key_with_nulls(CompositeKeyType keyType)
    {
        using var context = new FindContext();
        var entities = new[]
        {
            context.Attach(new StringNonKey { Id = Guid.NewGuid(), String = "87" }).Entity,
            context.Attach(new StringNonKey { Id = Guid.NewGuid(), String = null }).Entity,
            context.Attach(new StringNonKey { Id = Guid.NewGuid(), String = null }).Entity,
            context.Attach(new StringNonKey { Id = Guid.NewGuid(), String = "89" }).Entity,
        };

        AssertMultiple(context.Set<StringNonKey>(), nameof(StringNonKey.String), entities, null, "99", keyType);
    }

    [ConditionalTheory]
    [InlineData(CompositeKeyType.Array)]
    [InlineData(CompositeKeyType.List)]
    [InlineData(CompositeKeyType.Enumerable)]
    public virtual void Find_shadow_string_foreign_key_with_nulls(CompositeKeyType keyType)
    {
        using var context = new FindContext();
        var entities = new[]
        {
            CreateShadowEntity<ShadowForeignStringKey>(context, "StringKeyId", "87"),
            CreateShadowEntity<ShadowForeignStringKey>(context, "StringKeyId", null),
            CreateShadowEntity<ShadowForeignStringKey>(context, "StringKeyId", null),
            CreateShadowEntity<ShadowForeignStringKey>(context, "StringKeyId", "89"),
        };

        AssertMultiple(context.Set<ShadowForeignStringKey>(), "StringKeyId", entities, null, "99", keyType);
    }

    [ConditionalTheory]
    [InlineData(CompositeKeyType.Array)]
    [InlineData(CompositeKeyType.List)]
    [InlineData(CompositeKeyType.Enumerable)]
    public virtual void Find_shadow_string_non_key_with_nulls(CompositeKeyType keyType)
    {
        using var context = new FindContext();
        var entities = new[]
        {
            CreateShadowEntity<ShadowStringNonKey>(context, "String", "87"),
            CreateShadowEntity<ShadowStringNonKey>(context, "String", null),
            CreateShadowEntity<ShadowStringNonKey>(context, "String", null),
            CreateShadowEntity<ShadowStringNonKey>(context, "String", "89"),
        };

        AssertMultiple(context.Set<ShadowStringNonKey>(), "String", entities, null, "99", keyType);
    }

    [ConditionalTheory]
    [InlineData(CompositeKeyType.Array)]
    [InlineData(CompositeKeyType.List)]
    [InlineData(CompositeKeyType.Enumerable)]
    public virtual void Find_composite_key(CompositeKeyType keyType)
    {
        using var context = new FindContext();
        var entities = new[]
        {
            context.Attach(
                new CompositeKey
                {
                    Id1 = 1,
                    Id2 = "87",
                    Foo = "foo"
                }).Entity,
            context.Attach(
                new CompositeKey
                {
                    Id1 = 1,
                    Id2 = "88",
                    Foo = "foo"
                }).Entity,
            context.Attach(
                new CompositeKey
                {
                    Id1 = 1,
                    Id2 = "89",
                    Foo = "foo"
                }).Entity,
        };

        AssertSingle(
            context.Set<CompositeKey>(),
            [nameof(CompositeKey.Id1), nameof(CompositeKey.Id2), nameof(CompositeKey.Foo)], entities, keyType, isPk: true);
    }

    [ConditionalTheory]
    [InlineData(CompositeKeyType.Array)]
    [InlineData(CompositeKeyType.List)]
    [InlineData(CompositeKeyType.Enumerable)]
    public virtual void Find_composite_alternate_key(CompositeKeyType keyType)
    {
        using var context = new FindContext();
        var entities = new[]
        {
            context.Attach(
                new AlternateCompositeKey
                {
                    Id = Guid.NewGuid(),
                    AlternateId1 = 1,
                    AlternateId2 = "87",
                    AlternateFoo = "foo"
                }).Entity,
            context.Attach(
                new AlternateCompositeKey
                {
                    Id = Guid.NewGuid(),
                    AlternateId1 = 1,
                    AlternateId2 = "88",
                    AlternateFoo = "foo"
                }).Entity,
            context.Attach(
                new AlternateCompositeKey
                {
                    Id = Guid.NewGuid(),
                    AlternateId1 = 1,
                    AlternateId2 = "89",
                    AlternateFoo = "foo"
                }).Entity,
        };

        AssertSingle(
            context.Set<AlternateCompositeKey>(),
            [
                nameof(AlternateCompositeKey.AlternateId1),
                nameof(AlternateCompositeKey.AlternateId2),
                nameof(AlternateCompositeKey.AlternateFoo)
            ],
            entities, keyType);
    }

    [ConditionalTheory]
    [InlineData(CompositeKeyType.Array, true)]
    [InlineData(CompositeKeyType.List, true)]
    [InlineData(CompositeKeyType.Enumerable, true)]
    [InlineData(CompositeKeyType.Array, false)]
    [InlineData(CompositeKeyType.List, false)]
    [InlineData(CompositeKeyType.Enumerable, false)]
    public virtual void Find_composite_foreign_key(CompositeKeyType keyType, bool trackPrincipal)
    {
        using var context = new FindContext();

        if (trackPrincipal)
        {
            context.Attach(
                new CompositeKey
                {
                    Id1 = 1,
                    Id2 = "88",
                    Foo = "foo"
                });
        }

        var entities = new[]
        {
            context.Attach(
                new ForeignCompositeKey
                {
                    Id = Guid.NewGuid(),
                    CompositeKeyId1 = 1,
                    CompositeKeyId2 = "87",
                    CompositeKeyFoo = "foo"
                }).Entity,
            context.Attach(
                new ForeignCompositeKey
                {
                    Id = Guid.NewGuid(),
                    CompositeKeyId1 = 1,
                    CompositeKeyId2 = "88",
                    CompositeKeyFoo = "foo"
                }).Entity,
            context.Attach(
                new ForeignCompositeKey
                {
                    Id = Guid.NewGuid(),
                    CompositeKeyId1 = 1,
                    CompositeKeyId2 = "88",
                    CompositeKeyFoo = "foo"
                }).Entity,
            context.Attach(
                new ForeignCompositeKey
                {
                    Id = Guid.NewGuid(),
                    CompositeKeyId1 = 1,
                    CompositeKeyId2 = "89",
                    CompositeKeyFoo = "foo"
                }).Entity,
        };

        AssertMultiple(
            context.Set<ForeignCompositeKey>(),
            [
                nameof(ForeignCompositeKey.CompositeKeyId1),
                nameof(ForeignCompositeKey.CompositeKeyId2),
                nameof(ForeignCompositeKey.CompositeKeyFoo)
            ],
            entities, keyType);
    }

    [ConditionalTheory]
    [InlineData(CompositeKeyType.Array)]
    [InlineData(CompositeKeyType.List)]
    [InlineData(CompositeKeyType.Enumerable)]
    public virtual void Find_composite_non_key(CompositeKeyType keyType)
    {
        using var context = new FindContext();
        var entities = new[]
        {
            context.Attach(
                new CompositeNonKey
                {
                    Id = Guid.NewGuid(),
                    Int = 1,
                    String = "87",
                    Foo = "foo"
                }).Entity,
            context.Attach(
                new CompositeNonKey
                {
                    Id = Guid.NewGuid(),
                    Int = 1,
                    String = "88",
                    Foo = "foo"
                }).Entity,
            context.Attach(
                new CompositeNonKey
                {
                    Id = Guid.NewGuid(),
                    Int = 1,
                    String = "88",
                    Foo = "foo"
                }).Entity,
            context.Attach(
                new CompositeNonKey
                {
                    Id = Guid.NewGuid(),
                    Int = 1,
                    String = "89",
                    Foo = "foo"
                }).Entity,
        };

        AssertMultiple(
            context.Set<CompositeNonKey>(),
            [nameof(CompositeNonKey.Int), nameof(CompositeNonKey.String), nameof(CompositeNonKey.Foo)],
            entities, keyType);
    }

    [ConditionalTheory]
    [InlineData(CompositeKeyType.Array)]
    [InlineData(CompositeKeyType.List)]
    [InlineData(CompositeKeyType.Enumerable)]
    public virtual void Find_shadow_composite_key(CompositeKeyType keyType)
    {
        using var context = new FindContext();
        var entities = new[]
        {
            CreateShadowEntity<ShadowCompositeKey>(
                context,
                ["Id1", "Id2", "Foo"],
                [1, "87", "foo"]),
            CreateShadowEntity<ShadowCompositeKey>(
                context,
                ["Id1", "Id2", "Foo"],
                [1, "88", "foo"]),
            CreateShadowEntity<ShadowCompositeKey>(
                context,
                ["Id1", "Id2", "Foo"],
                [1, "89", "foo"])
        };

        AssertSingle(
            context.Set<ShadowCompositeKey>(),
            ["Id1", "Id2", "Foo"], entities, keyType, isPk: true);
    }

    [ConditionalTheory]
    [InlineData(CompositeKeyType.Array)]
    [InlineData(CompositeKeyType.List)]
    [InlineData(CompositeKeyType.Enumerable)]
    public virtual void Find_shadow_alternate_composite_key(CompositeKeyType keyType)
    {
        using var context = new FindContext();
        var entities = new[]
        {
            CreateShadowEntity<ShadowAlternateCompositeKey>(
                context,
                ["AlternateId1", "AlternateId2", "AlternateFoo"],
                [1, "87", "foo"]),
            CreateShadowEntity<ShadowAlternateCompositeKey>(
                context,
                ["AlternateId1", "AlternateId2", "AlternateFoo"],
                [1, "88", "foo"]),
            CreateShadowEntity<ShadowAlternateCompositeKey>(
                context,
                ["AlternateId1", "AlternateId2", "AlternateFoo"],
                [1, "89", "foo"])
        };

        AssertSingle(
            context.Set<ShadowAlternateCompositeKey>(),
            ["AlternateId1", "AlternateId2", "AlternateFoo"], entities, keyType);
    }

    [ConditionalTheory]
    [InlineData(CompositeKeyType.Array, true)]
    [InlineData(CompositeKeyType.List, true)]
    [InlineData(CompositeKeyType.Enumerable, true)]
    [InlineData(CompositeKeyType.Array, false)]
    [InlineData(CompositeKeyType.List, false)]
    [InlineData(CompositeKeyType.Enumerable, false)]
    public virtual void Find_shadow_composite_foreign_key(CompositeKeyType keyType, bool trackPrincipal)
    {
        using var context = new FindContext();

        if (trackPrincipal)
        {
            context.Attach(
                new CompositeKey
                {
                    Id1 = 1,
                    Id2 = "88",
                    Foo = "foo"
                });
        }

        var entities = new[]
        {
            CreateShadowEntity<ShadowForeignCompositeKey>(
                context,
                ["CompositeKeyId1", "CompositeKeyId2", "CompositeKeyFoo"],
                [1, "87", "foo"]),
            CreateShadowEntity<ShadowForeignCompositeKey>(
                context,
                ["CompositeKeyId1", "CompositeKeyId2", "CompositeKeyFoo"],
                [1, "88", "foo"]),
            CreateShadowEntity<ShadowForeignCompositeKey>(
                context,
                ["CompositeKeyId1", "CompositeKeyId2", "CompositeKeyFoo"],
                [1, "88", "foo"]),
            CreateShadowEntity<ShadowForeignCompositeKey>(
                context,
                ["CompositeKeyId1", "CompositeKeyId2", "CompositeKeyFoo"],
                [1, "89", "foo"])
        };

        AssertMultiple(
            context.Set<ShadowForeignCompositeKey>(),
            ["CompositeKeyId1", "CompositeKeyId2", "CompositeKeyFoo"], entities, keyType);
    }

    [ConditionalTheory]
    [InlineData(CompositeKeyType.Array)]
    [InlineData(CompositeKeyType.List)]
    [InlineData(CompositeKeyType.Enumerable)]
    public virtual void Find_shadow_composite_non_key(CompositeKeyType keyType)
    {
        using var context = new FindContext();
        var entities = new[]
        {
            CreateShadowEntity<ShadowCompositeNonKey>(
                context,
                ["Int", "String", "Foo"],
                [1, "87", "foo"]),
            CreateShadowEntity<ShadowCompositeNonKey>(
                context,
                ["Int", "String", "Foo"],
                [1, "88", "foo"]),
            CreateShadowEntity<ShadowCompositeNonKey>(
                context,
                ["Int", "String", "Foo"],
                [1, "88", "foo"]),
            CreateShadowEntity<ShadowCompositeNonKey>(
                context,
                ["Int", "String", "Foo"],
                [1, "89", "foo"])
        };

        AssertMultiple(
            context.Set<ShadowCompositeNonKey>(),
            ["Int", "String", "Foo"], entities, keyType);
    }

    [ConditionalTheory]
    [InlineData(CompositeKeyType.Array)]
    [InlineData(CompositeKeyType.List)]
    [InlineData(CompositeKeyType.Enumerable)]
    public virtual void Find_composite_foreign_key_with_nulls(CompositeKeyType keyType)
    {
        using var context = new FindContext();

        var entities = new[]
        {
            context.Attach(
                new ForeignCompositeKey
                {
                    Id = Guid.NewGuid(),
                    CompositeKeyId1 = 1,
                    CompositeKeyId2 = "87",
                    CompositeKeyFoo = "foo"
                }).Entity,
            context.Attach(
                new ForeignCompositeKey
                {
                    Id = Guid.NewGuid(),
                    CompositeKeyId1 = 1,
                    CompositeKeyId2 = null,
                    CompositeKeyFoo = "foo"
                }).Entity,
            context.Attach(
                new ForeignCompositeKey
                {
                    Id = Guid.NewGuid(),
                    CompositeKeyId1 = 1,
                    CompositeKeyId2 = null,
                    CompositeKeyFoo = "foo"
                }).Entity,
            context.Attach(
                new ForeignCompositeKey
                {
                    Id = Guid.NewGuid(),
                    CompositeKeyId1 = 1,
                    CompositeKeyId2 = "89",
                    CompositeKeyFoo = "foo"
                }).Entity,
        };

        AssertMultiple(
            context.Set<ForeignCompositeKey>(),
            [
                nameof(ForeignCompositeKey.CompositeKeyId1),
                nameof(ForeignCompositeKey.CompositeKeyId2),
                nameof(ForeignCompositeKey.CompositeKeyFoo)
            ],
            entities, keyType, withNulls: true);
    }

    [ConditionalTheory]
    [InlineData(CompositeKeyType.Array)]
    [InlineData(CompositeKeyType.List)]
    [InlineData(CompositeKeyType.Enumerable)]
    public virtual void Find_composite_non_key_with_nulls(CompositeKeyType keyType)
    {
        using var context = new FindContext();
        var entities = new[]
        {
            context.Attach(
                new CompositeNonKey
                {
                    Id = Guid.NewGuid(),
                    Int = 1,
                    String = "87",
                    Foo = "foo"
                }).Entity,
            context.Attach(
                new CompositeNonKey
                {
                    Id = Guid.NewGuid(),
                    Int = 1,
                    String = null,
                    Foo = "foo"
                }).Entity,
            context.Attach(
                new CompositeNonKey
                {
                    Id = Guid.NewGuid(),
                    Int = 1,
                    String = null,
                    Foo = "foo"
                }).Entity,
            context.Attach(
                new CompositeNonKey
                {
                    Id = Guid.NewGuid(),
                    Int = 1,
                    String = "89",
                    Foo = "foo"
                }).Entity,
        };

        AssertMultiple(
            context.Set<CompositeNonKey>(),
            [nameof(CompositeNonKey.Int), nameof(CompositeNonKey.String), nameof(CompositeNonKey.Foo)],
            entities, keyType, withNulls: true);
    }

    [ConditionalTheory]
    [InlineData(CompositeKeyType.Array)]
    [InlineData(CompositeKeyType.List)]
    [InlineData(CompositeKeyType.Enumerable)]
    public virtual void Find_shadow_composite_foreign_key_with_nulls(CompositeKeyType keyType)
    {
        using var context = new FindContext();

        var entities = new[]
        {
            CreateShadowEntity<ShadowForeignCompositeKey>(
                context,
                ["CompositeKeyId1", "CompositeKeyId2", "CompositeKeyFoo"],
                [1, "87", "foo"]),
            CreateShadowEntity<ShadowForeignCompositeKey>(
                context,
                ["CompositeKeyId1", "CompositeKeyId2", "CompositeKeyFoo"],
                [1, null, "foo"]),
            CreateShadowEntity<ShadowForeignCompositeKey>(
                context,
                ["CompositeKeyId1", "CompositeKeyId2", "CompositeKeyFoo"],
                [1, null, "foo"]),
            CreateShadowEntity<ShadowForeignCompositeKey>(
                context,
                ["CompositeKeyId1", "CompositeKeyId2", "CompositeKeyFoo"],
                [1, "89", "foo"])
        };

        AssertMultiple(
            context.Set<ShadowForeignCompositeKey>(),
            ["CompositeKeyId1", "CompositeKeyId2", "CompositeKeyFoo"], entities, keyType, withNulls: true);
    }

    [ConditionalTheory]
    [InlineData(CompositeKeyType.Array)]
    [InlineData(CompositeKeyType.List)]
    [InlineData(CompositeKeyType.Enumerable)]
    public virtual void Find_shadow_composite_non_key_with_nulls(CompositeKeyType keyType)
    {
        using var context = new FindContext();
        var entities = new[]
        {
            CreateShadowEntity<ShadowCompositeNonKey>(
                context,
                ["Int", "String", "Foo"],
                [1, "87", "foo"]),
            CreateShadowEntity<ShadowCompositeNonKey>(
                context,
                ["Int", "String", "Foo"],
                [1, null, "foo"]),
            CreateShadowEntity<ShadowCompositeNonKey>(
                context,
                ["Int", "String", "Foo"],
                [1, null, "foo"]),
            CreateShadowEntity<ShadowCompositeNonKey>(
                context,
                ["Int", "String", "Foo"],
                [1, "89", "foo"])
        };

        AssertMultiple(
            context.Set<ShadowCompositeNonKey>(),
            ["Int", "String", "Foo"], entities, keyType, withNulls: true);
    }

    [ConditionalTheory]
    [InlineData(CompositeKeyType.Array)]
    [InlineData(CompositeKeyType.List)]
    [InlineData(CompositeKeyType.Enumerable)]
    public virtual void Find_base_type_tracked(CompositeKeyType keyType)
    {
        using var context = new FindContext();
        var entities = new[]
        {
            context.Attach(new BaseType { Id = 87 }).Entity,
            context.Attach(new BaseType { Id = 88 }).Entity,
            context.Attach(new BaseType { Id = 89 }).Entity,
        };

        AssertSingle(context.Set<BaseType>(), nameof(BaseType.Id), entities, 88, 99, keyType, isPk: true);
    }

    [ConditionalTheory]
    [InlineData(CompositeKeyType.Array)]
    [InlineData(CompositeKeyType.List)]
    [InlineData(CompositeKeyType.Enumerable)]
    public virtual void Find_derived_type_tracked(CompositeKeyType keyType)
    {
        using var context = new FindContext();
        var entities = new[]
        {
            context.Attach(new DerivedType { Id = 87 }).Entity,
            context.Attach(new DerivedType { Id = 88 }).Entity,
            context.Attach(new DerivedType { Id = 89 }).Entity,
        };

        AssertSingle(context.Set<DerivedType>(), nameof(BaseType.Id), entities, 88, 99, keyType, isPk: true);
    }

    [ConditionalFact]
    public virtual void Returns_null_for_null_key()
    {
        using var context = new FindContext();

        var local = context.Set<IntKey>().Local;
        Assert.Null(local.FindEntryUntyped(new object?[] { null }));
        Assert.Null(local.FindEntry(new[] { nameof(IntKey.Id) }, new object?[] { null }));
    }

    [ConditionalFact]
    public virtual void Returns_null_for_null_alternate_key()
    {
        using var context = new FindContext();

        var local = context.Set<AlternateIntKey>().Local;
        Assert.Null(local.FindEntry(new[] { nameof(AlternateIntKey.AlternateId) }, new object?[] { null }));
    }

    [ConditionalFact]
    public virtual void Returns_null_for_null_nullable_key()
    {
        using var context = new FindContext();

        var local = context.Set<NullableIntKey>().Local;
        Assert.Null(local.FindEntry((int?)null));
        Assert.Null(local.FindEntryUntyped(new object?[] { null }));
        Assert.Null(local.FindEntry(nameof(NullableIntKey.Id), (int?)null));
        Assert.Null(local.FindEntry(new[] { nameof(NullableIntKey.Id) }, new object?[] { null }));
    }

    [ConditionalFact]
    public virtual void Returns_null_for_null_nullable_alternate_key()
    {
        using var context = new FindContext();

        var local = context.Set<AlternateNullableIntKey>().Local;
        Assert.Null(local.FindEntry(nameof(AlternateNullableIntKey.AlternateId), (int?)null));
        Assert.Null(local.FindEntry(new[] { nameof(AlternateNullableIntKey.AlternateId) }, new object?[] { null }));
    }

    [ConditionalFact]
    public virtual void Returns_empty_for_null_key()
    {
        using var context = new FindContext();

        var local = context.Set<IntKey>().Local;
        Assert.Empty(local.GetEntries(new[] { nameof(IntKey.Id) }, new object?[] { null }));
    }

    [ConditionalFact]
    public virtual void Returns_empty_for_null_alternate_key()
    {
        using var context = new FindContext();

        var local = context.Set<AlternateIntKey>().Local;
        Assert.Empty(local.GetEntries(new[] { nameof(AlternateIntKey.AlternateId) }, new object?[] { null }));
    }

    [ConditionalFact]
    public virtual void Returns_empty_for_null_nullable_key()
    {
        using var context = new FindContext();

        var local = context.Set<NullableIntKey>().Local;
        Assert.Empty(local.GetEntries(nameof(NullableIntKey.Id), (int?)null));
        Assert.Empty(local.GetEntries(new[] { nameof(NullableIntKey.Id) }, new object?[] { null }));
    }

    [ConditionalFact]
    public virtual void Returns_empty_for_null_nullable_alternate_key()
    {
        using var context = new FindContext();

        var local = context.Set<AlternateNullableIntKey>().Local;
        Assert.Empty(local.GetEntries(nameof(AlternateNullableIntKey.AlternateId), (int?)null));
        Assert.Empty(local.GetEntries(new[] { nameof(AlternateNullableIntKey.AlternateId) }, new object?[] { null }));
    }

    [ConditionalFact]
    public virtual void Returns_null_for_composite_key_with_null()
    {
        using var context = new FindContext();

        var local = context.Set<CompositeKey>().Local;

        Assert.Null(local.FindEntryUntyped(new object?[] { null, "99", "foo" }));

        Assert.Null(
            local.FindEntry(
                new[] { nameof(CompositeKey.Id1), nameof(CompositeKey.Id2), nameof(CompositeKey.Foo) },
                new object?[] { 1, null, "foo" }));
    }

    [ConditionalFact]
    public virtual void Returns_null_for_composite_alternate_key_with_null()
    {
        using var context = new FindContext();

        var local = context.Set<AlternateCompositeKey>().Local;

        Assert.Null(
            local.FindEntry(
                new[]
                {
                    nameof(AlternateCompositeKey.AlternateId1),
                    nameof(AlternateCompositeKey.AlternateId2),
                    nameof(AlternateCompositeKey.AlternateFoo)
                },
                new object?[] { null, "88", "foo" }));
    }

    [ConditionalFact]
    public virtual void Throws_for_single_value_used_for_composite_key()
    {
        using var context = new FindContext();
        var local = context.Set<CompositeKey>().Local;

        Assert.Equal(
            CoreStrings.FindValueCountMismatch("CompositeKey", 3, 1),
            Assert.Throws<ArgumentException>(() => local.FindEntry(0)).Message);

        Assert.Equal(
            CoreStrings.FindWrongCount(1, 3),
            Assert.Throws<ArgumentException>(() => local.FindEntryUntyped(new object?[] { 1 })).Message);
    }

    [ConditionalFact]
    public virtual void Throws_for_multiple_values_passed_for_simple_key()
    {
        using var context = new FindContext();

        Assert.Equal(
            CoreStrings.FindWrongCount(2, 1),
            Assert.Throws<ArgumentException>(
                () => context.Set<IntKey>().Local.FindEntryUntyped(new object[] { 77, 88 })).Message);
    }

    [ConditionalFact]
    public virtual void Throws_for_wrong_number_of_values()
    {
        using var context = new FindContext();
        var set = context.Set<CompositeKey>();
        var property1 = set.EntityType.FindProperty(nameof(CompositeKey.Id2))!;
        var property2 = set.EntityType.FindProperty(nameof(CompositeKey.Foo))!;
        var oneValue = new object?[] { "1" };
        var twoValues = new object?[] { "1", "2" };

        Assert.Equal(
            CoreStrings.FindWrongCount(1, 3),
            Assert.Throws<ArgumentException>(
                () => set.Local.FindEntryUntyped(new object[] { 77 })).Message);

        Assert.Equal(
            CoreStrings.FindWrongCount(1, 2),
            Assert.Throws<ArgumentException>(
                () => set.Local.FindEntry(new[] { property1.Name, property2.Name }, oneValue)).Message);

        Assert.Equal(
            CoreStrings.FindWrongCount(2, 1),
            Assert.Throws<ArgumentException>(
                () => set.Local.FindEntry(new[] { property1.Name }, twoValues)).Message);

        Assert.Equal(
            CoreStrings.FindWrongCount(1, 2),
            Assert.Throws<ArgumentException>(
                () => set.Local.GetEntries(new[] { property1.Name, property2.Name }, oneValue).ToList()).Message);

        Assert.Equal(
            CoreStrings.FindWrongCount(2, 1),
            Assert.Throws<ArgumentException>(
                () => set.Local.GetEntries(new[] { property1.Name }, twoValues).ToList()).Message);

        Assert.Equal(
            CoreStrings.FindWrongCount(1, 2),
            Assert.Throws<ArgumentException>(
                () => set.Local.FindEntry(new[] { property1, property2 }, oneValue)).Message);

        Assert.Equal(
            CoreStrings.FindWrongCount(2, 1),
            Assert.Throws<ArgumentException>(
                () => set.Local.FindEntry(new[] { property1 }, twoValues)).Message);

        Assert.Equal(
            CoreStrings.FindWrongCount(1, 2),
            Assert.Throws<ArgumentException>(
                () => set.Local.GetEntries(new[] { property1, property2 }, oneValue).ToList()).Message);

        Assert.Equal(
            CoreStrings.FindWrongCount(2, 1),
            Assert.Throws<ArgumentException>(
                () => set.Local.GetEntries(new[] { property1 }, twoValues).ToList()).Message);
    }

    [ConditionalFact]
    public virtual void Throws_for_bad_single_key_type()
    {
        using var context = new FindContext();
        var local = context.Set<IntKey>().Local;

        Assert.Equal(
            CoreStrings.WrongGenericPropertyType("Id", "IntKey", "int", "string"),
            Assert.Throws<ArgumentException>(
                () => local.FindEntry("77")).Message);

        Assert.Equal(
            CoreStrings.FindWrongType("string", "Id", "int"),
            Assert.Throws<ArgumentException>(
                () => local.FindEntryUntyped(new object?[] { "77" })).Message);
    }

    [ConditionalFact]
    public virtual void Throws_for_bad_single_type()
    {
        using var context = new FindContext();

        var set = context.Set<CompositeKey>();
        var property1 = set.EntityType.FindProperty(nameof(CompositeKey.Id1))!;

        Assert.Equal(
            CoreStrings.WrongGenericPropertyType("Id1", "CompositeKey", "int", "string"),
            Assert.Throws<ArgumentException>(
                () => set.Local.FindEntry(property1.Name, "1")).Message);

        Assert.Equal(
            CoreStrings.FindWrongType("string", "Id1", "int"),
            Assert.Throws<ArgumentException>(
                () => set.Local.FindEntry(new[] { property1.Name }, new object?[] { "1" })).Message);

        Assert.Equal(
            CoreStrings.FindWrongType("string", "Id1", "int"),
            Assert.Throws<ArgumentException>(
                () => set.Local.GetEntries(new[] { property1.Name }, new object?[] { "1" }).ToList()).Message);

        Assert.Equal(
            CoreStrings.FindWrongType("string", "Id1", "int"),
            Assert.Throws<ArgumentException>(
                () => set.Local.FindEntry(property1, "1")).Message);

        Assert.Equal(
            CoreStrings.FindWrongType("string", "Id1", "int"),
            Assert.Throws<ArgumentException>(
                () => set.Local.FindEntry(new[] { property1 }, new object?[] { "1" })).Message);

        Assert.Equal(
            CoreStrings.FindWrongType("string", "Id1", "int"),
            Assert.Throws<ArgumentException>(
                () => set.Local.GetEntries(new[] { property1 }, new object?[] { "1" }).ToList()).Message);
    }

    [ConditionalFact]
    public virtual void Throws_for_bad_type_in_values_list()
    {
        using var context = new FindContext();
        var set = context.Set<CompositeKey>();
        var property1 = set.EntityType.FindProperty(nameof(CompositeKey.Id1))!;
        var property2 = set.EntityType.FindProperty(nameof(CompositeKey.Foo))!;

        Assert.Equal(
            CoreStrings.FindWrongType("int", "Id2", "string"),
            Assert.Throws<ArgumentException>(
                () => set.Local.FindEntryUntyped(new object[] { 77, 88, "X" })).Message);

        Assert.Equal(
            CoreStrings.FindWrongType("int", "Foo", "string"),
            Assert.Throws<ArgumentException>(
                () => set.Local.FindEntry(new[] { property1.Name, property2.Name }, new object?[] { 1, 2 })).Message);

        Assert.Equal(
            CoreStrings.FindWrongType("int", "Foo", "string"),
            Assert.Throws<ArgumentException>(
                () => set.Local.GetEntries(
                    new[] { property1.Name, property2.Name }, new object?[] { 1, 2 }).ToList()).Message);

        Assert.Equal(
            CoreStrings.FindWrongType("int", "Foo", "string"),
            Assert.Throws<ArgumentException>(
                () => set.Local.FindEntry(new[] { property1, property2 }, new object?[] { 1, 2 })).Message);

        Assert.Equal(
            CoreStrings.FindWrongType("int", "Foo", "string"),
            Assert.Throws<ArgumentException>(
                () => set.Local.GetEntries(new[] { property1, property2 }, new object?[] { 1, 2 }).ToList()).Message);
    }

    private static void AssertSingle<TEntity, TProperty>(
        DbSet<TEntity> set,
        string propertyName,
        TEntity[] entities,
        TProperty found,
        TProperty notFound,
        CompositeKeyType keyType,
        bool isPk = false,
        int index = 1)
        where TEntity : class
    {
        var property = set.EntityType.FindProperty(propertyName)!;
        var foundList = CreateKeyValues(keyType, found);
        var notFoundList = CreateKeyValues(keyType, notFound);

        if (isPk)
        {
            Assert.Same(entities[index], set.Local.FindEntry(found)!.Entity);
            Assert.Same(entities[index], set.Local.FindEntryUntyped(foundList)!.Entity);
        }

        Assert.Same(entities[index], set.Local.FindEntry(propertyName, found)!.Entity);
        Assert.Same(entities[index], set.Local.FindEntry(property, found)!.Entity);
        Assert.Same(entities[index], set.Local.FindEntry(new[] { propertyName }, foundList)!.Entity);
        Assert.Same(entities[index], set.Local.FindEntry(new[] { property }, foundList)!.Entity);
        Assert.Same(entities[index], set.Local.GetEntries(propertyName, found).Single().Entity);
        Assert.Same(entities[index], set.Local.GetEntries(property, found).Single().Entity);
        Assert.Same(entities[index], set.Local.GetEntries(new[] { propertyName }, foundList).Single().Entity);
        Assert.Same(entities[index], set.Local.GetEntries(new[] { property }, foundList).Single().Entity);

        if (isPk)
        {
            Assert.Null(set.Local.FindEntry(notFound));
            Assert.Null(set.Local.FindEntryUntyped(notFoundList));
        }

        Assert.Null(set.Local.FindEntry(propertyName, notFound));
        Assert.Null(set.Local.FindEntry(property, notFound));
        Assert.Null(set.Local.FindEntry(new[] { propertyName }, notFoundList));
        Assert.Null(set.Local.FindEntry(new[] { property }, notFoundList));
        Assert.Empty(set.Local.GetEntries(propertyName, notFound));
        Assert.Empty(set.Local.GetEntries(property, notFound));
        Assert.Empty(set.Local.GetEntries(new[] { propertyName }, notFoundList));
        Assert.Empty(set.Local.GetEntries(new[] { property }, notFoundList));
    }

    private static void AssertMultiple<TEntity, TProperty>(
        DbSet<TEntity> set,
        string propertyName,
        TEntity[] entities,
        TProperty found,
        TProperty notFound,
        CompositeKeyType keyType,
        int index = 1)
        where TEntity : class
    {
        var property = set.EntityType.FindProperty(propertyName)!;
        var foundList = CreateKeyValues(keyType, found);
        var notFoundList = CreateKeyValues(keyType, notFound);

        Assert.Contains(set.Local.FindEntry(propertyName, found)!.Entity, entities);
        Assert.Contains(set.Local.FindEntry(property, found)!.Entity, entities);
        Assert.Contains(set.Local.FindEntry(new[] { propertyName }, foundList)!.Entity, entities);
        Assert.Contains(set.Local.FindEntry(new[] { property }, foundList)!.Entity, entities);
        AssertFound(set.Local.GetEntries(propertyName, found).Select(e => e.Entity));
        AssertFound(set.Local.GetEntries(property, found).Select(e => e.Entity));
        AssertFound(set.Local.GetEntries(new[] { propertyName }, foundList).Select(e => e.Entity));
        AssertFound(set.Local.GetEntries(new[] { property }, foundList).Select(e => e.Entity));

        Assert.Null(set.Local.FindEntry(propertyName, notFound));
        Assert.Null(set.Local.FindEntry(property, notFound));
        Assert.Null(set.Local.FindEntry(new[] { propertyName }, notFoundList));
        Assert.Null(set.Local.FindEntry(new[] { property }, notFoundList));
        Assert.Empty(set.Local.GetEntries(propertyName, notFound).Select(e => e.Entity));
        Assert.Empty(set.Local.GetEntries(property, notFound).Select(e => e.Entity));
        Assert.Empty(set.Local.GetEntries(new[] { propertyName }, notFoundList).Select(e => e.Entity));
        Assert.Empty(set.Local.GetEntries(new[] { property }, notFoundList).Select(e => e.Entity));

        void AssertFound(IEnumerable<TEntity> actual)
        {
            var actualList = actual.ToList();
            Assert.Equal(2, actualList.Count);
            Assert.Contains(entities[index], actualList);
            Assert.Contains(entities[index + 1], actualList);
        }
    }

    private static void AssertSingle<TEntity>(
        DbSet<TEntity> set,
        string[] propertyNames,
        TEntity[] entities,
        CompositeKeyType keyType,
        bool isPk = false,
        int index = 1)
        where TEntity : class
    {
        var properties = propertyNames.Select(n => set.EntityType.FindProperty(n)!).ToList();
        var found = CreateKeyValues(keyType, 1, "88", "foo");
        var notFound = CreateKeyValues(keyType, 1, "99", "foo");

        if (isPk)
        {
            Assert.Same(entities[index], set.Local.FindEntryUntyped(found)!.Entity);
        }

        Assert.Same(entities[index], set.Local.FindEntry(propertyNames, found)!.Entity);
        Assert.Same(entities[index], set.Local.FindEntry(properties, found)!.Entity);
        Assert.Same(entities[index], set.Local.GetEntries(propertyNames, found).Single().Entity);
        Assert.Same(entities[index], set.Local.GetEntries(properties, found).Single().Entity);

        if (isPk)
        {
            Assert.Null(set.Local.FindEntryUntyped(notFound));
        }

        Assert.Null(set.Local.FindEntry(propertyNames, notFound));
        Assert.Null(set.Local.FindEntry(properties, notFound));
        Assert.Empty(set.Local.GetEntries(propertyNames, notFound));
        Assert.Empty(set.Local.GetEntries(properties, notFound));
    }

    private static void AssertMultiple<TEntity>(
        DbSet<TEntity> set,
        string[] propertyNames,
        TEntity[] entities,
        CompositeKeyType keyType,
        int index = 1,
        bool withNulls = false)
        where TEntity : class
    {
        var properties = propertyNames.Select(n => set.EntityType.FindProperty(n)!).ToList();
        var found = CreateKeyValues(keyType, 1, withNulls ? null : "88", "foo");
        var notFound = CreateKeyValues(keyType, 1, "99", "foo");

        Assert.Contains(set.Local.FindEntry(propertyNames, found)!.Entity, entities);
        Assert.Contains(set.Local.FindEntry(properties, found)!.Entity, entities);
        AssertFound(set.Local.GetEntries(propertyNames, found).Select(e => e.Entity));
        AssertFound(set.Local.GetEntries(properties, found).Select(e => e.Entity));

        Assert.Null(set.Local.FindEntry(propertyNames, notFound));
        Assert.Null(set.Local.FindEntry(properties, notFound));
        Assert.Empty(set.Local.GetEntries(propertyNames, notFound).Select(e => e.Entity));
        Assert.Empty(set.Local.GetEntries(properties, notFound).Select(e => e.Entity));

        void AssertFound(IEnumerable<TEntity> actual)
        {
            var actualList = actual.ToList();
            Assert.Equal(2, actualList.Count);
            Assert.Contains(entities[index], actualList);
            Assert.Contains(entities[index + 1], actualList);
        }
    }

    private TEntity CreateShadowEntity<TEntity>(FindContext context, string propertyName, object? value)
        where TEntity : class, new()
    {
        var entry = context.Entry(new TEntity());
        entry.Property(propertyName).CurrentValue = value;
        entry.State = EntityState.Added;
        entry.State = EntityState.Unchanged;
        return entry.Entity;
    }

    private TEntity CreateShadowEntity<TEntity>(FindContext context, string[] propertyNames, object?[] values)
        where TEntity : class, new()
    {
        var entry = context.Entry(new TEntity());
        for (var i = 0; i < propertyNames.Length; i++)
        {
            entry.Property(propertyNames[i]).CurrentValue = values[i];
        }

        entry.State = EntityState.Added;
        entry.State = EntityState.Unchanged;
        return entry.Entity;
    }

    private TEntity CreateShadowEntityShared<TEntity>(
        FindContextShared context,
        string entityTypeName,
        string propertyName,
        object value)
        where TEntity : class, new()
    {
        var entry = context.Set<TEntity>(entityTypeName).Entry(new TEntity());
        entry.Property(propertyName).CurrentValue = value;
        entry.State = EntityState.Added;
        entry.State = EntityState.Unchanged;
        return entry.Entity;
    }

    public enum CompositeKeyType
    {
        Array,
        List,
        Enumerable
    }

    private static IEnumerable<object?> CreateKeyValues(CompositeKeyType keyType, params object?[] values)
        => keyType switch
        {
            CompositeKeyType.Array => values,
            CompositeKeyType.List => values.ToList(),
            CompositeKeyType.Enumerable => values.Where(o => true),
            _ => throw new ArgumentOutOfRangeException(nameof(keyType), keyType, null)
        };

    protected class BaseType
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        public string? Foo { get; set; }
    }

    protected class DerivedType : BaseType
    {
        public string? Boo { get; set; }
    }

    protected class IntKey
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }
    }

    protected class AlternateIntKey
    {
        public Guid Id { get; set; }
        public int AlternateId { get; set; }
    }

    protected class ForeignIntKey
    {
        public Guid Id { get; set; }
        public int IntKeyId { get; set; }
        public IntKey? IntKey { get; set; }
    }

    protected class IntNonKey
    {
        public Guid Id { get; set; }
        public int Int { get; set; }
    }

    protected class ShadowIntKey;

    protected class ShadowAlternateIntKey
    {
        public Guid Id { get; set; }
    }

    protected class ShadowForeignIntKey
    {
        public Guid Id { get; set; }
        public ShadowIntKey? IntKey { get; set; }
    }

    protected class ShadowIntNonKey
    {
        public Guid Id { get; set; }
    }

    protected class NullableIntKey
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int? Id { get; set; }
    }

    protected class AlternateNullableIntKey
    {
        public Guid Id { get; set; }
        public int? AlternateId { get; set; }
    }

    protected class ForeignNullableIntKey
    {
        public Guid Id { get; set; }
        public int? NullableIntKeyId { get; set; }
        public NullableIntKey? NullableIntKey { get; set; }
    }

    protected class NullableIntNonKey
    {
        public Guid Id { get; set; }
        public int? NullableInt { get; set; }
    }

    protected class ShadowNullableIntKey;

    protected class ShadowAlternateNullableIntKey
    {
        public Guid Id { get; set; }
    }

    protected class ShadowForeignNullableIntKey
    {
        public Guid Id { get; set; }
        public ShadowNullableIntKey? NullableIntKey { get; set; }
    }

    protected class ShadowNullableIntNonKey
    {
        public Guid Id { get; set; }
    }

    protected class StringKey
    {
        public string Id { get; set; } = null!;
    }

    protected class AlternateStringKey
    {
        public Guid Id { get; set; }
        public string AlternateId { get; set; } = null!;
    }

    protected class ForeignStringKey
    {
        public Guid Id { get; set; }
        public string? StringKeyId { get; set; }
        public StringKey? StringKey { get; set; }
    }

    protected class StringNonKey
    {
        public Guid Id { get; set; }
        public string? String { get; set; }
    }

    protected class ShadowStringKey;

    protected class ShadowAlternateStringKey
    {
        public Guid Id { get; set; }
    }

    protected class ShadowForeignStringKey
    {
        public Guid Id { get; set; }
        public ShadowStringKey? StringKey { get; set; }
    }

    protected class ShadowStringNonKey
    {
        public Guid Id { get; set; }
    }

    protected class CompositeKey
    {
        public int Id1 { get; set; }
        public string Id2 { get; set; } = null!;
        public string Foo { get; set; } = null!;
    }

    protected class AlternateCompositeKey
    {
        public Guid Id { get; set; }
        public int AlternateId1 { get; set; }
        public string AlternateId2 { get; set; } = null!;
        public string AlternateFoo { get; set; } = null!;
    }

    protected class ForeignCompositeKey
    {
        public Guid Id { get; set; }
        public int? CompositeKeyId1 { get; set; }
        public string? CompositeKeyId2 { get; set; }
        public string? CompositeKeyFoo { get; set; }
        public CompositeKey? CompositeKey { get; set; }
    }

    protected class CompositeNonKey
    {
        public Guid Id { get; set; }
        public int? Int { get; set; }
        public string? String { get; set; }
        public string? Foo { get; set; }
    }

    protected class ShadowCompositeKey;

    protected class ShadowAlternateCompositeKey
    {
        public Guid Id { get; set; }
    }

    protected class ShadowForeignCompositeKey
    {
        public Guid Id { get; set; }
        public ShadowCompositeKey? CompositeKey { get; set; }
    }

    protected class ShadowCompositeNonKey
    {
        public Guid Id { get; set; }
    }

    private class FindContext : DbContext
    {
        private readonly IServiceProvider _serviceProvider;

        public FindContext()
        {
            _serviceProvider = InMemoryTestHelpers.Instance.CreateServiceProvider();
        }

        protected internal override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<IntKey>();
            modelBuilder.Entity<AlternateIntKey>().HasAlternateKey(e => e.AlternateId);
            modelBuilder.Entity<ForeignIntKey>();
            modelBuilder.Entity<IntNonKey>();

            modelBuilder.Entity<ShadowIntKey>().Property<int>("Id").ValueGeneratedNever();
            modelBuilder.Entity<ShadowAlternateIntKey>(
                b =>
                {
                    b.Property<int>("AlternateId");
                    b.HasAlternateKey("AlternateId");
                });
            modelBuilder.Entity<ShadowForeignIntKey>();
            modelBuilder.Entity<ShadowIntNonKey>().Property<int>("Int");

            modelBuilder.Entity<NullableIntKey>();
            modelBuilder.Entity<AlternateNullableIntKey>().HasAlternateKey(e => e.AlternateId);
            modelBuilder.Entity<ForeignNullableIntKey>();
            modelBuilder.Entity<NullableIntNonKey>();

            modelBuilder.Entity<ShadowNullableIntKey>().Property<int?>("Id").ValueGeneratedNever();
            modelBuilder.Entity<ShadowAlternateNullableIntKey>(
                b =>
                {
                    b.Property<int?>("AlternateId");
                    b.HasAlternateKey("AlternateId");
                });
            modelBuilder.Entity<ShadowForeignNullableIntKey>();
            modelBuilder.Entity<ShadowNullableIntNonKey>().Property<int?>("NullableInt");

            modelBuilder.Entity<StringKey>();
            modelBuilder.Entity<AlternateStringKey>().HasAlternateKey(e => e.AlternateId);
            modelBuilder.Entity<ForeignStringKey>();
            modelBuilder.Entity<StringNonKey>();

            modelBuilder.Entity<ShadowStringKey>().Property<string>("Id").ValueGeneratedNever();
            modelBuilder.Entity<ShadowAlternateStringKey>(
                b =>
                {
                    b.Property<string>("AlternateId");
                    b.HasAlternateKey("AlternateId");
                });
            modelBuilder.Entity<ShadowForeignStringKey>();
            modelBuilder.Entity<ShadowStringNonKey>().Property<string>("String");

            modelBuilder.Entity<CompositeKey>()
                .HasKey(
                    e => new
                    {
                        e.Id1,
                        e.Id2,
                        e.Foo
                    });

            modelBuilder.Entity<AlternateCompositeKey>()
                .HasAlternateKey(
                    e => new
                    {
                        e.AlternateId1,
                        e.AlternateId2,
                        e.AlternateFoo
                    });

            modelBuilder.Entity<ForeignCompositeKey>()
                .HasOne(e => e.CompositeKey)
                .WithMany()
                .HasForeignKey(
                    e => new
                    {
                        e.CompositeKeyId1,
                        e.CompositeKeyId2,
                        e.CompositeKeyFoo
                    });

            modelBuilder.Entity<CompositeNonKey>();

            modelBuilder.Entity<ShadowCompositeKey>(
                b =>
                {
                    b.Property<int>("Id1");
                    b.Property<string>("Id2");
                    b.Property<string>("Foo");
                    b.HasKey("Id1", "Id2", "Foo");
                });

            modelBuilder.Entity<ShadowAlternateCompositeKey>(
                b =>
                {
                    b.Property<int>("AlternateId1");
                    b.Property<string>("AlternateId2");
                    b.Property<string>("AlternateFoo");
                    b.HasAlternateKey("AlternateId1", "AlternateId2", "AlternateFoo");
                });

            modelBuilder.Entity<ShadowForeignCompositeKey>()
                .HasOne(e => e.CompositeKey)
                .WithMany()
                .HasForeignKey("CompositeKeyId1", "CompositeKeyId2", "CompositeKeyFoo");

            modelBuilder.Entity<ShadowCompositeNonKey>().Property<string>("String");
            modelBuilder.Entity<ShadowCompositeNonKey>(
                b =>
                {
                    b.Property<int?>("Int");
                    b.Property<string?>("String");
                    b.Property<string?>("Foo");
                });

            modelBuilder.Entity<BaseType>();
            modelBuilder.Entity<DerivedType>();
        }

        protected internal override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder
                .UseInternalServiceProvider(_serviceProvider)
                .UseInMemoryDatabase(nameof(FindContext));
    }

    private class FindContextShared : DbContext
    {
        private readonly IServiceProvider _serviceProvider;

        public FindContextShared()
        {
            _serviceProvider = InMemoryTestHelpers.Instance.CreateServiceProvider();
        }

        protected internal override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.SharedTypeEntity<IntKey>("IntKeyA");
            modelBuilder.SharedTypeEntity<IntKey>("IntKeyB");
            modelBuilder.SharedTypeEntity<AlternateIntKey>("AlternateIntKeyA").HasAlternateKey(e => e.AlternateId);
            modelBuilder.SharedTypeEntity<AlternateIntKey>("AlternateIntKeyB").HasAlternateKey(e => e.AlternateId);

            modelBuilder.SharedTypeEntity<ForeignIntKey>("ForeignIntKeyA").HasOne("IntKeyA", "IntKey").WithMany();
            modelBuilder.SharedTypeEntity<ForeignIntKey>("ForeignIntKeyB").HasOne("IntKeyB", "IntKey").WithMany();

            modelBuilder.SharedTypeEntity<IntNonKey>("IntNonKeyA");
            modelBuilder.SharedTypeEntity<IntNonKey>("IntNonKeyB");

            modelBuilder.SharedTypeEntity<ShadowIntKey>("ShadowIntKeyA").Property<int>("Id").ValueGeneratedNever();
            modelBuilder.SharedTypeEntity<ShadowIntKey>("ShadowIntKeyB").Property<int>("Id").ValueGeneratedNever();

            modelBuilder.SharedTypeEntity<ShadowAlternateIntKey>(
                "ShadowAlternateIntKeyA",
                b =>
                {
                    b.Property<int>("AlternateId");
                    b.HasAlternateKey("AlternateId");
                });
            modelBuilder.SharedTypeEntity<ShadowAlternateIntKey>(
                "ShadowAlternateIntKeyB",
                b =>
                {
                    b.Property<int>("AlternateId");
                    b.HasAlternateKey("AlternateId");
                });

            modelBuilder.SharedTypeEntity<ShadowForeignIntKey>("ShadowForeignIntKeyA").HasOne("ShadowIntKeyA", "IntKey").WithMany();
            modelBuilder.SharedTypeEntity<ShadowForeignIntKey>("ShadowForeignIntKeyB").HasOne("ShadowIntKeyB", "IntKey").WithMany();

            modelBuilder.SharedTypeEntity<ShadowIntNonKey>("ShadowIntNonKeyA").Property<int>("Int");
            modelBuilder.SharedTypeEntity<ShadowIntNonKey>("ShadowIntNonKeyB").Property<int>("Int");

            modelBuilder.SharedTypeEntity<NullableIntKey>("NullableIntKeyA");
            modelBuilder.SharedTypeEntity<NullableIntKey>("NullableIntKeyB");
            modelBuilder.SharedTypeEntity<AlternateNullableIntKey>("AlternateNullableIntKeyA").HasAlternateKey(e => e.AlternateId);
            modelBuilder.SharedTypeEntity<AlternateNullableIntKey>("AlternateNullableIntKeyB").HasAlternateKey(e => e.AlternateId);

            modelBuilder.SharedTypeEntity<ForeignNullableIntKey>("ForeignNullableIntKeyA").HasOne("NullableIntKeyA", "NullableIntKey")
                .WithMany();
            modelBuilder.SharedTypeEntity<ForeignNullableIntKey>("ForeignNullableIntKeyB").HasOne("NullableIntKeyB", "NullableIntKey")
                .WithMany();

            modelBuilder.SharedTypeEntity<NullableIntNonKey>("NullableIntNonKeyA");
            modelBuilder.SharedTypeEntity<NullableIntNonKey>("NullableIntNonKeyB");

            modelBuilder.SharedTypeEntity<ShadowNullableIntKey>("ShadowNullableIntKeyA").Property<int?>("Id").ValueGeneratedNever();
            modelBuilder.SharedTypeEntity<ShadowNullableIntKey>("ShadowNullableIntKeyB").Property<int?>("Id").ValueGeneratedNever();

            modelBuilder.SharedTypeEntity<ShadowAlternateNullableIntKey>(
                "ShadowAlternateNullableIntKeyA",
                b =>
                {
                    b.Property<int?>("AlternateId");
                    b.HasAlternateKey("AlternateId");
                });
            modelBuilder.SharedTypeEntity<ShadowAlternateNullableIntKey>(
                "ShadowAlternateNullableIntKeyB",
                b =>
                {
                    b.Property<int?>("AlternateId");
                    b.HasAlternateKey("AlternateId");
                });

            modelBuilder.SharedTypeEntity<ShadowForeignNullableIntKey>("ShadowForeignNullableIntKeyA")
                .HasOne("ShadowNullableIntKeyA", "NullableIntKey").WithMany();
            modelBuilder.SharedTypeEntity<ShadowForeignNullableIntKey>("ShadowForeignNullableIntKeyB")
                .HasOne("ShadowNullableIntKeyB", "NullableIntKey").WithMany();

            modelBuilder.SharedTypeEntity<ShadowNullableIntNonKey>("ShadowNullableIntNonKeyA").Property<int?>("NullableInt");
            modelBuilder.SharedTypeEntity<ShadowNullableIntNonKey>("ShadowNullableIntNonKeyB").Property<int?>("NullableInt");

            modelBuilder.SharedTypeEntity<StringKey>("StringKeyA");
            modelBuilder.SharedTypeEntity<StringKey>("StringKeyB");
            modelBuilder.SharedTypeEntity<AlternateStringKey>("AlternateStringKeyA").HasAlternateKey(e => e.AlternateId);
            modelBuilder.SharedTypeEntity<AlternateStringKey>("AlternateStringKeyB").HasAlternateKey(e => e.AlternateId);

            modelBuilder.SharedTypeEntity<ForeignStringKey>("ForeignStringKeyA").HasOne("StringKeyA", "StringKey").WithMany();
            modelBuilder.SharedTypeEntity<ForeignStringKey>("ForeignStringKeyB").HasOne("StringKeyB", "StringKey").WithMany();

            modelBuilder.SharedTypeEntity<StringNonKey>("StringNonKeyA");
            modelBuilder.SharedTypeEntity<StringNonKey>("StringNonKeyB");

            modelBuilder.SharedTypeEntity<ShadowStringKey>("ShadowStringKeyA").Property<string>("Id").ValueGeneratedNever();
            modelBuilder.SharedTypeEntity<ShadowStringKey>("ShadowStringKeyB").Property<string>("Id").ValueGeneratedNever();

            modelBuilder.SharedTypeEntity<ShadowAlternateStringKey>(
                "ShadowAlternateStringKeyA",
                b =>
                {
                    b.Property<string>("AlternateId");
                    b.HasAlternateKey("AlternateId");
                });
            modelBuilder.SharedTypeEntity<ShadowAlternateStringKey>(
                "ShadowAlternateStringKeyB",
                b =>
                {
                    b.Property<string>("AlternateId");
                    b.HasAlternateKey("AlternateId");
                });

            modelBuilder.SharedTypeEntity<ShadowForeignStringKey>("ShadowForeignStringKeyA").HasOne("ShadowStringKeyA", "StringKey")
                .WithMany();
            modelBuilder.SharedTypeEntity<ShadowForeignStringKey>("ShadowForeignStringKeyB").HasOne("ShadowStringKeyB", "StringKey")
                .WithMany();

            modelBuilder.SharedTypeEntity<ShadowStringNonKey>("ShadowStringNonKeyA").Property<string>("String");
            modelBuilder.SharedTypeEntity<ShadowStringNonKey>("ShadowStringNonKeyB").Property<string>("String");

            modelBuilder.SharedTypeEntity<CompositeKey>("CompositeKeyA")
                .HasKey(
                    e => new
                    {
                        e.Id1,
                        e.Id2,
                        e.Foo
                    });
            modelBuilder.SharedTypeEntity<CompositeKey>("CompositeKeyB")
                .HasKey(
                    e => new
                    {
                        e.Id1,
                        e.Id2,
                        e.Foo
                    });

            modelBuilder.SharedTypeEntity<AlternateCompositeKey>("AlternateCompositeKeyA")
                .HasAlternateKey(
                    e => new
                    {
                        e.AlternateId1,
                        e.AlternateId2,
                        e.AlternateFoo
                    });
            modelBuilder.SharedTypeEntity<AlternateCompositeKey>("AlternateCompositeKeyB")
                .HasAlternateKey(
                    e => new
                    {
                        e.AlternateId1,
                        e.AlternateId2,
                        e.AlternateFoo
                    });

            modelBuilder.SharedTypeEntity<ForeignCompositeKey>("ForeignCompositeKeyA")
                .HasOne("CompositeKeyA", "CompositeKey")
                .WithMany()
                .HasForeignKey("CompositeKeyId1", "CompositeKeyId2", "CompositeKeyFoo");
            modelBuilder.SharedTypeEntity<ForeignCompositeKey>("ForeignCompositeKeyB")
                .HasOne("CompositeKeyB", "CompositeKey")
                .WithMany()
                .HasForeignKey("CompositeKeyId1", "CompositeKeyId2", "CompositeKeyFoo");

            modelBuilder.SharedTypeEntity<CompositeNonKey>("CompositeNonKeyA");
            modelBuilder.SharedTypeEntity<CompositeNonKey>("CompositeNonKeyB");

            modelBuilder.SharedTypeEntity<ShadowCompositeKey>(
                "ShadowCompositeKeyA",
                b =>
                {
                    b.Property<int>("Id1");
                    b.Property<string>("Id2");
                    b.Property<string>("Foo");
                    b.HasKey("Id1", "Id2", "Foo");
                });
            modelBuilder.SharedTypeEntity<ShadowCompositeKey>(
                "ShadowCompositeKeyB",
                b =>
                {
                    b.Property<int>("Id1");
                    b.Property<string>("Id2");
                    b.Property<string>("Foo");
                    b.HasKey("Id1", "Id2", "Foo");
                });

            modelBuilder.SharedTypeEntity<ShadowAlternateCompositeKey>(
                "ShadowAlternateCompositeKeyA",
                b =>
                {
                    b.Property<int>("AlternateId1");
                    b.Property<string>("AlternateId2");
                    b.Property<string>("AlternateFoo");
                    b.HasAlternateKey("AlternateId1", "AlternateId2", "AlternateFoo");
                });
            modelBuilder.SharedTypeEntity<ShadowAlternateCompositeKey>(
                "ShadowAlternateCompositeKeyB",
                b =>
                {
                    b.Property<int>("AlternateId1");
                    b.Property<string>("AlternateId2");
                    b.Property<string>("AlternateFoo");
                    b.HasAlternateKey("AlternateId1", "AlternateId2", "AlternateFoo");
                });

            modelBuilder.SharedTypeEntity<ShadowForeignCompositeKey>("ShadowForeignCompositeKeyA")
                .HasOne("ShadowCompositeKeyA", "CompositeKey")
                .WithMany()
                .HasForeignKey("CompositeKeyId1", "CompositeKeyId2", "CompositeKeyFoo");
            modelBuilder.SharedTypeEntity<ShadowForeignCompositeKey>("ShadowForeignCompositeKeyB")
                .HasOne("ShadowCompositeKeyB", "CompositeKey")
                .WithMany()
                .HasForeignKey("CompositeKeyId1", "CompositeKeyId2", "CompositeKeyFoo");

            modelBuilder.SharedTypeEntity<ShadowCompositeNonKey>("ShadowCompositeNonKeyA").Property<string>("String");
            modelBuilder.SharedTypeEntity<ShadowCompositeNonKey>("ShadowCompositeNonKeyB").Property<string>("String");

            modelBuilder.SharedTypeEntity<ShadowCompositeNonKey>(
                "ShadowCompositeNonKeyA",
                b =>
                {
                    b.Property<int?>("Int");
                    b.Property<string?>("String");
                    b.Property<string?>("Foo");
                });
            modelBuilder.SharedTypeEntity<ShadowCompositeNonKey>(
                "ShadowCompositeNonKeyB",
                b =>
                {
                    b.Property<int?>("Int");
                    b.Property<string?>("String");
                    b.Property<string?>("Foo");
                });
        }

        protected internal override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder
                .UseInternalServiceProvider(_serviceProvider)
                .UseInMemoryDatabase(nameof(FindContextShared));
    }
}
