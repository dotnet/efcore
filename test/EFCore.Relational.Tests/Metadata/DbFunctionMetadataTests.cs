// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
using Xunit;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    public class DbFunctionMetadataTests
    {
        public class MyBaseContext : DbContext
        {
            [DbFunction]
            public static void Foo() {}

            public static void Skip2() {}

            private static void Skip() {}
        }

        public class MyDerivedContext : MyBaseContext
        {
            [DbFunction]
            public static void Bar() {}

            public static void Skip3() {}

            private static void Skip4() {}

            [DbFunction]
            public void NonStatic() { }
        }

        public static MethodInfo MethodAmi = typeof(TestMethods).GetRuntimeMethod(nameof(TestMethods.MethodA), new[] { typeof(string), typeof(int) });
        public static MethodInfo MethodBmi = typeof(TestMethods).GetRuntimeMethod(nameof(TestMethods.MethodB), new[] { typeof(string), typeof(int) });
        public static MethodInfo MethodCmi = typeof(TestMethods).GetRuntimeMethod(nameof(TestMethods.MethodC), new Type[] { });
        public static MethodInfo MethodHmi = typeof(TestMethods).GetTypeInfo().GetDeclaredMethod(nameof(TestMethods.MethodH));

        public class TestMethods
        {
            public static int MethodA(string a, int b)
            {
                throw new NotImplementedException();
            }

            [DbFunction(Schema = "bar", Name = "MethodFoo")]
            public int MethodB([DbFunctionParameter(ParameterIndex = 1)] string c,
                [DbFunctionParameter(ParameterIndex = 0)] int d)
            {
                throw new NotImplementedException();
            }

            public void MethodC()
            {
            }

            public TestMethods MethodD()
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
        public void Adding_method_fluent_only_convention_defaults()
        {
            var modelBuilder = GetModelBuilder();

            var dbFuncBuilder = modelBuilder.HasDbFunction(MethodAmi);
            var dbFunc = dbFuncBuilder.Metadata;

            Assert.Equal("MethodA", dbFunc.Name);
            Assert.Null(dbFunc.Schema);
            Assert.Equal(typeof(int), dbFunc.ReturnType);

            Assert.Equal(2, dbFunc.Parameters.Count);

            Assert.Equal("a", dbFunc.Parameters[0].Name);
            Assert.Equal(0, dbFunc.Parameters[0].Index);
            Assert.Equal(typeof(string), dbFunc.Parameters[0].ParameterType);

            Assert.Equal("b", dbFunc.Parameters[1].Name);
            Assert.Equal(1, dbFunc.Parameters[1].Index);
            Assert.Equal(typeof(int), dbFunc.Parameters[1].ParameterType);
        }

        [Fact]
        public void Adding_method_fluent_only_with_name_schema()
        {
            var modelBuilder = GetModelBuilder();

            var dbFuncBuilder = modelBuilder.HasDbFunction(MethodAmi, "foo", "bar");
            var dbFunc = dbFuncBuilder.Metadata;

            Assert.Equal("foo", dbFunc.Name);
            Assert.Equal("bar", dbFunc.Schema);
            Assert.Equal(typeof(int), dbFunc.ReturnType);

            Assert.Equal(2, dbFunc.Parameters.Count);

            Assert.Equal("a", dbFunc.Parameters[0].Name);
            Assert.Equal(0, dbFunc.Parameters[0].Index);
            Assert.Equal(typeof(string), dbFunc.Parameters[0].ParameterType);

            Assert.Equal("b", dbFunc.Parameters[1].Name);
            Assert.Equal(1, dbFunc.Parameters[1].Index);
            Assert.Equal(typeof(int), dbFunc.Parameters[1].ParameterType);
        }

        [Fact]
        public void Adding_method_fluent_only_with_builder()
        {
            var modelBuilder = GetModelBuilder();

            modelBuilder.HasDbFunction(MethodAmi, funcBuilder =>
            {
                funcBuilder.HasName("foo").HasSchema("bar") ;
            });

            var dbFunc = modelBuilder.HasDbFunction(MethodAmi).Metadata;

            Assert.Equal("foo", dbFunc.Name);
            Assert.Equal("bar", dbFunc.Schema);
            Assert.Equal(typeof(int), dbFunc.ReturnType);

            Assert.Equal(2, dbFunc.Parameters.Count);

            Assert.Equal("a", dbFunc.Parameters[0].Name);
            Assert.Equal(0, dbFunc.Parameters[0].Index);
            Assert.Equal(typeof(string), dbFunc.Parameters[0].ParameterType);

            Assert.Equal("b", dbFunc.Parameters[1].Name);
            Assert.Equal(1, dbFunc.Parameters[1].Index);
            Assert.Equal(typeof(int), dbFunc.Parameters[1].ParameterType);
        }

        [Fact]
        public void Adding_method_with_attribute_only()
        {
            var modelBuilder = GetModelBuilder();

            var dbFuncBuilder = modelBuilder.HasDbFunction(MethodBmi);
            var dbFunc = dbFuncBuilder.Metadata;

            Assert.Equal("MethodFoo", dbFunc.Name);
            Assert.Equal("bar", dbFunc.Schema);
            Assert.Equal(typeof(int), dbFunc.ReturnType);

            Assert.Equal(2, dbFunc.Parameters.Count);

            Assert.Equal("c", dbFunc.Parameters[0].Name);
            Assert.Equal(1, dbFunc.Parameters[0].Index);
            Assert.Equal(typeof(string), dbFunc.Parameters[0].ParameterType);

            Assert.Equal("d", dbFunc.Parameters[1].Name);
            Assert.Equal(0, dbFunc.Parameters[1].Index);
            Assert.Equal(typeof(int), dbFunc.Parameters[1].ParameterType);
        }

        [Fact]
        public void Adding_method_with_attribute_and_fluent_HasDbFunction_configurationSource()
        {
            var modelBuilder = GetModelBuilder();

            var dbFuncBuilder = modelBuilder.HasDbFunction(MethodBmi, "foo", "bar");
            var dbFunc = dbFuncBuilder.Metadata;

            Assert.Equal("foo", dbFunc.Name);
            Assert.Equal("bar", dbFunc.Schema);
            Assert.Equal(typeof(int), dbFunc.ReturnType);

            Assert.Equal(2, dbFunc.Parameters.Count);

            Assert.Equal("c", dbFunc.Parameters[0].Name);
            Assert.Equal(1, dbFunc.Parameters[0].Index);
            Assert.Equal(typeof(string), dbFunc.Parameters[0].ParameterType);

            Assert.Equal("d", dbFunc.Parameters[1].Name);
            Assert.Equal(0, dbFunc.Parameters[1].Index);
            Assert.Equal(typeof(int), dbFunc.Parameters[1].ParameterType);
        }

        [Fact]
        public void Adding_method_with_attribute_and_fluent_configurationSource()
        {
            var modelBuilder = GetModelBuilder();

            modelBuilder.HasDbFunction(MethodBmi, funcBuilder =>
            {
                funcBuilder.HasName("foo").HasSchema("bar");
                funcBuilder.HasParameter("c").HasIndex(0);
                funcBuilder.HasParameter("d").HasIndex(1);
            });

            var dbFunc = modelBuilder.HasDbFunction(MethodBmi).Metadata;
            
            Assert.Equal("foo", dbFunc.Name);
            Assert.Equal("bar", dbFunc.Schema);
            Assert.Equal(typeof(int), dbFunc.ReturnType);

            Assert.Equal(2, dbFunc.Parameters.Count);

            Assert.Equal("c", dbFunc.Parameters[0].Name);
            Assert.Equal(0, dbFunc.Parameters[0].Index);
            Assert.Equal(typeof(string), dbFunc.Parameters[0].ParameterType);

            Assert.Equal("d", dbFunc.Parameters[1].Name);
            Assert.Equal(1, dbFunc.Parameters[1].Index);
            Assert.Equal(typeof(int), dbFunc.Parameters[1].ParameterType);
        }

        [Fact]
        public void Adding_method_with_parameter_fluent_overrides()
        {
            var modelBuilder = GetModelBuilder();

            var dbFuncBuilder = modelBuilder.HasDbFunction(MethodAmi);
            var parameter = dbFuncBuilder.HasParameter("a").Metadata;

            Assert.Equal(0, parameter.Index);
            Assert.Equal("a", parameter.Name);
            Assert.Equal(typeof(string), parameter.ParameterType);

            parameter.Index = 5;
            parameter.Name = "abc";
            parameter.ParameterType = typeof(int);

            Assert.Equal(5, parameter.Index);
            Assert.Equal("abc", parameter.Name);
            Assert.Equal(typeof(int), parameter.ParameterType);
        }

        [Fact]
        public void DbFunctionReturnType()
        {
            var modelBuilder = GetModelBuilder();

            var dbFuncBuilder = modelBuilder.HasDbFunction(MethodAmi);

            Assert.Equal(typeof(int), dbFuncBuilder.Metadata.ReturnType);

            dbFuncBuilder.Metadata.ReturnType = typeof(string);

            Assert.Equal(typeof(string), dbFuncBuilder.Metadata.ReturnType);

        }

        [Fact]
        public void Adding_method_with_relational_scema()
        {
            var modelBuilder = GetModelBuilder();

            modelBuilder.HasDefaultSchema("dbo");

            var dbFuncBuilder = modelBuilder.HasDbFunction(MethodAmi);

            Assert.Equal("dbo", dbFuncBuilder.Metadata.Schema);
        }

        [Fact]
        public void Adding_method_with_relational_scema_fluent_overrides()
        {
            var modelBuilder = GetModelBuilder();

            modelBuilder.HasDefaultSchema("dbo");

            var dbFuncBuilder = modelBuilder.HasDbFunction(MethodAmi, schema:"bar");

            Assert.Equal("bar", dbFuncBuilder.Metadata.Schema);
        }

        [Fact]
        public void Adding_method_with_relational_scema_attribute_overrides()
        {
            var modelBuilder = GetModelBuilder();

            modelBuilder.HasDefaultSchema("dbo");

            var dbFuncBuilder = modelBuilder.HasDbFunction(MethodBmi);

            Assert.Equal("bar", dbFuncBuilder.Metadata.Schema);
        }

        [Fact]
        public void Adding_method_with_void_return_does_not_throw()
        {
            var modelBuilder = GetModelBuilder();

            var dbFuncBuilder = modelBuilder.HasDbFunction(MethodCmi);

            Assert.Equal(typeof(void), dbFuncBuilder.Metadata.ReturnType);
        }

        [Fact]
        public void Add_method_generic_not_supported_throws()
        {
            var modelBuilder = GetModelBuilder();

            var expectedMessage = CoreStrings.DbFunctionGenericMethodNotSupported(MethodHmi);

            Assert.Equal(expectedMessage, Assert.Throws<ArgumentException>(() => modelBuilder.HasDbFunction(MethodHmi)).Message);
        }


        [Fact]
        public virtual void Set_empty_function_name_throws()
        {
            var modelBuilder = GetModelBuilder();

            var expectedMessage = CoreStrings.ArgumentIsEmpty("name");

            Assert.Equal(expectedMessage, Assert.Throws<ArgumentException>(() => modelBuilder.HasDbFunction(MethodAmi, name: "")).Message);
        }

        [Fact]
        public virtual void Set_empty_function_schema_throws()
        {
            var modelBuilder = GetModelBuilder();

            var expectedMessage = CoreStrings.ArgumentIsEmpty("schema");

            Assert.Equal(expectedMessage, Assert.Throws<ArgumentException>(() => modelBuilder.HasDbFunction(MethodAmi, schema: "")).Message);
        }

        private ModelBuilder GetModelBuilder()
        {
            var conventionset = new ConventionSet();

            conventionset.ModelAnnotationChangedConventions.Add(new RelationalDbFunctionConvention());

            return new ModelBuilder(conventionset);
        }
    }
}
