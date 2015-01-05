// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Relational.Migrations.Infrastructure;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.Logging;

namespace Microsoft.Data.Entity.Commands.Utilities
{
    public abstract class CommandLogger : ILogger
    {
        private static string[] _includedNames = new[]
        {
            typeof(MigrationTool).FullName,
            typeof(Migrator).FullName
        };

        private readonly string _name;
        private readonly bool _enabledByName;

        public CommandLogger([NotNull] string name)
        {
            Check.NotNull(name, "name");

            _name = name;
            _enabledByName = _includedNames.Contains(name);
        }

        public virtual bool IsEnabled(LogLevel logLevel)
        {
            return _enabledByName;
        }

        public virtual void Write(LogLevel logLevel, int eventId, object state, Exception exception, Func<object, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }

            var message = new StringBuilder();
            if (formatter != null)
            {
                message.Append(formatter(state, exception));
            }
            else if (state != null)
            {
                message.Append(state);

                if (exception != null)
                {
                    message.Append(Environment.NewLine);
                    message.Append(exception);
                }
            }

            switch (logLevel)
            {
                case LogLevel.Warning:
                    WriteWarning(message.ToString());
                    break;
                case LogLevel.Information:
                    WriteInformation(message.ToString());
                    break;
                case LogLevel.Verbose:
                    WriteVerbose(message.ToString());
                    break;
                default:
                    Debug.Fail("Unexpected event type.");
                    break;
            }
        }

        public virtual IDisposable BeginScope(object state)
        {
            return null;
        }

        protected abstract void WriteWarning(string message);
        protected abstract void WriteInformation(string message);
        protected abstract void WriteVerbose(string message);
    }
}
