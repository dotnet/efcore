// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Design.Internal;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Relational.Design.Specification.Tests.ReverseEngineering
{
    public class InMemoryOperationReporter : IOperationReporter
    {
        public LoggerMessages Messages = new LoggerMessages();
        private readonly ITestOutputHelper _output;
        private static readonly bool _logToOutput = false;

        public InMemoryOperationReporter(ITestOutputHelper output)
        {
            _output = output;
        }

        public void WriteError(string message)
        {
            if (_logToOutput)
            {
                _output?.WriteLine("[ERROR]: " + message);
            }

            Messages.Error.Add(message);
        }

        public void WriteWarning(string message)
        {
            if (_logToOutput)
            {
                _output?.WriteLine("[WARN]: " + message);
            }

            Messages.Warn.Add(message);
        }

        public void WriteInformation(string message)
        {
            if (_logToOutput)
            {
                _output?.WriteLine("[INFO]: " + message);
            }

            Messages.Info.Add(message);
        }

        public void WriteVerbose(string message)
        {
            if (_logToOutput)
            {
                _output?.WriteLine("[DEBUG]: " + message);
            }

            Messages.Debug.Add(message);
        }
    }

    public class LoggerMessages
    {
        public List<string> Error = new List<string>();
        public List<string> Warn = new List<string>();
        public List<string> Info = new List<string>();
        public List<string> Debug = new List<string>();
    }
}
