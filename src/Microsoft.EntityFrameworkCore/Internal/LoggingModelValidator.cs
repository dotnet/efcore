// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.Internal
{
    public class LoggingModelValidator : ModelValidator
    {
        // ReSharper disable once SuggestBaseTypeForParameter
        public LoggingModelValidator([NotNull] ILogger<LoggingModelValidator> logger)
        {
            Logger = logger;
        }

        protected virtual ILogger Logger { get; }

        protected override void ShowWarning(string message) 
            => Logger.LogWarning(CoreLoggingEventId.ModelValidationWarning, () => message);
    }
}
