// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable InconsistentNaming

namespace Microsoft.EntityFrameworkCore.Utilities;

public class CheckTest
{
    [ConditionalTheory]
    [InlineData(null)]
    public void Not_null_throws_when_arg_is_null(string arg1)
        => Assert.Equal(nameof(arg1), Assert.Throws<ArgumentNullException>(() => Check.NotNull(arg1)).ParamName);

    [ConditionalTheory]
    [InlineData("")]
    [InlineData(" ")]
    public void Not_empty_throws_when_arg_is_empty(string arg1)
        => Assert.Equal(nameof(arg1), Assert.Throws<ArgumentException>(() => Check.NotEmpty(arg1)).ParamName);

    [ConditionalTheory]
    [InlineData(null)]
    public void Generic_Not_empty_throws_when_arg_is_null(string[] arg1)
        => Assert.Equal(nameof(arg1), Assert.Throws<ArgumentNullException>(() => Check.NotEmpty(arg1)).ParamName);

    [ConditionalTheory]
    [InlineData(new object[] { new string[0] })]
    public void Generic_Not_empty_throws_when_arg_is_empty(string[] arg1)
        => Assert.Equal(nameof(arg1), Assert.Throws<ArgumentException>(() => Check.NotEmpty(arg1)).ParamName);

    [ConditionalTheory]
    [InlineData("")]
    public void Not_but_not_empty_throws_when_arg_is_empty(string arg1)
        => Assert.Equal(nameof(arg1), Assert.Throws<ArgumentException>(() => Check.NullButNotEmpty(arg1)).ParamName);

    [ConditionalTheory]
    [InlineData(new object[] { new string[] { null } })]
    public void Has_no_nulls_throws_when_arg_has_nulls(string[] arg1)
        => Assert.Equal(nameof(arg1), Assert.Throws<ArgumentException>(() => Check.HasNoNulls(arg1)).ParamName);

    [ConditionalTheory]
    [InlineData(new object[] { new string[] { null } })]
    [InlineData(new object[] { new string[] { "" } })]
    [InlineData(new object[] { new string[] { " " } })]
    public void Has_no_empty_elements_throws_when_arg_has_empty_elements(string[] arg1)
        => Assert.Equal(nameof(arg1), Assert.Throws<ArgumentException>(() => Check.HasNoEmptyElements(arg1)).ParamName);
}
