// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.EntityFrameworkCore.Internal;

namespace Microsoft.EntityFrameworkCore.Design.Internal;

internal static class MemorySafetyRules
{
    // This is the feature flag name recognized by the C# parser (passed via '/features:updated-memory-safety-rules'
    // or the equivalent LangVersion/parse options). Roslyn does not yet expose a stable, non-experimental API for
    // querying whether a compilation was parsed with the updated memory safety rules enabled
    // (see https://github.com/dotnet/roslyn/issues/82789), so this flag is also checked even when the
    // (currently experimental) CSharpCompilationOptions.MemorySafetyRulesVersion API looked up reflectively below
    // is available: Roslyn treats the feature as an opt-in fallback when that option is Version1, so a
    // compilation carrying the feature can still use the updated rules.
    private const string UpdatedMemorySafetyRulesFeature = "updated-memory-safety-rules";

    // SyntaxKind.SafeKeyword is still experimental (RSEXPERIMENTAL006) and may not exist on the
    // Microsoft.CodeAnalysis.CSharp version this generator was built against, so its value is looked up as a
    // contextual keyword instead of being referenced directly. This evaluates to SyntaxKind.None on Roslyn
    // versions that don't recognize "safe" as a contextual keyword, which then simply never matches/gets added.
    public static readonly SyntaxKind SafeKeyword = SyntaxFacts.GetContextualKeywordKind("safe");

    // Looked up reflectively (rather than referenced directly) since this property doesn't exist on the
    // Microsoft.CodeAnalysis.CSharp version currently referenced by this project, but may exist on a newer one;
    // once it does, it is preferred over the Features dictionary fallback below.
    private static readonly PropertyInfo? MemorySafetyRulesVersionProperty
        = typeof(CSharpCompilationOptions).GetProperty("MemorySafetyRulesVersion", BindingFlags.Public | BindingFlags.Instance);

    public static bool UseUpdatedMemorySafetyRules(this Compilation compilation)
    {
        if (MemorySafetyRulesVersionProperty is not null
            && compilation is CSharpCompilation { Options: CSharpCompilationOptions options }
            && MemorySafetyRulesVersionProperty.GetValue(options)?.ToString() == "Version2")
        {
            return true;
        }

        foreach (var syntaxTree in compilation.SyntaxTrees)
        {
            if (syntaxTree.Options.Features.TryGetValue(UpdatedMemorySafetyRulesFeature, out var value)
                && bool.TryParse(value, out var enabled)
                && enabled)
            {
                return true;
            }
        }

        return false;
    }

    public static bool UseSafeKeyword(string? langVersion)
    {
        if (string.IsNullOrWhiteSpace(langVersion))
        {
            return SafeKeyword != SyntaxKind.None;
        }

        var normalized = langVersion.Trim();

        if (normalized.Equals("default", StringComparison.OrdinalIgnoreCase)
            || normalized.Equals("latest", StringComparison.OrdinalIgnoreCase)
            || normalized.Equals("latestmajor", StringComparison.OrdinalIgnoreCase)
            || normalized.Equals("preview", StringComparison.OrdinalIgnoreCase))
        {
            return SafeKeyword != SyntaxKind.None;
        }

        if (Version.TryParse(normalized, out var version))
        {
            return SafeKeyword != SyntaxKind.None && version.Major > 14;
        }

        if (int.TryParse(normalized, out var major))
        {
            return SafeKeyword != SyntaxKind.None && major > 14;
        }

        throw new ArgumentException(DesignStrings.InvalidCSharpLanguageVersion(langVersion), nameof(langVersion));
    }
}

