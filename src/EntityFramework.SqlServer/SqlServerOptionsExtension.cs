// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.DependencyInjection;

namespace Microsoft.Data.Entity.SqlServer
{
    public class SqlServerOptionsExtension : RelationalOptionsExtension
    {
        public SqlServerOptionsExtension()
        {
        }

        public SqlServerOptionsExtension([NotNull] SqlServerOptionsExtension copyFrom)
            : base(copyFrom)
        {
        }

        public override void ApplyServices(EntityFrameworkServicesBuilder builder)
            => Check.NotNull(builder, nameof(builder)).AddSqlServer();
    }
}
