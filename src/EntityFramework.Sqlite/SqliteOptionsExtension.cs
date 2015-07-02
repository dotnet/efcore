// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.DependencyInjection;

namespace Microsoft.Data.Entity.Sqlite
{
    public class SqliteOptionsExtension : RelationalOptionsExtension
    {
        public virtual bool ForeignKeys { get; set; } = true;

        public SqliteOptionsExtension()
        {
        }

        public SqliteOptionsExtension([NotNull] SqliteOptionsExtension copyFrom)
            : base(copyFrom)
        {
            ForeignKeys = copyFrom.ForeignKeys;
        }

        public override void ApplyServices(EntityFrameworkServicesBuilder builder)
        {
            Check.NotNull(builder, nameof(builder));

            builder.AddSqlite();
        }
    }
}
