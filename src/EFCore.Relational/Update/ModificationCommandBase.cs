// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;
using System.Collections.Generic;

namespace Microsoft.EntityFrameworkCore.Update
{
    public class ModificationCommandBase
    {
        public ModificationCommandBase(
            [NotNull] string name,
            [CanBeNull] string schema,
            [CanBeNull] IReadOnlyList<ColumnModificationBase> columnModificationsBase)
        {
            Check.NotNull(name, nameof(name));

            TableName = name;
            Schema = schema;
            ColumnModificationsBase = columnModificationsBase;
        }

        public virtual string TableName { get; }

        public virtual string Schema { get; }

        public virtual IReadOnlyList<ColumnModificationBase> ColumnModificationsBase { get; }
    }
}
