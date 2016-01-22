// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Tests.TestUtilities;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Tests.Infrastructure
{
    public class LoggingModelValidatorTest : ModelValidatorTest
    {
        public LoggingModelValidatorTest()
        {
            Log = new List<Tuple<LogLevel, string>>();
        }

        protected List<Tuple<LogLevel, string>> Log { get; }

        protected override void VerifyWarning(string expectedMessage, IModel model)
        {
            Validate(model);

            Assert.Equal(LogLevel.Warning, Log[0].Item1);
            Assert.Equal(expectedMessage, Log[0].Item2);
        }

        protected override void VerifyError(string expectedMessage, IModel model)
            => Assert.Equal(expectedMessage, Assert.Throws<InvalidOperationException>(() => Validate(model)).Message);

        protected override ModelValidator CreateModelValidator()
            => new LoggingModelValidator(
                new Logger<LoggingModelValidator>(
                    new ListLoggerFactory(Log, l => l == typeof(LoggingModelValidator).FullName)));
    }
}
