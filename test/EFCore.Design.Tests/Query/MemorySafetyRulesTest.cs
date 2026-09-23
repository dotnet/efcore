// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Microsoft.EntityFrameworkCore.Design.Internal;

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
    public void UseSafeKeyword_defaults_to_true_when_language_version_is_not_set()
        => Assert.Equal(MemorySafetyRules.SafeKeyword != SyntaxKind.None, MemorySafetyRules.UseSafeKeyword(null));

    [Fact]
    public void UseSafeKeyword_respects_lower_language_versions()
    {
        Assert.False(MemorySafetyRules.UseSafeKeyword("14.0"));
        Assert.False(MemorySafetyRules.UseSafeKeyword("14.1"));
        Assert.Equal(MemorySafetyRules.SafeKeyword != SyntaxKind.None, MemorySafetyRules.UseSafeKeyword("15.0"));
        Assert.Equal(MemorySafetyRules.SafeKeyword != SyntaxKind.None, MemorySafetyRules.UseSafeKeyword("latest"));
        Assert.Equal(MemorySafetyRules.SafeKeyword != SyntaxKind.None, MemorySafetyRules.UseSafeKeyword("default"));
        Assert.Equal(MemorySafetyRules.SafeKeyword != SyntaxKind.None, MemorySafetyRules.UseSafeKeyword("latestmajor"));
    }

    [Fact]
    public void UseSafeKeyword_trims_whitespace()
        => Assert.Equal(MemorySafetyRules.SafeKeyword != SyntaxKind.None, MemorySafetyRules.UseSafeKeyword(" 15.0 "));

    [Fact]
    public void UseSafeKeyword_throws_for_unsupported_language_versions()
    {
        var ex = Assert.Throws<ArgumentException>(() => MemorySafetyRules.UseSafeKeyword("CSharp 14.0"));
        Assert.Equal("langVersion", ex.ParamName);
        Assert.Contains("not supported", ex.Message);

        ex = Assert.Throws<ArgumentException>(() => MemorySafetyRules.UseSafeKeyword("C# 14.1"));
        Assert.Equal("langVersion", ex.ParamName);
        Assert.Contains("not supported", ex.Message);

        ex = Assert.Throws<ArgumentException>(() => MemorySafetyRules.UseSafeKeyword("latestminor"));
        Assert.Equal("langVersion", ex.ParamName);
        Assert.Contains("not supported", ex.Message);

        ex = Assert.Throws<ArgumentException>(() => MemorySafetyRules.UseSafeKeyword("not-a-version"));
        Assert.Equal("langVersion", ex.ParamName);
        Assert.Contains("not supported", ex.Message);
    }

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
