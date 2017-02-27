// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Migrations
{
    public class MigrationCommandListBuilder
    {
        private readonly IRelationalCommandBuilderFactory _commandBuilderFactory;
        private readonly List<MigrationCommand> _commands = new List<MigrationCommand>();

        private IRelationalCommandBuilder _commandBuilder;

        public MigrationCommandListBuilder([NotNull] IRelationalCommandBuilderFactory commandBuilderFactory)
        {
            Check.NotNull(commandBuilderFactory, nameof(commandBuilderFactory));

            _commandBuilderFactory = commandBuilderFactory;
            _commandBuilder = commandBuilderFactory.Create();
        }

        public virtual IReadOnlyList<MigrationCommand> GetCommandList() => _commands;

        public virtual MigrationCommandListBuilder EndCommand(bool suppressTransaction = false)
        {
            if (_commandBuilder.GetLength() != 0)
            {
                _commands.Add(new MigrationCommand(_commandBuilder.Build(), suppressTransaction));
                _commandBuilder = _commandBuilderFactory.Create();
            }

            return this;
        }

        public virtual MigrationCommandListBuilder Append([NotNull] object o)
        {
            Check.NotNull(o, nameof(o));

            _commandBuilder.Append(o);

            return this;
        }

        public virtual MigrationCommandListBuilder AppendLine()
        {
            _commandBuilder.AppendLine();

            return this;
        }

        public virtual MigrationCommandListBuilder AppendLine([NotNull] object o)
        {
            Check.NotNull(o, nameof(o));

            _commandBuilder.AppendLine(o);

            return this;
        }

        public virtual MigrationCommandListBuilder AppendLines([NotNull] object o)
        {
            Check.NotNull(o, nameof(o));

            _commandBuilder.AppendLines(o);

            return this;
        }

        public virtual IDisposable Indent() => _commandBuilder.Indent();

        public virtual MigrationCommandListBuilder IncrementIndent()
        {
            _commandBuilder.IncrementIndent();

            return this;
        }

        public virtual MigrationCommandListBuilder DecrementIndent()
        {
            _commandBuilder.DecrementIndent();

            return this;
        }
    }
}
