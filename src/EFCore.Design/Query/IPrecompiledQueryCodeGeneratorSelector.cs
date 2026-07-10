// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;

namespace Microsoft.EntityFrameworkCore.Query;

/// <summary>
///     Selects an <see cref="IPrecompiledQueryCodeGenerator" /> service for a given programming language.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-compiled-models">EF Core compiled models</see>, and
///     <see href="https://aka.ms/efcore-docs-design-time-services">EF Core design-time services</see> for more information and examples.
/// </remarks>
[Experimental(EFDiagnostics.PrecompiledQueryExperimental)]
public interface IPrecompiledQueryCodeGeneratorSelector
{
    /// <summary>
    ///     Selects an <see cref="IPrecompiledQueryCodeGenerator" /> service for a given programming language.
    /// </summary>
    /// <param name="language">The programming language.</param>
    /// <returns>The <see cref="IPrecompiledQueryCodeGenerator" />.</returns>
    IPrecompiledQueryCodeGenerator Select(string? language);
}
