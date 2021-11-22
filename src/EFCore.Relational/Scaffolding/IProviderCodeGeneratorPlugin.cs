// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Scaffolding;

/// <summary>
///     Implemented by plugins to generate code fragments for reverse engineering.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-scaffolding">Reverse engineering (scaffolding) an existing database</see>, and
///     <see href="https://aka.ms/efcore-docs-design-time-services">EF Core design-time services</see> for more information and examples.
/// </remarks>
public interface IProviderCodeGeneratorPlugin
{
    /// <summary>
    ///     Generates a method chain used to configure provider-specific options.
    /// </summary>
    /// <returns>The method chain. May be null.</returns>
    MethodCallCodeFragment? GenerateProviderOptions();

    /// <summary>
    ///     Generates a method chain to configure additional context options.
    /// </summary>
    /// <returns>The method chain. May be null.</returns>
    MethodCallCodeFragment? GenerateContextOptions();
}
