// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.ChangeTracking;

public class ValueComparerTest
{
    protected class FakeValueComparer : ValueComparer<double>
    {
        public FakeValueComparer()
            : base(false)
        {
        }
    }

    private class Foo
    {
        public int Id { get; set; }
        public int Bar { get; set; }
    }

    [ConditionalFact]
    public void Throws_for_comparer_with_wrong_type()
    {
        using var context = new InvalidDbContext();

        Assert.Equal(
            CoreStrings.ComparerPropertyMismatch("double", nameof(Foo), nameof(Foo.Bar), "int"),
            Assert.Throws<InvalidOperationException>(() => context.Model).Message);
    }

    private class InvalidDbContext : DbContext
    {
        protected internal override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseInMemoryDatabase(Guid.NewGuid().ToString());

        protected internal override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<Foo>().Property(e => e.Bar).HasConversion<string>(new FakeValueComparer());
    }

    [ConditionalFact]
    public void Throws_for_provider_comparer_with_wrong_type()
    {
        using var context = new InvalidProviderDbContext();

        Assert.Equal(
            CoreStrings.ComparerPropertyMismatch("double", nameof(Foo), nameof(Foo.Bar), "string"),
            Assert.Throws<InvalidOperationException>(() => context.Model).Message);
    }

    private class InvalidProviderDbContext : DbContext
    {
        protected internal override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseInMemoryDatabase(Guid.NewGuid().ToString());

        protected internal override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<Foo>().Property(e => e.Bar).HasConversion<string>((ValueComparer)null, new FakeValueComparer());
    }

    [ConditionalTheory]
    [InlineData(typeof(byte), (byte)1, (byte)2, 1)]
    [InlineData(typeof(ushort), (ushort)1, (ushort)2, 1)]
    [InlineData(typeof(uint), (uint)1, (uint)2, 1)]
    [InlineData(typeof(ulong), (ulong)1, (ulong)2, null)]
    [InlineData(typeof(sbyte), (sbyte)1, (sbyte)2, 1)]
    [InlineData(typeof(short), (short)1, (short)2, 1)]
    [InlineData(typeof(int), 1, 2, 1)]
    [InlineData(typeof(long), (long)1, (long)2, null)]
    [InlineData(typeof(char), 'A', 'B', (int)'A')]
    [InlineData(typeof(string), "A", "B", null)]
    [InlineData(typeof(bool), true, false, null)]
    [InlineData(typeof(object), 1, "B", null)]
    [InlineData(typeof(float), (float)1, (float)2, null)]
    [InlineData(typeof(double), (double)1, (double)2, null)]
    [InlineData(typeof(JustAnEnum), JustAnEnum.A, JustAnEnum.B, null)]
    [InlineData(typeof(int[]), new[] { 1, 2 }, new[] { 3, 4 }, null)]
    public ValueComparer Default_comparer_works_for_normal_types(Type type, object value1, object value2, int? hashCode)
        => CompareTest(type, value1, value2, hashCode);

    private static ValueComparer CompareTest(Type type, object value1, object value2, int? hashCode = null)
        => CompareTest(type, value1, value2, hashCode, false);

    private static ValueComparer CompareTest(Type type, object value1, object value2, int? hashCode, bool toNullable)
    {
        var comparer = (ValueComparer)Activator.CreateInstance(typeof(ValueComparer<>).MakeGenericType(type), [false]);
        if (toNullable)
        {
            comparer = ToNonNullNullableComparer(comparer);
        }

        Assert.True(comparer.Equals(value1, value1));
        Assert.True(comparer.Equals(value2, value2));
        Assert.False(comparer.Equals(value1, value2));
        Assert.False(comparer.Equals(value2, value1));
        Assert.False(comparer.Equals(value1, null));
        Assert.False(comparer.Equals(null, value2));
        Assert.True(comparer.Equals(null, null));

        Assert.Equal(hashCode ?? value1.GetHashCode(), comparer.GetHashCode(value1));

        var keyComparer = (ValueComparer)Activator.CreateInstance(typeof(ValueComparer<>).MakeGenericType(type), [true]);
        if (toNullable)
        {
            keyComparer = ToNonNullNullableComparer(keyComparer);
        }

        Assert.True(keyComparer.Equals(value1, value1));
        Assert.True(keyComparer.Equals(value2, value2));
        Assert.False(keyComparer.Equals(value1, value2));
        Assert.False(keyComparer.Equals(value2, value1));
        Assert.False(keyComparer.Equals(value1, null));
        Assert.False(keyComparer.Equals(null, value2));
        Assert.True(keyComparer.Equals(null, null));

        return comparer;
    }

    public static ValueComparer ToNonNullNullableComparer(ValueComparer comparer)
    {
        var type = comparer.EqualsExpression.Parameters[0].Type;
        var nullableType = type.MakeNullable();

        var newEqualsParam1 = Expression.Parameter(nullableType, "v1");
        var newEqualsParam2 = Expression.Parameter(nullableType, "v2");
        var newHashCodeParam = Expression.Parameter(nullableType, "v");
        var newSnapshotParam = Expression.Parameter(nullableType, "v");

        return (ValueComparer)Activator.CreateInstance(
            typeof(NonNullNullableValueComparer<>).MakeGenericType(nullableType),
            Expression.Lambda(
                comparer.ExtractEqualsBody(
                    Expression.Convert(newEqualsParam1, type),
                    Expression.Convert(newEqualsParam2, type)),
                newEqualsParam1, newEqualsParam2),
            Expression.Lambda(
                comparer.ExtractHashCodeBody(
                    Expression.Convert(newHashCodeParam, type)),
                newHashCodeParam),
            Expression.Lambda(
                Expression.Convert(
                    comparer.ExtractSnapshotBody(
                        Expression.Convert(newSnapshotParam, type)),
                    nullableType),
                newSnapshotParam))!;
    }

    private sealed class NonNullNullableValueComparer<T>(
        LambdaExpression equalsExpression,
        LambdaExpression hashCodeExpression,
        LambdaExpression snapshotExpression) : ValueComparer<T>(
            (Expression<Func<T, T, bool>>)equalsExpression,
            (Expression<Func<T, int>>)hashCodeExpression,
            (Expression<Func<T, T>>)snapshotExpression);

    private enum JustAnEnum : ushort
    {
        A,
        B
    }

    [ConditionalFact]
    public void Default_comparer_works_for_decimals()
        => CompareTest(typeof(decimal), (decimal)1, (decimal)2);

    [ConditionalFact]
    public void Default_comparer_works_for_structs()
    {
        CompareTest(
            typeof(JustAStruct),
            new JustAStruct { A = 1, B = "B1" },
            new JustAStruct { A = 1, B = "B2" });

        CompareTest(
            typeof(JustAStruct),
            new JustAStruct { A = 1, B = "B" },
            new JustAStruct { A = 2, B = "B" });
    }

    private struct JustAStruct
    {
        public int A { get; set; }
        public string B { get; set; }
    }

    [ConditionalFact]
    public void Default_comparer_works_for_structs_with_equality()
        => CompareTest(
            typeof(JustAStructWithEquality),
            new JustAStructWithEquality { A = 1, B = "B" },
            new JustAStructWithEquality { A = 2, B = "B" });

    private struct JustAStructWithEquality
    {
        public int A { get; set; }
        public string B { get; set; }

        private bool Equals(JustAStructWithEquality other)
            => A == other.A;

        public override bool Equals(object obj)
            => obj is JustAStructWithEquality o && Equals(o);

        public override int GetHashCode()
            => A;
    }

    [ConditionalFact]
    public void Default_comparer_works_for_structs_with_equality_operators()
        => CompareTest(
            typeof(JustAStructWithEqualityOperators),
            new JustAStructWithEqualityOperators { A = 1, B = "B" },
            new JustAStructWithEqualityOperators { A = 2, B = "B" });

#pragma warning disable 660,661
    private struct JustAStructWithEqualityOperators
#pragma warning restore 660,661
    {
        public int A { get; set; }
        public string B { get; set; }

        public static bool operator ==(JustAStructWithEqualityOperators left, JustAStructWithEqualityOperators right)
            => left.A == right.A
                && left.B == right.B;

        public static bool operator !=(JustAStructWithEqualityOperators left, JustAStructWithEqualityOperators right)
            => !(left == right);
    }

    [ConditionalFact]
    public void Default_comparer_works_for_classes()
        => CompareTest(
            typeof(JustAClass), // Reference equality
            new JustAClass { A = 1 },
            new JustAClass { A = 1 });

    private class JustAClass
    {
        public int A { get; set; }
    }

    [ConditionalFact]
    public void Default_comparer_works_for_classes_with_equality_members()
    {
        var comparer = CompareTest(
            typeof(JustAClassWithEquality),
            new JustAClassWithEquality { A = 1 },
            new JustAClassWithEquality { A = 2 });

        Assert.True(
            comparer.Equals(
                new JustAClassWithEquality { A = 1 },
                new JustAClassWithEquality { A = 1 }));
    }

    private sealed class JustAClassWithEquality
    {
        public int A { get; set; }

        private bool Equals(JustAClassWithEquality other)
            => A == other.A;

        public override bool Equals(object obj)
            => obj is not null
                && (ReferenceEquals(this, obj)
                    || obj is JustAClassWithEquality o
                    && Equals(o));

        public override int GetHashCode()
            => A;
    }

    [ConditionalFact]
    public void Default_comparer_works_for_classes_with_equality_operators()
    {
        var comparer = CompareTest(
            typeof(JustAClassWithEqualityOperators),
            new JustAClassWithEqualityOperators { A = 1 },
            new JustAClassWithEqualityOperators { A = 2 });

        Assert.True(
            comparer.Equals(
                new JustAClassWithEqualityOperators { A = 1 },
                new JustAClassWithEqualityOperators { A = 1 }));
    }

#pragma warning disable 660,661
    private sealed class JustAClassWithEqualityOperators
#pragma warning restore 660,661
    {
        public int A { get; set; }

        private static bool InternalEquals(JustAClassWithEqualityOperators left, JustAClassWithEqualityOperators right)
            => left is null
                || right is null
                    ? left is null && right is null
                    : left.A == right.A;

        public static bool operator ==(JustAClassWithEqualityOperators left, JustAClassWithEqualityOperators right)
            => InternalEquals(left, right);

        public static bool operator !=(JustAClassWithEqualityOperators left, JustAClassWithEqualityOperators right)
            => !InternalEquals(left, right);
    }

    private void GenericCompareTest<T>(T value1, T value2, int? hashCode = null)
    {
        var comparer = new ValueComparer<T>(false);
        var equals = comparer.EqualsExpression.Compile();
        var getHashCode = comparer.HashCodeExpression.Compile();

        Assert.True(equals(value1, value1));
        Assert.True(equals(value2, value2));
        Assert.False(equals(value1, value2));
        Assert.False(equals(value2, value1));

        var keyComparer = new ValueComparer<T>(true);
        var keyEquals = keyComparer.EqualsExpression.Compile();
        var getKeyHashCode = keyComparer.HashCodeExpression.Compile();

        Assert.True(keyEquals(value1, value1));
        Assert.True(keyEquals(value2, value2));
        Assert.False(keyEquals(value1, value2));
        Assert.False(keyEquals(value2, value1));

        Assert.Equal(hashCode ?? value1.GetHashCode(), getHashCode(value1));
        Assert.Equal(hashCode ?? value1.GetHashCode(), getKeyHashCode(value1));
    }

    private void GenericCompareTestWithNulls<T>(T value1, T value2, int? hashCode = null)
        where T : class
    {
        var comparer = new ValueComparer<T>(false);
        var equals = comparer.EqualsExpression.Compile();
        var getHashCode = comparer.HashCodeExpression.Compile();

        Assert.True(equals(value1, value1));
        Assert.True(equals(value2, value2));
        Assert.False(equals(value1, value2));
        Assert.False(equals(value2, value1));
        Assert.False(equals(value1, null));
        Assert.False(equals(null, value2));
        Assert.True(equals(null, null));

        var keyComparer = new ValueComparer<T>(true);
        var keyEquals = keyComparer.EqualsExpression.Compile();
        var getKeyHashCode = keyComparer.HashCodeExpression.Compile();

        Assert.True(keyEquals(value1, value1));
        Assert.True(keyEquals(value2, value2));
        Assert.False(keyEquals(value1, value2));
        Assert.False(keyEquals(value2, value1));
        Assert.False(keyEquals(value1, null));
        Assert.False(keyEquals(null, value2));
        Assert.True(keyEquals(null, null));

        Assert.Equal(hashCode ?? value1.GetHashCode(), getHashCode(value1));
        Assert.Equal(hashCode ?? value1.GetHashCode(), getKeyHashCode(value1));
    }

    [ConditionalFact]
    public void Default_raw_comparer_works_for_non_null_normal_types()
    {
        GenericCompareTest<byte>(1, 2, 1);
        GenericCompareTest<ushort>(1, 2, 1);
        GenericCompareTest<uint>(1, 2, 1);
        GenericCompareTest<ulong>(1, 2);
        GenericCompareTest<sbyte>(1, 2, 1);
        GenericCompareTest<short>(1, 2, 1);
        GenericCompareTest(1, 2, 1);
        GenericCompareTest<long>(1, 2);
        GenericCompareTest<float>(1, 2);
        GenericCompareTest<double>(1, 2);
        GenericCompareTest<decimal>(1, 2);
        GenericCompareTest('A', 'B', 'A');
        GenericCompareTest("A", "B");
        GenericCompareTest(JustAnEnum.A, JustAnEnum.B);
        GenericCompareTest(new JustAStruct { A = 1 }, new JustAStruct { A = 2 });
        GenericCompareTest(new JustAStructWithEquality { A = 1 }, new JustAStructWithEquality { A = 2 });
        GenericCompareTest(new JustAStructWithEqualityOperators { A = 1 }, new JustAStructWithEqualityOperators { A = 2 });
    }

    [ConditionalFact]
    public void Default_raw_comparer_works_for_reference_types()
    {
        GenericCompareTestWithNulls<object>(1, "A");
        GenericCompareTestWithNulls(new JustAClass { A = 1 }, new JustAClass { A = 2 });
        GenericCompareTestWithNulls(new JustAClassWithEquality { A = 1 }, new JustAClassWithEquality { A = 2 });
        GenericCompareTestWithNulls(new JustAClassWithEqualityOperators { A = 1 }, new JustAClassWithEqualityOperators { A = 2 });
    }

    [ConditionalFact]
    public void Default_comparer_works_for_normal_types_mixing_nullables()
    {
        CompareTest(typeof(byte), (byte)1, (byte?)2, 1);
        CompareTest(typeof(ushort), (ushort?)1, (ushort?)2, 1);
        CompareTest(typeof(uint), (uint?)1, (uint)2, 1);
        CompareTest(typeof(ulong), (ulong)1, (ulong?)2);
        CompareTest(typeof(sbyte), (sbyte?)1, (sbyte)2, 1);
        CompareTest(typeof(short), (short)1, (short?)2, 1);
        CompareTest(typeof(int), (int?)1, 2, 1);
        CompareTest(typeof(long), (long)1, (long?)2);
        CompareTest(typeof(float), (float?)1, (float)2);
        CompareTest(typeof(double), (double)1, (double?)2);
        CompareTest(typeof(decimal), (decimal?)1, (decimal)2);
        CompareTest(typeof(char), (char)1, (char?)2, 1);
        CompareTest(typeof(JustAnEnum), JustAnEnum.A, (JustAnEnum?)JustAnEnum.B);

        CompareTest(
            typeof(JustAStruct),
            (JustAStruct?)new JustAStruct { A = 1 },
            new JustAStruct { A = 2 });

        CompareTest(
            typeof(JustAStructWithEquality),
            (JustAStructWithEquality?)new JustAStructWithEquality { A = 1 },
            new JustAStructWithEquality { A = 2 });

        CompareTest(
            typeof(JustAStructWithEqualityOperators),
            (JustAStructWithEqualityOperators?)new JustAStructWithEqualityOperators { A = 1 },
            new JustAStructWithEqualityOperators { A = 2 });
    }

    [ConditionalFact]
    public void Default_comparer_works_for_normal_nullable_types_mixing_nullables()
    {
        CompareTest(typeof(byte?), (byte)1, (byte?)2, 1);
        CompareTest(typeof(ushort?), (ushort?)1, (ushort?)2, 1);
        CompareTest(typeof(uint?), (uint?)1, (uint)2, 1);
        CompareTest(typeof(ulong?), (ulong)1, (ulong?)2);
        CompareTest(typeof(sbyte?), (sbyte?)1, (sbyte)2, 1);
        CompareTest(typeof(short?), (short)1, (short?)2, 1);
        CompareTest(typeof(int?), (int?)1, 2, 1);
        CompareTest(typeof(long?), (long)1, (long?)2);
        CompareTest(typeof(float?), (float?)1, (float)2);
        CompareTest(typeof(double?), (double)1, (double?)2);
        CompareTest(typeof(decimal?), (decimal?)1, (decimal)2);
        CompareTest(typeof(char?), (char)1, (char?)2, 1);
        CompareTest(typeof(JustAnEnum?), JustAnEnum.A, (JustAnEnum?)JustAnEnum.B);

        CompareTest(
            typeof(JustAStruct?),
            (JustAStruct?)new JustAStruct { A = 1 },
            new JustAStruct { A = 2 });

        CompareTest(
            typeof(JustAStructWithEquality?),
            (JustAStructWithEquality?)new JustAStructWithEquality { A = 1 },
            new JustAStructWithEquality { A = 2 });

        CompareTest(
            typeof(JustAStructWithEqualityOperators?),
            (JustAStructWithEqualityOperators?)new JustAStructWithEqualityOperators { A = 1 },
            new JustAStructWithEqualityOperators { A = 2 });
    }

    [ConditionalFact]
    public void Can_clone_to_nullable()
    {
        CompareTest(typeof(byte), (byte)1, (byte?)2, 1, true);
        CompareTest(typeof(ushort), (ushort?)1, (ushort?)2, 1, true);
        CompareTest(typeof(uint), (uint?)1, (uint)2, 1, true);
        CompareTest(typeof(ulong), (ulong)1, (ulong?)2, null, true);
        CompareTest(typeof(sbyte), (sbyte?)1, (sbyte)2, 1, true);
        CompareTest(typeof(short), (short)1, (short?)2, 1, true);
        CompareTest(typeof(int), (int?)1, 2, 1, true);
        CompareTest(typeof(long), (long)1, (long?)2, null, true);
        CompareTest(typeof(float), (float?)1, (float)2, null, true);
        CompareTest(typeof(double), (double)1, (double?)2, null, true);
        CompareTest(typeof(decimal), (decimal?)1, (decimal)2, null, true);
        CompareTest(typeof(char), (char)1, (char?)2, 1, true);
        CompareTest(typeof(JustAnEnum), JustAnEnum.A, (JustAnEnum?)JustAnEnum.B, null, true);

        CompareTest(
            typeof(JustAStruct),
            (JustAStruct?)new JustAStruct { A = 1 },
            new JustAStruct { A = 2 },
            null,
            true);

        CompareTest(
            typeof(JustAStructWithEquality),
            (JustAStructWithEquality?)new JustAStructWithEquality { A = 1 },
            new JustAStructWithEquality { A = 2 },
            null,
            true);

        CompareTest(
            typeof(JustAStructWithEqualityOperators),
            (JustAStructWithEqualityOperators?)new JustAStructWithEqualityOperators { A = 1 },
            new JustAStructWithEqualityOperators { A = 2 },
            null,
            true);
    }

    [ConditionalFact]
    public void Structural_objects_get_deep_key_comparer_by_default()
    {
        var comparer = new ValueComparer<byte[]>(false);
        var keyComparer = new ValueComparer<byte[]>(true);

        var equals = comparer.EqualsExpression.Compile();
        var keyEquals = keyComparer.EqualsExpression.Compile();
        var getHashCode = comparer.HashCodeExpression.Compile();
        var getKeyHashCode = keyComparer.HashCodeExpression.Compile();
        var snapshot = comparer.SnapshotExpression.Compile();
        var keySnapshot = keyComparer.SnapshotExpression.Compile();

        var value1a = new byte[] { 1, 2 };
        var value1b = new byte[] { 1, 2 };
        var value2 = new byte[] { 2, 1 };

        Assert.True(equals(value1a, value1a));
        Assert.True(equals(value1a, value1b));
        Assert.False(equals(value1a, value2));

        Assert.True(keyEquals(value1a, value1a));
        Assert.True(keyEquals(value1a, value1b));
        Assert.False(keyEquals(value1a, value2));

        Assert.Equal(value1a.GetHashCode(), getHashCode(value1a));
        Assert.NotEqual(value1a.GetHashCode(), getKeyHashCode(value1a));

        var copy = snapshot(value1a);
        var keyCopy = keySnapshot(value2);

        Assert.Same(value1a, copy);
        Assert.NotSame(value2, keyCopy);
        Assert.Equal(value1a, copy);
        Assert.Equal(value2, keyCopy);
    }

    private class Binary(byte value0, byte value1)
    {
        public byte Value0 { get; } = value0;
        public byte Value1 { get; } = value1;
    }

    [ConditionalFact]
    public void Can_define_different_custom_equals_for_key_and_non_key()
    {
        var comparer = new ValueComparer<Binary>(
            (v1, v2) => v1.Equals(v2),
            v => v.GetHashCode());

        var keyComparer = new ValueComparer<Binary>(
            (v1, v2) => v1.Value0 == v2.Value0 && v1.Value1 == v2.Value1,
            v => v.Value0 << 8 | v.Value1);

        var equals = comparer.EqualsExpression.Compile();
        var keyEquals = keyComparer.EqualsExpression.Compile();
        var getHashCode = comparer.HashCodeExpression.Compile();
        var getKeyHashCode = keyComparer.HashCodeExpression.Compile();

        var value1a = new Binary(1, 2);
        var value1b = new Binary(1, 2);
        var value2 = new Binary(2, 1);

        Assert.True(equals(value1a, value1a));
        Assert.False(equals(value1a, value1b));
        Assert.False(equals(value1a, value2));

        Assert.True(keyEquals(value1a, value1a));
        Assert.True(keyEquals(value1a, value1b));
        Assert.False(keyEquals(value1a, value2));

        Assert.Equal(value1a.GetHashCode(), getHashCode(value1a));
        Assert.Equal(258, getKeyHashCode(value1a));
    }

    private class DeepBinary(byte[] value0, byte[] value1)
    {
        public byte[] Value0 { get; } = value0;
        public byte[] Value1 { get; } = value1;
    }

    private static readonly MethodInfo _getValue0Method
        = typeof(DeepBinary).GetProperty(nameof(DeepBinary.Value0)).GetMethod;

    private static readonly MethodInfo _getValue1Method
        = typeof(DeepBinary).GetProperty(nameof(DeepBinary.Value1)).GetMethod;

    [ConditionalFact]
    public void Can_create_new_comparer_composing_existing_comparers()
    {
        var bytesComparer = new ValueComparer<byte[]>(false);
        var bytesKeyComparer = new ValueComparer<byte[]>(true);

        var comparer = new ValueComparer<DeepBinary>(
            (Expression<Func<DeepBinary, DeepBinary, bool>>)CreateAndExpression(bytesComparer),
            (Expression<Func<DeepBinary, int>>)CreateHashCodeExpression(bytesComparer));

        var keyComparer = new ValueComparer<DeepBinary>(
            (Expression<Func<DeepBinary, DeepBinary, bool>>)CreateAndExpression(bytesKeyComparer),
            (Expression<Func<DeepBinary, int>>)CreateHashCodeExpression(bytesKeyComparer));

        var equals = comparer.EqualsExpression.Compile();
        var keyEquals = keyComparer.EqualsExpression.Compile();
        var getHashCode = comparer.HashCodeExpression.Compile();
        var getKeyHashCode = keyComparer.HashCodeExpression.Compile();

        var array1a = new byte[] { 1, 2 };
        var array1b = new byte[] { 1, 2 };
        var array2 = new byte[] { 2, 1 };

        var value1a = new DeepBinary(array1a, array2);
        var value1b = new DeepBinary(array1a, array2);
        var value1c = new DeepBinary(array1b, array2);
        var value2 = new DeepBinary(array2, array1a);

        Assert.True(equals(value1a, value1a));
        Assert.True(equals(value1a, value1b)); // Underlying array instances the same
        Assert.True(equals(value1a, value1c)); // Underlying array instances different
        Assert.False(keyEquals(value1a, value2)); // Underlying array instances different values

        Assert.True(keyEquals(value1a, value1a));
        Assert.True(keyEquals(value1a, value1b)); // Underlying array instances the same
        Assert.True(keyEquals(value1a, value1c)); // Underlying array instances same values
        Assert.False(keyEquals(value1a, value2)); // Underlying array instances different values

        Assert.Equal(getHashCode(value1b), getHashCode(value1a));
        Assert.Equal(getKeyHashCode(value1b), getKeyHashCode(value1a));

        Assert.NotEqual(getHashCode(value1c), getHashCode(value1a));
        Assert.Equal(getKeyHashCode(value1c), getKeyHashCode(value1a));

        Assert.NotEqual(getHashCode(value2), getHashCode(value1a));
        Assert.NotEqual(getKeyHashCode(value2), getKeyHashCode(value1a));
    }

    private static LambdaExpression CreateAndExpression(ValueComparer comparer)
    {
        var param1 = Expression.Parameter(typeof(DeepBinary), "v1");
        var param2 = Expression.Parameter(typeof(DeepBinary), "v2");

        var firstEquals = comparer.ExtractEqualsBody(
            Expression.Call(param1, _getValue0Method),
            Expression.Call(param2, _getValue0Method));

        var secondEquals = comparer.ExtractEqualsBody(
            Expression.Call(param1, _getValue1Method),
            Expression.Call(param2, _getValue1Method));

        return Expression.Lambda(
            Expression.AndAlso(firstEquals, secondEquals),
            param1, param2);
    }

    private static LambdaExpression CreateHashCodeExpression(ValueComparer comparer)
    {
        var param = Expression.Parameter(typeof(DeepBinary), "v");

        var firstHashCode = comparer.ExtractHashCodeBody(Expression.Call(param, _getValue0Method));
        var secondHashCode = comparer.ExtractHashCodeBody(Expression.Call(param, _getValue1Method));

        return Expression.Lambda(
            Expression.ExclusiveOr(
                Expression.Multiply(
                    firstHashCode,
                    Expression.Constant(397, typeof(int))),
                secondHashCode),
            param);
    }
}
