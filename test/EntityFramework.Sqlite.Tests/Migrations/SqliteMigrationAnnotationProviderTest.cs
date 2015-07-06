// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Conventions.Internal;
using Microsoft.Data.Entity.Sqlite.Metadata;
using Xunit;

namespace Microsoft.Data.Entity.Sqlite.Migrations
{
    public class SqliteMigrationAnnotationProviderTest
    {
        private readonly ModelBuilder _modelBuilder;
        private readonly SqliteMigrationAnnotationProvider _provider;

        private readonly Annotation _autoincrement = new Annotation(SqliteAnnotationNames.Prefix + SqliteAnnotationNames.Autoincrement, true);

        public SqliteMigrationAnnotationProviderTest()
        {
            _modelBuilder = new ModelBuilder(new CoreConventionSetBuilder().CreateConventionSet(), new Model());
            _provider = new SqliteMigrationAnnotationProvider();
        }

        [Theory]
        [InlineData(StoreGeneratedPattern.Computed, false)]
        [InlineData(StoreGeneratedPattern.Identity, true)]
        [InlineData(StoreGeneratedPattern.None, false)]
        public void Adds_Autoincrement_by_store_generated_pattern(StoreGeneratedPattern pattern, bool addsAnnotation)
        {
            _modelBuilder.Entity<Entity>(b => { b.Property(e => e.Prop).StoreGeneratedPattern(pattern); });
            var property = _modelBuilder.Model.GetEntityType(typeof(Entity)).GetProperty("Prop");

            var annotation = _provider.For(property);
            if (addsAnnotation)
            {
                Assert.Contains(annotation, a => a.Name == _autoincrement.Name && (bool)a.Value);
            }
            else
            {
                Assert.DoesNotContain(annotation, a => a.Name == _autoincrement.Name && (bool)a.Value);
            }
        }

        private class Entity
        {
            public int Id { get; set; }
            public long Prop { get; set; }
        }
    }
}
