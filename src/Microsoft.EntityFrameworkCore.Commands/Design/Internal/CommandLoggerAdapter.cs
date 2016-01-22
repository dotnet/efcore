// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Design.Internal
{
    public class CommandLoggerAdapter : CommandLogger
    {
        private readonly IOperationLogHandler _logHandler;

        public CommandLoggerAdapter([NotNull] string name, [NotNull] IOperationLogHandler logHandler)
            : base(name)
        {
            Check.NotNull(logHandler, nameof(logHandler));

            _logHandler = logHandler;
        }

        protected override void WriteError(string message) => _logHandler.WriteError(message);
        protected override void WriteInformation(string message) => _logHandler.WriteInformation(message);
        protected override void WriteWarning(string message) => _logHandler.WriteWarning(message);
        protected override void WriteDebug(string message) => _logHandler.WriteDebug(message);
        protected override void WriteTrace(string message) => _logHandler.WriteTrace(message);
    }
}
