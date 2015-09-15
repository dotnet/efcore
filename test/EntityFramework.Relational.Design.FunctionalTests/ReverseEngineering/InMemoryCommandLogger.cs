// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.Data.Entity.Design.Internal;
using Microsoft.Framework.Logging;

namespace Microsoft.Data.Entity.Relational.Design.FunctionalTests.ReverseEngineering
{
    public class InMemoryCommandLogger : CommandLogger
    {
        public LoggerMessages Messages = new LoggerMessages();

        public InMemoryCommandLogger(string name)
            : base(name)
        {
        }

        public override bool IsEnabled(LogLevel logLevel) => true;

        protected override void WriteError(string message)
        {
            Messages.Error.Add(message);
        }

        protected override void WriteWarning(string message)
        {
            Messages.Warn.Add(message);
        }

        protected override void WriteInformation(string message)
        {
            Messages.Info.Add(message);
        }

        protected override void WriteVerbose(string message)
        {
            Messages.Verbose.Add(message);
        }

        protected override void WriteDebug(string message)
        {
            Messages.Debug.Add(message);
        }
    }

    public class LoggerMessages
    {
        public List<string> Error = new List<string>();
        public List<string> Warn = new List<string>();
        public List<string> Info = new List<string>();
        public List<string> Verbose = new List<string>();
        public List<string> Debug = new List<string>();
    }
}
