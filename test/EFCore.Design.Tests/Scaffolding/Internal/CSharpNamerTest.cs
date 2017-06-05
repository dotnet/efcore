// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Scaffolding.Internal;
using Xunit;

namespace Microsoft.EntityFrameworkCore
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
            Assert.Equal(output, new CSharpNamer<string>(s => s, new CSharpUtilities()).GetName(input));
        }
    }
}
