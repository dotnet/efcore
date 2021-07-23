// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Migrations
{
    /// <summary>
    ///     A builder for creating a list of <see cref="MigrationCommand" />s that can then be
    ///     executed to migrate a database.
    /// </summary>
    public class MigrationCommandListBuilder
    {
        private readonly List<MigrationCommand> _commands = new();
        private readonly MigrationsSqlGeneratorDependencies _dependencies;

        private IRelationalCommandBuilder _commandBuilder;

        /// <summary>
        ///     Creates a new instance of the builder.
        /// </summary>
        /// <param name="dependencies"> Dependencies needed for SQL generations. </param>
        public MigrationCommandListBuilder(
            MigrationsSqlGeneratorDependencies dependencies)
        {
            Check.NotNull(dependencies, nameof(dependencies));

            _dependencies = dependencies;
            _commandBuilder = dependencies.CommandBuilderFactory.Create();
        }

        /// <summary>
        ///     Gets the list of built commands.
        /// </summary>
        /// <returns> The <see cref="MigrationCommand" />s that have been built. </returns>
        public virtual IReadOnlyList<MigrationCommand> GetCommandList()
            => _commands;

        /// <summary>
        ///     Ends the building of the current command and adds it to the list of built commands.
        ///     The next call to one of the builder methods will start building a new command.
        /// </summary>
        /// <param name="suppressTransaction">
        ///     Indicates whether or not transactions should be suppressed while executing the built command.
        /// </param>
        /// <returns> This builder so that additional calls can be chained. </returns>
        public virtual MigrationCommandListBuilder EndCommand(bool suppressTransaction = false)
        {
            if (_commandBuilder.CommandTextLength != 0)
            {
                _commands.Add(
                    new MigrationCommand(
                        _commandBuilder.Build(),
                        _dependencies.CurrentContext.Context,
                        _dependencies.Logger,
                        suppressTransaction));

                _commandBuilder = _dependencies.CommandBuilderFactory.Create();
            }

            return this;
        }

        /// <summary>
        ///     Appends the given string to the command being built.
        /// </summary>
        /// <param name="o"> The string to append. </param>
        /// <returns> This builder so that additional calls can be chained. </returns>
        public virtual MigrationCommandListBuilder Append(string o)
        {
            Check.NotNull(o, nameof(o));

            _commandBuilder.Append(o);

            return this;
        }

        /// <summary>
        ///     Starts a new line on the command being built.
        /// </summary>
        /// <returns> This builder so that additional calls can be chained. </returns>
        public virtual MigrationCommandListBuilder AppendLine()
        {
            _commandBuilder.AppendLine();

            return this;
        }

        /// <summary>
        ///     Appends the given string to the command being built, and then starts a new line.
        /// </summary>
        /// <param name="value"> The string to append. </param>
        /// <returns> This builder so that additional calls can be chained. </returns>
        public virtual MigrationCommandListBuilder AppendLine(string value)
        {
            Check.NotNull(value, nameof(value));

            _commandBuilder.AppendLine(value);

            return this;
        }

        /// <summary>
        ///     Appends the given object to the command being built as multiple lines of text. That is,
        ///     each line in the passed string is added as a line to the command being built.
        ///     This results in the lines having the correct indentation.
        /// </summary>
        /// <param name="value"> The string to append. </param>
        /// <returns> This builder so that additional calls can be chained. </returns>
        public virtual MigrationCommandListBuilder AppendLines(string value)
        {
            Check.NotNull(value, nameof(value));

            _commandBuilder.AppendLines(value);

            return this;
        }

        /// <summary>
        ///     Starts a new indentation block, so all 'Append...' calls until the
        ///     block is disposed will be indented one level more than the current level.
        /// </summary>
        /// <returns> The object to dispose to indicate that the indentation should go back up a level. </returns>
        public virtual IDisposable Indent()
            => _commandBuilder.Indent();

        /// <summary>
        ///     Increases the current indentation by one level.
        /// </summary>
        /// <returns> This builder so that additional calls can be chained. </returns>
        public virtual MigrationCommandListBuilder IncrementIndent()
        {
            _commandBuilder.IncrementIndent();

            return this;
        }

        /// <summary>
        ///     Decreases the current indentation by one level.
        /// </summary>
        /// <returns> This builder so that additional calls can be chained. </returns>
        public virtual MigrationCommandListBuilder DecrementIndent()
        {
            _commandBuilder.DecrementIndent();

            return this;
        }
    }
}
