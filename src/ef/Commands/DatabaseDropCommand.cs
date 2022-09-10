// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Microsoft.EntityFrameworkCore.Tools.Properties;

namespace Microsoft.EntityFrameworkCore.Tools.Commands;

// ReSharper disable once ArrangeTypeModifiers
internal partial class DatabaseDropCommand
{
    protected override int Execute(string[] args)
    {
        using var executor = CreateExecutor(args);

        void LogDropCommand(Func<object?, object?, string> resource)
        {
            var result = executor.GetContextInfo(Context!.Value());
            var databaseName = result["DatabaseName"] as string;
            var dataSource = result["DataSource"] as string;
            Reporter.WriteInformation(resource(databaseName, dataSource));
        }

        if (_dryRun!.HasValue())
        {
            LogDropCommand(Resources.DatabaseDropDryRun);

            return 0;
        }

        if (!_force!.HasValue())
        {
            LogDropCommand(Resources.DatabaseDropPrompt);
            var response = Console.ReadLine()!.Trim().ToUpperInvariant();
            if (response != "Y")
            {
                return 1;
            }
        }

        executor.DropDatabase(Context!.Value());

        return base.Execute(args);
    }
}
