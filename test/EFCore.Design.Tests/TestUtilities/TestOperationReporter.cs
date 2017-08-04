// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Design.Internal;

namespace Microsoft.EntityFrameworkCore.TestUtilities
{
    public class TestOperationReporter : IOperationReporter
    {
        private readonly List<string> _messages = new List<string>();

        public IReadOnlyList<string> Messages => _messages;

        public void Clear() => _messages.Clear();

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
