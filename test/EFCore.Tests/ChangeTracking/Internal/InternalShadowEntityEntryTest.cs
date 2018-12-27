// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.ChangeTracking.Internal
{
    public class InternalShadowEntityEntryTest : InternalEntityEntryTestBase
    {
        [Fact]
        public void Entity_is_null()
        {
            var model = BuildModel();
            var configuration = InMemoryTestHelpers.Instance.CreateContextServices(model);

            var entry = CreateInternalEntry(
                configuration,
                model.FindEntityType(typeof(SomeEntity).FullName),
                null,
                new ValueBuffer(new object[] { 1, "Kool" }));

            Assert.Null(entry.Entity);
        }

        protected override Model BuildModel()
        {
            var modelBuilder = new ModelBuilder(new ConventionSet());
            var model = modelBuilder.Model;

            var someSimpleEntityType = model.AddEntityType(typeof(SomeSimpleEntityBase).FullName);
            var simpleKeyProperty = someSimpleEntityType.AddProperty("Id", typeof(int));
            simpleKeyProperty.ValueGenerated = ValueGenerated.OnAdd;
            someSimpleEntityType.GetOrSetPrimaryKey(simpleKeyProperty);

            var someCompositeEntityType = model.AddEntityType(typeof(SomeCompositeEntityBase).FullName);
            var compositeKeyProperty1 = someCompositeEntityType.AddProperty("Id1", typeof(int));
            var compositeKeyProperty2 = someCompositeEntityType.AddProperty("Id2", typeof(string));
            compositeKeyProperty2.IsNullable = false;
            someCompositeEntityType.GetOrSetPrimaryKey(new[] { compositeKeyProperty1, compositeKeyProperty2 });

            var entityType1 = model.AddEntityType(typeof(SomeEntity).FullName);
            entityType1.BaseType = someSimpleEntityType;
            var property3 = entityType1.AddProperty("Name", typeof(string));
            property3.IsConcurrencyToken = true;
            property3.ValueGenerated = ValueGenerated.OnAdd;

            var entityType2 = model.AddEntityType(typeof(SomeDependentEntity).FullName);
            entityType2.BaseType = someCompositeEntityType;
            var fk = entityType2.AddProperty("SomeEntityId", typeof(int));
            entityType2.GetOrAddForeignKey(new[] { fk }, entityType1.FindPrimaryKey(), entityType1);
            // TODO: declare this on the derived type
            // #2611
            var justAProperty = someCompositeEntityType.AddProperty("JustAProperty", typeof(int));
            justAProperty.ValueGenerated = ValueGenerated.OnAdd;
            someCompositeEntityType.AddKey(justAProperty);

            var entityType3 = model.AddEntityType(typeof(FullNotificationEntity));
            entityType3.GetOrSetPrimaryKey(entityType3.AddProperty("Id", typeof(int)));
            var property6 = entityType3.AddProperty("Name", typeof(string));
            property6.IsConcurrencyToken = true;
            ((EntityType)entityType3).ChangeTrackingStrategy = ChangeTrackingStrategy.ChangingAndChangedNotifications;

            var entityType4 = model.AddEntityType(typeof(ChangedOnlyEntity));
            entityType4.GetOrSetPrimaryKey(entityType4.AddProperty("Id", typeof(int)));
            var property8 = entityType4.AddProperty("Name", typeof(string));
            property8.IsConcurrencyToken = true;
            ((EntityType)entityType4).ChangeTrackingStrategy = ChangeTrackingStrategy.ChangedNotifications;

            var entityType5 = model.AddEntityType(typeof(SomeMoreDependentEntity).FullName);
            entityType5.BaseType = someSimpleEntityType;
            var fk5a = entityType5.AddProperty("Fk1", typeof(int));
            var fk5b = entityType5.AddProperty("Fk2", typeof(string));
            entityType5.GetOrAddForeignKey(new[] { fk5a, fk5b }, entityType2.FindPrimaryKey(), entityType2);

            modelBuilder.Entity(
                typeof(OwnerClass).FullName, eb =>
                {
                    eb.Property<int>(nameof(OwnerClass.Id));
                    eb.HasKey(nameof(OwnerClass.Id));
                    var owned = eb.OwnsOne(typeof(OwnedClass).FullName, nameof(OwnerClass.Owned)).HasForeignKey("Id");
                    owned.HasKey("Id");
                    owned.Property<string>(nameof(OwnedClass.Value));
                });

            return (Model)model;
        }
    }
}
