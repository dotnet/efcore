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

        public virtual string DefaultSchemaName { get; [param: CanBeNull] set; }

        public virtual ICollection<TableModel> Tables { get; } = new List<TableModel>();
        public virtual ICollection<SequenceModel> Sequences { get; } = new List<SequenceModel>();
    }
}
