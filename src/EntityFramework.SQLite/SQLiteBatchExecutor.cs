// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Relational.Update;

namespace Microsoft.Data.Entity.SQLite
{
    public class SQLiteBatchExecutor : BatchExecutor
    {
        /// <summary>
        ///     This constructor is intended only for use when creating test doubles that will override members
        ///     with mocked or faked behavior. Use of this constructor for other purposes may result in unexpected
        ///     behavior including but not limited to throwing <see cref="NullReferenceException" />.
        /// </summary>
        protected SQLiteBatchExecutor()
        {
        }

        public SQLiteBatchExecutor(
            [NotNull] SQLiteTypeMapper typeMapper,
            [NotNull] DbContextConfiguration contextConfiguration)
            : base(typeMapper, contextConfiguration)
        {
        }
    }
}
