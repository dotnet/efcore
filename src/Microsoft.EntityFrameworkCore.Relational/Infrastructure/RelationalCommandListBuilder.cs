// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Infrastructure
{
    public class RelationalCommandListBuilder
    {
        private readonly IRelationalCommandBuilderFactory _commandBuilderFactory;
        private readonly List<IRelationalCommand> _commands = new List<IRelationalCommand>();

        private IRelationalCommandBuilder _commandBuilder;

        public RelationalCommandListBuilder([NotNull] IRelationalCommandBuilderFactory commandBuilderFactory)
        {
            Check.NotNull(commandBuilderFactory, nameof(commandBuilderFactory));

            _commandBuilderFactory = commandBuilderFactory;
            _commandBuilder = commandBuilderFactory.Create();
        }

        public virtual IReadOnlyList<IRelationalCommand> GetCommands() => _commands;

        public virtual RelationalCommandListBuilder EndCommand()
        {
            if (_commandBuilder.GetLength() != 0)
            {
                _commands.Add(_commandBuilder.Build());
                _commandBuilder = _commandBuilderFactory.Create();
            }

            return this;
        }

        public virtual RelationalCommandListBuilder Append([NotNull] object o)
        {
            Check.NotNull(o, nameof(o));

            _commandBuilder.Append(o);

            return this;
        }

        public virtual RelationalCommandListBuilder AppendLine()
        {
            _commandBuilder.AppendLine();

            return this;
        }

        public virtual RelationalCommandListBuilder AppendLine([NotNull] object o)
        {
            Check.NotNull(o, nameof(o));

            _commandBuilder.AppendLine(o);

            return this;
        }

        public virtual RelationalCommandListBuilder AppendLines([NotNull] object o)
        {
            Check.NotNull(o, nameof(o));

            _commandBuilder.AppendLines(o);

            return this;
        }

        public virtual IDisposable Indent() => _commandBuilder.Indent();

        public virtual RelationalCommandListBuilder IncrementIndent()
        {
            _commandBuilder.IncrementIndent();

            return this;
        }

        public virtual RelationalCommandListBuilder DecrementIndent()
        {
            _commandBuilder.DecrementIndent();

            return this;
        }
    }
}
