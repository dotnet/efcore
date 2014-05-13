// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Metadata;
using Microsoft.WindowsAzure.Storage.Table;

namespace Microsoft.Data.Entity.AzureTableStorage.Tests.Helpers
{
    public class TestStateEntry : StateEntry
    {
        private object _entity;
        private IEntityType _entityType;
        private EntityState _entityState;

        public override object Entity
        {
            get { return _entity; }
        }

        public override IEntityType EntityType
        {
            get { return _entityType; }
        }

        public override EntityState EntityState { get; set; }

        protected override object ReadPropertyValue(IProperty property)
        {
            throw new NotImplementedException();
        }

        protected override void WritePropertyValue(IProperty property, object value)
        {
            throw new NotImplementedException();
        }

        public static TestStateEntry Mock()
        {
            var entry = new TestStateEntry
                {
                    _entity = new TableEntity { ETag = "*" }
                };
            return entry;
        }

        public TestStateEntry WithState(EntityState state)
        {
            EntityState = state;
            return this;
        }

        public TestStateEntry WithName(string name)
        {
            _entityType = new EntityType(name);
            return this;
        }
    }
}
