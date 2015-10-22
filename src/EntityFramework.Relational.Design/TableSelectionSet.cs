// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Scaffolding
{
    public class TableSelectionSet
    {
        public readonly static TableSelectionSet All = new TableSelectionSet();

        public TableSelectionSet()
            : this(null, null)
        {
        }

        public TableSelectionSet([CanBeNull] IReadOnlyList<string> tables)
            : this(tables, null)
        {
        }

        public TableSelectionSet([CanBeNull] IReadOnlyList<string> tables, [CanBeNull] IReadOnlyList<string> schemas)
        {
            Schemas = schemas ?? new List<string>().AsReadOnly();
            Tables = tables ?? new List<string>().AsReadOnly();
        }

        public virtual IReadOnlyList<string> Schemas { get; }
        public virtual IReadOnlyList<string> Tables { get; }
    }
}
