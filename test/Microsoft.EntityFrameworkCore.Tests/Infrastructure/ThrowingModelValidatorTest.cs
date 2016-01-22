// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Tests.Infrastructure
{
    public class ThrowingModelValidatorTest : ModelValidatorTest
    {
        protected override void VerifyWarning(string expectedMessage, IModel model)
            => Assert.Equal(expectedMessage, Assert.Throws<InvalidOperationException>(() => Validate(model)).Message);

        protected override void VerifyError(string expectedMessage, IModel model) => VerifyWarning(expectedMessage, model);

        protected override ModelValidator CreateModelValidator() => new ThrowingModelValidator();

        private class ThrowingModelValidator : ModelValidator
        {
            protected override void ShowWarning(string message)
            {
                throw new InvalidOperationException(message);
            }
        }
    }
}
