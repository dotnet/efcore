// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.WindowsAzure.Storage.Table;

namespace Microsoft.Data.Entity.AzureTableStorage.Query
{
    public class AtsNamedValueBuffer
    {
        private IDictionary<string, EntityProperty> _values;

        internal AtsNamedValueBuffer(IDictionary<string, EntityProperty> values)
        {
            _values = values;
        }

        public object TryGet(string key)
        {
            try
            {
                return this[key];
            }
            catch (KeyNotFoundException)
            {
                return null;
            }
        }

        public object this[string key]
        {
            get { return _values[key].PropertyAsObject; }
        }

        public void Add(string key, string value)
        {
            _values[key] = new EntityProperty(value);
        }

        public void Add(string key, DateTimeOffset value)
        {
            _values[key] = new EntityProperty(value);
        }
    }
}
