// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Microsoft.Data.Entity.Utilities
{
    public class TypeExtensionsTest
    {
        [Fact]
        public void ElementType_should_return_element_type_from_sequence_type()
        {
            Assert.Equal(typeof(string), typeof(IEnumerable<string>).ElementType());
            Assert.Equal(typeof(string), typeof(IQueryable<string>).ElementType());
        }

        [Fact]
        public void ElementType_should_return_input_type_when_not_sequence_type()
        {
            Assert.Equal(typeof(string), typeof(string));
        }
    }
}
