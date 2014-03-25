// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Metadata;
using Xunit;

namespace Microsoft.Data.Entity.Tests.ChangeTracking
{
    public class ShadowStateEntryTest : StateEntryTest
    {
        [Fact]
        public void Entity_is_null()
        {
            var model = BuildModel();
            var configuration = CreateConfiguration(model);

            var entry = CreateStateEntry(configuration, model.GetEntityType("SomeEntity"), (object)null);

            Assert.Null(entry.Entity);
        }

        protected override StateEntry CreateStateEntry(ContextConfiguration stateManager, IEntityType entityType, object entity)
        {
            return new ShadowStateEntry(stateManager, entityType);
        }

        protected override StateEntry CreateStateEntry(ContextConfiguration stateManager, IEntityType entityType, object[] valueBuffer)
        {
            return new ShadowStateEntry(stateManager, entityType, valueBuffer);
        }

        protected override IModel BuildModel()
        {
            var model = new Model();

            var entityType1 = new EntityType("SomeEntity");
            model.AddEntityType(entityType1);
            var key1 = entityType1.AddProperty("Id", typeof(int), shadowProperty: true);
            entityType1.SetKey(key1);
            entityType1.AddProperty("Kool", typeof(string), shadowProperty: true);

            var entityType2 = new EntityType("SomeDependentEntity");
            model.AddEntityType(entityType2);
            var key2 = entityType2.AddProperty("Id", typeof(int), shadowProperty: true);
            entityType2.SetKey(key2);
            var fk = entityType2.AddProperty("SomeEntityId", typeof(int), shadowProperty: true);
            entityType2.AddForeignKey(entityType1.GetKey(), new[] { fk });

            return model;
        }
    }
}
