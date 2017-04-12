// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    public class DbFunctionTests
    {
        public class A
        {
            public static int MethodA(string a, int b)
            {
                throw new NotImplementedException();
            }

            [DbFunction("dbo", "MethodFoo")]
            public int MethodB([DbFunctionParameter(IsIdentifier = true, ParameterIndex = 1, Value = "foobar")] string c,
                [DbFunctionParameter(ParameterIndex = 0)] int d)
            {
                throw new NotImplementedException();
            }

            public int MethodC(params string[] e)
            {
                throw new NotImplementedException();
            }

            public A MethodD()
            {
                throw new NotImplementedException();
            }

            public int MethodE()
            {
                throw new NotImplementedException();
            }

            [DbFunction("dbo", "MethodF")]
            public int MethodF()
            {
                throw new NotImplementedException();
            }
        }

        [Fact]
        public void adding_method_no_convention()
        {
            Model m = new Model();

            var dbFunc = m.AddDbFunction(typeof(DbFunctionTests.A).GetTypeInfo().GetDeclaredMethod(nameof(DbFunctionTests.A.MethodA)), false);

            Assert.True(dbFunc.Parameters.Count == 0);
            Assert.True(dbFunc.Name == "MethodA");
            Assert.True(dbFunc.Schema == String.Empty);
        }

        [Fact]
        public void adding_method_loads_parameters()
        {
            var m = GetModel();

            var dbFunc = m.AddDbFunction(typeof(DbFunctionTests.A).GetTypeInfo().GetDeclaredMethod(nameof(DbFunctionTests.A.MethodA)));

            Assert.True(dbFunc.Name == "MethodA");
            Assert.True(dbFunc.Schema == String.Empty);

            Assert.True(dbFunc.Parameters.Count == 2);

            var paramA = dbFunc.FindParameter("a");

            Assert.NotNull(paramA);
            Assert.True(paramA.ParameterType == typeof(string));
            Assert.True(paramA.ParameterIndex == 0);

            var paramB = dbFunc.FindParameter("b");

            Assert.NotNull(paramB);
            Assert.True(paramB.ParameterType == typeof(int));
            Assert.True(paramB.ParameterIndex == 1);
        }

        [Fact]
        public void adding_method_loads_attribute_data()
        {
            var m = GetModel();

            var dbFunc = m.AddDbFunction(typeof(DbFunctionTests.A).GetTypeInfo().GetDeclaredMethod(nameof(DbFunctionTests.A.MethodB)));

            Assert.True(dbFunc.Name == "MethodFoo");
            Assert.True(dbFunc.Schema == "dbo");

            Assert.True(dbFunc.Parameters.Count == 2);

            var paramC = dbFunc.FindParameter("c");

            Assert.NotNull(paramC);
            Assert.True(paramC.ParameterType == typeof(string));
            Assert.True(paramC.ParameterIndex == 1);
            Assert.True(paramC.IsIdentifier == true);

            var paramD = dbFunc.FindParameter("d");

            Assert.NotNull(paramD);
            Assert.True(paramD.ParameterType == typeof(int));
            Assert.True(paramD.ParameterIndex == 0);
        }

        [Fact]
        public void adding_method_loads_params_verifies_array_parameter_type()
        {
            var m = GetModel();

            var dbFunc = m.AddDbFunction(typeof(DbFunctionTests.A).GetTypeInfo().GetDeclaredMethod(nameof(DbFunctionTests.A.MethodC)));

            var paramE = dbFunc.FindParameter("e");

            Assert.NotNull(paramE);
            Assert.True(paramE.ParameterType == typeof(string[]));
            Assert.True(paramE.ParameterIndex == 0);
            Assert.True(paramE.IsParams == true);
        }

        [Fact]
        public void adding_method_invalid_parameter_type_throws()
        {
            Model m = new Model();

            var mi = typeof(DbFunctionTests.A).GetTypeInfo().GetDeclaredMethod(nameof(DbFunctionTests.A.MethodA));

            var dbFunc = m.AddDbFunction(mi, false);

            var parameter = dbFunc.AddParameter("paramA");

            Assert.NotNull(parameter);

            var expectedMessage = CoreStrings.DbFunctionInvalidParameterType(mi, "paramA", "A");

            Assert.Equal(expectedMessage, Assert.Throws<ArgumentException>(() => parameter.ParameterType = typeof(DbFunctionTests.A)).Message);
        }

        [Fact]
        public void adding_method_invalid_return_type_throws()
        {
            Model m = new Model();

            var mi = typeof(DbFunctionTests.A).GetTypeInfo().GetDeclaredMethod(nameof(DbFunctionTests.A.MethodD));

            var expectedMessage = CoreStrings.DbFunctionInvalidReturnType(mi, "A");

            Assert.Equal(expectedMessage, Assert.Throws<ArgumentException>(() => m.AddDbFunction(mi, false)).Message);
        }

        [Fact]
        public void adding_method_isObject_parameter_invalid_type_no_callback_throws()
        {
            Model m = new Model();

            var mi = typeof(DbFunctionTests.A).GetTypeInfo().GetDeclaredMethod(nameof(DbFunctionTests.A.MethodE));

            var dbFunc = m.AddDbFunction(mi, false);

            var expectedMessage = CoreStrings.DbFunctionInvalidTypeObjectParamMissingTranslate(mi, "object", "A");

            Assert.Equal(expectedMessage, Assert.Throws<ArgumentException>(() => dbFunc.AddParameter("object").IsObjectParameter = true).Message);
        }

        [Fact]
        public void adding_method_isObject_parameter_invalid_type_with_callback_does_not_throw()
        {
            Model m = new Model();

            var mi = typeof(DbFunctionTests.A).GetTypeInfo().GetDeclaredMethod(nameof(DbFunctionTests.A.MethodE));

            var dbFunc = m.AddDbFunction(mi, false);
            dbFunc.TranslateCallback = (a, b) => null;

            dbFunc.AddParameter("object").IsObjectParameter = true;
        }

        private Model GetModel()
        {
            var conventionset = new ConventionSet();
            conventionset.DbFunctionAddedConventions.Add(new DefaultDbFunctionConvention());

            return new Model(conventionset);
        }
    }
}