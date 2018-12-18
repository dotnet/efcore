// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

// ReSharper disable UnusedMember.Local
// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Infrastructure
{
    public class ModelValidatorTest : ModelValidatorTestBase
    {
        [Fact]
        public virtual void Detects_filter_on_derived_type()
        {
            var model = new Model();
            var entityTypeA = model.AddEntityType(typeof(A));
            SetPrimaryKey(entityTypeA);
            var entityTypeD = model.AddEntityType(typeof(D));
            entityTypeD.HasBaseType(entityTypeA);

            entityTypeD.QueryFilter = (Expression<Func<D, bool>>)(_ => true);

            VerifyError(CoreStrings.BadFilterDerivedType(entityTypeD.QueryFilter, entityTypeD.DisplayName()), model);
        }

        [Fact]
        public virtual void Detects_shadow_entities()
        {
            var model = new Model();
            model.AddEntityType("A");

            VerifyError(CoreStrings.ShadowEntity("A"), model);
        }

        [Fact]
        public virtual void Passes_on_shadow_key_created_explicitly()
        {
            var model = new Model();
            var entityType = model.AddEntityType(typeof(A));
            SetPrimaryKey(entityType);
            var keyProperty = entityType.AddProperty("Key", typeof(int));
            entityType.AddKey(keyProperty);

            VerifyWarning(CoreStrings.LogShadowPropertyCreated.GenerateMessage("Key", "A"), model, LogLevel.Debug);
        }

        [Fact]
        public virtual void Passes_on_shadow_primary_key_created_by_convention_in_dependent_type()
        {
            var model = new Model();
            var entityType = model.AddEntityType(typeof(A));
            var keyProperty = entityType.AddProperty("Key", typeof(int), ConfigurationSource.Convention);
            entityType.SetPrimaryKey(keyProperty);

            VerifyWarning(CoreStrings.LogShadowPropertyCreated.GenerateMessage("Key", "A"), model, LogLevel.Debug);
        }

        [Fact]
        public virtual void Detects_shadow_key_referenced_by_foreign_key_by_convention()
        {
            var modelBuilder = new InternalModelBuilder(new Model());
            var dependentEntityBuilder = modelBuilder.Entity(typeof(SampleEntity), ConfigurationSource.Convention);
            dependentEntityBuilder.Property("Id", typeof(int), ConfigurationSource.Convention);
            dependentEntityBuilder.PrimaryKey(
                new List<string>
                {
                    "Id"
                }, ConfigurationSource.Convention);
            var principalEntityBuilder = modelBuilder.Entity(typeof(ReferencedEntity), ConfigurationSource.Convention);
            principalEntityBuilder.Property("Id", typeof(int), ConfigurationSource.Convention);
            principalEntityBuilder.PrimaryKey(
                new List<string>
                {
                    "Id"
                }, ConfigurationSource.Convention);

            dependentEntityBuilder.Property("Foo", typeof(string), ConfigurationSource.Convention);
            principalEntityBuilder.Property("ReferencedFoo", typeof(string), ConfigurationSource.Convention);
            dependentEntityBuilder.HasForeignKey(
                principalEntityBuilder,
                dependentEntityBuilder.GetOrCreateProperties(
                    new List<string>
                    {
                        "Foo"
                    }, ConfigurationSource.Convention),
                principalEntityBuilder.HasKey(new[] { "ReferencedFoo" }, ConfigurationSource.Convention).Metadata,
                ConfigurationSource.Convention);

            VerifyError(
                CoreStrings.ReferencedShadowKey(
                    typeof(SampleEntity).Name,
                    typeof(ReferencedEntity).Name,
                    "{'Foo' : string}",
                    "{'Id' : int}"),
                modelBuilder.Metadata);
        }

        [Fact]
        public virtual void Detects_a_null_primary_key()
        {
            var model = new Model();
            model.AddEntityType(typeof(A));

            VerifyError(CoreStrings.EntityRequiresKey(nameof(A)), model);
        }

        [Fact]
        public virtual void Detects_key_property_with_value_generated_on_update()
        {
            var model = new Model();
            var entityTypeA = model.AddEntityType(typeof(A));
            SetPrimaryKey(entityTypeA);
            entityTypeA.FindPrimaryKey().Properties.Single().ValueGenerated = ValueGenerated.OnUpdate;

            VerifyError(CoreStrings.MutableKeyProperty(nameof(A.Id)), model);
        }

        [Fact]
        public virtual void Detects_key_property_with_value_generated_on_add_or_update()
        {
            var model = new Model();
            var entityTypeA = model.AddEntityType(typeof(A));
            SetPrimaryKey(entityTypeA);
            entityTypeA.FindPrimaryKey().Properties.Single().ValueGenerated = ValueGenerated.OnAddOrUpdate;

            VerifyError(CoreStrings.MutableKeyProperty(nameof(A.Id)), model);
        }

        [Fact]
        public virtual void Detects_relationship_cycle()
        {
            var modelBuilder = CreateConventionalModelBuilder();

            modelBuilder.Entity<A>();
            modelBuilder.Entity<B>();
            modelBuilder.Entity<C>().HasBaseType((string)null);
            modelBuilder.Entity<A>().HasOne<B>().WithOne().IsRequired().HasForeignKey<A>(a => a.Id).HasPrincipalKey<B>(b => b.Id);
            modelBuilder.Entity<A>().HasOne<C>().WithOne().IsRequired().HasForeignKey<C>(a => a.Id).HasPrincipalKey<A>(b => b.Id);
            modelBuilder.Entity<C>().HasOne<B>().WithOne().IsRequired().HasForeignKey<B>(a => a.Id).HasPrincipalKey<C>(b => b.Id);

            VerifyError(
                CoreStrings.IdentifyingRelationshipCycle(nameof(A)),
                modelBuilder.Model);
        }

        [Fact]
        public virtual void Passes_on_redundant_foreign_key()
        {
            var modelBuilder = CreateConventionalModelBuilder();

            modelBuilder.Entity<A>().HasOne<A>().WithOne().IsRequired().HasForeignKey<A>(a => a.Id).HasPrincipalKey<A>(b => b.Id);

            VerifyWarning(CoreStrings.LogRedundantForeignKey.GenerateMessage("{'Id'}", "A"), modelBuilder.Model, LogLevel.Warning);
        }

        [Fact]
        public virtual void Passes_on_escapable_foreign_key_cycles()
        {
            var model = new Model();
            var entityA = model.AddEntityType(typeof(A));
            SetPrimaryKey(entityA);
            var keyA1 = CreateKey(entityA);
            var keyA2 = CreateKey(entityA, startingPropertyIndex: 0, propertyCount: 2);
            var entityB = model.AddEntityType(typeof(B));
            SetPrimaryKey(entityB);
            var keyB1 = CreateKey(entityB);
            var keyB2 = CreateKey(entityB, startingPropertyIndex: 1, propertyCount: 2);

            CreateForeignKey(keyA1, keyB1);
            CreateForeignKey(keyB1, keyA1);
            CreateForeignKey(keyA2, keyB2);

            Validate(model);
        }

        [Fact]
        public virtual void Passes_on_escapable_foreign_key_cycles_not_starting_at_hub()
        {
            var model = new Model();
            var entityA = model.AddEntityType(typeof(A));
            SetPrimaryKey(entityA);
            var keyA1 = CreateKey(entityA);
            var keyA2 = CreateKey(entityA, startingPropertyIndex: 1, propertyCount: 2);
            var entityB = model.AddEntityType(typeof(B));
            SetPrimaryKey(entityB);
            var keyB1 = CreateKey(entityB);
            var keyB2 = CreateKey(entityB, startingPropertyIndex: 0, propertyCount: 2);

            CreateForeignKey(keyA1, keyB1);
            CreateForeignKey(keyB1, keyA1);
            CreateForeignKey(keyB2, keyA2);

            Validate(model);
        }

        [Fact]
        public virtual void Passes_on_foreign_key_cycle_with_one_GenerateOnAdd()
        {
            var model = new Model();
            var entityA = model.AddEntityType(typeof(A));
            SetPrimaryKey(entityA);
            var keyA = CreateKey(entityA);
            var entityB = model.AddEntityType(typeof(B));
            SetPrimaryKey(entityB);
            var keyB = CreateKey(entityB);

            CreateForeignKey(keyA, keyB);
            CreateForeignKey(keyB, keyA);

            keyA.Properties[0].ValueGenerated = ValueGenerated.OnAdd;

            Validate(model);
        }

        [Fact]
        public virtual void Pases_on_double_reference_to_root_principal_property()
        {
            var model = new Model();
            var entityA = model.AddEntityType(typeof(A));
            SetPrimaryKey(entityA);
            var keyA1 = CreateKey(entityA);
            var keyA2 = CreateKey(entityA, startingPropertyIndex: 0, propertyCount: 2);
            var entityB = model.AddEntityType(typeof(B));
            SetPrimaryKey(entityB);
            var keyB1 = CreateKey(entityB);
            var keyB2 = CreateKey(entityB, startingPropertyIndex: 0, propertyCount: 2);

            CreateForeignKey(keyA1, keyB1);
            CreateForeignKey(keyA2, keyB2);

            Validate(model);
        }

        [Fact]
        public virtual void Pases_on_diamond_path_to_root_principal_property()
        {
            var model = new Model();
            var entityA = model.AddEntityType(typeof(A));
            SetPrimaryKey(entityA);
            var keyA1 = CreateKey(entityA);
            var keyA2 = CreateKey(entityA, startingPropertyIndex: 0, propertyCount: 2);
            var keyA3 = CreateKey(entityA);
            var keyA4 = CreateKey(entityA, startingPropertyIndex: 2, propertyCount: 2);
            var entityB = model.AddEntityType(typeof(B));
            SetPrimaryKey(entityB);
            var keyB1 = CreateKey(entityB);
            var keyB2 = CreateKey(entityB, startingPropertyIndex: 1, propertyCount: 2);

            CreateForeignKey(keyA1, keyB1);
            CreateForeignKey(keyA2, keyB2);

            CreateForeignKey(keyB1, keyA3);
            CreateForeignKey(keyB2, keyA4);

            Validate(model);
        }

        [Fact]
        public virtual void Pases_on_correct_inheritance()
        {
            var model = new Model();
            var entityA = model.AddEntityType(typeof(A));
            SetPrimaryKey(entityA);
            var entityD = model.AddEntityType(typeof(D));
            SetBaseType(entityD, entityA);

            Validate(model);
        }

        [Fact]
        public virtual void Detects_skipped_base_type()
        {
            var model = new Model();
            var entityA = model.AddEntityType(typeof(A));
            SetPrimaryKey(entityA);
            var entityD = model.AddEntityType(typeof(D));
            SetBaseType(entityD, entityA);
            var entityF = model.AddEntityType(typeof(F));
            SetBaseType(entityF, entityA);

            VerifyError(CoreStrings.InconsistentInheritance(nameof(F), nameof(D)), model);
        }

        [Fact]
        public virtual void Detects_abstract_leaf_type()
        {
            var model = new Model();
            var entityA = model.AddEntityType(typeof(A));
            SetPrimaryKey(entityA);
            var entityAbstract = model.AddEntityType(typeof(Abstract));
            SetBaseType(entityAbstract, entityA);

            VerifyError(CoreStrings.AbstractLeafEntityType(entityAbstract.DisplayName()), model);
        }

        [Fact]
        public virtual void Detects_generic_leaf_type()
        {
            var model = new Model();
            var entityAbstract = model.AddEntityType(typeof(Abstract));
            SetPrimaryKey(entityAbstract);
            var entityGeneric = model.AddEntityType(typeof(Generic<>));
            entityGeneric.HasBaseType(entityAbstract);

            VerifyError(CoreStrings.AbstractLeafEntityType(entityGeneric.DisplayName()), model);
        }

        [Fact]
        public virtual void Pases_on_valid_owned_entity_types()
        {
            var modelBuilder = new InternalModelBuilder(new Model());
            var entityTypeBuilder = modelBuilder.Entity(typeof(SampleEntity), ConfigurationSource.Convention);
            entityTypeBuilder.PrimaryKey(new[] { nameof(SampleEntity.Id) }, ConfigurationSource.Convention);
            var ownershipBuilder = entityTypeBuilder.Owns(
                typeof(ReferencedEntity), nameof(SampleEntity.ReferencedEntity), ConfigurationSource.Convention);
            var ownedTypeBuilder = ownershipBuilder.Metadata.DeclaringEntityType.Builder;
            ownedTypeBuilder.PrimaryKey(ownershipBuilder.Metadata.Properties.Select(p => p.Name).ToList(), ConfigurationSource.Convention);

            Validate(modelBuilder.Metadata);
        }

        [Fact]
        public virtual void Detects_weak_entity_type_without_defining_navigation()
        {
            var modelBuilder = new InternalModelBuilder(new Model());
            var entityTypeBuilder = modelBuilder.Entity(typeof(SampleEntity), ConfigurationSource.Convention);
            entityTypeBuilder.PrimaryKey(new[] { nameof(SampleEntity.Id) }, ConfigurationSource.Convention);

            var anotherEntityTypeBuilder = modelBuilder.Entity(typeof(AnotherSampleEntity), ConfigurationSource.Convention);
            anotherEntityTypeBuilder.PrimaryKey(new[] { nameof(AnotherSampleEntity.Id) }, ConfigurationSource.Convention);

            var anotherOwnershipBuilder = anotherEntityTypeBuilder.Owns(
                typeof(ReferencedEntity), nameof(AnotherSampleEntity.ReferencedEntity), ConfigurationSource.Convention);
            anotherOwnershipBuilder.Metadata.DeclaringEntityType.Builder.PrimaryKey(
                anotherOwnershipBuilder.Metadata.Properties.Select(p => p.Name).ToList(), ConfigurationSource.Convention);

            var ownershipBuilder = entityTypeBuilder.Owns(
                typeof(ReferencedEntity), nameof(SampleEntity.ReferencedEntity), ConfigurationSource.Convention);
            var ownedTypeBuilder = ownershipBuilder.Metadata.DeclaringEntityType.Builder;
            ownedTypeBuilder.PrimaryKey(ownershipBuilder.Metadata.Properties.Select(p => p.Name).ToList(), ConfigurationSource.Convention);

            entityTypeBuilder.Metadata.RemoveNavigation(nameof(SampleEntity.ReferencedEntity));

            VerifyError(
                CoreStrings.NoDefiningNavigation(
                    nameof(SampleEntity.ReferencedEntity),
                    nameof(SampleEntity) + "." + nameof(SampleEntity.ReferencedEntity) + "#" + nameof(ReferencedEntity),
                    nameof(SampleEntity)),
                modelBuilder.Metadata);
        }

        [Fact]
        public virtual void Detects_entity_type_with_multiple_ownerships()
        {
            var modelBuilder = new InternalModelBuilder(new Model());
            var entityTypeBuilder = modelBuilder.Entity(typeof(SampleEntity), ConfigurationSource.Convention);
            entityTypeBuilder.PrimaryKey(new[] { nameof(SampleEntity.Id) }, ConfigurationSource.Convention);
            var ownershipBuilder = entityTypeBuilder.Owns(
                typeof(ReferencedEntity), nameof(SampleEntity.ReferencedEntity), ConfigurationSource.Convention);
            var ownedTypeBuilder = ownershipBuilder.Metadata.DeclaringEntityType.Builder;
            ownedTypeBuilder.PrimaryKey(ownershipBuilder.Metadata.Properties.Select(p => p.Name).ToList(), ConfigurationSource.Convention);

            ownedTypeBuilder.Relationship(entityTypeBuilder, (string)null, null, ConfigurationSource.Convention, setTargetAsPrincipal: true)
                .Metadata.IsOwnership = true;

            VerifyError(
                CoreStrings.MultipleOwnerships(nameof(ReferencedEntity)),
                modelBuilder.Metadata);
        }

        [Fact]
        public virtual void Detects_weak_entity_type_with_non_defining_ownership()
        {
            var modelBuilder = new InternalModelBuilder(new Model());
            var entityTypeBuilder = modelBuilder.Entity(typeof(SampleEntity), ConfigurationSource.Convention);
            entityTypeBuilder.PrimaryKey(new[] { nameof(SampleEntity.Id) }, ConfigurationSource.Convention);

            var anotherEntityTypeBuilder = modelBuilder.Entity(typeof(AnotherSampleEntity), ConfigurationSource.Convention);
            anotherEntityTypeBuilder.PrimaryKey(new[] { nameof(AnotherSampleEntity.Id) }, ConfigurationSource.Convention);

            var anotherOwnershipBuilder = anotherEntityTypeBuilder.Owns(
                typeof(ReferencedEntity), nameof(AnotherSampleEntity.ReferencedEntity), ConfigurationSource.Convention);
            anotherOwnershipBuilder.Metadata.DeclaringEntityType.Builder.PrimaryKey(
                anotherOwnershipBuilder.Metadata.Properties.Select(p => p.Name).ToList(), ConfigurationSource.Convention);

            var ownershipBuilder = entityTypeBuilder.Owns(
                typeof(ReferencedEntity), nameof(SampleEntity.ReferencedEntity), ConfigurationSource.Convention);
            var ownedTypeBuilder = ownershipBuilder.Metadata.DeclaringEntityType.Builder;
            ownedTypeBuilder.PrimaryKey(ownershipBuilder.Metadata.Properties.Select(p => p.Name).ToList(), ConfigurationSource.Convention);

            ownershipBuilder.Metadata.IsOwnership = false;
            ownedTypeBuilder.Relationship(entityTypeBuilder, (string)null, null, ConfigurationSource.Convention, setTargetAsPrincipal: true)
                .Metadata.IsOwnership = true;

            VerifyError(
                CoreStrings.NonDefiningOwnership(
                    nameof(SampleEntity),
                    nameof(SampleEntity.ReferencedEntity),
                    nameof(SampleEntity) + "." + nameof(SampleEntity.ReferencedEntity) + "#" + nameof(ReferencedEntity)),
                modelBuilder.Metadata);
        }

        [Fact]
        public virtual void Detects_weak_entity_type_without_ownership()
        {
            var modelBuilder = new InternalModelBuilder(new Model());
            var entityTypeBuilder = modelBuilder.Entity(typeof(SampleEntity), ConfigurationSource.Convention);
            entityTypeBuilder.PrimaryKey(new[] { nameof(SampleEntity.Id) }, ConfigurationSource.Convention);
            var ownershipBuilder = entityTypeBuilder.Owns(
                typeof(ReferencedEntity), nameof(SampleEntity.ReferencedEntity), ConfigurationSource.Convention);
            var ownedTypeBuilder = ownershipBuilder.Metadata.DeclaringEntityType.Builder;
            ownedTypeBuilder.PrimaryKey(ownershipBuilder.Metadata.Properties.Select(p => p.Name).ToList(), ConfigurationSource.Convention);

            var anotherEntityTypeBuilder = modelBuilder.Entity(typeof(AnotherSampleEntity), ConfigurationSource.Convention);
            anotherEntityTypeBuilder.PrimaryKey(new[] { nameof(AnotherSampleEntity.Id) }, ConfigurationSource.Convention);

            var anotherOwnershipBuilder = anotherEntityTypeBuilder.Owns(
                typeof(ReferencedEntity), nameof(AnotherSampleEntity.ReferencedEntity), ConfigurationSource.Convention);
            anotherOwnershipBuilder.Metadata.DeclaringEntityType.Builder.PrimaryKey(
                anotherOwnershipBuilder.Metadata.Properties.Select(p => p.Name).ToList(), ConfigurationSource.Convention);
            anotherOwnershipBuilder.Metadata.IsOwnership = false;

            VerifyError(
                CoreStrings.InconsistentOwnership(
                    nameof(SampleEntity) + "." + nameof(SampleEntity.ReferencedEntity) + "#" + nameof(ReferencedEntity),
                    nameof(AnotherSampleEntity) + "." + nameof(AnotherSampleEntity.ReferencedEntity) + "#" + nameof(ReferencedEntity)),
                modelBuilder.Metadata);
        }

        [Fact]
        public virtual void Detects_principal_owned_entity_type()
        {
            var modelBuilder = new InternalModelBuilder(new Model());
            var entityTypeBuilder = modelBuilder.Entity(typeof(SampleEntity), ConfigurationSource.Convention);
            entityTypeBuilder.PrimaryKey(new[] { nameof(SampleEntity.Id) }, ConfigurationSource.Convention);
            var ownershipBuilder = entityTypeBuilder.Owns(
                typeof(ReferencedEntity), nameof(SampleEntity.ReferencedEntity), ConfigurationSource.Convention);
            var ownedTypeBuilder = ownershipBuilder.Metadata.DeclaringEntityType.Builder;
            ownedTypeBuilder.PrimaryKey(ownershipBuilder.Metadata.Properties.Select(p => p.Name).ToList(), ConfigurationSource.Convention);
            var anotherEntityTypeBuilder = modelBuilder.Entity(typeof(AnotherSampleEntity), ConfigurationSource.Convention);
            anotherEntityTypeBuilder.PrimaryKey(new[] { nameof(AnotherSampleEntity.Id) }, ConfigurationSource.Convention);
            anotherEntityTypeBuilder.Navigation(
                ownedTypeBuilder, nameof(AnotherSampleEntity.ReferencedEntity), ConfigurationSource.Convention,
                setTargetAsPrincipal: true);

            VerifyError(
                CoreStrings.PrincipalOwnedType(
                    nameof(AnotherSampleEntity) + "." + nameof(AnotherSampleEntity.ReferencedEntity),
                    nameof(ReferencedEntity),
                    nameof(ReferencedEntity)),
                modelBuilder.Metadata);
        }

        [Fact]
        public virtual void Detects_non_owner_navigation_to_owned_entity_type()
        {
            var modelBuilder = new InternalModelBuilder(new Model());
            var entityTypeBuilder = modelBuilder.Entity(typeof(SampleEntity), ConfigurationSource.Convention);
            entityTypeBuilder.PrimaryKey(new[] { nameof(SampleEntity.Id) }, ConfigurationSource.Convention);
            var ownershipBuilder = entityTypeBuilder.Owns(
                typeof(ReferencedEntity), nameof(SampleEntity.ReferencedEntity), ConfigurationSource.Convention);
            var ownedTypeBuilder = ownershipBuilder.Metadata.DeclaringEntityType.Builder;
            ownedTypeBuilder.PrimaryKey(ownershipBuilder.Metadata.Properties.Select(p => p.Name).ToList(), ConfigurationSource.Convention);
            var anotherEntityTypeBuilder = modelBuilder.Entity(typeof(AnotherSampleEntity), ConfigurationSource.Convention);
            anotherEntityTypeBuilder.PrimaryKey(new[] { nameof(AnotherSampleEntity.Id) }, ConfigurationSource.Convention);
            anotherEntityTypeBuilder.Navigation(ownedTypeBuilder, nameof(AnotherSampleEntity.ReferencedEntity), ConfigurationSource.Convention)
                .RelatedEntityTypes(anotherEntityTypeBuilder.Metadata, ownedTypeBuilder.Metadata, ConfigurationSource.Convention);

            VerifyError(
                CoreStrings.InverseToOwnedType(
                    nameof(AnotherSampleEntity), nameof(SampleEntity.ReferencedEntity), nameof(ReferencedEntity), nameof(SampleEntity)),
                modelBuilder.Metadata);
        }

        [Fact]
        public virtual void Detects_derived_owned_entity_type()
        {
            var modelBuilder = new InternalModelBuilder(new Model());
            var entityTypeBuilder = modelBuilder.Entity(typeof(B), ConfigurationSource.Convention);
            entityTypeBuilder.PrimaryKey(new[] { nameof(B.Id) }, ConfigurationSource.Convention);
            var ownershipBuilder = entityTypeBuilder.Owns(typeof(D), nameof(B.A), ConfigurationSource.Convention);
            var ownedTypeBuilder = ownershipBuilder.Metadata.DeclaringEntityType.Builder;
            ownedTypeBuilder.PrimaryKey(ownershipBuilder.Metadata.Properties.Select(p => p.Name).ToList(), ConfigurationSource.Convention);
            var anotherEntityTypeBuilder = modelBuilder.Entity(typeof(A), ConfigurationSource.Convention);
            anotherEntityTypeBuilder.PrimaryKey(new[] { nameof(A.Id) }, ConfigurationSource.Convention);
            ownedTypeBuilder.HasBaseType(typeof(A), ConfigurationSource.Convention);

            VerifyError(CoreStrings.OwnedDerivedType(nameof(D)), modelBuilder.Metadata);
        }

        [Fact]
        public virtual void Detects_owned_entity_type_without_ownership()
        {
            var modelBuilder = new InternalModelBuilder(new Model());
            modelBuilder.Entity(typeof(A), ConfigurationSource.Convention);
            modelBuilder.Owned(typeof(A), ConfigurationSource.Convention);

            VerifyError(CoreStrings.OwnerlessOwnedType(nameof(A)), modelBuilder.Metadata);
        }

        [Fact]
        public virtual void Detects_ForeignKey_on_inherited_generated_key_property()
        {
            var modelBuilder = CreateModelBuilder();
            modelBuilder.Entity<Abstract>().Property(e => e.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<Generic<int>>().HasOne<Abstract>().WithOne().HasForeignKey<Generic<int>>(e => e.Id);
            modelBuilder.Entity<Generic<string>>();

            VerifyError(
                CoreStrings.ForeignKeyPropertyInKey(
                    nameof(Abstract.Id),
                    "Generic<int>",
                    "{'" + nameof(Abstract.Id) + "'}",
                    nameof(Abstract)), modelBuilder.Model);
        }

        [Fact]
        public virtual void Passes_ForeignKey_on_inherited_generated_key_property_abstract_base()
        {
            var modelBuilder = CreateModelBuilder();
            modelBuilder.Entity<Abstract>().Property(e => e.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<Generic<int>>().HasOne<Abstract>().WithOne().HasForeignKey<Generic<int>>(e => e.Id);

            Validate(modelBuilder.Model);
        }

        [Fact]
        public virtual void Detects_ToQuery_on_derived_query_types()
        {
            var modelBuilder = CreateConventionalModelBuilder();
            var context = new DbContext(new DbContextOptions<DbContext>());
            modelBuilder.Query<Abstract>().ToQuery(() => context.Set<Abstract>());
            modelBuilder.Query<Generic<int>>().ToQuery(() => context.Set<Generic<int>>());

            VerifyError(
                CoreStrings.DerivedQueryTypeDefiningQuery("Generic<int>", nameof(Abstract)),
                modelBuilder.Model);
        }

        [Fact]
        public virtual void Detects_keys_on_query_types()
        {
            var modelBuilder = CreateConventionalModelBuilder();
            var context = new DbContext(new DbContextOptions<DbContext>());
            var queryType = modelBuilder.Query<A>().Metadata;

            if (queryType.GetKeys().Count() == 0)
            {
                queryType.AddKey(queryType.FindProperty(nameof(A.Id)));
            }

            VerifyError(
                CoreStrings.QueryTypeWithKey("{'Id'}", nameof(A)),
                modelBuilder.Model);
        }

        [Theory]
        [InlineData(ChangeTrackingStrategy.ChangedNotifications)]
        [InlineData(ChangeTrackingStrategy.ChangingAndChangedNotifications)]
        [InlineData(ChangeTrackingStrategy.ChangingAndChangedNotificationsWithOriginalValues)]
        public virtual void Detects_non_notifying_entities(ChangeTrackingStrategy changeTrackingStrategy)
        {
            var model = new Model();
            var entityType = model.AddEntityType(typeof(NonNotifyingEntity));
            var id = entityType.AddProperty("Id");
            entityType.SetPrimaryKey(id);

            model.ChangeTrackingStrategy = changeTrackingStrategy;

            VerifyError(
                CoreStrings.ChangeTrackingInterfaceMissing("NonNotifyingEntity", changeTrackingStrategy, "INotifyPropertyChanged"),
                model);
        }

        [Theory]
        [InlineData(ChangeTrackingStrategy.ChangingAndChangedNotifications)]
        [InlineData(ChangeTrackingStrategy.ChangingAndChangedNotificationsWithOriginalValues)]
        public virtual void Detects_changed_only_notifying_entities(ChangeTrackingStrategy changeTrackingStrategy)
        {
            var model = new Model();
            var entityType = model.AddEntityType(typeof(ChangedOnlyEntity));
            var id = entityType.AddProperty("Id");
            entityType.SetPrimaryKey(id);

            model.ChangeTrackingStrategy = changeTrackingStrategy;

            VerifyError(
                CoreStrings.ChangeTrackingInterfaceMissing("ChangedOnlyEntity", changeTrackingStrategy, "INotifyPropertyChanging"),
                model);
        }

        [Theory]
        [InlineData(ChangeTrackingStrategy.Snapshot)]
        [InlineData(ChangeTrackingStrategy.ChangedNotifications)]
        [InlineData(ChangeTrackingStrategy.ChangingAndChangedNotifications)]
        [InlineData(ChangeTrackingStrategy.ChangingAndChangedNotificationsWithOriginalValues)]
        public virtual void Passes_for_fully_notifying_entities(ChangeTrackingStrategy changeTrackingStrategy)
        {
            var model = new Model();
            var entityType = model.AddEntityType(typeof(FullNotificationEntity));
            var id = entityType.AddProperty("Id");
            entityType.SetPrimaryKey(id);

            model.ChangeTrackingStrategy = changeTrackingStrategy;

            Validate(model);
        }

        [Theory]
        [InlineData(ChangeTrackingStrategy.Snapshot)]
        [InlineData(ChangeTrackingStrategy.ChangedNotifications)]
        public virtual void Passes_for_changed_only_entities_with_snapshot_or_changed_only_tracking(ChangeTrackingStrategy changeTrackingStrategy)
        {
            var model = new Model();
            var entityType = model.AddEntityType(typeof(ChangedOnlyEntity));
            var id = entityType.AddProperty("Id");
            entityType.SetPrimaryKey(id);

            model.ChangeTrackingStrategy = changeTrackingStrategy;

            Validate(model);
        }

        [Fact]
        public virtual void Passes_for_non_notifying_entities_with_snapshot_tracking()
        {
            var model = new Model();
            var entityType = model.AddEntityType(typeof(NonNotifyingEntity));
            var id = entityType.AddProperty("Id");
            entityType.SetPrimaryKey(id);

            model.ChangeTrackingStrategy = ChangeTrackingStrategy.Snapshot;

            Validate(model);
        }

        [Fact]
        public virtual void Passes_for_valid_seeds()
        {
            var modelBuilder = CreateModelBuilder();
            modelBuilder.Entity<A>().HasData(
                new A
                {
                    Id = 1
                });
            modelBuilder.Entity<D>().HasData(
                new D
                {
                    Id = 2,
                    P0 = 3
                });

            Validate(modelBuilder.Model);
        }

        [Fact]
        public virtual void Passes_for_ignored_invalid_properties()
        {
            var modelBuilder = CreateModelBuilder();
            modelBuilder.Entity<EntityWithInvalidProperties>(eb =>
            {
                eb.Ignore(e => e.NotImplemented);

                eb.HasData(
                    new EntityWithInvalidProperties
                    {
                        Id = -1
                    });

                eb.HasData(
                    new {
                        Id = -2,
                        NotImplemented = true,
                        Static = 1,
                        WriteOnly = 1,
                        ReadOnly = 1,
                        PrivateGetter = 1
                    });
            });

            Validate(modelBuilder.Model);

            var data = modelBuilder.Model.GetEntityTypes().Single().GetData();
            Assert.Equal(-1, data.First().Values.Single());
            Assert.Equal(-2, data.Last().Values.Single());
        }

        [Fact]
        public virtual void Detects_derived_seeds()
        {
            var modelBuilder = CreateModelBuilder();

            Assert.Equal(
                CoreStrings.SeedDatumDerivedType(nameof(A), nameof(D)),
                Assert.Throws<InvalidOperationException>(
                    () => modelBuilder.Entity<A>().HasData(
                        new D
                        {
                            Id = 2,
                            P0 = 3
                        })).Message);
        }

        [Fact]
        public virtual void Detects_derived_seeds_for_owned_types()
        {
            var modelBuilder = CreateModelBuilder();

            Assert.Equal(
                CoreStrings.SeedDatumDerivedType(nameof(A), nameof(D)),
                Assert.Throws<InvalidOperationException>(
                    () => modelBuilder.Entity<B>()
                        .OwnsOne(
                            b => b.A, a => a.HasData(
                                new D
                                {
                                    Id = 2,
                                    P0 = 3
                                }))
                        .OwnsOne(b => b.AnotherA)).Message);
        }

        [Fact]
        public virtual void Detects_missing_required_values_in_seeds()
        {
            var modelBuilder = CreateModelBuilder();
            modelBuilder.Entity<A>(
                e =>
                {
                    e.Property(a => a.P0).IsRequired();
                    e.HasData(
                        new A
                        {
                            Id = 1
                        });
                });

            VerifyError(
                CoreStrings.SeedDatumMissingValue(nameof(A), nameof(A.P0)),
                modelBuilder.Model);
        }

        [Fact]
        public virtual void Passes_on_missing_required_store_generated_values_in_seeds()
        {
            var modelBuilder = CreateModelBuilder();
            modelBuilder.Entity<A>(
                e =>
                {
                    e.Property(a => a.P0).IsRequired().ValueGeneratedOnAddOrUpdate();
                    e.HasData(
                        new A
                        {
                            Id = 1
                        });
                });

            Validate(modelBuilder.Model);
        }

        [Fact]
        public virtual void Detects_missing_key_values_in_seeds()
        {
            var entity = new NonSignedIntegerKeyEntity();
            var modelBuilder = CreateModelBuilder();
            modelBuilder.Entity<NonSignedIntegerKeyEntity>(e => e.HasData(entity));

            Assert.Equal(ValueGenerated.OnAdd,
                modelBuilder.Model.FindEntityType(typeof(NonSignedIntegerKeyEntity)).FindProperty(nameof(NonSignedIntegerKeyEntity.Id)).ValueGenerated);
            VerifyError(
                CoreStrings.SeedDatumDefaultValue(nameof(NonSignedIntegerKeyEntity), nameof(NonSignedIntegerKeyEntity.Id), entity.Id),
                modelBuilder.Model);
        }

        [Fact]
        public virtual void Detects_missing_signed_integer_key_values_in_seeds()
        {
            var modelBuilder = CreateModelBuilder();
            modelBuilder.Entity<A>(e => e.HasData(new A()));

            VerifyError(
                CoreStrings.SeedDatumSignedNumericValue(nameof(A), nameof(A.Id)),
                modelBuilder.Model);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public virtual void Detects_duplicate_seeds(bool sensitiveDataLoggingEnabled)
        {
            ValidationLogger = CreateValidationLogger(sensitiveDataLoggingEnabled);
            var modelBuilder = CreateModelBuilder();
            modelBuilder.Entity<A>().HasData(
                new A
                {
                    Id = 1
                });
            modelBuilder.Entity<D>().HasData(
                new D
                {
                    Id = 1
                });

            VerifyError(
                sensitiveDataLoggingEnabled
                    ? CoreStrings.SeedDatumDuplicateSensitive(nameof(D), $"{nameof(A.Id)}:1")
                    : CoreStrings.SeedDatumDuplicate(nameof(D), $"{{'{nameof(A.Id)}'}}"),
                modelBuilder.Model);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public virtual void Detects_incompatible_values(bool sensitiveDataLoggingEnabled)
        {
            ValidationLogger = CreateValidationLogger(sensitiveDataLoggingEnabled);
            var modelBuilder = CreateModelBuilder();
            modelBuilder.Entity<A>(
                e =>
                {
                    e.HasData(
                        new
                        {
                            Id = 1,
                            P0 = "invalid"
                        });
                });

            VerifyError(
                sensitiveDataLoggingEnabled
                    ? CoreStrings.SeedDatumIncompatibleValueSensitive(nameof(A), "invalid", nameof(A.P0), "System.Nullable<int>")
                    : CoreStrings.SeedDatumIncompatibleValue(nameof(A), nameof(A.P0), "System.Nullable<int>"),
                modelBuilder.Model);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public virtual void Detects_reference_navigations_in_seeds(bool sensitiveDataLoggingEnabled)
        {
            ValidationLogger = CreateValidationLogger(sensitiveDataLoggingEnabled);
            var modelBuilder = CreateModelBuilder();
            modelBuilder.Entity<SampleEntity>(
                e =>
                {
                    e.HasData(
                        new SampleEntity
                        {
                            Id = 1,
                            ReferencedEntity = new ReferencedEntity
                            {
                                Id = 2
                            }
                        });
                });

            VerifyError(
                sensitiveDataLoggingEnabled
                    ? CoreStrings.SeedDatumNavigationSensitive(
                        nameof(SampleEntity),
                        $"{nameof(SampleEntity.Id)}:1",
                        nameof(SampleEntity.ReferencedEntity),
                        nameof(ReferencedEntity),
                        $"{{'{nameof(ReferencedEntity.SampleEntityId)}'}}")
                    : CoreStrings.SeedDatumNavigation(
                        nameof(SampleEntity),
                        nameof(SampleEntity.ReferencedEntity),
                        nameof(ReferencedEntity),
                        $"{{'{nameof(ReferencedEntity.SampleEntityId)}'}}"),
                modelBuilder.Model);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public virtual void Detects_collection_navigations_in_seeds(bool sensitiveDataLoggingEnabled)
        {
            ValidationLogger = CreateValidationLogger(sensitiveDataLoggingEnabled);
            var modelBuilder = CreateModelBuilder();
            modelBuilder.Entity<SampleEntity>(
                e =>
                {
                    e.HasData(
                        new SampleEntity
                        {
                            Id = 1,
                            OtherSamples = new HashSet<SampleEntity>(
                                new[]
                                {
                                    new SampleEntity
                                    {
                                        Id = 2
                                    }
                                })
                        });
                });

            VerifyError(
                sensitiveDataLoggingEnabled
                    ? CoreStrings.SeedDatumNavigationSensitive(
                        nameof(SampleEntity),
                        $"{nameof(SampleEntity.Id)}:1",
                        nameof(SampleEntity.OtherSamples),
                        nameof(SampleEntity),
                        "{'SampleEntityId'}")
                    : CoreStrings.SeedDatumNavigation(
                        nameof(SampleEntity),
                        nameof(SampleEntity.OtherSamples),
                        nameof(SampleEntity),
                        "{'SampleEntityId'}"),
                modelBuilder.Model);
        }

        private ModelBuilder CreateModelBuilder()
            => new ModelBuilder(
                InMemoryTestHelpers.Instance.CreateContextServices().GetRequiredService<ICoreConventionSetBuilder>().CreateConventionSet());

        // INotify interfaces not really implemented; just marking the classes to test metadata construction
        private class FullNotificationEntity : INotifyPropertyChanging, INotifyPropertyChanged
        {
            public int Id { get; set; }

#pragma warning disable 67
            public event PropertyChangingEventHandler PropertyChanging;
            public event PropertyChangedEventHandler PropertyChanged;
#pragma warning restore 67
        }

        // INotify interfaces not really implemented; just marking the classes to test metadata construction
        private class ChangedOnlyEntity : INotifyPropertyChanged
        {
            public int Id { get; set; }

#pragma warning disable 67
            public event PropertyChangedEventHandler PropertyChanged;
#pragma warning restore 67
        }

        private class NonNotifyingEntity
        {
            public int Id { get; set; }
        }

        protected override IModelValidator CreateModelValidator()
            => new ModelValidator(new ModelValidatorDependencies(ValidationLogger, ModelLogger));

        private class NonSignedIntegerKeyEntity
        {
            public uint Id { get; set; }
        }
    }
}
