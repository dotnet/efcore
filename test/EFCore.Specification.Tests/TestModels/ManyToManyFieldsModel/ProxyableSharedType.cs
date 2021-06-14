// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.EntityFrameworkCore.TestModels.ManyToManyFieldsModel
{
    public class ProxyableSharedType
    {
        private readonly Dictionary<string, object> _keyValueStore = new();

        public virtual object this[string key]
        {
            get => _keyValueStore.TryGetValue(key, out var value) ? value : default;
            set => _keyValueStore[key] = value;
        }
    }
}
