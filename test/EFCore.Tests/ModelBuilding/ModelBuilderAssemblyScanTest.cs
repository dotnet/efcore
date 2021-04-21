// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Reflection;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore.ModelBuilding
{
    public class ModelBuilderAssemblyScanTest : ModelBuilderTest
    {
        private readonly Assembly _mockEntityTypeAssembly;

        public ModelBuilderAssemblyScanTest()
        {
            _mockEntityTypeAssembly = MockAssembly.Create(
                typeof(ScannerCustomerEntityConfiguration), typeof(ScannerCustomerEntityConfiguration2),
                typeof(AbstractCustomerEntityConfiguration), typeof(AbstractCustomerEntityConfigurationImpl));
        }

        [ConditionalFact]
        public void Should_scan_assemblies_for_entity_type_configurations()
        {
            var builder = CreateModelBuilder();
            builder.ApplyConfigurationsFromAssembly(_mockEntityTypeAssembly);

            var entityType = builder.Model.FindEntityType(typeof(ScannerCustomer));
            // ScannerCustomerEntityConfiguration called
            Assert.Equal(200, entityType.FindProperty(nameof(ScannerCustomer.FirstName)).GetMaxLength());
            // ScannerCustomerEntityConfiguration2 called
            Assert.Equal(1000, entityType.FindProperty(nameof(ScannerCustomer.LastName)).GetMaxLength());
            // AbstractCustomerEntityConfiguration not called
            Assert.Null(entityType.FindProperty(nameof(ScannerCustomer.MiddleName)).GetMaxLength());
            // AbstractCustomerEntityConfigurationImpl called
            Assert.Single(entityType.GetIndexes());
        }

        [ConditionalFact]
        public void Should_support_filtering_for_entity_type_configurations()
        {
            var builder = CreateModelBuilder();
            builder.ApplyConfigurationsFromAssembly(
                _mockEntityTypeAssembly, type => type.Name == nameof(ScannerCustomerEntityConfiguration));

            var entityType = builder.Model.FindEntityType(typeof(ScannerCustomer));
            // ScannerCustomerEntityConfiguration called
            Assert.Equal(200, entityType.FindProperty(nameof(ScannerCustomer.FirstName)).GetMaxLength());
            // ScannerCustomerEntityConfiguration2 not called
            Assert.Null(entityType.FindProperty(nameof(ScannerCustomer.LastName)).GetMaxLength());
            // AbstractCustomerEntityConfiguration not called
            Assert.Null(entityType.FindProperty(nameof(ScannerCustomer.MiddleName)).GetMaxLength());
            // AbstractCustomerEntityConfigurationImpl not called
            Assert.Empty(entityType.GetIndexes());
        }

        [ConditionalFact]
        public void Should_skip_abstract_classes_for_entity_type_configurations()
        {
            var builder = CreateModelBuilder();
            builder.ApplyConfigurationsFromAssembly(
                _mockEntityTypeAssembly, type => type.Name == nameof(AbstractCustomerEntityConfiguration));

            var entityType = builder.Model.FindEntityType(typeof(ScannerCustomer));
            // No configuration should occur
            Assert.Null(entityType);
        }

        protected virtual ModelBuilder CreateModelBuilder()
            => InMemoryTestHelpers.Instance.CreateConventionBuilder();

        protected class ScannerCustomer
        {
            public int Id { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string MiddleName { get; set; }
            public string Address { get; set; }
            public int IndexedField { get; set; }
        }

        protected class ScannerCustomer2
        {
            public int Id { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string MiddleName { get; set; }
            public string Address { get; set; }
            public int IndexedField { get; set; }
        }

        private class ScannerCustomerEntityConfiguration : IEntityTypeConfiguration<ScannerCustomer>
        {
            public void Configure(EntityTypeBuilder<ScannerCustomer> builder)
            {
                builder.Property(c => c.FirstName).HasMaxLength(200);
            }
        }

        private class ScannerCustomerEntityConfiguration2 : IEntityTypeConfiguration<ScannerCustomer>
        {
            public void Configure(EntityTypeBuilder<ScannerCustomer> builder)
            {
                builder.Property(c => c.LastName).HasMaxLength(1000);
            }
        }

        private abstract class AbstractCustomerEntityConfiguration : IEntityTypeConfiguration<ScannerCustomer>
        {
            public virtual void Configure(EntityTypeBuilder<ScannerCustomer> builder)
            {
                builder.Property(c => c.MiddleName).HasMaxLength(500);
            }
        }

        private class AbstractCustomerEntityConfigurationImpl : AbstractCustomerEntityConfiguration
        {
            public override void Configure(EntityTypeBuilder<ScannerCustomer> builder)
            {
                builder.HasIndex(c => c.IndexedField);
            }
        }
    }
}
