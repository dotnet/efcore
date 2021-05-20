// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.SqlServer.Metadata.Internal;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore.SqlServer.Migrations.Internal
{
    public class SqlServerMigrationsAnnotationProviderTest
    {
        private readonly SqlServerAnnotationProvider _annotations;

        public SqlServerMigrationsAnnotationProviderTest()
        {
            _annotations = new SqlServerAnnotationProvider(new RelationalAnnotationProviderDependencies());
        }

        [Fact]
        public void For_property_handles_identity_annotations()
        {
            var modelBuilder = SqlServerTestHelpers.Instance.CreateConventionBuilder();
            modelBuilder.Entity<Entity>().Property<int>("Id").UseIdentityColumn(2, 3);

            var model = SqlServerTestHelpers.Instance.Finalize(modelBuilder, designTime: true);
            var property = model.FindEntityType(typeof(Entity)).FindProperty("Id");

            var migrationAnnotations = _annotations.For(property.GetTableColumnMappings().Single().Column, true).ToList();

            var identity = Assert.Single(migrationAnnotations, a => a.Name == SqlServerAnnotationNames.Identity);
            Assert.Equal("2, 3", identity.Value);
        }

        [ConditionalFact]
        public void Resolves_column_names_for_Index_with_included_properties()
        {
            var modelBuilder = SqlServerTestHelpers.Instance.CreateConventionBuilder();
            modelBuilder.Entity<Entity>().Property(e => e.IncludedProp).HasColumnName("IncludedColumn");
            modelBuilder.Entity<Entity>().HasIndex(e => e.IndexedProp).IncludeProperties(e => e.IncludedProp);
            var model = SqlServerTestHelpers.Instance.Finalize(modelBuilder, designTime: true);

            Assert.Contains(
                _annotations.For(model.FindEntityType(typeof(Entity)).GetIndexes().Single().GetMappedTableIndexes().Single(), true),
                a => a.Name == SqlServerAnnotationNames.Include && ((string[])a.Value).Contains("IncludedColumn"));
        }

        private class Entity
        {
            public int Id { get; set; }
            public string IndexedProp { get; set; }
            public string IncludedProp { get; set; }
        }
    }
}
