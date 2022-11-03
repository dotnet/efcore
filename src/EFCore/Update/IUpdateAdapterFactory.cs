// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;

namespace Microsoft.EntityFrameworkCore.Update;

/// <summary>
///     <para>
///         Factory for creating <see cref="IUpdateAdapter" /> instances.
///     </para>
///     <para>
///         This interface is typically used by database providers (and other extensions). It is generally
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
///         See <see href="https://aka.ms/efcore-docs-providers">Implementation of database providers and extensions</see>
///         for more information and examples.
///     </para>
/// </remarks>
public interface IUpdateAdapterFactory
{
    /// <summary>
    ///     Creates a tracker for the model currently in use.
    /// </summary>
    /// <returns>The new tracker.</returns>
    IUpdateAdapter Create();

    /// <summary>
    ///     Creates a standalone tracker that works with its own <see cref="IStateManager" /> and hence will not
    ///     impact tracking on the state manager currently in use.
    /// </summary>
    /// <remarks>
    ///     The <see cref="IUpdateAdapter.Entries" /> from this update adapter should be used explicitly
    ///     once they have been setup. They will not be visible to other parts of the stack,
    ///     including <see cref="DbContext.SaveChanges()" />.
    /// </remarks>
    /// <param name="model">The model for which a tracker is needed, or null to use the current model.</param>
    /// <returns>The new tracker.</returns>
    IUpdateAdapter CreateStandalone(IModel? model = null);
}
