// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Core.Utilities
{
    using Xunit;
    using Xunit.Sdk;

    public class DebugCheckFacts
    {
        [Fact]
        public void NotNull_throws_when_arg_is_null()
        {
            Assert.Throws<TraceAssertException>(() => DebugCheck.NotNull(null));
        }

        [Fact]
        public void NotEmpty_throws_when_empty()
        {
            Assert.Throws<TraceAssertException>(() => DebugCheck.NotEmpty(""));
        }

        [Fact]
        public void NotEmpty_throws_when_whitespace()
        {
            Assert.Throws<TraceAssertException>(() => DebugCheck.NotEmpty(" "));
        }
    }
}
