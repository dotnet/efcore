// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Xunit;
using Microsoft.EntityFrameworkCore.Scaffolding.Internal;

namespace Microsoft.EntityFrameworkCore.Design.Tests.Scaffolding.Internal
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

        struct SomeGenericStruct<T> {}

        [Theory]
        [InlineData("", "\"\"")]
        [InlineData("SomeValue", "\"SomeValue\"")]
        [InlineData("Contains\\Backslash\"QuoteAnd\tTab", "\"Contains\\\\Backslash\\\"QuoteAnd\\tTab\"")]
        [InlineData("Contains\r\nNewlinesAnd\"Quotes", "@\"Contains\r\nNewlinesAnd\"\"Quotes\"")]
        public void DelimitString(string input, string expectedOutput)
        {
            Assert.Equal(expectedOutput, new CSharpUtilities().DelimitString(input));
        }
    }
}