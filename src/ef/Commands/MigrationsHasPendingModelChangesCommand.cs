// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Tools.Commands;

internal partial class MigrationsHasPendingModelChangesCommand
{
    protected override int Execute(string[] args)
    {
        using var executor = CreateExecutor(args);

        executor.HasPendingModelChanges(Context!.Value());

        return base.Execute(args);
    }
}
