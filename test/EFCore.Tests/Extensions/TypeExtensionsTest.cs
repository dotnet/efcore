// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable InconsistentNaming

using System.Collections;
using System.Collections.ObjectModel;
namespace Microsoft.EntityFrameworkCore;

public class TypeExtensionsTest
{
    [ConditionalFact]
    public void GetSequenceType_finds_element_type()
    {
        Assert.Equal(typeof(int), typeof(IEnumerable<int>).GetSequenceType());
        Assert.Equal(typeof(int), typeof(IQueryable<int>).GetSequenceType());
        Assert.Equal(typeof(int), typeof(IAsyncEnumerable<int>).GetSequenceType());
        Assert.Equal(typeof(int), typeof(List<int>).GetSequenceType());
    }

    [ConditionalFact]
    public void IsInteger_returns_true_only_for_integer_types()
    {
        Assert.True(typeof(long).IsInteger());
        Assert.True(typeof(int).IsInteger());
        Assert.True(typeof(short).IsInteger());
        Assert.True(typeof(byte).IsInteger());
        Assert.True(typeof(ulong).IsInteger());
        Assert.True(typeof(uint).IsInteger());
        Assert.True(typeof(ushort).IsInteger());
        Assert.True(typeof(sbyte).IsInteger());
        Assert.True(typeof(long?).IsInteger());
        Assert.True(typeof(int?).IsInteger());
        Assert.True(typeof(short?).IsInteger());
        Assert.True(typeof(byte?).IsInteger());
        Assert.True(typeof(long?).IsInteger());
        Assert.True(typeof(int?).IsInteger());
        Assert.True(typeof(short?).IsInteger());
        Assert.True(typeof(byte?).IsInteger());
        Assert.False(typeof(bool).IsInteger());
        Assert.False(typeof(bool?).IsInteger());
        Assert.False(typeof(decimal).IsInteger());
        Assert.False(typeof(float).IsInteger());
        Assert.False(typeof(SomeEnum).IsInteger());
    }

    public class CtorFixture
    {
        public CtorFixture()
        {
        }

        // ReSharper disable once UnusedParameter.Local
        public CtorFixture(int frob)
        {
        }
    }

    [ConditionalFact]
    public void GetDeclaredConstructor_finds_ctor_no_args()
    {
        var constructorInfo = typeof(CtorFixture).GetDeclaredConstructor(null);

        Assert.NotNull(constructorInfo);
        Assert.Empty(constructorInfo.GetParameters());
    }

    [ConditionalFact]
    public void GetDeclaredConstructor_returns_null_when_no_match()
        => Assert.Null(typeof(CtorFixture).GetDeclaredConstructor([typeof(string)]));

    [ConditionalFact]
    public void GetDeclaredConstructor_finds_ctor_args()
    {
        var constructorInfo = typeof(CtorFixture).GetDeclaredConstructor([typeof(int)]);

        Assert.NotNull(constructorInfo);
        Assert.Single(constructorInfo.GetParameters());
    }

    [ConditionalFact]
    public void IsNullableType_when_value_or_nullable_type()
    {
        Assert.True(typeof(string).IsNullableType());
        Assert.False(typeof(int).IsNullableType());
        Assert.False(typeof(Guid).IsNullableType());
        Assert.True(typeof(int?).IsNullableType());
    }

    [ConditionalFact]
    public void Element_type_should_return_input_type_when_not_sequence_type()
        => Assert.Equal(typeof(string), typeof(string));

    [ConditionalFact]
    public void Get_any_property_returns_any_property()
    {
        Assert.Same(typeof(TindersticksII), typeof(TindersticksII).GetAnyProperty("ElDiabloEnElOjo").DeclaringType);
        Assert.Same(typeof(TindersticksII), typeof(TindersticksII).GetAnyProperty("ANightIn").DeclaringType);
        Assert.Same(typeof(TindersticksII), typeof(TindersticksII).GetAnyProperty("MySister").DeclaringType);
        Assert.Same(typeof(TindersticksII), typeof(TindersticksII).GetAnyProperty("TinyTears").DeclaringType);
        Assert.Same(typeof(TindersticksII), typeof(TindersticksII).GetAnyProperty("SnowyInFSharpMinor").DeclaringType);
        Assert.Same(typeof(TindersticksII), typeof(TindersticksII).GetAnyProperty("Seaweed").DeclaringType);
        Assert.Same(typeof(TindersticksII), typeof(TindersticksII).GetAnyProperty("VertrauenII").DeclaringType);
        Assert.Same(typeof(TindersticksII), typeof(TindersticksII).GetAnyProperty("TalkToMe").DeclaringType);
        Assert.Same(typeof(TindersticksII), typeof(TindersticksII).GetAnyProperty("NoMoreAffairs").DeclaringType);
        Assert.Same(typeof(TindersticksII), typeof(TindersticksII).GetAnyProperty("Singing").DeclaringType);
        Assert.Same(typeof(TindersticksII), typeof(TindersticksII).GetAnyProperty("TravellingLight").DeclaringType);
        Assert.Same(typeof(TindersticksII), typeof(TindersticksII).GetAnyProperty("CherryBlossoms").DeclaringType);
        Assert.Same(typeof(TindersticksII), typeof(TindersticksII).GetAnyProperty("ShesGone").DeclaringType);
        Assert.Same(typeof(TindersticksII), typeof(TindersticksII).GetAnyProperty("Mistakes").DeclaringType);
        Assert.Null(typeof(TindersticksII).GetAnyProperty("VertrauenIII"));
        Assert.Same(typeof(TindersticksII), typeof(TindersticksII).GetAnyProperty("SleepySong").DeclaringType);

        Assert.Same(typeof(TindersticksIIVinyl), typeof(TindersticksIIVinyl).GetAnyProperty("ElDiabloEnElOjo").DeclaringType);
        Assert.Same(typeof(TindersticksIIVinyl), typeof(TindersticksIIVinyl).GetAnyProperty("ANightIn").DeclaringType);
        Assert.Same(typeof(TindersticksIIVinyl), typeof(TindersticksIIVinyl).GetAnyProperty("MySister").DeclaringType);
        Assert.Same(typeof(TindersticksIIVinyl), typeof(TindersticksIIVinyl).GetAnyProperty("TinyTears").DeclaringType);
        Assert.Same(typeof(TindersticksIIVinyl), typeof(TindersticksIIVinyl).GetAnyProperty("SnowyInFSharpMinor").DeclaringType);
        Assert.Same(typeof(TindersticksIIVinyl), typeof(TindersticksIIVinyl).GetAnyProperty("Seaweed").DeclaringType);
        Assert.Same(typeof(TindersticksIIVinyl), typeof(TindersticksIIVinyl).GetAnyProperty("VertrauenII").DeclaringType);
        Assert.Same(typeof(TindersticksIIVinyl), typeof(TindersticksIIVinyl).GetAnyProperty("TalkToMe").DeclaringType);
        Assert.Same(typeof(TindersticksIIVinyl), typeof(TindersticksIIVinyl).GetAnyProperty("NoMoreAffairs").DeclaringType);
        Assert.Same(typeof(TindersticksIIVinyl), typeof(TindersticksIIVinyl).GetAnyProperty("Singing").DeclaringType);
        Assert.Same(typeof(TindersticksIIVinyl), typeof(TindersticksIIVinyl).GetAnyProperty("TravellingLight").DeclaringType);
        Assert.Same(typeof(TindersticksIIVinyl), typeof(TindersticksIIVinyl).GetAnyProperty("CherryBlossoms").DeclaringType);
        Assert.Same(typeof(TindersticksIIVinyl), typeof(TindersticksIIVinyl).GetAnyProperty("ShesGone").DeclaringType);
        Assert.Same(typeof(TindersticksII), typeof(TindersticksIIVinyl).GetAnyProperty("Mistakes").DeclaringType);
        Assert.Same(typeof(TindersticksIIVinyl), typeof(TindersticksIIVinyl).GetAnyProperty("VertrauenIII").DeclaringType);
        Assert.Throws<AmbiguousMatchException>(() => typeof(TindersticksIICd).GetAnyProperty("SleepySong"));

        Assert.Same(typeof(TindersticksIICd), typeof(TindersticksIICd).GetAnyProperty("ANightIn").DeclaringType);
        Assert.Same(typeof(TindersticksIICd), typeof(TindersticksIICd).GetAnyProperty("MySister").DeclaringType);
        Assert.Same(typeof(TindersticksIICd), typeof(TindersticksIICd).GetAnyProperty("TinyTears").DeclaringType);
        Assert.Same(typeof(TindersticksIICd), typeof(TindersticksIICd).GetAnyProperty("SnowyInFSharpMinor").DeclaringType);
        Assert.Same(typeof(TindersticksIICd), typeof(TindersticksIICd).GetAnyProperty("Seaweed").DeclaringType);
        Assert.Same(typeof(TindersticksIICd), typeof(TindersticksIICd).GetAnyProperty("VertrauenII").DeclaringType);
        Assert.Same(typeof(TindersticksIICd), typeof(TindersticksIICd).GetAnyProperty("TalkToMe").DeclaringType);
        Assert.Same(typeof(TindersticksIICd), typeof(TindersticksIICd).GetAnyProperty("NoMoreAffairs").DeclaringType);
        Assert.Same(typeof(TindersticksIICd), typeof(TindersticksIICd).GetAnyProperty("Singing").DeclaringType);
        Assert.Same(typeof(TindersticksIICd), typeof(TindersticksIICd).GetAnyProperty("TravellingLight").DeclaringType);
        Assert.Same(typeof(TindersticksIICd), typeof(TindersticksIICd).GetAnyProperty("CherryBlossoms").DeclaringType);
        Assert.Same(typeof(TindersticksIIVinyl), typeof(TindersticksIICd).GetAnyProperty("ShesGone").DeclaringType);
        Assert.Same(typeof(TindersticksIICd), typeof(TindersticksIICd).GetAnyProperty("Mistakes").DeclaringType);
        Assert.Same(typeof(TindersticksIICd), typeof(TindersticksIICd).GetAnyProperty("VertrauenIII").DeclaringType);
        Assert.Throws<AmbiguousMatchException>(() => typeof(TindersticksIICd).GetAnyProperty("SleepySong"));
    }

    public class TindersticksII
    {
        public virtual int ElDiabloEnElOjo { get; set; }
        internal virtual int ANightIn { get; set; }

        // ReSharper disable once UnusedMember.Local
        private int MySister { get; set; }

        protected virtual int TinyTears { get; set; }
        public virtual int SnowyInFSharpMinor { get; }
        public virtual int Seaweed { private get; set; }
        public virtual int VertrauenII { get; protected set; }
        public virtual int TalkToMe { protected get; set; }

        public virtual int NoMoreAffairs
            => 1995;

        public virtual int Singing
        {
            // ReSharper disable once ValueParameterNotUsed
            set { }
        }

        public virtual int TravellingLight { get; set; }
        public int CherryBlossoms { get; set; }
        public int ShesGone { get; set; }
        public virtual int Mistakes { get; set; }
        public int SleepySong { get; set; }
    }

    public class TindersticksIIVinyl : TindersticksII
    {
        public override int ElDiabloEnElOjo { get; set; }
        internal override int ANightIn { get; set; }
        private int MySister { get; set; }
        protected override int TinyTears { get; set; }

        public override int SnowyInFSharpMinor
            => 1995;

        public override int Seaweed
        {
            set { }
        }

        public override int VertrauenII { get; protected set; }
        public override int TalkToMe { protected get; set; }

        public override int NoMoreAffairs
            => 1995;

        public override int Singing
        {
            set { }
        }

        public new virtual int TravellingLight { get; set; }
        public new virtual int CherryBlossoms { get; set; }
        public new int ShesGone { get; set; }
        public virtual int VertrauenIII { get; set; }
        public static new int SleepySong { get; set; }
    }

    public class TindersticksIICd : TindersticksIIVinyl
    {
        internal override int ANightIn { get; set; }
        private int MySister { get; set; }
        protected override int TinyTears { get; set; }

        public override int SnowyInFSharpMinor
            => 1995;

        public override int Seaweed
        {
            set { }
        }

        public override int VertrauenII { get; protected set; }
        public override int TalkToMe { protected get; set; }

        public override int NoMoreAffairs
            => 1995;

        public override int Singing
        {
            set { }
        }

        public override int TravellingLight { get; set; }
        public override int CherryBlossoms { get; set; }
        public override int Mistakes { get; set; }
        public override int VertrauenIII { get; set; }
        public static new int SleepySong { get; set; }
    }

    [ConditionalFact]
    public void TryGetElementType_returns_element_type_for_given_interface()
    {
        Assert.Same(typeof(string), typeof(ICollection<string>).TryGetElementType(typeof(ICollection<>)));
        Assert.Same(typeof(Random), typeof(IObservable<Random>).TryGetElementType(typeof(IObservable<>)));
        Assert.Same(typeof(int), typeof(List<int>).TryGetElementType(typeof(IList<>)));
        Assert.Same(
            typeof(Random), typeof(MultipleImplementor<Random, string>).TryGetElementType(typeof(IObservable<>)));
        Assert.Same(typeof(string), typeof(MultipleImplementor<Random, string>).TryGetElementType(typeof(IEnumerable<>)));
    }

    [ConditionalFact]
    public void TryGetElementType_returns_element_type_for_given_class()
    {
        Assert.Same(typeof(string), typeof(Collection<string>).TryGetElementType(typeof(Collection<>)));
        Assert.Same(typeof(int), typeof(List<int>).TryGetElementType(typeof(List<>)));
    }

    [ConditionalFact]
    public void TryGetElementType_returns_null_if_type_is_generic_type_definition()
        => Assert.Null(typeof(ICollection<>).TryGetElementType(typeof(ICollection<>)));

    [ConditionalFact]
    public void TryGetElementType_returns_null_if_type_doesnt_implement_interface()
    {
        Assert.Null(typeof(ICollection<string>).TryGetElementType(typeof(IObservable<>)));
        Assert.Null(typeof(Random).TryGetElementType(typeof(IObservable<>)));
    }

    [ConditionalFact]
    public void TryGetElementType_returns_null_if_type_doesnt_implement_class()
    {
        Assert.Null(typeof(ICollection<string>).TryGetElementType(typeof(List<>)));
        Assert.Null(typeof(Random).TryGetElementType(typeof(Collection<>)));
    }

    // CodePlex 2014
    [ConditionalFact]
    public void TryGetElementType_returns_null_when_ICollection_implemented_more_than_once()
        => Assert.Null(typeof(RoleCollection2014).TryGetElementType(typeof(ICollection<>)));

    private class MultipleImplementor<TRandom, TElement> : IObservable<TRandom>, IEnumerable<TElement>
        where TRandom : Random
    {
        public IEnumerator<TElement> GetEnumerator()
            => throw new NotImplementedException();

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

        public IDisposable Subscribe(IObserver<TRandom> observer)
            => throw new NotImplementedException();
    }

    private interface IRole2014
    {
        string Permissions { get; set; }
    }

    private interface IRoleCollection2014 : ICollection<IRole2014>;

#pragma warning disable CA1061 // Do not hide base class methods
    private class RoleCollection2014 : List<Role2014>, IRoleCollection2014
    {
        public new IEnumerator<IRole2014> GetEnumerator()
            => throw new NotImplementedException();

        public void Add(IRole2014 item)
            => throw new NotImplementedException();

        public bool Contains(IRole2014 item)
            => throw new NotImplementedException();

        public void CopyTo(IRole2014[] array, int arrayIndex)
            => throw new NotImplementedException();

        public bool Remove(IRole2014 item)
            => throw new NotImplementedException();

        public bool IsReadOnly { get; }
    }
#pragma warning restore CA1061 // Do not hide base class methods

    private class Role2014 : IRole2014
    {
        public int RoleId { get; set; }
        public string Permissions { get; set; }
    }

    [ConditionalFact]
    public void GetBaseTypes_return_all_base_types()
    {
        Assert.Equal(3, typeof(MultipleHierarchy).GetBaseTypes().Count());
        Assert.Contains(typeof(Some), typeof(MultipleHierarchy).GetBaseTypes());
        Assert.Contains(typeof(Base), typeof(MultipleHierarchy).GetBaseTypes());
        Assert.Contains(typeof(object), typeof(MultipleHierarchy).GetBaseTypes());
    }

    [ConditionalFact]
    public void GetBaseTypes_return_empty_if_no_base_type_exists()
        => Assert.False(typeof(object).GetBaseTypes().Any());

    private class MultipleHierarchy : Some;

    private class Some : Base;

    private class Base;

    // ReSharper restore InconsistentNaming

    [ConditionalFact]
    public void Can_get_default_value_for_type()
    {
        Assert.False((bool)typeof(bool).GetDefaultValue());
        Assert.Equal((sbyte)0, typeof(sbyte).GetDefaultValue());
        Assert.Equal((short)0, typeof(short).GetDefaultValue());
        Assert.Equal(0, typeof(int).GetDefaultValue());
        Assert.Equal((long)0, typeof(long).GetDefaultValue());
        Assert.Equal((byte)0, typeof(byte).GetDefaultValue());
        Assert.Equal((ushort)0, typeof(ushort).GetDefaultValue());
        Assert.Equal((uint)0, typeof(uint).GetDefaultValue());
        Assert.Equal((ulong)0, typeof(ulong).GetDefaultValue());
        Assert.Equal((float)0.0, typeof(float).GetDefaultValue());
        Assert.Equal(0.0, typeof(double).GetDefaultValue());
        Assert.Equal((char)0, typeof(char).GetDefaultValue());
#pragma warning disable IDE0034 // Simplify 'default' expression - GetDefaultValue returns object causing inference of default(object)
        Assert.Equal(default(Guid), typeof(Guid).GetDefaultValue());
        Assert.Equal(default(DateTime), typeof(DateTime).GetDefaultValue());
        Assert.Equal(default(DateTimeOffset), typeof(DateTimeOffset).GetDefaultValue());
        Assert.Equal(default(SomeStruct), typeof(SomeStruct).GetDefaultValue());
        Assert.Equal(default(SomeEnum), typeof(SomeEnum).GetDefaultValue());
#pragma warning restore IDE0034 // Simplify 'default' expression
        Assert.Null(typeof(string).GetDefaultValue());
        Assert.Null(typeof(bool?).GetDefaultValue());
        Assert.Null(typeof(sbyte?).GetDefaultValue());
        Assert.Null(typeof(short?).GetDefaultValue());
        Assert.Null(typeof(int?).GetDefaultValue());
        Assert.Null(typeof(long?).GetDefaultValue());
        Assert.Null(typeof(byte?).GetDefaultValue());
        Assert.Null(typeof(ushort?).GetDefaultValue());
        Assert.Null(typeof(uint?).GetDefaultValue());
        Assert.Null(typeof(ulong?).GetDefaultValue());
        Assert.Null(typeof(float?).GetDefaultValue());
        Assert.Null(typeof(double?).GetDefaultValue());
        Assert.Null(typeof(char?).GetDefaultValue());
        Assert.Null(typeof(Guid?).GetDefaultValue());
        Assert.Null(typeof(DateTime?).GetDefaultValue());
        Assert.Null(typeof(DateTimeOffset?).GetDefaultValue());
        Assert.Null(typeof(SomeStruct?).GetDefaultValue());
        Assert.Null(typeof(SomeEnum?).GetDefaultValue());
    }

    private struct SomeStruct
    {
        public int Value1 { get; set; }
        public long Value2 { get; set; }
    }

    private enum SomeEnum
    {
        Default
    }

    [ConditionalFact]
    public void GetConstructibleTypes_works()
    {
        var assembly = MockAssembly.Create(
            typeof(SomeAbstractClass),
            typeof(SomeGenericClass<>),
            typeof(SomeGenericClass<int>),
            typeof(SomeTypeWithoutDefaultCtor));

        var types = assembly.GetConstructibleTypes().Select(t => t.AsType()).ToList();

        Assert.DoesNotContain(typeof(SomeAbstractClass), types);
        Assert.DoesNotContain(typeof(SomeGenericClass<>), types);
        Assert.Contains(typeof(SomeGenericClass<int>), types);
        Assert.Contains(typeof(SomeTypeWithoutDefaultCtor), types);
    }

    private abstract class SomeAbstractClass;

    private class SomeGenericClass<T>;

#pragma warning disable CS9113 // Parameter 'value' is unread
    private class SomeTypeWithoutDefaultCtor(int value);
#pragma warning restore CS9113

    [ConditionalFact]
    public void GetNamespaces_works()
    {
        // Predefined Types
        Assert.Empty(typeof(int).GetNamespaces().ToArray());
        Assert.Equal(new[] { "System" }, typeof(Guid).GetNamespaces().ToArray());
        Assert.Equal(new[] { "System.Collections.Generic", "System" }, typeof(List<Guid>).GetNamespaces().ToArray());

        Assert.Equal(new[] { "Microsoft.EntityFrameworkCore" }, typeof(A).GetNamespaces().ToArray());
        Assert.Equal(
            new[] { "System.Collections.Generic", "Microsoft.EntityFrameworkCore" },
            typeof(List<A>).GetNamespaces().ToArray());
        Assert.Equal(
            new[] { "System.Collections.Generic", "System", "System.Collections.Generic", "Microsoft.EntityFrameworkCore" },
            typeof(Dictionary<Version, List<A>>).GetNamespaces().ToArray());

        Assert.Equal(new[] { "Microsoft.EntityFrameworkCore", "System" }, typeof(Outer<Guid>).GetNamespaces().ToArray());
        Assert.Equal(
            new[] { "Microsoft.EntityFrameworkCore", "System.Collections.Generic", "System" },
            typeof(Outer<List<Guid>>).GetNamespaces().ToArray());
    }

    [ConditionalTheory]
    // Predefined Types
    [InlineData(typeof(int), "int")]
    [InlineData(typeof(List<int>), "System.Collections.Generic.List<int>")]
    [InlineData(typeof(Dictionary<int, string>), "System.Collections.Generic.Dictionary<int, string>")]
    [InlineData(
        typeof(Dictionary<int, List<string>>), "System.Collections.Generic.Dictionary<int, System.Collections.Generic.List<string>>")]
    [InlineData(typeof(List<List<string>>), "System.Collections.Generic.List<System.Collections.Generic.List<string>>")]
    // Classes inside NonGeneric class
    [InlineData(
        typeof(A),
        "Microsoft.EntityFrameworkCore.TypeExtensionsTest+A")]
    [InlineData(
        typeof(B<int>),
        "Microsoft.EntityFrameworkCore.TypeExtensionsTest+B<int>")]
    [InlineData(
        typeof(C<int, string>),
        "Microsoft.EntityFrameworkCore.TypeExtensionsTest+C<int, string>")]
    [InlineData(
        typeof(B<B<string>>),
        "Microsoft.EntityFrameworkCore.TypeExtensionsTest+B<Microsoft.EntityFrameworkCore.TypeExtensionsTest+B<string>>")]
    [InlineData(
        typeof(C<int, B<string>>),
        "Microsoft.EntityFrameworkCore.TypeExtensionsTest+C<int, Microsoft.EntityFrameworkCore.TypeExtensionsTest+B<string>>")]
    // Classes inside Generic class
    [InlineData(
        typeof(Outer<int>.D),
        "Microsoft.EntityFrameworkCore.TypeExtensionsTest+Outer<int>+D")]
    [InlineData(
        typeof(Outer<int>.E<int>),
        "Microsoft.EntityFrameworkCore.TypeExtensionsTest+Outer<int>+E<int>")]
    [InlineData(
        typeof(Outer<int>.F<int, string>),
        "Microsoft.EntityFrameworkCore.TypeExtensionsTest+Outer<int>+F<int, string>")]
    [InlineData(
        typeof(Level1<int>.Level2<bool>.Level3<int>),
        "Microsoft.EntityFrameworkCore.TypeExtensionsTest+Level1<int>+Level2<bool>+Level3<int>")]
    [InlineData(
        typeof(Outer<int>.E<Outer<int>.E<string>>),
        "Microsoft.EntityFrameworkCore.TypeExtensionsTest+Outer<int>+E<Microsoft.EntityFrameworkCore.TypeExtensionsTest+Outer<int>+E<string>>")]
    [InlineData(
        typeof(Outer<int>.F<int, Outer<int>.E<string>>),
        "Microsoft.EntityFrameworkCore.TypeExtensionsTest+Outer<int>+F<int, Microsoft.EntityFrameworkCore.TypeExtensionsTest+Outer<int>+E<string>>")]
    [InlineData(
        typeof(OuterGeneric<int>.InnerNonGeneric.InnerGeneric<int, string>.InnerGenericLeafNode<bool>),
        "Microsoft.EntityFrameworkCore.TypeExtensionsTest+OuterGeneric<int>+InnerNonGeneric+InnerGeneric<int, string>+InnerGenericLeafNode<bool>")]
    public void Can_pretty_print_CLR_full_name(Type type, string expected)
        => Assert.Equal(expected, type.DisplayName());

    [ConditionalTheory]
    // Predefined Types
    [InlineData(typeof(int), "int")]
    [InlineData(typeof(List<int>), "List<int>")]
    [InlineData(typeof(Dictionary<int, string>), "Dictionary<int, string>")]
    [InlineData(typeof(Dictionary<int, List<string>>), "Dictionary<int, List<string>>")]
    [InlineData(typeof(List<List<string>>), "List<List<string>>")]
    // Classes inside NonGeneric class
    [InlineData(typeof(A), "A")]
    [InlineData(typeof(B<int>), "B<int>")]
    [InlineData(typeof(C<int, string>), "C<int, string>")]
    [InlineData(typeof(C<int, B<string>>), "C<int, B<string>>")]
    [InlineData(typeof(B<B<string>>), "B<B<string>>")]
    // Classes inside Generic class
    [InlineData(typeof(Outer<int>.D), "D")]
    [InlineData(typeof(Outer<int>.E<int>), "E<int>")]
    [InlineData(typeof(Outer<int>.F<int, string>), "F<int, string>")]
    [InlineData(typeof(Outer<int>.F<int, Outer<int>.E<string>>), "F<int, E<string>>")]
    [InlineData(typeof(Outer<int>.E<Outer<int>.E<string>>), "E<E<string>>")]
    [InlineData(
        typeof(OuterGeneric<int>.InnerNonGeneric.InnerGeneric<int, string>.InnerGenericLeafNode<bool>), "InnerGenericLeafNode<bool>")]
    public void Can_pretty_print_CLR_name(Type type, string expected)
        => Assert.Equal(expected, type.ShortDisplayName());

    [ConditionalTheory]
    [InlineData(typeof(bool), "bool")]
    [InlineData(typeof(byte), "byte")]
    [InlineData(typeof(char), "char")]
    [InlineData(typeof(decimal), "decimal")]
    [InlineData(typeof(double), "double")]
    [InlineData(typeof(float), "float")]
    [InlineData(typeof(int), "int")]
    [InlineData(typeof(long), "long")]
    [InlineData(typeof(object), "object")]
    [InlineData(typeof(sbyte), "sbyte")]
    [InlineData(typeof(short), "short")]
    [InlineData(typeof(string), "string")]
    [InlineData(typeof(uint), "uint")]
    [InlineData(typeof(ulong), "ulong")]
    [InlineData(typeof(ushort), "ushort")]
    [InlineData(typeof(void), "void")]
    public void Returns_common_name_for_built_in_types(Type type, string expected)
        => Assert.Equal(expected, type.DisplayName());

    [ConditionalTheory]
    [InlineData(typeof(int[]), true, "int[]")]
    [InlineData(typeof(string[][]), true, "string[][]")]
    [InlineData(typeof(int[,]), true, "int[,]")]
    [InlineData(typeof(bool[,,,]), true, "bool[,,,]")]
    [InlineData(typeof(A[,][,,]), true, "Microsoft.EntityFrameworkCore.TypeExtensionsTest+A[,][,,]")]
    [InlineData(typeof(List<int[,][,,]>), true, "System.Collections.Generic.List<int[,][,,]>")]
    [InlineData(typeof(List<int[,,][,]>[,][,,]), false, "List<int[,,][,]>[,][,,]")]
    public void Can_pretty_print_array_name(Type type, bool fullName, string expected)
        => Assert.Equal(expected, type.DisplayName(fullName));

    public static TheoryData OpenGenericsTestData { get; } = CreateOpenGenericsTestData();

    public static TheoryData CreateOpenGenericsTestData()
    {
        var openDictionaryType = typeof(Dictionary<,>);
        var genArgsDictionary = openDictionaryType.GetGenericArguments();
        genArgsDictionary[0] = typeof(B<>);
        var closedDictionaryType = openDictionaryType.MakeGenericType(genArgsDictionary);
        var openLevelType = typeof(Level1<>.Level2<>.Level3<>);
        var genArgsLevel = openLevelType.GetGenericArguments();
        genArgsLevel[1] = typeof(string);
        var closedLevelType = openLevelType.MakeGenericType(genArgsLevel);
        var openInnerType = typeof(OuterGeneric<>.InnerNonGeneric.InnerGeneric<,>.InnerGenericLeafNode<>);
        var genArgsInnerType = openInnerType.GetGenericArguments();
        genArgsInnerType[3] = typeof(bool);
        var closedInnerType = openInnerType.MakeGenericType(genArgsInnerType);
        return new TheoryData<Type, bool, string>
        {
            { typeof(List<>), false, "List<>" },
            { typeof(Dictionary<,>), false, "Dictionary<,>" },
            { typeof(List<>), true, "System.Collections.Generic.List<>" },
            { typeof(Dictionary<,>), true, "System.Collections.Generic.Dictionary<,>" },
            {
                typeof(Level1<>.Level2<>.Level3<>), true,
                "Microsoft.EntityFrameworkCore.TypeExtensionsTest+Level1<>+Level2<>+Level3<>"
            },
            { typeof(PartiallyClosedGeneric<>).BaseType, true, "Microsoft.EntityFrameworkCore.TypeExtensionsTest+C<, int>" },
            {
                typeof(OuterGeneric<>.InnerNonGeneric.InnerGeneric<,>.InnerGenericLeafNode<>), true,
                "Microsoft.EntityFrameworkCore.TypeExtensionsTest+OuterGeneric<>+InnerNonGeneric+InnerGeneric<,>+InnerGenericLeafNode<>"
            },
            {
                closedDictionaryType, true,
                "System.Collections.Generic.Dictionary<Microsoft.EntityFrameworkCore.TypeExtensionsTest+B<>,>"
            },
            { closedLevelType, true, "Microsoft.EntityFrameworkCore.TypeExtensionsTest+Level1<>+Level2<string>+Level3<>" },
            {
                closedInnerType, true,
                "Microsoft.EntityFrameworkCore.TypeExtensionsTest+OuterGeneric<>+InnerNonGeneric+InnerGeneric<,>+InnerGenericLeafNode<bool>"
            }
        };
    }

    [ConditionalFact]
    public void Can_pretty_print_open_generics()
    {
        foreach (var testData in OpenGenericsTestData)
        {
            var type = (Type)testData[0];
            var fullName = (bool)testData[1];
            var expected = (string)testData[2];

            Assert.Equal(expected, type.DisplayName(fullName));
        }
    }

    private class A;

    private class B<T>;

    private class C<T1, T2>;

    private class PartiallyClosedGeneric<T> : C<T, int>;

    private static class Outer<T>
    {
        public class D;

        public class E<T1>;

        public class F<T1, T2>;
    }

    private static class OuterGeneric<T1>
    {
        public static class InnerNonGeneric
        {
            public static class InnerGeneric<T2, T3>
            {
                public class InnerGenericLeafNode<T4>;

                public class InnerLeafNode;
            }
        }
    }

    private static class Level1<T1>
    {
        public static class Level2<T2>
        {
            public class Level3<T3>;
        }
    }
}
