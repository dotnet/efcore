// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.SqlServer.Metadata
{
    public class SqlServerForeignKeyAnnotations : ReadOnlySqlServerForeignKeyAnnotations
    {
        public SqlServerForeignKeyAnnotations([NotNull] ForeignKey foreignKey)
            : base(foreignKey)
        {
        }

        [CanBeNull]
        public new virtual string Name
        {
            get { return base.Name; }
            [param: CanBeNull]
            set
            {
                Check.NullButNotEmpty(value, nameof(value));

                ((ForeignKey)ForeignKey)[SqlServerNameAnnotation] = value;
            }
        }
    }
}
