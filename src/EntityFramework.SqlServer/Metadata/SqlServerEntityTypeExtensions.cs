// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.SqlServer.Utilities;

namespace Microsoft.Data.Entity.SqlServer.Metadata
{
    public class SqlServerEntityTypeExtensions : ReadOnlySqlServerEntityTypeExtensions
    {
        public SqlServerEntityTypeExtensions([NotNull] EntityType entityType)
            : base(entityType)
        {
        }

        public new virtual string Table
        {
            get { return base.Table; }
            [param: CanBeNull]
            set
            {
                Check.NullButNotEmpty(value, "value");

                ((EntityType)EntityType)[SqlServerTableAnnotation] = value;
            }
        }

        [CanBeNull]
        public new virtual string Schema
        {
            get { return base.Schema; }
            [param: CanBeNull]
            set
            {
                Check.NullButNotEmpty(value, "value");
                
                ((EntityType)EntityType)[SqlServerSchemaAnnotation] = value;
            }
        }
    }
}
