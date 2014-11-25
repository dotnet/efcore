// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Relational.Update;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.Logging;

namespace Microsoft.Data.Entity.Sqlite
{
    public class SqliteBatchExecutor : BatchExecutor
    {
        /// <summary>
        ///     This constructor is intended only for use when creating test doubles that will override members
        ///     with mocked or faked behavior. Use of this constructor for other purposes may result in unexpected
        ///     behavior including but not limited to throwing <see cref="NullReferenceException" />.
        /// </summary>
        protected SqliteBatchExecutor()
        {
        }

        public SqliteBatchExecutor(
            [NotNull] SqliteTypeMapper typeMapper,
            [NotNull] LazyRef<DbContext> context,
            [NotNull] ILoggerFactory loggerFactory)
            : base(typeMapper, context, loggerFactory)
        {
        }
    }
}
