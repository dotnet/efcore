// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Specification.Tests;
using Microsoft.EntityFrameworkCore.Storage;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Tests.ChangeTracking.Internal
{
    public class InternalShadowEntityEntryTest : InternalEntityEntryTestBase
    {
        [Fact]
        public void Entity_is_null()
        {
            var model = BuildModel();
            var configuration = TestHelpers.Instance.CreateContextServices(model);

            var entry = CreateInternalEntry(
                configuration,
                model.FindEntityType(typeof(SomeEntity).FullName),
                null,
                new ValueBuffer(new object[] { 1, "Kool" }));

            Assert.Null(entry.Entity);
        }

        protected override Model BuildModel()
        {
            var model = new Model();

            var someSimpleEntityType = model.AddEntityType(typeof(SomeSimpleEntityBase).FullName);
            var simpleKeyProperty = someSimpleEntityType.AddProperty("Id", typeof(int));
            simpleKeyProperty.RequiresValueGenerator = true;
            someSimpleEntityType.GetOrSetPrimaryKey(simpleKeyProperty);

            var someCompositeEntityType = model.AddEntityType(typeof(SomeCompositeEntityBase).FullName);
            var compositeKeyProperty1 = someCompositeEntityType.AddProperty("Id1", typeof(int));
            var compositeKeyProperty2 = someCompositeEntityType.AddProperty("Id2", typeof(string));
            compositeKeyProperty2.IsNullable = false;
            someCompositeEntityType.GetOrSetPrimaryKey(new[] { compositeKeyProperty1, compositeKeyProperty2 });

            var entityType1 = model.AddEntityType(typeof(SomeEntity).FullName);
            entityType1.HasBaseType(someSimpleEntityType);
            var property3 = entityType1.AddProperty("Name", typeof(string));
            property3.IsConcurrencyToken = true;

            var entityType2 = model.AddEntityType(typeof(SomeDependentEntity).FullName);
            entityType2.HasBaseType(someCompositeEntityType);
            var fk = entityType2.AddProperty("SomeEntityId", typeof(int));
            entityType2.GetOrAddForeignKey(new[] { fk }, entityType1.FindPrimaryKey(), entityType1);
            var justAProperty = entityType2.AddProperty("JustAProperty", typeof(int));
            justAProperty.RequiresValueGenerator = true;

            var entityType3 = model.AddEntityType(typeof(FullNotificationEntity));
            entityType3.GetOrSetPrimaryKey(entityType3.AddProperty("Id", typeof(int)));
            var property6 = entityType3.AddProperty("Name", typeof(string));
            property6.IsConcurrencyToken = true;
            entityType3.ChangeTrackingStrategy = ChangeTrackingStrategy.ChangingAndChangedNotifications;

            var entityType4 = model.AddEntityType(typeof(ChangedOnlyEntity));
            entityType4.GetOrSetPrimaryKey(entityType4.AddProperty("Id", typeof(int)));
            var property8 = entityType4.AddProperty("Name", typeof(string));
            property8.IsConcurrencyToken = true;
            entityType4.ChangeTrackingStrategy = ChangeTrackingStrategy.ChangedNotifications;

            var entityType5 = model.AddEntityType(typeof(SomeMoreDependentEntity).FullName);
            entityType5.HasBaseType(someSimpleEntityType);
            var fk5a = entityType5.AddProperty("Fk1", typeof(int));
            var fk5b = entityType5.AddProperty("Fk2", typeof(string));
            entityType5.GetOrAddForeignKey(new[] { fk5a, fk5b }, entityType2.FindPrimaryKey(), entityType2);

            return model;
        }
    }
}
