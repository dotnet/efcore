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

            _partitionKeyProp = entry.EntityType.GetPropertyByColumnName("PartitionKey");
            _rowKeyProp = entry.EntityType.GetPropertyByColumnName("RowKey");

            // An optional CLR fields: required DTO
            _timestampProp = entry.EntityType.TryGetPropertyByColumnName("Timestamp");
            _etagProp = entry.EntityType.TryGetPropertyByColumnName("ETag");
        }

        public virtual StateEntry StateEntry
        {
            get { return _entry; }
        }

        public virtual T Entity
        {
            get { return (T)_entry.Entity; }
        }

        public virtual void ReadEntity(IDictionary<string, EntityProperty> properties, OperationContext operationContext)
        {
            var entityType = _entry.EntityType;
            foreach (var property in properties)
            {
                var entityProp = entityType.TryGetPropertyByColumnName(property.Key);
                if (entityProp != null
                    && (entityProp.IsShadowProperty
                        || EdmTypeMatchesClrType(property.Value.PropertyType, entityProp.PropertyType)))
                {
                    SetProperty(entityProp, property.Value.PropertyAsObject);
                }
            }
        }

        public virtual IDictionary<string, EntityProperty> WriteEntity(OperationContext operationContext)
        {
            var retVals = new Dictionary<string, EntityProperty>();

            foreach (var property in _entry.EntityType.Properties)
            {
                if (IsReservedStorageProperty(property.AzureTableStorage().Column))
                {
                    continue;
                }
                var newProperty = EntityProperty.CreateEntityPropertyFromObject(_entry[property]);

                // property will be null if unknown type
                if (newProperty != null)
                {
                    retVals.Add(property.AzureTableStorage().Column, newProperty);
                }
            }
            return retVals;
        }

        public virtual string PartitionKey
        {
            get { return _entry[_partitionKeyProp].ToString(); }
            set { SetProperty(_partitionKeyProp, value); }
        }

        public virtual string RowKey
        {
            get { return _entry[_rowKeyProp].ToString(); }
            set { SetProperty(_rowKeyProp, value); }
        }

        public virtual DateTimeOffset Timestamp
        {
            get { return (_timestampProp != null) ? (DateTimeOffset)_entry[_timestampProp] : default(DateTimeOffset); }
            set
            {
                if (_timestampProp != null)
                {
                    SetProperty(_timestampProp, value);
                }
            }
        }

        // An optional field: required for DTO
        public virtual string ETag
        {
            get { return _etagProp != null ? _entry[_etagProp].ToString() : _etag; }
            set
            {
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
            _entry[prop] = value;
        }

        private static bool EdmTypeMatchesClrType(EdmType edmType, Type clrType)
        {
            switch (edmType)
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
