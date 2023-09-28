// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class MetadataTracker : IReferenceRoot<IConventionForeignKey>
{
    private readonly Dictionary<IConventionForeignKey, Reference<IConventionForeignKey>> _trackedForeignKeys = new();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void Update(IConventionForeignKey oldForeignKey, IConventionForeignKey newForeignKey)
    {
        Check.DebugAssert(
            !oldForeignKey.IsInModel && newForeignKey.IsInModel,
            $"{nameof(oldForeignKey)} is in the model or {nameof(newForeignKey)} isn't");

        if (_trackedForeignKeys.TryGetValue(oldForeignKey, out var reference))
        {
            _trackedForeignKeys.Remove(oldForeignKey);
            reference.Object = newForeignKey;
            _trackedForeignKeys.Add(newForeignKey, reference);
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual Reference<IConventionForeignKey> Track(IConventionForeignKey foreignKey)
    {
        if (_trackedForeignKeys.TryGetValue(foreignKey, out var reference))
        {
            reference.IncreaseReferenceCount();
            return reference;
        }

        reference = new Reference<IConventionForeignKey>(foreignKey, this);
        _trackedForeignKeys.Add(foreignKey, reference);

        return reference;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    void IReferenceRoot<IConventionForeignKey>.Release(Reference<IConventionForeignKey> foreignKeyReference)
        => _trackedForeignKeys.Remove(foreignKeyReference.Object);
}
