﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Diagnostics.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.Logging;
using Xunit;

// ReSharper disable UnusedMember.Local
// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Infrastructure
{
    public partial class ModelValidatorTest : ModelValidatorTestBase
    {
        [ConditionalFact]
        public virtual void Detects_key_property_which_cannot_be_compared()
        {
            var modelBuilder = CreateConventionalModelBuilder();

            modelBuilder.Entity<WithNonComparableKey>(eb =>
            {
                eb.Property(e => e.Id);
                eb.HasKey(e => e.Id);
            });

            VerifyError(
                CoreStrings.NonComparableKeyType(nameof(WithNonComparableKey), nameof(WithNonComparableKey.Id), nameof(NotComparable)),
                modelBuilder);
        }

        protected class WithNonComparableKey
        {
            public NotComparable Id { get; set; }
        }

        [ConditionalFact]
        public virtual void Detects_unique_index_property_which_cannot_be_compared()
        {
            var modelBuilder = CreateConventionalModelBuilder();

            modelBuilder.Entity<WithNonComparableUniqueIndex>(eb =>
            {
                eb.HasIndex(e => e.Index).IsUnique();
            });

            VerifyError(
                CoreStrings.NonComparableKeyType(
                    nameof(WithNonComparableUniqueIndex), nameof(WithNonComparableUniqueIndex.Index), nameof(NotComparable)),
                modelBuilder);
        }

        protected class WithNonComparableUniqueIndex
        {
            public int Id { get; set; }
            public NotComparable Index { get; set; }
        }

        [ConditionalFact]
        public virtual void Ignores_normal_property_which_cannot_be_compared()
        {
            var modelBuilder = CreateConventionalModelBuilder();

            modelBuilder.Entity<WithNonComparableNormalProperty>(eb =>
            {
                eb.Property(e => e.Id);
                eb.HasKey(e => e.Id);
                eb.Property(e => e.Foo);
            });

            Validate(modelBuilder);
        }

        protected class WithNonComparableNormalProperty
        {
            public int Id { get; set; }
            public NotComparable Foo { get; set; }
        }

        protected struct NotComparable
        {
        }

        [ConditionalFact]
        public virtual void Detects_custom_converter_for_collection_type_without_comparer()
        {
            var modelBuilder = CreateConventionalModelBuilder();

            IMutableProperty convertedProperty = null;
            modelBuilder.Entity<WithCollectionConversion>(eb =>
            {
                eb.Property(e => e.Id);
                convertedProperty = eb.Property(e => e.SomeStrings).Metadata;
                convertedProperty.SetValueConverter(
                new ValueConverter<string[], string>(
                    v => string.Join(',', v),
                    v => v.Split(',', StringSplitOptions.None)));
            });

            VerifyWarning(
                CoreResources.LogCollectionWithoutComparer(
                    new TestLogger<TestLoggingDefinitions>()).GenerateMessage("WithCollectionConversion", "SomeStrings"),
                modelBuilder);
        }

        [ConditionalFact]
        public virtual void Ignores_custom_converter_for_collection_type_with_comparer()
        {
            var modelBuilder = CreateConventionalModelBuilder();

            IMutableProperty convertedProperty = null;
            modelBuilder.Entity<WithCollectionConversion>(eb =>
            {
                eb.Property(e => e.Id);
                convertedProperty = eb.Property(e => e.SomeStrings).Metadata;
                convertedProperty.SetValueConverter(
                new ValueConverter<string[], string>(
                    v => string.Join(',', v),
                    v => v.Split(',', StringSplitOptions.None)));
            });

            convertedProperty.SetValueComparer(
                new ValueComparer<string[]>(
                    (v1, v2) => v1.SequenceEqual(v2),
                    v => v.GetHashCode()));

            Validate(modelBuilder);

            Assert.Empty(LoggerFactory.Log.Where(l => l.Level == LogLevel.Warning));
        }

        protected class WithCollectionConversion
        {
            public int Id { get; set; }
            public string[] SomeStrings { get; set; }
        }

        [ConditionalFact]
        public virtual void Ignores_binary_keys_and_strings_without_custom_comparer()
        {
            var modelBuilder = CreateConventionlessModelBuilder();
            var model = modelBuilder.Model;

            var entityType = model.AddEntityType(typeof(WithStringAndBinaryKey));

            var keyProperty = entityType.AddProperty(nameof(WithStringAndBinaryKey.Id), typeof(byte[]));
            keyProperty.IsNullable = false;
            entityType.SetPrimaryKey(keyProperty);
            keyProperty.SetValueConverter(
                new ValueConverter<byte[], byte[]>(v => v, v => v));

            var stringProperty = entityType.AddProperty(nameof(WithStringAndBinaryKey.AString), typeof(string));
            stringProperty.SetValueConverter(
                new ValueConverter<string, string>(v => v, v => v));

            Validate(modelBuilder);

            Assert.Empty(LoggerFactory.Log.Where(l => l.Level == LogLevel.Warning));
        }

        protected class WithStringAndBinaryKey
        {
            public byte[] Id { get; set; }
            public string AString { get; set; }
        }

        [ConditionalFact]
        public virtual void Detects_filter_on_derived_type()
        {
            var modelBuilder = CreateConventionalModelBuilder();
            var entityTypeA = modelBuilder.Entity<A>().Metadata;
            var entityTypeD = modelBuilder.Entity<D>().Metadata;

            entityTypeD.SetQueryFilter((Expression<Func<D, bool>>)(_ => true));

            VerifyError(CoreStrings.BadFilterDerivedType(entityTypeD.GetQueryFilter(), entityTypeD.DisplayName(), entityTypeA.DisplayName()),
                modelBuilder);
        }

        [ConditionalFact]
        public virtual void Detects_filter_on_owned_type()
        {
            var modelBuilder = CreateConventionalModelBuilder();
            var queryFilter = (Expression<Func<ReferencedEntity, bool>>)(_ => true);
            modelBuilder.Entity<SampleEntity>()
                .OwnsOne(
                    s => s.ReferencedEntity, eb =>
                    {
                        eb.OwnedEntityType.SetQueryFilter(queryFilter);
                    });

            VerifyError(CoreStrings.BadFilterOwnedType(queryFilter, nameof(ReferencedEntity)), modelBuilder);
        }

        [ConditionalFact]
        public virtual void Passes_on_shadow_key_created_explicitly()
        {
            var modelBuilder = CreateConventionlessModelBuilder();
            var model = modelBuilder.Model;

            var entityType = model.AddEntityType(typeof(A));
            SetPrimaryKey(entityType);
            AddProperties(entityType);

            var keyProperty = ((IConventionEntityType)entityType).AddProperty("Key", typeof(int));
            ((IConventionEntityType)entityType).AddKey(keyProperty);

            VerifyWarning(
                CoreResources.LogShadowPropertyCreated(new TestLogger<TestLoggingDefinitions>()).GenerateMessage("A", "Key"), modelBuilder,
                LogLevel.Debug);
        }

        [ConditionalFact]
        public virtual void Passes_on_shadow_primary_key_created_by_convention_in_dependent_type()
        {
            var modelBuilder = CreateConventionlessModelBuilder();
            var model = (IConventionModel)modelBuilder.Model;

            var entityType = model.AddEntityType(typeof(A));
            AddProperties((IMutableEntityType)entityType);
            entityType.AddProperty(nameof(A.Id), typeof(int));

            var keyProperty = entityType.AddProperty("Key", typeof(int));
            entityType.SetPrimaryKey(keyProperty);

            VerifyWarning(
                CoreResources.LogShadowPropertyCreated(new TestLogger<TestLoggingDefinitions>())
                    .GenerateMessage("A", "Key"), modelBuilder, LogLevel.Debug);
        }

        [ConditionalFact]
        public virtual void Detects_shadow_key_referenced_by_foreign_key_by_convention()
        {
            var builder = CreateConventionlessModelBuilder();
            var modelBuilder = (InternalModelBuilder)builder.GetInfrastructure();
            var dependentEntityBuilder = modelBuilder.Entity(typeof(SampleEntityMinimal), ConfigurationSource.Convention);
            dependentEntityBuilder.Property(typeof(int), "Id", ConfigurationSource.Convention);
            dependentEntityBuilder.Ignore(nameof(SampleEntityMinimal.ReferencedEntity), ConfigurationSource.Explicit);

            dependentEntityBuilder.PrimaryKey(
                new List<string> { "Id" }, ConfigurationSource.Convention);

            var principalEntityBuilder = modelBuilder.Entity(typeof(ReferencedEntityMinimal), ConfigurationSource.Convention);
            principalEntityBuilder.Property(typeof(int), "Id", ConfigurationSource.Convention);
            principalEntityBuilder.PrimaryKey(
                new List<string> { "Id" }, ConfigurationSource.Convention);

            dependentEntityBuilder.Property(typeof(string), "Foo", ConfigurationSource.Convention);
            principalEntityBuilder.Property(typeof(string), "ReferencedFoo", ConfigurationSource.Convention);

            dependentEntityBuilder.HasRelationship(
                principalEntityBuilder.Metadata,
                dependentEntityBuilder.GetOrCreateProperties(
                    new List<string> { "Foo" }, ConfigurationSource.Convention),
                principalEntityBuilder.HasKey(new[] { "ReferencedFoo" }, ConfigurationSource.Convention).Metadata,
                ConfigurationSource.Convention);

            VerifyError(
                CoreStrings.ReferencedShadowKey(
                    typeof(SampleEntityMinimal).Name,
                    typeof(ReferencedEntityMinimal).Name,
                    "{'Foo' : string}",
                    "{'Id' : int}"),
                builder);
        }

        [ConditionalFact]
        public virtual void Detects_a_null_primary_key()
        {
            var modelBuilder = CreateConventionlessModelBuilder();
            modelBuilder.Entity<A>(
                b =>
                {
                    b.Property(e => e.Id);
                    b.Property(e => e.P0);
                    b.Property(e => e.P1);
                    b.Property(e => e.P2);
                    b.Property(e => e.P3);
                });

            VerifyError(CoreStrings.EntityRequiresKey(nameof(A)), modelBuilder);
        }

        [ConditionalFact]
        public virtual void Detects_key_property_with_value_generated_on_update()
        {
            var modelBuilder = CreateConventionlessModelBuilder();
            var model = modelBuilder.Model;
            var entityTypeA = model.AddEntityType(typeof(A));
            SetPrimaryKey(entityTypeA);
            AddProperties(entityTypeA);
            entityTypeA.FindPrimaryKey().Properties.Single().ValueGenerated = ValueGenerated.OnUpdate;

            VerifyError(CoreStrings.MutableKeyProperty(nameof(A.Id)), modelBuilder);
        }

        [ConditionalFact]
        public virtual void Detects_key_property_with_value_generated_on_add_or_update()
        {
            var modelBuilder = CreateConventionlessModelBuilder();
            var model = modelBuilder.Model;

            var entityTypeA = model.AddEntityType(typeof(A));
            SetPrimaryKey(entityTypeA);
            AddProperties(entityTypeA);
            entityTypeA.FindPrimaryKey().Properties.Single().ValueGenerated = ValueGenerated.OnAddOrUpdate;

            VerifyError(CoreStrings.MutableKeyProperty(nameof(A.Id)), modelBuilder);
        }

        [ConditionalFact]
        public virtual void Detects_relationship_cycle()
        {
            var modelBuilder = base.CreateConventionalModelBuilder();

            modelBuilder.Entity<A>();
            modelBuilder.Entity<B>();
            modelBuilder.Entity<C>().HasBaseType((string)null);
            modelBuilder.Entity<A>().HasOne<B>().WithOne().HasForeignKey<A>(a => a.Id).HasPrincipalKey<B>(b => b.Id).IsRequired();
            modelBuilder.Entity<A>().HasOne<C>().WithOne().HasForeignKey<C>(a => a.Id).HasPrincipalKey<A>(b => b.Id).IsRequired();
            modelBuilder.Entity<C>().HasOne<B>().WithOne().HasForeignKey<B>(a => a.Id).HasPrincipalKey<C>(b => b.Id).IsRequired();

            VerifyError(
                CoreStrings.IdentifyingRelationshipCycle("A -> B -> C"),
                modelBuilder);
        }

        [ConditionalFact]
        public virtual void Passes_on_multiple_relationship_paths()
        {
            var modelBuilder = base.CreateConventionalModelBuilder();

            modelBuilder.Entity<A>();
            modelBuilder.Entity<B>();
            modelBuilder.Entity<C>().HasBaseType((string)null);
            modelBuilder.Entity<A>().HasOne<B>().WithOne().HasForeignKey<A>(a => a.Id).HasPrincipalKey<B>(b => b.Id).IsRequired();
            modelBuilder.Entity<A>().HasOne<C>().WithOne().HasForeignKey<A>(a => a.Id).HasPrincipalKey<C>(b => b.Id).IsRequired();
            modelBuilder.Entity<C>().HasOne<B>().WithOne().HasForeignKey<B>(a => a.Id).HasPrincipalKey<C>(b => b.Id).IsRequired();

            Validate(modelBuilder);
        }

        [ConditionalFact]
        public virtual void Passes_on_redundant_foreign_key()
        {
            var modelBuilder = base.CreateConventionalModelBuilder();

            modelBuilder.Entity<A>().HasOne<A>().WithOne().IsRequired().HasForeignKey<A>(a => a.Id).HasPrincipalKey<A>(b => b.Id);

            VerifyWarning(
                CoreResources.LogRedundantForeignKey(new TestLogger<TestLoggingDefinitions>()).GenerateMessage("{'Id'}", "A"),
                modelBuilder);
        }

        [ConditionalFact]
        public virtual void Passes_on_escapable_foreign_key_cycles()
        {
            var modelBuilder = CreateConventionlessModelBuilder();
            var model = modelBuilder.Model;

            var entityA = model.AddEntityType(typeof(A));
            SetPrimaryKey(entityA);
            AddProperties(entityA);
            var keyA1 = CreateKey(entityA);
            var keyA2 = CreateKey(entityA, startingPropertyIndex: 0, propertyCount: 2);

            var entityB = model.AddEntityType(typeof(B));
            SetPrimaryKey(entityB);
            AddProperties(entityB);

            entityB.AddIgnored(nameof(B.A));
            entityB.AddIgnored(nameof(B.AnotherA));
            entityB.AddIgnored(nameof(B.ManyAs));

            var keyB1 = CreateKey(entityB);
            var keyB2 = CreateKey(entityB, startingPropertyIndex: 1, propertyCount: 2);

            CreateForeignKey(keyA1, keyB1);
            CreateForeignKey(keyB1, keyA1);
            CreateForeignKey(keyA2, keyB2);

            Validate(modelBuilder);
        }

        [ConditionalFact]
        public virtual void Passes_on_escapable_foreign_key_cycles_not_starting_at_hub()
        {
            var modelBuilder = CreateConventionlessModelBuilder();
            var model = modelBuilder.Model;

            var entityA = model.AddEntityType(typeof(A));
            SetPrimaryKey(entityA);
            AddProperties(entityA);

            var keyA1 = CreateKey(entityA);
            var keyA2 = CreateKey(entityA, startingPropertyIndex: 1, propertyCount: 2);

            var entityB = model.AddEntityType(typeof(B));
            SetPrimaryKey(entityB);
            AddProperties(entityB);

            entityB.AddIgnored(nameof(B.A));
            entityB.AddIgnored(nameof(B.AnotherA));
            entityB.AddIgnored(nameof(B.ManyAs));

            var keyB1 = CreateKey(entityB);
            var keyB2 = CreateKey(entityB, startingPropertyIndex: 0, propertyCount: 2);

            CreateForeignKey(keyA1, keyB1);
            CreateForeignKey(keyB1, keyA1);
            CreateForeignKey(keyB2, keyA2);

            Validate(modelBuilder);
        }

        [ConditionalFact]
        public virtual void Passes_on_foreign_key_cycle_with_one_GenerateOnAdd()
        {
            var modelBuilder = CreateConventionlessModelBuilder();
            var model = modelBuilder.Model;

            var entityA = model.AddEntityType(typeof(A));
            SetPrimaryKey(entityA);
            AddProperties(entityA);

            var keyA = CreateKey(entityA);

            var entityB = model.AddEntityType(typeof(B));
            AddProperties(entityB);
            SetPrimaryKey(entityB);

            entityB.AddIgnored(nameof(B.A));
            entityB.AddIgnored(nameof(B.AnotherA));
            entityB.AddIgnored(nameof(B.ManyAs));

            var keyB = CreateKey(entityB);

            CreateForeignKey(keyA, keyB);
            CreateForeignKey(keyB, keyA);

            keyA.Properties[0].ValueGenerated = ValueGenerated.OnAdd;

            Validate(modelBuilder);
        }

        [ConditionalFact]
        public virtual void Passes_on_double_reference_to_root_principal_property()
        {
            var modelBuilder = CreateConventionlessModelBuilder();
            var model = modelBuilder.Model;

            var entityA = model.AddEntityType(typeof(A));
            SetPrimaryKey(entityA);
            AddProperties(entityA);

            var keyA1 = CreateKey(entityA);
            var keyA2 = CreateKey(entityA, startingPropertyIndex: 0, propertyCount: 2);

            var entityB = model.AddEntityType(typeof(B));
            SetPrimaryKey(entityB);
            AddProperties(entityB);

            entityB.AddIgnored(nameof(B.A));
            entityB.AddIgnored(nameof(B.AnotherA));
            entityB.AddIgnored(nameof(B.ManyAs));

            var keyB1 = CreateKey(entityB);
            var keyB2 = CreateKey(entityB, startingPropertyIndex: 0, propertyCount: 2);

            CreateForeignKey(keyA1, keyB1);
            CreateForeignKey(keyA2, keyB2);

            Validate(modelBuilder);
        }

        [ConditionalFact]
        public virtual void Passes_on_diamond_path_to_root_principal_property()
        {
            var modelBuilder = CreateConventionlessModelBuilder();
            var model = modelBuilder.Model;

            var entityA = model.AddEntityType(typeof(A));
            SetPrimaryKey(entityA);
            AddProperties(entityA);

            var keyA1 = CreateKey(entityA);
            var keyA2 = CreateKey(entityA, startingPropertyIndex: 0, propertyCount: 2);
            var keyA3 = CreateKey(entityA);
            var keyA4 = CreateKey(entityA, startingPropertyIndex: 2, propertyCount: 2);

            var entityB = model.AddEntityType(typeof(B));

            SetPrimaryKey(entityB);
            AddProperties(entityB);
            entityB.AddIgnored(nameof(B.A));
            entityB.AddIgnored(nameof(B.AnotherA));
            entityB.AddIgnored(nameof(B.ManyAs));

            var keyB1 = CreateKey(entityB);
            var keyB2 = CreateKey(entityB, startingPropertyIndex: 1, propertyCount: 2);

            CreateForeignKey(keyA1, keyB1);
            CreateForeignKey(keyA2, keyB2);

            CreateForeignKey(keyB1, keyA3);
            CreateForeignKey(keyB2, keyA4);

            Validate(modelBuilder);
        }

        [ConditionalFact]
        public virtual void Passes_on_correct_inheritance()
        {
            var modelBuilder = CreateConventionalModelBuilder();
            modelBuilder.Entity<A>();
            modelBuilder.Entity<D>();

            Validate(modelBuilder);
        }

        [ConditionalFact]
        public virtual void Detects_skipped_base_type()
        {
            var modelBuilder = CreateConventionlessModelBuilder();
            var model = modelBuilder.Model;

            var entityA = model.AddEntityType(typeof(A));
            SetPrimaryKey(entityA);
            AddProperties(entityA);

            var entityD = modelBuilder.Entity<D>();
            entityD.HasBaseType<A>();

            var entityF = modelBuilder.Entity<F>();
            entityF.HasBaseType<A>();

            VerifyError(CoreStrings.InconsistentInheritance(nameof(F), nameof(A), nameof(D)), modelBuilder);
        }

        [ConditionalFact]
        public virtual void Detects_abstract_leaf_type()
        {
            var modelBuilder = CreateConventionlessModelBuilder();
            var model = modelBuilder.Model;

            var entityA = model.AddEntityType(typeof(A));
            SetPrimaryKey(entityA);
            AddProperties(entityA);

            var entityAbstract = model.AddEntityType(typeof(Abstract));
            SetBaseType(entityAbstract, entityA);

            VerifyError(CoreStrings.AbstractLeafEntityType(entityAbstract.DisplayName()), modelBuilder);
        }

        [ConditionalFact]
        public virtual void Detects_generic_leaf_type()
        {
            var modelBuilder = CreateConventionlessModelBuilder();
            var model = modelBuilder.Model;

            var entityAbstract = model.AddEntityType(typeof(Abstract));
            SetPrimaryKey(entityAbstract);
            AddProperties(entityAbstract);

            var entityGeneric = model.AddEntityType(typeof(Generic<>));
            SetBaseType(entityGeneric, entityAbstract);

            VerifyError(CoreStrings.AbstractLeafEntityType(entityGeneric.DisplayName()), modelBuilder);
        }

        [ConditionalFact]
        public virtual void Passes_on_valid_many_to_many_navigations()
        {
            var modelBuilder = CreateConventionalModelBuilder();

            var model = modelBuilder.Model;
            var orderProductEntity = model.AddEntityType(typeof(OrderProduct));
            var orderEntity = model.FindEntityType(typeof(Order));
            var productEntity = model.FindEntityType(typeof(Product));
            var orderProductForeignKey = orderProductEntity
                .GetForeignKeys().Single(fk => fk.PrincipalEntityType == orderEntity);
            var productOrderForeignKey = orderProductEntity
                .GetForeignKeys().Single(fk => fk.PrincipalEntityType == productEntity);
            orderProductEntity.SetPrimaryKey(
                new[] { orderProductForeignKey.Properties.Single(), productOrderForeignKey.Properties.Single() });

            var productsNavigation = orderEntity.AddSkipNavigation(
                nameof(Order.Products), null, productEntity, true, false);
            productsNavigation.SetForeignKey(orderProductForeignKey);

            var ordersNavigation = productEntity.AddSkipNavigation(
                nameof(Product.Orders), null, orderEntity, true, false);
            ordersNavigation.SetForeignKey(productOrderForeignKey);

            productsNavigation.SetInverse(ordersNavigation);
            ordersNavigation.SetInverse(productsNavigation);

            Validate(modelBuilder);
        }

        [ConditionalFact]
        public virtual void Detects_missing_foreign_key_for_skip_navigations()
        {
            var modelBuilder = CreateConventionalModelBuilder();

            var model = modelBuilder.Model;
            var orderProductEntity = model.AddEntityType(typeof(OrderProduct));
            var orderEntity = model.FindEntityType(typeof(Order));
            var productEntity = model.FindEntityType(typeof(Product));
            var orderProductForeignKey = orderProductEntity
                .GetForeignKeys().Single(fk => fk.PrincipalEntityType == orderEntity);
            var productOrderForeignKey = orderProductEntity
                .GetForeignKeys().Single(fk => fk.PrincipalEntityType == productEntity);
            orderProductEntity.SetPrimaryKey(
                new[] { orderProductForeignKey.Properties.Single(), productOrderForeignKey.Properties.Single() });

            var productsNavigation = orderEntity.AddSkipNavigation(
                nameof(Order.Products), null, productEntity, true, false);

            VerifyError(
                CoreStrings.SkipNavigationNoForeignKey(nameof(Order.Products), nameof(Order)),
                modelBuilder);
        }

        [ConditionalFact]
        public virtual void Detects_missing_inverse_skip_navigations()
        {
            var modelBuilder = CreateConventionalModelBuilder();

            var model = modelBuilder.Model;
            var orderProductEntity = model.AddEntityType(typeof(OrderProduct));
            var orderEntity = model.FindEntityType(typeof(Order));
            var productEntity = model.FindEntityType(typeof(Product));
            var orderProductForeignKey = orderProductEntity
                .GetForeignKeys().Single(fk => fk.PrincipalEntityType == orderEntity);
            var productOrderForeignKey = orderProductEntity
                .GetForeignKeys().Single(fk => fk.PrincipalEntityType == productEntity);
            orderProductEntity.SetPrimaryKey(
                new[] { orderProductForeignKey.Properties.Single(), productOrderForeignKey.Properties.Single() });

            var productsNavigation = orderEntity.AddSkipNavigation(
                nameof(Order.Products), null, productEntity, true, false);
            productsNavigation.SetForeignKey(orderProductForeignKey);

            VerifyError(
                CoreStrings.SkipNavigationNoInverse(nameof(Order.Products), nameof(Order)),
                modelBuilder);
        }

        [ConditionalFact]
        public virtual void Detects_nonCollection_skip_navigations()
        {
            var modelBuilder = CreateConventionalModelBuilder();

            var model = modelBuilder.Model;
            var customerEntity = model.AddEntityType(typeof(Customer));
            var orderEntity = model.FindEntityType(typeof(Order));
            var orderDetailsEntity = model.FindEntityType(typeof(OrderDetails));
            new EntityTypeBuilder<OrderDetails>(orderDetailsEntity).Ignore(e => e.Customer);

            var productsNavigation = orderDetailsEntity.AddSkipNavigation(
                nameof(OrderDetails.Customer), null, customerEntity, false, false);
            orderDetailsEntity.RemoveIgnored(nameof(OrderDetails.Customer));

            VerifyError(
                CoreStrings.SkipNavigationNonCollection(nameof(OrderDetails.Customer), nameof(OrderDetails)),
                modelBuilder);
        }

        [ConditionalFact]
        public virtual void Passes_on_valid_owned_entity_types()
        {
            var builder = CreateConventionlessModelBuilder();
            var modelBuilder = (InternalModelBuilder)builder.GetInfrastructure();
            var entityTypeBuilder = modelBuilder.Entity(typeof(SampleEntity), ConfigurationSource.Convention);
            entityTypeBuilder.PrimaryKey(new[] { nameof(SampleEntity.Id) }, ConfigurationSource.Convention);
            entityTypeBuilder.Ignore(nameof(SampleEntity.Name), ConfigurationSource.Explicit);
            entityTypeBuilder.Ignore(nameof(SampleEntity.Number), ConfigurationSource.Explicit);
            entityTypeBuilder.Ignore(nameof(SampleEntity.OtherSamples), ConfigurationSource.Explicit);
            entityTypeBuilder.Ignore(nameof(SampleEntity.AnotherReferencedEntity), ConfigurationSource.Explicit);

            var ownershipBuilder = entityTypeBuilder.HasOwnership(
                typeof(ReferencedEntity), nameof(SampleEntity.ReferencedEntity), ConfigurationSource.Convention);

            var ownedTypeBuilder = ownershipBuilder.Metadata.DeclaringEntityType.Builder;
            ownedTypeBuilder.PrimaryKey(ownershipBuilder.Metadata.Properties.Select(p => p.Name).ToList(), ConfigurationSource.Convention);
            ownedTypeBuilder.Ignore(nameof(ReferencedEntity.Id), ConfigurationSource.Explicit);
            ownedTypeBuilder.Ignore(nameof(ReferencedEntity.SampleEntityId), ConfigurationSource.Explicit);

            Validate(builder);
        }

        [ConditionalFact]
        public virtual void Detects_entity_type_with_multiple_ownerships()
        {
            var builder = CreateConventionlessModelBuilder();
            var modelBuilder = (InternalModelBuilder)builder.GetInfrastructure();

            var entityTypeBuilder = modelBuilder.Entity(typeof(SampleEntity), ConfigurationSource.Convention);
            entityTypeBuilder.PrimaryKey(new[] { nameof(SampleEntity.Id) }, ConfigurationSource.Convention);
            entityTypeBuilder.Ignore(nameof(SampleEntity.Number), ConfigurationSource.Explicit);
            entityTypeBuilder.Ignore(nameof(SampleEntity.Name), ConfigurationSource.Explicit);
            entityTypeBuilder.Ignore(nameof(SampleEntity.OtherSamples), ConfigurationSource.Explicit);

            var ownershipBuilder = entityTypeBuilder.HasOwnership(
                typeof(ReferencedEntity), nameof(SampleEntity.ReferencedEntity), ConfigurationSource.Convention);

            var ownedTypeBuilder = ownershipBuilder.Metadata.DeclaringEntityType.Builder;
            ownedTypeBuilder.PrimaryKey(ownershipBuilder.Metadata.Properties.Select(p => p.Name).ToList(), ConfigurationSource.Convention);

            ownedTypeBuilder.HasRelationship(
                    entityTypeBuilder.Metadata, null, nameof(SampleEntity.AnotherReferencedEntity), ConfigurationSource.Convention, setTargetAsPrincipal: true)
                .Metadata.IsOwnership = true;

            ownedTypeBuilder.Ignore(nameof(ReferencedEntity.Id), ConfigurationSource.Explicit);
            ownedTypeBuilder.Ignore(nameof(ReferencedEntity.SampleEntityId), ConfigurationSource.Explicit);

            VerifyError(
                CoreStrings.MultipleOwnerships(nameof(ReferencedEntity), "'SampleEntity.ReferencedEntity', 'SampleEntity.AnotherReferencedEntity'"),
                builder);
        }

        [ConditionalFact]
        public virtual void Detects_principal_owned_entity_type()
        {
            var builder = CreateConventionlessModelBuilder();
            var modelBuilder = (InternalModelBuilder)builder.GetInfrastructure();

            var entityTypeBuilder = modelBuilder.Entity(typeof(SampleEntity), ConfigurationSource.Convention);
            entityTypeBuilder.PrimaryKey(new[] { nameof(SampleEntity.Id) }, ConfigurationSource.Convention);
            entityTypeBuilder.Ignore(nameof(SampleEntity.Number), ConfigurationSource.Explicit);
            entityTypeBuilder.Ignore(nameof(SampleEntity.Name), ConfigurationSource.Explicit);
            entityTypeBuilder.Ignore(nameof(SampleEntity.OtherSamples), ConfigurationSource.Explicit);
            entityTypeBuilder.Ignore(nameof(SampleEntity.AnotherReferencedEntity), ConfigurationSource.Explicit);

            var ownershipBuilder = entityTypeBuilder.HasOwnership(
                typeof(ReferencedEntity), nameof(SampleEntity.ReferencedEntity), ConfigurationSource.Convention);

            var ownedTypeBuilder = ownershipBuilder.Metadata.DeclaringEntityType.Builder;
            ownedTypeBuilder.Ignore(nameof(ReferencedEntity.Id), ConfigurationSource.Explicit);
            ownedTypeBuilder.Ignore(nameof(ReferencedEntity.SampleEntityId), ConfigurationSource.Explicit);
            ownedTypeBuilder.PrimaryKey(ownershipBuilder.Metadata.Properties.Select(p => p.Name).ToList(), ConfigurationSource.Convention);

            var anotherEntityTypeBuilder = modelBuilder.Entity(typeof(AnotherSampleEntity), ConfigurationSource.Convention);
            anotherEntityTypeBuilder.PrimaryKey(new[] { nameof(AnotherSampleEntity.Id) }, ConfigurationSource.Convention);

            anotherEntityTypeBuilder.HasRelationship(
                ownedTypeBuilder.Metadata, nameof(AnotherSampleEntity.ReferencedEntity), ConfigurationSource.Convention,
                targetIsPrincipal: true);

            VerifyError(
                CoreStrings.PrincipalOwnedType(
                    nameof(AnotherSampleEntity) + "." + nameof(AnotherSampleEntity.ReferencedEntity),
                    nameof(ReferencedEntity),
                    nameof(ReferencedEntity)),
                builder);
        }

        [ConditionalFact]
        public virtual void Detects_non_owner_navigation_to_owned_entity_type()
        {
            var builder = CreateConventionlessModelBuilder();
            var modelBuilder = (InternalModelBuilder)builder.GetInfrastructure();

            var entityTypeBuilder = modelBuilder.Entity(typeof(SampleEntity), ConfigurationSource.Convention);
            entityTypeBuilder.PrimaryKey(new[] { nameof(SampleEntity.Id) }, ConfigurationSource.Convention);
            entityTypeBuilder.Ignore(nameof(SampleEntity.Number), ConfigurationSource.Explicit);
            entityTypeBuilder.Ignore(nameof(SampleEntity.Name), ConfigurationSource.Explicit);
            entityTypeBuilder.Ignore(nameof(SampleEntity.OtherSamples), ConfigurationSource.Explicit);
            entityTypeBuilder.Ignore(nameof(SampleEntity.AnotherReferencedEntity), ConfigurationSource.Explicit);

            var ownershipBuilder = entityTypeBuilder.HasOwnership(
                typeof(ReferencedEntity), nameof(SampleEntity.ReferencedEntity), ConfigurationSource.Convention);

            var ownedTypeBuilder = ownershipBuilder.Metadata.DeclaringEntityType.Builder;
            ownedTypeBuilder.PrimaryKey(ownershipBuilder.Metadata.Properties.Select(p => p.Name).ToList(), ConfigurationSource.Convention);
            ownedTypeBuilder.Ignore(nameof(ReferencedEntity.Id), ConfigurationSource.Explicit);
            ownedTypeBuilder.Ignore(nameof(ReferencedEntity.SampleEntityId), ConfigurationSource.Explicit);

            var anotherEntityTypeBuilder = modelBuilder.Entity(typeof(AnotherSampleEntity), ConfigurationSource.Convention);
            anotherEntityTypeBuilder.PrimaryKey(new[] { nameof(AnotherSampleEntity.Id) }, ConfigurationSource.Convention);

            anotherEntityTypeBuilder.HasRelationship(
                    ownedTypeBuilder.Metadata, nameof(AnotherSampleEntity.ReferencedEntity), ConfigurationSource.Convention)
                .HasEntityTypes(anotherEntityTypeBuilder.Metadata, ownedTypeBuilder.Metadata, ConfigurationSource.Convention);

            VerifyError(
                CoreStrings.InverseToOwnedType(
                    nameof(AnotherSampleEntity), nameof(SampleEntity.ReferencedEntity), nameof(ReferencedEntity), nameof(SampleEntity)),
                builder);
        }

        [ConditionalFact]
        public virtual void Detects_derived_owned_entity_type()
        {
            var builder = CreateConventionlessModelBuilder();
            var modelBuilder = (InternalModelBuilder)builder.GetInfrastructure();

            var entityTypeBuilder = modelBuilder.Entity(typeof(B), ConfigurationSource.Convention);
            entityTypeBuilder.PrimaryKey(new[] { nameof(B.Id) }, ConfigurationSource.Convention);
            entityTypeBuilder.Property(typeof(int?), nameof(B.P0), ConfigurationSource.Explicit);
            entityTypeBuilder.Property(typeof(int?), nameof(B.P1), ConfigurationSource.Explicit);
            entityTypeBuilder.Property(typeof(int?), nameof(B.P2), ConfigurationSource.Explicit);
            entityTypeBuilder.Property(typeof(int?), nameof(B.P3), ConfigurationSource.Explicit);
            entityTypeBuilder.Ignore(nameof(B.AnotherA), ConfigurationSource.Explicit);
            entityTypeBuilder.Ignore(nameof(B.ManyAs), ConfigurationSource.Explicit);

            var ownershipBuilder = entityTypeBuilder.HasOwnership(typeof(D), nameof(B.A), ConfigurationSource.Convention);
            var ownedTypeBuilder = ownershipBuilder.Metadata.DeclaringEntityType.Builder;
            ownedTypeBuilder.PrimaryKey(ownershipBuilder.Metadata.Properties.Select(p => p.Name).ToList(), ConfigurationSource.Convention);

            var baseOwnershipBuilder = entityTypeBuilder.HasOwnership(typeof(A), nameof(B.A), ConfigurationSource.Convention);
            var anotherEntityTypeBuilder = baseOwnershipBuilder.Metadata.DeclaringEntityType.Builder;
            anotherEntityTypeBuilder = modelBuilder.Entity(typeof(A), ConfigurationSource.Convention);
            anotherEntityTypeBuilder.PrimaryKey(new[] { nameof(A.Id) }, ConfigurationSource.Convention);
            anotherEntityTypeBuilder.Property(typeof(int?), nameof(A.P0), ConfigurationSource.Explicit);
            anotherEntityTypeBuilder.Property(typeof(int?), nameof(A.P1), ConfigurationSource.Explicit);
            anotherEntityTypeBuilder.Property(typeof(int?), nameof(A.P2), ConfigurationSource.Explicit);
            anotherEntityTypeBuilder.Property(typeof(int?), nameof(A.P3), ConfigurationSource.Explicit);

            Assert.NotNull(ownedTypeBuilder.HasBaseType(typeof(A), ConfigurationSource.DataAnnotation));

            VerifyError(CoreStrings.OwnedDerivedType(nameof(D)), builder);
        }

        [ConditionalFact]
        public virtual void Detects_owned_entity_type_without_ownership()
        {
            var builder = CreateConventionlessModelBuilder();
            var modelBuilder = (InternalModelBuilder)builder.GetInfrastructure();
            var aBuilder = modelBuilder.Entity(typeof(A), ConfigurationSource.Convention);
            aBuilder.Ignore(nameof(A.Id), ConfigurationSource.Explicit);
            aBuilder.Ignore(nameof(A.P0), ConfigurationSource.Explicit);
            aBuilder.Ignore(nameof(A.P1), ConfigurationSource.Explicit);
            aBuilder.Ignore(nameof(A.P2), ConfigurationSource.Explicit);
            aBuilder.Ignore(nameof(A.P3), ConfigurationSource.Explicit);

            modelBuilder.Owned(typeof(A), ConfigurationSource.Convention);

            VerifyError(CoreStrings.OwnerlessOwnedType(nameof(A)), builder);
        }

        [ConditionalFact]
        public virtual void Detects_ForeignKey_on_inherited_generated_key_property()
        {
            var modelBuilder = CreateConventionalModelBuilder();
            modelBuilder.Entity<Abstract>().Property<int>("SomeId").ValueGeneratedOnAdd();
            modelBuilder.Entity<Abstract>().HasAlternateKey("SomeId");
            modelBuilder.Entity<Generic<int>>().HasOne<Abstract>().WithOne().HasForeignKey<Generic<int>>("SomeId");
            modelBuilder.Entity<Generic<string>>();

            VerifyError(
                CoreStrings.ForeignKeyPropertyInKey(
                    "SomeId",
                    "Generic<int>",
                    "{'SomeId'}",
                    nameof(Abstract)), modelBuilder);
        }

        [ConditionalFact]
        public virtual void Passes_for_ForeignKey_on_inherited_generated_key_property_abstract_base()
        {
            var modelBuilder = CreateConventionalModelBuilder();
            modelBuilder.Entity<Abstract>().Property(e => e.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<Generic<int>>().HasOne<Abstract>().WithOne().HasForeignKey<Generic<int>>(e => e.Id);

            Validate(modelBuilder);
        }

        [ConditionalFact]
        public virtual void Passes_for_ForeignKey_on_inherited_generated_composite_key_property()
        {
            var modelBuilder = CreateConventionalModelBuilder();
            modelBuilder.Entity<Abstract>().Property<int>("SomeId").ValueGeneratedOnAdd();
            modelBuilder.Entity<Abstract>().Property<int>("SomeOtherId").ValueGeneratedOnAdd();
            modelBuilder.Entity<Abstract>().HasAlternateKey("SomeId", "SomeOtherId");
            modelBuilder.Entity<Generic<int>>().HasOne<Abstract>().WithOne().HasForeignKey<Generic<int>>("SomeId");
            modelBuilder.Entity<Generic<string>>();

            Validate(modelBuilder);
        }

        [ConditionalTheory]
        [InlineData(ChangeTrackingStrategy.ChangedNotifications)]
        [InlineData(ChangeTrackingStrategy.ChangingAndChangedNotifications)]
        [InlineData(ChangeTrackingStrategy.ChangingAndChangedNotificationsWithOriginalValues)]
        public virtual void Detects_non_notifying_entities(ChangeTrackingStrategy changeTrackingStrategy)
        {
            var modelBuilder = CreateConventionlessModelBuilder();
            var model = modelBuilder.Model;

            var entityType = model.AddEntityType(typeof(NonNotifyingEntity));
            var id = entityType.AddProperty("Id");
            entityType.SetPrimaryKey(id);

            model.SetChangeTrackingStrategy(changeTrackingStrategy);

            VerifyError(
                CoreStrings.ChangeTrackingInterfaceMissing("NonNotifyingEntity", changeTrackingStrategy, "INotifyPropertyChanged"),
                modelBuilder);
        }

        [ConditionalTheory]
        [InlineData(ChangeTrackingStrategy.ChangingAndChangedNotifications)]
        [InlineData(ChangeTrackingStrategy.ChangingAndChangedNotificationsWithOriginalValues)]
        public virtual void Detects_changed_only_notifying_entities(ChangeTrackingStrategy changeTrackingStrategy)
        {
            var modelBuilder = CreateConventionlessModelBuilder();
            var model = modelBuilder.Model;

            var entityType = model.AddEntityType(typeof(ChangedOnlyEntity));
            var id = entityType.AddProperty("Id");
            entityType.SetPrimaryKey(id);

            model.SetChangeTrackingStrategy(changeTrackingStrategy);

            VerifyError(
                CoreStrings.ChangeTrackingInterfaceMissing("ChangedOnlyEntity", changeTrackingStrategy, "INotifyPropertyChanging"),
                modelBuilder);
        }

        [ConditionalTheory]
        [InlineData(ChangeTrackingStrategy.Snapshot)]
        [InlineData(ChangeTrackingStrategy.ChangedNotifications)]
        [InlineData(ChangeTrackingStrategy.ChangingAndChangedNotifications)]
        [InlineData(ChangeTrackingStrategy.ChangingAndChangedNotificationsWithOriginalValues)]
        public virtual void Passes_for_fully_notifying_entities(ChangeTrackingStrategy changeTrackingStrategy)
        {
            var modelBuilder = CreateConventionlessModelBuilder();
            var model = modelBuilder.Model;

            var entityType = model.AddEntityType(typeof(FullNotificationEntity));
            var id = entityType.AddProperty("Id");
            entityType.SetPrimaryKey(id);

            model.SetChangeTrackingStrategy(changeTrackingStrategy);

            Validate(modelBuilder);
        }

        [ConditionalTheory]
        [InlineData(ChangeTrackingStrategy.Snapshot)]
        [InlineData(ChangeTrackingStrategy.ChangedNotifications)]
        public virtual void Passes_for_changed_only_entities_with_snapshot_or_changed_only_tracking(
            ChangeTrackingStrategy changeTrackingStrategy)
        {
            var modelBuilder = CreateConventionlessModelBuilder();
            var model = modelBuilder.Model;

            var entityType = model.AddEntityType(typeof(ChangedOnlyEntity));
            var id = entityType.AddProperty("Id");
            entityType.SetPrimaryKey(id);

            model.SetChangeTrackingStrategy(changeTrackingStrategy);

            Validate(modelBuilder);
        }

        [ConditionalFact]
        public virtual void Passes_for_non_notifying_entities_with_snapshot_tracking()
        {
            var modelBuilder = CreateConventionlessModelBuilder();
            var model = modelBuilder.Model;

            var entityType = model.AddEntityType(typeof(NonNotifyingEntity));
            var id = entityType.AddProperty("Id");
            entityType.SetPrimaryKey(id);

            model.SetChangeTrackingStrategy(ChangeTrackingStrategy.Snapshot);

            Validate(modelBuilder);
        }

        [ConditionalFact]
        public virtual void Passes_for_valid_seeds()
        {
            var modelBuilder = CreateConventionalModelBuilder();
            modelBuilder.Entity<A>().HasData(
                new A { Id = 1 });
            modelBuilder.Entity<D>().HasData(
                new D { Id = 2, P0 = 3 });

            Validate(modelBuilder);
        }

        [ConditionalFact]
        public virtual void Passes_for_ignored_invalid_seeded_properties()
        {
            var modelBuilder = CreateConventionalModelBuilder();
            modelBuilder.Entity<EntityWithInvalidProperties>(
                eb =>
                {
                    eb.Ignore(e => e.NotImplemented);

                    eb.HasData(
                        new EntityWithInvalidProperties { Id = -1 });

                    eb.HasData(
                        new
                        {
                            Id = -2,
                            NotImplemented = true,
                            Static = 1,
                            WriteOnly = 1,
                            ReadOnly = 1,
                            PrivateGetter = 1
                        });
                });

            Validate(modelBuilder);

            var data = modelBuilder.Model.GetEntityTypes().Single().GetSeedData();
            Assert.Equal(-1, data.First().Values.Single());
            Assert.Equal(-2, data.Last().Values.Single());
        }

        [ConditionalFact]
        public virtual void Detects_derived_seeds()
        {
            var modelBuilder = CreateConventionalModelBuilder();

            Assert.Equal(
                CoreStrings.SeedDatumDerivedType(nameof(A), nameof(D)),
                Assert.Throws<InvalidOperationException>(
                    () => modelBuilder.Entity<A>().HasData(
                        new D { Id = 2, P0 = 3 })).Message);
        }

        [ConditionalFact]
        public virtual void Detects_derived_seeds_for_owned_types()
        {
            var modelBuilder = CreateConventionalModelBuilder();

            Assert.Equal(
                CoreStrings.SeedDatumDerivedType(nameof(A), nameof(D)),
                Assert.Throws<InvalidOperationException>(
                    () => modelBuilder.Entity<B>()
                        .OwnsOne(
                            b => b.A, a => a.HasData(
                                new D { Id = 2, P0 = 3 }))
                        .OwnsOne(b => b.AnotherA)).Message);
        }

        [ConditionalFact]
        public virtual void Detects_missing_required_values_in_seeds()
        {
            var modelBuilder = CreateConventionalModelBuilder();
            modelBuilder.Entity<A>(
                e =>
                {
                    e.Property(a => a.P0).IsRequired();
                    e.HasData(
                        new A { Id = 1 });
                });

            VerifyError(
                CoreStrings.SeedDatumMissingValue(nameof(A), nameof(A.P0)),
                modelBuilder);
        }

        [ConditionalFact]
        public virtual void Passes_on_missing_required_store_generated_values_in_seeds()
        {
            var modelBuilder = CreateConventionalModelBuilder();
            modelBuilder.Entity<A>(
                e =>
                {
                    e.Property(a => a.P0).IsRequired().ValueGeneratedOnAddOrUpdate();
                    e.HasData(
                        new A { Id = 1 });
                });

            Validate(modelBuilder);
        }

        [ConditionalFact]
        public virtual void Detects_missing_key_values_in_seeds()
        {
            var entity = new NonSignedIntegerKeyEntity();
            var modelBuilder = CreateConventionalModelBuilder();
            modelBuilder.Entity<NonSignedIntegerKeyEntity>(e => e.HasData(entity));

            Assert.Equal(
                ValueGenerated.OnAdd,
                modelBuilder.Model.FindEntityType(typeof(NonSignedIntegerKeyEntity)).FindProperty(nameof(NonSignedIntegerKeyEntity.Id))
                    .ValueGenerated);
            VerifyError(
                CoreStrings.SeedDatumDefaultValue(nameof(NonSignedIntegerKeyEntity), nameof(NonSignedIntegerKeyEntity.Id), entity.Id),
                modelBuilder);
        }

        [ConditionalFact]
        public virtual void Detects_missing_signed_integer_key_values_in_seeds()
        {
            var modelBuilder = CreateConventionalModelBuilder();
            modelBuilder.Entity<A>(e => e.HasData(new A()));

            VerifyError(
                CoreStrings.SeedDatumSignedNumericValue(nameof(A), nameof(A.Id)),
                modelBuilder);
        }

        [ConditionalTheory]
        [InlineData(true)]
        [InlineData(false)]
        public virtual void Detects_duplicate_seeds(bool sensitiveDataLoggingEnabled)
        {
            var modelBuilder = CreateConventionalModelBuilder(sensitiveDataLoggingEnabled: sensitiveDataLoggingEnabled);
            modelBuilder.Entity<A>().HasData(
                new A { Id = 1 });
            modelBuilder.Entity<D>().HasData(
                new D { Id = 1 });

            VerifyError(
                sensitiveDataLoggingEnabled
                    ? CoreStrings.SeedDatumDuplicateSensitive(nameof(D), $"{nameof(A.Id)}:1")
                    : CoreStrings.SeedDatumDuplicate(nameof(D), $"{{'{nameof(A.Id)}'}}"),
                modelBuilder,
                sensitiveDataLoggingEnabled);
        }

        [ConditionalTheory]
        [InlineData(true)]
        [InlineData(false)]
        public virtual void Detects_incompatible_values(bool sensitiveDataLoggingEnabled)
        {
            var modelBuilder = CreateConventionalModelBuilder(sensitiveDataLoggingEnabled: sensitiveDataLoggingEnabled);
            modelBuilder.Entity<A>(
                e =>
                {
                    e.HasData(
                        new { Id = 1, P0 = "invalid" });
                });

            VerifyError(
                sensitiveDataLoggingEnabled
                    ? CoreStrings.SeedDatumIncompatibleValueSensitive(nameof(A), "invalid", nameof(A.P0), "int?")
                    : CoreStrings.SeedDatumIncompatibleValue(nameof(A), nameof(A.P0), "int?"),
                modelBuilder,
                sensitiveDataLoggingEnabled);
        }

        [ConditionalTheory]
        [InlineData(true)]
        [InlineData(false)]
        public virtual void Detects_reference_navigations_in_seeds(bool sensitiveDataLoggingEnabled)
        {
            var modelBuilder = CreateConventionalModelBuilder(sensitiveDataLoggingEnabled: sensitiveDataLoggingEnabled);
            modelBuilder.Entity<SampleEntity>(
                e =>
                {
                    e.HasData(
                        new SampleEntity { Id = 1, ReferencedEntity = new ReferencedEntity { Id = 2 } });
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
                modelBuilder,
                sensitiveDataLoggingEnabled);
        }

        [ConditionalTheory]
        [InlineData(true)]
        [InlineData(false)]
        public virtual void Detects_reference_navigations_in_seeds2(bool sensitiveDataLoggingEnabled)
        {
            var modelBuilder = CreateConventionalModelBuilder(sensitiveDataLoggingEnabled: sensitiveDataLoggingEnabled);
            modelBuilder.Entity<Order>(
                e =>
                {
                    e.HasMany(o => o.Products)
                     .WithMany(p => p.Orders);
                    e.HasData(
                        new Order { Id = 1, Products = new List<Product> { new() } });
                });

            VerifyError(
                sensitiveDataLoggingEnabled
                    ? CoreStrings.SeedDatumNavigationSensitive(
                        nameof(Order),
                        $"{nameof(Order.Id)}:1",
                        nameof(Order.Products),
                        "OrderProduct (Dictionary<string, object>)",
                        "{'OrdersId'}")
                    : CoreStrings.SeedDatumNavigation(
                        nameof(Order),
                        nameof(Order.Products),
                        "OrderProduct (Dictionary<string, object>)",
                        "{'OrdersId'}"),
                modelBuilder,
                sensitiveDataLoggingEnabled);
        }

        [ConditionalTheory]
        [InlineData(true)]
        [InlineData(false)]
        public virtual void Detects_collection_navigations_in_seeds(bool sensitiveDataLoggingEnabled)
        {
            var modelBuilder = CreateConventionalModelBuilder(sensitiveDataLoggingEnabled: sensitiveDataLoggingEnabled);
            modelBuilder.Entity<SampleEntity>(
                e =>
                {
                    e.HasData(
                        new SampleEntity
                        {
                            Id = 1,
                            OtherSamples = new HashSet<SampleEntity>(
                                new[] { new SampleEntity { Id = 2 } })
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
                modelBuilder,
                sensitiveDataLoggingEnabled);
        }

        [ConditionalFact]
        public virtual void Detects_missing_discriminator_property()
        {
            var modelBuilder = CreateConventionlessModelBuilder();
            var model = modelBuilder.Model;

            var entityA = model.AddEntityType(typeof(A));
            SetPrimaryKey(entityA);
            AddProperties(entityA);

            var entityC = model.AddEntityType(typeof(C));
            entityC.BaseType = entityA;

            VerifyError(CoreStrings.NoDiscriminatorProperty(entityA.DisplayName()), modelBuilder);
        }

        [ConditionalFact]
        public virtual void Detects_missing_discriminator_value_on_base()
        {
            var modelBuilder = CreateConventionlessModelBuilder();
            var model = modelBuilder.Model;

            var entityA = model.AddEntityType(typeof(A));
            SetPrimaryKey(entityA);
            AddProperties(entityA);

            var entityC = model.AddEntityType(typeof(C));
            SetBaseType(entityC, entityA);

            entityA.SetDiscriminatorProperty(entityA.AddProperty("D", typeof(int)));
            entityC.SetDiscriminatorValue(1);

            VerifyError(CoreStrings.NoDiscriminatorValue(entityA.DisplayName()), modelBuilder);
        }

        [ConditionalFact]
        public virtual void Detects_missing_discriminator_value_on_leaf()
        {
            var modelBuilder = CreateConventionlessModelBuilder();
            var model = modelBuilder.Model;

            var entityAbstract = model.AddEntityType(typeof(Abstract));
            SetPrimaryKey(entityAbstract);
            AddProperties(entityAbstract);

            var entityGeneric = model.AddEntityType(typeof(Generic<string>));
            SetBaseType(entityGeneric, entityAbstract);

            entityAbstract.SetDiscriminatorProperty(entityAbstract.AddProperty("D", typeof(int)));
            entityAbstract.SetDiscriminatorValue(0);

            VerifyError(CoreStrings.NoDiscriminatorValue(entityGeneric.DisplayName()), modelBuilder);
        }

        [ConditionalFact]
        public virtual void Detects_missing_non_string_discriminator_values()
        {
            var modelBuilder = CreateConventionalModelBuilder();
            modelBuilder.Entity<C>();
            modelBuilder.Entity<A>().HasDiscriminator<byte>("ClassType")
                .HasValue<A>(0)
                .HasValue<D>(1);

            VerifyError(CoreStrings.NoDiscriminatorValue(typeof(C).Name), modelBuilder);
        }

        [ConditionalFact]
        public virtual void Detects_duplicate_discriminator_values()
        {
            var modelBuilder = CreateConventionalModelBuilder();
            modelBuilder.Entity<A>().HasDiscriminator<byte>("ClassType")
                .HasValue<A>(1)
                .HasValue<C>(1)
                .HasValue<D>(2);

            VerifyError(CoreStrings.DuplicateDiscriminatorValue(typeof(C).Name, 1, typeof(A).Name), modelBuilder);
        }

        [ConditionalFact]
        public virtual void Required_navigation_with_query_filter_on_one_side_issues_a_warning()
        {
            var modelBuilder = CreateConventionalModelBuilder();
            modelBuilder.Entity<Customer>().HasMany(x => x.Orders).WithOne(x => x.Customer).IsRequired();
            modelBuilder.Entity<Customer>().HasQueryFilter(x => x.Id > 5);
            modelBuilder.Ignore<OrderDetails>();

            var message = CoreResources.LogPossibleIncorrectRequiredNavigationWithQueryFilterInteraction(
                CreateValidationLogger()).GenerateMessage(nameof(Customer), nameof(Order));

            VerifyWarning(message, modelBuilder);
        }

        [ConditionalFact]
        public virtual void Optional_navigation_with_query_filter_on_one_side_doesnt_issue_a_warning()
        {
            var modelBuilder = CreateConventionalModelBuilder();
            modelBuilder.Entity<Customer>().HasMany(x => x.Orders).WithOne(x => x.Customer).IsRequired(false);
            modelBuilder.Entity<Customer>().HasQueryFilter(x => x.Id > 5);

            var message = CoreResources.LogPossibleIncorrectRequiredNavigationWithQueryFilterInteraction(
                CreateValidationLogger()).GenerateMessage(nameof(Customer), nameof(Order));

            VerifyLogDoesNotContain(message, modelBuilder);
        }

        [ConditionalFact]
        public virtual void Required_navigation_with_query_filter_on_both_sides_doesnt_issue_a_warning()
        {
            var modelBuilder = CreateConventionalModelBuilder();
            modelBuilder.Entity<Customer>().HasMany(x => x.Orders).WithOne(x => x.Customer).IsRequired();
            modelBuilder.Entity<Customer>().HasQueryFilter(x => x.Id > 5);
            modelBuilder.Entity<Order>().HasQueryFilter(x => x.Customer.Id > 5);

            var message = CoreResources.LogPossibleIncorrectRequiredNavigationWithQueryFilterInteraction(
                CreateValidationLogger()).GenerateMessage(nameof(Customer), nameof(Order));

            VerifyLogDoesNotContain(message, modelBuilder);
        }

        [ConditionalFact]
        public virtual void Shared_type_inheritance_throws()
        {
            var modelBuilder = CreateConventionalModelBuilder();
            modelBuilder.SharedTypeEntity<A>("Shared1");
            modelBuilder.SharedTypeEntity<C>("Shared2").HasBaseType("Shared1");

            VerifyError(CoreStrings.SharedTypeDerivedType("Shared2 (C)"), modelBuilder);
        }

        [ConditionalFact]
        public virtual void Seeding_keyless_entity_throws()
        {
            var modelBuilder = CreateConventionalModelBuilder();
            modelBuilder.Entity<KeylessSeed>(
                e =>
                {
                    e.HasNoKey();
                    e.HasData(
                        new KeylessSeed
                        {
                            Species = "Apple"
                        });
                });

            VerifyError(CoreStrings.SeedKeylessEntity(nameof(KeylessSeed)), modelBuilder);
        }

        // INotify interfaces not really implemented; just marking the classes to test metadata construction
        protected class FullNotificationEntity : INotifyPropertyChanging, INotifyPropertyChanged
        {
            public int Id { get; set; }

#pragma warning disable 67
            public event PropertyChangingEventHandler PropertyChanging;
            public event PropertyChangedEventHandler PropertyChanged;
#pragma warning restore 67
        }

        // INotify interfaces not really implemented; just marking the classes to test metadata construction
        protected class ChangedOnlyEntity : INotifyPropertyChanged
        {
            public int Id { get; set; }

#pragma warning disable 67
            public event PropertyChangedEventHandler PropertyChanged;
#pragma warning restore 67
        }

        protected class NonNotifyingEntity
        {
            public int Id { get; set; }
        }

        protected class NonSignedIntegerKeyEntity
        {
            public uint Id { get; set; }
        }
    }
}
