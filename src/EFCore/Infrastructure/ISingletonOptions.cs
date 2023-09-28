// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Infrastructure;

/// <summary>
///     <para>
///         Implemented by any class that represents options that can only be set at the
///         <see cref="IServiceProvider" /> singleton level.
///     </para>
/// </summary>
/// <remarks>
///     <para>
///         The service lifetime is <see cref="ServiceLifetime.Singleton" /> and multiple registrations
///         are allowed. This means a single instance of each service is used by many <see cref="DbContext" />
///         instances. The implementation must be thread-safe.
///         This service cannot depend on services registered as <see cref="ServiceLifetime.Scoped" />.
///     </para>
///     <para>
///         See <see href="https://aka.ms/efcore-docs-providers">Implementation of database providers and extensions</see>
///         for more information and examples.
///     </para>
/// </remarks>
public interface ISingletonOptions
{
    /// <summary>
    ///     Initializes the singleton options from the given <see cref="IDbContextOptions" />.
    /// </summary>
    void Initialize(IDbContextOptions options);

    /// <summary>
    ///     Validates that the options in given <see cref="IDbContextOptions" /> have not
    ///     changed when compared to the options already set here, and throws if they have.
    /// </summary>
    void Validate(IDbContextOptions options);
}
