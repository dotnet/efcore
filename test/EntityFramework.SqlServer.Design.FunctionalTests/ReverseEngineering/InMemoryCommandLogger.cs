// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.Data.Entity.Commands.Utilities;
using Microsoft.Framework.Logging;

namespace EntityFramework.SqlServer.Design.ReverseEngineering.FunctionalTests
{
    public class InMemoryCommandLogger : CommandLogger
    {
        public List<string> InformationMessages { get; } = new List<string>();
        public List<string> VerboseMessages { get; } = new List<string>();
        public List<string> WarningMessages { get; } = new List<string>();

        public InMemoryCommandLogger(string name)
            : base(name)
        {
        }

        public override bool IsEnabled(LogLevel logLevel) => true;

        protected override void WriteWarning(string message)
        {
            WarningMessages.Add(message);
        }

        protected override void WriteInformation(string message)
        {
            InformationMessages.Add(message);
        }

        protected override void WriteVerbose(string message)
        {
            VerboseMessages.Add(message);
        }
    }
}
