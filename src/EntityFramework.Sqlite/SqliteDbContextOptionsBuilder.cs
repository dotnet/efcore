// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Sqlite;

namespace Microsoft.Data.Entity
{
    public class SqliteDbContextOptionsBuilder : RelationalDbContextOptionsBuilder
    {
        public SqliteDbContextOptionsBuilder([NotNull] DbContextOptionsBuilder optionsBuilder)
            : base(optionsBuilder)
        {
        }

        public virtual SqliteDbContextOptionsBuilder SuppressForeignKeysEnforcement()
        {
            var extension = new SqliteOptionsExtension(OptionsBuilder.Options.GetExtension<SqliteOptionsExtension>())
            {
                ForeignKeys = false
            };

            ((IDbContextOptionsBuilderInfrastructure)OptionsBuilder).AddOrUpdateExtension(extension);

            return this;
        }
    }
}
