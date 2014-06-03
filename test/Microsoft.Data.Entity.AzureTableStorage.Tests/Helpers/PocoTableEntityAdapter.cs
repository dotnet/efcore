// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Security;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.WindowsAzure.Storage.Table.Protocol;

namespace Microsoft.Data.Entity.AzureTableStorage.Tests.Helpers
{
    public class PocoTableEntityAdapter<TEntity> : ITableEntity
        where TEntity : class, new()
    {
        private static MethodInfo _partitionKeyGetMethod;
        private static MethodInfo _partitionKeySetMethod;
        private static MethodInfo _rowKeyGetMethod;
        private static MethodInfo _rowKeySetMethod;
        private static MethodInfo _timestampGetMethod;
        private static MethodInfo _timestampSetMethod;
        private static MethodInfo _etagGetMethod;
        private static MethodInfo _etagSetMethod;

        public static void CheckProperties()
        {
            CheckProperty(name: "PartitionKey",
                type: typeof(string),
                getMethod: ref _partitionKeyGetMethod,
                setMethod: ref _partitionKeySetMethod);

            CheckProperty(name: "RowKey",
                type: typeof(string),
                getMethod: ref _rowKeyGetMethod,
                setMethod: ref _rowKeySetMethod);

            CheckProperty(name: "ETag",
                type: typeof(string),
                getMethod: ref _etagGetMethod,
                setMethod: ref _etagSetMethod,
                optional: true);

            CheckProperty(
                name: "Timestamp",
                type: typeof(DateTimeOffset),
                getMethod: ref _timestampGetMethod,
                setMethod: ref _timestampSetMethod,
                optional: true);
        }

        private static void CheckProperty(string name, Type type, ref MethodInfo getMethod, ref MethodInfo setMethod, bool optional = false)
        {
            try
            {
                var prop = typeof(TEntity).GetProperty(name);
                if (prop == null)
                {
                    if (optional)
                    {
                        return;
                    }
                    throw new TypeAccessException();
                }
                if (prop.PropertyType != type)
                {
                    throw new TypeAccessException();
                }
                //TODO support for special cases e.g. private getters in base case that get overridden
                getMethod = getMethod ?? prop.GetGetMethod(nonPublic: false);
                if (getMethod == null)
                {
                    throw new TypeAccessException();
                }
                setMethod = setMethod ?? prop.GetSetMethod(nonPublic: false);
                if (setMethod == null)
                {
                    throw new TypeAccessException();
                }
            }
            catch (AmbiguousMatchException e)
            {
                throw new TypeAccessException();
            }
            catch (SecurityException e)
            {
                throw new TypeAccessException();
            }
        }

        private readonly IEnumerable<PropertyInfo> _clrProperties;
        private string _etag;
        private DateTimeOffset _timestamp;
        public TEntity ClrInstance { get; private set; }

        public PocoTableEntityAdapter(TEntity clrInstance)
        {
            CheckProperties();
            ClrInstance = clrInstance;
            _clrProperties = ClrInstance.GetType().GetProperties();
        }

        public PocoTableEntityAdapter()
            : this(new TEntity())
        {
        }

        public string PartitionKey
        {
            get { return (string)_partitionKeyGetMethod.Invoke(ClrInstance, null); }
            set { _partitionKeySetMethod.Invoke(ClrInstance, new object[] { value }); }
        }

        public string RowKey
        {
            get { return (string)_rowKeyGetMethod.Invoke(ClrInstance, null); }
            set { _rowKeySetMethod.Invoke(ClrInstance, new object[] { value }); }
        }

        public DateTimeOffset Timestamp
        {
            get
            {
                if (_timestampGetMethod != null)
                {
                    return (DateTimeOffset)_timestampGetMethod.Invoke(ClrInstance, null);
                }
                return _timestamp;
            }
            set
            {
                if (_timestampSetMethod != null)
                {
                    _timestampSetMethod.Invoke(ClrInstance, new object[] { value });
                }
                else
                {
                    _timestamp = value;
                }
            }
        }

        public string ETag
        {
            get
            {
                if (_etagGetMethod != null)
                {
                    return (string)_etagGetMethod.Invoke(ClrInstance, null);
                }
                return _etag;
            }
            set
            {
                if (_etagSetMethod != null)
                {
                    _etagSetMethod.Invoke(ClrInstance, new object[] { value });
                }
                else
                {
                    _etag = value;
                }
            }
        }

        public void ReadEntity(IDictionary<string, EntityProperty> properties, OperationContext operationContext)
        {
            foreach (var property in _clrProperties)
            {
                if (ShouldSkipProperty(property))
                {
                    continue;
                }

                // only proceed with properties that have a corresponding entry in the dictionary
                if (!properties.ContainsKey(property.Name))
                {
                    continue;
                }

                var entityProperty = properties[property.Name];

                var isNull = false;
                try
                {
                    isNull = (bool)entityProperty.GetType()
                        .GetProperty("IsNull", BindingFlags.Instance | BindingFlags.NonPublic)
                        .GetGetMethod(true)
                        .Invoke(entityProperty, null);
                }
                catch
                {
                }

                if (isNull)
                {
                    property.SetValue(ClrInstance, null, null);
                }
                else
                {
                    switch (entityProperty.PropertyType)
                    {
                        case EdmType.String:
                            if (property.PropertyType != typeof(string))
                            {
                                continue;
                            }

                            property.SetValue(ClrInstance, entityProperty.StringValue, null);
                            break;
                        case EdmType.Binary:
                            if (property.PropertyType != typeof(byte[]))
                            {
                                continue;
                            }

                            property.SetValue(ClrInstance, entityProperty.BinaryValue, null);
                            break;
                        case EdmType.Boolean:
                            if (property.PropertyType != typeof(bool)
                                && property.PropertyType != typeof(bool?))
                            {
                                continue;
                            }

                            property.SetValue(ClrInstance, entityProperty.BooleanValue, null);
                            break;
                        case EdmType.DateTime:
                            if (property.PropertyType == typeof(DateTime))
                            {
                                property.SetValue(ClrInstance, entityProperty.DateTimeOffsetValue.Value.UtcDateTime, null);
                            }
                            else if (property.PropertyType == typeof(DateTime?))
                            {
                                property.SetValue(ClrInstance, entityProperty.DateTimeOffsetValue.HasValue ? entityProperty.DateTimeOffsetValue.Value.UtcDateTime : (DateTime?)null, null);
                            }
                            else if (property.PropertyType == typeof(DateTimeOffset))
                            {
                                property.SetValue(ClrInstance, entityProperty.DateTimeOffsetValue.Value, null);
                            }
                            else if (property.PropertyType == typeof(DateTimeOffset?))
                            {
                                property.SetValue(ClrInstance, entityProperty.DateTimeOffsetValue, null);
                            }

                            break;
                        case EdmType.Double:
                            if (property.PropertyType != typeof(double)
                                && property.PropertyType != typeof(double?))
                            {
                                continue;
                            }

                            property.SetValue(ClrInstance, entityProperty.DoubleValue, null);
                            break;
                        case EdmType.Guid:
                            if (property.PropertyType != typeof(Guid)
                                && property.PropertyType != typeof(Guid?))
                            {
                                continue;
                            }

                            property.SetValue(ClrInstance, entityProperty.GuidValue, null);
                            break;
                        case EdmType.Int32:
                            if (property.PropertyType != typeof(int)
                                && property.PropertyType != typeof(int?))
                            {
                                continue;
                            }

                            property.SetValue(ClrInstance, entityProperty.Int32Value, null);
                            break;
                        case EdmType.Int64:
                            if (property.PropertyType != typeof(long)
                                && property.PropertyType != typeof(long?))
                            {
                                continue;
                            }

                            property.SetValue(ClrInstance, entityProperty.Int64Value, null);
                            break;
                    }
                }
            }
        }

        public IDictionary<string, EntityProperty> WriteEntity(OperationContext operationContext)
        {
            var retVals = new Dictionary<string, EntityProperty>();

            foreach (var property in _clrProperties)
            {
                if (ShouldSkipProperty(property))
                {
                    continue;
                }

                var newProperty = EntityProperty.CreateEntityPropertyFromObject(property.GetValue(ClrInstance, null));

                // property will be null if unknown type
                if (newProperty != null)
                {
                    retVals.Add(property.Name, newProperty);
                }
            }
            return retVals;
        }

        private static bool ShouldSkipProperty(PropertyInfo property)
        {
            // reserved properties
            var propName = property.Name;
            if (propName == TableConstants.PartitionKey
                ||
                propName == TableConstants.RowKey
                ||
                propName == TableConstants.Timestamp
                ||
                propName == TableConstants.Etag)
            {
                return true;
            }

            var setter = property.GetSetMethod();
            var getter = property.GetGetMethod();

            // Enforce public getter / setter
            if (setter == null
                || !setter.IsPublic
                || getter == null
                || !getter.IsPublic)
            {
                return true;
            }

            // Skip static properties
            return setter.IsStatic;
        }
    }
}
