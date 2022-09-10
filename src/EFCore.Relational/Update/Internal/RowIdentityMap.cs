// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Update.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class RowIdentityMap<TKey> : IRowIdentityMap
    where TKey : notnull
{
    private readonly IUniqueConstraint _key;
    private readonly Dictionary<TKey, INonTrackedModificationCommand> _identityMap;
    private readonly IRowKeyValueFactory<TKey> _principalKeyValueFactory;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public RowIdentityMap(IUniqueConstraint key)
    {
        _key = key;
        _principalKeyValueFactory = (IRowKeyValueFactory<TKey>)((UniqueConstraint)_key).GetRowKeyValueFactory();
        _identityMap = new Dictionary<TKey, INonTrackedModificationCommand>(_principalKeyValueFactory.EqualityComparer);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IEnumerable<INonTrackedModificationCommand> Rows
        => _identityMap.Values;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual INonTrackedModificationCommand? FindCommand(object?[] keyValues)
    {
        var key = _principalKeyValueFactory.CreateKeyValue(keyValues);
        return key != null && _identityMap.TryGetValue(key, out var command) ? command : null;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void Add(object?[] keyValues, INonTrackedModificationCommand command)
        => Add(_principalKeyValueFactory.CreateKeyValue(keyValues), command);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual void Add(TKey key, INonTrackedModificationCommand command)
    {
#if DEBUG
        if (_identityMap.TryGetValue(key, out var existingCommand))
        {
            Check.DebugAssert(existingCommand == command, $"Command with key {key} already added");
        }
#endif

        _identityMap[key] = command;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void Remove(INonTrackedModificationCommand command)
        => Remove(_principalKeyValueFactory.CreateKeyValue(command), command);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    private void Remove(TKey key, INonTrackedModificationCommand command)
    {
        if (_identityMap.TryGetValue(key, out var existingEntry)
            && existingEntry == command)
        {
            _identityMap.Remove(key);
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void Clear()
        => _identityMap.Clear();
}
