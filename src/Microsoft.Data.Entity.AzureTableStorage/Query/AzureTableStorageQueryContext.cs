// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Data.Entity.AzureTableStorage.Utilities;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Query;
using Microsoft.Framework.Logging;
using Microsoft.WindowsAzure.Storage.Table;

namespace Microsoft.Data.Entity.AzureTableStorage.Query
{
    public class AzureTableStorageQueryContext : QueryContext
    {
        private readonly AzureTableStorageConnection _database;

        public AzureTableStorageQueryContext(
            [NotNull] IModel model,
            [NotNull] ILogger logger,
            [NotNull] StateManager stateManager,
            [NotNull] AzureTableStorageConnection database,
            [NotNull] AtsValueReaderFactory readerFactory)
            : base(model, logger, stateManager)
        {
            Check.NotNull(model, "model");
            Check.NotNull(logger, "logger");
            Check.NotNull(stateManager, "stateManager");
            Check.NotNull(readerFactory, "readerFactory");

            _database = database;
            ValueReaderFactory = readerFactory;
        }

        public virtual AzureTableStorageConnection Database
        {
            get { return _database; }
        }

        public AtsValueReaderFactory ValueReaderFactory { get; private set; }
    }

    public class AtsValueReaderFactory
    {
        public IValueReader Create(IEntityType type, AtsNamedValueBuffer source)
        {
            var valueBuffer = new object[type.Properties.Count];
            foreach (var property in type.Properties)
            {
                valueBuffer[property.Index] = source.TryGet(property.StorageName);
            }
            return new AtsObjectArrayValueReader(valueBuffer);
        }
    }

    public class AtsObjectArrayValueReader : ObjectArrayValueReader
    {
        public AtsObjectArrayValueReader(object[] valueBuffer)
            :base(valueBuffer)
        {
        }

        public override T ReadValue<T>(int index)
        {
            try
            {
                return base.ReadValue<T>(index);
            }
            catch (InvalidCastException)
            {
                var readValue = base.ReadValue<object>(index);
                if (typeof(int).IsAssignableFrom(typeof(T)) && readValue is string)
                {
                    return (T) FromString<int>((string)readValue);
                }
                throw;
            }
        }

        private static object FromString<T>(string readValue)
        {
            if (typeof(int).IsAssignableFrom(typeof(T)))
            {
                return int.Parse(readValue);
            }
            return readValue;
        }
    }

    public class AtsNamedValueBuffer
    {
        private readonly Dictionary<string, object> _values = new Dictionary<string, object>();

        public AtsNamedValueBuffer()
        {
        }

        internal AtsNamedValueBuffer(IEnumerable<KeyValuePair<string, EntityProperty>> properties)
        {
            foreach (var property in properties)
            {
                this[property.Key] = property.Value.PropertyAsObject;
            }
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
            get { return _values[key]; }
            set { _values[key] = value; }
        }
    }
}
