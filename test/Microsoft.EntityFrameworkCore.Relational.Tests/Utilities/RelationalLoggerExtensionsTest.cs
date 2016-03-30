// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.EntityFrameworkCore.Storage;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Relational.Tests.Utilities
{
    public class RelationalLoggerExtensionsTest
    {
        [Fact]
        public void Short_byte_arrays_are_not_truncated()
        {
            var shortArray = new Guid("21EC2020-3AEA-4069-A2DD-08002B30309D").ToByteArray();
            var longerShortArray = shortArray.Concat(shortArray).ToArray();

            Assert.Equal("0x2020EC21EA3A6940A2DD08002B30309D", RelationalLoggerExtensions.FormatParameterValue(shortArray));
            Assert.Equal("0x2020EC21EA3A6940A2DD08002B30309D2020EC21EA3A6940A2DD08002B30309D", RelationalLoggerExtensions.FormatParameterValue(longerShortArray));
        }

        [Fact]
        public void Long_byte_arrays_are_truncated()
        {
            var shortArray = new Guid("21EC2020-3AEA-4069-A2DD-08002B30309D").ToByteArray();
            var longArray = shortArray.Concat(shortArray).Concat(shortArray).ToArray();
            Assert.Equal("0x2020EC21EA3A6940A2DD08002B30309D2020EC21EA3A6940A2DD08002B30309D...", RelationalLoggerExtensions.FormatParameterValue(longArray));
        }
    }
}
