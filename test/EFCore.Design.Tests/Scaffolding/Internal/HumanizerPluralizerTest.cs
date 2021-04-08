// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Design.Internal;
using Xunit;

namespace Microsoft.EntityFrameworkCore
{
    public class HumanizerPluralizerTest
    {
        [ConditionalTheory]
        [InlineData("Unicorn", "Unicorns")]
        [InlineData("Ox", "Oxen")]
        [InlineData(null, null)]
        public void Returns_expected_pluralized_name(string word, string inflected)
        {
            var pluralizer = new HumanizerPluralizer();
            Assert.Equal(inflected, pluralizer.Pluralize(word));
        }

        [ConditionalTheory]
        [InlineData("Unicorns", "Unicorn")]
        [InlineData("Oxen", "Ox")]
        [InlineData(null, null)]
        public void Returns_expected_singularized_name(string word, string inflected)
        {
            var pluralizer = new HumanizerPluralizer();
            Assert.Equal(inflected, pluralizer.Singularize(word));
        }
    }
}
