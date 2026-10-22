// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Design.Internal;

namespace Microsoft.EntityFrameworkCore.Scaffolding.Internal;

public class CSharpNamerTest
{
    [ConditionalTheory]
    [InlineData("Name with space", "Name_with_space")]
    [InlineData("namespace", "_namespace")]
    [InlineData("@namespace", "@namespace")]
    [InlineData("8ball", "_8ball")]
    [InlineData(" ", "_")]
    [InlineData("", "_")]
    public void Sanitizes_name_with_no_singularize_or_pluralize(string input, string output)
        => Assert.Equal(output, new CSharpNamer<string>(s => s, new CSharpUtilities(), null).GetName(input));

    [ConditionalTheory]
    [InlineData("Name ending with s", "Name_ending_with_")]
    [InlineData("Name with no s at end", "Name_with_no_s_at_end")]
    public void Sanitizes_name_with_singularizer(string input, string output)
    {
        var pluralizer = new HumanizerPluralizer();
        Assert.Equal(output, new CSharpNamer<string>(s => s, new CSharpUtilities(), pluralizer.Singularize).GetName(input));
    }

    [ConditionalTheory]
    [InlineData("Name ending with s", "Name_ending_with_s")]
    [InlineData("Name with no s at end", "Name_with_no_s_at_ends")]
    public void Sanitizes_name_with_pluralizer(string input, string output)
    {
        var pluralizer = new HumanizerPluralizer();
        Assert.Equal(output, new CSharpNamer<string>(s => s, new CSharpUtilities(), pluralizer.Pluralize).GetName(input));
    }
}
