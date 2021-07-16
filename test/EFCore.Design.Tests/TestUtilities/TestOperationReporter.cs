// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Design.Internal;

namespace Microsoft.EntityFrameworkCore.TestUtilities
{
    public class TestOperationReporter : IOperationReporter
    {
        private readonly List<string> _messages = new();

        public IReadOnlyList<string> Messages
            => _messages;

        public void Clear()
            => _messages.Clear();

        public void WriteInformation(string message)
            => _messages.Add("info: " + message);

        public void WriteVerbose(string message)
            => _messages.Add("verbose: " + message);

        public void WriteWarning(string message)
            => _messages.Add("warn: " + message);

        public void WriteError(string message)
            => _messages.Add("error: " + message);
    }
}
