// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Relational.Model;

namespace Microsoft.Data.Entity.Migrations
{
    public interface IMigrationOperationSqlGeneratorFactory
    {
        MigrationOperationSqlGenerator Create();
        MigrationOperationSqlGenerator Create([NotNull] DatabaseModel database);
    }
}
