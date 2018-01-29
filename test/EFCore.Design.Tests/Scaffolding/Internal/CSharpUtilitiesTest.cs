// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Design;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Scaffolding.Internal
{
    public class CSharpUtilitiesTest
    {
        [Theory]
        [InlineData(typeof(int), "int")]
        [InlineData(typeof(int?), "int?")]
        [InlineData(typeof(int[]), "int[]")]
        [InlineData(typeof(Dictionary<string, List<int>>), "Dictionary<string, List<int>>")]
        [InlineData(typeof(List<int?>), "List<int?>")]
        [InlineData(typeof(SomeGenericStruct<int>?), "SomeGenericStruct<int>?")]
        public void GetTypeName(Type type, string typeName)
        {
            Assert.Equal(typeName, new CSharpUtilities().GetTypeName(type));
        }

        private struct SomeGenericStruct<T>
        {
        }

        [Theory]
        [InlineData("", "\"\"")]
        [InlineData("SomeValue", "\"SomeValue\"")]
        [InlineData("Contains\\Backslash\"QuoteAnd\tTab", "\"Contains\\\\Backslash\\\"QuoteAnd\\tTab\"")]
        [InlineData("Contains\r\nNewlinesAnd\"Quotes", "@\"Contains\r\nNewlinesAnd\"\"Quotes\"")]
        public void DelimitString(string input, string expectedOutput)
        {
            Assert.Equal(expectedOutput, new CSharpUtilities().DelimitString(input));
        }

        [Fact]
        public void Generate_MethodCallCodeFragment_works()
        {
            var method = new MethodCallCodeFragment("Test", true, 42);

            var result = new CSharpUtilities().Generate(method);

            Assert.Equal(".Test(true, 42)", result);
        }

        [Fact]
        public void Generate_MethodCallCodeFragment_works_when_niladic()
        {
            var method = new MethodCallCodeFragment("Test");

            var result = new CSharpUtilities().Generate(method);

            Assert.Equal(".Test()", result);
        }

        [Fact]
        public void Generate_MethodCallCodeFragment_works_when_chaining()
        {
            var method = new MethodCallCodeFragment("Test")
                .Chain("Test");

            var result = new CSharpUtilities().Generate(method);

            Assert.Equal(".Test().Test()", result);
        }

        [Fact]
        public void Generate_MethodCallCodeFragment_works_when_nested_closure()
        {
            var method = new MethodCallCodeFragment(
                "Test",
                new NestedClosureCodeFragment("x", new MethodCallCodeFragment("Test")));

            var result = new CSharpUtilities().Generate(method);

            Assert.Equal(".Test(x => x.Test())", result);
        }
    }
}
