// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Diagnostics;

/// <summary>
///     A <see cref="IIdentityResolutionInterceptor"/> that copies property values from the new entity instance into the
///     tracked entity instance.
/// </summary>
public class CopyingIdentityResolutionInterceptor : IIdentityResolutionInterceptor
{
    /// <summary>
    ///     Called when a <see cref="DbContext"/> attempts to track a new instance of an entity with the same primary key value as
    ///     an already tracked instance. This implementation copies property values from the new entity instance into the
    ///     tracked entity instance.
    /// </summary>
    /// <param name="context">The <see cref="DbContext"/> is use.</param>
    /// <param name="existingEntry">The entry for the existing tracked entity instance.</param>
    /// <param name="newInstance">The new entity instance, which will be discarded after this call.</param>
    public virtual void UpdateTrackedInstance(DbContext context, EntityEntry existingEntry, object newInstance)
    {
        var tempEntry = context.Entry(newInstance);

        if (existingEntry.State == EntityState.Added)
        {
            foreach (var propertyEntry in tempEntry.Properties.Where(
                         e => e.Metadata.GetBeforeSaveBehavior() != PropertySaveBehavior.Throw))
            {
                existingEntry.Property(propertyEntry.Metadata.Name).CurrentValue = propertyEntry.CurrentValue;
            }
        }
        else
        {
            foreach (var propertyEntry in tempEntry.Properties.Where(
                         e => e.Metadata.GetAfterSaveBehavior() != PropertySaveBehavior.Throw))
            {
                existingEntry.Property(propertyEntry.Metadata.Name).CurrentValue = propertyEntry.CurrentValue;
            }
        }
        
    }
}
