// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Commands.Utilities
{
    public class CommandLoggerAdapter : CommandLogger
    {
        private readonly ILogHandler _logHandler;

        public CommandLoggerAdapter([NotNull] string name, [NotNull] ILogHandler logHandler)
            : base(name)
        {
            Check.NotNull(logHandler, nameof(logHandler));

            _logHandler = logHandler;
        }

        protected override void WriteInformation(string message) => _logHandler.WriteInformation(message);
        protected override void WriteVerbose(string message) => _logHandler.WriteVerbose(message);
        protected override void WriteWarning(string message) => _logHandler.WriteWarning(message);
    }
}
