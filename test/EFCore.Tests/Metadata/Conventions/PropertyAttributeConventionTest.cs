// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Local
// ReSharper disable UnusedAutoPropertyAccessor.Local
// ReSharper disable ClassNeverInstantiated.Local
namespace Microsoft.EntityFrameworkCore.Metadata.Conventions
{
    public class PropertyAttributeConventionTest
    {
        #region ConcurrencyCheckAttribute

        [ConditionalFact]
        public void ConcurrencyCheckAttribute_overrides_configuration_from_convention_source()
        {
            var entityTypeBuilder = CreateInternalEntityTypeBuilder<A>();

            var propertyBuilder = entityTypeBuilder.Property(typeof(Guid), "RowVersion", ConfigurationSource.Explicit);

            propertyBuilder.IsConcurrencyToken(false, ConfigurationSource.Convention);

            RunConvention(propertyBuilder);

            Assert.True(propertyBuilder.Metadata.IsConcurrencyToken);
        }

        [ConditionalFact]
        public void ConcurrencyCheckAttribute_does_not_override_configuration_from_explicit_source()
        {
            var entityTypeBuilder = CreateInternalEntityTypeBuilder<A>();

            var propertyBuilder = entityTypeBuilder.Property(typeof(Guid), "RowVersion", ConfigurationSource.Explicit);

            propertyBuilder.IsConcurrencyToken(false, ConfigurationSource.Explicit);

            RunConvention(propertyBuilder);

            Assert.False(propertyBuilder.Metadata.IsConcurrencyToken);
        }

        [ConditionalFact]
        public void ConcurrencyCheckAttribute_sets_concurrency_token_with_conventional_builder()
        {
            var modelBuilder = CreateModelBuilder();
            var entityTypeBuilder = modelBuilder.Entity<A>();

            Assert.True(entityTypeBuilder.Property(e => e.RowVersion).Metadata.IsConcurrencyToken);
        }

        [ConditionalFact]
        public void ConcurrencyCheckAttribute_on_field_sets_concurrency_token_with_conventional_builder()
        {
            var modelBuilder = CreateModelBuilder();
            var entityTypeBuilder = modelBuilder.Entity<F>();

            Assert.True(entityTypeBuilder.Property<Guid>(nameof(F.RowVersion)).Metadata.IsConcurrencyToken);
        }

        #endregion

        #region DatabaseGeneratedAttribute

        [ConditionalFact]
        public void DatabaseGeneratedAttribute_overrides_configuration_from_convention_source()
        {
            var entityTypeBuilder = CreateInternalEntityTypeBuilder<A>();

            var propertyBuilder = entityTypeBuilder.Property(typeof(int), "Id", ConfigurationSource.Explicit);

            propertyBuilder.ValueGenerated(ValueGenerated.OnAdd, ConfigurationSource.Convention);

            RunConvention(propertyBuilder);

            Assert.Equal(ValueGenerated.OnAddOrUpdate, propertyBuilder.Metadata.ValueGenerated);
        }

        [ConditionalFact]
        public void DatabaseGeneratedAttribute_does_not_override_configuration_from_explicit_source()
        {
            var entityTypeBuilder = CreateInternalEntityTypeBuilder<A>();

            var propertyBuilder = entityTypeBuilder.Property(typeof(int), "Id", ConfigurationSource.Explicit);

            propertyBuilder.ValueGenerated(ValueGenerated.Never, ConfigurationSource.Explicit);

            RunConvention(propertyBuilder);

            Assert.Equal(ValueGenerated.Never, propertyBuilder.Metadata.ValueGenerated);
        }

        [ConditionalFact]
        public void DatabaseGeneratedAttribute_sets_store_generated_pattern_with_conventional_builder()
        {
            var modelBuilder = CreateModelBuilder();
            var entityTypeBuilder = modelBuilder.Entity<A>();

            Assert.Equal(ValueGenerated.OnAddOrUpdate, entityTypeBuilder.Property(e => e.Id).Metadata.ValueGenerated);
        }

        [ConditionalFact]
        public void DatabaseGeneratedAttribute_in_field_sets_store_generated_pattern_with_conventional_builder()
        {
            var modelBuilder = CreateModelBuilder();
            var entityTypeBuilder = modelBuilder.Entity<F>();

            Assert.Equal(ValueGenerated.OnAddOrUpdate, entityTypeBuilder.Property<int>(nameof(F.Id)).Metadata.ValueGenerated);
        }

        #endregion

        #region KeyAttribute

        [ConditionalFact]
        public void KeyAttribute_overrides_configuration_from_convention_source()
        {
            var entityTypeBuilder = CreateInternalEntityTypeBuilder<A>();

            var propertyBuilder = entityTypeBuilder.Property(typeof(int), "MyPrimaryKey", ConfigurationSource.Explicit);

            entityTypeBuilder.PrimaryKey(
                new List<string>
                {
                    "Id"
                }, ConfigurationSource.Convention);

            RunConvention(propertyBuilder);

            Assert.Equal("MyPrimaryKey", entityTypeBuilder.Metadata.FindPrimaryKey().Properties[0].Name);
        }

        [ConditionalFact]
        public void KeyAttribute_does_not_override_configuration_from_explicit_source()
        {
            var entityTypeBuilder = CreateInternalEntityTypeBuilder<A>();

            var propertyBuilder = entityTypeBuilder.Property(typeof(int), "MyPrimaryKey", ConfigurationSource.Explicit);

            entityTypeBuilder.PrimaryKey(
                new List<string>
                {
                    "Id"
                }, ConfigurationSource.Explicit);

            RunConvention(propertyBuilder);

            Assert.Equal("Id", entityTypeBuilder.Metadata.FindPrimaryKey().Properties[0].Name);
        }

        [ConditionalFact]
        public void KeyAttribute_sets_primary_key_for_single_property()
        {
            var entityTypeBuilder = CreateInternalEntityTypeBuilder<A>();

            var propertyBuilder = entityTypeBuilder.Property(typeof(int), "MyPrimaryKey", ConfigurationSource.Explicit);

            Assert.Null(entityTypeBuilder.Metadata.FindDeclaredPrimaryKey());

            RunConvention(propertyBuilder);

            Assert.Equal(1, entityTypeBuilder.Metadata.FindDeclaredPrimaryKey().Properties.Count);
            Assert.Equal("MyPrimaryKey", entityTypeBuilder.Metadata.FindDeclaredPrimaryKey().Properties[0].Name);
        }

        [ConditionalFact]
        public void KeyAttribute_throws_when_setting_composite_primary_key()
        {
            var entityTypeBuilder = CreateInternalEntityTypeBuilder<B>();

            Assert.Null(entityTypeBuilder.Metadata.FindDeclaredPrimaryKey());

            var idPropertyBuilder = entityTypeBuilder.Property(typeof(int), "Id", ConfigurationSource.Explicit);
            var myPrimaryKeyPropertyBuilder = entityTypeBuilder.Property(typeof(int), "MyPrimaryKey", ConfigurationSource.Explicit);

            RunConvention(idPropertyBuilder);

            Assert.Equal(1, entityTypeBuilder.Metadata.FindDeclaredPrimaryKey().Properties.Count);
            Assert.Equal("Id", entityTypeBuilder.Metadata.FindDeclaredPrimaryKey().Properties[0].Name);

            RunConvention(myPrimaryKeyPropertyBuilder);

            Assert.Equal(2, entityTypeBuilder.Metadata.FindPrimaryKey().Properties.Count);
            Assert.Equal("Id", entityTypeBuilder.Metadata.FindPrimaryKey().Properties[0].Name);
            Assert.Equal("MyPrimaryKey", entityTypeBuilder.Metadata.FindPrimaryKey().Properties[1].Name);

            Assert.Equal(
                CoreStrings.CompositePKWithDataAnnotation(entityTypeBuilder.Metadata.DisplayName()),
                Assert.Throws<InvalidOperationException>(() => Validate(entityTypeBuilder)).Message);
        }

        [ConditionalFact]
        public void KeyAttribute_does_not_throw_when_setting_composite_primary_key_if_fluent_api_used()
        {
            var model = new MyContext().Model;

            Assert.Equal(2, model.FindEntityType(typeof(B)).FindPrimaryKey().Properties.Count);
            Assert.Equal("MyPrimaryKey", model.FindEntityType(typeof(B)).FindPrimaryKey().Properties[0].Name);
            Assert.Equal("Id", model.FindEntityType(typeof(B)).FindPrimaryKey().Properties[1].Name);
        }

        [ConditionalFact]
        public void KeyAttribute_throws_when_setting_key_in_derived_type()
        {
            var derivedEntityTypeBuilder = CreateInternalEntityTypeBuilder<DerivedEntity>();
            var baseEntityType = derivedEntityTypeBuilder.ModelBuilder.Entity(typeof(BaseEntity), ConfigurationSource.Explicit).Metadata;
            derivedEntityTypeBuilder.HasBaseType(baseEntityType, ConfigurationSource.Explicit);

            var propertyBuilder = derivedEntityTypeBuilder.Property(typeof(int), "Number", ConfigurationSource.Explicit);

            Assert.Equal(
                CoreStrings.KeyAttributeOnDerivedEntity(derivedEntityTypeBuilder.Metadata.DisplayName(), propertyBuilder.Metadata.Name),
                Assert.Throws<InvalidOperationException>(() => Validate(derivedEntityTypeBuilder))
                    .Message);
        }

        [ConditionalFact]
        public void KeyAttribute_allows_composite_key_with_inheritance()
        {
            var derivedEntityTypeBuilder = CreateInternalEntityTypeBuilder<CompositeKeyDerivedEntity>();
            var baseEntityTypeBuilder = derivedEntityTypeBuilder.ModelBuilder.Entity(typeof(BaseEntity), ConfigurationSource.Explicit);
            derivedEntityTypeBuilder.HasBaseType(baseEntityTypeBuilder.Metadata, ConfigurationSource.Explicit);

            baseEntityTypeBuilder.PrimaryKey(
                new List<string>
                {
                    "Id",
                    "Name"
                }, ConfigurationSource.Explicit);

            Validate(derivedEntityTypeBuilder);

            Assert.Equal(2, baseEntityTypeBuilder.Metadata.FindPrimaryKey().Properties.Count);
        }

        [ConditionalFact]
        public void KeyAttribute_on_field_sets_primary_key()
        {
            var modelBuilder = CreateModelBuilder();
            var entityTypeBuilder = modelBuilder.Entity<F>();
            entityTypeBuilder.Property<int>(nameof(F.MyPrimaryKey));

            Assert.Equal(nameof(F.MyPrimaryKey), entityTypeBuilder.Metadata.FindPrimaryKey().Properties.Single().Name);
        }

        #endregion

        #region MaxLengthAttribute

        [ConditionalFact]
        public void MaxLengthAttribute_overrides_configuration_from_convention_source()
        {
            var entityTypeBuilder = CreateInternalEntityTypeBuilder<A>();

            var propertyBuilder = entityTypeBuilder.Property(typeof(string), "MaxLengthProperty", ConfigurationSource.Explicit);

            propertyBuilder.HasMaxLength(100, ConfigurationSource.Convention);

            RunConvention(propertyBuilder);

            Assert.Equal(10, propertyBuilder.Metadata.GetMaxLength());
        }

        [ConditionalFact]
        public void MaxLengthAttribute_does_not_override_configuration_from_explicit_source()
        {
            var entityTypeBuilder = CreateInternalEntityTypeBuilder<A>();

            var propertyBuilder = entityTypeBuilder.Property(typeof(string), "MaxLengthProperty", ConfigurationSource.Explicit);

            propertyBuilder.HasMaxLength(100, ConfigurationSource.Explicit);

            RunConvention(propertyBuilder);

            Assert.Equal(100, propertyBuilder.Metadata.GetMaxLength());
        }

        [ConditionalFact]
        public void MaxLengthAttribute_sets_max_length_with_conventional_builder()
        {
            var modelBuilder = CreateModelBuilder();
            var entityTypeBuilder = modelBuilder.Entity<A>();

            Assert.Equal(10, entityTypeBuilder.Property(e => e.MaxLengthProperty).Metadata.GetMaxLength());
        }

        [ConditionalFact]
        public void MaxLengthAttribute_on_field_sets_max_length_with_conventional_builder()
        {
            var modelBuilder = CreateModelBuilder();
            var entityTypeBuilder = modelBuilder.Entity<F>();

            Assert.Equal(10, entityTypeBuilder.Property<string>(nameof(F.MaxLengthProperty)).Metadata.GetMaxLength());
        }

        #endregion

        #region NotMappedAttribute

        [ConditionalFact]
        public void NotMappedAttribute_overrides_configuration_from_convention_source()
        {
            var entityTypeBuilder = CreateInternalEntityTypeBuilder<A>();
            entityTypeBuilder.Property(typeof(string), "IgnoredProperty", ConfigurationSource.Convention);

            RunConvention(entityTypeBuilder);

            Assert.False(entityTypeBuilder.Metadata.GetProperties().Any(p => p.Name == "IgnoredProperty"));
        }

        [ConditionalFact]
        public void NotMappedAttribute_does_not_override_configuration_from_explicit_source()
        {
            var entityTypeBuilder = CreateInternalEntityTypeBuilder<A>();
            entityTypeBuilder.Property(typeof(string), "IgnoredProperty", ConfigurationSource.Explicit);

            RunConvention(entityTypeBuilder);

            Assert.True(entityTypeBuilder.Metadata.GetProperties().Any(p => p.Name == "IgnoredProperty"));
        }

        [ConditionalFact]
        public void NotMappedAttribute_ignores_property_with_conventional_builder()
        {
            var modelBuilder = CreateModelBuilder();
            var entityTypeBuilder = modelBuilder.Entity<A>();

            Assert.False(entityTypeBuilder.Metadata.GetProperties().Any(p => p.Name == "IgnoredProperty"));
        }

        [ConditionalFact]
        public void NotMappedAttribute_on_field_does_not_ignore_property_with_conventional_builder()
        {
            var modelBuilder = CreateModelBuilder();
            var entityTypeBuilder = modelBuilder.Entity<F>();
            entityTypeBuilder.Property<string>(nameof(F.IgnoredProperty));

            // Because bringing the property in by the fluent API overrides the annotation it has no effect
            Assert.True(entityTypeBuilder.Metadata.GetProperties().Any(p => p.Name == "IgnoredProperty"));
        }

        [ConditionalFact]
        public void NotMappedAttribute_on_field_overrides_configuration_from_convention_source()
        {
            var entityTypeBuilder = CreateInternalEntityTypeBuilder<F>();
            entityTypeBuilder.Property(typeof(string), "IgnoredProperty", ConfigurationSource.Convention);

            RunConvention(entityTypeBuilder);

            Assert.False(entityTypeBuilder.Metadata.GetProperties().Any(p => p.Name == "IgnoredProperty"));
        }

        #endregion

        #region RequiredAttribute

        [ConditionalFact]
        public void RequiredAttribute_overrides_configuration_from_convention_source()
        {
            var entityTypeBuilder = CreateInternalEntityTypeBuilder<A>();

            var propertyBuilder = entityTypeBuilder.Property(typeof(string), "Name", ConfigurationSource.Explicit);

            propertyBuilder.IsRequired(false, ConfigurationSource.Convention);

            RunConvention(propertyBuilder);

            Assert.False(propertyBuilder.Metadata.IsNullable);
        }

        [ConditionalFact]
        public void RequiredAttribute_does_not_override_configuration_from_explicit_source()
        {
            var entityTypeBuilder = CreateInternalEntityTypeBuilder<A>();

            var propertyBuilder = entityTypeBuilder.Property(typeof(string), "Name", ConfigurationSource.Explicit);

            propertyBuilder.IsRequired(false, ConfigurationSource.Explicit);

            RunConvention(propertyBuilder);

            Assert.True(propertyBuilder.Metadata.IsNullable);
        }

        [ConditionalFact]
        public void RequiredAttribute_sets_is_nullable_with_conventional_builder()
        {
            var modelBuilder = CreateModelBuilder();
            var entityTypeBuilder = modelBuilder.Entity<A>();

            Assert.False(entityTypeBuilder.Property(e => e.Name).Metadata.IsNullable);
        }

        [ConditionalFact]
        public void RequiredAttribute_on_field_sets_is_nullable_with_conventional_builder()
        {
            var modelBuilder = CreateModelBuilder();
            var entityTypeBuilder = modelBuilder.Entity<F>();

            Assert.False(entityTypeBuilder.Property<string>(nameof(F.Name)).Metadata.IsNullable);
        }

        #endregion

        #region StringLengthAttribute

        [ConditionalFact]
        public void StringLengthAttribute_overrides_configuration_from_convention_source()
        {
            var entityTypeBuilder = CreateInternalEntityTypeBuilder<A>();

            var propertyBuilder = entityTypeBuilder.Property(typeof(string), "StringLengthProperty", ConfigurationSource.Explicit);

            propertyBuilder.HasMaxLength(100, ConfigurationSource.Convention);

            RunConvention(propertyBuilder);

            Assert.Equal(20, propertyBuilder.Metadata.GetMaxLength());
        }

        [ConditionalFact]
        public void StringLengthAttribute_does_not_override_configuration_from_explicit_source()
        {
            var entityTypeBuilder = CreateInternalEntityTypeBuilder<A>();

            var propertyBuilder = entityTypeBuilder.Property(typeof(string), "StringLengthProperty", ConfigurationSource.Explicit);

            propertyBuilder.HasMaxLength(100, ConfigurationSource.Explicit);

            RunConvention(propertyBuilder);

            Assert.Equal(100, propertyBuilder.Metadata.GetMaxLength());
        }

        [ConditionalFact]
        public void StringLengthAttribute_sets_max_length_with_conventional_builder()
        {
            var modelBuilder = CreateModelBuilder();
            var entityTypeBuilder = modelBuilder.Entity<A>();

            Assert.Equal(20, entityTypeBuilder.Property(e => e.StringLengthProperty).Metadata.GetMaxLength());
        }

        [ConditionalFact]
        public void StringLengthAttribute_on_field_sets_max_length_with_conventional_builder()
        {
            var modelBuilder = CreateModelBuilder();
            var entityTypeBuilder = modelBuilder.Entity<F>();

            Assert.Equal(20, entityTypeBuilder.Property<string>(nameof(F.StringLengthProperty)).Metadata.GetMaxLength());
        }

        #endregion

        #region TimestampAttribute

        [ConditionalFact]
        public void TimestampAttribute_overrides_configuration_from_convention_source()
        {
            var entityTypeBuilder = CreateInternalEntityTypeBuilder<A>();

            var propertyBuilder = entityTypeBuilder.Property(typeof(byte[]), "Timestamp", ConfigurationSource.Explicit);

            propertyBuilder.ValueGenerated(ValueGenerated.Never, ConfigurationSource.Convention);
            propertyBuilder.IsConcurrencyToken(false, ConfigurationSource.Convention);

            RunConvention(propertyBuilder);

            Assert.Equal(ValueGenerated.OnAddOrUpdate, propertyBuilder.Metadata.ValueGenerated);
            Assert.True(propertyBuilder.Metadata.IsConcurrencyToken);
        }

        [ConditionalFact]
        public void TimestampAttribute_does_not_override_configuration_from_explicit_source()
        {
            var entityTypeBuilder = CreateInternalEntityTypeBuilder<A>();

            var propertyBuilder = entityTypeBuilder.Property(typeof(byte[]), "Timestamp", ConfigurationSource.Explicit);

            propertyBuilder.ValueGenerated(ValueGenerated.Never, ConfigurationSource.Explicit);
            propertyBuilder.IsConcurrencyToken(false, ConfigurationSource.Explicit);

            RunConvention(propertyBuilder);

            Assert.Equal(ValueGenerated.Never, propertyBuilder.Metadata.ValueGenerated);
            Assert.False(propertyBuilder.Metadata.IsConcurrencyToken);
        }

        [ConditionalFact]
        public void TimestampAttribute_sets_concurrency_token_with_conventional_builder()
        {
            var modelBuilder = CreateModelBuilder();
            var entityTypeBuilder = modelBuilder.Entity<A>();

            Assert.Equal(ValueGenerated.OnAddOrUpdate, entityTypeBuilder.Property(e => e.Timestamp).Metadata.ValueGenerated);
            Assert.True(entityTypeBuilder.Property(e => e.Timestamp).Metadata.IsConcurrencyToken);
        }

        [ConditionalFact]
        public void TimestampAttribute_on_field_sets_concurrency_token_with_conventional_builder()
        {
            var modelBuilder = CreateModelBuilder();
            var entityTypeBuilder = modelBuilder.Entity<F>();

            Assert.Equal(ValueGenerated.OnAddOrUpdate, entityTypeBuilder.Property<byte[]>(nameof(F.Timestamp)).Metadata.ValueGenerated);
            Assert.True(entityTypeBuilder.Property<byte[]>(nameof(F.Timestamp)).Metadata.IsConcurrencyToken);
        }

        #endregion

        [ConditionalFact]
        public void Property_attribute_convention_runs_for_private_property()
        {
            var modelBuilder = CreateModelBuilder();
            var propertyBuilder = modelBuilder.Entity<A>().Property<int?>("PrivateProperty");

            Assert.False(propertyBuilder.Metadata.IsNullable);
        }

        private InternalEntityTypeBuilder CreateInternalEntityTypeBuilder<T>()
        {
            var conventionSet = new ConventionSet();
            conventionSet.EntityTypeAddedConventions.Add(
                new PropertyDiscoveryConvention(CreateDependencies()));

            var modelBuilder = new InternalModelBuilder(new Model(conventionSet));

            return modelBuilder.Entity(typeof(T), ConfigurationSource.Explicit);
        }

        private static void RunConvention(InternalPropertyBuilder propertyBuilder)
        {
            var dependencies = CreateDependencies();
            var context = new ConventionContext<IConventionPropertyBuilder>(
                propertyBuilder.Metadata.DeclaringEntityType.Model.ConventionDispatcher);

            new ConcurrencyCheckAttributeConvention(dependencies)
                .ProcessPropertyAdded(propertyBuilder, context);

            new DatabaseGeneratedAttributeConvention(dependencies)
                .ProcessPropertyAdded(propertyBuilder, context);

            new KeyAttributeConvention(dependencies)
                .ProcessPropertyAdded(propertyBuilder, context);

            new MaxLengthAttributeConvention(dependencies)
                .ProcessPropertyAdded(propertyBuilder, context);

            new RequiredPropertyAttributeConvention(dependencies)
                .ProcessPropertyAdded(propertyBuilder, context);

            new StringLengthAttributeConvention(dependencies)
                .ProcessPropertyAdded(propertyBuilder, context);

            new TimestampAttributeConvention(dependencies)
                .ProcessPropertyAdded(propertyBuilder, context);
        }

        private void RunConvention(InternalEntityTypeBuilder entityTypeBuilder)
        {
            var context = new ConventionContext<IConventionEntityTypeBuilder>(entityTypeBuilder.Metadata.Model.ConventionDispatcher);

            new NotMappedMemberAttributeConvention(CreateDependencies())
                .ProcessEntityTypeAdded(entityTypeBuilder, context);
        }

        private void Validate(InternalEntityTypeBuilder entityTypeBuilder)
        {
            var context = new ConventionContext<IConventionModelBuilder>(
                entityTypeBuilder.Metadata.Model.ConventionDispatcher);

            new KeyAttributeConvention(CreateDependencies())
                .ProcessModelFinalized(entityTypeBuilder.ModelBuilder, context);
        }

        private static ProviderConventionSetBuilderDependencies CreateDependencies()
            => InMemoryTestHelpers.Instance.CreateContextServices().GetRequiredService<ProviderConventionSetBuilderDependencies>();

        private class A
        {
            [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
            public int Id { get; set; }

            [ConcurrencyCheck]
            public Guid RowVersion { get; set; }

            [Required]
            public string Name { get; set; }

            [Key]
            public int MyPrimaryKey { get; set; }

            [NotMapped]
            public string IgnoredProperty { get; set; }

            [MaxLength(10)]
            public string MaxLengthProperty { get; set; }

            [StringLength(20)]
            public string StringLengthProperty { get; set; }

            [Timestamp]
            public byte[] Timestamp { get; set; }

            [Required]
            private int? PrivateProperty { get; set; }
        }

        private class B
        {
            [Key]
            public int Id { get; set; }

            [Key]
            public int MyPrimaryKey { get; set; }
        }

        public class F
        {
            [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
            public int Id;

            [ConcurrencyCheck]
            public Guid RowVersion;

            [Required]
            public string Name;

            [Key]
            public int MyPrimaryKey;

            [NotMapped]
            public string IgnoredProperty;

            [MaxLength(10)]
            public string MaxLengthProperty;

            [StringLength(20)]
            public string StringLengthProperty;

            [Timestamp]
            public byte[] Timestamp;
        }

        private class BaseEntity
        {
            public int Id { get; set; }

            public string Name { get; set; }
        }

        private class DerivedEntity : BaseEntity
        {
            [Key]
            public int Number { get; set; }
        }

        private class CompositeKeyDerivedEntity : BaseEntity
        {
        }

        private static ModelBuilder CreateModelBuilder() => InMemoryTestHelpers.Instance.CreateConventionBuilder();

        private class MyContext : DbContext
        {
            protected internal override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
                => optionsBuilder
                    .UseInternalServiceProvider(InMemoryFixture.DefaultServiceProvider)
                    .UseInMemoryDatabase(nameof(MyContext));

            protected internal override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<B>().HasKey(
                    e => new
                    {
                        e.MyPrimaryKey,
                        e.Id
                    });
        }
    }
}
