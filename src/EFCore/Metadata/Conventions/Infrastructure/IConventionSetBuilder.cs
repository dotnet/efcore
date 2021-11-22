// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;

/// <summary>
///     <para>
///         A service on the EF internal service provider that creates the <see cref="ConventionSet" />
///         that will be used by the current <see cref="DbContext" /> instance.
///     </para>
///     <para>
///         This type is typically used by database providers (and other extensions). It is generally
///         not used in application code.
///     </para>
/// </summary>
/// <remarks>
///     <para>
///         The service lifetime is <see cref="ServiceLifetime.Scoped" />. This means that each
///         <see cref="DbContext" /> instance will use its own instance of this service.
///         The implementation may depend on other services registered with any lifetime.
///         The implementation does not need to be thread-safe.
///     </para>
///     <para>
///         See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see> for more information and examples.
///     </para>
/// </remarks>
public interface IConventionSetBuilder
{
    /// <summary>
    ///     Builds and returns the convention set to use.
    /// </summary>
    /// <returns>The convention set to use.</returns>
    ConventionSet CreateConventionSet();
}
