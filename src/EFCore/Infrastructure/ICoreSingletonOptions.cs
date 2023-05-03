// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Infrastructure;

/// <summary>
///     Options set at the <see cref="IServiceProvider" /> singleton level to control core options.
/// </summary>
/// <remarks>
///     <para>
///         The service lifetime is <see cref="ServiceLifetime.Singleton" />. This means a single instance
///         is used by many <see cref="DbContext" /> instances. The implementation must be thread-safe.
///         This service cannot depend on services registered as <see cref="ServiceLifetime.Scoped" />.
///     </para>
///     <para>
///         See <see href="https://aka.ms/efcore-docs-providers">Implementation of database providers and extensions</see>
///         for more information and examples.
///     </para>
/// </remarks>
public interface ICoreSingletonOptions : ISingletonOptions
{
    /// <summary>
    ///     Reflects the option set by <see cref="DbContextOptionsBuilder.EnableDetailedErrors" />.
    /// </summary>
    bool AreDetailedErrorsEnabled { get; }

    /// <summary>
    ///     Reflects the option set by <see cref="DbContextOptionsBuilder.EnableThreadSafetyChecks" />.
    /// </summary>
    bool AreThreadSafetyChecksEnabled { get; }

    /// <summary>
    ///     The root service provider for the application, if available. />.
    /// </summary>
    IServiceProvider? RootApplicationServiceProvider { get; }
}
