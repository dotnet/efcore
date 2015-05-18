// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.SqlServer.Metadata
{
    public class ReadOnlySqlServerForeignKeyExtensions : ISqlServerForeignKeyExtensions
    {
        protected const string SqlServerNameAnnotation = SqlServerAnnotationNames.Prefix + RelationalAnnotationNames.Name;

        public ReadOnlySqlServerForeignKeyExtensions([NotNull] IForeignKey foreignKey)
        {
            Check.NotNull(foreignKey, nameof(foreignKey));

            ForeignKey = foreignKey;
        }

        public virtual string Name
            => ForeignKey[SqlServerNameAnnotation] as string
               ?? ForeignKey.Relational().Name;

        protected virtual IForeignKey ForeignKey { get; }
    }
}
