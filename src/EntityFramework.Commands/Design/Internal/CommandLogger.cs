// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Migrations.Design;
using Microsoft.Data.Entity.Migrations.Internal;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.Logging;

#if DNX451 || DNXCORE50
using Microsoft.Data.Entity.Commands;
#endif

namespace Microsoft.Data.Entity.Design.Internal
{
    public abstract class CommandLogger : ILogger
    {
        private static readonly string[] _includedNames =
        {
#if DNX451 || DNXCORE50
            typeof(Program).FullName,
#endif
            typeof(DatabaseOperations).FullName,
            typeof(DbContextOperations).FullName,
            typeof(MigrationsOperations).FullName,
            typeof(MigrationsScaffolder).FullName,
            typeof(Migrator).FullName
        };

        private readonly string _name;
        private readonly bool _enabledByName;

        protected CommandLogger([NotNull] string name)
        {
            Check.NotEmpty(name, nameof(name));

            _name = name;
            _enabledByName = _includedNames.Contains(name);
        }

        public virtual bool IsEnabled(LogLevel logLevel) => _enabledByName;

        public virtual IDisposable BeginScope([NotNull] object state)
        {
            throw new NotImplementedException();
        }

        public virtual void Log(
            LogLevel logLevel,
            int eventId,
            object state,
            Exception exception,
            Func<object, Exception, string> formatter)
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
                case LogLevel.Error:
                    WriteError(message.ToString());
                    break;
                case LogLevel.Warning:
                    WriteWarning(message.ToString());
                    break;
                case LogLevel.Information:
                    WriteInformation(message.ToString());
                    break;
                case LogLevel.Verbose:
                    WriteVerbose(message.ToString());
                    break;
                case LogLevel.Debug:
                    WriteDebug(message.ToString());
                    break;
                default:
                    Debug.Fail("Unexpected event type: " + logLevel);
                    WriteVerbose(message.ToString());
                    break;
            }
        }

        public virtual IDisposable BeginScopeImpl(object state) => null;

        protected abstract void WriteError([NotNull] string message);
        protected abstract void WriteWarning([NotNull] string message);
        protected abstract void WriteInformation([NotNull] string message);
        protected abstract void WriteVerbose([NotNull] string message);
        protected abstract void WriteDebug([NotNull] string message);
    }
}
