// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;

namespace Microsoft.EntityFrameworkCore.ChangeTracking.Internal;

public class CurrentValueComparerTest
{
    [ConditionalTheory]
    [InlineData(typeof(EntryCurrentValueComparer<int>), nameof(Godzilla.Id))]
    [InlineData(typeof(EntryCurrentValueComparer<int>), nameof(Godzilla.Int))]
    [InlineData(typeof(EntryCurrentValueComparer<ulong>), nameof(Godzilla.ULong))]
    [InlineData(typeof(CurrentProviderValueComparer<IntStruct, int>), nameof(Godzilla.IntStruct))]
    [InlineData(typeof(StructuralEntryCurrentProviderValueComparer), nameof(Godzilla.BytesStruct))]
    [InlineData(typeof(EntryCurrentValueComparer), nameof(Godzilla.ComparableIntStruct))]
    [InlineData(typeof(EntryCurrentValueComparer), nameof(Godzilla.ComparableBytesStruct))]
    [InlineData(typeof(EntryCurrentValueComparer<GenericComparableIntStruct>), nameof(Godzilla.GenericComparableIntStruct))]
    [InlineData(typeof(EntryCurrentValueComparer<GenericComparableBytesStruct>), nameof(Godzilla.GenericComparableBytesStruct))]
    [InlineData(typeof(StructuralEntryCurrentValueComparer), nameof(Godzilla.StructuralComparableBytesStruct))]
    [InlineData(typeof(NullableStructCurrentProviderValueComparer<IntStruct, int>), nameof(Godzilla.NullableIntStruct))]
    [InlineData(typeof(StructuralEntryCurrentProviderValueComparer), nameof(Godzilla.NullableBytesStruct))]
    [InlineData(typeof(EntryCurrentValueComparer), nameof(Godzilla.NullableComparableIntStruct))]
    [InlineData(typeof(EntryCurrentValueComparer), nameof(Godzilla.NullableComparableBytesStruct))]
    [InlineData(typeof(EntryCurrentValueComparer<GenericComparableIntStruct?>), nameof(Godzilla.NullableGenericComparableIntStruct))]
    [InlineData(
        typeof(EntryCurrentValueComparer<GenericComparableBytesStruct?>), nameof(Godzilla.NullableGenericComparableBytesStruct))]
    [InlineData(typeof(StructuralEntryCurrentValueComparer), nameof(Godzilla.NullableStructuralComparableBytesStruct))]
    [InlineData(typeof(EntryCurrentValueComparer<int?>), nameof(Godzilla.NullableInt))]
    [InlineData(typeof(EntryCurrentValueComparer<ulong?>), nameof(Godzilla.NullableULong))]
    [InlineData(typeof(EntryCurrentValueComparer<string>), nameof(Godzilla.String))]
    [InlineData(typeof(StructuralEntryCurrentValueComparer), nameof(Godzilla.Bytes))]
    [InlineData(typeof(NullableClassCurrentProviderValueComparer<IntClass, int>), nameof(Godzilla.IntClass))]
    [InlineData(typeof(EntryCurrentValueComparer), nameof(Godzilla.ComparableIntClass))]
    [InlineData(typeof(EntryCurrentValueComparer<GenericComparableIntClass>), nameof(Godzilla.GenericComparableIntClass))]
    public void Factory_creates_expected_comparer(Type expectedComparer, string property)
    {
        using var context = new GodzillaContext();

        var factory = CurrentValueComparerFactory.Instance;

        Assert.IsType(expectedComparer, factory.Create(context.Model.FindEntityType(typeof(Godzilla)).FindProperty(property)));
    }

    [ConditionalFact]
    public void Factory_throws_if_provider_type_is_not_comparable()
    {
        using var context = new GodzillaContext();

        var factory = CurrentValueComparerFactory.Instance;

        Assert.Equal(
            CoreStrings.NonComparableKeyType(
                nameof(Godzilla), nameof(Godzilla.NotComparable), nameof(NotComparable)),
            Assert.Throws<InvalidOperationException>(
                () => factory.Create(
                    context.Model.FindEntityType(typeof(Godzilla)).FindProperty(nameof(Godzilla.NotComparable)))).Message);
    }

    [ConditionalFact]
    public void Factory_throws_if_model_and_provider_type_are_not_comparable()
    {
        using var context = new GodzillaContext();

        var factory = CurrentValueComparerFactory.Instance;

        Assert.Equal(
            CoreStrings.NonComparableKeyTypes(
                nameof(Godzilla), nameof(Godzilla.NotComparableConverted), nameof(NotComparable), nameof(NotComparable)),
            Assert.Throws<InvalidOperationException>(
                () => factory.Create(
                    context.Model.FindEntityType(typeof(Godzilla)).FindProperty(nameof(Godzilla.NotComparableConverted)))).Message);
    }

    [ConditionalFact]
    public void Can_sort_ints()
        => CanSort(nameof(Godzilla.Int), i => new Godzilla { Int = i }, g => g.Int);

    [ConditionalFact]
    public void Can_sort_ulongs()
        => CanSort(nameof(Godzilla.ULong), i => new Godzilla { ULong = (ulong)i }, g => (int)g.ULong);

    [ConditionalFact]
    public void Can_sort_IntStructs()
        => CanSort(
            nameof(Godzilla.IntStruct),
            i => new Godzilla { IntStruct = new IntStruct { Value = i } },
            g => g.IntStruct.Value);

    [ConditionalFact]
    public void Can_sort_ComparableIntStructs()
        => CanSort(
            nameof(Godzilla.ComparableIntStruct),
            i => new Godzilla { ComparableIntStruct = new ComparableIntStruct { Value = i } },
            g => g.ComparableIntStruct.Value);

    [ConditionalFact]
    public void Can_sort_GenericComparableIntStructs()
        => CanSort(
            nameof(Godzilla.GenericComparableIntStruct),
            i => new Godzilla { GenericComparableIntStruct = new GenericComparableIntStruct { Value = i } },
            g => g.GenericComparableIntStruct.Value);

    private void CanSort(
        string propertyName,
        Func<int, Godzilla> generator,
        Func<Godzilla, int> selector)
    {
        using var context = new GodzillaContext();

        context.AttachRange(
            generator(0), generator(9),
            generator(0), generator(3),
            generator(1), generator(9), generator(7), generator(3));

        var comparer = context.Model
            .FindEntityType(typeof(Godzilla))
            .FindProperty(propertyName)
            .GetCurrentValueComparer();

        var entries = context.ChangeTracker.Entries<Godzilla>()
            .OrderBy(e => e.GetInfrastructure(), comparer)
            .Select(e => selector(e.Entity))
            .ToList();

        Assert.Equal(new[] { 0, 0, 1, 3, 3, 7, 9, 9 }, entries);
    }

    [ConditionalFact]
    public void Can_sort_BytesStructs()
        => CanSort(
            nameof(Godzilla.BytesStruct),
            i => new Godzilla { BytesStruct = new BytesStruct { Value = i } },
            g => g.BytesStruct.Value);

    [ConditionalFact]
    public void Can_sort_ComparableBytesStructs()
        => CanSort(
            nameof(Godzilla.ComparableBytesStruct),
            i => new Godzilla { ComparableBytesStruct = new ComparableBytesStruct { Value = i } },
            g => g.ComparableBytesStruct.Value);

    [ConditionalFact]
    public void Can_sort_GenericComparableBytesStruct()
        => CanSort(
            nameof(Godzilla.GenericComparableBytesStruct),
            i => new Godzilla { GenericComparableBytesStruct = new GenericComparableBytesStruct { Value = i } },
            g => g.GenericComparableBytesStruct.Value);

    [ConditionalFact]
    public void Can_sort_StructuralComparableBytesStruct()
        => CanSort(
            nameof(Godzilla.StructuralComparableBytesStruct),
            i => new Godzilla { StructuralComparableBytesStruct = new StructuralComparableBytesStruct { Value = i } },
            g => g.StructuralComparableBytesStruct.Value);

    private void CanSort(
        string propertyName,
        Func<byte[], Godzilla> generator,
        Func<Godzilla, byte[]> selector)
    {
        using var context = new GodzillaContext();

        context.AttachRange(
            generator([]), generator([9]),
            generator([]), generator([3, 3, 3]),
            generator([1, 1]), generator([9]), generator([7]), generator([3, 3]));

        var comparer = context.Model
            .FindEntityType(typeof(Godzilla))
            .FindProperty(propertyName)
            .GetCurrentValueComparer();

        var entries = context.ChangeTracker.Entries<Godzilla>()
            .OrderBy(e => e.GetInfrastructure(), comparer)
            .Select(e => selector(e.Entity))
            .ToList();

        Assert.Equal(
            new byte[][]
            {
                [],
                [],
                [7],
                [9],
                [9],
                [1, 1],
                [3, 3],
                [3, 3, 3]
            },
            entries);
    }

    [ConditionalFact]
    public void Can_sort_NullableIntStructs()
        => CanSortNullable(
            nameof(Godzilla.NullableIntStruct),
            i => new Godzilla { NullableIntStruct = i.HasValue ? new IntStruct { Value = i.Value } : null },
            g => g.NullableIntStruct?.Value);

    [ConditionalFact]
    public void Can_sort_NullableComparableIntStructs()
        => CanSortNullable(
            nameof(Godzilla.NullableComparableIntStruct),
            i => new Godzilla { NullableComparableIntStruct = i.HasValue ? new ComparableIntStruct { Value = i.Value } : null },
            g => g.NullableComparableIntStruct?.Value);

    [ConditionalFact]
    public void Can_sort_NullableGenericComparableIntStructs()
        => CanSortNullable(
            nameof(Godzilla.NullableGenericComparableIntStruct),
            i => new Godzilla
            {
                NullableGenericComparableIntStruct =
                    i.HasValue ? new GenericComparableIntStruct { Value = i.Value } : null
            },
            g => g.NullableGenericComparableIntStruct?.Value);

    [ConditionalFact]
    public void Can_sort_NullableInts()
        => CanSortNullable(
            nameof(Godzilla.NullableInt),
            i => new Godzilla { NullableInt = i },
            g => g.NullableInt);

    [ConditionalFact]
    public void Can_sort_NullableULongs()
        => CanSortNullable(
            nameof(Godzilla.NullableULong),
            i => new Godzilla { NullableULong = (ulong?)i },
            g => (int?)g.NullableULong);

    [ConditionalFact]
    public void Can_sort_Strings()
        => CanSortNullable(
            nameof(Godzilla.String),
            i => new Godzilla { String = i?.ToString() },
            g => g.String == null ? null : int.Parse(g.String));

    [ConditionalFact]
    public void Can_sort_IntClasses()
        => CanSortNullable(
            nameof(Godzilla.IntClass),
            i => new Godzilla { IntClass = i.HasValue ? new IntClass { Value = i.Value } : null },
            g => g.IntClass?.Value);

    [ConditionalFact]
    public void Can_sort_ComparableIntClasses()
        => CanSortNullable(
            nameof(Godzilla.ComparableIntClass),
            i => new Godzilla { ComparableIntClass = i.HasValue ? new ComparableIntClass { Value = i.Value } : null },
            g => g.ComparableIntClass?.Value);

    [ConditionalFact]
    public void Can_sort_GenericComparableIntClass()
        => CanSortNullable(
            nameof(Godzilla.GenericComparableIntClass),
            i => new Godzilla { GenericComparableIntClass = i.HasValue ? new GenericComparableIntClass { Value = i.Value } : null },
            g => g.GenericComparableIntClass?.Value);

    private void CanSortNullable(
        string propertyName,
        Func<int?, Godzilla> generator,
        Func<Godzilla, int?> selector)
    {
        using var context = new GodzillaContext();

        context.AttachRange(
            generator(null), generator(9),
            generator(null), generator(3),
            generator(1), generator(9), generator(7), generator(3));

        var comparer = context.Model
            .FindEntityType(typeof(Godzilla))
            .FindProperty(propertyName)
            .GetCurrentValueComparer();

        var entries = context.ChangeTracker.Entries<Godzilla>()
            .OrderBy(e => e.GetInfrastructure(), comparer)
            .Select(e => selector(e.Entity))
            .ToList();

        Assert.Equal(new int?[] { null, null, 1, 3, 3, 7, 9, 9 }, entries);
    }

    [ConditionalFact]
    public void Can_sort_NullableBytesStructs()
        => CanSortNullable(
            nameof(Godzilla.NullableBytesStruct),
            i => new Godzilla { NullableBytesStruct = i == null ? null : new BytesStruct { Value = i } },
            g => g.NullableBytesStruct?.Value);

    [ConditionalFact]
    public void Can_sort_NullableComparableBytesStructs()
        => CanSortNullable(
            nameof(Godzilla.NullableComparableBytesStruct),
            i => new Godzilla { NullableComparableBytesStruct = i == null ? null : new ComparableBytesStruct { Value = i } },
            g => g.NullableComparableBytesStruct?.Value);

    [ConditionalFact]
    public void Can_sort_NullableGenericComparableBytesStructs()
        => CanSortNullable(
            nameof(Godzilla.NullableGenericComparableBytesStruct),
            i => new Godzilla
            {
                NullableGenericComparableBytesStruct =
                    i == null ? null : new GenericComparableBytesStruct { Value = i }
            },
            g => g.NullableGenericComparableBytesStruct?.Value);

    [ConditionalFact]
    public void Can_sort_NullableStructuralComparableBytesStructs()
        => CanSortNullable(
            nameof(Godzilla.NullableStructuralComparableBytesStruct),
            i => new Godzilla
            {
                NullableStructuralComparableBytesStruct = i == null
                    ? null
                    : new StructuralComparableBytesStruct { Value = i }
            },
            g => g.NullableStructuralComparableBytesStruct?.Value);

    [ConditionalFact]
    public void Can_sort_Bytes()
        => CanSortNullable(
            nameof(Godzilla.Bytes),
            i => new Godzilla { Bytes = i },
            g => g.Bytes);

    private void CanSortNullable(
        string propertyName,
        Func<byte[], Godzilla> generator,
        Func<Godzilla, byte[]> selector)
    {
        using var context = new GodzillaContext();

        context.AttachRange(
            generator(null), generator([9]),
            generator(null), generator([3, 3, 3]),
            generator([1, 1]), generator([9]), generator([7]), generator([3, 3]));

        var comparer = context.Model
            .FindEntityType(typeof(Godzilla))
            .FindProperty(propertyName)
            .GetCurrentValueComparer();

        var entries = context.ChangeTracker.Entries<Godzilla>()
            .OrderBy(e => e.GetInfrastructure(), comparer)
            .Select(e => selector(e.Entity))
            .ToList();

        Assert.Equal(
            new[]
            {
                null,
                null,
                [7],
                [9],
                [9],
                [1, 1],
                [3, 3],
                new byte[] { 3, 3, 3 }
            },
            entries);
    }

    private class GodzillaContext : DbContext
    {
        protected internal override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseInMemoryDatabase(nameof(GodzillaContext));

        protected internal override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<Godzilla>(
                b =>
                {
                    b.Property(e => e.IntStruct).HasConversion(IntStruct.Converter);
                    b.Property(e => e.BytesStruct).HasConversion(BytesStruct.Converter);
                    b.Property(e => e.ComparableIntStruct).HasConversion(ComparableIntStruct.Converter);
                    b.Property(e => e.ComparableBytesStruct).HasConversion(ComparableBytesStruct.Converter);
                    b.Property(e => e.GenericComparableIntStruct).HasConversion(GenericComparableIntStruct.Converter);
                    b.Property(e => e.GenericComparableBytesStruct).HasConversion(GenericComparableBytesStruct.Converter);
                    b.Property(e => e.StructuralComparableBytesStruct).HasConversion(StructuralComparableBytesStruct.Converter);

                    b.Property(e => e.NullableIntStruct).HasConversion(IntStruct.Converter);
                    b.Property(e => e.NullableBytesStruct).HasConversion(BytesStruct.Converter);
                    b.Property(e => e.NullableComparableIntStruct).HasConversion(ComparableIntStruct.Converter);
                    b.Property(e => e.NullableComparableBytesStruct).HasConversion(ComparableBytesStruct.Converter);
                    b.Property(e => e.NullableGenericComparableIntStruct).HasConversion(GenericComparableIntStruct.Converter);
                    b.Property(e => e.NullableGenericComparableBytesStruct).HasConversion(GenericComparableBytesStruct.Converter);
                    b.Property(e => e.NullableStructuralComparableBytesStruct).HasConversion(StructuralComparableBytesStruct.Converter);

                    b.Property(e => e.IntClass).HasConversion(IntClass.Converter);
                    b.Property(e => e.ComparableIntClass).HasConversion(ComparableIntClass.Converter);
                    b.Property(e => e.GenericComparableIntClass).HasConversion(GenericComparableIntClass.Converter);

                    b.Property(e => e.NotComparable);
                    b.Property(e => e.NotComparableConverted).HasConversion(NotComparable.Converter);
                });
    }

    private class Godzilla
    {
        public int Id { get; set; }
        public int Int { get; set; }
        public ulong ULong { get; set; }
        public IntStruct IntStruct { get; set; }
        public BytesStruct BytesStruct { get; set; }
        public ComparableIntStruct ComparableIntStruct { get; set; }
        public ComparableBytesStruct ComparableBytesStruct { get; set; }
        public GenericComparableIntStruct GenericComparableIntStruct { get; set; }
        public GenericComparableBytesStruct GenericComparableBytesStruct { get; set; }
        public StructuralComparableBytesStruct StructuralComparableBytesStruct { get; set; }
        public IntStruct? NullableIntStruct { get; set; }
        public BytesStruct? NullableBytesStruct { get; set; }
        public ComparableIntStruct? NullableComparableIntStruct { get; set; }
        public ComparableBytesStruct? NullableComparableBytesStruct { get; set; }
        public GenericComparableIntStruct? NullableGenericComparableIntStruct { get; set; }
        public GenericComparableBytesStruct? NullableGenericComparableBytesStruct { get; set; }
        public StructuralComparableBytesStruct? NullableStructuralComparableBytesStruct { get; set; }
        public int? NullableInt { get; set; }
        public ulong? NullableULong { get; set; }
        public string String { get; set; }
        public byte[] Bytes { get; set; }
        public IntClass IntClass { get; set; }
        public ComparableIntClass ComparableIntClass { get; set; }
        public GenericComparableIntClass GenericComparableIntClass { get; set; }
        public NotComparable NotComparable { get; set; }
        public NotComparable NotComparableConverted { get; set; }
    }

    private struct NotComparable
    {
        public static readonly ValueConverter<NotComparable, NotComparable> Converter
            = new(v => new NotComparable(), v => new NotComparable());
    }

    private struct IntStruct
    {
        public static readonly ValueConverter<IntStruct, int> Converter
            = new(v => v.Value, v => new IntStruct { Value = v });

        public int Value { get; set; }
    }

    private struct BytesStruct
    {
        public static readonly ValueConverter<BytesStruct, byte[]> Converter
            = new(v => v.Value, v => new BytesStruct { Value = v });

        public byte[] Value { get; set; }

        public bool Equals(BytesStruct other)
            => (Value == null
                    && other.Value == null)
                || (other.Value != null
                    && Value?.SequenceEqual(other.Value) == true);

        public override int GetHashCode()
        {
            var code = new HashCode();
            if (Value != null)
            {
                foreach (var b in Value)
                {
                    code.Add(b);
                }
            }

            return code.ToHashCode();
        }
    }

    private struct ComparableIntStruct : IComparable
    {
        public static readonly ValueConverter<ComparableIntStruct, int> Converter
            = new(v => v.Value, v => new ComparableIntStruct { Value = v });

        public int Value { get; set; }

        public int CompareTo(object other)
            => Value - ((ComparableIntStruct)other).Value;
    }

    private struct ComparableBytesStruct : IComparable
    {
        public static readonly ValueConverter<ComparableBytesStruct, byte[]> Converter
            = new(v => v.Value, v => new ComparableBytesStruct { Value = v });

        public byte[] Value { get; set; }

        public bool Equals(ComparableBytesStruct other)
            => (Value == null
                    && other.Value == null)
                || (other.Value != null
                    && Value?.SequenceEqual(other.Value) == true);

        public override int GetHashCode()
        {
            var code = new HashCode();
            if (Value != null)
            {
                foreach (var b in Value)
                {
                    code.Add(b);
                }
            }

            return code.ToHashCode();
        }

        public int CompareTo(object other)
        {
            var result = Value.Length - ((ComparableBytesStruct)other).Value.Length;
            if (result != 0)
            {
                return result;
            }

            return StructuralComparisons.StructuralComparer.Compare(Value, ((ComparableBytesStruct)other).Value);
        }
    }

    private struct GenericComparableIntStruct : IComparable<GenericComparableIntStruct>
    {
        public static readonly ValueConverter<GenericComparableIntStruct, int> Converter
            = new(v => v.Value, v => new GenericComparableIntStruct { Value = v });

        public int Value { get; set; }

        public int CompareTo(GenericComparableIntStruct other)
            => Value - other.Value;
    }

    private struct GenericComparableBytesStruct : IComparable<GenericComparableBytesStruct>
    {
        public static readonly ValueConverter<GenericComparableBytesStruct, byte[]> Converter
            = new(v => v.Value, v => new GenericComparableBytesStruct { Value = v });

        public byte[] Value { get; set; }

        public bool Equals(GenericComparableBytesStruct other)
            => (Value == null
                    && other.Value == null)
                || (other.Value != null
                    && Value?.SequenceEqual(other.Value) == true);

        public override int GetHashCode()
        {
            var code = new HashCode();
            if (Value != null)
            {
                foreach (var b in Value)
                {
                    code.Add(b);
                }
            }

            return code.ToHashCode();
        }

        public int CompareTo(GenericComparableBytesStruct other)
        {
            var result = Value.Length - other.Value.Length;
            if (result != 0)
            {
                return result;
            }

            return StructuralComparisons.StructuralComparer.Compare(Value, other.Value);
        }
    }

    private struct StructuralComparableBytesStruct : IStructuralComparable
    {
        public static readonly ValueConverter<StructuralComparableBytesStruct, byte[]> Converter
            = new(v => v.Value, v => new StructuralComparableBytesStruct { Value = v });

        public byte[] Value { get; set; }

        public bool Equals(StructuralComparableBytesStruct other)
            => (Value == null
                    && other.Value == null)
                || (other.Value != null
                    && Value?.SequenceEqual(other.Value) == true);

        public override int GetHashCode()
        {
            var code = new HashCode();
            if (Value != null)
            {
                foreach (var b in Value)
                {
                    code.Add(b);
                }
            }

            return code.ToHashCode();
        }

        public int CompareTo(object other, IComparer comparer)
        {
            var typedOther = ((StructuralComparableBytesStruct)other);

            var i = -1;
            var result = Value.Length - typedOther.Value.Length;

            while (result == 0
                   && ++i < Value.Length)
            {
                result = comparer.Compare(Value[i], typedOther.Value[i]);
            }

            return result;
        }
    }

    private class IntClass
    {
        public static readonly ValueConverter<IntClass, int> Converter
            = new(v => v.Value, v => new IntClass { Value = v });

        private bool Equals(IntClass other)
            => other != null && Value == other.Value;

        public override bool Equals(object obj)
            => obj == this
                || obj?.GetType() == GetType()
                && Equals((IntClass)obj);

        public override int GetHashCode()
            => Value;

        public int Value { get; set; }
    }

    private class ComparableIntClass : IComparable
    {
        public static readonly ValueConverter<ComparableIntClass, int> Converter
            = new(v => v.Value, v => new ComparableIntClass { Value = v });

        public int Value { get; set; }

        private bool Equals(ComparableIntClass other)
            => other != null && Value == other.Value;

        public override bool Equals(object obj)
            => obj == this
                || obj?.GetType() == GetType()
                && Equals((ComparableIntClass)obj);

        public override int GetHashCode()
            => Value;

        public int CompareTo(object other)
            => Value - ((ComparableIntClass)other).Value;
    }

    private class GenericComparableIntClass : IComparable<GenericComparableIntClass>
    {
        public static readonly ValueConverter<GenericComparableIntClass, int> Converter
            = new(v => v.Value, v => new GenericComparableIntClass { Value = v });

        public int Value { get; set; }

        private bool Equals(GenericComparableIntClass other)
            => other != null && Value == other.Value;

        public override bool Equals(object obj)
            => obj == this
                || obj?.GetType() == GetType()
                && Equals((GenericComparableIntClass)obj);

        public override int GetHashCode()
            => Value;

        public int CompareTo(GenericComparableIntClass other)
            => Value - other.Value;
    }
}
