// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Diagnostics;

/// <summary>
///     A <see cref="IIdentityResolutionInterceptor" /> that copies property values from the new entity instance into the
///     tracked entity instance.
/// </summary>
public class UpdatingIdentityResolutionInterceptor : IIdentityResolutionInterceptor
{
    private readonly bool _preserveModifiedValues;
    private readonly bool _updateOriginalValues;

    /// <summary>
    ///     Creates a new instance of the interceptor.
    /// </summary>
    /// <param name="preserveModifiedValues">
    ///     If <see langword="true" />, then values for properties marked as modified in the current instance will
    ///     not be updated by values from the new instance.
    /// </param>
    /// <param name="updateOriginalValues">
    ///     If <see langword="true" />, then both current and original values of the current instance are updated to
    ///     current values from the new instance.
    /// </param>
    public UpdatingIdentityResolutionInterceptor(
        bool preserveModifiedValues = false,
        bool updateOriginalValues = false)
    {
        _preserveModifiedValues = preserveModifiedValues;
        _updateOriginalValues = updateOriginalValues;
    }

    /// <summary>
    ///     Called when a <see cref="DbContext" /> attempts to track a new instance of an entity with the same primary key value as
    ///     an already tracked instance. This implementation copies property values from the new entity instance into the
    ///     tracked entity instance.
    /// </summary>
    /// <param name="interceptionData">Contextual information about the identity resolution.</param>
    /// <param name="existingEntry">The entry for the existing tracked entity instance.</param>
    /// <param name="newEntity">The new entity instance, which will be discarded after this call.</param>
    public virtual void UpdateTrackedInstance(
        IdentityResolutionInterceptionData interceptionData,
        EntityEntry existingEntry,
        object newEntity)
    {
        var tempEntry = interceptionData.Context.Entry(newEntity);

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
                var existingPropertyEntry = existingEntry.Property(propertyEntry.Metadata.Name);

                if (!_preserveModifiedValues || !existingPropertyEntry.IsModified)
                {
                    existingPropertyEntry.CurrentValue = propertyEntry.CurrentValue;
                }

                if (_updateOriginalValues)
                {
                    existingPropertyEntry.OriginalValue = propertyEntry.CurrentValue;
                }
            }
        }
    }
}
