// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Microsoft.Data.Entity.Utilities
{
    public class TypeExtensionsTest
    {
        [Fact]
        public void ElementTypeShouldReturnElementTypeFromSequenceType()
        {
            Assert.Equal(typeof(string), typeof(IEnumerable<string>).ElementType());
            Assert.Equal(typeof(string), typeof(IQueryable<string>).ElementType());
        }

        [Fact]
        public void ElementTypeShouldReturnInputTypeWhenNotSequenceType()
        {
            Assert.Equal(typeof(string), typeof(string));
        }
    }
}
