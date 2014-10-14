// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Framework.Logging;

namespace Microsoft.Data.Entity.Commands.Utilities
{
    public class LoggerProvider : ILoggerProvider
    {
        private readonly Func<string, ILogger> _creator;

        public LoggerProvider([NotNull] Func<string, ILogger> creator)
        {
            Check.NotNull(creator, "creator");

            _creator = creator;
        }

        public virtual ILogger Create(string name)
        {
            return _creator(name);
        }
    }
}
