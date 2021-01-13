// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal
{
    public class TableValuedDbFunctionConventionTest
    {
        [ConditionalFact]
        public void Configures_return_entity_as_not_mapped()
        {
            var modelBuilder = CreateModelBuilder();
            modelBuilder.HasDbFunction(
                typeof(TableValuedDbFunctionConventionTest).GetMethod(
                    nameof(GetKeylessEntities),
                    BindingFlags.NonPublic | BindingFlags.Static));

            modelBuilder.Entity<KeylessEntity>().HasNoKey();

            var model = modelBuilder.FinalizeModel();

            var entityType = model.FindEntityType(typeof(KeylessEntity));

            Assert.Null(entityType.FindPrimaryKey());
            Assert.Empty(entityType.GetViewOrTableMappings());
        }

        [ConditionalFact]
        public void Finds_existing_entity_type()
        {
            var modelBuilder = CreateModelBuilder();
            modelBuilder.Entity<TestEntity>().ToTable("TestTable").HasKey(e => e.Name);
            modelBuilder.HasDbFunction(
                typeof(TableValuedDbFunctionConventionTest).GetMethod(
                    nameof(GetEntities),
                    BindingFlags.NonPublic | BindingFlags.Static));

            var model = modelBuilder.FinalizeModel();

            var entityType = model.FindEntityType(typeof(TestEntity));

            Assert.Equal(nameof(TestEntity.Name), entityType.FindPrimaryKey().Properties.Single().Name);
            Assert.Equal("TestTable", entityType.GetViewOrTableMappings().Single().Table.Name);
        }

        [ConditionalFact]
        public void Throws_when_adding_a_function_returning_an_owned_type()
        {
            var modelBuilder = CreateModelBuilder();
            modelBuilder.Owned<KeylessEntity>();
            modelBuilder.HasDbFunction(
                typeof(TableValuedDbFunctionConventionTest).GetMethod(
                    nameof(GetKeylessEntities),
                    BindingFlags.NonPublic | BindingFlags.Static));

            Assert.Equal(
                RelationalStrings.DbFunctionInvalidIQueryableOwnedReturnType(
                    "Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal.TableValuedDbFunctionConventionTest.GetKeylessEntities(System.Int32)",
                    typeof(KeylessEntity).ShortDisplayName()),
                Assert.Throws<InvalidOperationException>(() => modelBuilder.FinalizeModel()).Message);
        }

        [ConditionalFact]
        public void Throws_when_adding_a_function_returning_an_existing_owned_type()
        {
            var modelBuilder = CreateModelBuilder();
            modelBuilder.Entity<TestEntity>().OwnsOne(e => e.KeylessEntity);
            modelBuilder.HasDbFunction(
                typeof(TableValuedDbFunctionConventionTest).GetMethod(
                    nameof(GetKeylessEntities),
                    BindingFlags.NonPublic | BindingFlags.Static));

            Assert.Equal(
                RelationalStrings.DbFunctionInvalidIQueryableOwnedReturnType(
                    "Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal.TableValuedDbFunctionConventionTest.GetKeylessEntities(System.Int32)",
                    typeof(KeylessEntity).ShortDisplayName()),
                Assert.Throws<InvalidOperationException>(() => modelBuilder.FinalizeModel()).Message);
        }

        [ConditionalFact]
        public void Throws_when_adding_a_function_returning_a_scalar()
        {
            var modelBuilder = CreateModelBuilder();
            modelBuilder.HasDbFunction(
                typeof(TableValuedDbFunctionConventionTest).GetMethod(
                    nameof(GetScalars),
                    BindingFlags.NonPublic | BindingFlags.Static));

            Assert.Equal(
                RelationalStrings.DbFunctionInvalidIQueryableReturnType(
                    "Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal.TableValuedDbFunctionConventionTest.GetScalars(System.Int32)",
                    typeof(IQueryable<int>).ShortDisplayName()),
                Assert.Throws<InvalidOperationException>(() => modelBuilder.FinalizeModel()).Message);
        }

        private static ModelBuilder CreateModelBuilder()
            => RelationalTestHelpers.Instance.CreateConventionBuilder();

        private static IQueryable<TestEntity> GetEntities(int id)
            => throw new NotImplementedException();

        private static IQueryable<KeylessEntity> GetKeylessEntities(int id)
            => throw new NotImplementedException();

        private static IQueryable<int> GetScalars(int id)
            => throw new NotImplementedException();

        private class TestEntity
        {
            public int Id { get; set; }

            public string Name { get; set; }

            [NotMapped]
            public KeylessEntity KeylessEntity { get; set; }
        }

        private class KeylessEntity
        {
            public string Name { get; set; }
        }
    }
}
