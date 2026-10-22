// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable InconsistentNaming

namespace Microsoft.EntityFrameworkCore;

public class EnumerableExtensionsTest
{
    [ConditionalFact]
    public void Order_by_ordinal_should_respect_case()
        => Assert.Equal(new[] { "A", "a", "b" }, new[] { "b", "A", "a" }.OrderByOrdinal(s => s));

    [ConditionalFact]
    public void Join_empty_input_returns_empty_string()
        => Assert.Equal("", Array.Empty<object>().Join());

    [ConditionalFact]
    public void Join_single_element_does_not_use_separator()
        => Assert.Equal("42", new object[] { 42 }.Join());

    [ConditionalFact]
    public void Join_should_use_comma_by_default()
        => Assert.Equal("42, bar", new object[] { 42, "bar" }.Join());

    [ConditionalFact]
    public void Join_should_use_explicit_separator_when_provided()
        => Assert.Equal("42-bar", new object[] { 42, "bar" }.Join("-"));

    [ConditionalFact]
    public void Structural_sequence_equal_uses_structural_comparison_for_elements_()
    {
        var value1A = new byte[] { 1, 2, 3, 4 };
        var value1B = new byte[] { 1, 2, 3, 4 };
        var value2A = new byte[] { 2, 1, 3, 4 };
        var value2B = new byte[] { 2, 1, 3, 4 };

        Assert.True(new[] { value1A, value2A }.StructuralSequenceEqual(new[] { value1A, value2A }));
        Assert.True(new[] { value1A, value2A }.StructuralSequenceEqual(new[] { value1B, value2B }));

        Assert.False(new[] { value1A, value2A }.StructuralSequenceEqual(new[] { value1B }));
        Assert.False(new[] { value1A }.StructuralSequenceEqual(new[] { value1B, value2B }));
        Assert.False(new[] { value1A, value2A }.StructuralSequenceEqual(new[] { value2A, value2B }));

        var singleReference = new[] { value1A, value2A };
        Assert.True(singleReference.StructuralSequenceEqual(singleReference));
    }
}
