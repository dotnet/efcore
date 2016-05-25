// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.Design.Internal
{
    public class LoggerProvider : ILoggerProvider
    {
        private readonly Func<string, ILogger> _creator;

        public LoggerProvider([NotNull] Func<string, ILogger> creator)
        {
            Check.NotNull(creator, nameof(creator));

            _creator = creator;
        }

        public virtual ILogger CreateLogger(string categoryName) => _creator(categoryName);

        public virtual void Dispose()
        {
        }
    }
}
