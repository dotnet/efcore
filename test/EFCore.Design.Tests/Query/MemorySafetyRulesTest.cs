// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.EntityFrameworkCore.Design.Internal;

namespace Microsoft.EntityFrameworkCore.Query;

public class MemorySafetyRulesTest
{
    [Fact]
    public void UseUpdatedMemorySafetyRules_returns_false_when_feature_not_set()
    {
        var compilation = CreateCompilation(features: null);

        Assert.False(compilation.UseUpdatedMemorySafetyRules());
    }

    [Fact]
    public void UseUpdatedMemorySafetyRules_returns_true_when_feature_set_via_parse_options()
    {
        var compilation = CreateCompilation(features: [new("updated-memory-safety-rules", "true")]);

        Assert.True(compilation.UseUpdatedMemorySafetyRules());
    }

    [Fact]
    public void UseUpdatedMemorySafetyRules_returns_true_when_feature_set_on_any_syntax_tree()
    {
        var parseOptions = CSharpParseOptions.Default.WithFeatures([new KeyValuePair<string, string>("updated-memory-safety-rules", "true")]);
        var compilation = CSharpCompilation.Create(
            "Test",
            [
                CSharpSyntaxTree.ParseText("class First { }", parseOptions),
                CSharpSyntaxTree.ParseText("class Second { }", parseOptions)
            ]);

        Assert.True(compilation.UseUpdatedMemorySafetyRules());
    }

    [Fact]
    public void UseUpdatedMemorySafetyRules_ignores_unrelated_features()
    {
        var compilation = CreateCompilation(features: [new("some-other-feature", "true")]);

        Assert.False(compilation.UseUpdatedMemorySafetyRules());
    }

    [Fact]
    public void SafeKeyword_does_not_throw_and_is_stable()
        => Assert.Equal(MemorySafetyRules.SafeKeyword, MemorySafetyRules.SafeKeyword);

    private static CSharpCompilation CreateCompilation(IEnumerable<KeyValuePair<string, string>>? features)
    {
        var parseOptions = CSharpParseOptions.Default;
        if (features is not null)
        {
            parseOptions = parseOptions.WithFeatures(features);
        }

        var syntaxTree = CSharpSyntaxTree.ParseText("class C { }", parseOptions);

        return CSharpCompilation.Create("Test", [syntaxTree]);
    }
}
