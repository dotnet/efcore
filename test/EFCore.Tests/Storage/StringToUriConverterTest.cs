// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Storage
{
    public class StringToUriConverterTest
    {
        private static readonly StringToUriConverter _stringToUri
            = new StringToUriConverter();

        [ConditionalFact]
        public void Can_convert_strings_to_uris()
        {
            var converter = _stringToUri.ConvertToProviderExpression.Compile();

            Assert.Equal(new Uri("https://www.github.com"), converter("https://www.github.com"));
            Assert.Equal(new Uri("/relative/path", UriKind.Relative), converter("/relative/path"));
            Assert.Equal(new Uri("ftp://www.github.com", UriKind.Absolute), converter("ftp://www.github.com/"));
            Assert.Equal(new Uri(".", UriKind.Relative), converter("."));
            Assert.Null(converter("http:///"));
            Assert.Null(converter(null));
        }

        [ConditionalFact]
        public void Can_convert_strings_to_uris_object()
        {
            var converter = _stringToUri.ConvertToProvider;

            Assert.Equal(new Uri("https://www.github.com"), converter("https://www.github.com"));
            Assert.Equal(new Uri("/relative/path", UriKind.Relative), converter("/relative/path"));
            Assert.Equal(new Uri("ftp://www.github.com", UriKind.Absolute), converter("ftp://www.github.com/"));
            Assert.Equal(new Uri(".", UriKind.Relative), converter("."));
            Assert.Null(converter("http:///"));
            Assert.Null(converter(null));
        }

        [ConditionalFact]
        public void Can_convert_uris_to_strings()
        {
            var converter = _stringToUri.ConvertFromProviderExpression.Compile();

            Assert.Equal("https://www.github.com/", converter(new Uri("https://www.github.com")));
            Assert.Equal("/relative/path", converter(new Uri("/relative/path", UriKind.Relative)));
            Assert.Equal("ftp://www.github.com/", converter(new Uri("ftp://www.github.com/", UriKind.Absolute)));
            Assert.Equal(".", converter(new Uri(".", UriKind.Relative)));
            Assert.Null(converter(null));
        }

        [ConditionalFact]
        public void Can_convert_uris_to_strings_object()
        {
            var converter = _stringToUri.ConvertFromProvider;

            Assert.Equal("https://www.github.com/", converter(new Uri("https://www.github.com")));
            Assert.Equal("/relative/path", converter(new Uri("/relative/path", UriKind.Relative)));
            Assert.Equal("ftp://www.github.com/", converter(new Uri("ftp://www.github.com/", UriKind.Absolute)));
            Assert.Equal(".", converter(new Uri(".", UriKind.Relative)));
            Assert.Null(converter(null));
        }
    }
}
