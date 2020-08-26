// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Design;
using Xunit;

namespace Microsoft.EntityFrameworkCore
{
    public class DesignExceptionTest
    {
        [ConditionalFact]
        public void OperationException_exposes_public_empty_constructor()
        {
            new OperationException();
        }

        [ConditionalFact]
        public void OperationException_exposes_public_string_constructor()
        {
            Assert.Equal("Foo", new OperationException("Foo").Message);
        }

        [ConditionalFact]
        public void OperationException_exposes_public_string_and_inner_exception_constructor()
        {
            var inner = new Exception();

            var ex = new OperationException("Foo", inner);

            Assert.Equal("Foo", ex.Message);
            Assert.Same(inner, ex.InnerException);
        }
    }
}
