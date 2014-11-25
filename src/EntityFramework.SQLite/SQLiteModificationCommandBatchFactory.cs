// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Relational.Update;

namespace Microsoft.Data.Entity.Sqlite
{
    public class SqliteModificationCommandBatchFactory : ModificationCommandBatchFactory
    {
        /// <summary>
        ///     This constructor is intended only for use when creating test doubles that will override members
        ///     with mocked or faked behavior. Use of this constructor for other purposes may result in unexpected
        ///     behavior including but not limited to throwing <see cref="NullReferenceException" />.
        /// </summary>
        protected SqliteModificationCommandBatchFactory()
        {
        }

        public SqliteModificationCommandBatchFactory(
            [NotNull] SqliteSqlGenerator sqlGenerator)
            : base(sqlGenerator)
        {
        }

        public override ModificationCommandBatch Create()
        {
            return new SqliteSingularModificationCommandBatch(SqlGenerator);
        }
    }
}
