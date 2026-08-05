// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.ManyToManyModel;

#nullable disable

public class ProxyableSharedType
{
    private readonly Dictionary<string, object> _keyValueStore = new();

    public virtual object this[string key]
    {
        get => _keyValueStore.TryGetValue(key, out var value) ? value : default;
        set => _keyValueStore[key] = value;
    }
}
