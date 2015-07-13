// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.SqlServer.Metadata
{
    public class ReadOnlySqlServerForeignKeyAnnotations : ReadOnlyRelationalForeignKeyAnnotations, ISqlServerForeignKeyAnnotations
    {
        protected const string SqlServerNameAnnotation = SqlServerAnnotationNames.Prefix + RelationalAnnotationNames.Name;

        public ReadOnlySqlServerForeignKeyAnnotations([NotNull] IForeignKey foreignKey)
            : base(foreignKey)
        {
        }

        public override string Name
            => ForeignKey[SqlServerNameAnnotation] as string
               ?? base.Name;
    }
}
