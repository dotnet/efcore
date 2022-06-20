﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Diagnostics;

/// <summary>
///     A <see cref="IIdentityResolutionInterceptor"/> that ignores the new instance and retains property values from the existing
///     tracked instance.
/// </summary>
public class SkippingIdentityResolutionInterceptor : IIdentityResolutionInterceptor
{
    /// <summary>
    ///     Called when a <see cref="DbContext"/> attempts to track a new instance of an entity with the same primary key value as
    ///     an already tracked instance. This implementation does nothing, such that property values from the existing tracked
    ///     instance are retained.
    /// </summary>
    /// <param name="context">The <see cref="DbContext"/> is use.</param>
    /// <param name="existingEntry">The entry for the existing tracked entity instance.</param>
    /// <param name="newInstance">The new entity instance, which will be discarded after this call.</param>
    public virtual void UpdateTrackedInstance(DbContext context, EntityEntry existingEntry, object newInstance)
    {
    }
}
