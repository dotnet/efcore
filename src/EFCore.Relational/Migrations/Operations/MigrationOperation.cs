// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Microsoft.EntityFrameworkCore.Migrations.Operations
{
    public abstract class MigrationOperation : Annotatable
    {
        public virtual bool IsDestructiveChange { get; set; }
    }
}
