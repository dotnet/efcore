// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Framework.Logging;

namespace Microsoft.Data.Entity.Internal
{
    public class LoggingModelValidator : ModelValidator
    {
        public LoggingModelValidator([NotNull] ILoggerFactory loggerFactory)
        {
            Logger = new LazyRef<ILogger>(loggerFactory.CreateLogger<ModelValidator>);
        }

        protected LazyRef<ILogger> Logger { get; }

        protected override void ShowWarning(string message) => Logger.Value.LogWarning(message);
    }
}
