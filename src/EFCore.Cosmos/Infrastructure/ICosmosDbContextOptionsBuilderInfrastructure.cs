// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Infrastructure;

/// <summary>
///     Explicitly implemented by <see cref="CosmosDbContextOptionsBuilder" /> to hide
///     methods that are used by database provider extension methods but not intended to be called by application
///     developers.
/// </summary>
public interface ICosmosDbContextOptionsBuilderInfrastructure
{
    /// <summary>
    ///     Gets the core options builder.
    /// </summary>
    DbContextOptionsBuilder OptionsBuilder { get; }
}
