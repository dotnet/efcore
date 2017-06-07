// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Xunit;

// ReSharper disable ArgumentsStyleStringLiteral
// ReSharper disable InconsistentNaming

namespace Microsoft.EntityFrameworkCore
{
    public class DbFunctionsTest
    {
        private readonly DbFunctions _functions = new DbFunctions();

        [Fact]
        public void Like_when_null_inputs()
        {
            Assert.False(_functions.Like(null, "abc"));
            Assert.False(_functions.Like("abc", null));
            Assert.False(_functions.Like(null, null));
        }

        [Fact]
        public void Like_when_empty_inputs()
        {
            Assert.True(_functions.Like("", ""));
            Assert.False(_functions.Like("abc", ""));
            Assert.False(_functions.Like("", "ABC"));
        }

        [Fact]
        public void Like_when_no_wildcards()
        {
            Assert.True(_functions.Like("abc", "abc"));
            Assert.True(_functions.Like("abc", "ABC"));
            Assert.True(_functions.Like("ABC", "abc"));

            Assert.False(_functions.Like("ABC", "ab"));
            Assert.False(_functions.Like("ab", "abc"));
        }

        [Fact]
        public void Like_when_wildcards()
        {
            Assert.True(_functions.Like("abc", "%"));
            Assert.True(_functions.Like("abc", "%_"));
            Assert.True(_functions.Like("abc", "___"));
            Assert.True(_functions.Like("ABC", "a_c"));
            Assert.True(_functions.Like("ABC ", "a_c"));
            Assert.True(_functions.Like("ABC ", "%%%"));
            Assert.True(_functions.Like("a\\b", "a\\_"));

            Assert.False(_functions.Like("ABC", "__"));
            Assert.False(_functions.Like("ab", "___"));
            Assert.False(_functions.Like("a_", "a\\_"));
        }

        [Fact]
        public void Like_when_regex_chars()
        {
            Assert.True(_functions.Like("a.c", "a.c"));
            Assert.True(_functions.Like("a$c", "a$c"));
            Assert.True(_functions.Like("a^c", "a^c"));
            Assert.True(_functions.Like("a{c", "a{c"));
            Assert.True(_functions.Like("a}c", "a}c"));
            Assert.True(_functions.Like("a(c", "a(c"));
            Assert.True(_functions.Like("a)c", "a)c"));
            Assert.True(_functions.Like("a[c", "a[c"));
            Assert.True(_functions.Like("a]c", "a]c"));
            Assert.True(_functions.Like("a|c", "a|c"));
            Assert.True(_functions.Like("a*c", "a*c"));
            Assert.True(_functions.Like("a+c", "a+c"));
            Assert.True(_functions.Like("a?c", "a?c"));
            Assert.True(_functions.Like("a\\c", "a\\c"));

            Assert.False(_functions.Like("abc", "a.c"));
            Assert.False(_functions.Like("abc", "a$c"));
            Assert.False(_functions.Like("abc", "a^c"));
            Assert.False(_functions.Like("abc", "a{c"));
            Assert.False(_functions.Like("abc", "a}c"));
            Assert.False(_functions.Like("abc", "a(c"));
            Assert.False(_functions.Like("abc", "a)c"));
            Assert.False(_functions.Like("abc", "a[c"));
            Assert.False(_functions.Like("abc", "a]c"));
            Assert.False(_functions.Like("abc", "a|c"));
            Assert.False(_functions.Like("abc", "a*c"));
            Assert.False(_functions.Like("abc", "a+c"));
            Assert.False(_functions.Like("abc", "a?c"));
            Assert.False(_functions.Like("abc", "a\\c"));
        }

        [Fact]
        public void Like_when_escaping()
        {
            Assert.True(_functions.Like("50%", "%!%", "!"));
            Assert.True(_functions.Like("50%", "50!%", "!"));
            Assert.True(_functions.Like("50%", "__!%", "!"));
            Assert.True(_functions.Like("_%_%_%", "!_!%!_!%!_!%", "!"));

            Assert.False(_functions.Like("abc", "!%", "!"));
            Assert.False(_functions.Like("50%abc", "50!%", "!"));
        }

        [Fact]
        public void Like_when_escaping_with_regex_char()
        {
            Assert.True(_functions.Like("50%", "%|%", "|"));
            Assert.True(_functions.Like("50%", "50|%", "|"));
            Assert.True(_functions.Like("50%", "__|%", "|"));
            Assert.True(_functions.Like("_%_%_%", "|_|%|_|%|_|%", "|"));

            Assert.False(_functions.Like("abc", "|%", "|"));
            Assert.False(_functions.Like("50%abc", "50|%", "|"));
        }

        [Fact]
        public void Like_when_trailing_spaces()
        {
            Assert.True(_functions.Like("abc ", "abc "));
            Assert.True(_functions.Like("abc ", "abc"));
            Assert.True(_functions.Like("abc  ", "ABC "));

            Assert.False(_functions.Like("ABC", "ab "));
        }

        [Fact]
        public void Like_when_multiline()
        {
            Assert.True(_functions.Like("abc\r\ndef", "abc%"));
            Assert.True(_functions.Like("abc\r\ndef", "abc__def"));
            Assert.True(_functions.Like("abc\ndef", "abc_def"));
            Assert.True(_functions.Like("abc\rdef", "abc_def"));

            Assert.False(_functions.Like("abc\r\ndef", "ab%c"));
            Assert.False(_functions.Like("abc\r\ndef", "abc_def"));
            Assert.False(_functions.Like("abc\ndef", "abcdef"));
            Assert.False(_functions.Like("abc\rdef", "abcdef"));
        }
    }
}
