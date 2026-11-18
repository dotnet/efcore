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

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("default")]
    [InlineData("latest")]
    [InlineData("latestmajor")]
    [InlineData(" LATEST ")]
    [InlineData("14")]
    [InlineData("14.0")]
    [InlineData("14.1")]
    [InlineData("15")]
    [InlineData("15.0")]
    [InlineData(" 15.0 ")]
    public void UseSafeKeyword_returns_false_for_non_preview_language_versions(string? langVersion)
        => Assert.False(MemorySafetyRules.UseSafeKeyword(langVersion));

    [Theory]
    [InlineData("preview")]
    [InlineData("Preview")]
    [InlineData(" preview ")]
    public void UseSafeKeyword_requires_compiler_support_for_preview(string langVersion)
        => Assert.Equal(MemorySafetyRules.SafeKeyword != SyntaxKind.None, MemorySafetyRules.UseSafeKeyword(langVersion));

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
