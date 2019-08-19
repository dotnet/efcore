// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Diagnostics.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.Logging;
using Xunit;

// ReSharper disable UnusedMember.Local
// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Infrastructure
{
    public class ModelValidatorTest : ModelValidatorTestBase
    {
        [ConditionalFact]
        public virtual void Detects_filter_on_derived_type()
        {
            var modelBuilder = CreateConventionalModelBuilder();
            modelBuilder.Entity<A>();
            var entityTypeD = modelBuilder.Entity<D>().Metadata;

            entityTypeD.SetQueryFilter((Expression<Func<D, bool>>)(_ => true));

            VerifyError(CoreStrings.BadFilterDerivedType(entityTypeD.GetQueryFilter(), entityTypeD.DisplayName()), modelBuilder.Model);
        }

        [ConditionalFact]
        public virtual void Detects_defining_query_on_keyed_entity_type()
        {
            var modelBuilder = CreateConventionalModelBuilder();
            modelBuilder.Entity<A>().ToQuery(() => new List<A>().AsQueryable());

            VerifyError(CoreStrings.DefiningQueryWithKey("A"), modelBuilder.Model);
        }

        [ConditionalFact]
        public virtual void Detects_shadow_entities()
        {
            var model = CreateConventionlessModelBuilder().Model;
            var entityType = model.AddEntityType("BewareTheShadows");
            SetPrimaryKey(entityType);

            VerifyError(CoreStrings.ShadowEntity("BewareTheShadows"), model);
        }

        [ConditionalFact]
        public virtual void Passes_on_shadow_key_created_explicitly()
        {
            var model = CreateConventionlessModelBuilder().Model;

            var entityType = model.AddEntityType(typeof(A));
            SetPrimaryKey(entityType);
            AddProperties(entityType);

            var keyProperty = entityType.AddProperty("Key", typeof(int));
            entityType.AddKey(keyProperty);

            VerifyWarning(CoreResources.LogShadowPropertyCreated(new TestLogger<TestLoggingDefinitions>()).GenerateMessage("Key", "A"), model, LogLevel.Debug);
        }

        [ConditionalFact]
        public virtual void Passes_on_shadow_primary_key_created_by_convention_in_dependent_type()
        {
            var model = (IConventionModel)CreateConventionlessModelBuilder().Model;

            var entityType = model.AddEntityType(typeof(A));
            AddProperties((IMutableEntityType)entityType);
            entityType.AddProperty(nameof(A.Id), typeof(int));

            var keyProperty = entityType.AddProperty("Key", typeof(int));
            entityType.SetPrimaryKey(keyProperty);

            VerifyWarning(CoreResources.LogShadowPropertyCreated(new TestLogger<TestLoggingDefinitions>())
                .GenerateMessage("Key", "A"), (IMutableModel)model, LogLevel.Debug);
        }

        [ConditionalFact]
        public virtual void Detects_shadow_key_referenced_by_foreign_key_by_convention()
        {
            var modelBuilder = CreateConventionlessModelBuilder().GetInfrastructure();
            var dependentEntityBuilder = modelBuilder.Entity(typeof(SampleEntityMinimal), ConfigurationSource.Convention);
            dependentEntityBuilder.Property(typeof(int), "Id", ConfigurationSource.Convention);
            dependentEntityBuilder.Ignore(nameof(SampleEntityMinimal.ReferencedEntity), ConfigurationSource.Explicit);

            dependentEntityBuilder.PrimaryKey(
                new List<string>
                {
                    "Id"
                }, ConfigurationSource.Convention);

            var principalEntityBuilder = modelBuilder.Entity(typeof(ReferencedEntityMinimal), ConfigurationSource.Convention);
            principalEntityBuilder.Property(typeof(int), "Id", ConfigurationSource.Convention);
            principalEntityBuilder.PrimaryKey(
                new List<string>
                {
                    "Id"
                }, ConfigurationSource.Convention);

            dependentEntityBuilder.Property(typeof(string), "Foo", ConfigurationSource.Convention);
            principalEntityBuilder.Property(typeof(string), "ReferencedFoo", ConfigurationSource.Convention);

            dependentEntityBuilder.HasRelationship(
                principalEntityBuilder.Metadata,
                dependentEntityBuilder.GetOrCreateProperties(
                    new List<string>
                    {
                        "Foo"
                    }, ConfigurationSource.Convention),
                principalEntityBuilder.HasKey(new[] { "ReferencedFoo" }, ConfigurationSource.Convention).Metadata,
                ConfigurationSource.Convention);

            VerifyError(
                CoreStrings.ReferencedShadowKey(
                    typeof(SampleEntityMinimal).Name,
                    typeof(ReferencedEntityMinimal).Name,
                    "{'Foo' : string}",
                    "{'Id' : int}"),
                modelBuilder.Metadata);
        }

        [ConditionalFact]
        public virtual void Detects_a_null_primary_key()
        {
            var builder = CreateConventionlessModelBuilder();
            builder.Entity<A>(
                b =>
                {
                    b.Property(e => e.Id);
                    b.Property(e => e.P0);
                    b.Property(e => e.P1);
                    b.Property(e => e.P2);
                    b.Property(e => e.P3);
                });

            VerifyError(CoreStrings.EntityRequiresKey(nameof(A)), builder.Model);
        }

        [ConditionalFact]
        public virtual void Detects_key_property_with_value_generated_on_update()
        {
            var model = CreateConventionlessModelBuilder().Model;
            var entityTypeA = model.AddEntityType(typeof(A));
            SetPrimaryKey(entityTypeA);
            AddProperties(entityTypeA);
            entityTypeA.FindPrimaryKey().Properties.Single().ValueGenerated = ValueGenerated.OnUpdate;

            VerifyError(CoreStrings.MutableKeyProperty(nameof(A.Id)), model);
        }

        [ConditionalFact]
        public virtual void Detects_key_property_with_value_generated_on_add_or_update()
        {
            var model = CreateConventionlessModelBuilder().Model;
            var entityTypeA = model.AddEntityType(typeof(A));
            SetPrimaryKey(entityTypeA);
            AddProperties(entityTypeA);
            entityTypeA.FindPrimaryKey().Properties.Single().ValueGenerated = ValueGenerated.OnAddOrUpdate;

            VerifyError(CoreStrings.MutableKeyProperty(nameof(A.Id)), model);
        }

        [ConditionalFact]
        public virtual void Detects_relationship_cycle()
        {
            var modelBuilder = base.CreateConventionalModelBuilder();

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

        [ConditionalFact]
        public virtual void Passes_on_redundant_foreign_key()
        {
            var modelBuilder = base.CreateConventionalModelBuilder();

            modelBuilder.Entity<A>().HasOne<A>().WithOne().IsRequired().HasForeignKey<A>(a => a.Id).HasPrincipalKey<A>(b => b.Id);

            VerifyWarning(CoreResources.LogRedundantForeignKey(new TestLogger<TestLoggingDefinitions>()).GenerateMessage("{'Id'}", "A"), modelBuilder.Model, LogLevel.Warning);
        }

        [ConditionalFact]
        public virtual void Passes_on_escapable_foreign_key_cycles()
        {
            var model = CreateConventionlessModelBuilder().Model;

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

            Validate(model);
        }

        [ConditionalFact]
        public virtual void Passes_on_escapable_foreign_key_cycles_not_starting_at_hub()
        {
            var model = CreateConventionlessModelBuilder().Model;

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

            Validate(model);
        }

        [ConditionalFact]
        public virtual void Passes_on_foreign_key_cycle_with_one_GenerateOnAdd()
        {
            var model = CreateConventionlessModelBuilder().Model;

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

            Validate(model);
        }

        [ConditionalFact]
        public virtual void Passes_on_double_reference_to_root_principal_property()
        {
            var model = CreateConventionlessModelBuilder().Model;

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

            Validate(model);
        }

        [ConditionalFact]
        public virtual void Passes_on_diamond_path_to_root_principal_property()
        {
            var model = CreateConventionlessModelBuilder().Model;

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

            Validate(model);
        }

        [ConditionalFact]
        public virtual void Passes_on_correct_inheritance()
        {
            var modelBuilder = CreateConventionalModelBuilder();
            modelBuilder.Entity<A>();
            modelBuilder.Entity<D>();

            Validate(modelBuilder.Model);
        }

        [ConditionalFact]
        public virtual void Detects_skipped_base_type()
        {
            var model = CreateConventionlessModelBuilder().Model;

            var entityA = model.AddEntityType(typeof(A));
            SetPrimaryKey(entityA);
            AddProperties(entityA);

            var entityD = model.AddEntityType(typeof(D));
            SetBaseType(entityD, entityA);

            var entityF = model.AddEntityType(typeof(F));
            SetBaseType(entityF, entityA);

            VerifyError(CoreStrings.InconsistentInheritance(nameof(F), nameof(D)), model);
        }

        [ConditionalFact]
        public virtual void Detects_abstract_leaf_type()
        {
            var model = CreateConventionlessModelBuilder().Model;

            var entityA = model.AddEntityType(typeof(A));
            SetPrimaryKey(entityA);
            AddProperties(entityA);

            var entityAbstract = model.AddEntityType(typeof(Abstract));
            SetBaseType(entityAbstract, entityA);

            VerifyError(CoreStrings.AbstractLeafEntityType(entityAbstract.DisplayName()), model);
        }

        [ConditionalFact]
        public virtual void Detects_generic_leaf_type()
        {
            var model = CreateConventionlessModelBuilder().Model;

            var entityAbstract = model.AddEntityType(typeof(Abstract));
            SetPrimaryKey(entityAbstract);
            AddProperties(entityAbstract);

            var entityGeneric = model.AddEntityType(typeof(Generic<>));
            SetBaseType(entityGeneric, entityAbstract);

            VerifyError(CoreStrings.AbstractLeafEntityType(entityGeneric.DisplayName()), model);
        }

        [ConditionalFact]
        public virtual void Passes_on_valid_owned_entity_types()
        {
            var modelBuilder = CreateConventionlessModelBuilder().GetInfrastructure();
            var entityTypeBuilder = modelBuilder.Entity(typeof(SampleEntity), ConfigurationSource.Convention);
            entityTypeBuilder.PrimaryKey(new[] { nameof(SampleEntity.Id) }, ConfigurationSource.Convention);
            entityTypeBuilder.Ignore(nameof(SampleEntity.Name), ConfigurationSource.Explicit);
            entityTypeBuilder.Ignore(nameof(SampleEntity.Number), ConfigurationSource.Explicit);
            entityTypeBuilder.Ignore(nameof(SampleEntity.OtherSamples), ConfigurationSource.Explicit);

            var ownershipBuilder = entityTypeBuilder.HasOwnership(
                typeof(ReferencedEntity), nameof(SampleEntity.ReferencedEntity), ConfigurationSource.Convention);

            var ownedTypeBuilder = ownershipBuilder.Metadata.DeclaringEntityType.Builder;
            ownedTypeBuilder.PrimaryKey(ownershipBuilder.Metadata.Properties.Select(p => p.Name).ToList(), ConfigurationSource.Convention);
            ownedTypeBuilder.Ignore(nameof(ReferencedEntity.Id), ConfigurationSource.Explicit);
            ownedTypeBuilder.Ignore(nameof(ReferencedEntity.SampleEntityId), ConfigurationSource.Explicit);

            Validate(modelBuilder.Metadata);
        }

        [ConditionalFact]
        public virtual void Detects_weak_entity_type_without_defining_navigation()
        {
            var modelBuilder = CreateConventionlessModelBuilder().GetInfrastructure();
            var entityTypeBuilder = modelBuilder.Entity(typeof(SampleEntityMinimal), ConfigurationSource.Convention);
            entityTypeBuilder.PrimaryKey(new[] { nameof(SampleEntityMinimal.Id) }, ConfigurationSource.Convention);

            var anotherEntityTypeBuilder = modelBuilder.Entity(typeof(AnotherSampleEntityMinimal), ConfigurationSource.Convention);
            anotherEntityTypeBuilder.PrimaryKey(new[] { nameof(AnotherSampleEntityMinimal.Id) }, ConfigurationSource.Convention);

            var anotherOwnershipBuilder = anotherEntityTypeBuilder.HasOwnership(
                typeof(ReferencedEntityMinimal), nameof(AnotherSampleEntityMinimal.ReferencedEntity), ConfigurationSource.Convention);
            anotherOwnershipBuilder.Metadata.DeclaringEntityType.Builder.PrimaryKey(
                anotherOwnershipBuilder.Metadata.Properties.Select(p => p.Name).ToList(), ConfigurationSource.Convention);

            var ownershipBuilder = entityTypeBuilder.HasOwnership(
                typeof(ReferencedEntityMinimal), nameof(SampleEntityMinimal.ReferencedEntity), ConfigurationSource.Convention);
            var ownedTypeBuilder = ownershipBuilder.Metadata.DeclaringEntityType.Builder;
            ownedTypeBuilder.PrimaryKey(ownershipBuilder.Metadata.Properties.Select(p => p.Name).ToList(), ConfigurationSource.Convention);

            entityTypeBuilder.Metadata.RemoveNavigation(nameof(SampleEntityMinimal.ReferencedEntity));
            entityTypeBuilder.Ignore(nameof(SampleEntityMinimal.ReferencedEntity), ConfigurationSource.Explicit);

            VerifyError(
                CoreStrings.NoDefiningNavigation(
                    nameof(SampleEntityMinimal.ReferencedEntity),
                    nameof(SampleEntityMinimal) + "." + nameof(SampleEntityMinimal.ReferencedEntity) + "#" + nameof(ReferencedEntityMinimal),
                    nameof(SampleEntityMinimal)),
                modelBuilder.Metadata);
        }

        [ConditionalFact]
        public virtual void Detects_entity_type_with_multiple_ownerships()
        {
            var modelBuilder = CreateConventionlessModelBuilder().GetInfrastructure();

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
                    entityTypeBuilder.Metadata, (string)null, null, ConfigurationSource.Convention, setTargetAsPrincipal: true)
                .Metadata.IsOwnership = true;

            ownedTypeBuilder.Ignore(nameof(ReferencedEntity.Id), ConfigurationSource.Explicit);
            ownedTypeBuilder.Ignore(nameof(ReferencedEntity.SampleEntityId), ConfigurationSource.Explicit);

            VerifyError(
                CoreStrings.MultipleOwnerships(nameof(ReferencedEntity)),
                modelBuilder.Metadata);
        }

        [ConditionalFact]
        public virtual void Detects_weak_entity_type_with_non_defining_ownership()
        {
            var modelBuilder = CreateConventionlessModelBuilder().GetInfrastructure();
            var entityTypeBuilder = modelBuilder.Entity(typeof(SampleEntityMinimal), ConfigurationSource.Convention);
            entityTypeBuilder.PrimaryKey(new[] { nameof(SampleEntityMinimal.Id) }, ConfigurationSource.Convention);

            var anotherEntityTypeBuilder = modelBuilder.Entity(typeof(AnotherSampleEntityMinimal), ConfigurationSource.Convention);
            anotherEntityTypeBuilder.PrimaryKey(new[] { nameof(AnotherSampleEntityMinimal.Id) }, ConfigurationSource.Convention);

            var anotherOwnershipBuilder = anotherEntityTypeBuilder.HasOwnership(
                typeof(ReferencedEntityMinimal), nameof(AnotherSampleEntityMinimal.ReferencedEntity), ConfigurationSource.Convention);
            anotherOwnershipBuilder.Metadata.DeclaringEntityType.Builder.PrimaryKey(
                anotherOwnershipBuilder.Metadata.Properties.Select(p => p.Name).ToList(), ConfigurationSource.Convention);

            var ownershipBuilder = entityTypeBuilder.HasOwnership(
                typeof(ReferencedEntityMinimal), nameof(SampleEntityMinimal.ReferencedEntity), ConfigurationSource.Convention);
            var ownedTypeBuilder = ownershipBuilder.Metadata.DeclaringEntityType.Builder;
            ownedTypeBuilder.PrimaryKey(ownershipBuilder.Metadata.Properties.Select(p => p.Name).ToList(), ConfigurationSource.Convention);

            ownershipBuilder.Metadata.IsOwnership = false;
            ownedTypeBuilder.HasRelationship(entityTypeBuilder.Metadata, (string)null, null, ConfigurationSource.Convention, setTargetAsPrincipal: true)
                .Metadata.IsOwnership = true;

            VerifyError(
                CoreStrings.NonDefiningOwnership(
                    nameof(SampleEntityMinimal),
                    nameof(SampleEntityMinimal.ReferencedEntity),
                    nameof(SampleEntityMinimal) + "." + nameof(SampleEntityMinimal.ReferencedEntity) + "#" + nameof(ReferencedEntityMinimal)),
                modelBuilder.Metadata);
        }

        [ConditionalFact]
        public virtual void Detects_weak_entity_type_without_ownership()
        {
            var modelBuilder = CreateConventionlessModelBuilder().GetInfrastructure();
            var entityTypeBuilder = modelBuilder.Entity(typeof(SampleEntityMinimal), ConfigurationSource.Convention);
            entityTypeBuilder.PrimaryKey(new[] { nameof(SampleEntityMinimal.Id) }, ConfigurationSource.Convention);

            var ownershipBuilder = entityTypeBuilder.HasOwnership(
                typeof(ReferencedEntityMinimal), nameof(SampleEntityMinimal.ReferencedEntity), ConfigurationSource.Convention);
            var ownedTypeBuilder = ownershipBuilder.Metadata.DeclaringEntityType.Builder;
            ownedTypeBuilder.PrimaryKey(ownershipBuilder.Metadata.Properties.Select(p => p.Name).ToList(), ConfigurationSource.Convention);

            var anotherEntityTypeBuilder = modelBuilder.Entity(typeof(AnotherSampleEntityMinimal), ConfigurationSource.Convention);
            anotherEntityTypeBuilder.PrimaryKey(new[] { nameof(AnotherSampleEntityMinimal.Id) }, ConfigurationSource.Convention);

            var anotherOwnershipBuilder = anotherEntityTypeBuilder.HasOwnership(
                typeof(ReferencedEntityMinimal), nameof(AnotherSampleEntityMinimal.ReferencedEntity), ConfigurationSource.Convention);
            anotherOwnershipBuilder.Metadata.DeclaringEntityType.Builder.PrimaryKey(
                anotherOwnershipBuilder.Metadata.Properties.Select(p => p.Name).ToList(), ConfigurationSource.Convention);
            anotherOwnershipBuilder.Metadata.IsOwnership = false;

            VerifyError(
                CoreStrings.InconsistentOwnership(
                    nameof(SampleEntityMinimal) + "." + nameof(SampleEntityMinimal.ReferencedEntity) + "#" + nameof(ReferencedEntityMinimal),
                    nameof(AnotherSampleEntityMinimal) + "." + nameof(AnotherSampleEntityMinimal.ReferencedEntity) + "#" + nameof(ReferencedEntityMinimal)),
                modelBuilder.Metadata);
        }

        [ConditionalFact]
        public virtual void Detects_principal_owned_entity_type()
        {
            var modelBuilder = CreateConventionlessModelBuilder().GetInfrastructure();

            var entityTypeBuilder = modelBuilder.Entity(typeof(SampleEntity), ConfigurationSource.Convention);
            entityTypeBuilder.PrimaryKey(new[] { nameof(SampleEntity.Id) }, ConfigurationSource.Convention);
            entityTypeBuilder.Ignore(nameof(SampleEntity.Number), ConfigurationSource.Explicit);
            entityTypeBuilder.Ignore(nameof(SampleEntity.Name), ConfigurationSource.Explicit);
            entityTypeBuilder.Ignore(nameof(SampleEntity.OtherSamples), ConfigurationSource.Explicit);

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
                setTargetAsPrincipal: true);

            VerifyError(
                CoreStrings.PrincipalOwnedType(
                    nameof(AnotherSampleEntity) + "." + nameof(AnotherSampleEntity.ReferencedEntity),
                    nameof(ReferencedEntity),
                    nameof(ReferencedEntity)),
                modelBuilder.Metadata);
        }

        [ConditionalFact]
        public virtual void Detects_non_owner_navigation_to_owned_entity_type()
        {
            var modelBuilder = CreateConventionlessModelBuilder().GetInfrastructure();

            var entityTypeBuilder = modelBuilder.Entity(typeof(SampleEntity), ConfigurationSource.Convention);
            entityTypeBuilder.PrimaryKey(new[] { nameof(SampleEntity.Id) }, ConfigurationSource.Convention);
            entityTypeBuilder.Ignore(nameof(SampleEntity.Number), ConfigurationSource.Explicit);
            entityTypeBuilder.Ignore(nameof(SampleEntity.Name), ConfigurationSource.Explicit);
            entityTypeBuilder.Ignore(nameof(SampleEntity.OtherSamples), ConfigurationSource.Explicit);

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
                modelBuilder.Metadata);
        }

        [ConditionalFact]
        public virtual void Detects_derived_owned_entity_type()
        {
            var modelBuilder = CreateConventionlessModelBuilder().GetInfrastructure();

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

            var anotherEntityTypeBuilder = modelBuilder.Entity(typeof(A), ConfigurationSource.Convention);
            anotherEntityTypeBuilder.PrimaryKey(new[] { nameof(A.Id) }, ConfigurationSource.Convention);
            anotherEntityTypeBuilder.Property(typeof(int?), nameof(A.P0), ConfigurationSource.Explicit);
            anotherEntityTypeBuilder.Property(typeof(int?), nameof(A.P1), ConfigurationSource.Explicit);
            anotherEntityTypeBuilder.Property(typeof(int?), nameof(A.P2), ConfigurationSource.Explicit);
            anotherEntityTypeBuilder.Property(typeof(int?), nameof(A.P3), ConfigurationSource.Explicit);

            ownedTypeBuilder.HasBaseType(typeof(A), ConfigurationSource.Convention);

            VerifyError(CoreStrings.OwnedDerivedType(nameof(D)), modelBuilder.Metadata);
        }

        [ConditionalFact]
        public virtual void Detects_owned_entity_type_without_ownership()
        {
            var modelBuilder = CreateConventionlessModelBuilder().GetInfrastructure();
            var aBuilder = modelBuilder.Entity(typeof(A), ConfigurationSource.Convention);
            aBuilder.Ignore(nameof(A.Id), ConfigurationSource.Explicit);
            aBuilder.Ignore(nameof(A.P0), ConfigurationSource.Explicit);
            aBuilder.Ignore(nameof(A.P1), ConfigurationSource.Explicit);
            aBuilder.Ignore(nameof(A.P2), ConfigurationSource.Explicit);
            aBuilder.Ignore(nameof(A.P3), ConfigurationSource.Explicit);

            modelBuilder.Owned(typeof(A), ConfigurationSource.Convention);

            VerifyError(CoreStrings.OwnerlessOwnedType(nameof(A)), modelBuilder.Metadata);
        }

        [ConditionalFact]
        public virtual void Detects_ForeignKey_on_inherited_generated_key_property()
        {
            var modelBuilder = CreateConventionalModelBuilder();
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

        [ConditionalFact]
        public virtual void Passes_ForeignKey_on_inherited_generated_key_property_abstract_base()
        {
            var modelBuilder = CreateConventionalModelBuilder();
            modelBuilder.Entity<Abstract>().Property(e => e.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<Generic<int>>().HasOne<Abstract>().WithOne().HasForeignKey<Generic<int>>(e => e.Id);

            Validate(modelBuilder.Model);
        }

        [ConditionalFact]
        public virtual void Detects_ToQuery_on_derived_keyless_types()
        {
            var modelBuilder = base.CreateConventionalModelBuilder();
            var context = new DbContext(new DbContextOptions<DbContext>());
            modelBuilder.Entity<Abstract>().HasNoKey().ToQuery(() => context.Set<Abstract>());
            modelBuilder.Entity<Generic<int>>().ToQuery(() => context.Set<Generic<int>>());

            VerifyError(
                CoreStrings.DerivedTypeDefiningQuery("Generic<int>", nameof(Abstract)),
                modelBuilder.Model);
        }

        [ConditionalTheory]
        [InlineData(ChangeTrackingStrategy.ChangedNotifications)]
        [InlineData(ChangeTrackingStrategy.ChangingAndChangedNotifications)]
        [InlineData(ChangeTrackingStrategy.ChangingAndChangedNotificationsWithOriginalValues)]
        public virtual void Detects_non_notifying_entities(ChangeTrackingStrategy changeTrackingStrategy)
        {
            var model = CreateConventionlessModelBuilder().Model;
            var entityType = model.AddEntityType(typeof(NonNotifyingEntity));
            var id = entityType.AddProperty("Id");
            entityType.SetPrimaryKey(id);

            model.SetChangeTrackingStrategy(changeTrackingStrategy);

            VerifyError(
                CoreStrings.ChangeTrackingInterfaceMissing("NonNotifyingEntity", changeTrackingStrategy, "INotifyPropertyChanged"),
                model);
        }

        [ConditionalTheory]
        [InlineData(ChangeTrackingStrategy.ChangingAndChangedNotifications)]
        [InlineData(ChangeTrackingStrategy.ChangingAndChangedNotificationsWithOriginalValues)]
        public virtual void Detects_changed_only_notifying_entities(ChangeTrackingStrategy changeTrackingStrategy)
        {
            var model = CreateConventionlessModelBuilder().Model;
            var entityType = model.AddEntityType(typeof(ChangedOnlyEntity));
            var id = entityType.AddProperty("Id");
            entityType.SetPrimaryKey(id);

            model.SetChangeTrackingStrategy(changeTrackingStrategy);

            VerifyError(
                CoreStrings.ChangeTrackingInterfaceMissing("ChangedOnlyEntity", changeTrackingStrategy, "INotifyPropertyChanging"),
                model);
        }

        [ConditionalTheory]
        [InlineData(ChangeTrackingStrategy.Snapshot)]
        [InlineData(ChangeTrackingStrategy.ChangedNotifications)]
        [InlineData(ChangeTrackingStrategy.ChangingAndChangedNotifications)]
        [InlineData(ChangeTrackingStrategy.ChangingAndChangedNotificationsWithOriginalValues)]
        public virtual void Passes_for_fully_notifying_entities(ChangeTrackingStrategy changeTrackingStrategy)
        {
            var model = CreateConventionlessModelBuilder().Model;
            var entityType = model.AddEntityType(typeof(FullNotificationEntity));
            var id = entityType.AddProperty("Id");
            entityType.SetPrimaryKey(id);

            model.SetChangeTrackingStrategy(changeTrackingStrategy);

            Validate(model);
        }

        [ConditionalTheory]
        [InlineData(ChangeTrackingStrategy.Snapshot)]
        [InlineData(ChangeTrackingStrategy.ChangedNotifications)]
        public virtual void Passes_for_changed_only_entities_with_snapshot_or_changed_only_tracking(
            ChangeTrackingStrategy changeTrackingStrategy)
        {
            var model = CreateConventionlessModelBuilder().Model;
            var entityType = model.AddEntityType(typeof(ChangedOnlyEntity));
            var id = entityType.AddProperty("Id");
            entityType.SetPrimaryKey(id);

            model.SetChangeTrackingStrategy(changeTrackingStrategy);

            Validate(model);
        }

        [ConditionalFact]
        public virtual void Passes_for_non_notifying_entities_with_snapshot_tracking()
        {
            var model = CreateConventionlessModelBuilder().Model;
            var entityType = model.AddEntityType(typeof(NonNotifyingEntity));
            var id = entityType.AddProperty("Id");
            entityType.SetPrimaryKey(id);

            model.SetChangeTrackingStrategy(ChangeTrackingStrategy.Snapshot);

            Validate(model);
        }

        [ConditionalFact]
        public virtual void Passes_for_valid_seeds()
        {
            var modelBuilder = CreateConventionalModelBuilder();
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

        [ConditionalFact]
        public virtual void Passes_for_ignored_invalid_properties()
        {
            var modelBuilder = CreateConventionalModelBuilder();
            modelBuilder.Entity<EntityWithInvalidProperties>(
                eb =>
                {
                    eb.Ignore(e => e.NotImplemented);

                    eb.HasData(
                        new EntityWithInvalidProperties
                        {
                            Id = -1
                        });

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

            Validate(modelBuilder.Model);

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
                        new D
                        {
                            Id = 2,
                            P0 = 3
                        })).Message);
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
                                new D
                                {
                                    Id = 2,
                                    P0 = 3
                                }))
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
                        new A
                        {
                            Id = 1
                        });
                });

            VerifyError(
                CoreStrings.SeedDatumMissingValue(nameof(A), nameof(A.P0)),
                modelBuilder.Model);
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
                        new A
                        {
                            Id = 1
                        });
                });

            Validate(modelBuilder.Model);
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
                modelBuilder.Model);
        }

        [ConditionalFact]
        public virtual void Detects_missing_signed_integer_key_values_in_seeds()
        {
            var modelBuilder = CreateConventionalModelBuilder();
            modelBuilder.Entity<A>(e => e.HasData(new A()));

            VerifyError(
                CoreStrings.SeedDatumSignedNumericValue(nameof(A), nameof(A.Id)),
                modelBuilder.Model);
        }

        [ConditionalTheory]
        [InlineData(true)]
        [InlineData(false)]
        public virtual void Detects_duplicate_seeds(bool sensitiveDataLoggingEnabled)
        {
            var modelBuilder = CreateConventionalModelBuilder(sensitiveDataLoggingEnabled);
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

        [ConditionalTheory]
        [InlineData(true)]
        [InlineData(false)]
        public virtual void Detects_incompatible_values(bool sensitiveDataLoggingEnabled)
        {
            var modelBuilder = CreateConventionalModelBuilder(sensitiveDataLoggingEnabled);
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

        [ConditionalTheory]
        [InlineData(true)]
        [InlineData(false)]
        public virtual void Detects_reference_navigations_in_seeds(bool sensitiveDataLoggingEnabled)
        {
            var modelBuilder = CreateConventionalModelBuilder(sensitiveDataLoggingEnabled);
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

        [ConditionalTheory]
        [InlineData(true)]
        [InlineData(false)]
        public virtual void Detects_collection_navigations_in_seeds(bool sensitiveDataLoggingEnabled)
        {
            var modelBuilder = CreateConventionalModelBuilder(sensitiveDataLoggingEnabled);
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


        [ConditionalFact]
        public virtual void Detects_missing_discriminator_property()
        {
            var model = CreateConventionlessModelBuilder().Model;

            var entityA = model.AddEntityType(typeof(A));
            SetPrimaryKey(entityA);
            AddProperties(entityA);

            var entityC = model.AddEntityType(typeof(C));
            entityC.BaseType = entityA;

            VerifyError(CoreStrings.NoDiscriminatorProperty(entityA.DisplayName()), model);
        }

        [ConditionalFact]
        public virtual void Detects_missing_discriminator_value_on_base()
        {
            var model = CreateConventionlessModelBuilder().Model;

            var entityA = model.AddEntityType(typeof(A));
            SetPrimaryKey(entityA);
            AddProperties(entityA);

            var entityC = model.AddEntityType(typeof(C));
            SetBaseType(entityC, entityA);

            entityA.SetDiscriminatorProperty(entityA.AddProperty("D", typeof(int)));
            entityC.SetDiscriminatorValue(1);

            VerifyError(CoreStrings.NoDiscriminatorValue(entityA.DisplayName()), model);
        }

        [ConditionalFact]
        public virtual void Detects_missing_discriminator_value_on_leaf()
        {
            var model = CreateConventionlessModelBuilder().Model;

            var entityAbstract = model.AddEntityType(typeof(Abstract));
            SetPrimaryKey(entityAbstract);
            AddProperties(entityAbstract);

            var entityGeneric = model.AddEntityType(typeof(Generic<string>));
            SetBaseType(entityGeneric, entityAbstract);

            entityAbstract.SetDiscriminatorProperty(entityAbstract.AddProperty("D", typeof(int)));
            entityAbstract.SetDiscriminatorValue(0);

            VerifyError(CoreStrings.NoDiscriminatorValue(entityGeneric.DisplayName()), model);
        }

        [ConditionalFact]
        public virtual void Detects_missing_non_string_discriminator_values()
        {
            var modelBuilder = CreateConventionalModelBuilder();
            modelBuilder.Entity<C>();
            modelBuilder.Entity<A>().HasDiscriminator<byte>("ClassType")
                .HasValue<A>(0)
                .HasValue<D>(1);

            var model = modelBuilder.Model;
            VerifyError(CoreStrings.NoDiscriminatorValue(typeof(C).Name), model);
        }

        [ConditionalFact]
        public virtual void Detects_duplicate_discriminator_values()
        {
            var modelBuilder = CreateConventionalModelBuilder();
            modelBuilder.Entity<A>().HasDiscriminator<byte>("ClassType")
                .HasValue<A>(1)
                .HasValue<C>(1)
                .HasValue<D>(2);

            var model = modelBuilder.Model;
            VerifyError(CoreStrings.DuplicateDiscriminatorValue(typeof(C).Name, 1, typeof(A).Name), model);
        }

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

        private class NonSignedIntegerKeyEntity
        {
            public uint Id { get; set; }
        }
    }
}
