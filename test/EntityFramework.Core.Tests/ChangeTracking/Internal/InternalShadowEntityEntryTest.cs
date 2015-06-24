// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Storage;
using Xunit;

namespace Microsoft.Data.Entity.Tests.ChangeTracking.Internal
{
    public class InternalShadowEntityEntryTest : InternalEntityEntryTest
    {
        [Fact]
        public void Entity_is_null()
        {
            var model = BuildModel();
            var configuration = TestHelpers.Instance.CreateContextServices(model);

            var entry = CreateInternalEntry(
                 configuration,
                 model.GetEntityType(typeof(SomeEntity).FullName),
                 null,
                 new ValueBuffer(new object[] { 1, "Kool" }));

            Assert.Null(entry.Entity);
        }

        [Fact]
        public void Original_values_are_not_tracked_unless_needed_by_default_for_shadow_properties()
        {
            var model = BuildModel();
            var entityType = model.GetEntityType(typeof(SomeEntity).FullName);
            var idProperty = entityType.GetProperty("Id");
            var configuration = TestHelpers.Instance.CreateContextServices(model);

            var entry = CreateInternalEntry(configuration, entityType, new ValueBuffer(new object[] { 1, "Kool" }));

            Assert.Equal(
                Strings.OriginalValueNotTracked("Id", typeof(SomeEntity).FullName),
                Assert.Throws<InvalidOperationException>(() => entry.OriginalValues[idProperty] = 1).Message);

            Assert.Equal(
                Strings.OriginalValueNotTracked("Id", typeof(SomeEntity).FullName),
                Assert.Throws<InvalidOperationException>(() => entry.OriginalValues[idProperty]).Message);
        }

        protected override Model BuildModel()
        {
            var model = new Model();

            var someSimpleEntityType = model.AddEntityType(typeof(SomeSimpleEntityBase).FullName);
            var simpleKeyProperty = someSimpleEntityType.GetOrAddProperty("Id", typeof(int), shadowProperty: true);
            simpleKeyProperty.RequiresValueGenerator = true;
            someSimpleEntityType.GetOrSetPrimaryKey(simpleKeyProperty);

            var someCompositeEntityType = model.AddEntityType(typeof(SomeCompositeEntityBase).FullName);
            var compositeKeyProperty1 = someCompositeEntityType.GetOrAddProperty("Id1", typeof(int), shadowProperty: true);
            var compositeKeyProperty2 = someCompositeEntityType.GetOrAddProperty("Id2", typeof(string), shadowProperty: true);
            someCompositeEntityType.GetOrSetPrimaryKey(new[] { compositeKeyProperty1, compositeKeyProperty2 });

            var entityType1 = model.AddEntityType(typeof(SomeEntity).FullName);
            entityType1.BaseType = someSimpleEntityType;
            entityType1.GetOrAddProperty("Name", typeof(string), shadowProperty: true).IsConcurrencyToken = true;

            var entityType2 = model.AddEntityType(typeof(SomeDependentEntity).FullName);
            entityType2.BaseType = someCompositeEntityType;
            var fk = entityType2.GetOrAddProperty("SomeEntityId", typeof(int), shadowProperty: true);
            entityType2.GetOrAddForeignKey(new[] { fk }, entityType1.GetPrimaryKey(), entityType1);
            var justAProperty = entityType2.GetOrAddProperty("JustAProperty", typeof(int), shadowProperty: true);
            justAProperty.RequiresValueGenerator = true;

            var entityType3 = model.AddEntityType(typeof(FullNotificationEntity));
            entityType3.GetOrSetPrimaryKey(entityType3.GetOrAddProperty("Id", typeof(int), shadowProperty: true));
            entityType3.GetOrAddProperty("Name", typeof(string), shadowProperty: true).IsConcurrencyToken = true;

            var entityType4 = model.AddEntityType(typeof(ChangedOnlyEntity));
            entityType4.GetOrSetPrimaryKey(entityType4.GetOrAddProperty("Id", typeof(int), shadowProperty: true));
            entityType4.GetOrAddProperty("Name", typeof(string), shadowProperty: true).IsConcurrencyToken = true;

            var entityType5 = model.AddEntityType(typeof(SomeMoreDependentEntity).FullName);
            entityType5.BaseType = someSimpleEntityType;
            var fk5a = entityType5.GetOrAddProperty("Fk1", typeof(int), shadowProperty: true);
            var fk5b = entityType5.GetOrAddProperty("Fk2", typeof(string), shadowProperty: true);
            entityType5.GetOrAddForeignKey(new[] { fk5a, fk5b }, entityType2.GetPrimaryKey(), entityType2);

            return model;
        }
    }
}
