// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.SqlServer.Diagnostics.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore
{
    public class SqlServerDbFunctionMetadataTests
    {
        public static class TestMethods
        {
            public static int Foo()
            {
                throw new Exception();
            }
        }

        public static MethodInfo MethodFoo = typeof(TestMethods).GetRuntimeMethod(nameof(TestMethods.Foo), Array.Empty<Type>());

        [Fact]
        public virtual void DbFuction_defaults_schema_to_dbo_if_no_default_schema_or_set_schema()
        {
            var modelBuilder = GetModelBuilder();

            var dbFunction = modelBuilder.HasDbFunction(MethodFoo);

            modelBuilder.FinalizeModel();

            Assert.Equal("dbo", dbFunction.Metadata.Schema);
        }

        [Fact]
        public virtual void DbFuction_set_schmea_is_not_overridden_by_default_or_dbo()
        {
            var modelBuilder = GetModelBuilder();

            modelBuilder.HasDefaultSchema("qwerty");

            var dbFunction = modelBuilder.HasDbFunction(MethodFoo).HasSchema("abc");

            modelBuilder.FinalizeModel();

            Assert.Equal("abc", dbFunction.Metadata.Schema);
        }

        [Fact]
        public virtual void DbFuction_default_schema_not_overridden_by_dbo()
        {
            var modelBuilder = GetModelBuilder();

            modelBuilder.HasDefaultSchema("qwerty");

            var dbFunction = modelBuilder.HasDbFunction(MethodFoo);

            modelBuilder.FinalizeModel();

            Assert.Equal("qwerty", dbFunction.Metadata.Schema);
        }

        private ModelBuilder GetModelBuilder()
        {
            var conventionset = new ConventionSet();

            conventionset.ModelAnnotationChangedConventions.Add(
                new SqlServerDbFunctionConvention(new TestLogger<DbLoggerCategory.Model, SqlServerLoggingDefinitions>()));

            return new ModelBuilder(conventionset);
        }
    }
}
