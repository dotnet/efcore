// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.AzureTableStorage.Utilities;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.WindowsAzure.Storage.Table;

namespace Microsoft.Data.Entity.AzureTableStorage.Adapters
{
    public class TableEntityAdapterFactory
    {
        public ITableEntity CreateFromStateEntry([NotNull] StateEntry entry)
        {
            Check.NotNull(entry, "entry");

            var type = entry.Entity.GetType();
            var ctor = GetOrMakeStateAdapterCtor(type);
            return (ITableEntity) ctor.Invoke(new object[] { entry });
        }
        private readonly IDictionary<Type, ConstructorInfo> _stateCtors = new Dictionary<Type, ConstructorInfo>();

        private ConstructorInfo GetOrMakeStateAdapterCtor(Type objType)
        {
            if (_stateCtors.ContainsKey(objType))
            {
                return _stateCtors[objType];
            }
            var ctor = typeof(StateEntryTableEntityAdapter<>).MakeGenericType(objType).GetConstructor(new[] { typeof(StateEntry) });
            _stateCtors[objType] = ctor;
            return ctor;
        }
    }
}
