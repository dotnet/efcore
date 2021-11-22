// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Update.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class UpdateAdapter : IUpdateAdapter
{
    private readonly IStateManager _stateManager;
    private readonly IChangeDetector _changeDetector;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public UpdateAdapter(IStateManager stateManager)
    {
        _stateManager = stateManager;
        _changeDetector = _stateManager.Context.GetDependencies().ChangeDetector;
    }

    /// <summary>
    ///     Gets or sets a value indicating when a dependent/child entity will have its state
    ///     set to <see cref="EntityState.Deleted" /> once severed from a parent/principal entity
    ///     through either a navigation or foreign key property being set to null. The default
    ///     value is <see cref="CascadeTiming.Immediate" />.
    /// </summary>
    /// <remarks>
    ///     Dependent/child entities are only deleted automatically when the relationship
    ///     is configured with <see cref="DeleteBehavior.Cascade" />. This is set by default
    ///     for required relationships.
    /// </remarks>
    public virtual CascadeTiming DeleteOrphansTiming
    {
        get => _stateManager.DeleteOrphansTiming;
        set => _stateManager.DeleteOrphansTiming = value;
    }

    /// <summary>
    ///     Gets or sets a value indicating when a dependent/child entity will have its state
    ///     set to <see cref="EntityState.Deleted" /> once its parent/principal entity has been marked
    ///     as <see cref="EntityState.Deleted" />. The default value is<see cref="CascadeTiming.Immediate" />.
    /// </summary>
    /// <remarks>
    ///     Dependent/child entities are only deleted automatically when the relationship
    ///     is configured with <see cref="DeleteBehavior.Cascade" />. This is set by default
    ///     for required relationships.
    /// </remarks>
    public virtual CascadeTiming CascadeDeleteTiming
    {
        get => _stateManager.CascadeDeleteTiming;
        set => _stateManager.CascadeDeleteTiming = value;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IUpdateEntry? FindPrincipal(IUpdateEntry dependentEntry, IForeignKey foreignKey)
        => _stateManager.FindPrincipal((InternalEntityEntry)dependentEntry, foreignKey);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IEnumerable<IUpdateEntry> GetDependents(IUpdateEntry principalEntry, IForeignKey foreignKey)
        => _stateManager.GetDependents((InternalEntityEntry)principalEntry, foreignKey);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IUpdateEntry? TryGetEntry(IKey key, object?[] keyValues)
        => _stateManager.TryGetEntry(key, keyValues);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IEnumerable<IUpdateEntry> Entries
        => _stateManager.Entries;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void DetectChanges()
    {
        if (!((IRuntimeModel)_stateManager.Model).SkipDetectChanges)
        {
            _changeDetector.DetectChanges(_stateManager);
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void CascadeChanges()
        => _stateManager.CascadeChanges(force: true);

    /// <summary>
    ///     Forces immediate cascading deletion of child/dependent entities when they are either
    ///     severed from a required parent/principal entity, or the required parent/principal entity
    ///     is itself deleted. See <see cref="DeleteBehavior" />.
    /// </summary>
    /// <param name="entry">The entry.</param>
    /// <param name="foreignKeys">The foreign keys to consider when cascading.</param>
    public virtual void CascadeDelete(IUpdateEntry entry, IEnumerable<IForeignKey>? foreignKeys = null)
        => _stateManager.CascadeDelete((InternalEntityEntry)entry, force: true, foreignKeys);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IList<IUpdateEntry> GetEntriesToSave()
        => _stateManager.GetEntriesToSave(cascadeChanges: false);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IUpdateEntry CreateEntry(
        IDictionary<string, object?> values,
        IEntityType entityType)
        => _stateManager.CreateEntry(values, entityType);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IModel Model
        => _stateManager.Model;
}
