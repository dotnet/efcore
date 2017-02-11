// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    public class InternalDbFunctionBuilderTests
    {
        public class A
        {
            public static int MethodA()
            {
                throw new NotImplementedException();
            }

            public static int MethodB(string a, int b)
            {
                throw new NotImplementedException();
            }

            public int MethodC(string a)
            {
                throw new NotImplementedException();
            }

            public static int MethodD(params string[] a)
            {
                throw new NotImplementedException();
            }

            public static int MethodE(string a, int b)
            {
                throw new NotImplementedException();
            }

            [DbFunction()]
            public int MethodF(string a, int b)
            {
                throw new Exception();
            }

            [DbFunction()]
            public static int MethodG(string a, int b)
            {
                throw new Exception();
            }

            public static int MethodH<T>(T a, string b)
            {
                throw new Exception();
            }

            public static int MethodJ()
            {
                throw new Exception();
            }

            public static int MethodJ(string a)
            {
                throw new Exception();
            }
        }

        [Fact]
        public void Add_method_no_paramters_no_conventions()
        {
            var mb = CreateModelBuilder();

            var dbFuncBuilder = mb.DbFunction(typeof(InternalDbFunctionBuilderTests.A).GetTypeInfo().GetDeclaredMethod(nameof(InternalDbFunctionBuilderTests.A.MethodA)), false);

            Assert.NotNull(dbFuncBuilder);

            dbFuncBuilder.HasSchema("dbo");
            dbFuncBuilder.HasName("foo");

            Assert.Equal("foo", dbFuncBuilder.Metadata.Name);
            Assert.Equal("dbo", dbFuncBuilder.Metadata.Schema);
            Assert.Equal(0, dbFuncBuilder.Metadata.Parameters.Count);
        }

        [Fact]
        public void Add_method_with_paramters_no_conventions()
        {
            var mb = CreateModelBuilder();

            var dbFuncBuilder = mb.DbFunction(typeof(InternalDbFunctionBuilderTests.A).GetTypeInfo().GetDeclaredMethod(nameof(InternalDbFunctionBuilderTests.A.MethodB)), false);

            Assert.NotNull(dbFuncBuilder);

            dbFuncBuilder.HasSchema("dbo");
            dbFuncBuilder.HasName("fooB");

            Assert.Equal("fooB", dbFuncBuilder.Metadata.Name);
            Assert.Equal("dbo", dbFuncBuilder.Metadata.Schema);

            dbFuncBuilder.Parameter("a").HasParameterIndex(0);
            dbFuncBuilder.Parameter("b").HasParameterIndex(1);

            Assert.Equal("a", dbFuncBuilder.Metadata.FindParameter("a").Name);
            Assert.Equal("b", dbFuncBuilder.Metadata.FindParameter("b").Name);

            Assert.Equal(0, dbFuncBuilder.Metadata.FindParameter("a").ParameterIndex);
            Assert.Equal(1, dbFuncBuilder.Metadata.FindParameter("b").ParameterIndex);
        }

        [Fact]
        public void Add_method_with_shift_with_paramters_no_conventions()
        {
            var mb = CreateModelBuilder();

            var dbFuncBuilder = mb.DbFunction(typeof(InternalDbFunctionBuilderTests.A).GetTypeInfo().GetDeclaredMethod(nameof(InternalDbFunctionBuilderTests.A.MethodB)), false);

            Assert.NotNull(dbFuncBuilder);

            dbFuncBuilder.HasSchema("dbo");
            dbFuncBuilder.HasName("fooB");

            Assert.Equal("fooB", dbFuncBuilder.Metadata.Name);
            Assert.Equal("dbo", dbFuncBuilder.Metadata.Schema);

            dbFuncBuilder.Parameter("a").HasParameterIndex(0);
            dbFuncBuilder.Parameter("b").HasParameterIndex(1);

            Assert.Equal("a", dbFuncBuilder.Metadata.FindParameter("a").Name);
            Assert.Equal("b", dbFuncBuilder.Metadata.FindParameter("b").Name);

            Assert.Equal(0, dbFuncBuilder.Metadata.FindParameter("a").ParameterIndex);
            Assert.Equal(1, dbFuncBuilder.Metadata.FindParameter("b").ParameterIndex);

            dbFuncBuilder.Parameter("c").HasParameterIndex(0, true);

            Assert.Equal(0, dbFuncBuilder.Metadata.FindParameter("c").ParameterIndex);
            Assert.Equal(1, dbFuncBuilder.Metadata.FindParameter("a").ParameterIndex);
            Assert.Equal(2, dbFuncBuilder.Metadata.FindParameter("b").ParameterIndex);
        }

        [Fact]
        public void Add_method_with_paramter_object_identity_no_conventions()
        {
            var mb = CreateModelBuilder();

            var dbFuncBuilder = mb.DbFunction(typeof(InternalDbFunctionBuilderTests.A).GetTypeInfo().GetDeclaredMethod(nameof(InternalDbFunctionBuilderTests.A.MethodC)), false);

            Assert.NotNull(dbFuncBuilder);

            dbFuncBuilder.HasSchema("dbo");
            dbFuncBuilder.HasName("fooC");
            dbFuncBuilder.TranslateWith((a, b) => null);

            Assert.Equal("fooC", dbFuncBuilder.Metadata.Name);
            Assert.Equal("dbo", dbFuncBuilder.Metadata.Schema);

            var paramBuilder = dbFuncBuilder.Parameter("object");
            paramBuilder.HasParameterIndex(0);
            paramBuilder.IsObjectParameter(true);

            paramBuilder = dbFuncBuilder.Parameter("a");
            paramBuilder.HasParameterIndex(1);
            paramBuilder.IsIdentifier(true);

            var parameter = dbFuncBuilder.Metadata.FindParameter("object");

            Assert.NotNull(parameter);
            Assert.Equal(0, parameter.ParameterIndex);
            Assert.True(parameter.IsObjectParameter);

            parameter = dbFuncBuilder.Metadata.FindParameter("A");

            Assert.NotNull(parameter);
            Assert.Equal(1, parameter.ParameterIndex);
            Assert.True(parameter.IsIdentifier);
        }

        [Fact]
        public void Add_method_with_params_no_conventions()
        {
            var mb = CreateModelBuilder();

            var dbFuncBuilder = mb.DbFunction(typeof(InternalDbFunctionBuilderTests.A).GetTypeInfo().GetDeclaredMethod(nameof(InternalDbFunctionBuilderTests.A.MethodD)), false);

            Assert.NotNull(dbFuncBuilder);

            dbFuncBuilder.HasSchema("dbo");
            dbFuncBuilder.HasName("fooD");

            Assert.Equal("fooD", dbFuncBuilder.Metadata.Name);
            Assert.Equal("dbo", dbFuncBuilder.Metadata.Schema);

            var paramBuilder = dbFuncBuilder.Parameter("a");
            paramBuilder.IsParams(true);
            paramBuilder.HasParameterIndex(0);

            var parameter = dbFuncBuilder.Metadata.FindParameter("a");

            Assert.NotNull(parameter);
            Assert.Equal(0, parameter.ParameterIndex);
            Assert.True(parameter.IsParams);
        }

        [Fact]
        public void Add_method_with_value_no_conventions()
        {
            var mb = CreateModelBuilder();

            var dbFuncBuilder = mb.DbFunction(typeof(InternalDbFunctionBuilderTests.A).GetTypeInfo().GetDeclaredMethod(nameof(InternalDbFunctionBuilderTests.A.MethodE)), false);

            Assert.NotNull(dbFuncBuilder);

            dbFuncBuilder.HasSchema("dbo");
            dbFuncBuilder.HasName("fooC");

            Assert.Equal("fooC", dbFuncBuilder.Metadata.Name);
            Assert.Equal("dbo", dbFuncBuilder.Metadata.Schema);

            var paramBuilder = dbFuncBuilder.Parameter("a");
            paramBuilder.HasParameterIndex(1);
            paramBuilder.HasValue("abc");

            var parameter = dbFuncBuilder.Metadata.FindParameter("a");

            Assert.NotNull(parameter);
            Assert.Equal(1, parameter.ParameterIndex);
            Assert.Equal("abc", parameter.Value as string);
        }

        [Fact]
        public void Add_method_remove_parameter_no_shift_no_conventions()
        {
            var mb = CreateModelBuilder();
            var dbFuncBuilder = mb.DbFunction(typeof(InternalDbFunctionBuilderTests.A).GetTypeInfo().GetDeclaredMethod(nameof(InternalDbFunctionBuilderTests.A.MethodB)), false);
            Assert.NotNull(dbFuncBuilder);

            dbFuncBuilder.Parameter("a").HasParameterIndex(0);
            dbFuncBuilder.Parameter("b").HasParameterIndex(1);

            var parameter = dbFuncBuilder.Metadata.FindParameter("a");

            Assert.NotNull(parameter);
            dbFuncBuilder.RemoveParameter("a");
            Assert.Null(dbFuncBuilder.Metadata.FindParameter("a"));
            Assert.Equal(1, dbFuncBuilder.Metadata.FindParameter("b").ParameterIndex);
        }

        [Fact]
        public void Add_method_remove_parameter_with_shift_no_conventions()
        {
            var mb = CreateModelBuilder();
            var dbFuncBuilder = mb.DbFunction(typeof(InternalDbFunctionBuilderTests.A).GetTypeInfo().GetDeclaredMethod(nameof(InternalDbFunctionBuilderTests.A.MethodB)), false);
            Assert.NotNull(dbFuncBuilder);

            dbFuncBuilder.Parameter("a").HasParameterIndex(0);
            dbFuncBuilder.Parameter("b").HasParameterIndex(1);

            var parameter = dbFuncBuilder.Metadata.FindParameter("a");

            Assert.NotNull(parameter);
            dbFuncBuilder.RemoveParameter("a", true);
            Assert.Null(dbFuncBuilder.Metadata.FindParameter("a"));
            Assert.Equal(0, dbFuncBuilder.Metadata.FindParameter("b").ParameterIndex);
        }

        [Fact]
        public void Add_method_find_attribute_loaded_parameter_with_conventions()
        {
            var mb = CreateModelBuilder();
            var dbFuncBuilder = mb.DbFunction(typeof(InternalDbFunctionBuilderTests.A).GetTypeInfo().GetDeclaredMethod(nameof(InternalDbFunctionBuilderTests.A.MethodB)), true);
            Assert.NotNull(dbFuncBuilder);

            var parameter = dbFuncBuilder.Metadata.FindParameter("b");

            Assert.NotNull(parameter);
            Assert.Equal(1, parameter.ParameterIndex);
            Assert.Equal("b", parameter.Name);
        }

        [Fact]
        public void Add_method_callbacks_get_set()
        {
            var mb = CreateModelBuilder();
            var dbFuncBuilder = mb.DbFunction(typeof(InternalDbFunctionBuilderTests.A).GetTypeInfo().GetDeclaredMethod(nameof(InternalDbFunctionBuilderTests.A.MethodB)), true);
            Assert.NotNull(dbFuncBuilder);

            dbFuncBuilder.TranslateWith((arg, func) => Expression.Constant(true));
            dbFuncBuilder.BeforeInitialization((mce, func) => true);

            Assert.NotNull(dbFuncBuilder.Metadata.BeforeDbFunctionExpressionCreateCallback);
            Assert.NotNull(dbFuncBuilder.Metadata.TranslateCallback);
        }

        [Fact]
        public void Find_methods_on_type()
        {
            var mb = CreateModelBuilder();

            mb = mb.LoadDbFunctions(typeof(A));

            Assert.NotNull(mb);
            Assert.Equal(2, mb.Metadata.GetDbFunctions().Count());
        }

        [Fact]
        public void Add_method_generic_not_supported_throws()
        {
            var mb = CreateModelBuilder();
            Assert.NotNull(mb);

            var mi = typeof(InternalDbFunctionBuilderTests.A).GetTypeInfo().GetDeclaredMethod(nameof(InternalDbFunctionBuilderTests.A.MethodH));

            var expectedMessage = CoreStrings.DbFunctionGenericMethodNotSupported(mi);

            Assert.Equal(expectedMessage, Assert.Throws<ArgumentException>(() => mb.DbFunction(mi, true)).Message);
        }

        [Fact]
        public void adding_method_no_method_found_throws()
        {
            var mb = CreateModelBuilder();
            Assert.NotNull(mb);

            var expectedMessage = CoreStrings.DbFunctionMethodNotFound(typeof(A).Name, "foo");

            Assert.Equal(expectedMessage, Assert.Throws<ArgumentException>(() => mb.DbFunction(typeof(A), "foo")).Message);
        }

        [Fact]
        public void adding_method_too_many_overloads_throws()
        {
            var mb = CreateModelBuilder();
            Assert.NotNull(mb);

            var expectedMessage = CoreStrings.DbFunctionMethodTooManyOverloads(typeof(A).Name, nameof(A.MethodJ));

            Assert.Equal(expectedMessage, Assert.Throws<ArgumentException>(() => mb.DbFunction(typeof(A), nameof(A.MethodJ))).Message);
        }


        private InternalModelBuilder CreateModelBuilder()
        {
            var conventionset = new ConventionSet();
            conventionset.DbFunctionAddedConventions.Add(new DefaultDbFunctionConvention());

            return new InternalModelBuilder(new Model(conventionset));
        }
    }
}
