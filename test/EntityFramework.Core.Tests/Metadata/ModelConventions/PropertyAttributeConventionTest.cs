// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Metadata.Conventions.Internal;
using Microsoft.Data.Entity.Metadata.Internal;
using Xunit;

namespace Microsoft.Data.Entity.Metadata.Conventions
{
    public class PropertyAttributeConventionTest
    {
        #region ConcurrencyCheckAttribute

        [Fact]
        public void ConcurrencyCheckAttribute_overrides_configuration_from_convention_source()
        {
            var entityTypeBuilder = CreateInternalEntityTypeBuilder<A>();

            var propertyBuilder = entityTypeBuilder.Property("RowVersion", typeof(Guid), ConfigurationSource.Explicit);

            propertyBuilder.ConcurrencyToken(false, ConfigurationSource.Convention);

            new ConcurrencyCheckAttributeConvention().Apply(propertyBuilder);

            Assert.True(propertyBuilder.Metadata.IsConcurrencyToken);
        }

        [Fact]
        public void ConcurrencyCheckAttribute_does_not_override_configuration_from_explicit_source()
        {
            var entityTypeBuilder = CreateInternalEntityTypeBuilder<A>();

            var propertyBuilder = entityTypeBuilder.Property("RowVersion", typeof(Guid), ConfigurationSource.Explicit);

            propertyBuilder.ConcurrencyToken(false, ConfigurationSource.Explicit);

            new ConcurrencyCheckAttributeConvention().Apply(propertyBuilder);

            Assert.False(propertyBuilder.Metadata.IsConcurrencyToken);
        }

        [Fact]
        public void ConcurrencyCheckAttribute_sets_concurrency_token_with_conventional_builder()
        {
            var modelBuilder = new ModelBuilder(new CoreConventionSetBuilder().CreateConventionSet());
            var entityTypeBuilder = modelBuilder.Entity<A>();

            Assert.True(entityTypeBuilder.Property(e => e.RowVersion).Metadata.IsConcurrencyToken);
        }

        #endregion

        #region DatabaseGeneratedAttribute

        [Fact]
        public void DatabaseGeneratedAttribute_overrides_configuration_from_convention_source()
        {
            var entityTypeBuilder = CreateInternalEntityTypeBuilder<A>();

            var propertyBuilder = entityTypeBuilder.Property("Id", typeof(int), ConfigurationSource.Explicit);

            propertyBuilder.ValueGenerated(ValueGenerated.OnAdd, ConfigurationSource.Convention);

            new DatabaseGeneratedAttributeConvention().Apply(propertyBuilder);

            Assert.Equal(ValueGenerated.OnAddOrUpdate, propertyBuilder.Metadata.ValueGenerated);
        }

        [Fact]
        public void DatabaseGeneratedAttribute_does_not_override_configuration_from_explicit_source()
        {
            var entityTypeBuilder = CreateInternalEntityTypeBuilder<A>();

            var propertyBuilder = entityTypeBuilder.Property("Id", typeof(int), ConfigurationSource.Explicit);

            propertyBuilder.ValueGenerated(ValueGenerated.Never, ConfigurationSource.Explicit);

            new DatabaseGeneratedAttributeConvention().Apply(propertyBuilder);

            Assert.Equal(ValueGenerated.Never, propertyBuilder.Metadata.ValueGenerated);
        }

        [Fact]
        public void DatabaseGeneratedAttribute_sets_store_generated_pattern_with_conventional_builder()
        {
            var modelBuilder = new ModelBuilder(new CoreConventionSetBuilder().CreateConventionSet());
            var entityTypeBuilder = modelBuilder.Entity<A>();

            Assert.Equal(ValueGenerated.OnAddOrUpdate, entityTypeBuilder.Property(e => e.Id).Metadata.ValueGenerated);
        }

        #endregion

        #region KeyAttribute

        [Fact]
        public void KeyAttribute_overrides_configuration_from_convention_source()
        {
            var entityTypeBuilder = CreateInternalEntityTypeBuilder<A>();

            var propertyBuilder = entityTypeBuilder.Property("MyPrimaryKey", typeof(int), ConfigurationSource.Explicit);

            entityTypeBuilder.PrimaryKey(new List<string> { "Id" }, ConfigurationSource.Convention);

            new KeyAttributeConvention().Apply(propertyBuilder);

            Assert.Equal("MyPrimaryKey", entityTypeBuilder.Metadata.GetPrimaryKey().Properties[0].Name);
        }

        [Fact]
        public void KeyAttribute_does_not_override_configuration_from_explicit_source()
        {
            var entityTypeBuilder = CreateInternalEntityTypeBuilder<A>();

            var propertyBuilder = entityTypeBuilder.Property("MyPrimaryKey", typeof(int), ConfigurationSource.Explicit);

            entityTypeBuilder.PrimaryKey(new List<string> { "Id" }, ConfigurationSource.Explicit);

            new KeyAttributeConvention().Apply(propertyBuilder);

            Assert.Equal("Id", entityTypeBuilder.Metadata.GetPrimaryKey().Properties[0].Name);
        }

        [Fact]
        public void KeyAttribute_sets_primary_key_for_single_property()
        {
            var entityTypeBuilder = CreateInternalEntityTypeBuilder<A>();

            var propertyBuilder = entityTypeBuilder.Property("MyPrimaryKey", typeof(int), ConfigurationSource.Explicit);

            Assert.Null(entityTypeBuilder.Metadata.FindDeclaredPrimaryKey());

            new KeyAttributeConvention().Apply(propertyBuilder);

            Assert.Equal(1, entityTypeBuilder.Metadata.FindDeclaredPrimaryKey().Properties.Count);
            Assert.Equal("MyPrimaryKey", entityTypeBuilder.Metadata.FindDeclaredPrimaryKey().Properties[0].Name);
        }

        [Fact]
        public void KeyAttribute_throws_when_setting_composite_primary_key()
        {
            var entityTypeBuilder = CreateInternalEntityTypeBuilder<B>();
            var keyAttributeConvention = new KeyAttributeConvention();

            Assert.Null(entityTypeBuilder.Metadata.FindDeclaredPrimaryKey());

            var idPropertyBuilder = entityTypeBuilder.Property("Id", typeof(int), ConfigurationSource.Explicit);
            var myPrimaryKeyPropertyBuilder = entityTypeBuilder.Property("MyPrimaryKey", typeof(int), ConfigurationSource.Explicit);

            keyAttributeConvention.Apply(idPropertyBuilder);

            Assert.Equal(1, entityTypeBuilder.Metadata.FindDeclaredPrimaryKey().Properties.Count);
            Assert.Equal("Id", entityTypeBuilder.Metadata.FindDeclaredPrimaryKey().Properties[0].Name);

            keyAttributeConvention.Apply(myPrimaryKeyPropertyBuilder);

            Assert.Equal(2, entityTypeBuilder.Metadata.GetPrimaryKey().Properties.Count);
            Assert.Equal("Id", entityTypeBuilder.Metadata.GetPrimaryKey().Properties[0].Name);
            Assert.Equal("MyPrimaryKey", entityTypeBuilder.Metadata.GetPrimaryKey().Properties[1].Name);

            Assert.Equal(
                Strings.CompositePKWithDataAnnotation(entityTypeBuilder.Metadata.DisplayName()),
                Assert.Throws<InvalidOperationException>(() => keyAttributeConvention.Apply(entityTypeBuilder.ModelBuilder)).Message);
        }

        [Fact]
        public void KeyAttribute_does_not_throw_when_setting_composite_primary_key_if_fluent_api_used()
        {
            var model = new MyContext().Model;

            Assert.Equal(2, model.GetEntityType(typeof(B)).GetPrimaryKey().Properties.Count);
            Assert.Equal("MyPrimaryKey", model.GetEntityType(typeof(B)).GetPrimaryKey().Properties[0].Name);
            Assert.Equal("Id", model.GetEntityType(typeof(B)).GetPrimaryKey().Properties[1].Name);
        }

        [Fact]
        public void KeyAttribute_throws_when_setting_key_in_derived_type()
        {
            var derivedEntityTypeBuilder = CreateInternalEntityTypeBuilder<DerivedEntity>();
            var baseEntityType = derivedEntityTypeBuilder.ModelBuilder.Entity(typeof(BaseEntity), ConfigurationSource.Explicit).Metadata;
            derivedEntityTypeBuilder.BaseType(baseEntityType, ConfigurationSource.Explicit);

            var propertyBuilder = derivedEntityTypeBuilder.Property("Number", typeof(int), ConfigurationSource.Explicit);

            Assert.Equal(
                Strings.KeyAttributeOnDerivedEntity(derivedEntityTypeBuilder.Metadata.DisplayName(), propertyBuilder.Metadata.Name),
                Assert.Throws<InvalidOperationException>(() => new KeyAttributeConvention().Apply(propertyBuilder)).Message);
        }

        #endregion

        #region MaxLengthAttribute

        [Fact]
        public void MaxLengthAttribute_overrides_configuration_from_convention_source()
        {
            var entityTypeBuilder = CreateInternalEntityTypeBuilder<A>();

            var propertyBuilder = entityTypeBuilder.Property("MaxLengthProperty", typeof(string), ConfigurationSource.Explicit);

            propertyBuilder.MaxLength(100, ConfigurationSource.Convention);

            new MaxLengthAttributeConvention().Apply(propertyBuilder);

            Assert.Equal(10, propertyBuilder.Metadata.GetMaxLength());
        }

        [Fact]
        public void MaxLengthAttribute_does_not_override_configuration_from_explicit_source()
        {
            var entityTypeBuilder = CreateInternalEntityTypeBuilder<A>();

            var propertyBuilder = entityTypeBuilder.Property("MaxLengthProperty", typeof(string), ConfigurationSource.Explicit);

            propertyBuilder.MaxLength(100, ConfigurationSource.Explicit);

            new MaxLengthAttributeConvention().Apply(propertyBuilder);

            Assert.Equal(100, propertyBuilder.Metadata.GetMaxLength());
        }

        [Fact]
        public void MaxLengthAttribute_set_max_length_with_conventional_builder()
        {
            var modelBuilder = new ModelBuilder(new CoreConventionSetBuilder().CreateConventionSet());
            var entityTypeBuilder = modelBuilder.Entity<A>();

            Assert.Equal(10, entityTypeBuilder.Property(e => e.MaxLengthProperty).Metadata.GetMaxLength());
        }

        #endregion

        #region NotMappedAttribute

        [Fact]
        public void NotMappedAttribute_overrides_configuration_from_convention_source()
        {
            var entityTypeBuilder = CreateInternalEntityTypeBuilder<A>();
            entityTypeBuilder.Property("IgnoredProperty", typeof(string), ConfigurationSource.Convention);

            entityTypeBuilder = new NotMappedMemberAttributeConvention().Apply(entityTypeBuilder);

            Assert.False(entityTypeBuilder.Metadata.Properties.Any(p => p.Name == "IgnoredProperty"));
        }

        [Fact]
        public void NotMappedAttribute_does_not_override_configuration_from_explicit_source()
        {
            var entityTypeBuilder = CreateInternalEntityTypeBuilder<A>();
            entityTypeBuilder.Property("IgnoredProperty", typeof(string), ConfigurationSource.Explicit);

            entityTypeBuilder = new NotMappedMemberAttributeConvention().Apply(entityTypeBuilder);

            Assert.True(entityTypeBuilder.Metadata.Properties.Any(p => p.Name == "IgnoredProperty"));
        }

        [Fact]
        public void NotMappedAttribute_ignores_property_with_conventional_builder()
        {
            var modelBuilder = new ModelBuilder(new CoreConventionSetBuilder().CreateConventionSet());
            var entityTypeBuilder = modelBuilder.Entity<A>();

            Assert.False(entityTypeBuilder.Metadata.Properties.Any(p => p.Name == "IgnoredProperty"));
        }

        #endregion

        #region RequiredAttribute

        [Fact]
        public void RequiredAttribute_overrides_configuration_from_convention_source()
        {
            var entityTypeBuilder = CreateInternalEntityTypeBuilder<A>();

            var propertyBuilder = entityTypeBuilder.Property("Name", typeof(string), ConfigurationSource.Explicit);

            propertyBuilder.Required(false, ConfigurationSource.Convention);

            new RequiredPropertyAttributeConvention().Apply(propertyBuilder);

            Assert.False(propertyBuilder.Metadata.IsNullable);
        }

        [Fact]
        public void RequiredAttribute_does_not_override_configuration_from_explicit_source()
        {
            var entityTypeBuilder = CreateInternalEntityTypeBuilder<A>();

            var propertyBuilder = entityTypeBuilder.Property("Name", typeof(string), ConfigurationSource.Explicit);

            propertyBuilder.Required(false, ConfigurationSource.Explicit);

            new RequiredPropertyAttributeConvention().Apply(propertyBuilder);

            Assert.True(propertyBuilder.Metadata.IsNullable);
        }

        [Fact]
        public void RequiredAttribute_sets_is_nullable_with_conventional_builder()
        {
            var modelBuilder = new ModelBuilder(new CoreConventionSetBuilder().CreateConventionSet());
            var entityTypeBuilder = modelBuilder.Entity<A>();

            Assert.False(entityTypeBuilder.Property(e => e.Name).Metadata.IsNullable);
        }

        #endregion

        #region StringLengthAttribute

        [Fact]
        public void StringLengthAttribute_overrides_configuration_from_convention_source()
        {
            var entityTypeBuilder = CreateInternalEntityTypeBuilder<A>();

            var propertyBuilder = entityTypeBuilder.Property("StringLengthProperty", typeof(string), ConfigurationSource.Explicit);

            propertyBuilder.MaxLength(100, ConfigurationSource.Convention);

            new StringLengthAttributeConvention().Apply(propertyBuilder);

            Assert.Equal(20, propertyBuilder.Metadata.GetMaxLength());
        }

        [Fact]
        public void StringLengthAttribute_does_not_override_configuration_from_explicit_source()
        {
            var entityTypeBuilder = CreateInternalEntityTypeBuilder<A>();

            var propertyBuilder = entityTypeBuilder.Property("StringLengthProperty", typeof(string), ConfigurationSource.Explicit);

            propertyBuilder.MaxLength(100, ConfigurationSource.Explicit);

            new StringLengthAttributeConvention().Apply(propertyBuilder);

            Assert.Equal(100, propertyBuilder.Metadata.GetMaxLength());
        }

        [Fact]
        public void StringLengthAttribute_set_max_length_with_conventional_builder()
        {
            var modelBuilder = new ModelBuilder(new CoreConventionSetBuilder().CreateConventionSet());
            var entityTypeBuilder = modelBuilder.Entity<A>();

            Assert.Equal(20, entityTypeBuilder.Property(e => e.StringLengthProperty).Metadata.GetMaxLength());
        }

        #endregion

        #region TimestampAttribute

        [Fact]
        public void TimestampAttribute_overrides_configuration_from_convention_source()
        {
            var entityTypeBuilder = CreateInternalEntityTypeBuilder<A>();

            var propertyBuilder = entityTypeBuilder.Property("Timestamp", typeof(byte[]), ConfigurationSource.Explicit);

            propertyBuilder.ValueGenerated(ValueGenerated.Never, ConfigurationSource.Convention);
            propertyBuilder.ConcurrencyToken(false, ConfigurationSource.Convention);

            new TimestampAttributeConvention().Apply(propertyBuilder);

            Assert.Equal(ValueGenerated.OnAddOrUpdate, propertyBuilder.Metadata.ValueGenerated);
            Assert.True(propertyBuilder.Metadata.IsConcurrencyToken);
        }

        [Fact]
        public void TimestampAttribute_does_not_override_configuration_from_explicit_source()
        {
            var entityTypeBuilder = CreateInternalEntityTypeBuilder<A>();

            var propertyBuilder = entityTypeBuilder.Property("Timestamp", typeof(byte[]), ConfigurationSource.Explicit);

            propertyBuilder.ValueGenerated(ValueGenerated.Never, ConfigurationSource.Explicit);
            propertyBuilder.ConcurrencyToken(false, ConfigurationSource.Explicit);

            new TimestampAttributeConvention().Apply(propertyBuilder);

            Assert.Equal(ValueGenerated.Never, propertyBuilder.Metadata.ValueGenerated);
            Assert.False(propertyBuilder.Metadata.IsConcurrencyToken);
        }

        [Fact]
        public void TimestampAttribute_sets_concurrency_token_with_conventional_builder()
        {
            var modelBuilder = new ModelBuilder(new CoreConventionSetBuilder().CreateConventionSet());
            var entityTypeBuilder = modelBuilder.Entity<A>();

            Assert.Equal(ValueGenerated.OnAddOrUpdate, entityTypeBuilder.Property(e => e.Timestamp).Metadata.ValueGenerated);
            Assert.True(entityTypeBuilder.Property(e => e.Timestamp).Metadata.IsConcurrencyToken);
        }

        #endregion

        [Fact]
        public void Property_attribute_convention_runs_for_private_property()
        {
            var modelBuilder = new ModelBuilder(new CoreConventionSetBuilder().CreateConventionSet());
            var propertyBuilder = modelBuilder.Entity<A>().Property<int?>("PrivateProperty");

            Assert.False(propertyBuilder.Metadata.IsNullable);
        }

        private InternalEntityTypeBuilder CreateInternalEntityTypeBuilder<T>()
        {
            var conventionSet = new ConventionSet();
            conventionSet.EntityTypeAddedConventions.Add(new PropertyDiscoveryConvention());

            var modelBuilder = new InternalModelBuilder(new Model(), conventionSet);

            return modelBuilder.Entity(typeof(T), ConfigurationSource.Explicit);
        }

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

        private class BaseEntity
        {
            public int Id { get; set; }
        }

        private class DerivedEntity : BaseEntity
        {
            [Key]
            public int Number { get; set; }
        }

        private class MyContext : DbContext
        {
            protected internal override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            {
                optionsBuilder.UseInMemoryDatabase();
            }

            protected internal override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<B>().Key(e => new { e.MyPrimaryKey, e.Id });
            }
        }
    }
}
