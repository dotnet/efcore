// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.Internal
{
    public class InterceptingLogger<T> : ILogger<T>
    {
        private readonly ILogger _logger;

        public InterceptingLogger(
            [NotNull] IDbContextServices contextServices,
            [NotNull] IServiceProvider serviceProvider)
        {
            _logger = (contextServices.LoggerFactory
                       ?? serviceProvider.GetRequiredService<ILoggerFactory>())
                .CreateLogger(typeof(T).DisplayName());
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

        public virtual IDisposable BeginScope<TState>(TState state)
            => _logger.BeginScope(state);
    }
}
