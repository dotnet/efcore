// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Diagnostics;

/// <summary>
///     A parameter object passed to <see cref="IIdentityResolutionInterceptor" /> methods.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-diagnostics">Logging, events, and diagnostics</see> for more information and examples.
/// </remarks>
public readonly struct IdentityResolutionInterceptionData
{
    /// <summary>
    ///     Constructs the parameter object.
    /// </summary>
    /// <param name="context">The <see cref="DbContext" /> in use.</param>
    [EntityFrameworkInternal]
    [UsedImplicitly]
    public IdentityResolutionInterceptionData(DbContext context)
    {
        Context = context;
    }

    /// <summary>
    ///     The current <see cref="DbContext" /> instance being used.
    /// </summary>
    public DbContext Context { get; }
}
