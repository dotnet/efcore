// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Reflection;
using Xunit;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    public class InternalIndexBuilderTest
    {
        [ConditionalFact]
        public void Can_only_override_lower_source_IsUnique()
        {
            var builder = CreateInternalIndexBuilder();
            var metadata = builder.Metadata;

            Assert.NotNull(builder.IsUnique(true, ConfigurationSource.Convention));
            Assert.NotNull(builder.IsUnique(false, ConfigurationSource.DataAnnotation));

            Assert.False(metadata.IsUnique);

            Assert.Null(builder.IsUnique(true, ConfigurationSource.Convention));

            Assert.False(metadata.IsUnique);
        }

        [ConditionalFact]
        public void Can_only_override_existing_IsUnique_value_explicitly()
        {
            var builder = CreateInternalIndexBuilder();
            var metadata = builder.Metadata;
            metadata.IsUnique = true;

            Assert.Equal(ConfigurationSource.Explicit, metadata.GetConfigurationSource());
            Assert.NotNull(builder.IsUnique(true, ConfigurationSource.DataAnnotation));
            Assert.Null(builder.IsUnique(false, ConfigurationSource.DataAnnotation));

            Assert.True(metadata.IsUnique);

            Assert.NotNull(builder.IsUnique(false, ConfigurationSource.Explicit));

            Assert.False(metadata.IsUnique);
        }

        [ConditionalFact]
        public void Can_only_override_lower_source_Name()
        {
            var builder = CreateInternalIndexBuilder();
            var metadata = builder.Metadata;

            Assert.NotNull(builder.HasName("ConventionIndexName", ConfigurationSource.Convention));
            Assert.NotNull(builder.HasName("AnnotationIndexName", ConfigurationSource.DataAnnotation));

            Assert.Equal("AnnotationIndexName", metadata.Name);

            Assert.Null(builder.HasName("UpdatedConventionIndexName", ConfigurationSource.Convention));

            Assert.Equal("AnnotationIndexName", metadata.Name);
        }

        [ConditionalFact]
        public void Can_only_override_existing_Name_value_explicitly()
        {
            var builder = CreateInternalIndexBuilder();
            var metadata = builder.Metadata;
            metadata.Name = "TestIndexName";

            Assert.Equal(ConfigurationSource.Explicit, metadata.GetConfigurationSource());
            Assert.NotNull(builder.HasName("TestIndexName", ConfigurationSource.DataAnnotation));
            Assert.Null(builder.HasName("NewIndexName", ConfigurationSource.DataAnnotation));

            Assert.Equal("TestIndexName", metadata.Name);

            Assert.NotNull(builder.HasName("ExplicitIndexName", ConfigurationSource.Explicit));

            Assert.Equal("ExplicitIndexName", metadata.Name);
        }

        private InternalIndexBuilder CreateInternalIndexBuilder()
        {
            var modelBuilder = new InternalModelBuilder(new Model());
            var entityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);

            return entityBuilder.HasIndex(new[] { Customer.IdProperty, Customer.NameProperty }, ConfigurationSource.Explicit);
        }

        private class Customer
        {
            public static readonly PropertyInfo IdProperty = typeof(Customer).GetProperty("Id");
            public static readonly PropertyInfo NameProperty = typeof(Customer).GetProperty("Name");

            public int Id { get; set; }
            public string Name { get; set; }
        }
    }
}
