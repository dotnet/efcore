// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Tools.Properties;

namespace Microsoft.EntityFrameworkCore.Tools.Commands
{
    partial class DatabaseDropCommand
    {
        protected override int Execute()
        {
            var executor = CreateExecutor();

            var result = executor.GetContextInfo(Context.Value());
            var databaseName = result["DatabaseName"] as string;
            var dataSource = result["DataSource"] as string;

            if (_dryRun.HasValue())
            {
                Reporter.WriteInformation(Resources.DatabaseDropDryRun(databaseName, dataSource));

                return 0;
            }

            if (!_force.HasValue())
            {
                Reporter.WriteInformation(Resources.DatabaseDropPrompt(databaseName, dataSource));
                var response = Console.ReadLine().Trim().ToUpperInvariant();
                if (response != "Y")
                {
                    return 1;
                }
            }

            executor.DropDatabase(Context.Value());

            return base.Execute();
        }
    }
}
