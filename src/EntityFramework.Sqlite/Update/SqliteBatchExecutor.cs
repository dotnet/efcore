// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Relational.Update;
using Microsoft.Framework.Logging;

namespace Microsoft.Data.Entity.Sqlite.Update
{
    public class SqliteBatchExecutor : BatchExecutor, ISqliteBatchExecutor
    {
        public SqliteBatchExecutor(
            [NotNull] ISqliteTypeMapper typeMapper,
            [NotNull] DbContext context,
            [NotNull] ILoggerFactory loggerFactory)
            : base(typeMapper, context, loggerFactory)
        {
        }
    }
}
