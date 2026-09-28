// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable InconsistentNaming

namespace Microsoft.EntityFrameworkCore;

public class SharedTypeExtensionsTest
{
    [Fact]
    public void UnwrapNullableType_returns_underlying_type_for_nullable_value_type()
    {
        Assert.Equal(typeof(int), typeof(int?).UnwrapNullableType());
        Assert.Equal(typeof(SomeEnum), typeof(SomeEnum?).UnwrapNullableType());
    }

    [Fact]
    public void UnwrapNullableType_returns_input_for_non_nullable_types()
    {
        Assert.Equal(typeof(int), typeof(int).UnwrapNullableType());
        Assert.Equal(typeof(string), typeof(string).UnwrapNullableType());
    }

    [Fact]
    public void IsNullableValueType_is_true_only_for_nullable_value_types()
    {
        Assert.True(typeof(int?).IsNullableValueType());
        Assert.True(typeof(SomeEnum?).IsNullableValueType());
        Assert.False(typeof(int).IsNullableValueType());
        Assert.False(typeof(string).IsNullableValueType());
        Assert.False(typeof(object).IsNullableValueType());
    }

    [Fact]
    public void MakeNullable_wraps_value_types_and_is_idempotent()
    {
        Assert.Equal(typeof(int?), typeof(int).MakeNullable());
        Assert.Equal(typeof(int?), typeof(int?).MakeNullable());
        // Reference types are already nullable, so they are returned unchanged.
        Assert.Equal(typeof(string), typeof(string).MakeNullable());
    }

    [Fact]
    public void MakeNullable_with_false_unwraps_nullable_value_types()
    {
        Assert.Equal(typeof(int), typeof(int?).MakeNullable(nullable: false));
        Assert.Equal(typeof(int), typeof(int).MakeNullable(nullable: false));
        Assert.Equal(typeof(string), typeof(string).MakeNullable(nullable: false));
    }

    [Fact]
    public void IsNumeric_covers_integers_and_floating_point_and_decimal_including_nullable()
    {
        Assert.True(typeof(int).IsNumeric());
        Assert.True(typeof(int?).IsNumeric());
        Assert.True(typeof(decimal).IsNumeric());
        Assert.True(typeof(double).IsNumeric());
        Assert.True(typeof(float).IsNumeric());
        Assert.True(typeof(byte).IsNumeric());

        Assert.False(typeof(bool).IsNumeric());
        Assert.False(typeof(string).IsNumeric());
        Assert.False(typeof(DateTime).IsNumeric());
    }

    [Fact]
    public void IsSignedInteger_is_true_only_for_signed_integral_types()
    {
        Assert.True(typeof(int).IsSignedInteger());
        Assert.True(typeof(long).IsSignedInteger());
        Assert.True(typeof(short).IsSignedInteger());
        Assert.True(typeof(sbyte).IsSignedInteger());

        Assert.False(typeof(uint).IsSignedInteger());
        Assert.False(typeof(byte).IsSignedInteger());
        Assert.False(typeof(ulong).IsSignedInteger());
        Assert.False(typeof(ushort).IsSignedInteger());
        Assert.False(typeof(char).IsSignedInteger());
    }

    [Fact]
    public void IsSignedInteger_does_not_unwrap_nullable_types()
        // Unlike IsInteger, IsSignedInteger does not unwrap Nullable<T>; callers pass non-nullable CLR types.
        => Assert.False(typeof(int?).IsSignedInteger());

    [Fact]
    public void UnwrapEnumType_returns_underlying_type_preserving_nullability()
    {
        Assert.Equal(typeof(int), typeof(SomeEnum).UnwrapEnumType());
        Assert.Equal(typeof(int?), typeof(SomeEnum?).UnwrapEnumType());
        Assert.Equal(typeof(byte), typeof(ByteEnum).UnwrapEnumType());
    }

    [Fact]
    public void UnwrapEnumType_returns_input_for_non_enum_types()
    {
        Assert.Equal(typeof(int), typeof(int).UnwrapEnumType());
        Assert.Equal(typeof(string), typeof(string).UnwrapEnumType());
    }

    [Fact]
    public void IsScalarType_is_true_for_string_and_common_primitive_types()
    {
        Assert.True(typeof(string).IsScalarType());
        Assert.True(typeof(int).IsScalarType());
        Assert.True(typeof(Guid).IsScalarType());
        Assert.True(typeof(DateTime).IsScalarType());
        Assert.True(typeof(decimal).IsScalarType());

        Assert.False(typeof(SomeClass).IsScalarType());
        // The common-type set contains only non-nullable value types.
        Assert.False(typeof(int?).IsScalarType());
    }

    [Fact]
    public void IsInstantiable_is_false_for_abstract_interface_and_open_generic_types()
    {
        Assert.True(typeof(SomeClass).IsInstantiable());
        Assert.True(typeof(List<int>).IsInstantiable());

        Assert.False(typeof(SomeAbstractClass).IsInstantiable());
        Assert.False(typeof(ISomeInterface).IsInstantiable());
        Assert.False(typeof(List<>).IsInstantiable());
    }

    [Fact]
    public void IsValidEntityType_is_true_only_for_non_string_non_array_classes()
    {
        Assert.True(typeof(SomeClass).IsValidEntityType());

        Assert.False(typeof(string).IsValidEntityType());
        Assert.False(typeof(int).IsValidEntityType());
        Assert.False(typeof(SomeStruct).IsValidEntityType());
        Assert.False(typeof(int[]).IsValidEntityType());
        Assert.False(typeof(ISomeInterface).IsValidEntityType());
    }

    [Fact]
    public void IsCompatibleWith_is_true_when_either_type_is_assignable_to_the_other()
    {
        Assert.True(typeof(SomeClass).IsCompatibleWith(typeof(SomeClass)));
        Assert.True(typeof(object).IsCompatibleWith(typeof(string)));
        Assert.True(typeof(string).IsCompatibleWith(typeof(object)));
        Assert.True(typeof(IEnumerable<int>).IsCompatibleWith(typeof(List<int>)));
    }

    [Fact]
    public void IsCompatibleWith_recurses_into_sequence_element_types()
        // Neither List<object> nor List<string> is assignable to the other, but their element types are compatible.
        => Assert.True(typeof(List<object>).IsCompatibleWith(typeof(List<string>)));

    [Fact]
    public void IsCompatibleWith_is_false_for_unrelated_types()
    {
        Assert.False(typeof(int).IsCompatibleWith(typeof(string)));
        Assert.False(typeof(List<int>).IsCompatibleWith(typeof(List<string>)));
    }

    [Fact]
    public void GetTypesInHierarchy_yields_the_type_and_all_base_types_including_object()
    {
        var hierarchy = typeof(Derived).GetTypesInHierarchy().ToList();

        Assert.Equal([typeof(Derived), typeof(SomeBase), typeof(object)], hierarchy);
    }

    [Fact]
    public void GetBaseTypesAndInterfacesInclusive_includes_the_type_its_bases_and_interfaces()
    {
        var types = typeof(Derived).GetBaseTypesAndInterfacesInclusive();

        Assert.Contains(typeof(Derived), types);
        Assert.Contains(typeof(SomeBase), types);
        Assert.Contains(typeof(object), types);
        Assert.Contains(typeof(ISomeInterface), types);
    }

    private enum SomeEnum
    {
        Default
    }

    private enum ByteEnum : byte
    {
        Default
    }

    private class SomeClass;

    private struct SomeStruct;

    private abstract class SomeAbstractClass;

    private interface ISomeInterface;

    private class SomeBase;

    private class Derived : SomeBase, ISomeInterface;
}
