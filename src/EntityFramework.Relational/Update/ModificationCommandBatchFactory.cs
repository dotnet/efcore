// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Relational.Utilities;

namespace Microsoft.Data.Entity.Relational.Update
{
    public class ModificationCommandBatchFactory
    {
        private readonly SqlGenerator _sqlGenerator;

        /// <summary>
        ///     This constructor is intended only for use when creating test doubles that will override members
        ///     with mocked or faked behavior. Use of this constructor for other purposes may result in unexpected
        ///     behavior including but not limited to throwing <see cref="NullReferenceException" />.
        /// </summary>
        protected ModificationCommandBatchFactory()
        {
        }

        public ModificationCommandBatchFactory(
            [NotNull] SqlGenerator sqlGenerator)
        {
            Check.NotNull(sqlGenerator, "sqlGenerator");

            _sqlGenerator = sqlGenerator;
        }

        protected SqlGenerator SqlGenerator
        {
            get { return _sqlGenerator; }
        }

        public virtual ModificationCommandBatch Create()
        {
            return new SingularModificationCommandBatch(SqlGenerator);
        }

        public virtual bool AddCommand(
            [NotNull] ModificationCommandBatch modificationCommandBatch,
            [NotNull] ModificationCommand modificationCommand)
        {
            Check.NotNull(modificationCommandBatch, "modificationCommandBatch");
            Check.NotNull(modificationCommand, "modificationCommand");

            return modificationCommandBatch.AddCommand(modificationCommand);
        }
    }
}
