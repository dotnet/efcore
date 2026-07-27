// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Design.Internal;

namespace Microsoft.EntityFrameworkCore.Scaffolding.Internal;

public class HumanizerPluralizerTest
{
    [Theory, InlineData("Unicorn", "Unicorns"), InlineData("Ox", "Oxen"), InlineData(null, null)]
    public void Returns_expected_pluralized_name(string word, string inflected)
    {
        var pluralizer = new HumanizerPluralizer();
        Assert.Equal(inflected, pluralizer.Pluralize(word));
    }

    [Theory, InlineData("Unicorns", "Unicorn"), InlineData("Oxen", "Ox"), InlineData(null, null)]
    public void Returns_expected_singularized_name(string word, string inflected)
    {
        var pluralizer = new HumanizerPluralizer();
        Assert.Equal(inflected, pluralizer.Singularize(word));
    }
}
