// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Design.Internal;
using Xunit;

namespace Microsoft.EntityFrameworkCore
{
    public class NullPluralizerTest
    {
        [Fact]
        public void Returns_same_name()
        {
            var pluralizer = new NullPluralizer();
            var name = "blogs";
            Assert.Equal(name, pluralizer.Pluralize(name));
            Assert.Equal(name, pluralizer.Singularize(name));
        }

        [Fact]
        public void Returns_same_name_when_null()
        {
            var pluralizer = new NullPluralizer();
            Assert.Null(pluralizer.Pluralize(null));
            Assert.Null(pluralizer.Singularize(null));
        }
    }
}
