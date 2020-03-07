// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Storage
{
    public class StringToGuidConverterTest
    {
        private static readonly StringToGuidConverter _stringToGuid
            = new StringToGuidConverter();

        [ConditionalFact]
        public void Can_convert_String_to_GUIDs()
        {
            var converter = _stringToGuid.ConvertToProviderExpression.Compile();

            Assert.Equal(
                new Guid("96EE27B4-868B-4049-BA67-CBB83CE5B462"),
                converter("96EE27B4-868B-4049-BA67-CBB83CE5B462"));

            Assert.Equal(
                Guid.Empty,
                converter("00000000-0000-0000-0000-000000000000"));

            Assert.Equal(Guid.Empty, converter(null));
        }

        [ConditionalFact]
        public void Can_convert_GUIDs_to_String()
        {
            var converter = _stringToGuid.ConvertFromProviderExpression.Compile();

            Assert.Equal(
                "96ee27b4-868b-4049-ba67-cbb83ce5b462",
                converter(new Guid("96EE27B4-868B-4049-BA67-CBB83CE5B462")));

            Assert.Equal(
                "00000000-0000-0000-0000-000000000000",
                converter(Guid.Empty));
        }
    }
}
