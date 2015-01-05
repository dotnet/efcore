// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Relational.Migrations.MigrationsModel
{
    // TODO: Add an equivalent of "object anonymousArguments" to operations, if needed.
    public abstract class MigrationOperation
    {
        public virtual bool IsDestructiveChange
        {
            get { return false; }
        }

        // TODO: Consider removing GenerateSql and GenerateCode and using Accept instead.
        public abstract void Accept<TVisitor, TContext>([NotNull] TVisitor visitor, [NotNull] TContext context)
            where TVisitor : MigrationOperationVisitor<TContext>;

        public abstract void GenerateSql([NotNull] MigrationOperationSqlGenerator generator, [NotNull] SqlBatchBuilder batchBuilder);

        public abstract void GenerateCode([NotNull] MigrationCodeGenerator generator, [NotNull] IndentedStringBuilder stringBuilder);
    }
}
