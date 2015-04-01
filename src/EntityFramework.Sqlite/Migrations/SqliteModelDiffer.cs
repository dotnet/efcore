// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Relational.Migrations.Infrastructure;

namespace Microsoft.Data.Entity.Sqlite.Migrations
{
    public class SqliteModelDiffer : ModelDiffer, ISqliteModelDiffer
    {
        public SqliteModelDiffer([NotNull] ISqliteTypeMapper typeMapper)
            : base(typeMapper)
        {
        }
    }
}
