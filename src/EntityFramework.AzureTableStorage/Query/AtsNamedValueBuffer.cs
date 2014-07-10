// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Data.Entity.AzureTableStorage.Utilities;
using Microsoft.WindowsAzure.Storage.Table;

namespace Microsoft.Data.Entity.AzureTableStorage.Query
{
    public class AtsNamedValueBuffer
    {
        private readonly IDictionary<string, EntityProperty> _values;

        public AtsNamedValueBuffer([NotNull] IDictionary<string, EntityProperty> values)
        {
            Check.NotNull(values, "values");

            _values = values;
        }

        public virtual object TryGet([NotNull] string key)
        {
            EntityProperty obj;
            if (_values.TryGetValue(key, out obj))
            {
                return obj.PropertyAsObject;
            }
            return null;
        }

        public virtual object this[[NotNull] string key]
        {
            get
            {
                Check.NotNull(key, "key");

                return _values[key].PropertyAsObject;
            }
        }

        public virtual void Add([NotNull] string key, [CanBeNull] string value)
        {
            Check.NotNull(key, "key");

            _values[key] = new EntityProperty(value);
        }

        public virtual void Add([NotNull] string key, DateTimeOffset value)
        {
            Check.NotNull(key, "key");

            _values[key] = new EntityProperty(value);
        }
    }
}
