// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Data.Entity.AzureTableStorage.Utilities;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Metadata;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace Microsoft.Data.Entity.AzureTableStorage.Adapters
{
    public class StateEntryTableEntityAdapter<T> : ITableEntityAdapter<T>
    {
        private readonly StateEntry _entry;
        private readonly IProperty _partitionKeyProp;
        private readonly IProperty _rowKeyProp;
        [CanBeNull]
        private readonly IProperty _etagProp;
        private readonly IProperty _timestampProp;
        private string _etag;

        public StateEntryTableEntityAdapter([NotNull] StateEntry entry)
        {
            Check.NotNull(entry, "entry");
            _entry = entry;

            _partitionKeyProp = entry.EntityType.GetPropertyByStorageName("PartitionKey");
            _rowKeyProp = entry.EntityType.GetPropertyByStorageName("RowKey");
            _timestampProp = entry.EntityType.GetPropertyByStorageName("Timestamp");

            // An optional field: required DTO
            _etagProp = entry.EntityType.TryGetPropertyByStorageName("ETag");
        }

        public StateEntry StateEntry { get { return _entry; } }
        public T Entity { get { return (T)_entry.Entity; } }

        public void ReadEntity(IDictionary<string, EntityProperty> properties, OperationContext operationContext)
        {
            var entityType = _entry.EntityType;
            foreach (var property in properties)
            {
                var entityProp = entityType.TryGetPropertyByStorageName(property.Key);
                if (entityProp == null
                    || entityProp.IsClrProperty && MismatchedTypes(property.Value.PropertyType, entityProp.PropertyType))
                {
                    continue;
                }
                SetProperty(entityProp,property.Value.PropertyAsObject);
            }
        }

        public IDictionary<string, EntityProperty> WriteEntity(OperationContext operationContext)
        {
            var retVals = new Dictionary<string, EntityProperty>();

            foreach (var property in _entry.EntityType.Properties)
            {
                if (IsReservedStorageProperty(property.StorageName))
                {
                    continue;
                }
                var newProperty = EntityProperty.CreateEntityPropertyFromObject(_entry[property]);

                // property will be null if unknown type
                if (newProperty != null)
                {
                    retVals.Add(property.StorageName, newProperty);
                }
            }
            return retVals;
        }

        public string PartitionKey
        {
            get { return _entry[_partitionKeyProp].ToString(); }
            set { SetProperty(_partitionKeyProp,value);}
        }

        public string RowKey
        {
            get { return _entry[_rowKeyProp].ToString(); }
            set { SetProperty(_rowKeyProp, value); }
        }

        public DateTimeOffset Timestamp
        {
            get { return (DateTimeOffset)_entry[_timestampProp]; } //TODO handle DateTime and shadow states
            set { SetProperty(_timestampProp, value); }
        }

        // An optional field: required for DTO
        public string ETag
        {
            get
            {
                return _etagProp != null ? _entry[_etagProp].ToString() : _etag;
            }
            set {
                if (_etagProp == null)
                {
                    _etag = value;
                }
                else
                {
                    SetProperty(_etagProp, value);
                }
            }
        }

        private void SetProperty(IProperty prop, object value)
        {
            if (prop.IsClrProperty)
            {
                //TODO type checking/formatting
                _entry[prop] = value;
            }
            else
            {
                //TODO set shadow state properties
                _entry[prop] = value;
            }
        }

        private bool MismatchedTypes(EdmType propertyType, Type clrType)
        {
            switch (propertyType)
            {
                case EdmType.String:
                    return typeof(string).IsAssignableFrom(clrType);
                case EdmType.Binary:
                    return typeof(byte[]).IsAssignableFrom(clrType);
                case EdmType.Boolean:
                    return typeof(bool).IsAssignableFrom(clrType);
                case EdmType.DateTime:
                    return typeof(DateTime).IsAssignableFrom(clrType);
                case EdmType.Double:
                    return typeof(double).IsAssignableFrom(clrType);
                case EdmType.Guid:
                    return typeof(Guid).IsAssignableFrom(clrType);
                case EdmType.Int32:
                    return typeof(Int32).IsAssignableFrom(clrType);
                case EdmType.Int64:
                    return typeof(Int64).IsAssignableFrom(clrType);
                default:
                    return false;
            }
        }

        private static readonly ISet<string> _reservedStorageNames = new HashSet<string> { "PartitionKey", "RowKey", "ETag", "Timestamp" };

        private static bool IsReservedStorageProperty(string propertyName)
        {
            return _reservedStorageNames.Contains(propertyName);
        }
    }
}
