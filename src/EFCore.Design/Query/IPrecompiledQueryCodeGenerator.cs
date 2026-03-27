// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.EntityFrameworkCore.Design.Internal;
using static Microsoft.EntityFrameworkCore.Query.Internal.PrecompiledQueryCodeGenerator;

namespace Microsoft.EntityFrameworkCore.Query;

/// <summary>
///     Used to generate code for precompiled queries.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-compiled-models">EF Core compiled models</see>, and
///     <see href="https://aka.ms/efcore-docs-design-time-services">EF Core design-time services</see> for more information and examples.
/// </remarks>
[Experimental(EFDiagnostics.PrecompiledQueryExperimental)]
public interface IPrecompiledQueryCodeGenerator : ILanguageBasedService
{
    /// <summary>
    ///     Generates the precompiled queries code.
    /// </summary>
    /// <param name="compilation">The compilation.</param>
    /// <param name="syntaxGenerator">The syntax generator.</param>
    /// <param name="dbContext">The context.</param>
    /// <param name="memberAccessReplacements">The member access replacements.</param>
    /// <param name="precompilationErrors">A list that will contain precompilation errors.</param>
    /// <param name="generatedFileNames">The set of file names generated so far.</param>
    /// <param name="assembly">The assembly corresponding to the provided compilation.</param>
    /// <param name="suffix">The suffix to attach to the name of all the generated files.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The files containing precompiled queries code.</returns>
    IReadOnlyList<ScaffoldedFile> GeneratePrecompiledQueries(
        Compilation compilation,
        SyntaxGenerator syntaxGenerator,
        DbContext dbContext,
        IReadOnlyDictionary<MemberInfo, QualifiedName> memberAccessReplacements,
        List<QueryPrecompilationError> precompilationErrors,
        ISet<string> generatedFileNames,
        Assembly? assembly = null,
        string? suffix = null,
        CancellationToken cancellationToken = default);
}
