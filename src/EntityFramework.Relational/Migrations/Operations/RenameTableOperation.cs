// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Relational.Migrations.Operations
{
    public class RenameTableOperation : MigrationOperation
    {
        public virtual string Schema { get;[param: CanBeNull] set; }
        public virtual string Name { get;[param: NotNull] set; }
        public virtual string NewSchema { get;[param: CanBeNull] set; }
        public virtual string NewName { get;[param: CanBeNull] set; }
    }
}
