// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Microsoft.EntityFrameworkCore.Scaffolding.Metadata
{
    public class DatabaseModel : Annotatable
    {
        public virtual string DatabaseName { get; [param: CanBeNull] set; }

        public virtual string DefaultSchema { get; [param: CanBeNull] set; }

        public virtual IList<DatabaseTable> Tables { get; } = new List<DatabaseTable>();
        public virtual IList<DatabaseSequence> Sequences { get; } = new List<DatabaseSequence>();
    }
}
