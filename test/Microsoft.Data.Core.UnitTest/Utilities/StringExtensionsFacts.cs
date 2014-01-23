// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Core.Utilities
{
    using Xunit;

    public class StringExtensionsFacts
    {
        [Fact]
        public void EqualsOrdinal_should_consider_case()
        {
            Assert.True("Ab".EqualsOrdinal("Ab"));
            Assert.False("Ab".EqualsOrdinal("ab"));
        }

        [Fact]
        public void EqualsIgnoreCase_should_not_consider_case()
        {
            Assert.True("Ab".EqualsIgnoreCase("Ab"));
            Assert.True("Ab".EqualsIgnoreCase("ab"));
        }

        [Fact]
        public void Format_should_embed_parameters()
        {
            Assert.Equal("A1b", "A{0}b".Format(1));
        }
    }
}
