// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure.Internal;

namespace Microsoft.Data.Entity.Infrastructure
{
    public class SqliteDbContextOptionsBuilder : RelationalDbContextOptionsBuilder<SqliteDbContextOptionsBuilder, SqliteOptionsExtension>
    {
        public SqliteDbContextOptionsBuilder([NotNull] DbContextOptionsBuilder optionsBuilder)
            : base(optionsBuilder)
        {
        }

        protected override SqliteOptionsExtension CloneExtension()
            => new SqliteOptionsExtension(OptionsBuilder.Options.GetExtension<SqliteOptionsExtension>());

        public virtual SqliteDbContextOptionsBuilder SuppressForeignKeyEnforcement()
            => SetOption(e => e.EnforceForeignKeys = false);
    }
}
