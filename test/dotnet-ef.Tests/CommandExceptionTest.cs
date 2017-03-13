// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Xunit;

namespace Microsoft.EntityFrameworkCore.Tools
{
    public class CommandExceptionTest
    {
        [Fact]
        public void Ctor_works()
        {
            var ex = new CommandException("Message1");

            Assert.Equal("Message1", ex.Message);
        }
    }
}
