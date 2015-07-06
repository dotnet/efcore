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
            var entityBuilder = CreateInternalEntityTypeBuilder<A>();

            var propertyBuilder = entityBuilder.Property(typeof(Guid), "RowVersion", ConfigurationSource.Explicit);

            propertyBuilder.ConcurrencyToken(false, ConfigurationSource.Convention);

            new ConcurrencyCheckAttributeConvention().Apply(propertyBuilder);

            Assert.True(propertyBuilder.Metadata.IsConcurrencyToken);
        }

        [Fact]
        public void ConcurrencyCheckAttribute_does_not_override_configuration_from_explicit_source()
        {
            var entityBuilder = CreateInternalEntityTypeBuilder<A>();

            var propertyBuilder = entityBuilder.Property(typeof(Guid), "RowVersion", ConfigurationSource.Explicit);

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
            var entityBuilder = CreateInternalEntityTypeBuilder<A>();

            var propertyBuilder = entityBuilder.Property(typeof(int), "Id", ConfigurationSource.Explicit);

            propertyBuilder.StoreGeneratedPattern(StoreGeneratedPattern.Identity, ConfigurationSource.Convention);

            new DatabaseGeneratedAttributeConvention().Apply(propertyBuilder);

            Assert.Equal(StoreGeneratedPattern.Computed, propertyBuilder.Metadata.StoreGeneratedPattern);
        }

        [Fact]
        public void DatabaseGeneratedAttribute_does_not_override_configuration_from_explicit_source()
        {
            var entityBuilder = CreateInternalEntityTypeBuilder<A>();

            var propertyBuilder = entityBuilder.Property(typeof(int), "Id", ConfigurationSource.Explicit);

            propertyBuilder.StoreGeneratedPattern(StoreGeneratedPattern.None, ConfigurationSource.Explicit);

            new DatabaseGeneratedAttributeConvention().Apply(propertyBuilder);

            Assert.Equal(StoreGeneratedPattern.None, propertyBuilder.Metadata.StoreGeneratedPattern);
        }

        [Fact]
        public void DatabaseGeneratedAttribute_sets_store_generated_pattern_with_conventional_builder()
        {
            var modelBuilder = new ModelBuilder(new CoreConventionSetBuilder().CreateConventionSet());
            var entityTypeBuilder = modelBuilder.Entity<A>();

            Assert.Equal(StoreGeneratedPattern.Computed, entityTypeBuilder.Property(e => e.Id).Metadata.StoreGeneratedPattern);
        }

        #endregion

        #region KeyAttribute

        [Fact]
        public void KeyAttribute_overrides_configuration_from_convention_source()
        {
            var entityBuilder = CreateInternalEntityTypeBuilder<A>();

            var propertyBuilder = entityBuilder.Property(typeof(int), "MyPrimaryKey", ConfigurationSource.Explicit);

            entityBuilder.PrimaryKey(new List<string> { "Id" }, ConfigurationSource.Convention);

            new KeyAttributeConvention().Apply(propertyBuilder);

            Assert.Equal("MyPrimaryKey", entityBuilder.Metadata.GetPrimaryKey().Properties[0].Name);
        }

        [Fact]
        public void KeyAttribute_does_not_override_configuration_from_explicit_source()
        {
            var entityBuilder = CreateInternalEntityTypeBuilder<A>();

            var propertyBuilder = entityBuilder.Property(typeof(int), "MyPrimaryKey", ConfigurationSource.Explicit);

            entityBuilder.PrimaryKey(new List<string> { "Id" }, ConfigurationSource.Explicit);

            new KeyAttributeConvention().Apply(propertyBuilder);

            Assert.Equal("Id", entityBuilder.Metadata.GetPrimaryKey().Properties[0].Name);
        }

        [Fact]
        public void KeyAttribute_sets_primary_key_for_single_property()
        {
            var modelBuilder = new ModelBuilder(new CoreConventionSetBuilder().CreateConventionSet());
            var entityTypeBuilder = modelBuilder.Entity<A>();

            Assert.Equal(1, entityTypeBuilder.Metadata.GetPrimaryKey().Properties.Count);
            Assert.Equal("MyPrimaryKey", entityTypeBuilder.Metadata.GetPrimaryKey().Properties[0].Name);
        }

        [Fact]
        public void KeyAttribute_throws_when_setting_composite_primary_key()
        {
            var modelBuilder = new ModelBuilder(new CoreConventionSetBuilder().CreateConventionSet());
            var entityTypeBuilder = modelBuilder.Entity<B>();

            Assert.Equal(2, entityTypeBuilder.Metadata.GetPrimaryKey().Properties.Count);
            Assert.Equal("Id", entityTypeBuilder.Metadata.GetPrimaryKey().Properties[0].Name);
            Assert.Equal("MyPrimaryKey", entityTypeBuilder.Metadata.GetPrimaryKey().Properties[1].Name);

            Assert.Equal(
                Strings.CompositePKWithDataAnnotation(entityTypeBuilder.Metadata.DisplayName()),
                Assert.Throws<InvalidOperationException>(() => modelBuilder.Validate()).Message);
        }

        [Fact]
        public void KeyAttribute_does_not_throw_when_setting_composite_primary_key_if_fluent_api_used()
        {
            var model = new MyContext().Model;

            Assert.Equal(2, model.GetEntityType(typeof(B)).GetPrimaryKey().Properties.Count);
            Assert.Equal("MyPrimaryKey", model.GetEntityType(typeof(B)).GetPrimaryKey().Properties[0].Name);
            Assert.Equal("Id", model.GetEntityType(typeof(B)).GetPrimaryKey().Properties[1].Name);
        }

        #endregion

        #region MaxLengthAttribute

        [Fact]
        public void MaxLengthAttribute_overrides_configuration_from_convention_source()
        {
            var entityBuilder = CreateInternalEntityTypeBuilder<A>();

            var propertyBuilder = entityBuilder.Property(typeof(string), "MaxLengthProperty", ConfigurationSource.Explicit);

            propertyBuilder.MaxLength(100, ConfigurationSource.Convention);

            new MaxLengthAttributeConvention().Apply(propertyBuilder);

            Assert.Equal(10, propertyBuilder.Metadata.GetMaxLength());
        }

        [Fact]
        public void MaxLengthAttribute_does_not_override_configuration_from_explicit_source()
        {
            var entityBuilder = CreateInternalEntityTypeBuilder<A>();

            var propertyBuilder = entityBuilder.Property(typeof(string), "MaxLengthProperty", ConfigurationSource.Explicit);

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

        #region RequiredAttribute

        [Fact]
        public void RequiredAttribute_overrides_configuration_from_convention_source()
        {
            var entityBuilder = CreateInternalEntityTypeBuilder<A>();

            var propertyBuilder = entityBuilder.Property(typeof(string), "Name", ConfigurationSource.Explicit);

            propertyBuilder.Required(false, ConfigurationSource.Convention);

            new RequiredAttributeConvention().Apply(propertyBuilder);

            Assert.False(propertyBuilder.Metadata.IsNullable);
        }

        [Fact]
        public void RequiredAttribute_does_not_override_configuration_from_explicit_source()
        {
            var entityBuilder = CreateInternalEntityTypeBuilder<A>();

            var propertyBuilder = entityBuilder.Property(typeof(string), "Name", ConfigurationSource.Explicit);

            propertyBuilder.Required(false, ConfigurationSource.Explicit);

            new RequiredAttributeConvention().Apply(propertyBuilder);

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
            var entityBuilder = CreateInternalEntityTypeBuilder<A>();

            var propertyBuilder = entityBuilder.Property(typeof(string), "StringLengthProperty", ConfigurationSource.Explicit);

            propertyBuilder.MaxLength(100, ConfigurationSource.Convention);

            new StringLengthAttributeConvention().Apply(propertyBuilder);

            Assert.Equal(20, propertyBuilder.Metadata.GetMaxLength());
        }

        [Fact]
        public void StringLengthAttribute_does_not_override_configuration_from_explicit_source()
        {
            var entityBuilder = CreateInternalEntityTypeBuilder<A>();

            var propertyBuilder = entityBuilder.Property(typeof(string), "StringLengthProperty", ConfigurationSource.Explicit);

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
            var entityBuilder = CreateInternalEntityTypeBuilder<A>();

            var propertyBuilder = entityBuilder.Property(typeof(byte[]), "Timestamp", ConfigurationSource.Explicit);

            propertyBuilder.StoreGeneratedPattern(StoreGeneratedPattern.None, ConfigurationSource.Convention);
            propertyBuilder.ConcurrencyToken(false, ConfigurationSource.Convention);

            new TimestampAttributeConvention().Apply(propertyBuilder);

            Assert.Equal(StoreGeneratedPattern.Computed, propertyBuilder.Metadata.StoreGeneratedPattern);
            Assert.True(propertyBuilder.Metadata.IsConcurrencyToken);
        }

        [Fact]
        public void TimestampAttribute_does_not_override_configuration_from_explicit_source()
        {
            var entityBuilder = CreateInternalEntityTypeBuilder<A>();

            var propertyBuilder = entityBuilder.Property(typeof(byte[]), "Timestamp", ConfigurationSource.Explicit);

            propertyBuilder.StoreGeneratedPattern(StoreGeneratedPattern.None, ConfigurationSource.Explicit);
            propertyBuilder.ConcurrencyToken(false, ConfigurationSource.Explicit);

            new TimestampAttributeConvention().Apply(propertyBuilder);

            Assert.Equal(StoreGeneratedPattern.None, propertyBuilder.Metadata.StoreGeneratedPattern);
            Assert.False(propertyBuilder.Metadata.IsConcurrencyToken);
        }

        [Fact]
        public void TimestampAttribute_sets_concurrency_token_with_conventional_builder()
        {
            var modelBuilder = new ModelBuilder(new CoreConventionSetBuilder().CreateConventionSet());
            var entityTypeBuilder = modelBuilder.Entity<A>();

            Assert.Equal(StoreGeneratedPattern.Computed, entityTypeBuilder.Property(e => e.Timestamp).Metadata.StoreGeneratedPattern);
            Assert.True(entityTypeBuilder.Property(e => e.Timestamp).Metadata.IsConcurrencyToken);
        }

        [Fact]
        public void TimestampAttribute_throws_if_used_on_non_binary_property()
        {
            var entityBuilder = CreateInternalEntityTypeBuilder<C>();

            var propertyBuilder = entityBuilder.Property(typeof(string), "Timestamp", ConfigurationSource.Explicit);

            Assert.Equal(Strings.TimestampAttributeOnNonBinary("Timestamp"),
                Assert.Throws<InvalidOperationException>(() => new TimestampAttributeConvention().Apply(propertyBuilder)).Message);
        }

        #endregion

        private InternalEntityTypeBuilder CreateInternalEntityTypeBuilder<T>()
        {
            var conventionSet = new ConventionSet();
            conventionSet.EntityTypeAddedConventions.Add(new PropertyDiscoveryConvention());

            var modelBuilder = new InternalModelBuilder(new Model(), conventionSet);

            return modelBuilder.Entity(typeof(T), ConfigurationSource.Explicit);
        }

        public class A
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
        }

        public class B
        {
            [Key]
            public int Id { get; set; }

            [Key]
            public int MyPrimaryKey { get; set; }
        }

        public class C
        {
            [Key]
            public int Id { get; set; }

            public string Data { get; set; }

            [Timestamp]
            public string Timestamp { get; set; }
        }

        public class MyContext : DbContext
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
