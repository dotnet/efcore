// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Microsoft.EntityFrameworkCore.Query.Internal;

/// <summary>
///     Detects whether a <see cref="Compilation" /> was parsed with C#'s "updated memory safety rules"
///     (a.k.a. "unsafe evolution") enabled, and resolves the <c>safe</c> contextual keyword so that it can be
///     emitted on generated <see cref="System.Runtime.CompilerServices.UnsafeAccessorAttribute" /> accessor
///     methods. Adapted from
///     https://github.com/dotnet/runtime/blob/main/src/libraries/System.Runtime.InteropServices/gen/Common/MemorySafetyRules.cs.
/// </summary>
internal static class MemorySafetyRules
{
    // This is the feature flag name recognized by the C# parser (passed via '/features:updated-memory-safety-rules'
    // or the equivalent LangVersion/parse options). Roslyn does not yet expose a stable, non-experimental API for
    // querying whether a compilation was parsed with the updated memory safety rules enabled
    // (see https://github.com/dotnet/roslyn/issues/82546), so this flag is used as a fallback whenever the
    // Roslyn version referenced by this generator doesn't expose the (currently experimental)
    // CSharpCompilationOptions.MemorySafetyRulesVersion API looked up reflectively below.
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
            && compilation is CSharpCompilation { Options: CSharpCompilationOptions options })
        {
            return MemorySafetyRulesVersionProperty.GetValue(options)?.ToString() == "Version2";
        }

        return compilation.SyntaxTrees.FirstOrDefault()?.Options.Features.ContainsKey(UpdatedMemorySafetyRulesFeature) == true;
    }
}
