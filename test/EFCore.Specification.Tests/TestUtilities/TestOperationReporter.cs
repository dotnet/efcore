// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Design.Internal;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.TestUtilities
{
    public class TestOperationReporter : IOperationReporter
    {
        private readonly List<(LogLevel, string)> _messages = new();

        public IReadOnlyList<(LogLevel Level, string Message)> Messages
            => _messages;

        public void Clear()
            => _messages.Clear();

        public void WriteInformation(string message)
            => _messages.Add((LogLevel.Information, message));

        public void WriteVerbose(string message)
            => _messages.Add((LogLevel.Debug, message));

        public void WriteWarning(string message)
            => _messages.Add((LogLevel.Warning, message));

        public void WriteError(string message)
            => _messages.Add((LogLevel.Error, message));
    }
}
