// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.DotNet.Cli.CommandLine;
using Microsoft.EntityFrameworkCore.Tools.Properties;

namespace Microsoft.EntityFrameworkCore.Tools.Commands;

internal partial class MigrationsScriptCommand : ContextCommandBase
{
    private CommandArgument? _from;
    private CommandArgument? _to;
    private CommandOption? _output;
    private CommandOption? _idempotent;
    private CommandOption? _noTransactions;

    public override void Configure(CommandLineApplication command)
    {
        command.Description = Resources.MigrationsScriptDescription;

        _from = command.Argument("<FROM>", Resources.MigrationFromDescription);
        _to = command.Argument("<TO>", Resources.MigrationToDescription);

        _output = command.Option("-o|--output <FILE>", Resources.OutputDescription);
        _idempotent = command.Option("-i|--idempotent", Resources.IdempotentDescription);
        _noTransactions = command.Option("--no-transactions", Resources.NoTransactionsDescription);

        base.Configure(command);
    }
}
