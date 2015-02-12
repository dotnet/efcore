// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.Logging;

namespace Microsoft.Data.Entity.Infrastructure
{
    public class LoggingModelValidator : ModelValidatorBase
    {
        public LoggingModelValidator([NotNull] ILoggerFactory loggerFactory)
        {
            Logger = new LazyRef<ILogger>(loggerFactory.Create<ModelValidatorBase>);
        }

        protected LazyRef<ILogger> Logger { get; }
        
        protected override void ShowWarning(string message)
        {
            Logger.Value.WriteWarning(message);
        }
    }
}
