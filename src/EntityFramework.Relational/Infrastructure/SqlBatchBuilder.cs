// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Storage.Commands;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Infrastructure
{
    public class SqlBatchBuilder
    {
        private readonly List<RelationalCommand> _commands = new List<RelationalCommand>();
        private readonly RelationalCommandBuilder _commandBuilder = new RelationalCommandBuilder();

        public virtual IReadOnlyList<RelationalCommand> RelationalCommands => _commands;

        public virtual SqlBatchBuilder EndBatch()
        {
            if (_commandBuilder.Length != 0)
            {
                _commands.Add(_commandBuilder.RelationalCommand);
                _commandBuilder.Clear();
            }

            return this;
        }

        public virtual SqlBatchBuilder Append([NotNull] object o)
        {
            Check.NotNull(o, nameof(o));

            _commandBuilder.Append(o);

            return this;
        }

        public virtual SqlBatchBuilder AppendLine()
        {
            _commandBuilder.AppendLine();

            return this;
        }

        public virtual SqlBatchBuilder AppendLine([NotNull] object o)
        {
            Check.NotNull(o, nameof(o));

            _commandBuilder.AppendLine(o);

            return this;
        }

        public virtual SqlBatchBuilder AppendLines([NotNull] object o)
        {
            Check.NotNull(o, nameof(o));

            _commandBuilder.AppendLines(o);

            return this;
        }

        public virtual IDisposable Indent() => _commandBuilder.Indent();

        public virtual SqlBatchBuilder IncrementIndent()
        {
            _commandBuilder.IncrementIndent();

            return this;
        }

        public virtual SqlBatchBuilder DecrementIndent()
        {
            _commandBuilder.DecrementIndent();

            return this;
        }
    }
}
