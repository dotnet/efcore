// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Migrations.Operations
{
    public class DeleteRowsOperation : MigrationOperation
    {
        public virtual string Table { get; [param: NotNull] set; }
        public virtual string Schema { get; [param: CanBeNull] set; }

        /// <summary>
        ///     The Keys should be objects with attribute names that map to the column names,
        ///     and not to the model attributes. They must all be the same type.
        /// </summary>
        public virtual object[] Keys { get; [param: NotNull] set; }
    }
}
