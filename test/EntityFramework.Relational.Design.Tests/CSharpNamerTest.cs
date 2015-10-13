// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Relational.Design.Model;
using Microsoft.Data.Entity.Relational.Design.ReverseEngineering;
using Xunit;

namespace Microsoft.Data.Entity.Relational.Design
{
    public class CSharpNamerTest
    {
        [Theory]
        [InlineData("Name with space", "Name_with_space")]
        [InlineData("namespace", "_namespace")]
        [InlineData("@namespace", "@namespace")]
        [InlineData("8ball", "_8ball")]
        public void Sanitizes_name(string input, string output)
        {
            Assert.Equal(output, new CSharpNamer<string>(s => s).GetName(input));
        }
    }
}
