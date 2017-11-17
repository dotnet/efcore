// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Xunit;

namespace Microsoft.EntityFrameworkCore.Tools
{
    public class JsonTest
    {
        [Fact]
        public void Literal_escapes()
        {
            Assert.Equal("\"test\\\\test\\\"test\"", Json.Literal("test\\test\"test"));
        }

        [Fact]
        public void Literal_handles_null()
        {
            Assert.Equal("null", Json.Literal(null));
        }
    }
}
