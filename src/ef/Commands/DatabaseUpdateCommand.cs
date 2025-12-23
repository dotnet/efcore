// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Tools.Commands;

// ReSharper disable once ArrangeTypeModifiers
internal partial class DatabaseUpdateCommand
{
    protected override int Execute(string[] args)
    {
        using var executor = CreateExecutor(args);

        if (_add!.HasValue())
        {
            // Create and apply a new migration in one step
            executor.CreateAndApplyMigration(
                _add.Value()!,
                _connection!.Value(),
                Context!.Value(),
                _outputDir!.Value(),
                _namespace!.Value());
        }
        else
        {
            executor.UpdateDatabase(_migration!.Value, _connection!.Value(), Context!.Value());
        }

        return base.Execute(args);
    }
}
