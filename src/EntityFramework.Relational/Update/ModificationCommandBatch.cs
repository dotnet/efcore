// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.Logging;

namespace Microsoft.Data.Entity.Relational.Update
{
    public abstract class ModificationCommandBatch
    {
        /// <summary>
        ///     This constructor is intended only for use when creating test doubles that will override members
        ///     with mocked or faked behavior. Use of this constructor for other purposes may result in unexpected
        ///     behavior including but not limited to throwing <see cref="NullReferenceException" />.
        /// </summary>
        protected ModificationCommandBatch()
        {
        }

        protected ModificationCommandBatch([NotNull] ISqlGenerator sqlGenerator)
        {
            Check.NotNull(sqlGenerator, nameof(sqlGenerator));

            SqlGenerator = sqlGenerator;
        }

        protected ISqlGenerator SqlGenerator { get; private set; }

        public abstract IReadOnlyList<ModificationCommand> ModificationCommands { get; }

        public abstract bool AddCommand([NotNull] ModificationCommand modificationCommand);

        public abstract int Execute(
            [NotNull] RelationalTransaction transaction,
            [NotNull] RelationalTypeMapper typeMapper,
            [NotNull] DbContext context,
            [NotNull] ILogger logger);

        public abstract Task<int> ExecuteAsync(
            [NotNull] RelationalTransaction transaction,
            [NotNull] RelationalTypeMapper typeMapper,
            [NotNull] DbContext context,
            [NotNull] ILogger logger,
            CancellationToken cancellationToken = default(CancellationToken));
    }
}
