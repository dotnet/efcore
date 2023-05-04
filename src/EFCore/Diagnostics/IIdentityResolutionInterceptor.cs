// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Diagnostics;

/// <summary>
///     Allows interception of identity resolution conflicts when the <see cref="DbContext" /> starts tracking new entity
///     instances.
/// </summary>
/// <remarks>
///     <para>
///         A <see cref="DbContext" /> can only track one entity instance with any given primary key value. This means multiple instances
///         of an entity with the same key value must be resolved to a single instance. An interceptor of this type can be used to do
///         this. It is called with the existing tracked instance and the new instance and must apply any property values and relationship
///         changes from the new instance into the existing instance. The new instance is then discarded.
///     </para>
///     <para>
///         Use <see cref="DbContextOptionsBuilder.AddInterceptors(Microsoft.EntityFrameworkCore.Diagnostics.IInterceptor[])" />
///         to register application interceptors.
///     </para>
///     <para>
///         Extensions can also register interceptors in the internal service provider.
///         If both injected and application interceptors are found, then the injected interceptors are run in the
///         order that they are resolved from the service provider, and then the application interceptors are run last.
///     </para>
///     <para>
///         See <see href="https://aka.ms/efcore-docs-interceptors">EF Core interceptors</see>
///         and <see href="https://aka.ms/efcore-docs-change-tracking">EF Core change tracking</see> for more information and examples.
///     </para>
/// </remarks>
public interface IIdentityResolutionInterceptor : IInterceptor
{
    /// <summary>
    ///     Called when a <see cref="DbContext" /> attempts to track a new instance of an entity with the same primary key value as
    ///     an already tracked instance. This method must apply any property values and relationship changes from the new instance
    ///     into the existing instance. The new instance is then discarded.
    /// </summary>
    /// <param name="interceptionData">Contextual information about the identity resolution.</param>
    /// <param name="existingEntry">The entry for the existing tracked entity instance.</param>
    /// <param name="newEntity">The new entity instance, which will be discarded after this call.</param>
    void UpdateTrackedInstance(IdentityResolutionInterceptionData interceptionData, EntityEntry existingEntry, object newEntity);
}
