// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;

/// <summary>
///     <para>
///         A service on the EF internal service provider that creates the <see cref="ConventionSet" />
///         for the current database provider. This is combined with <see cref="IConventionSetPlugin" />
///         instances to produce the full convention set exposed by the <see cref="IConventionSetBuilder" />
///         service.
///     </para>
///     <para>
///         This type is typically used by database providers (and other extensions). It is generally
///         not used in application code.
///     </para>
/// </summary>
/// <remarks>
///     <para>
///         Database providers should implement this service by inheriting from either
///         <see cref="ProviderConventionSetBuilder" /> (for non-relational providers) or
///         `RelationalConventionSetBuilder` (for relational providers).
///     </para>
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
public interface IProviderConventionSetBuilder
{
    /// <summary>
    ///     Builds and returns the convention set for the current database provider.
    /// </summary>
    /// <returns>The convention set for the current database provider.</returns>
    ConventionSet CreateConventionSet();
}
