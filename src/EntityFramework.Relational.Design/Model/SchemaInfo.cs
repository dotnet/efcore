// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Relational.Design.Model
{
    public class SchemaInfo
    {
        [CanBeNull]
        public virtual string DatabaseName { get; [param: CanBeNull] set; }

        [CanBeNull]
        public virtual string DefaultSchemaName { get; [param: CanBeNull] set; }

        public virtual IList<Table> Tables { get; [param: NotNull] set; } = new List<Table>();
    }
}
