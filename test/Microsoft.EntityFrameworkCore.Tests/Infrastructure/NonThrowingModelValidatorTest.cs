// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Tests.TestUtilities;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.Tests.Infrastructure
{
    public class NonThrowingModelValidatorTest : LoggingModelValidatorTest
    {
        protected override void VerifyError(string expectedMessage, IModel model) => VerifyWarning(expectedMessage, model);

        protected override ModelValidator CreateModelValidator()
            => new NonThrowingModelValidator(
                new Logger<NonThrowingModelValidator>(
                    new ListLoggerFactory(Log, l => l == typeof(NonThrowingModelValidator).FullName.Replace('+', '.'))));

        private class NonThrowingModelValidator : LoggingModelValidator
        {
            public NonThrowingModelValidator([NotNull] ILogger<NonThrowingModelValidator> logger)
                : base(logger)
            {
            }

            protected override void ShowError(string message) => ShowWarning(message);
        }
    }
}
