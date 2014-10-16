// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Metadata;
using Microsoft.WindowsAzure.Storage.Table;

namespace Microsoft.Data.Entity.AzureTableStorage.Tests.Helpers
{
    public class TestStateEntry : StateEntry
    {
        private object _entity;
        private IEntityType _entityType;
        private readonly Dictionary<string, object> _propertyBag = new Dictionary<string, object>();

        private TestStateEntry()
        {
            _propertyBag["ETag"] = "*";
        }

        public override object Entity
        {
            get { return _entity; }
        }

        public override IEntityType EntityType
        {
            get { return _entityType; }
        }

        public override EntityState EntityState { get; set; }

        protected override object ReadPropertyValue(IPropertyBase property)
        {
            return _propertyBag[((IProperty)property).AzureTableStorage().Column];
        }

        protected override void WritePropertyValue(IPropertyBase property, object value)
        {
            _propertyBag[((IProperty)property).AzureTableStorage().Column] = value;
        }

        public override object this[IPropertyBase property]
        {
            get { return ReadPropertyValue(property); }
            set { WritePropertyValue(property, value); }
        }

        public static TestStateEntry Mock()
        {
            var entry = new TestStateEntry
                {
                    _entity = new TableEntity { ETag = "*" }
                }
                .WithType(typeof(TableEntity));

            return entry;
        }

        public TestStateEntry WithState(EntityState state)
        {
            EntityState = state;
            return this;
        }

        public TestStateEntry WithType(string name)
        {
            var e = new Model().AddEntityType(name);
            e.GetOrAddProperty("PartitionKey", typeof(string), shadowProperty: true);
            e.GetOrAddProperty("RowKey", typeof(string), shadowProperty: true);
            e.GetOrAddProperty("ETag", typeof(string), shadowProperty: true);
            e.GetOrAddProperty("Timestamp", typeof(DateTime), shadowProperty: true);
            _entityType = e;
            return this;
        }

        public TestStateEntry WithType(Type type)
        {
            var e = new Model().AddEntityType(type);
            e.GetOrAddProperty("PartitionKey", typeof(string));
            e.GetOrAddProperty("RowKey", typeof(string));
            e.GetOrAddProperty("ETag", typeof(string));
            e.GetOrAddProperty("Timestamp", typeof(DateTimeOffset));
            _entityType = e;
            return this;
        }

        public TestStateEntry WithEntityType(IEntityType entityType)
        {
            _entityType = entityType;
            return this;
        }

        public TestStateEntry WithProperty(string property, string value)
        {
            _propertyBag[property] = value;
            return this;
        }

        public TestStateEntry WithEntity(object entity)
        {
            _entity = entity;
            return this;
        }
    }
}
