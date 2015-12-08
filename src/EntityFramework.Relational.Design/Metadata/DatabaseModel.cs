// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Scaffolding.Metadata
{
    public class DatabaseModel
    {
        [CanBeNull]
        public virtual string DatabaseName { get; [param: CanBeNull] set; }

        [CanBeNull]
        public virtual string DefaultSchemaName { get; [param: CanBeNull] set; }

        public virtual IList<TableModel> Tables { get; } = new List<TableModel>();

        public virtual IList<TypeAliasModel> TypeAliases { get; } = new List<TypeAliasModel>();
    }
}
