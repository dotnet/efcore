// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Design;

/// <summary>
///     Enables configuring design-time services. Tools will automatically discover implementations of this
///     interface that are in the startup assembly.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-providers">Implementation of database providers and extensions</see>
///     for more information and examples.
/// </remarks>
public interface IDesignTimeServices
{
    /// <summary>
    ///     Configures design-time services. Use this method to override the default design-time services with your
    ///     own implementations.
    /// </summary>
    /// <param name="serviceCollection">The design-time service collection.</param>
    void ConfigureDesignTimeServices(IServiceCollection serviceCollection);
}
