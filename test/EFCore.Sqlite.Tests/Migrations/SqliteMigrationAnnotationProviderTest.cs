﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Migrations.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Migrations
{
    public class SqliteMigrationAnnotationProviderTest
    {
        private readonly ModelBuilder _modelBuilder;
        private readonly SqliteMigrationsAnnotationProvider _provider;

        private readonly Annotation _autoincrement = new Annotation(SqliteAnnotationNames.Autoincrement, true);

        public SqliteMigrationAnnotationProviderTest()
        {
            _modelBuilder = SqliteTestHelpers.Instance.CreateConventionBuilder();

            _provider = new SqliteMigrationsAnnotationProvider(new MigrationsAnnotationProviderDependencies());
        }

        [Fact]
        public void Adds_Autoincrement_for_OnAdd_integer_property()
        {
            var property = _modelBuilder.Entity<Entity>().Property(e => e.IntProp).ValueGeneratedOnAdd().Metadata;

            Assert.Contains(_provider.For(property), a => a.Name == _autoincrement.Name && (bool)a.Value);
        }

        [Fact]
        public void Does_not_add_Autoincrement_for_OnAddOrUpdate_integer_property()
        {
            var property = _modelBuilder.Entity<Entity>().Property(e => e.IntProp).ValueGeneratedOnAddOrUpdate().Metadata;

            Assert.DoesNotContain(_provider.For(property), a => a.Name == _autoincrement.Name);
        }

        [Fact]
        public void Does_not_add_Autoincrement_for_OnUpdate_integer_property()
        {
            var property = _modelBuilder.Entity<Entity>().Property(e => e.IntProp).ValueGeneratedOnUpdate().Metadata;

            Assert.DoesNotContain(_provider.For(property), a => a.Name == _autoincrement.Name);
        }

        [Fact]
        public void Does_not_add_Autoincrement_for_Never_value_generated_integer_property()
        {
            var property = _modelBuilder.Entity<Entity>().Property(e => e.IntProp).ValueGeneratedNever().Metadata;

            Assert.DoesNotContain(_provider.For(property), a => a.Name == _autoincrement.Name);
        }

        [Fact]
        public void Does_not_add_Autoincrement_for_default_integer_property()
        {
            var property = _modelBuilder.Entity<Entity>().Property(e => e.IntProp).Metadata;

            Assert.DoesNotContain(_provider.For(property), a => a.Name == _autoincrement.Name);
        }

        [Fact]
        public void Does_not_add_Autoincrement_for_non_integer_OnAdd_property()
        {
            var property = _modelBuilder.Entity<Entity>().Property(e => e.StringProp).ValueGeneratedOnAdd().Metadata;

            Assert.DoesNotContain(_provider.For(property), a => a.Name == _autoincrement.Name);
        }

        private class Entity
        {
            public int Id { get; set; }
            public long IntProp { get; set; }
            public string StringProp { get; set; }
        }
    }
}
