// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Internal;

/// <summary>
///     <para>
///         This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///         the same compatibility standards as public APIs. It may be changed or removed without notice in
///         any release. You should only use it directly in your code with extreme caution and knowing that
///         doing so can result in application failures when updating to a new Entity Framework Core release.
///     </para>
///     <para>
///         Implemented by service property types to notify services instances of lifecycle changes.
///     </para>
/// </summary>
public interface IInjectableService
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    void ServiceObtained(DbContext context, ParameterBindingInfo bindingInfo);

    /// <summary>
    ///     <para>
    ///         This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///         the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///         any release. You should only use it directly in your code with extreme caution and knowing that
    ///         doing so can result in application failures when updating to a new Entity Framework Core release.
    ///     </para>
    ///     <para>
    ///         Called when the entity holding this service is being detached from a DbContext.
    ///     </para>
    /// </summary>
    /// <param name="context">The <see cref="DbContext" /> instance.</param>
    /// <param name="entity">The entity instance that is being detached.</param>
    /// <returns><see langword="true" /> if the service property should be set to <see langword="null" />. </returns>
    bool Detaching(DbContext context, object entity);

    /// <summary>
    ///     <para>
    ///         This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///         the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///         any release. You should only use it directly in your code with extreme caution and knowing that
    ///         doing so can result in application failures when updating to a new Entity Framework Core release.
    ///     </para>
    ///     <para>
    ///         Called when an entity that needs this is being attached to a DbContext.
    ///     </para>
    /// </summary>
    /// <param name="context">The <see cref="DbContext" /> instance.</param>
    /// <param name="entity">The entity instance that is being attached.</param>
    /// <param name="existingService">
    ///     The existing instance of this service being held by the entity instance, or <see langword="null" /> if there
    ///     is no existing instance.
    /// </param>
    /// <returns>The service instance to use, or <see langword="null" /> if a new instance should be created.</returns>
    IInjectableService? Attaching(DbContext context, object entity, IInjectableService? existingService);
}
