// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.Data.Entity.Design.Internal;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace Microsoft.Data.Entity.Relational.Design.FunctionalTests.ReverseEngineering
{
    public class InMemoryCommandLogger : CommandLogger
    {
        public LoggerMessages Messages = new LoggerMessages();
        private readonly ITestOutputHelper _output;
        private static readonly bool _logToOutput = false;

        public InMemoryCommandLogger(string name, ITestOutputHelper output)
            : base(name)
        {
            _output = output;
        }

        public override bool IsEnabled(LogLevel logLevel) => true;

        protected override void WriteError(string message)
        {
            if (_logToOutput)
            {
                _output?.WriteLine("[ERROR]: " + message);
            }

            Messages.Error.Add(message);
        }

        protected override void WriteWarning(string message)
        {
            if (_logToOutput)
            {
                _output?.WriteLine("[WARN]: " + message);
            }

            Messages.Warn.Add(message);
        }

        protected override void WriteInformation(string message)
        {
            if (_logToOutput)
            {
                _output?.WriteLine("[INFO]: " + message);
            }

            Messages.Info.Add(message);
        }

        protected override void WriteDebug(string message)
        {
            if (_logToOutput)
            {
                _output?.WriteLine("[DEBUG]: " + message);
            }

            Messages.Debug.Add(message);
        }

        protected override void WriteTrace(string message)
        {
            if (_logToOutput)
            {
                _output?.WriteLine("[TRACE]: " + message);
            }

            Messages.Trace.Add(message);
        }
    }

    public class LoggerMessages
    {
        public List<string> Error = new List<string>();
        public List<string> Warn = new List<string>();
        public List<string> Info = new List<string>();
        public List<string> Trace = new List<string>();
        public List<string> Debug = new List<string>();
    }
}
