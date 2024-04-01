// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;

namespace Microsoft.EntityFrameworkCore;

public abstract class StoreGeneratedSqlServerTestBase<TFixture> : StoreGeneratedTestBase<TFixture>
    where TFixture : StoreGeneratedSqlServerTestBase<TFixture>.StoreGeneratedSqlServerFixtureBase, new()
{
    protected StoreGeneratedSqlServerTestBase(TFixture fixture)
        : base(fixture)
    {
    }

    public class WrappedIntHiLoClass
    {
        public int Value { get; set; }
    }

    protected class WrappedIntHiLoClassConverter : ValueConverter<WrappedIntHiLoClass, int>
    {
        public WrappedIntHiLoClassConverter()
            : base(
                v => v.Value,
                v => new WrappedIntHiLoClass { Value = v })
        {
        }
    }

    protected class WrappedIntHiLoClassComparer : ValueComparer<WrappedIntHiLoClass?>
    {
        public WrappedIntHiLoClassComparer()
            : base(
                (v1, v2) => (v1 == null && v2 == null) || (v1 != null && v2 != null && v1.Value.Equals(v2.Value)),
                v => v != null ? v.Value : 0,
                v => v == null ? null : new WrappedIntHiLoClass { Value = v.Value })
        {
        }
    }

    protected class WrappedIntHiLoClassValueGenerator : ValueGenerator<WrappedIntHiLoClass>
    {
        public override WrappedIntHiLoClass Next(EntityEntry entry)
            => new() { Value = 66 };

        public override bool GeneratesTemporaryValues
            => false;
    }

    public struct WrappedIntHiLoStruct
    {
        public int Value { get; set; }
    }

    protected class WrappedIntHiLoStructConverter : ValueConverter<WrappedIntHiLoStruct, int>
    {
        public WrappedIntHiLoStructConverter()
            : base(
                v => v.Value,
                v => new WrappedIntHiLoStruct { Value = v })
        {
        }
    }

    protected class WrappedIntHiLoStructValueGenerator : ValueGenerator<WrappedIntHiLoStruct>
    {
        public override WrappedIntHiLoStruct Next(EntityEntry entry)
            => new() { Value = 66 };

        public override bool GeneratesTemporaryValues
            => false;
    }

    public record WrappedIntHiLoRecord
    {
        public int Value { get; set; }
    }

    protected class WrappedIntHiLoRecordConverter : ValueConverter<WrappedIntHiLoRecord, int>
    {
        public WrappedIntHiLoRecordConverter()
            : base(
                v => v.Value,
                v => new WrappedIntHiLoRecord { Value = v })
        {
        }
    }

    protected class WrappedIntHiLoRecordValueGenerator : ValueGenerator<WrappedIntHiLoRecord>
    {
        public override WrappedIntHiLoRecord Next(EntityEntry entry)
            => new() { Value = 66 };

        public override bool GeneratesTemporaryValues
            => false;
    }

    public class WrappedIntHiLoKeyClass
    {
        public int Value { get; set; }
    }

    protected class WrappedIntHiLoKeyClassConverter : ValueConverter<WrappedIntHiLoKeyClass, int>
    {
        public WrappedIntHiLoKeyClassConverter()
            : base(
                v => v.Value,
                v => new WrappedIntHiLoKeyClass { Value = v })
        {
        }
    }

    protected class WrappedIntHiLoKeyClassComparer : ValueComparer<WrappedIntHiLoKeyClass?>
    {
        public WrappedIntHiLoKeyClassComparer()
            : base(
                (v1, v2) => (v1 == null && v2 == null) || (v1 != null && v2 != null && v1.Value.Equals(v2.Value)),
                v => v != null ? v.Value : 0,
                v => v == null ? null : new WrappedIntHiLoKeyClass { Value = v.Value })
        {
        }
    }

    public struct WrappedIntHiLoKeyStruct
    {
        public int Value { get; set; }

        public override bool Equals(object? obj)
            => obj is WrappedIntHiLoKeyStruct other && Value == other.Value;

        public override int GetHashCode()
            => Value;

        public static bool operator ==(WrappedIntHiLoKeyStruct left, WrappedIntHiLoKeyStruct right)
            => left.Equals(right);

        public static bool operator !=(WrappedIntHiLoKeyStruct left, WrappedIntHiLoKeyStruct right)
            => !left.Equals(right);
    }

    protected class WrappedIntHiLoKeyStructConverter : ValueConverter<WrappedIntHiLoKeyStruct, int>
    {
        public WrappedIntHiLoKeyStructConverter()
            : base(
                v => v.Value,
                v => new WrappedIntHiLoKeyStruct { Value = v })
        {
        }
    }

    public record WrappedIntHiLoKeyRecord
    {
        public int Value { get; set; }
    }

    protected class WrappedIntHiLoKeyRecordConverter : ValueConverter<WrappedIntHiLoKeyRecord, int>
    {
        public WrappedIntHiLoKeyRecordConverter()
            : base(
                v => v.Value,
                v => new WrappedIntHiLoKeyRecord { Value = v })
        {
        }
    }

    protected class WrappedIntHiLoClassPrincipal
    {
        public WrappedIntHiLoKeyClass Id { get; set; } = null!;
        public ICollection<WrappedIntHiLoClassDependentShadow> Dependents { get; } = new List<WrappedIntHiLoClassDependentShadow>();

        public ICollection<WrappedIntHiLoClassDependentRequired> RequiredDependents { get; } =
            new List<WrappedIntHiLoClassDependentRequired>();

        public ICollection<WrappedIntHiLoClassDependentOptional> OptionalDependents { get; } =
            new List<WrappedIntHiLoClassDependentOptional>();
    }

    protected class WrappedIntHiLoClassDependentShadow
    {
        public WrappedIntHiLoClass Id { get; set; } = null!;
        public WrappedIntHiLoClassPrincipal? Principal { get; set; }
    }

    protected class WrappedIntHiLoClassDependentRequired
    {
        public WrappedIntHiLoClass Id { get; set; } = null!;
        public WrappedIntHiLoKeyClass PrincipalId { get; set; } = null!;
        public WrappedIntHiLoClassPrincipal Principal { get; set; } = null!;
    }

    protected class WrappedIntHiLoClassDependentOptional
    {
        public WrappedIntHiLoClass Id { get; set; } = null!;
        public WrappedIntHiLoKeyClass? PrincipalId { get; set; }
        public WrappedIntHiLoClassPrincipal? Principal { get; set; }
    }

    protected class WrappedIntHiLoStructPrincipal
    {
        public WrappedIntHiLoKeyStruct Id { get; set; }
        public ICollection<WrappedIntHiLoStructDependentShadow> Dependents { get; } = new List<WrappedIntHiLoStructDependentShadow>();

        public ICollection<WrappedIntHiLoStructDependentOptional> OptionalDependents { get; } =
            new List<WrappedIntHiLoStructDependentOptional>();

        public ICollection<WrappedIntHiLoStructDependentRequired> RequiredDependents { get; } =
            new List<WrappedIntHiLoStructDependentRequired>();
    }

    protected class WrappedIntHiLoStructDependentShadow
    {
        public WrappedIntHiLoStruct Id { get; set; }
        public WrappedIntHiLoStructPrincipal? Principal { get; set; }
    }

    protected class WrappedIntHiLoStructDependentOptional
    {
        public WrappedIntHiLoStruct Id { get; set; }
        public WrappedIntHiLoKeyStruct? PrincipalId { get; set; }
        public WrappedIntHiLoStructPrincipal? Principal { get; set; }
    }

    protected class WrappedIntHiLoStructDependentRequired
    {
        public WrappedIntHiLoStruct Id { get; set; }
        public WrappedIntHiLoKeyStruct PrincipalId { get; set; }
        public WrappedIntHiLoStructPrincipal Principal { get; set; } = null!;
    }

    protected class WrappedIntHiLoRecordPrincipal
    {
        public WrappedIntHiLoKeyRecord Id { get; set; } = null!;
        public ICollection<WrappedIntHiLoRecordDependentShadow> Dependents { get; } = new List<WrappedIntHiLoRecordDependentShadow>();

        public ICollection<WrappedIntHiLoRecordDependentOptional> OptionalDependents { get; } =
            new List<WrappedIntHiLoRecordDependentOptional>();

        public ICollection<WrappedIntHiLoRecordDependentRequired> RequiredDependents { get; } =
            new List<WrappedIntHiLoRecordDependentRequired>();
    }

    protected class WrappedIntHiLoRecordDependentShadow
    {
        public WrappedIntHiLoRecord Id { get; set; } = null!;
        public WrappedIntHiLoRecordPrincipal? Principal { get; set; }
    }

    protected class WrappedIntHiLoRecordDependentOptional
    {
        public WrappedIntHiLoRecord Id { get; set; } = null!;
        public WrappedIntHiLoKeyRecord? PrincipalId { get; set; }
        public WrappedIntHiLoRecordPrincipal? Principal { get; set; }
    }

    protected class WrappedIntHiLoRecordDependentRequired
    {
        public WrappedIntHiLoRecord Id { get; set; } = null!;
        public WrappedIntHiLoKeyRecord PrincipalId { get; set; } = null!;
        public WrappedIntHiLoRecordPrincipal Principal { get; set; } = null!;
    }

    protected class LongToDecimalPrincipal
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        public ICollection<LongToDecimalDependentShadow> Dependents { get; } = new List<LongToDecimalDependentShadow>();
        public ICollection<LongToDecimalDependentRequired> RequiredDependents { get; } = new List<LongToDecimalDependentRequired>();
        public ICollection<LongToDecimalDependentOptional> OptionalDependents { get; } = new List<LongToDecimalDependentOptional>();
    }

    protected class LongToDecimalDependentShadow
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        public LongToDecimalPrincipal? Principal { get; set; }
    }

    protected class LongToDecimalDependentRequired
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        public long PrincipalId { get; set; }
        public LongToDecimalPrincipal Principal { get; set; } = null!;
    }

    protected class LongToDecimalDependentOptional
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        public long? PrincipalId { get; set; }
        public LongToDecimalPrincipal? Principal { get; set; }
    }

    [ConditionalFact]
    public virtual Task Insert_update_and_delete_with_long_to_decimal_conversion()
    {
        var id1 = 0L;
        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var principal1 = context.Add(
                    new LongToDecimalPrincipal
                    {
                        Id = Fixture.LongToDecimalPrincipalSentinel,
                        Dependents = { new LongToDecimalDependentShadow(), new LongToDecimalDependentShadow() },
                        OptionalDependents = { new LongToDecimalDependentOptional(), new LongToDecimalDependentOptional() },
                        RequiredDependents = { new LongToDecimalDependentRequired(), new LongToDecimalDependentRequired() }
                    }).Entity;

                await context.SaveChangesAsync();

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
            }, async context =>
            {
                var principal1 = await context.Set<LongToDecimalPrincipal>()
                    .Include(e => e.Dependents)
                    .Include(e => e.OptionalDependents)
                    .Include(e => e.RequiredDependents)
                    .SingleAsync();

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

                await context.SaveChangesAsync();
            }, async context =>
            {
                var dependents1 = await context.Set<LongToDecimalDependentShadow>().Include(e => e.Principal).ToListAsync();
                Assert.Equal(2, dependents1.Count);
                Assert.Null(
                    context.Entry(dependents1.Single(e => e.Principal == null))
                        .Property<long?>("PrincipalId").CurrentValue);

                var optionalDependents1 = await context.Set<LongToDecimalDependentOptional>().Include(e => e.Principal).ToListAsync();
                Assert.Equal(2, optionalDependents1.Count);
                Assert.Null(optionalDependents1.Single(e => e.Principal == null).PrincipalId);

                var requiredDependents1 = await context.Set<LongToDecimalDependentRequired>().Include(e => e.Principal).ToListAsync();
                Assert.Single(requiredDependents1);

                context.Remove(dependents1.Single(e => e.Principal != null));
                context.Remove(optionalDependents1.Single(e => e.Principal != null));
                context.Remove(requiredDependents1.Single());
                context.Remove(requiredDependents1.Single().Principal);

                await context.SaveChangesAsync();
            }, async context =>
            {
                Assert.Equal(1, await context.Set<LongToDecimalDependentShadow>().CountAsync());
                Assert.Equal(1, await context.Set<LongToDecimalDependentOptional>().CountAsync());
                Assert.Equal(0, await context.Set<LongToDecimalDependentRequired>().CountAsync());
            });
    }

    [ConditionalFact]
    public virtual Task Insert_update_and_delete_with_wrapped_int_key_using_hi_lo()
    {
        var id1 = 0;
        var id2 = 0;
        var id3 = 0;
        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var principal1 = context.Add(
                    new WrappedIntHiLoClassPrincipal
                    {
                        Id = Fixture.WrappedIntHiLoKeyClassSentinel!,
                        Dependents = { new WrappedIntHiLoClassDependentShadow(), new WrappedIntHiLoClassDependentShadow() },
                        OptionalDependents = { new WrappedIntHiLoClassDependentOptional(), new WrappedIntHiLoClassDependentOptional() },
                        RequiredDependents = { new WrappedIntHiLoClassDependentRequired(), new WrappedIntHiLoClassDependentRequired() }
                    }).Entity;

                var principal2 = context.Add(
                    new WrappedIntHiLoStructPrincipal
                    {
                        Id = Fixture.WrappedIntHiLoKeyStructSentinel!,
                        Dependents = { new WrappedIntHiLoStructDependentShadow(), new WrappedIntHiLoStructDependentShadow() },
                        OptionalDependents =
                        {
                            new WrappedIntHiLoStructDependentOptional(), new WrappedIntHiLoStructDependentOptional()
                        },
                        RequiredDependents =
                        {
                            new WrappedIntHiLoStructDependentRequired(), new WrappedIntHiLoStructDependentRequired()
                        }
                    }).Entity;

                var principal3 = context.Add(
                    new WrappedIntHiLoRecordPrincipal
                    {
                        Id = Fixture.WrappedIntHiLoKeyRecordSentinel!,
                        Dependents = { new WrappedIntHiLoRecordDependentShadow(), new WrappedIntHiLoRecordDependentShadow() },
                        OptionalDependents =
                        {
                            new WrappedIntHiLoRecordDependentOptional(), new WrappedIntHiLoRecordDependentOptional()
                        },
                        RequiredDependents =
                        {
                            new WrappedIntHiLoRecordDependentRequired(), new WrappedIntHiLoRecordDependentRequired()
                        }
                    }).Entity;

                await context.SaveChangesAsync();

                id1 = principal1.Id.Value;
                Assert.NotEqual(0, id1);
                foreach (var dependent in principal1.Dependents)
                {
                    Assert.NotEqual(0, dependent.Id.Value);
                    Assert.Same(principal1, dependent.Principal);
                    Assert.Equal(id1, context.Entry(dependent).Property<WrappedIntHiLoKeyClass?>("PrincipalId").CurrentValue!.Value);
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

                id2 = principal2.Id.Value;
                Assert.NotEqual(0, id2);
                foreach (var dependent in principal2.Dependents)
                {
                    Assert.NotEqual(0, dependent.Id.Value);
                    Assert.Same(principal2, dependent.Principal);
                    Assert.Equal(id2, context.Entry(dependent).Property<WrappedIntHiLoKeyStruct?>("PrincipalId").CurrentValue!.Value.Value);
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

                id3 = principal3.Id.Value;
                Assert.NotEqual(0, id3);
                foreach (var dependent in principal3.Dependents)
                {
                    Assert.NotEqual(0, dependent.Id.Value);
                    Assert.Same(principal3, dependent.Principal);
                    Assert.Equal(id3, context.Entry(dependent).Property<WrappedIntHiLoKeyRecord?>("PrincipalId").CurrentValue!.Value);
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
            }, async context =>
            {
                var principal1 = await context.Set<WrappedIntHiLoClassPrincipal>()
                    .Include(e => e.Dependents)
                    .Include(e => e.OptionalDependents)
                    .Include(e => e.RequiredDependents)
                    .SingleAsync();

                Assert.Equal(principal1.Id.Value, id1);
                foreach (var dependent in principal1.Dependents)
                {
                    Assert.Same(principal1, dependent.Principal);
                    Assert.Equal(id1, context.Entry(dependent).Property<WrappedIntHiLoKeyClass?>("PrincipalId").CurrentValue!.Value);
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

                var principal2 = await context.Set<WrappedIntHiLoStructPrincipal>()
                    .Include(e => e.Dependents)
                    .Include(e => e.OptionalDependents)
                    .Include(e => e.RequiredDependents)
                    .SingleAsync();

                Assert.Equal(principal2.Id.Value, id2);
                foreach (var dependent in principal2.Dependents)
                {
                    Assert.Same(principal2, dependent.Principal);
                    Assert.Equal(id2, context.Entry(dependent).Property<WrappedIntHiLoKeyStruct?>("PrincipalId").CurrentValue!.Value.Value);
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

                var principal3 = await context.Set<WrappedIntHiLoRecordPrincipal>()
                    .Include(e => e.Dependents)
                    .Include(e => e.OptionalDependents)
                    .Include(e => e.RequiredDependents)
                    .SingleAsync();

                Assert.Equal(principal3.Id.Value, id3);
                foreach (var dependent in principal3.Dependents)
                {
                    Assert.Same(principal3, dependent.Principal);
                    Assert.Equal(id3, context.Entry(dependent).Property<WrappedIntHiLoKeyRecord?>("PrincipalId").CurrentValue!.Value);
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

                await context.SaveChangesAsync();
            }, async context =>
            {
                var dependents1 = await context.Set<WrappedIntHiLoClassDependentShadow>().Include(e => e.Principal).ToListAsync();
                Assert.Equal(2, dependents1.Count);
                Assert.Null(
                    context.Entry(dependents1.Single(e => e.Principal == null))
                        .Property<WrappedIntHiLoKeyClass?>("PrincipalId").CurrentValue);

                var optionalDependents1 = await context.Set<WrappedIntHiLoClassDependentOptional>().Include(e => e.Principal).ToListAsync();
                Assert.Equal(2, optionalDependents1.Count);
                Assert.Null(optionalDependents1.Single(e => e.Principal == null).PrincipalId);

                var requiredDependents1 = await context.Set<WrappedIntHiLoClassDependentRequired>().Include(e => e.Principal).ToListAsync();
                Assert.Single(requiredDependents1);

                var dependents2 = await context.Set<WrappedIntHiLoStructDependentShadow>().Include(e => e.Principal).ToListAsync();
                Assert.Equal(2, dependents2.Count);
                Assert.Null(
                    context.Entry(dependents2.Single(e => e.Principal == null))
                        .Property<WrappedIntHiLoKeyStruct?>("PrincipalId").CurrentValue);

                var optionalDependents2 =
                    await context.Set<WrappedIntHiLoStructDependentOptional>().Include(e => e.Principal).ToListAsync();
                Assert.Equal(2, optionalDependents2.Count);
                Assert.Null(optionalDependents2.Single(e => e.Principal == null).PrincipalId);

                var requiredDependents2 =
                    await context.Set<WrappedIntHiLoStructDependentRequired>().Include(e => e.Principal).ToListAsync();
                Assert.Single(requiredDependents2);

                var dependents3 = await context.Set<WrappedIntHiLoRecordDependentShadow>().Include(e => e.Principal).ToListAsync();
                Assert.Equal(2, dependents3.Count);
                Assert.Null(
                    context.Entry(dependents3.Single(e => e.Principal == null))
                        .Property<WrappedIntHiLoKeyRecord?>("PrincipalId").CurrentValue);

                var optionalDependents3 =
                    await context.Set<WrappedIntHiLoRecordDependentOptional>().Include(e => e.Principal).ToListAsync();
                Assert.Equal(2, optionalDependents3.Count);
                Assert.Null(optionalDependents3.Single(e => e.Principal == null).PrincipalId);

                var requiredDependents3 =
                    await context.Set<WrappedIntHiLoRecordDependentRequired>().Include(e => e.Principal).ToListAsync();
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

                await context.SaveChangesAsync();
            }, async context =>
            {
                Assert.Equal(1, await context.Set<WrappedIntHiLoClassDependentShadow>().CountAsync());
                Assert.Equal(1, await context.Set<WrappedIntHiLoStructDependentShadow>().CountAsync());
                Assert.Equal(1, await context.Set<WrappedIntHiLoRecordDependentShadow>().CountAsync());

                Assert.Equal(1, await context.Set<WrappedIntHiLoClassDependentOptional>().CountAsync());
                Assert.Equal(1, await context.Set<WrappedIntHiLoStructDependentOptional>().CountAsync());
                Assert.Equal(1, await context.Set<WrappedIntHiLoRecordDependentOptional>().CountAsync());

                Assert.Equal(0, await context.Set<WrappedIntHiLoClassDependentRequired>().CountAsync());
                Assert.Equal(0, await context.Set<WrappedIntHiLoStructDependentRequired>().CountAsync());
                Assert.Equal(0, await context.Set<WrappedIntHiLoRecordDependentRequired>().CountAsync());
            });
    }

    protected override void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
        => facade.UseTransaction(transaction.GetDbTransaction());

    [ConditionalFact]
    public virtual async Task Exception_in_SaveChanges_causes_store_values_to_be_reverted()
    {
        var entities = new List<Darwin>();
        for (var i = 0; i < 100; i++)
        {
            entities.Add(
                new Darwin
                {
                    _id = Fixture.NullableIntSentinel,
                    Species = new Species { Id = Fixture.IntSentinel, Name = "Goldfish (with legs)" },
                    MixedMetaphors = new List<Species>
                    {
                        new() { Id = Fixture.IntSentinel, Name = "Large ground finch" },
                        new() { Id = Fixture.IntSentinel, Name = "Medium ground finch" },
                        new() { Id = Fixture.IntSentinel, Name = "Small tree finch" },
                        new() { Id = Fixture.IntSentinel, Name = "Green warbler-finch" }
                    }
                });
        }

        entities.Add(
            new Darwin
            {
                Id = 1777,
                Species = new Species { Id = Fixture.IntSentinel, Name = "Goldfish (with legs)" },
                MixedMetaphors = new List<Species>
                {
                    new() { Id = Fixture.IntSentinel, Name = "Large ground finch" },
                    new() { Id = Fixture.IntSentinel, Name = "Medium ground finch" },
                    new() { Id = Fixture.IntSentinel, Name = "Small tree finch" },
                    new() { Id = Fixture.IntSentinel, Name = "Green warbler-finch" }
                }
            });

        for (var i = 0; i < 2; i++)
        {
            await ExecuteWithStrategyInTransactionAsync(
                async context =>
                {
                    context.AddRange(entities);

                    foreach (var entity in entities.Take(100))
                    {
                        Assert.Equal(Fixture.NullableIntSentinel ?? 0, entity.Id);
                        Assert.Equal(Fixture.NullableIntSentinel, entity._id);
                    }

                    Assert.Equal(1777, entities[100].Id);

                    var tempValueIdentityMap = entities.ToDictionary(
                        e => context.Entry(e).Property(p => p.Id).CurrentValue,
                        e => e);

                    var stateManager = context.GetService<IStateManager>();
                    var key = context.Model.FindEntityType(typeof(Darwin))!.FindPrimaryKey()!;

                    foreach (var entity in entities)
                    {
                        Assert.Same(
                            entity,
                            stateManager.TryGetEntryTyped(
                                key,
                                context.Entry(entity).Property(p => p.Id).CurrentValue)!.Entity);
                    }

                    // DbUpdateException : An error occurred while updating the entries. See the
                    // inner exception for details.
                    // SqlException : Cannot insert explicit value for identity column in table
                    // 'Blog' when IDENTITY_INSERT is set to OFF.
                    var updateException = await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
                    Assert.Single(updateException.Entries);

                    foreach (var entity in entities.Take(100))
                    {
                        Assert.Equal(Fixture.NullableIntSentinel ?? 0, entity.Id);
                        Assert.Equal(Fixture.NullableIntSentinel, entity._id);
                        Assert.Null(entity.Species!.DarwinId);
                        foreach (var species in entity.MixedMetaphors)
                        {
                            Assert.Null(species.MetaphoricId);
                        }
                    }

                    Assert.Equal(1777, entities[100].Id);
                    Assert.Equal(1777, entities[100].Species!.DarwinId);
                    foreach (var species in entities[100].MixedMetaphors)
                    {
                        Assert.Equal(1777, species.MetaphoricId);
                    }

                    foreach (var entity in entities)
                    {
                        Assert.Same(
                            entity,
                            tempValueIdentityMap[context.Entry(entity).Property(p => p.Id).CurrentValue]);
                    }

                    foreach (var entity in entities)
                    {
                        Assert.Same(
                            entity,
                            stateManager.TryGetEntryTyped(
                                key,
                                context.Entry(entity).Property(p => p.Id).CurrentValue)!.Entity);
                    }
                });
        }
    }

    public abstract class StoreGeneratedSqlServerFixtureBase : StoreGeneratedFixtureBase
    {
        public virtual long LongToDecimalPrincipalSentinel
            => default;

        public virtual WrappedIntHiLoKeyClass? WrappedIntHiLoKeyClassSentinel
            => default;

        public virtual WrappedIntHiLoKeyStruct WrappedIntHiLoKeyStructSentinel
            => default;

        public virtual WrappedIntHiLoKeyRecord? WrappedIntHiLoKeyRecordSentinel
            => default;

        protected override ITestStoreFactory TestStoreFactory
            => SqlServerTestStoreFactory.Instance;

        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            => builder
                .EnableSensitiveDataLogging()
                .ConfigureWarnings(
                    b => b.Default(WarningBehavior.Throw)
                        .Ignore(CoreEventId.SensitiveDataLoggingEnabledWarning)
                        .Ignore(RelationalEventId.BoolWithDefaultWarning));

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            modelBuilder.Entity<Gumball>(
                b =>
                {
                    b.Property(e => e.Id).UseIdentityColumn();
                    b.Property(e => e.Identity).HasDefaultValue("Banana Joe");
                    b.Property(e => e.IdentityReadOnlyBeforeSave).HasDefaultValue("Doughnut Sheriff");
                    b.Property(e => e.IdentityReadOnlyAfterSave).HasDefaultValue("Anton");
                    b.Property(e => e.AlwaysIdentity).HasDefaultValue("Banana Joe");
                    b.Property(e => e.AlwaysIdentityReadOnlyBeforeSave).HasDefaultValue("Doughnut Sheriff");
                    b.Property(e => e.AlwaysIdentityReadOnlyAfterSave).HasDefaultValue("Anton");
                    b.Property(e => e.Computed).HasDefaultValue("Alan");
                    b.Property(e => e.ComputedReadOnlyBeforeSave).HasDefaultValue("Carmen");
                    b.Property(e => e.ComputedReadOnlyAfterSave).HasDefaultValue("Tina Rex");
                    b.Property(e => e.AlwaysComputed).HasDefaultValue("Alan");
                    b.Property(e => e.AlwaysComputedReadOnlyBeforeSave).HasDefaultValue("Carmen");
                    b.Property(e => e.AlwaysComputedReadOnlyAfterSave).HasDefaultValue("Tina Rex");
                });

            modelBuilder.Entity<Anais>(
                b =>
                {
                    b.Property(e => e.OnAdd).HasDefaultValue("Rabbit");
                    b.Property(e => e.OnAddUseBeforeUseAfter).HasDefaultValue("Rabbit");
                    b.Property(e => e.OnAddIgnoreBeforeUseAfter).HasDefaultValue("Rabbit");
                    b.Property(e => e.OnAddThrowBeforeUseAfter).HasDefaultValue("Rabbit");
                    b.Property(e => e.OnAddUseBeforeIgnoreAfter).HasDefaultValue("Rabbit");
                    b.Property(e => e.OnAddIgnoreBeforeIgnoreAfter).HasDefaultValue("Rabbit");
                    b.Property(e => e.OnAddThrowBeforeIgnoreAfter).HasDefaultValue("Rabbit");
                    b.Property(e => e.OnAddUseBeforeThrowAfter).HasDefaultValue("Rabbit");
                    b.Property(e => e.OnAddIgnoreBeforeThrowAfter).HasDefaultValue("Rabbit");
                    b.Property(e => e.OnAddThrowBeforeThrowAfter).HasDefaultValue("Rabbit");

                    b.Property(e => e.OnAddOrUpdate).HasDefaultValue("Rabbit");
                    b.Property(e => e.OnAddOrUpdateUseBeforeUseAfter).HasDefaultValue("Rabbit");
                    b.Property(e => e.OnAddOrUpdateIgnoreBeforeUseAfter).HasDefaultValue("Rabbit");
                    b.Property(e => e.OnAddOrUpdateThrowBeforeUseAfter).HasDefaultValue("Rabbit");
                    b.Property(e => e.OnAddOrUpdateUseBeforeIgnoreAfter).HasDefaultValue("Rabbit");
                    b.Property(e => e.OnAddOrUpdateIgnoreBeforeIgnoreAfter).HasDefaultValue("Rabbit");
                    b.Property(e => e.OnAddOrUpdateThrowBeforeIgnoreAfter).HasDefaultValue("Rabbit");
                    b.Property(e => e.OnAddOrUpdateUseBeforeThrowAfter).HasDefaultValue("Rabbit");
                    b.Property(e => e.OnAddOrUpdateIgnoreBeforeThrowAfter).HasDefaultValue("Rabbit");
                    b.Property(e => e.OnAddOrUpdateThrowBeforeThrowAfter).HasDefaultValue("Rabbit");

                    b.Property(e => e.OnUpdate).HasDefaultValue("Rabbit");
                    b.Property(e => e.OnUpdateUseBeforeUseAfter).HasDefaultValue("Rabbit");
                    b.Property(e => e.OnUpdateIgnoreBeforeUseAfter).HasDefaultValue("Rabbit");
                    b.Property(e => e.OnUpdateThrowBeforeUseAfter).HasDefaultValue("Rabbit");
                    b.Property(e => e.OnUpdateUseBeforeIgnoreAfter).HasDefaultValue("Rabbit");
                    b.Property(e => e.OnUpdateIgnoreBeforeIgnoreAfter).HasDefaultValue("Rabbit");
                    b.Property(e => e.OnUpdateThrowBeforeIgnoreAfter).HasDefaultValue("Rabbit");
                    b.Property(e => e.OnUpdateUseBeforeThrowAfter).HasDefaultValue("Rabbit");
                    b.Property(e => e.OnUpdateIgnoreBeforeThrowAfter).HasDefaultValue("Rabbit");
                    b.Property(e => e.OnUpdateThrowBeforeThrowAfter).HasDefaultValue("Rabbit");
                });

            modelBuilder.Entity<WithBackingFields>(
                b =>
                {
                    b.Property(e => e.NullableAsNonNullable).HasComputedColumnSql("1");
                    b.Property(e => e.NonNullableAsNullable).HasComputedColumnSql("1");
                });

            modelBuilder.Entity<WithNoBackingFields>(
                b =>
                {
                    b.Property(e => e.TrueDefault).HasDefaultValue(true);
                    b.Property(e => e.NonZeroDefault).HasDefaultValue(-1);
                    b.Property(e => e.FalseDefault).HasDefaultValue(false);
                    b.Property(e => e.ZeroDefault).HasDefaultValue(0);
                });

            modelBuilder.Entity<WithNullableBackingFields>(
                b =>
                {
                    b.Property(e => e.NullableBackedBoolTrueDefault).HasDefaultValue(true);
                    b.Property(e => e.NullableBackedIntNonZeroDefault).HasDefaultValue(-1);
                    b.Property(e => e.NullableBackedBoolFalseDefault).HasDefaultValue(false);
                    b.Property(e => e.NullableBackedIntZeroDefault).HasDefaultValue(0);
                });

            modelBuilder.Entity<WithObjectBackingFields>(
                b =>
                {
                    b.Property(e => e.NullableBackedBoolTrueDefault).HasDefaultValue(true);
                    b.Property(e => e.NullableBackedIntNonZeroDefault).HasDefaultValue(-1);
                    b.Property(e => e.NullableBackedBoolFalseDefault).HasDefaultValue(false);
                    b.Property(e => e.NullableBackedIntZeroDefault).HasDefaultValue(0);
                });

            modelBuilder.Entity<NonStoreGenDependent>().Property(e => e.HasTemp).HasDefaultValue(777);

            modelBuilder.Entity<CompositePrincipal>().Property(e => e.Id).UseIdentityColumn();

            modelBuilder.Entity<WrappedIntHiLoClassPrincipal>().Property(e => e.Id).UseHiLo();
            modelBuilder.Entity<WrappedIntHiLoStructPrincipal>().Property(e => e.Id).UseHiLo();
            modelBuilder.Entity<WrappedIntHiLoRecordPrincipal>().Property(e => e.Id).UseHiLo();
            modelBuilder.Entity<WrappedIntHiLoClassDependentShadow>().Property(e => e.Id).UseHiLo();
            modelBuilder.Entity<WrappedIntHiLoStructDependentShadow>().Property(e => e.Id).UseHiLo();
            modelBuilder.Entity<WrappedIntHiLoRecordDependentShadow>().Property(e => e.Id).UseHiLo();
            modelBuilder.Entity<WrappedIntHiLoClassDependentOptional>().Property(e => e.Id).UseHiLo();
            modelBuilder.Entity<WrappedIntHiLoStructDependentOptional>().Property(e => e.Id).UseHiLo();
            modelBuilder.Entity<WrappedIntHiLoRecordDependentOptional>().Property(e => e.Id).UseHiLo();
            modelBuilder.Entity<WrappedIntHiLoClassDependentRequired>().Property(e => e.Id).UseHiLo();
            modelBuilder.Entity<WrappedIntHiLoStructDependentRequired>().Property(e => e.Id).UseHiLo();
            modelBuilder.Entity<WrappedIntHiLoRecordDependentRequired>().Property(e => e.Id).UseHiLo();

            modelBuilder.Entity<LongToDecimalPrincipal>(
                entity =>
                {
                    var keyConverter = new ValueConverter<long, decimal>(
                        v => new decimal(v),
                        v => decimal.ToInt64(v));

                    entity.Property(e => e.Id)
                        .HasPrecision(18, 0)
                        .HasConversion(keyConverter);
                });

            base.OnModelCreating(modelBuilder, context);
        }

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            base.ConfigureConventions(configurationBuilder);

            configurationBuilder.Properties<WrappedIntHiLoClass>()
                .HaveConversion<WrappedIntHiLoClassConverter, WrappedIntHiLoClassComparer>();
            configurationBuilder.Properties<WrappedIntHiLoKeyClass>()
                .HaveConversion<WrappedIntHiLoKeyClassConverter, WrappedIntHiLoKeyClassComparer>();
            configurationBuilder.Properties<WrappedIntHiLoStruct>().HaveConversion<WrappedIntHiLoStructConverter>();
            configurationBuilder.Properties<WrappedIntHiLoKeyStruct>().HaveConversion<WrappedIntHiLoKeyStructConverter>();
            configurationBuilder.Properties<WrappedIntHiLoRecord>().HaveConversion<WrappedIntHiLoRecordConverter>();
            configurationBuilder.Properties<WrappedIntHiLoKeyRecord>().HaveConversion<WrappedIntHiLoKeyRecordConverter>();
        }
    }
}
