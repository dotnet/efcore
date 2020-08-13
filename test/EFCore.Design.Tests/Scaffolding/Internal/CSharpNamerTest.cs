// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Scaffolding.Internal;
using Xunit;

namespace Microsoft.EntityFrameworkCore
{
    public class CSharpNamerTest
    {
        [ConditionalTheory]
        [InlineData("Name with space", "Name_with_space")]
        [InlineData("namespace", "_namespace")]
        [InlineData("@namespace", "@namespace")]
        [InlineData("8ball", "_8ball")]
        public void Sanitizes_name_with_no_singularize_or_pluralize(string input, string output)
        {
            Assert.Equal(output, new CSharpNamer<string>(s => s, new CSharpUtilities(), null).GetName(input));
        }

        [ConditionalTheory]
        [InlineData("Name ending with s", "Name_ending_with_")]
        [InlineData("Name with no s at end", "Name_with_no_s_at_end")]
        public void Sanitizes_name_with_singularizer(string input, string output)
        {
            var fakePluralizer = new RelationalDatabaseModelFactoryTest.FakePluralizer();
            Assert.Equal(output, new CSharpNamer<string>(s => s, new CSharpUtilities(), fakePluralizer.Singularize).GetName(input));
        }

        [ConditionalTheory]
        [InlineData("Name ending with s", "Name_ending_with_s")]
        [InlineData("Name with no s at end", "Name_with_no_s_at_ends")]
        public void Sanitizes_name_with_pluralizer(string input, string output)
        {
            var fakePluralizer = new RelationalDatabaseModelFactoryTest.FakePluralizer();
            Assert.Equal(output, new CSharpNamer<string>(s => s, new CSharpUtilities(), fakePluralizer.Pluralize).GetName(input));
        }
    }
}
