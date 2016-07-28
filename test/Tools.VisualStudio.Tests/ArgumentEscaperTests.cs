// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Tools.VisualStudio;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Tools.VisualStudio.Tests
{
    public class ArgumentEscaperTests
    {
        [Theory]
        [InlineData(new[] { "one", "two", "three" }, "one two three")]
        [InlineData(new[] { "line1\nline2", "word1\tword2" }, "\"line1\nline2\" \"word1\tword2\"")]
        [InlineData(new[] { "with spaces" }, "\"with spaces\"")]
        [InlineData(new[] { @"with\backslash" }, @"with\backslash")]
        [InlineData(new[] { @"""quotedwith\backslash""" }, @"\""quotedwith\backslash\""")]
        [InlineData(new[] { @"C:\Users\" }, @"C:\Users\")]
        [InlineData(new[] { @"C:\Program Files\dotnet\" }, @"""C:\Program Files\dotnet\\""")]
        [InlineData(new[] { @"backslash\""preceedingquote" }, @"backslash\\\""preceedingquote")]
        public void EscapesArguments(string[] args, string expected)
          => Assert.Equal(expected, ArgumentEscaper.Escape(args));
    }
}
