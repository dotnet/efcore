// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Scaffolding;

/// <summary>
///     Base class used by plugins to generate code fragments for reverse engineering.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-scaffolding">Reverse engineering (scaffolding) an existing database</see>, and
///     <see href="https://aka.ms/efcore-docs-design-time-services">EF Core design-time services</see> for more information and examples.
/// </remarks>
public class ProviderCodeGeneratorPlugin : IProviderCodeGeneratorPlugin
{
    /// <summary>
    ///     Generates a method chain used to configure provider-specific options.
    /// </summary>
    /// <returns>The method chain. May be null.</returns>
    public virtual MethodCallCodeFragment? GenerateContextOptions()
        => null;

    /// <summary>
    ///     Generates a method chain to configure additional context options.
    /// </summary>
    /// <returns>The method chain. May be null.</returns>
    public virtual MethodCallCodeFragment? GenerateProviderOptions()
        => null;
}
