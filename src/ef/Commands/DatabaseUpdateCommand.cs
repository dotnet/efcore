// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Tools.Properties;

namespace Microsoft.EntityFrameworkCore.Tools.Commands;

// ReSharper disable once ArrangeTypeModifiers
internal partial class DatabaseUpdateCommand
{
    protected override int Execute(string[] args)
    {
        // Validate that -o and -n are only used with --add
        if (!_add!.HasValue())
        {
            if (_outputDir!.HasValue())
            {
                throw new CommandException(Resources.OutputDirRequiresAdd);
            }

            if (_namespace!.HasValue())
            {
                throw new CommandException(Resources.NamespaceRequiresAdd);
            }
        }

        using var executor = CreateExecutor(args);

        if (_add!.HasValue())
        {
            // Create and apply a new migration in one step
            executor.AddAndApplyMigration(
                _add.Value()!,
                _outputDir!.Value(),
                Context!.Value(),
                _namespace!.Value(),
                _connection!.Value());
        }
        else
        {
            executor.UpdateDatabase(_migration!.Value, _connection!.Value(), Context!.Value());
        }

        return base.Execute(args);
    }
}
