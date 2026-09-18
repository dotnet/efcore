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
    public void UseUpdatedMemorySafetyRules_returns_false_when_feature_explicitly_disabled()
    {
        var compilation = CreateCompilation(features: [new("updated-memory-safety-rules", "false")]);

        Assert.False(compilation.UseUpdatedMemorySafetyRules());
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

    [Fact]
    public void UseSafeKeyword_defaults_to_the_compiler_support_when_language_version_is_not_set()
        => Assert.Equal(MemorySafetyRules.SafeKeyword != SyntaxKind.None, MemorySafetyRules.UseSafeKeyword(null));

    [Fact]
    public void UseSafeKeyword_respects_lower_language_versions()
    {
        Assert.False(MemorySafetyRules.UseSafeKeyword("14.0"));
        Assert.Equal(MemorySafetyRules.SafeKeyword != SyntaxKind.None, MemorySafetyRules.UseSafeKeyword("14.1"));
        Assert.Equal(MemorySafetyRules.SafeKeyword != SyntaxKind.None, MemorySafetyRules.UseSafeKeyword("latest"));
    }

    [Fact]
    public void UseSafeKeyword_trims_csharp_prefix_and_whitespace()
        => Assert.False(MemorySafetyRules.UseSafeKeyword(" C# 14.0 "));

    [Fact]
    public void UseSafeKeyword_returns_false_for_unrecognized_language_versions()
        => Assert.False(MemorySafetyRules.UseSafeKeyword("not-a-version"));

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
