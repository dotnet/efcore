// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable enable

using System.ComponentModel.DataAnnotations.Schema;

namespace Microsoft.EntityFrameworkCore;

public class StoreGeneratedInMemoryTest
{
    [ConditionalFact]
    public virtual void Value_generation_works_for_common_GUID_conversions()
    {
        ValueGenerationPositive<Guid, GuidToString>();
        ValueGenerationPositive<Guid, GuidToBytes>();
    }

    private void ValueGenerationPositive<TKey, TEntity>()
        where TEntity : WithConverter<TKey>, new()
    {
        var databaseName = Guid.NewGuid().ToString();
        TKey? id;

        using (var context = new StoreContext(databaseName))
        {
            var entity = context.Add(new TEntity()).Entity;

            context.SaveChanges();

            id = entity.Id;
        }

        using (var context = new StoreContext(databaseName))
        {
            Assert.Equal(id, context.Set<TEntity>().Single(e => e.Id!.Equals(id)).Id);
        }
    }

    protected class NonStoreGenDependent
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        public int? StoreGenPrincipalId { get; set; }
        public StoreGenPrincipal StoreGenPrincipal { get; set; } = null!;
    }

    protected class StoreGenPrincipal
    {
        public int Id { get; set; }
    }

    protected class WithConverter<TKey>
    {
        public TKey? Id { get; set; }
    }

    protected class IntToString : WithConverter<int>;

    protected class GuidToString : WithConverter<Guid>;

    protected class GuidToBytes : WithConverter<Guid>;

    protected class ShortToBytes : WithConverter<short>;

    protected class WrappedIntClass
    {
        public int Value { get; set; }
    }

    protected class WrappedIntClassConverter : ValueConverter<WrappedIntClass, int>
    {
        public WrappedIntClassConverter()
            : base(
                v => v.Value,
                v => new WrappedIntClass { Value = v })
        {
        }
    }

    protected class WrappedIntClassComparer : ValueComparer<WrappedIntClass?>
    {
        public WrappedIntClassComparer()
            : base(
                (v1, v2) => (v1 == null && v2 == null) || (v1 != null && v2 != null && v1.Value.Equals(v2.Value)),
                v => v != null ? v.Value : 0,
                v => v == null ? null : new WrappedIntClass { Value = v.Value })
        {
        }
    }

    protected class WrappedIntClassValueGenerator : ValueGenerator<WrappedIntClass>
    {
        public override WrappedIntClass Next(EntityEntry entry)
            => new() { Value = 66 };

        public override bool GeneratesTemporaryValues
            => false;
    }

    protected struct WrappedIntStruct
    {
        public int Value { get; set; }
    }

    protected class WrappedIntStructConverter : ValueConverter<WrappedIntStruct, int>
    {
        public WrappedIntStructConverter()
            : base(
                v => v.Value,
                v => new WrappedIntStruct { Value = v })
        {
        }
    }

    protected class WrappedIntStructValueGenerator : ValueGenerator<WrappedIntStruct>
    {
        public override WrappedIntStruct Next(EntityEntry entry)
            => new() { Value = 66 };

        public override bool GeneratesTemporaryValues
            => false;
    }

    protected record WrappedIntRecord
    {
        public int Value { get; set; }
    }

    protected class WrappedIntRecordConverter : ValueConverter<WrappedIntRecord, int>
    {
        public WrappedIntRecordConverter()
            : base(
                v => v.Value,
                v => new WrappedIntRecord { Value = v })
        {
        }
    }

    protected class WrappedIntRecordValueGenerator : ValueGenerator<WrappedIntRecord>
    {
        public override WrappedIntRecord Next(EntityEntry entry)
            => new() { Value = 66 };

        public override bool GeneratesTemporaryValues
            => false;
    }

    protected class WrappedIntKeyClass
    {
        public int Value { get; set; }
    }

    protected class WrappedIntKeyClassConverter : ValueConverter<WrappedIntKeyClass, int>
    {
        public WrappedIntKeyClassConverter()
            : base(
                v => v.Value,
                v => new WrappedIntKeyClass { Value = v })
        {
        }
    }

    protected class WrappedIntKeyClassComparer : ValueComparer<WrappedIntKeyClass?>
    {
        public WrappedIntKeyClassComparer()
            : base(
                (v1, v2) => (v1 == null && v2 == null) || (v1 != null && v2 != null && v1.Value.Equals(v2.Value)),
                v => v != null ? v.Value : 0,
                v => v == null ? null : new WrappedIntKeyClass { Value = v.Value })
        {
        }
    }

    protected struct WrappedIntKeyStruct
    {
        public int Value { get; set; }

        public override bool Equals(object? obj)
            => obj is WrappedIntKeyStruct other && Value == other.Value;

        public override int GetHashCode()
            => Value;

        public static bool operator ==(WrappedIntKeyStruct left, WrappedIntKeyStruct right)
            => left.Equals(right);

        public static bool operator !=(WrappedIntKeyStruct left, WrappedIntKeyStruct right)
            => !left.Equals(right);
    }

    protected class WrappedIntKeyStructConverter : ValueConverter<WrappedIntKeyStruct, int>
    {
        public WrappedIntKeyStructConverter()
            : base(
                v => v.Value,
                v => new WrappedIntKeyStruct { Value = v })
        {
        }
    }

    protected record WrappedIntKeyRecord
    {
        public int Value { get; set; }
    }

    protected class WrappedIntKeyRecordConverter : ValueConverter<WrappedIntKeyRecord, int>
    {
        public WrappedIntKeyRecordConverter()
            : base(
                v => v.Value,
                v => new WrappedIntKeyRecord { Value = v })
        {
        }
    }

    protected class WrappedIntClassPrincipal
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public WrappedIntKeyClass Id { get; set; } = null!;

        public WrappedIntClass? NonKey { get; set; }
        public ICollection<WrappedIntClassDependentShadow> Dependents { get; } = new List<WrappedIntClassDependentShadow>();
        public ICollection<WrappedIntClassDependentRequired> RequiredDependents { get; } = new List<WrappedIntClassDependentRequired>();
        public ICollection<WrappedIntClassDependentOptional> OptionalDependents { get; } = new List<WrappedIntClassDependentOptional>();
    }

    protected class WrappedIntClassDependentShadow
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public WrappedIntClass Id { get; set; } = null!;

        public WrappedIntClassPrincipal? Principal { get; set; }
    }

    protected class WrappedIntClassDependentRequired
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public WrappedIntClass Id { get; set; } = null!;

        public WrappedIntKeyClass PrincipalId { get; set; } = null!;
        public WrappedIntClassPrincipal Principal { get; set; } = null!;
    }

    protected class WrappedIntClassDependentOptional
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public WrappedIntClass Id { get; set; } = null!;

        public WrappedIntKeyClass? PrincipalId { get; set; }
        public WrappedIntClassPrincipal? Principal { get; set; }
    }

    protected class WrappedIntStructPrincipal
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public WrappedIntKeyStruct Id { get; set; }

        public WrappedIntStruct NonKey { get; set; }
        public ICollection<WrappedIntStructDependentShadow> Dependents { get; } = new List<WrappedIntStructDependentShadow>();
        public ICollection<WrappedIntStructDependentOptional> OptionalDependents { get; } = new List<WrappedIntStructDependentOptional>();
        public ICollection<WrappedIntStructDependentRequired> RequiredDependents { get; } = new List<WrappedIntStructDependentRequired>();
    }

    protected class WrappedIntStructDependentShadow
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public WrappedIntStruct Id { get; set; }

        public WrappedIntStructPrincipal? Principal { get; set; }
    }

    protected class WrappedIntStructDependentOptional
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public WrappedIntStruct Id { get; set; }

        public WrappedIntKeyStruct? PrincipalId { get; set; }
        public WrappedIntStructPrincipal? Principal { get; set; }
    }

    protected class WrappedIntStructDependentRequired
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public WrappedIntStruct Id { get; set; }

        public WrappedIntKeyStruct PrincipalId { get; set; }
        public WrappedIntStructPrincipal Principal { get; set; } = null!;
    }

    protected class WrappedIntRecordPrincipal
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public WrappedIntKeyRecord Id { get; set; } = null!;

        public WrappedIntRecord? NonKey { get; set; }
        public ICollection<WrappedIntRecordDependentShadow> Dependents { get; } = new List<WrappedIntRecordDependentShadow>();
        public ICollection<WrappedIntRecordDependentOptional> OptionalDependents { get; } = new List<WrappedIntRecordDependentOptional>();
        public ICollection<WrappedIntRecordDependentRequired> RequiredDependents { get; } = new List<WrappedIntRecordDependentRequired>();
    }

    protected class WrappedIntRecordDependentShadow
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public WrappedIntRecord Id { get; set; } = null!;

        public WrappedIntRecordPrincipal? Principal { get; set; }
    }

    protected class WrappedIntRecordDependentOptional
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public WrappedIntRecord Id { get; set; } = null!;

        public WrappedIntKeyRecord? PrincipalId { get; set; }
        public WrappedIntRecordPrincipal? Principal { get; set; }
    }

    protected class WrappedIntRecordDependentRequired
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public WrappedIntRecord Id { get; set; } = null!;

        public WrappedIntKeyRecord PrincipalId { get; set; } = null!;
        public WrappedIntRecordPrincipal Principal { get; set; } = null!;
    }

    [ConditionalFact]
    public virtual void Insert_update_and_delete_with_wrapped_int_key()
    {
        var databaseName = Guid.NewGuid().ToString();

        var id1 = 0;
        var id2 = 0;
        var id3 = 0;

        using (var context = new StoreContext(databaseName))
        {
            var principal1 = context.Add(
                new WrappedIntClassPrincipal
                {
                    Dependents = { new WrappedIntClassDependentShadow(), new WrappedIntClassDependentShadow() },
                    OptionalDependents = { new WrappedIntClassDependentOptional(), new WrappedIntClassDependentOptional() },
                    RequiredDependents = { new WrappedIntClassDependentRequired(), new WrappedIntClassDependentRequired() }
                }).Entity;

            var principal2 = context.Add(
                new WrappedIntStructPrincipal
                {
                    Dependents = { new WrappedIntStructDependentShadow(), new WrappedIntStructDependentShadow() },
                    OptionalDependents = { new WrappedIntStructDependentOptional(), new WrappedIntStructDependentOptional() },
                    RequiredDependents = { new WrappedIntStructDependentRequired(), new WrappedIntStructDependentRequired() }
                }).Entity;

            var principal3 = context.Add(
                new WrappedIntRecordPrincipal
                {
                    Dependents = { new WrappedIntRecordDependentShadow(), new WrappedIntRecordDependentShadow() },
                    OptionalDependents = { new WrappedIntRecordDependentOptional(), new WrappedIntRecordDependentOptional() },
                    RequiredDependents = { new WrappedIntRecordDependentRequired(), new WrappedIntRecordDependentRequired() }
                }).Entity;

            context.SaveChanges();

            id1 = principal1.Id.Value;
            Assert.NotEqual(0, id1);
            foreach (var dependent in principal1.Dependents)
            {
                Assert.NotEqual(0, dependent.Id.Value);
                Assert.Same(principal1, dependent.Principal);
                Assert.Equal(id1, context.Entry(dependent).Property<WrappedIntKeyClass?>("PrincipalId").CurrentValue!.Value);
            }

            foreach (var dependent in principal1.OptionalDependents)
            {
                Assert.NotEqual(0, dependent.Id.Value);
                Assert.Same(principal1, dependent.Principal);
                Assert.Equal(id1, dependent.PrincipalId!.Value);
            }

            foreach (var dependent in principal1.RequiredDependents)
            {
                Assert.NotEqual(0, dependent.Id.Value);
                Assert.Same(principal1, dependent.Principal);
                Assert.Equal(id1, dependent.PrincipalId.Value);
            }

            Assert.Equal(66, principal1.NonKey!.Value);

            id2 = principal2.Id.Value;
            Assert.NotEqual(0, id2);
            foreach (var dependent in principal2.Dependents)
            {
                Assert.NotEqual(0, dependent.Id.Value);
                Assert.Same(principal2, dependent.Principal);
                Assert.Equal(id2, context.Entry(dependent).Property<WrappedIntKeyStruct?>("PrincipalId").CurrentValue!.Value.Value);
            }

            foreach (var dependent in principal2.OptionalDependents)
            {
                Assert.NotEqual(0, dependent.Id.Value);
                Assert.Same(principal2, dependent.Principal);
                Assert.Equal(id2, dependent.PrincipalId!.Value.Value);
            }

            foreach (var dependent in principal2.RequiredDependents)
            {
                Assert.NotEqual(0, dependent.Id.Value);
                Assert.Same(principal2, dependent.Principal);
                Assert.Equal(id2, dependent.PrincipalId.Value);
            }

            Assert.Equal(66, principal2.NonKey.Value);

            id3 = principal3.Id.Value;
            Assert.NotEqual(0, id3);
            foreach (var dependent in principal3.Dependents)
            {
                Assert.NotEqual(0, dependent.Id.Value);
                Assert.Same(principal3, dependent.Principal);
                Assert.Equal(id3, context.Entry(dependent).Property<WrappedIntKeyRecord?>("PrincipalId").CurrentValue!.Value);
            }

            foreach (var dependent in principal3.OptionalDependents)
            {
                Assert.NotEqual(0, dependent.Id.Value);
                Assert.Same(principal3, dependent.Principal);
                Assert.Equal(id3, dependent.PrincipalId!.Value);
            }

            foreach (var dependent in principal3.RequiredDependents)
            {
                Assert.NotEqual(0, dependent.Id.Value);
                Assert.Same(principal3, dependent.Principal);
                Assert.Equal(id3, dependent.PrincipalId.Value);
            }

            Assert.Equal(66, principal3.NonKey!.Value);
        }

        using (var context = new StoreContext(databaseName))
        {
            var principal1 = context.Set<WrappedIntClassPrincipal>()
                .Include(e => e.Dependents)
                .Include(e => e.OptionalDependents)
                .Include(e => e.RequiredDependents)
                .Single();

            Assert.Equal(principal1.Id.Value, id1);
            foreach (var dependent in principal1.Dependents)
            {
                Assert.Same(principal1, dependent.Principal);
                Assert.Equal(id1, context.Entry(dependent).Property<WrappedIntKeyClass?>("PrincipalId").CurrentValue!.Value);
            }

            foreach (var dependent in principal1.OptionalDependents)
            {
                Assert.Same(principal1, dependent.Principal);
                Assert.Equal(id1, dependent.PrincipalId!.Value);
            }

            foreach (var dependent in principal1.RequiredDependents)
            {
                Assert.Same(principal1, dependent.Principal);
                Assert.Equal(id1, dependent.PrincipalId.Value);
            }

            var principal2 = context.Set<WrappedIntStructPrincipal>()
                .Include(e => e.Dependents)
                .Include(e => e.OptionalDependents)
                .Include(e => e.RequiredDependents)
                .Single();

            Assert.Equal(principal2.Id.Value, id2);
            foreach (var dependent in principal2.Dependents)
            {
                Assert.Same(principal2, dependent.Principal);
                Assert.Equal(id2, context.Entry(dependent).Property<WrappedIntKeyStruct?>("PrincipalId").CurrentValue!.Value.Value);
            }

            foreach (var dependent in principal2.OptionalDependents)
            {
                Assert.Same(principal2, dependent.Principal);
                Assert.Equal(id2, dependent.PrincipalId!.Value.Value);
            }

            foreach (var dependent in principal2.RequiredDependents)
            {
                Assert.Same(principal2, dependent.Principal);
                Assert.Equal(id2, dependent.PrincipalId.Value);
            }

            var principal3 = context.Set<WrappedIntRecordPrincipal>()
                .Include(e => e.Dependents)
                .Include(e => e.OptionalDependents)
                .Include(e => e.RequiredDependents)
                .Single();

            Assert.Equal(principal3.Id.Value, id3);
            foreach (var dependent in principal3.Dependents)
            {
                Assert.Same(principal3, dependent.Principal);
                Assert.Equal(id3, context.Entry(dependent).Property<WrappedIntKeyRecord?>("PrincipalId").CurrentValue!.Value);
            }

            foreach (var dependent in principal3.OptionalDependents)
            {
                Assert.Same(principal3, dependent.Principal);
                Assert.Equal(id3, dependent.PrincipalId!.Value);
            }

            foreach (var dependent in principal3.RequiredDependents)
            {
                Assert.Same(principal3, dependent.Principal);
                Assert.Equal(id3, dependent.PrincipalId.Value);
            }

            principal1.Dependents.Remove(principal1.Dependents.First());
            principal2.Dependents.Remove(principal2.Dependents.First());
            principal3.Dependents.Remove(principal3.Dependents.First());

            principal1.OptionalDependents.Remove(principal1.OptionalDependents.First());
            principal2.OptionalDependents.Remove(principal2.OptionalDependents.First());
            principal3.OptionalDependents.Remove(principal3.OptionalDependents.First());

            principal1.RequiredDependents.Remove(principal1.RequiredDependents.First());
            principal2.RequiredDependents.Remove(principal2.RequiredDependents.First());
            principal3.RequiredDependents.Remove(principal3.RequiredDependents.First());

            context.SaveChanges();
        }

        using (var context = new StoreContext(databaseName))
        {
            var dependents1 = context.Set<WrappedIntClassDependentShadow>().Include(e => e.Principal).ToList();
            Assert.Equal(2, dependents1.Count);
            Assert.Null(
                context.Entry(dependents1.Single(e => e.Principal == null))
                    .Property<WrappedIntKeyClass?>("PrincipalId").CurrentValue);

            var optionalDependents1 = context.Set<WrappedIntClassDependentOptional>().Include(e => e.Principal).ToList();
            Assert.Equal(2, optionalDependents1.Count);
            Assert.Null(optionalDependents1.Single(e => e.Principal == null).PrincipalId);

            var requiredDependents1 = context.Set<WrappedIntClassDependentRequired>().Include(e => e.Principal).ToList();
            Assert.Single(requiredDependents1);

            var dependents2 = context.Set<WrappedIntStructDependentShadow>().Include(e => e.Principal).ToList();
            Assert.Equal(2, dependents2.Count);
            Assert.Null(
                context.Entry(dependents2.Single(e => e.Principal == null))
                    .Property<WrappedIntKeyStruct?>("PrincipalId").CurrentValue);

            var optionalDependents2 = context.Set<WrappedIntStructDependentOptional>().Include(e => e.Principal).ToList();
            Assert.Equal(2, optionalDependents2.Count);
            Assert.Null(optionalDependents2.Single(e => e.Principal == null).PrincipalId);

            var requiredDependents2 = context.Set<WrappedIntStructDependentRequired>().Include(e => e.Principal).ToList();
            Assert.Single(requiredDependents2);

            var dependents3 = context.Set<WrappedIntRecordDependentShadow>().Include(e => e.Principal).ToList();
            Assert.Equal(2, dependents3.Count);
            Assert.Null(
                context.Entry(dependents3.Single(e => e.Principal == null))
                    .Property<WrappedIntKeyRecord?>("PrincipalId").CurrentValue);

            var optionalDependents3 = context.Set<WrappedIntRecordDependentOptional>().Include(e => e.Principal).ToList();
            Assert.Equal(2, optionalDependents3.Count);
            Assert.Null(optionalDependents3.Single(e => e.Principal == null).PrincipalId);

            var requiredDependents3 = context.Set<WrappedIntRecordDependentRequired>().Include(e => e.Principal).ToList();
            Assert.Single(requiredDependents3);

            context.Remove(dependents1.Single(e => e.Principal != null));
            context.Remove(optionalDependents1.Single(e => e.Principal != null));
            context.Remove(requiredDependents1.Single());
            context.Remove(requiredDependents1.Single().Principal);

            context.Remove(dependents2.Single(e => e.Principal != null));
            context.Remove(optionalDependents2.Single(e => e.Principal != null));
            context.Remove(requiredDependents2.Single());
            context.Remove(requiredDependents2.Single().Principal);

            context.Remove(dependents3.Single(e => e.Principal != null));
            context.Remove(optionalDependents3.Single(e => e.Principal != null));
            context.Remove(requiredDependents3.Single());
            context.Remove(requiredDependents3.Single().Principal);

            context.SaveChanges();
        }

        using (var context = new StoreContext(databaseName))
        {
            Assert.Equal(1, context.Set<WrappedIntClassDependentShadow>().Count());
            Assert.Equal(1, context.Set<WrappedIntStructDependentShadow>().Count());
            Assert.Equal(1, context.Set<WrappedIntRecordDependentShadow>().Count());

            Assert.Equal(1, context.Set<WrappedIntClassDependentOptional>().Count());
            Assert.Equal(1, context.Set<WrappedIntStructDependentOptional>().Count());
            Assert.Equal(1, context.Set<WrappedIntRecordDependentOptional>().Count());

            Assert.Equal(0, context.Set<WrappedIntClassDependentRequired>().Count());
            Assert.Equal(0, context.Set<WrappedIntStructDependentRequired>().Count());
            Assert.Equal(0, context.Set<WrappedIntRecordDependentRequired>().Count());
        }
    }

    protected class LongToIntPrincipal
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        public ICollection<LongToIntDependentShadow> Dependents { get; } = new List<LongToIntDependentShadow>();
        public ICollection<LongToIntDependentRequired> RequiredDependents { get; } = new List<LongToIntDependentRequired>();
        public ICollection<LongToIntDependentOptional> OptionalDependents { get; } = new List<LongToIntDependentOptional>();
    }

    protected class LongToIntDependentShadow
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        public LongToIntPrincipal? Principal { get; set; }
    }

    protected class LongToIntDependentRequired
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        public long PrincipalId { get; set; }
        public LongToIntPrincipal Principal { get; set; } = null!;
    }

    protected class LongToIntDependentOptional
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        public long? PrincipalId { get; set; }
        public LongToIntPrincipal? Principal { get; set; }
    }

    [ConditionalFact]
    public virtual void Insert_update_and_delete_with_long_to_int_conversion()
    {
        var databaseName = Guid.NewGuid().ToString();

        var id1 = 0L;
        using (var context = new StoreContext(databaseName))
        {
            var principal1 = context.Add(
                new LongToIntPrincipal
                {
                    Dependents = { new LongToIntDependentShadow(), new LongToIntDependentShadow() },
                    OptionalDependents = { new LongToIntDependentOptional(), new LongToIntDependentOptional() },
                    RequiredDependents = { new LongToIntDependentRequired(), new LongToIntDependentRequired() }
                }).Entity;

            context.SaveChanges();

            id1 = principal1.Id;
            Assert.NotEqual(0L, id1);
            foreach (var dependent in principal1.Dependents)
            {
                Assert.NotEqual(0L, dependent.Id);
                Assert.Same(principal1, dependent.Principal);
                Assert.Equal(id1, context.Entry(dependent).Property<long?>("PrincipalId").CurrentValue!.Value);
            }

            foreach (var dependent in principal1.OptionalDependents)
            {
                Assert.NotEqual(0L, dependent.Id);
                Assert.Same(principal1, dependent.Principal);
                Assert.Equal(id1, dependent.PrincipalId);
            }

            foreach (var dependent in principal1.RequiredDependents)
            {
                Assert.NotEqual(0L, dependent.Id);
                Assert.Same(principal1, dependent.Principal);
                Assert.Equal(id1, dependent.PrincipalId);
            }
        }

        using (var context = new StoreContext(databaseName))
        {
            var principal1 = context.Set<LongToIntPrincipal>()
                .Include(e => e.Dependents)
                .Include(e => e.OptionalDependents)
                .Include(e => e.RequiredDependents)
                .Single();

            Assert.Equal(principal1.Id, id1);
            foreach (var dependent in principal1.Dependents)
            {
                Assert.Same(principal1, dependent.Principal);
                Assert.Equal(id1, context.Entry(dependent).Property<long?>("PrincipalId").CurrentValue!.Value);
            }

            foreach (var dependent in principal1.OptionalDependents)
            {
                Assert.Same(principal1, dependent.Principal);
                Assert.Equal(id1, dependent.PrincipalId!.Value);
            }

            foreach (var dependent in principal1.RequiredDependents)
            {
                Assert.Same(principal1, dependent.Principal);
                Assert.Equal(id1, dependent.PrincipalId);
            }

            principal1.Dependents.Remove(principal1.Dependents.First());
            principal1.OptionalDependents.Remove(principal1.OptionalDependents.First());
            principal1.RequiredDependents.Remove(principal1.RequiredDependents.First());

            context.SaveChanges();
        }

        using (var context = new StoreContext(databaseName))
        {
            var dependents1 = context.Set<LongToIntDependentShadow>().Include(e => e.Principal).ToList();
            Assert.Equal(2, dependents1.Count);
            Assert.Null(
                context.Entry(dependents1.Single(e => e.Principal == null))
                    .Property<long?>("PrincipalId").CurrentValue);

            var optionalDependents1 = context.Set<LongToIntDependentOptional>().Include(e => e.Principal).ToList();
            Assert.Equal(2, optionalDependents1.Count);
            Assert.Null(optionalDependents1.Single(e => e.Principal == null).PrincipalId);

            var requiredDependents1 = context.Set<LongToIntDependentRequired>().Include(e => e.Principal).ToList();
            Assert.Single(requiredDependents1);

            context.Remove(dependents1.Single(e => e.Principal != null));
            context.Remove(optionalDependents1.Single(e => e.Principal != null));
            context.Remove(requiredDependents1.Single());
            context.Remove(requiredDependents1.Single().Principal);

            context.SaveChanges();
        }

        using (var context = new StoreContext(databaseName))
        {
            Assert.Equal(1, context.Set<LongToIntDependentShadow>().Count());
            Assert.Equal(1, context.Set<LongToIntDependentOptional>().Count());
            Assert.Equal(0, context.Set<LongToIntDependentRequired>().Count());
        }
    }

    protected class WrappedStringClass
    {
        public string? Value { get; set; }
    }

    protected class WrappedStringClassConverter : ValueConverter<WrappedStringClass, string>
    {
        public WrappedStringClassConverter()
            : base(
                v => v.Value!,
                v => new WrappedStringClass { Value = v })
        {
        }
    }

    protected class WrappedStringClassComparer : ValueComparer<WrappedStringClass?>
    {
        public WrappedStringClassComparer()
            : base(
                (v1, v2) => (v1 == null && v2 == null) || (v1 != null && v2 != null && v1.Value!.Equals(v2.Value)),
                v => v != null ? v.Value!.GetHashCode() : 0,
                v => v == null ? null : new WrappedStringClass { Value = v.Value })
        {
        }
    }

    protected class WrappedStringClassValueGenerator : ValueGenerator<WrappedStringClass>
    {
        public override WrappedStringClass Next(EntityEntry entry)
            => new() { Value = "66" };

        public override bool GeneratesTemporaryValues
            => false;
    }

    protected struct WrappedStringStruct
    {
        public string? Value { get; set; }
    }

    protected class WrappedStringStructConverter : ValueConverter<WrappedStringStruct, string>
    {
        public WrappedStringStructConverter()
            : base(
                v => v.Value!,
                v => new WrappedStringStruct { Value = v })
        {
        }
    }

    protected class WrappedStringStructValueGenerator : ValueGenerator<WrappedStringStruct>
    {
        public override WrappedStringStruct Next(EntityEntry entry)
            => new() { Value = "66" };

        public override bool GeneratesTemporaryValues
            => false;
    }

    protected record WrappedStringRecord
    {
        public string? Value { get; set; }
    }

    protected class WrappedStringRecordConverter : ValueConverter<WrappedStringRecord, string>
    {
        public WrappedStringRecordConverter()
            : base(
                v => v.Value!,
                v => new WrappedStringRecord { Value = v })
        {
        }
    }

    protected class WrappedStringRecordValueGenerator : ValueGenerator<WrappedStringRecord>
    {
        public override WrappedStringRecord Next(EntityEntry entry)
            => new() { Value = "66" };

        public override bool GeneratesTemporaryValues
            => false;
    }

    protected class WrappedStringKeyClass
    {
        public string? Value { get; set; }
    }

    protected class WrappedStringKeyClassConverter : ValueConverter<WrappedStringKeyClass, string>
    {
        public WrappedStringKeyClassConverter()
            : base(
                v => v.Value!,
                v => new WrappedStringKeyClass { Value = v })
        {
        }
    }

    protected class WrappedStringKeyClassComparer : ValueComparer<WrappedStringKeyClass?>
    {
        public WrappedStringKeyClassComparer()
            : base(
                (v1, v2) => (v1 == null && v2 == null) || (v1 != null && v2 != null && v1.Value!.Equals(v2.Value)),
                v => v != null ? v.Value!.GetHashCode() : 0,
                v => v == null ? null : new WrappedStringKeyClass { Value = v.Value })
        {
        }
    }

    protected struct WrappedStringKeyStruct
    {
        public string Value { get; set; }

        public override bool Equals(object? obj)
            => obj is WrappedStringKeyStruct other && Value == other.Value;

        public override int GetHashCode()
            => Value.GetHashCode();

        public static bool operator ==(WrappedStringKeyStruct left, WrappedStringKeyStruct right)
            => left.Equals(right);

        public static bool operator !=(WrappedStringKeyStruct left, WrappedStringKeyStruct right)
            => !left.Equals(right);
    }

    protected class WrappedStringKeyStructConverter : ValueConverter<WrappedStringKeyStruct, string>
    {
        public WrappedStringKeyStructConverter()
            : base(
                v => v.Value,
                v => new WrappedStringKeyStruct { Value = v })
        {
        }
    }

    protected record WrappedStringKeyRecord
    {
        public string? Value { get; set; }
    }

    protected class WrappedStringKeyRecordConverter : ValueConverter<WrappedStringKeyRecord, string>
    {
        public WrappedStringKeyRecordConverter()
            : base(
                v => v.Value!,
                v => new WrappedStringKeyRecord { Value = v })
        {
        }
    }

    protected class WrappedStringClassPrincipal
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public WrappedStringKeyClass Id { get; set; } = null!;

        public WrappedStringClass? NonKey { get; set; }
        public ICollection<WrappedStringClassDependentShadow> Dependents { get; } = new List<WrappedStringClassDependentShadow>();

        public ICollection<WrappedStringClassDependentRequired> RequiredDependents { get; } =
            new List<WrappedStringClassDependentRequired>();

        public ICollection<WrappedStringClassDependentOptional> OptionalDependents { get; } =
            new List<WrappedStringClassDependentOptional>();
    }

    protected class WrappedStringClassDependentShadow
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public WrappedStringClass Id { get; set; } = null!;

        public WrappedStringClassPrincipal? Principal { get; set; }
    }

    protected class WrappedStringClassDependentRequired
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public WrappedStringClass Id { get; set; } = null!;

        public WrappedStringKeyClass PrincipalId { get; set; } = null!;
        public WrappedStringClassPrincipal Principal { get; set; } = null!;
    }

    protected class WrappedStringClassDependentOptional
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public WrappedStringClass Id { get; set; } = null!;

        public WrappedStringKeyClass? PrincipalId { get; set; }
        public WrappedStringClassPrincipal? Principal { get; set; }
    }

    protected class WrappedStringStructPrincipal
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public WrappedStringKeyStruct Id { get; set; }

        public WrappedStringStruct NonKey { get; set; }
        public ICollection<WrappedStringStructDependentShadow> Dependents { get; } = new List<WrappedStringStructDependentShadow>();

        public ICollection<WrappedStringStructDependentOptional> OptionalDependents { get; } =
            new List<WrappedStringStructDependentOptional>();

        public ICollection<WrappedStringStructDependentRequired> RequiredDependents { get; } =
            new List<WrappedStringStructDependentRequired>();
    }

    protected class WrappedStringStructDependentShadow
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public WrappedStringStruct Id { get; set; }

        public WrappedStringStructPrincipal? Principal { get; set; }
    }

    protected class WrappedStringStructDependentOptional
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public WrappedStringStruct Id { get; set; }

        public WrappedStringKeyStruct? PrincipalId { get; set; }
        public WrappedStringStructPrincipal? Principal { get; set; }
    }

    protected class WrappedStringStructDependentRequired
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public WrappedStringStruct Id { get; set; }

        public WrappedStringKeyStruct PrincipalId { get; set; }
        public WrappedStringStructPrincipal Principal { get; set; } = null!;
    }

    protected class WrappedStringRecordPrincipal
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public WrappedStringKeyRecord Id { get; set; } = null!;

        public WrappedStringRecord? NonKey { get; set; }
        public ICollection<WrappedStringRecordDependentShadow> Dependents { get; } = new List<WrappedStringRecordDependentShadow>();

        public ICollection<WrappedStringRecordDependentOptional> OptionalDependents { get; } =
            new List<WrappedStringRecordDependentOptional>();

        public ICollection<WrappedStringRecordDependentRequired> RequiredDependents { get; } =
            new List<WrappedStringRecordDependentRequired>();
    }

    protected class WrappedStringRecordDependentShadow
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public WrappedStringRecord Id { get; set; } = null!;

        public WrappedStringRecordPrincipal? Principal { get; set; }
    }

    protected class WrappedStringRecordDependentOptional
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public WrappedStringRecord Id { get; set; } = null!;

        public WrappedStringKeyRecord? PrincipalId { get; set; }
        public WrappedStringRecordPrincipal? Principal { get; set; }
    }

    protected class WrappedStringRecordDependentRequired
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public WrappedStringRecord Id { get; set; } = null!;

        public WrappedStringKeyRecord PrincipalId { get; set; } = null!;
        public WrappedStringRecordPrincipal Principal { get; set; } = null!;
    }

    [ConditionalFact]
    public virtual void Insert_update_and_delete_with_wrapped_string_key()
    {
        var databaseName = Guid.NewGuid().ToString();

        string? id1 = null;
        string? id2 = null;
        string? id3 = null;
        using (var context = new StoreContext(databaseName))
        {
            var principal1 = context.Add(
                new WrappedStringClassPrincipal
                {
                    Dependents = { new WrappedStringClassDependentShadow(), new WrappedStringClassDependentShadow() },
                    OptionalDependents = { new WrappedStringClassDependentOptional(), new WrappedStringClassDependentOptional() },
                    RequiredDependents = { new WrappedStringClassDependentRequired(), new WrappedStringClassDependentRequired() }
                }).Entity;

            var principal2 = context.Add(
                new WrappedStringStructPrincipal
                {
                    Dependents = { new WrappedStringStructDependentShadow(), new WrappedStringStructDependentShadow() },
                    OptionalDependents = { new WrappedStringStructDependentOptional(), new WrappedStringStructDependentOptional() },
                    RequiredDependents = { new WrappedStringStructDependentRequired(), new WrappedStringStructDependentRequired() }
                }).Entity;

            var principal3 = context.Add(
                new WrappedStringRecordPrincipal
                {
                    Dependents = { new WrappedStringRecordDependentShadow(), new WrappedStringRecordDependentShadow() },
                    OptionalDependents = { new WrappedStringRecordDependentOptional(), new WrappedStringRecordDependentOptional() },
                    RequiredDependents = { new WrappedStringRecordDependentRequired(), new WrappedStringRecordDependentRequired() }
                }).Entity;

            context.SaveChanges();

            id1 = principal1.Id.Value;
            Assert.NotNull(id1);
            foreach (var dependent in principal1.Dependents)
            {
                Assert.NotNull(dependent.Id.Value);
                Assert.Same(principal1, dependent.Principal);
                Assert.Equal(id1, context.Entry(dependent).Property<WrappedStringKeyClass?>("PrincipalId").CurrentValue!.Value);
            }

            foreach (var dependent in principal1.OptionalDependents)
            {
                Assert.NotNull(dependent.Id.Value);
                Assert.Same(principal1, dependent.Principal);
                Assert.Equal(id1, dependent.PrincipalId!.Value);
            }

            foreach (var dependent in principal1.RequiredDependents)
            {
                Assert.NotNull(dependent.Id.Value);
                Assert.Same(principal1, dependent.Principal);
                Assert.Equal(id1, dependent.PrincipalId.Value);
            }

            Assert.Equal("66", principal1.NonKey!.Value);

            id2 = principal2.Id.Value;
            Assert.NotNull(id2);
            foreach (var dependent in principal2.Dependents)
            {
                Assert.NotNull(dependent.Id.Value);
                Assert.Same(principal2, dependent.Principal);
                Assert.Equal(id2, context.Entry(dependent).Property<WrappedStringKeyStruct?>("PrincipalId").CurrentValue!.Value.Value);
            }

            foreach (var dependent in principal2.OptionalDependents)
            {
                Assert.NotNull(dependent.Id.Value);
                Assert.Same(principal2, dependent.Principal);
                Assert.Equal(id2, dependent.PrincipalId!.Value.Value);
            }

            foreach (var dependent in principal2.RequiredDependents)
            {
                Assert.NotNull(dependent.Id.Value);
                Assert.Same(principal2, dependent.Principal);
                Assert.Equal(id2, dependent.PrincipalId.Value);
            }

            Assert.Equal("66", principal2.NonKey.Value);

            id3 = principal3.Id.Value;
            Assert.NotNull(id3);
            foreach (var dependent in principal3.Dependents)
            {
                Assert.NotNull(dependent.Id.Value);
                Assert.Same(principal3, dependent.Principal);
                Assert.Equal(id3, context.Entry(dependent).Property<WrappedStringKeyRecord?>("PrincipalId").CurrentValue!.Value);
            }

            foreach (var dependent in principal3.OptionalDependents)
            {
                Assert.NotNull(dependent.Id.Value);
                Assert.Same(principal3, dependent.Principal);
                Assert.Equal(id3, dependent.PrincipalId!.Value);
            }

            foreach (var dependent in principal3.RequiredDependents)
            {
                Assert.NotNull(dependent.Id.Value);
                Assert.Same(principal3, dependent.Principal);
                Assert.Equal(id3, dependent.PrincipalId.Value);
            }

            Assert.Equal("66", principal3.NonKey!.Value);
        }

        using (var context = new StoreContext(databaseName))
        {
            var principal1 = context.Set<WrappedStringClassPrincipal>()
                .Include(e => e.Dependents)
                .Include(e => e.OptionalDependents)
                .Include(e => e.RequiredDependents)
                .Single();

            Assert.Equal(principal1.Id.Value, id1);
            foreach (var dependent in principal1.Dependents)
            {
                Assert.Same(principal1, dependent.Principal);
                Assert.Equal(id1, context.Entry(dependent).Property<WrappedStringKeyClass?>("PrincipalId").CurrentValue!.Value);
            }

            foreach (var dependent in principal1.OptionalDependents)
            {
                Assert.Same(principal1, dependent.Principal);
                Assert.Equal(id1, dependent.PrincipalId!.Value);
            }

            foreach (var dependent in principal1.RequiredDependents)
            {
                Assert.Same(principal1, dependent.Principal);
                Assert.Equal(id1, dependent.PrincipalId.Value);
            }

            var principal2 = context.Set<WrappedStringStructPrincipal>()
                .Include(e => e.Dependents)
                .Include(e => e.OptionalDependents)
                .Include(e => e.RequiredDependents)
                .Single();

            Assert.Equal(principal2.Id.Value, id2);
            foreach (var dependent in principal2.Dependents)
            {
                Assert.Same(principal2, dependent.Principal);
                Assert.Equal(id2, context.Entry(dependent).Property<WrappedStringKeyStruct?>("PrincipalId").CurrentValue!.Value.Value);
            }

            foreach (var dependent in principal2.OptionalDependents)
            {
                Assert.Same(principal2, dependent.Principal);
                Assert.Equal(id2, dependent.PrincipalId!.Value.Value);
            }

            foreach (var dependent in principal2.RequiredDependents)
            {
                Assert.Same(principal2, dependent.Principal);
                Assert.Equal(id2, dependent.PrincipalId.Value);
            }

            var principal3 = context.Set<WrappedStringRecordPrincipal>()
                .Include(e => e.Dependents)
                .Include(e => e.OptionalDependents)
                .Include(e => e.RequiredDependents)
                .Single();

            Assert.Equal(principal3.Id.Value, id3);
            foreach (var dependent in principal3.Dependents)
            {
                Assert.Same(principal3, dependent.Principal);
                Assert.Equal(id3, context.Entry(dependent).Property<WrappedStringKeyRecord?>("PrincipalId").CurrentValue!.Value);
            }

            foreach (var dependent in principal3.OptionalDependents)
            {
                Assert.Same(principal3, dependent.Principal);
                Assert.Equal(id3, dependent.PrincipalId!.Value);
            }

            foreach (var dependent in principal3.RequiredDependents)
            {
                Assert.Same(principal3, dependent.Principal);
                Assert.Equal(id3, dependent.PrincipalId.Value);
            }

            principal1.Dependents.Remove(principal1.Dependents.First());
            principal2.Dependents.Remove(principal2.Dependents.First());
            principal3.Dependents.Remove(principal3.Dependents.First());

            principal1.OptionalDependents.Remove(principal1.OptionalDependents.First());
            principal2.OptionalDependents.Remove(principal2.OptionalDependents.First());
            principal3.OptionalDependents.Remove(principal3.OptionalDependents.First());

            principal1.RequiredDependents.Remove(principal1.RequiredDependents.First());
            principal2.RequiredDependents.Remove(principal2.RequiredDependents.First());
            principal3.RequiredDependents.Remove(principal3.RequiredDependents.First());

            context.SaveChanges();
        }

        using (var context = new StoreContext(databaseName))
        {
            var dependents1 = context.Set<WrappedStringClassDependentShadow>().Include(e => e.Principal).ToList();
            Assert.Equal(2, dependents1.Count);
            Assert.Null(
                context.Entry(dependents1.Single(e => e.Principal == null))
                    .Property<WrappedStringKeyClass?>("PrincipalId").CurrentValue);

            var optionalDependents1 = context.Set<WrappedStringClassDependentOptional>().Include(e => e.Principal).ToList();
            Assert.Equal(2, optionalDependents1.Count);
            Assert.Null(optionalDependents1.Single(e => e.Principal == null).PrincipalId);

            var requiredDependents1 = context.Set<WrappedStringClassDependentRequired>().Include(e => e.Principal).ToList();
            Assert.Single(requiredDependents1);

            var dependents2 = context.Set<WrappedStringStructDependentShadow>().Include(e => e.Principal).ToList();
            Assert.Equal(2, dependents2.Count);
            Assert.Null(
                context.Entry(dependents2.Single(e => e.Principal == null))
                    .Property<WrappedStringKeyStruct?>("PrincipalId").CurrentValue);

            var optionalDependents2 = context.Set<WrappedStringStructDependentOptional>().Include(e => e.Principal).ToList();
            Assert.Equal(2, optionalDependents2.Count);
            Assert.Null(optionalDependents2.Single(e => e.Principal == null).PrincipalId);

            var requiredDependents2 = context.Set<WrappedStringStructDependentRequired>().Include(e => e.Principal).ToList();
            Assert.Single(requiredDependents2);

            var dependents3 = context.Set<WrappedStringRecordDependentShadow>().Include(e => e.Principal).ToList();
            Assert.Equal(2, dependents3.Count);
            Assert.Null(
                context.Entry(dependents3.Single(e => e.Principal == null))
                    .Property<WrappedStringKeyRecord?>("PrincipalId").CurrentValue);

            var optionalDependents3 = context.Set<WrappedStringRecordDependentOptional>().Include(e => e.Principal).ToList();
            Assert.Equal(2, optionalDependents3.Count);
            Assert.Null(optionalDependents3.Single(e => e.Principal == null).PrincipalId);

            var requiredDependents3 = context.Set<WrappedStringRecordDependentRequired>().Include(e => e.Principal).ToList();
            Assert.Single(requiredDependents3);

            context.Remove(dependents1.Single(e => e.Principal != null));
            context.Remove(optionalDependents1.Single(e => e.Principal != null));
            context.Remove(requiredDependents1.Single());
            context.Remove(requiredDependents1.Single().Principal);

            context.Remove(dependents2.Single(e => e.Principal != null));
            context.Remove(optionalDependents2.Single(e => e.Principal != null));
            context.Remove(requiredDependents2.Single());
            context.Remove(requiredDependents2.Single().Principal);

            context.Remove(dependents3.Single(e => e.Principal != null));
            context.Remove(optionalDependents3.Single(e => e.Principal != null));
            context.Remove(requiredDependents3.Single());
            context.Remove(requiredDependents3.Single().Principal);

            context.SaveChanges();
        }

        using (var context = new StoreContext(databaseName))
        {
            Assert.Equal(1, context.Set<WrappedStringClassDependentShadow>().Count());
            Assert.Equal(1, context.Set<WrappedStringStructDependentShadow>().Count());
            Assert.Equal(1, context.Set<WrappedStringRecordDependentShadow>().Count());

            Assert.Equal(1, context.Set<WrappedStringClassDependentOptional>().Count());
            Assert.Equal(1, context.Set<WrappedStringStructDependentOptional>().Count());
            Assert.Equal(1, context.Set<WrappedStringRecordDependentOptional>().Count());

            Assert.Equal(0, context.Set<WrappedStringClassDependentRequired>().Count());
            Assert.Equal(0, context.Set<WrappedStringStructDependentRequired>().Count());
            Assert.Equal(0, context.Set<WrappedStringRecordDependentRequired>().Count());
        }
    }

    // ReSharper disable once StaticMemberInGenericType
    protected static readonly Guid KnownGuid = Guid.Parse("E871CEA4-8DBE-4269-99F4-87F7128AF399");

    protected class WrappedGuidClass
    {
        public Guid Value { get; set; }
    }

    protected class WrappedGuidClassConverter : ValueConverter<WrappedGuidClass, Guid>
    {
        public WrappedGuidClassConverter()
            : base(
                v => v.Value,
                v => new WrappedGuidClass { Value = v })
        {
        }
    }

    protected class WrappedGuidClassComparer : ValueComparer<WrappedGuidClass?>
    {
        public WrappedGuidClassComparer()
            : base(
                (v1, v2) => (v1 == null && v2 == null) || (v1 != null && v2 != null && v1.Value.Equals(v2.Value)),
                v => v != null ? v.Value.GetHashCode() : 0,
                v => v == null ? null : new WrappedGuidClass { Value = v.Value })
        {
        }
    }

    protected class WrappedGuidClassValueGenerator : ValueGenerator<WrappedGuidClass>
    {
        public override WrappedGuidClass Next(EntityEntry entry)
            => new() { Value = KnownGuid };

        public override bool GeneratesTemporaryValues
            => false;
    }

    protected struct WrappedGuidStruct
    {
        public Guid Value { get; set; }
    }

    protected class WrappedGuidStructConverter : ValueConverter<WrappedGuidStruct, Guid>
    {
        public WrappedGuidStructConverter()
            : base(
                v => v.Value,
                v => new WrappedGuidStruct { Value = v })
        {
        }
    }

    protected class WrappedGuidStructValueGenerator : ValueGenerator<WrappedGuidStruct>
    {
        public override WrappedGuidStruct Next(EntityEntry entry)
            => new() { Value = KnownGuid };

        public override bool GeneratesTemporaryValues
            => false;
    }

    protected record WrappedGuidRecord
    {
        public Guid Value { get; set; }
    }

    protected class WrappedGuidRecordConverter : ValueConverter<WrappedGuidRecord, Guid>
    {
        public WrappedGuidRecordConverter()
            : base(
                v => v.Value,
                v => new WrappedGuidRecord { Value = v })
        {
        }
    }

    protected class WrappedGuidRecordValueGenerator : ValueGenerator<WrappedGuidRecord>
    {
        public override WrappedGuidRecord Next(EntityEntry entry)
            => new() { Value = KnownGuid };

        public override bool GeneratesTemporaryValues
            => false;
    }

    protected class WrappedGuidKeyClass
    {
        public Guid Value { get; set; }
    }

    protected class WrappedGuidKeyClassConverter : ValueConverter<WrappedGuidKeyClass, Guid>
    {
        public WrappedGuidKeyClassConverter()
            : base(
                v => v.Value,
                v => new WrappedGuidKeyClass { Value = v })
        {
        }
    }

    protected class WrappedGuidKeyClassComparer : ValueComparer<WrappedGuidKeyClass?>
    {
        public WrappedGuidKeyClassComparer()
            : base(
                (v1, v2) => (v1 == null && v2 == null) || (v1 != null && v2 != null && v1.Value.Equals(v2.Value)),
                v => v != null ? v.Value.GetHashCode() : 0,
                v => v == null ? null : new WrappedGuidKeyClass { Value = v.Value })
        {
        }
    }

    protected struct WrappedGuidKeyStruct
    {
        public Guid Value { get; set; }

        public override bool Equals(object? obj)
            => obj is WrappedGuidKeyStruct other && Value.Equals(other.Value);

        public override int GetHashCode()
            => Value.GetHashCode();

        public static bool operator ==(WrappedGuidKeyStruct left, WrappedGuidKeyStruct right)
            => left.Equals(right);

        public static bool operator !=(WrappedGuidKeyStruct left, WrappedGuidKeyStruct right)
            => !left.Equals(right);
    }

    protected class WrappedGuidKeyStructConverter : ValueConverter<WrappedGuidKeyStruct, Guid>
    {
        public WrappedGuidKeyStructConverter()
            : base(
                v => v.Value,
                v => new WrappedGuidKeyStruct { Value = v })
        {
        }
    }

    protected record WrappedGuidKeyRecord
    {
        public Guid Value { get; set; }
    }

    protected class WrappedGuidKeyRecordConverter : ValueConverter<WrappedGuidKeyRecord, Guid>
    {
        public WrappedGuidKeyRecordConverter()
            : base(
                v => v.Value,
                v => new WrappedGuidKeyRecord { Value = v })
        {
        }
    }

    protected class WrappedGuidClassPrincipal
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public WrappedGuidKeyClass Id { get; set; } = null!;

        public WrappedGuidClass? NonKey { get; set; }
        public ICollection<WrappedGuidClassDependentShadow> Dependents { get; } = new List<WrappedGuidClassDependentShadow>();
        public ICollection<WrappedGuidClassDependentRequired> RequiredDependents { get; } = new List<WrappedGuidClassDependentRequired>();
        public ICollection<WrappedGuidClassDependentOptional> OptionalDependents { get; } = new List<WrappedGuidClassDependentOptional>();
    }

    protected class WrappedGuidClassDependentShadow
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public WrappedGuidClass Id { get; set; } = null!;

        public WrappedGuidClassPrincipal? Principal { get; set; }
    }

    protected class WrappedGuidClassDependentRequired
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public WrappedGuidClass Id { get; set; } = null!;

        public WrappedGuidKeyClass PrincipalId { get; set; } = null!;
        public WrappedGuidClassPrincipal Principal { get; set; } = null!;
    }

    protected class WrappedGuidClassDependentOptional
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public WrappedGuidClass Id { get; set; } = null!;

        public WrappedGuidKeyClass? PrincipalId { get; set; }
        public WrappedGuidClassPrincipal? Principal { get; set; }
    }

    protected class WrappedGuidStructPrincipal
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public WrappedGuidKeyStruct Id { get; set; }

        public WrappedGuidStruct NonKey { get; set; }
        public ICollection<WrappedGuidStructDependentShadow> Dependents { get; } = new List<WrappedGuidStructDependentShadow>();
        public ICollection<WrappedGuidStructDependentOptional> OptionalDependents { get; } = new List<WrappedGuidStructDependentOptional>();
        public ICollection<WrappedGuidStructDependentRequired> RequiredDependents { get; } = new List<WrappedGuidStructDependentRequired>();
    }

    protected class WrappedGuidStructDependentShadow
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public WrappedGuidStruct Id { get; set; }

        public WrappedGuidStructPrincipal? Principal { get; set; }
    }

    protected class WrappedGuidStructDependentOptional
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public WrappedGuidStruct Id { get; set; }

        public WrappedGuidKeyStruct? PrincipalId { get; set; }
        public WrappedGuidStructPrincipal? Principal { get; set; }
    }

    protected class WrappedGuidStructDependentRequired
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public WrappedGuidStruct Id { get; set; }

        public WrappedGuidKeyStruct PrincipalId { get; set; }
        public WrappedGuidStructPrincipal Principal { get; set; } = null!;
    }

    protected class WrappedGuidRecordPrincipal
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public WrappedGuidKeyRecord Id { get; set; } = null!;

        public WrappedGuidRecord? NonKey { get; set; }
        public ICollection<WrappedGuidRecordDependentShadow> Dependents { get; } = new List<WrappedGuidRecordDependentShadow>();
        public ICollection<WrappedGuidRecordDependentOptional> OptionalDependents { get; } = new List<WrappedGuidRecordDependentOptional>();
        public ICollection<WrappedGuidRecordDependentRequired> RequiredDependents { get; } = new List<WrappedGuidRecordDependentRequired>();
    }

    protected class WrappedGuidRecordDependentShadow
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public WrappedGuidRecord Id { get; set; } = null!;

        public WrappedGuidRecordPrincipal? Principal { get; set; }
    }

    protected class WrappedGuidRecordDependentOptional
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public WrappedGuidRecord Id { get; set; } = null!;

        public WrappedGuidKeyRecord? PrincipalId { get; set; }
        public WrappedGuidRecordPrincipal? Principal { get; set; }
    }

    protected class WrappedGuidRecordDependentRequired
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public WrappedGuidRecord Id { get; set; } = null!;

        public WrappedGuidKeyRecord PrincipalId { get; set; } = null!;
        public WrappedGuidRecordPrincipal Principal { get; set; } = null!;
    }

    [ConditionalFact]
    public virtual void Insert_update_and_delete_with_wrapped_Guid_key()
    {
        var databaseName = Guid.NewGuid().ToString();

        var id1 = Guid.Empty;
        var id2 = Guid.Empty;
        var id3 = Guid.Empty;
        using (var context = new StoreContext(databaseName))
        {
            var principal1 = context.Add(
                new WrappedGuidClassPrincipal
                {
                    Dependents = { new WrappedGuidClassDependentShadow(), new WrappedGuidClassDependentShadow() },
                    OptionalDependents = { new WrappedGuidClassDependentOptional(), new WrappedGuidClassDependentOptional() },
                    RequiredDependents = { new WrappedGuidClassDependentRequired(), new WrappedGuidClassDependentRequired() }
                }).Entity;

            var principal2 = context.Add(
                new WrappedGuidStructPrincipal
                {
                    Dependents = { new WrappedGuidStructDependentShadow(), new WrappedGuidStructDependentShadow() },
                    OptionalDependents = { new WrappedGuidStructDependentOptional(), new WrappedGuidStructDependentOptional() },
                    RequiredDependents = { new WrappedGuidStructDependentRequired(), new WrappedGuidStructDependentRequired() }
                }).Entity;

            var principal3 = context.Add(
                new WrappedGuidRecordPrincipal
                {
                    Dependents = { new WrappedGuidRecordDependentShadow(), new WrappedGuidRecordDependentShadow() },
                    OptionalDependents = { new WrappedGuidRecordDependentOptional(), new WrappedGuidRecordDependentOptional() },
                    RequiredDependents = { new WrappedGuidRecordDependentRequired(), new WrappedGuidRecordDependentRequired() }
                }).Entity;

            context.SaveChanges();

            id1 = principal1.Id.Value;
            Assert.NotEqual(Guid.Empty, id1);
            foreach (var dependent in principal1.Dependents)
            {
                Assert.NotEqual(Guid.Empty, dependent.Id.Value);
                Assert.Same(principal1, dependent.Principal);
                Assert.Equal(id1, context.Entry(dependent).Property<WrappedGuidKeyClass?>("PrincipalId").CurrentValue!.Value);
            }

            foreach (var dependent in principal1.OptionalDependents)
            {
                Assert.NotEqual(Guid.Empty, dependent.Id.Value);
                Assert.Same(principal1, dependent.Principal);
                Assert.Equal(id1, dependent.PrincipalId!.Value);
            }

            foreach (var dependent in principal1.RequiredDependents)
            {
                Assert.NotEqual(Guid.Empty, dependent.Id.Value);
                Assert.Same(principal1, dependent.Principal);
                Assert.Equal(id1, dependent.PrincipalId.Value);
            }

            Assert.Equal(KnownGuid, principal1.NonKey!.Value);

            id2 = principal2.Id.Value;
            Assert.NotEqual(Guid.Empty, id2);
            foreach (var dependent in principal2.Dependents)
            {
                Assert.NotEqual(Guid.Empty, dependent.Id.Value);
                Assert.Same(principal2, dependent.Principal);
                Assert.Equal(id2, context.Entry(dependent).Property<WrappedGuidKeyStruct?>("PrincipalId").CurrentValue!.Value.Value);
            }

            foreach (var dependent in principal2.OptionalDependents)
            {
                Assert.NotEqual(Guid.Empty, dependent.Id.Value);
                Assert.Same(principal2, dependent.Principal);
                Assert.Equal(id2, dependent.PrincipalId!.Value.Value);
            }

            foreach (var dependent in principal2.RequiredDependents)
            {
                Assert.NotEqual(Guid.Empty, dependent.Id.Value);
                Assert.Same(principal2, dependent.Principal);
                Assert.Equal(id2, dependent.PrincipalId.Value);
            }

            Assert.Equal(KnownGuid, principal2.NonKey.Value);

            id3 = principal3.Id.Value;
            Assert.NotEqual(Guid.Empty, id3);
            foreach (var dependent in principal3.Dependents)
            {
                Assert.NotEqual(Guid.Empty, dependent.Id.Value);
                Assert.Same(principal3, dependent.Principal);
                Assert.Equal(id3, context.Entry(dependent).Property<WrappedGuidKeyRecord?>("PrincipalId").CurrentValue!.Value);
            }

            foreach (var dependent in principal3.OptionalDependents)
            {
                Assert.NotEqual(Guid.Empty, dependent.Id.Value);
                Assert.Same(principal3, dependent.Principal);
                Assert.Equal(id3, dependent.PrincipalId!.Value);
            }

            foreach (var dependent in principal3.RequiredDependents)
            {
                Assert.NotEqual(Guid.Empty, dependent.Id.Value);
                Assert.Same(principal3, dependent.Principal);
                Assert.Equal(id3, dependent.PrincipalId.Value);
            }

            Assert.Equal(KnownGuid, principal3.NonKey!.Value);
        }

        using (var context = new StoreContext(databaseName))
        {
            var principal1 = context.Set<WrappedGuidClassPrincipal>()
                .Include(e => e.Dependents)
                .Include(e => e.OptionalDependents)
                .Include(e => e.RequiredDependents)
                .Single();

            Assert.Equal(principal1.Id.Value, id1);
            foreach (var dependent in principal1.Dependents)
            {
                Assert.Same(principal1, dependent.Principal);
                Assert.Equal(id1, context.Entry(dependent).Property<WrappedGuidKeyClass?>("PrincipalId").CurrentValue!.Value);
            }

            foreach (var dependent in principal1.OptionalDependents)
            {
                Assert.Same(principal1, dependent.Principal);
                Assert.Equal(id1, dependent.PrincipalId!.Value);
            }

            foreach (var dependent in principal1.RequiredDependents)
            {
                Assert.Same(principal1, dependent.Principal);
                Assert.Equal(id1, dependent.PrincipalId.Value);
            }

            var principal2 = context.Set<WrappedGuidStructPrincipal>()
                .Include(e => e.Dependents)
                .Include(e => e.OptionalDependents)
                .Include(e => e.RequiredDependents)
                .Single();

            Assert.Equal(principal2.Id.Value, id2);
            foreach (var dependent in principal2.Dependents)
            {
                Assert.Same(principal2, dependent.Principal);
                Assert.Equal(id2, context.Entry(dependent).Property<WrappedGuidKeyStruct?>("PrincipalId").CurrentValue!.Value.Value);
            }

            foreach (var dependent in principal2.OptionalDependents)
            {
                Assert.Same(principal2, dependent.Principal);
                Assert.Equal(id2, dependent.PrincipalId!.Value.Value);
            }

            foreach (var dependent in principal2.RequiredDependents)
            {
                Assert.Same(principal2, dependent.Principal);
                Assert.Equal(id2, dependent.PrincipalId.Value);
            }

            var principal3 = context.Set<WrappedGuidRecordPrincipal>()
                .Include(e => e.Dependents)
                .Include(e => e.OptionalDependents)
                .Include(e => e.RequiredDependents)
                .Single();

            Assert.Equal(principal3.Id.Value, id3);
            foreach (var dependent in principal3.Dependents)
            {
                Assert.Same(principal3, dependent.Principal);
                Assert.Equal(id3, context.Entry(dependent).Property<WrappedGuidKeyRecord?>("PrincipalId").CurrentValue!.Value);
            }

            foreach (var dependent in principal3.OptionalDependents)
            {
                Assert.Same(principal3, dependent.Principal);
                Assert.Equal(id3, dependent.PrincipalId!.Value);
            }

            foreach (var dependent in principal3.RequiredDependents)
            {
                Assert.Same(principal3, dependent.Principal);
                Assert.Equal(id3, dependent.PrincipalId.Value);
            }

            principal1.Dependents.Remove(principal1.Dependents.First());
            principal2.Dependents.Remove(principal2.Dependents.First());
            principal3.Dependents.Remove(principal3.Dependents.First());

            principal1.OptionalDependents.Remove(principal1.OptionalDependents.First());
            principal2.OptionalDependents.Remove(principal2.OptionalDependents.First());
            principal3.OptionalDependents.Remove(principal3.OptionalDependents.First());

            principal1.RequiredDependents.Remove(principal1.RequiredDependents.First());
            principal2.RequiredDependents.Remove(principal2.RequiredDependents.First());
            principal3.RequiredDependents.Remove(principal3.RequiredDependents.First());

            context.SaveChanges();
        }

        using (var context = new StoreContext(databaseName))
        {
            var dependents1 = context.Set<WrappedGuidClassDependentShadow>().Include(e => e.Principal).ToList();
            Assert.Equal(2, dependents1.Count);
            Assert.Null(
                context.Entry(dependents1.Single(e => e.Principal == null))
                    .Property<WrappedGuidKeyClass?>("PrincipalId").CurrentValue);

            var optionalDependents1 = context.Set<WrappedGuidClassDependentOptional>().Include(e => e.Principal).ToList();
            Assert.Equal(2, optionalDependents1.Count);
            Assert.Null(optionalDependents1.Single(e => e.Principal == null).PrincipalId);

            var requiredDependents1 = context.Set<WrappedGuidClassDependentRequired>().Include(e => e.Principal).ToList();
            Assert.Single(requiredDependents1);

            var dependents2 = context.Set<WrappedGuidStructDependentShadow>().Include(e => e.Principal).ToList();
            Assert.Equal(2, dependents2.Count);
            Assert.Null(
                context.Entry(dependents2.Single(e => e.Principal == null))
                    .Property<WrappedGuidKeyStruct?>("PrincipalId").CurrentValue);

            var optionalDependents2 = context.Set<WrappedGuidStructDependentOptional>().Include(e => e.Principal).ToList();
            Assert.Equal(2, optionalDependents2.Count);
            Assert.Null(optionalDependents2.Single(e => e.Principal == null).PrincipalId);

            var requiredDependents2 = context.Set<WrappedGuidStructDependentRequired>().Include(e => e.Principal).ToList();
            Assert.Single(requiredDependents2);

            var dependents3 = context.Set<WrappedGuidRecordDependentShadow>().Include(e => e.Principal).ToList();
            Assert.Equal(2, dependents3.Count);
            Assert.Null(
                context.Entry(dependents3.Single(e => e.Principal == null))
                    .Property<WrappedGuidKeyRecord?>("PrincipalId").CurrentValue);

            var optionalDependents3 = context.Set<WrappedGuidRecordDependentOptional>().Include(e => e.Principal).ToList();
            Assert.Equal(2, optionalDependents3.Count);
            Assert.Null(optionalDependents3.Single(e => e.Principal == null).PrincipalId);

            var requiredDependents3 = context.Set<WrappedGuidRecordDependentRequired>().Include(e => e.Principal).ToList();
            Assert.Single(requiredDependents3);

            context.Remove(dependents1.Single(e => e.Principal != null));
            context.Remove(optionalDependents1.Single(e => e.Principal != null));
            context.Remove(requiredDependents1.Single());
            context.Remove(requiredDependents1.Single().Principal);

            context.Remove(dependents2.Single(e => e.Principal != null));
            context.Remove(optionalDependents2.Single(e => e.Principal != null));
            context.Remove(requiredDependents2.Single());
            context.Remove(requiredDependents2.Single().Principal);

            context.Remove(dependents3.Single(e => e.Principal != null));
            context.Remove(optionalDependents3.Single(e => e.Principal != null));
            context.Remove(requiredDependents3.Single());
            context.Remove(requiredDependents3.Single().Principal);

            context.SaveChanges();
        }

        using (var context = new StoreContext(databaseName))
        {
            Assert.Equal(1, context.Set<WrappedGuidClassDependentShadow>().Count());
            Assert.Equal(1, context.Set<WrappedGuidStructDependentShadow>().Count());
            Assert.Equal(1, context.Set<WrappedGuidRecordDependentShadow>().Count());

            Assert.Equal(1, context.Set<WrappedGuidClassDependentOptional>().Count());
            Assert.Equal(1, context.Set<WrappedGuidStructDependentOptional>().Count());
            Assert.Equal(1, context.Set<WrappedGuidRecordDependentOptional>().Count());

            Assert.Equal(0, context.Set<WrappedGuidClassDependentRequired>().Count());
            Assert.Equal(0, context.Set<WrappedGuidStructDependentRequired>().Count());
            Assert.Equal(0, context.Set<WrappedGuidRecordDependentRequired>().Count());
        }
    }

    protected class WrappedUriClass
    {
        public Uri? Value { get; set; }
    }

    protected class WrappedUriClassConverter : ValueConverter<WrappedUriClass, Uri>
    {
        public WrappedUriClassConverter()
            : base(
                v => v.Value!,
                v => new WrappedUriClass { Value = v })
        {
        }
    }

    protected class WrappedUriClassComparer : ValueComparer<WrappedUriClass?>
    {
        public WrappedUriClassComparer()
            : base(
                (v1, v2) => (v1 == null && v2 == null) || (v1 != null && v2 != null && v1.Value!.Equals(v2.Value)),
                v => v != null ? v.Value!.GetHashCode() : 0,
                v => v == null ? null : new WrappedUriClass { Value = v.Value })
        {
        }
    }

    protected class WrappedUriClassValueGenerator : ValueGenerator<WrappedUriClass>
    {
        public override WrappedUriClass Next(EntityEntry entry)
            => new() { Value = new Uri("https://www.example.com") };

        public override bool GeneratesTemporaryValues
            => false;
    }

    protected struct WrappedUriStruct
    {
        public Uri Value { get; set; }
    }

    protected class WrappedUriStructConverter : ValueConverter<WrappedUriStruct, Uri>
    {
        public WrappedUriStructConverter()
            : base(
                v => v.Value,
                v => new WrappedUriStruct { Value = v })
        {
        }
    }

    protected class WrappedUriStructValueGenerator : ValueGenerator<WrappedUriStruct>
    {
        public override WrappedUriStruct Next(EntityEntry entry)
            => new() { Value = new Uri("https://www.example.com") };

        public override bool GeneratesTemporaryValues
            => false;
    }

    protected record WrappedUriRecord
    {
        public Uri? Value { get; set; }
    }

    protected class WrappedUriRecordConverter : ValueConverter<WrappedUriRecord, Uri>
    {
        public WrappedUriRecordConverter()
            : base(
                v => v.Value!,
                v => new WrappedUriRecord { Value = v })
        {
        }
    }

    protected class WrappedUriRecordValueGenerator : ValueGenerator<WrappedUriRecord>
    {
        public override WrappedUriRecord Next(EntityEntry entry)
            => new() { Value = new Uri("https://www.example.com") };

        public override bool GeneratesTemporaryValues
            => false;
    }

    protected class WrappedUriKeyClass
    {
        public Uri? Value { get; set; }
    }

    protected class WrappedUriKeyClassConverter : ValueConverter<WrappedUriKeyClass, Uri>
    {
        public WrappedUriKeyClassConverter()
            : base(
                v => v.Value!,
                v => new WrappedUriKeyClass { Value = v })
        {
        }
    }

    protected class WrappedUriKeyClassComparer : ValueComparer<WrappedUriKeyClass?>
    {
        public WrappedUriKeyClassComparer()
            : base(
                (v1, v2) => (v1 == null && v2 == null) || (v1 != null && v2 != null && v1.Value!.Equals(v2.Value)),
                v => v != null ? v.Value!.GetHashCode() : 0,
                v => v == null ? null : new WrappedUriKeyClass { Value = v.Value })
        {
        }
    }

    protected struct WrappedUriKeyStruct
    {
        public Uri? Value { get; set; }

        public bool Equals(WrappedUriKeyStruct other)
            => Equals(Value, other.Value);

        public override bool Equals(object? obj)
            => obj is WrappedUriKeyStruct other && Equals(other);

        public override int GetHashCode()
            => (Value != null ? Value.GetHashCode() : 0);

        public static bool operator ==(WrappedUriKeyStruct left, WrappedUriKeyStruct right)
            => left.Equals(right);

        public static bool operator !=(WrappedUriKeyStruct left, WrappedUriKeyStruct right)
            => !left.Equals(right);
    }

    protected class WrappedUriKeyStructConverter : ValueConverter<WrappedUriKeyStruct, Uri>
    {
        public WrappedUriKeyStructConverter()
            : base(
                v => v.Value!,
                v => new WrappedUriKeyStruct { Value = v })
        {
        }
    }

    protected record WrappedUriKeyRecord
    {
        public Uri? Value { get; set; }
    }

    protected class WrappedUriKeyRecordConverter : ValueConverter<WrappedUriKeyRecord, Uri>
    {
        public WrappedUriKeyRecordConverter()
            : base(
                v => v.Value!,
                v => new WrappedUriKeyRecord { Value = v })
        {
        }
    }

    protected class WrappedUriClassPrincipal
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public WrappedUriKeyClass Id { get; set; } = null!;

        public WrappedUriClass? NonKey { get; set; }
        public ICollection<WrappedUriClassDependentShadow> Dependents { get; } = new List<WrappedUriClassDependentShadow>();
        public ICollection<WrappedUriClassDependentRequired> RequiredDependents { get; } = new List<WrappedUriClassDependentRequired>();
        public ICollection<WrappedUriClassDependentOptional> OptionalDependents { get; } = new List<WrappedUriClassDependentOptional>();
    }

    protected class WrappedUriClassDependentShadow
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public WrappedUriClass Id { get; set; } = null!;

        public WrappedUriClassPrincipal? Principal { get; set; }
    }

    protected class WrappedUriClassDependentRequired
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public WrappedUriClass Id { get; set; } = null!;

        public WrappedUriKeyClass PrincipalId { get; set; } = null!;
        public WrappedUriClassPrincipal Principal { get; set; } = null!;
    }

    protected class WrappedUriClassDependentOptional
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public WrappedUriClass Id { get; set; } = null!;

        public WrappedUriKeyClass? PrincipalId { get; set; }
        public WrappedUriClassPrincipal? Principal { get; set; }
    }

    protected class WrappedUriStructPrincipal
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public WrappedUriKeyStruct Id { get; set; }

        public WrappedUriStruct NonKey { get; set; }
        public ICollection<WrappedUriStructDependentShadow> Dependents { get; } = new List<WrappedUriStructDependentShadow>();
        public ICollection<WrappedUriStructDependentOptional> OptionalDependents { get; } = new List<WrappedUriStructDependentOptional>();
        public ICollection<WrappedUriStructDependentRequired> RequiredDependents { get; } = new List<WrappedUriStructDependentRequired>();
    }

    protected class WrappedUriStructDependentShadow
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public WrappedUriStruct Id { get; set; }

        public WrappedUriStructPrincipal? Principal { get; set; }
    }

    protected class WrappedUriStructDependentOptional
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public WrappedUriStruct Id { get; set; }

        public WrappedUriKeyStruct? PrincipalId { get; set; }
        public WrappedUriStructPrincipal? Principal { get; set; }
    }

    protected class WrappedUriStructDependentRequired
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public WrappedUriStruct Id { get; set; }

        public WrappedUriKeyStruct PrincipalId { get; set; }
        public WrappedUriStructPrincipal Principal { get; set; } = null!;
    }

    protected class WrappedUriRecordPrincipal
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public WrappedUriKeyRecord Id { get; set; } = null!;

        public WrappedUriRecord? NonKey { get; set; }
        public ICollection<WrappedUriRecordDependentShadow> Dependents { get; } = new List<WrappedUriRecordDependentShadow>();
        public ICollection<WrappedUriRecordDependentOptional> OptionalDependents { get; } = new List<WrappedUriRecordDependentOptional>();
        public ICollection<WrappedUriRecordDependentRequired> RequiredDependents { get; } = new List<WrappedUriRecordDependentRequired>();
    }

    protected class WrappedUriRecordDependentShadow
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public WrappedUriRecord Id { get; set; } = null!;

        public WrappedUriRecordPrincipal? Principal { get; set; }
    }

    protected class WrappedUriRecordDependentOptional
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public WrappedUriRecord Id { get; set; } = null!;

        public WrappedUriKeyRecord? PrincipalId { get; set; }
        public WrappedUriRecordPrincipal? Principal { get; set; }
    }

    protected class WrappedUriRecordDependentRequired
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public WrappedUriRecord Id { get; set; } = null!;

        public WrappedUriKeyRecord PrincipalId { get; set; } = null!;
        public WrappedUriRecordPrincipal Principal { get; set; } = null!;
    }

    [ConditionalFact]
    public virtual void Insert_update_and_delete_with_wrapped_Uri_key()
    {
        var databaseName = Guid.NewGuid().ToString();

        Uri? id1 = null;
        Uri? id2 = null;
        Uri? id3 = null;
        using (var context = new StoreContext(databaseName))
        {
            var principal1 = context.Add(
                new WrappedUriClassPrincipal
                {
                    Dependents = { new WrappedUriClassDependentShadow(), new WrappedUriClassDependentShadow() },
                    OptionalDependents = { new WrappedUriClassDependentOptional(), new WrappedUriClassDependentOptional() },
                    RequiredDependents = { new WrappedUriClassDependentRequired(), new WrappedUriClassDependentRequired() }
                }).Entity;

            var principal2 = context.Add(
                new WrappedUriStructPrincipal
                {
                    Dependents = { new WrappedUriStructDependentShadow(), new WrappedUriStructDependentShadow() },
                    OptionalDependents = { new WrappedUriStructDependentOptional(), new WrappedUriStructDependentOptional() },
                    RequiredDependents = { new WrappedUriStructDependentRequired(), new WrappedUriStructDependentRequired() }
                }).Entity;

            var principal3 = context.Add(
                new WrappedUriRecordPrincipal
                {
                    Dependents = { new WrappedUriRecordDependentShadow(), new WrappedUriRecordDependentShadow() },
                    OptionalDependents = { new WrappedUriRecordDependentOptional(), new WrappedUriRecordDependentOptional() },
                    RequiredDependents = { new WrappedUriRecordDependentRequired(), new WrappedUriRecordDependentRequired() }
                }).Entity;

            context.SaveChanges();

            id1 = principal1.Id.Value;
            Assert.NotNull(id1);
            foreach (var dependent in principal1.Dependents)
            {
                Assert.NotNull(dependent.Id.Value);
                Assert.Same(principal1, dependent.Principal);
                Assert.Equal(id1, context.Entry(dependent).Property<WrappedUriKeyClass?>("PrincipalId").CurrentValue!.Value);
            }

            foreach (var dependent in principal1.OptionalDependents)
            {
                Assert.NotNull(dependent.Id.Value);
                Assert.Same(principal1, dependent.Principal);
                Assert.Equal(id1, dependent.PrincipalId!.Value);
            }

            foreach (var dependent in principal1.RequiredDependents)
            {
                Assert.NotNull(dependent.Id.Value);
                Assert.Same(principal1, dependent.Principal);
                Assert.Equal(id1, dependent.PrincipalId.Value);
            }

            Assert.Equal(new Uri("https://www.example.com"), principal1.NonKey!.Value);

            id2 = principal2.Id.Value;
            Assert.NotNull(id2);
            foreach (var dependent in principal2.Dependents)
            {
                Assert.NotNull(dependent.Id.Value);
                Assert.Same(principal2, dependent.Principal);
                Assert.Equal(id2, context.Entry(dependent).Property<WrappedUriKeyStruct?>("PrincipalId").CurrentValue!.Value.Value);
            }

            foreach (var dependent in principal2.OptionalDependents)
            {
                Assert.NotNull(dependent.Id.Value);
                Assert.Same(principal2, dependent.Principal);
                Assert.Equal(id2, dependent.PrincipalId!.Value.Value);
            }

            foreach (var dependent in principal2.RequiredDependents)
            {
                Assert.NotNull(dependent.Id.Value);
                Assert.Same(principal2, dependent.Principal);
                Assert.Equal(id2, dependent.PrincipalId.Value);
            }

            Assert.Equal(new Uri("https://www.example.com"), principal2.NonKey.Value);

            id3 = principal3.Id.Value;
            Assert.NotNull(id3);
            foreach (var dependent in principal3.Dependents)
            {
                Assert.NotNull(dependent.Id.Value);
                Assert.Same(principal3, dependent.Principal);
                Assert.Equal(id3, context.Entry(dependent).Property<WrappedUriKeyRecord?>("PrincipalId").CurrentValue!.Value);
            }

            foreach (var dependent in principal3.OptionalDependents)
            {
                Assert.NotNull(dependent.Id.Value);
                Assert.Same(principal3, dependent.Principal);
                Assert.Equal(id3, dependent.PrincipalId!.Value);
            }

            foreach (var dependent in principal3.RequiredDependents)
            {
                Assert.NotNull(dependent.Id.Value);
                Assert.Same(principal3, dependent.Principal);
                Assert.Equal(id3, dependent.PrincipalId.Value);
            }

            Assert.Equal(new Uri("https://www.example.com"), principal3.NonKey!.Value);
        }

        using (var context = new StoreContext(databaseName))
        {
            var principal1 = context.Set<WrappedUriClassPrincipal>()
                .Include(e => e.Dependents)
                .Include(e => e.OptionalDependents)
                .Include(e => e.RequiredDependents)
                .Single();

            Assert.Equal(principal1.Id.Value, id1);
            foreach (var dependent in principal1.Dependents)
            {
                Assert.Same(principal1, dependent.Principal);
                Assert.Equal(id1, context.Entry(dependent).Property<WrappedUriKeyClass?>("PrincipalId").CurrentValue!.Value);
            }

            foreach (var dependent in principal1.OptionalDependents)
            {
                Assert.Same(principal1, dependent.Principal);
                Assert.Equal(id1, dependent.PrincipalId!.Value);
            }

            foreach (var dependent in principal1.RequiredDependents)
            {
                Assert.Same(principal1, dependent.Principal);
                Assert.Equal(id1, dependent.PrincipalId.Value);
            }

            var principal2 = context.Set<WrappedUriStructPrincipal>()
                .Include(e => e.Dependents)
                .Include(e => e.OptionalDependents)
                .Include(e => e.RequiredDependents)
                .Single();

            Assert.Equal(principal2.Id.Value, id2);
            foreach (var dependent in principal2.Dependents)
            {
                Assert.Same(principal2, dependent.Principal);
                Assert.Equal(id2, context.Entry(dependent).Property<WrappedUriKeyStruct?>("PrincipalId").CurrentValue!.Value.Value);
            }

            foreach (var dependent in principal2.OptionalDependents)
            {
                Assert.Same(principal2, dependent.Principal);
                Assert.Equal(id2, dependent.PrincipalId!.Value.Value);
            }

            foreach (var dependent in principal2.RequiredDependents)
            {
                Assert.Same(principal2, dependent.Principal);
                Assert.Equal(id2, dependent.PrincipalId.Value);
            }

            var principal3 = context.Set<WrappedUriRecordPrincipal>()
                .Include(e => e.Dependents)
                .Include(e => e.OptionalDependents)
                .Include(e => e.RequiredDependents)
                .Single();

            Assert.Equal(principal3.Id.Value, id3);
            foreach (var dependent in principal3.Dependents)
            {
                Assert.Same(principal3, dependent.Principal);
                Assert.Equal(id3, context.Entry(dependent).Property<WrappedUriKeyRecord?>("PrincipalId").CurrentValue!.Value);
            }

            foreach (var dependent in principal3.OptionalDependents)
            {
                Assert.Same(principal3, dependent.Principal);
                Assert.Equal(id3, dependent.PrincipalId!.Value);
            }

            foreach (var dependent in principal3.RequiredDependents)
            {
                Assert.Same(principal3, dependent.Principal);
                Assert.Equal(id3, dependent.PrincipalId.Value);
            }

            principal1.Dependents.Remove(principal1.Dependents.First());
            principal2.Dependents.Remove(principal2.Dependents.First());
            principal3.Dependents.Remove(principal3.Dependents.First());

            principal1.OptionalDependents.Remove(principal1.OptionalDependents.First());
            principal2.OptionalDependents.Remove(principal2.OptionalDependents.First());
            principal3.OptionalDependents.Remove(principal3.OptionalDependents.First());

            principal1.RequiredDependents.Remove(principal1.RequiredDependents.First());
            principal2.RequiredDependents.Remove(principal2.RequiredDependents.First());
            principal3.RequiredDependents.Remove(principal3.RequiredDependents.First());

            context.SaveChanges();
        }

        using (var context = new StoreContext(databaseName))
        {
            var dependents1 = context.Set<WrappedUriClassDependentShadow>().Include(e => e.Principal).ToList();
            Assert.Equal(2, dependents1.Count);
            Assert.Null(
                context.Entry(dependents1.Single(e => e.Principal == null))
                    .Property<WrappedUriKeyClass?>("PrincipalId").CurrentValue);

            var optionalDependents1 = context.Set<WrappedUriClassDependentOptional>().Include(e => e.Principal).ToList();
            Assert.Equal(2, optionalDependents1.Count);
            Assert.Null(optionalDependents1.Single(e => e.Principal == null).PrincipalId);

            var requiredDependents1 = context.Set<WrappedUriClassDependentRequired>().Include(e => e.Principal).ToList();
            Assert.Single(requiredDependents1);

            var dependents2 = context.Set<WrappedUriStructDependentShadow>().Include(e => e.Principal).ToList();
            Assert.Equal(2, dependents2.Count);
            Assert.Null(
                context.Entry(dependents2.Single(e => e.Principal == null))
                    .Property<WrappedUriKeyStruct?>("PrincipalId").CurrentValue);

            var optionalDependents2 = context.Set<WrappedUriStructDependentOptional>().Include(e => e.Principal).ToList();
            Assert.Equal(2, optionalDependents2.Count);
            Assert.Null(optionalDependents2.Single(e => e.Principal == null).PrincipalId);

            var requiredDependents2 = context.Set<WrappedUriStructDependentRequired>().Include(e => e.Principal).ToList();
            Assert.Single(requiredDependents2);

            var dependents3 = context.Set<WrappedUriRecordDependentShadow>().Include(e => e.Principal).ToList();
            Assert.Equal(2, dependents3.Count);
            Assert.Null(
                context.Entry(dependents3.Single(e => e.Principal == null))
                    .Property<WrappedUriKeyRecord?>("PrincipalId").CurrentValue);

            var optionalDependents3 = context.Set<WrappedUriRecordDependentOptional>().Include(e => e.Principal).ToList();
            Assert.Equal(2, optionalDependents3.Count);
            Assert.Null(optionalDependents3.Single(e => e.Principal == null).PrincipalId);

            var requiredDependents3 = context.Set<WrappedUriRecordDependentRequired>().Include(e => e.Principal).ToList();
            Assert.Single(requiredDependents3);

            context.Remove(dependents1.Single(e => e.Principal != null));
            context.Remove(optionalDependents1.Single(e => e.Principal != null));
            context.Remove(requiredDependents1.Single());
            context.Remove(requiredDependents1.Single().Principal);

            context.Remove(dependents2.Single(e => e.Principal != null));
            context.Remove(optionalDependents2.Single(e => e.Principal != null));
            context.Remove(requiredDependents2.Single());
            context.Remove(requiredDependents2.Single().Principal);

            context.Remove(dependents3.Single(e => e.Principal != null));
            context.Remove(optionalDependents3.Single(e => e.Principal != null));
            context.Remove(requiredDependents3.Single());
            context.Remove(requiredDependents3.Single().Principal);

            context.SaveChanges();
        }

        using (var context = new StoreContext(databaseName))
        {
            Assert.Equal(1, context.Set<WrappedUriClassDependentShadow>().Count());
            Assert.Equal(1, context.Set<WrappedUriStructDependentShadow>().Count());
            Assert.Equal(1, context.Set<WrappedUriRecordDependentShadow>().Count());

            Assert.Equal(1, context.Set<WrappedUriClassDependentOptional>().Count());
            Assert.Equal(1, context.Set<WrappedUriStructDependentOptional>().Count());
            Assert.Equal(1, context.Set<WrappedUriRecordDependentOptional>().Count());

            Assert.Equal(0, context.Set<WrappedUriClassDependentRequired>().Count());
            Assert.Equal(0, context.Set<WrappedUriStructDependentRequired>().Count());
            Assert.Equal(0, context.Set<WrappedUriRecordDependentRequired>().Count());
        }
    }

    protected class UriPrincipal
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Uri Id { get; set; } = null!;

        public ICollection<UriDependentShadow> Dependents { get; } = new List<UriDependentShadow>();
        public ICollection<UriDependentRequired> RequiredDependents { get; } = new List<UriDependentRequired>();
        public ICollection<UriDependentOptional> OptionalDependents { get; } = new List<UriDependentOptional>();
    }

    protected class UriDependentShadow
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Uri Id { get; set; } = null!;

        public UriPrincipal? Principal { get; set; }
    }

    protected class UriDependentRequired
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Uri Id { get; set; } = null!;

        public Uri PrincipalId { get; set; } = null!;
        public UriPrincipal Principal { get; set; } = null!;
    }

    protected class UriDependentOptional
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Uri Id { get; set; } = null!;

        public Uri? PrincipalId { get; set; }
        public UriPrincipal? Principal { get; set; }
    }

    [ConditionalFact]
    public virtual void Insert_update_and_delete_with_Uri_key()
    {
        var databaseName = Guid.NewGuid().ToString();

        Uri? id1 = null;
        using (var context = new StoreContext(databaseName))
        {
            var principal1 = context.Add(
                new UriPrincipal
                {
                    Dependents = { new UriDependentShadow(), new UriDependentShadow() },
                    OptionalDependents = { new UriDependentOptional(), new UriDependentOptional() },
                    RequiredDependents = { new UriDependentRequired(), new UriDependentRequired() }
                }).Entity;

            context.SaveChanges();

            id1 = principal1.Id;
            Assert.NotNull(id1);
            foreach (var dependent in principal1.Dependents)
            {
                Assert.NotNull(dependent.Id);
                Assert.Same(principal1, dependent.Principal);
                Assert.Equal(id1, context.Entry(dependent).Property<Uri?>("PrincipalId").CurrentValue);
            }
        }

        using (var context = new StoreContext(databaseName))
        {
            var principal1 = context.Set<UriPrincipal>()
                .Include(e => e.Dependents)
                .Include(e => e.OptionalDependents)
                .Include(e => e.RequiredDependents)
                .Single();

            Assert.Equal(principal1.Id, id1);
            foreach (var dependent in principal1.Dependents)
            {
                Assert.Same(principal1, dependent.Principal);
                Assert.Equal(id1, context.Entry(dependent).Property<Uri?>("PrincipalId").CurrentValue);
            }

            principal1.Dependents.Remove(principal1.Dependents.First());
            principal1.OptionalDependents.Remove(principal1.OptionalDependents.First());
            principal1.RequiredDependents.Remove(principal1.RequiredDependents.First());
            context.SaveChanges();
        }

        using (var context = new StoreContext(databaseName))
        {
            var dependents1 = context.Set<UriDependentShadow>().Include(e => e.Principal).ToList();
            Assert.Equal(2, dependents1.Count);
            Assert.Null(context.Entry(dependents1.Single(e => e.Principal == null)).Property<Uri?>("PrincipalId").CurrentValue);

            var optionalDependents1 = context.Set<UriDependentOptional>().Include(e => e.Principal).ToList();
            Assert.Equal(2, optionalDependents1.Count);
            Assert.Null(optionalDependents1.Single(e => e.Principal == null).PrincipalId);

            var requiredDependents1 = context.Set<UriDependentRequired>().Include(e => e.Principal).ToList();
            Assert.Single(requiredDependents1);

            context.Remove(dependents1.Single(e => e.Principal != null));
            context.Remove(optionalDependents1.Single(e => e.Principal != null));
            context.Remove(requiredDependents1.Single());
            context.Remove(requiredDependents1.Single().Principal);

            context.SaveChanges();
        }

        using (var context = new StoreContext(databaseName))
        {
            Assert.Equal(1, context.Set<UriDependentShadow>().Count());
            Assert.Equal(1, context.Set<UriDependentOptional>().Count());
            Assert.Equal(0, context.Set<UriDependentRequired>().Count());
        }
    }

    protected enum KeyEnum
    {
        A,
        B,
        C,
        D,
        E
    }

    protected class EnumPrincipal
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public KeyEnum Id { get; set; }

        public ICollection<EnumDependentShadow> Dependents { get; } = new List<EnumDependentShadow>();
        public ICollection<EnumDependentRequired> RequiredDependents { get; } = new List<EnumDependentRequired>();
        public ICollection<EnumDependentOptional> OptionalDependents { get; } = new List<EnumDependentOptional>();
    }

    protected class EnumDependentShadow
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public KeyEnum Id { get; set; }

        public EnumPrincipal? Principal { get; set; }
    }

    protected class EnumDependentRequired
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public KeyEnum Id { get; set; }

        public KeyEnum PrincipalId { get; set; }
        public EnumPrincipal Principal { get; set; } = null!;
    }

    protected class EnumDependentOptional
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public KeyEnum Id { get; set; }

        public KeyEnum? PrincipalId { get; set; }
        public EnumPrincipal? Principal { get; set; }
    }

    [ConditionalFact]
    public virtual void Insert_update_and_delete_with_enum_key()
    {
        var databaseName = Guid.NewGuid().ToString();

        KeyEnum? id1 = null;
        using (var context = new StoreContext(databaseName))
        {
            var principal1 = context.Add(
                new EnumPrincipal
                {
                    Dependents = { new EnumDependentShadow(), new EnumDependentShadow() },
                    OptionalDependents = { new EnumDependentOptional(), new EnumDependentOptional() },
                    RequiredDependents = { new EnumDependentRequired(), new EnumDependentRequired() }
                }).Entity;

            context.SaveChanges();

            id1 = principal1.Id;
            Assert.NotNull(id1);
            foreach (var dependent in principal1.Dependents)
            {
                Assert.Same(principal1, dependent.Principal);
                Assert.Equal(id1, context.Entry(dependent).Property<KeyEnum?>("PrincipalId").CurrentValue);
            }
        }

        using (var context = new StoreContext(databaseName))
        {
            var principal1 = context.Set<EnumPrincipal>()
                .Include(e => e.Dependents)
                .Include(e => e.OptionalDependents)
                .Include(e => e.RequiredDependents)
                .Single();

            Assert.Equal(principal1.Id, id1);
            foreach (var dependent in principal1.Dependents)
            {
                Assert.Same(principal1, dependent.Principal);
                Assert.Equal(id1, context.Entry(dependent).Property<KeyEnum?>("PrincipalId").CurrentValue);
            }

            principal1.Dependents.Remove(principal1.Dependents.First());
            principal1.OptionalDependents.Remove(principal1.OptionalDependents.First());
            principal1.RequiredDependents.Remove(principal1.RequiredDependents.First());
            context.SaveChanges();
        }

        using (var context = new StoreContext(databaseName))
        {
            var dependents1 = context.Set<EnumDependentShadow>().Include(e => e.Principal).ToList();
            Assert.Equal(2, dependents1.Count);
            Assert.Null(context.Entry(dependents1.Single(e => e.Principal == null)).Property<KeyEnum?>("PrincipalId").CurrentValue);

            var optionalDependents1 = context.Set<EnumDependentOptional>().Include(e => e.Principal).ToList();
            Assert.Equal(2, optionalDependents1.Count);
            Assert.Null(optionalDependents1.Single(e => e.Principal == null).PrincipalId);

            var requiredDependents1 = context.Set<EnumDependentRequired>().Include(e => e.Principal).ToList();
            Assert.Single(requiredDependents1);

            context.Remove(dependents1.Single(e => e.Principal != null));
            context.Remove(optionalDependents1.Single(e => e.Principal != null));
            context.Remove(requiredDependents1.Single());
            context.Remove(requiredDependents1.Single().Principal);

            context.SaveChanges();
        }

        using (var context = new StoreContext(databaseName))
        {
            Assert.Equal(1, context.Set<EnumDependentShadow>().Count());
            Assert.Equal(1, context.Set<EnumDependentOptional>().Count());
            Assert.Equal(0, context.Set<EnumDependentRequired>().Count());
        }
    }

    protected class GuidAsStringPrincipal
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        public ICollection<GuidAsStringDependentShadow> Dependents { get; } = new List<GuidAsStringDependentShadow>();
        public ICollection<GuidAsStringDependentRequired> RequiredDependents { get; } = new List<GuidAsStringDependentRequired>();
        public ICollection<GuidAsStringDependentOptional> OptionalDependents { get; } = new List<GuidAsStringDependentOptional>();
    }

    protected class GuidAsStringDependentShadow
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        public GuidAsStringPrincipal? Principal { get; set; }
    }

    protected class GuidAsStringDependentRequired
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        public Guid PrincipalId { get; set; }
        public GuidAsStringPrincipal Principal { get; set; } = null!;
    }

    protected class GuidAsStringDependentOptional
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        public Guid? PrincipalId { get; set; }
        public GuidAsStringPrincipal? Principal { get; set; }
    }

    [ConditionalFact]
    public virtual void Insert_update_and_delete_with_GuidAsString_key()
    {
        var databaseName = Guid.NewGuid().ToString();

        Guid? id1 = null;
        using (var context = new StoreContext(databaseName))
        {
            var principal1 = context.Add(
                new GuidAsStringPrincipal
                {
                    Dependents = { new GuidAsStringDependentShadow(), new GuidAsStringDependentShadow() },
                    OptionalDependents = { new GuidAsStringDependentOptional(), new GuidAsStringDependentOptional() },
                    RequiredDependents = { new GuidAsStringDependentRequired(), new GuidAsStringDependentRequired() }
                }).Entity;

            context.SaveChanges();

            id1 = principal1.Id;
            Assert.NotNull(id1);
            foreach (var dependent in principal1.Dependents)
            {
                Assert.Same(principal1, dependent.Principal);
                Assert.Equal(id1, context.Entry(dependent).Property<Guid?>("PrincipalId").CurrentValue);
            }
        }

        using (var context = new StoreContext(databaseName))
        {
            var principal1 = context.Set<GuidAsStringPrincipal>()
                .Include(e => e.Dependents)
                .Include(e => e.OptionalDependents)
                .Include(e => e.RequiredDependents)
                .Single();

            Assert.Equal(principal1.Id, id1);
            foreach (var dependent in principal1.Dependents)
            {
                Assert.Same(principal1, dependent.Principal);
                Assert.Equal(id1, context.Entry(dependent).Property<Guid?>("PrincipalId").CurrentValue);
            }

            principal1.Dependents.Remove(principal1.Dependents.First());
            principal1.OptionalDependents.Remove(principal1.OptionalDependents.First());
            principal1.RequiredDependents.Remove(principal1.RequiredDependents.First());
            context.SaveChanges();
        }

        using (var context = new StoreContext(databaseName))
        {
            var dependents1 = context.Set<GuidAsStringDependentShadow>().Include(e => e.Principal).ToList();
            Assert.Equal(2, dependents1.Count);
            Assert.Null(context.Entry(dependents1.Single(e => e.Principal == null)).Property<Guid?>("PrincipalId").CurrentValue);

            var optionalDependents1 = context.Set<GuidAsStringDependentOptional>().Include(e => e.Principal).ToList();
            Assert.Equal(2, optionalDependents1.Count);
            Assert.Null(optionalDependents1.Single(e => e.Principal == null).PrincipalId);

            var requiredDependents1 = context.Set<GuidAsStringDependentRequired>().Include(e => e.Principal).ToList();
            Assert.Single(requiredDependents1);

            context.Remove(dependents1.Single(e => e.Principal != null));
            context.Remove(optionalDependents1.Single(e => e.Principal != null));
            context.Remove(requiredDependents1.Single());
            context.Remove(requiredDependents1.Single().Principal);

            context.SaveChanges();
        }

        using (var context = new StoreContext(databaseName))
        {
            Assert.Equal(1, context.Set<GuidAsStringDependentShadow>().Count());
            Assert.Equal(1, context.Set<GuidAsStringDependentOptional>().Count());
            Assert.Equal(0, context.Set<GuidAsStringDependentRequired>().Count());
        }
    }

    protected class StringAsGuidPrincipal
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; } = null!;

        public ICollection<StringAsGuidDependentShadow> Dependents { get; } = new List<StringAsGuidDependentShadow>();
        public ICollection<StringAsGuidDependentRequired> RequiredDependents { get; } = new List<StringAsGuidDependentRequired>();
        public ICollection<StringAsGuidDependentOptional> OptionalDependents { get; } = new List<StringAsGuidDependentOptional>();
    }

    protected class StringAsGuidDependentShadow
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; } = null!;

        public StringAsGuidPrincipal? Principal { get; set; }
    }

    protected class StringAsGuidDependentRequired
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; } = null!;

        public string PrincipalId { get; set; } = null!;
        public StringAsGuidPrincipal Principal { get; set; } = null!;
    }

    protected class StringAsGuidDependentOptional
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; } = null!;

        public string? PrincipalId { get; set; }
        public StringAsGuidPrincipal? Principal { get; set; }
    }

    [ConditionalFact]
    public virtual void Insert_update_and_delete_with_StringAsGuid_key()
    {
        var databaseName = Guid.NewGuid().ToString();

        string? id1 = null;
        using (var context = new StoreContext(databaseName))
        {
            var principal1 = context.Add(
                new StringAsGuidPrincipal
                {
                    Dependents = { new StringAsGuidDependentShadow(), new StringAsGuidDependentShadow() },
                    OptionalDependents = { new StringAsGuidDependentOptional(), new StringAsGuidDependentOptional() },
                    RequiredDependents = { new StringAsGuidDependentRequired(), new StringAsGuidDependentRequired() }
                }).Entity;

            context.SaveChanges();

            id1 = principal1.Id;
            Assert.NotNull(id1);
            foreach (var dependent in principal1.Dependents)
            {
                Assert.NotNull(dependent.Id);
                Assert.Same(principal1, dependent.Principal);
                Assert.Equal(id1, context.Entry(dependent).Property<string?>("PrincipalId").CurrentValue);
            }
        }

        using (var context = new StoreContext(databaseName))
        {
            var principal1 = context.Set<StringAsGuidPrincipal>()
                .Include(e => e.Dependents)
                .Include(e => e.OptionalDependents)
                .Include(e => e.RequiredDependents)
                .Single();

            Assert.Equal(principal1.Id, id1);
            foreach (var dependent in principal1.Dependents)
            {
                Assert.Same(principal1, dependent.Principal);
                Assert.Equal(id1, context.Entry(dependent).Property<string?>("PrincipalId").CurrentValue);
            }

            principal1.Dependents.Remove(principal1.Dependents.First());
            principal1.OptionalDependents.Remove(principal1.OptionalDependents.First());
            principal1.RequiredDependents.Remove(principal1.RequiredDependents.First());
            context.SaveChanges();
        }

        using (var context = new StoreContext(databaseName))
        {
            var dependents1 = context.Set<StringAsGuidDependentShadow>().Include(e => e.Principal).ToList();
            Assert.Equal(2, dependents1.Count);
            Assert.Null(context.Entry(dependents1.Single(e => e.Principal == null)).Property<string?>("PrincipalId").CurrentValue);

            var optionalDependents1 = context.Set<StringAsGuidDependentOptional>().Include(e => e.Principal).ToList();
            Assert.Equal(2, optionalDependents1.Count);
            Assert.Null(optionalDependents1.Single(e => e.Principal == null).PrincipalId);

            var requiredDependents1 = context.Set<StringAsGuidDependentRequired>().Include(e => e.Principal).ToList();
            Assert.Single(requiredDependents1);

            context.Remove(dependents1.Single(e => e.Principal != null));
            context.Remove(optionalDependents1.Single(e => e.Principal != null));
            context.Remove(requiredDependents1.Single());
            context.Remove(requiredDependents1.Single().Principal);

            context.SaveChanges();
        }

        using (var context = new StoreContext(databaseName))
        {
            Assert.Equal(1, context.Set<StringAsGuidDependentShadow>().Count());
            Assert.Equal(1, context.Set<StringAsGuidDependentOptional>().Count());
            Assert.Equal(0, context.Set<StringAsGuidDependentRequired>().Count());
        }
    }

    private class StoreContext(string databaseName) : DbContext
    {
        private readonly string _databaseName = databaseName;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseInMemoryDatabase(_databaseName);

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<IntToString>().Property(e => e.Id).HasConversion<string>();
            modelBuilder.Entity<GuidToString>().Property(e => e.Id).HasConversion<string>();
            modelBuilder.Entity<GuidToBytes>().Property(e => e.Id).HasConversion<byte[]>();
            modelBuilder.Entity<ShortToBytes>().Property(e => e.Id).HasConversion<byte[]>();

            modelBuilder.Entity<NonStoreGenDependent>();

            modelBuilder.Entity<WrappedIntClassPrincipal>(
                entity =>
                {
                    entity.Property(e => e.NonKey).HasValueGenerator<WrappedIntClassValueGenerator>();
                });
            modelBuilder.Entity<WrappedIntStructPrincipal>(
                entity =>
                {
                    entity.Property(e => e.NonKey).HasValueGenerator<WrappedIntStructValueGenerator>();
                });
            modelBuilder.Entity<WrappedIntRecordPrincipal>(
                entity =>
                {
                    entity.Property(e => e.NonKey).HasValueGenerator<WrappedIntRecordValueGenerator>();
                });

            modelBuilder.Entity<LongToIntPrincipal>(
                entity =>
                {
                    var keyConverter = new ValueConverter<long, int>(
                        v => (int)v,
                        v => v);

                    entity.Property(e => e.Id).HasConversion(keyConverter);
                });

            modelBuilder.Entity<WrappedGuidClassPrincipal>(
                entity =>
                {
                    entity.Property(e => e.NonKey).HasValueGenerator<WrappedGuidClassValueGenerator>();
                });
            modelBuilder.Entity<WrappedGuidStructPrincipal>(
                entity =>
                {
                    entity.Property(e => e.NonKey).HasValueGenerator<WrappedGuidStructValueGenerator>();
                });
            modelBuilder.Entity<WrappedGuidRecordPrincipal>(
                entity =>
                {
                    entity.Property(e => e.NonKey).HasValueGenerator<WrappedGuidRecordValueGenerator>();
                });

            modelBuilder.Entity<WrappedStringClassPrincipal>(
                entity =>
                {
                    entity.Property(e => e.NonKey).HasValueGenerator<WrappedStringClassValueGenerator>();
                });
            modelBuilder.Entity<WrappedStringStructPrincipal>(
                entity =>
                {
                    entity.Property(e => e.NonKey).HasValueGenerator<WrappedStringStructValueGenerator>();
                });
            modelBuilder.Entity<WrappedStringRecordPrincipal>(
                entity =>
                {
                    entity.Property(e => e.NonKey).HasValueGenerator<WrappedStringRecordValueGenerator>();
                });

            modelBuilder.Entity<WrappedUriClassPrincipal>(
                entity =>
                {
                    entity.Property(e => e.NonKey).HasValueGenerator<WrappedUriClassValueGenerator>();
                });
            modelBuilder.Entity<WrappedUriStructPrincipal>(
                entity =>
                {
                    entity.Property(e => e.NonKey).HasValueGenerator<WrappedUriStructValueGenerator>();
                });
            modelBuilder.Entity<WrappedUriRecordPrincipal>(
                entity =>
                {
                    entity.Property(e => e.NonKey).HasValueGenerator<WrappedUriRecordValueGenerator>();
                });

            modelBuilder.Entity<UriPrincipal>();
            modelBuilder.Entity<EnumPrincipal>();

            modelBuilder.Entity<GuidAsStringPrincipal>(
                entity =>
                {
                    entity.Property(e => e.Id).HasConversion<string>();
                });
            modelBuilder.Entity<GuidAsStringDependentShadow>(
                entity =>
                {
                    entity.Property(e => e.Id).HasConversion<string>();
                });
            modelBuilder.Entity<GuidAsStringDependentOptional>(
                entity =>
                {
                    entity.Property(e => e.Id).HasConversion<string>();
                    entity.Property(e => e.PrincipalId).HasConversion<string?>();
                });
            modelBuilder.Entity<GuidAsStringDependentRequired>(
                entity =>
                {
                    entity.Property(e => e.Id).HasConversion<string>();
                    entity.Property(e => e.PrincipalId).HasConversion<string>();
                });

            var stringToGuidConverter = new ValueConverter<string, Guid>(
                v => new Guid(v),
                v => v.ToString());

            modelBuilder.Entity<StringAsGuidPrincipal>(
                entity =>
                {
                    entity.Property(e => e.Id).HasConversion(stringToGuidConverter);
                });
            modelBuilder.Entity<StringAsGuidDependentShadow>(
                entity =>
                {
                    entity.Property(e => e.Id).HasConversion(stringToGuidConverter);
                });
            modelBuilder.Entity<StringAsGuidDependentOptional>(
                entity =>
                {
                    entity.Property(e => e.Id).HasConversion(stringToGuidConverter);
                    entity.Property(e => e.PrincipalId).HasConversion(stringToGuidConverter!);
                });
            modelBuilder.Entity<StringAsGuidDependentRequired>(
                entity =>
                {
                    entity.Property(e => e.Id).HasConversion(stringToGuidConverter);
                    entity.Property(e => e.PrincipalId).HasConversion(stringToGuidConverter);
                });
        }

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            configurationBuilder.Properties<WrappedIntClass>().HaveConversion<WrappedIntClassConverter, WrappedIntClassComparer>();
            configurationBuilder.Properties<WrappedIntKeyClass>().HaveConversion<WrappedIntKeyClassConverter, WrappedIntKeyClassComparer>();
            configurationBuilder.Properties<WrappedIntStruct>().HaveConversion<WrappedIntStructConverter>();
            configurationBuilder.Properties<WrappedIntKeyStruct>().HaveConversion<WrappedIntKeyStructConverter>();
            configurationBuilder.Properties<WrappedIntRecord>().HaveConversion<WrappedIntRecordConverter>();
            configurationBuilder.Properties<WrappedIntKeyRecord>().HaveConversion<WrappedIntKeyRecordConverter>();

            configurationBuilder.Properties<WrappedGuidClass>().HaveConversion<WrappedGuidClassConverter, WrappedGuidClassComparer>();
            configurationBuilder.Properties<WrappedGuidKeyClass>()
                .HaveConversion<WrappedGuidKeyClassConverter, WrappedGuidKeyClassComparer>();
            configurationBuilder.Properties<WrappedGuidStruct>().HaveConversion<WrappedGuidStructConverter>();
            configurationBuilder.Properties<WrappedGuidKeyStruct>().HaveConversion<WrappedGuidKeyStructConverter>();
            configurationBuilder.Properties<WrappedGuidRecord>().HaveConversion<WrappedGuidRecordConverter>();
            configurationBuilder.Properties<WrappedGuidKeyRecord>().HaveConversion<WrappedGuidKeyRecordConverter>();

            configurationBuilder.Properties<WrappedStringClass>().HaveConversion<WrappedStringClassConverter, WrappedStringClassComparer>();
            configurationBuilder.Properties<WrappedStringKeyClass>()
                .HaveConversion<WrappedStringKeyClassConverter, WrappedStringKeyClassComparer>();
            configurationBuilder.Properties<WrappedStringStruct>().HaveConversion<WrappedStringStructConverter>();
            configurationBuilder.Properties<WrappedStringKeyStruct>().HaveConversion<WrappedStringKeyStructConverter>();
            configurationBuilder.Properties<WrappedStringRecord>().HaveConversion<WrappedStringRecordConverter>();
            configurationBuilder.Properties<WrappedStringKeyRecord>().HaveConversion<WrappedStringKeyRecordConverter>();

            configurationBuilder.Properties<WrappedUriClass>().HaveConversion<WrappedUriClassConverter, WrappedUriClassComparer>();
            configurationBuilder.Properties<WrappedUriKeyClass>()
                .HaveConversion<WrappedUriKeyClassConverter, WrappedUriKeyClassComparer>();
            configurationBuilder.Properties<WrappedUriStruct>().HaveConversion<WrappedUriStructConverter>();
            configurationBuilder.Properties<WrappedUriKeyStruct>().HaveConversion<WrappedUriKeyStructConverter>();
            configurationBuilder.Properties<WrappedUriRecord>().HaveConversion<WrappedUriRecordConverter>();
            configurationBuilder.Properties<WrappedUriKeyRecord>().HaveConversion<WrappedUriKeyRecordConverter>();
        }
    }
}
