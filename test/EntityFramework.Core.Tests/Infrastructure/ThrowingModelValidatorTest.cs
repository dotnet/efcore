// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Xunit;

namespace Microsoft.Data.Entity.Tests.Infrastructure
{
    public class ThrowingModelValidatorTest : ModelValidatorBaseTest
    {
        protected override void VerifyWarning(string expectedMessage, IModel model)
        {
            Assert.Equal(expectedMessage,
                Assert.Throws<InvalidOperationException>(() => Validate(model)).Message);
        }

        protected override void VerifyError(string expectedMessage, IModel model)
        {
            VerifyWarning(expectedMessage, model);
        }

        protected override ModelValidatorBase CreateModelValidatorBase()
        {
            return new ThrowingModelValidator();
        }

        private class ThrowingModelValidator : ModelValidatorBase
        {
            protected override void ShowWarning(string message)
            {
                throw new InvalidOperationException(message);
            }
        }
    }
}
