// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.Internal
{
    public class InterceptingLogger<T> : ILogger<T>
    {
        private readonly ILogger _logger;

        public InterceptingLogger([NotNull] IDbContextServices contextServices)
        {
            _logger = contextServices.LoggerFactory.CreateLogger(typeof(T).DisplayName());
        }

        public virtual void Log<TState>(
            LogLevel logLevel, 
            EventId eventId, 
            TState state, 
            Exception exception, 
            Func<TState, Exception, string> formatter)
            => _logger.Log(logLevel, eventId, state, exception, formatter);

        public virtual bool IsEnabled(LogLevel logLevel)
            => _logger.IsEnabled(logLevel);

        public virtual IDisposable BeginScopeImpl(object state)
            => _logger.BeginScopeImpl(state);
    }
}
