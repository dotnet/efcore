// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.EntityFrameworkCore.Tools.Commands
{
    // ReSharper disable once ArrangeTypeModifiers
    internal partial class DatabaseUpdateCommand
    {
        protected override int Execute(string[] args)
        {
            CreateExecutor(args).UpdateDatabase(_migration.Value, _connection.Value(), Context.Value());

            return base.Execute(args);
        }
    }
}
