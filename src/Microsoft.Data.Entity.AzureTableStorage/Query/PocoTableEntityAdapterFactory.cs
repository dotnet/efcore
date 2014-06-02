// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.WindowsAzure.Storage.Table;

namespace Microsoft.Data.Entity.AzureTableStorage.Query
{
    public class PocoTableEntityAdapterFactory : ITableEntityFactory
    {
        private readonly IDictionary<Type, ConstructorInfo> _ctors = new Dictionary<Type, ConstructorInfo>();

        public ITableEntity MakeFromObject(object obj)
        {
            var objType = obj.GetType();
            var ctor = GetOrMakeAdapterCtor(objType);
            return (ITableEntity)ctor.Invoke(new[] { obj });
        }

        private ConstructorInfo GetOrMakeAdapterCtor(Type objType)
        {
            if (_ctors.ContainsKey(objType))
            {
                return _ctors[objType];
            }
            var ctor = typeof(PocoTableEntityAdapter<>).MakeGenericType(objType).GetConstructor(new[] { objType });
            _ctors[objType] = ctor;
            return ctor;
        }
    }
}
