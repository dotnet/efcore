// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Xunit;
// ReSharper disable UnusedMember.Local

namespace Microsoft.EntityFrameworkCore.Metadata
{
    public class DbFunctionMetadataTests
    {
        public class MyBaseContext : DbContext
        {
            [DbFunction]
            public static void Foo()
            {
            }

            public static void Skip2()
            {
            }

            private static void Skip()
            {
            }
        }

        public class MyDerivedContext : MyBaseContext
        {
            [DbFunction]
            public static void Bar()
            {
            }

            public static void Skip3()
            {
            }

            private static void Skip4()
            {
            }

            [DbFunction]
            public void NonStatic()
            {
            }
        }

        public static MethodInfo MethodAmi = typeof(TestMethods).GetRuntimeMethod(nameof(TestMethods.MethodA), new[] { typeof(string), typeof(int) });
        public static MethodInfo MethodBmi = typeof(TestMethods).GetRuntimeMethod(nameof(TestMethods.MethodB), new[] { typeof(string), typeof(int) });
        public static MethodInfo MethodHmi = typeof(TestMethods).GetTypeInfo().GetDeclaredMethod(nameof(TestMethods.MethodH));

        public class TestMethods
        {
            public static int Foo => 1;

            public static int MethodA(string a, int b)
            {
                throw new NotImplementedException();
            }

            [DbFunction(Schema = "bar", FunctionName = "MethodFoo")]
            public static int MethodB(string c, int d)
            {
                throw new NotImplementedException();
            }

            public static void MethodC()
            {
            }

            public static TestMethods MethodD()
            {
                throw new NotImplementedException();
            }

            public static int MethodF(MyBaseContext context)
            {
                throw new NotImplementedException();
            }

            public static int MethodH<T>(T a, string b)
            {
                throw new Exception();
            }
        }

        [Fact]
        public virtual void Detects_non_static_function_on_dbcontext()
        {
            var modelBuilder = GetModelBuilder();

            var methodInfo
                = typeof(MyDerivedContext)
                    .GetRuntimeMethod(nameof(MyDerivedContext.NonStatic), new Type[] { });
            
            Assert.Equal(
                RelationalStrings.DbFunctionMethodMustBeStatic("MyDerivedContext.NonStatic"),
                Assert.Throws<ArgumentException>(() => modelBuilder.HasDbFunction(methodInfo)).Message);
        }

        [Fact]
        public void Detects_void_return_throws()
        {
            var modelBuilder = GetModelBuilder();

            var methodInfo = typeof(TestMethods).GetRuntimeMethod(nameof(TestMethods.MethodC), new Type[] { });

            Assert.Equal(
                RelationalStrings.DbFunctionInvalidReturnType(methodInfo.DisplayName(), typeof(void).ShortDisplayName()),
                Assert.Throws<ArgumentException>(() => modelBuilder.HasDbFunction(methodInfo)).Message);
        }

        [Fact]
        public void Adding_method_fluent_only_convention_defaults()
        {
            var modelBuilder = GetModelBuilder();

            var dbFuncBuilder = modelBuilder.HasDbFunction(MethodAmi);
            var dbFunc = dbFuncBuilder.Metadata;

            Assert.Equal("MethodA", dbFunc.FunctionName);
            Assert.Null(dbFunc.Schema);
            Assert.Equal(typeof(int), dbFunc.MethodInfo.ReturnType);
        }

        [Fact]
        public void Adding_method_fluent_only_convention_defaults_fluent_method_info()
        {
            var modelBuilder = GetModelBuilder();

            var dbFuncBuilder = modelBuilder.HasDbFunction(() => TestMethods.MethodA(null, default(int)));
            var dbFunc = dbFuncBuilder.Metadata;

            Assert.Equal("MethodA", dbFunc.FunctionName);
            Assert.Null(dbFunc.Schema);
            Assert.Equal(typeof(int), dbFunc.MethodInfo.ReturnType);
        }

        [Fact]
        public void Adding_method_fluent_only_convention_defaults_non_method_call_throws()
        {
            var modelBuilder = GetModelBuilder();

            Expression<Func<int>> expression = () => 1;

            Assert.Equal(
                RelationalStrings.DbFunctionExpressionIsNotMethodCall(expression),
                Assert.Throws<ArgumentException>(() => modelBuilder.HasDbFunction(expression)).Message);
        }

        [Fact]
        public void Adding_method_fluent_only_convention_defaults_property_call_throws()
        {
            var modelBuilder = GetModelBuilder();

            Expression<Func<int>> expression = () => TestMethods.Foo;

            Assert.Equal(
                RelationalStrings.DbFunctionExpressionIsNotMethodCall(expression),
                Assert.Throws<ArgumentException>(() => modelBuilder.HasDbFunction(expression)).Message);
        }

        [Fact]
        public void Adding_method_fluent_only_with_name_schema()
        {
            var modelBuilder = GetModelBuilder();

            var dbFuncBuilder = modelBuilder.HasDbFunction(MethodAmi)
                .HasName("foo")
                .HasSchema("bar");

            var dbFunc = dbFuncBuilder.Metadata;

            Assert.Equal("foo", dbFunc.FunctionName);
            Assert.Equal("bar", dbFunc.Schema);
            Assert.Equal(typeof(int), dbFunc.MethodInfo.ReturnType);
        }

        [Fact]
        public void Adding_method_fluent_only_with_builder()
        {
            var modelBuilder = GetModelBuilder();

            modelBuilder.HasDbFunction(MethodAmi, funcBuilder => { funcBuilder.HasName("foo").HasSchema("bar"); });

            var dbFunc = modelBuilder.HasDbFunction(MethodAmi).Metadata;

            Assert.Equal("foo", dbFunc.FunctionName);
            Assert.Equal("bar", dbFunc.Schema);
            Assert.Equal(typeof(int), dbFunc.MethodInfo.ReturnType);
        }

        [Fact]
        public void Adding_method_with_attribute_only()
        {
            var modelBuilder = GetModelBuilder();

            var dbFuncBuilder = modelBuilder.HasDbFunction(MethodBmi);
            var dbFunc = dbFuncBuilder.Metadata;

            Assert.Equal("MethodFoo", dbFunc.FunctionName);
            Assert.Equal("bar", dbFunc.Schema);
            Assert.Equal(typeof(int), dbFunc.MethodInfo.ReturnType);
        }

        [Fact]
        public void Adding_method_with_attribute_and_fluent_api_configuration_source()
        {
            var modelBuilder = GetModelBuilder();

            var dbFuncBuilder = modelBuilder.HasDbFunction(MethodBmi)
                .HasName("foo")
                .HasSchema("bar");

            var dbFunc = dbFuncBuilder.Metadata;

            Assert.Equal("foo", dbFunc.FunctionName);
            Assert.Equal("bar", dbFunc.Schema);
            Assert.Equal(typeof(int), dbFunc.MethodInfo.ReturnType);
        }

        [Fact]
        public void Adding_method_with_attribute_and_fluent_configuration_source()
        {
            var modelBuilder = GetModelBuilder();

            modelBuilder.HasDbFunction(MethodBmi, funcBuilder => { funcBuilder.HasName("foo").HasSchema("bar"); });

            var dbFunc = modelBuilder.HasDbFunction(MethodBmi).Metadata;

            Assert.Equal("foo", dbFunc.FunctionName);
            Assert.Equal("bar", dbFunc.Schema);
            Assert.Equal(typeof(int), dbFunc.MethodInfo.ReturnType);
        }

        [Fact]
        public void Adding_method_with_relational_schema()
        {
            var modelBuilder = GetModelBuilder();

            modelBuilder.HasDefaultSchema("dbo");

            var dbFuncBuilder = modelBuilder.HasDbFunction(MethodAmi);

            Assert.Equal("dbo", dbFuncBuilder.Metadata.Schema);
        }

        [Fact]
        public void Adding_method_with_relational_schema_fluent_overrides()
        {
            var modelBuilder = GetModelBuilder();

            modelBuilder.HasDefaultSchema("dbo");

            var dbFuncBuilder = modelBuilder.HasDbFunction(MethodAmi).HasSchema("bar");

            Assert.Equal("bar", dbFuncBuilder.Metadata.Schema);
        }

        [Fact]
        public void Adding_method_with_relational_schema_attribute_overrides()
        {
            var modelBuilder = GetModelBuilder();

            modelBuilder.HasDefaultSchema("dbo");

            var dbFuncBuilder = modelBuilder.HasDbFunction(MethodBmi);

            Assert.Equal("bar", dbFuncBuilder.Metadata.Schema);
        }

        [Fact]
        public void Add_method_generic_not_supported_throws()
        {
            var modelBuilder = GetModelBuilder();

            Assert.Equal(
                RelationalStrings.DbFunctionGenericMethodNotSupported(MethodHmi.DisplayName()),
                Assert.Throws<ArgumentException>(() => modelBuilder.HasDbFunction(MethodHmi)).Message);
        }

        [Fact]
        public virtual void Set_empty_function_name_throws()
        {
            var modelBuilder = GetModelBuilder();

            var expectedMessage = CoreStrings.ArgumentIsEmpty("name");

            Assert.Equal(expectedMessage, Assert.Throws<ArgumentException>(() => modelBuilder.HasDbFunction(MethodAmi).HasName("")).Message);
        }

        private ModelBuilder GetModelBuilder()
        {
            var conventionset = new ConventionSet();

            conventionset.ModelAnnotationChangedConventions.Add(new RelationalDbFunctionConvention());

            return new ModelBuilder(conventionset);
        }
    }
}
