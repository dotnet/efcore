// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Relational.Design.ReverseEngineering.Internal
{
    public class TableSelectionSet
    {
        public static readonly TableSelectionSet InclusiveAll = new TableSelectionSet(null, null);

        public TableSelectionSet(
            [CanBeNull] List<string> schemas, [CanBeNull] List<string> tables)
        {
            Schemas = schemas ?? new List<string>();
            Tables = tables ?? new List<string>();
        }

        public virtual List<string> Schemas { get; }
        public virtual List<string> Tables { get; }
    }
}
