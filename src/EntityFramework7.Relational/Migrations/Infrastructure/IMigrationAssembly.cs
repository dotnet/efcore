// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.Migrations.Infrastructure
{
    public interface IMigrationAssembly
    {
        IReadOnlyList<Migration> Migrations { get; }
        ModelSnapshot ModelSnapshot { get; }
        IModel LastModel { get; }
    }
}
